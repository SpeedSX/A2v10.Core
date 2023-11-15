﻿// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Dynamic;
using System.Threading.Tasks;

using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

using A2v10.Infrastructure;
using A2v10.Xaml.Report;

namespace A2v10.ReportEngine.Pdf;

public class PdfReportEngine : IReportEngine
{
	private readonly IAppCodeProvider _appCodeProvider;
	private readonly IReportLocalizer _localizer;

	public PdfReportEngine(IAppCodeProvider appCodeProvider, ILocalizer localizer, ICurrentUser user)
	{
        Settings.License ??= LicenseType.Community;
        _appCodeProvider = appCodeProvider;
		_localizer = new PdfReportLocalizer(user.Locale.Locale, localizer);
	}

	private Page ReadTemplate(String path)
	{
		using var stream = _appCodeProvider.FileStreamRO(path)
			?? throw new InvalidOperationException($"File not found '{path}'");
		return TemplateReader.ReadReport(stream);
	}
	public Task<IInvokeResult> ExportAsync(IReportInfo reportInfo, ExportReportFormat format)
	{
		var repPath = Path.Combine(reportInfo.Path, reportInfo.Report) + ".xaml";
		Page page = ReadTemplate(repPath);

		var name = reportInfo.DataModel?.Root?.Resolve(reportInfo.Name) ?? "report";

		var model = reportInfo.DataModel?.Root ?? [];
		var context = new RenderContext(repPath, _localizer, model, page.Code);
		var doc = new ReportDocument(page, context);

		using MemoryStream outputStream = new();

		doc.GeneratePdf(outputStream);
		var result = new PdfInvokeResult(outputStream.ToArray(), name + ".pdf");
		return Task.FromResult<IInvokeResult>(result);
	}
}