﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;

namespace A2v10.Infrastructure;
public interface IModelBase
{
	[Flags]
	enum ParametersFlags
	{
		None = 0x00,
		SkipId = 0x01,
		SkipModelJsonParams = 0x02,
	}

	String? DataSource { get; }

	Boolean Signal { get; }

	String LoadProcedure();
    String UpdateProcedure();
    Boolean HasModel();

	Boolean CheckRoles(IEnumerable<String>? roles);
	String Path { get; }
	String BaseUrl { get; }

	Int32 CommandTimeout { get; }

	ExpandoObject CreateParameters(IPlatformUrl url, Object? id, Action<ExpandoObject>? setParams = null, ParametersFlags flags = ParametersFlags.None);
}

public enum ModelBlobType
{
    sql,
    json,
	clr,
	parse,
	azureBlob
}

public enum ModelParseType
{
	none,
    json,
	xlsx,
	excel,
	csv,
	dbf,
	xml,
	auto
}

public interface IModelBlob
{
	String? Id { get; }
	String? Key { get; }    
	ModelBlobType Type { get; }	
	ModelParseType Parse { get; }
    String? DataSource { get; }
    String? ClrType { get; }
    String? OutputFileName { get; }
	Boolean Zip { get; }
    Int32 CommandTimeout { get; }
    String LoadProcedure();
	String UpdateProcedure();
    
}

public interface IModelMerge : IModelBase
{
	ExpandoObject CreateMergeParameters(IDataModel model, ExpandoObject prms);
}

public enum ModelJsonExportFormat
{
    unknown,
    xlsx,
    dbf,
    csv
}

public interface IModelExport
{
    String? FileName { get;  }
    String? Template { get; }
	ModelJsonExportFormat Format { get; }
    String? Encoding { get; }
	String? GetTemplateExpression();
	Encoding GetEncoding();
}

public interface IModelView : IModelBase
{
	Boolean Copy { get; }
	String? Template { get; }

    IModelExport? Export { get; }
    Boolean Indirect { get; }
	String? Target { get; }
	String? TargetId { get; }
	IModelView? TargetModel { get; }

	IModelMerge? Merge { get; }

	List<String>? Scripts { get; }
	List<String>? Styles { get; }

	String GetView(Boolean bMobile);
	Boolean IsDialog { get; }
	Boolean IsIndex { get; }

	Boolean IsSkipDataStack { get; }

	Boolean IsPlain { get; }
	String? SqlTextKey();
	String ExpandProcedure();
	String LoadLazyProcedure(String property);
	String DeleteProcedure(String? property);

	IModelView Resolve(IDataModel model);
}

public interface IModelInvokeCommand
{
	Task<IInvokeResult> ExecuteAsync(IModelCommand command, ExpandoObject parameters);
}

public interface IModelCommand : IModelBase
{
	IModelInvokeCommand GetCommandHandler(IServiceProvider serviceProvider);

	String? Target { get; }
	String? File { get; }
	String? ClrType { get; }
	ExpandoObject? Args { get; }
}

public interface IModelReportHandler
{
	Task<IInvokeResult> ExportAsync(IModelReport report, ExportReportFormat format, ExpandoObject? query, Action<ExpandoObject> setParams);
	Task<IReportInfo> GetReportInfoAsync(IModelReport report, ExpandoObject? query, Action<ExpandoObject> setParams);
}

public interface IModelReport : IModelBase
{
	String? Name { get; }
	String? Report { get; }

	IModelReportHandler GetReportHandler(IServiceProvider serviceProvider);

	ExpandoObject CreateParameters(ExpandoObject? query, Action<ExpandoObject> setParams);
	ExpandoObject CreateVariables(ExpandoObject? query, Action<ExpandoObject> setParams);
}

public interface IModelJsonReader
{
	Task<IModelView?> TryGetViewAsync(IPlatformUrl url);
	Task<IModelView> GetViewAsync(IPlatformUrl url);
	Task<IModelBlob?> GetBlobAsync(IPlatformUrl url, String? suffix = null);
	Task<IModelCommand> GetCommandAsync(IPlatformUrl url, String command);
	Task<IModelReport> GetReportAsync(IPlatformUrl url);
}

