﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;

namespace A2v10.Infrastructure
{
	public interface IModelBase
	{
		[Flags]
		enum ParametersFlags
		{
			None = 0x00,
			SkipId = 0x01,
		}

		String DataSource { get; }

		String LoadProcedure();
		Boolean HasModel();

		String Path { get; }
		String BaseUrl { get; }

		ExpandoObject CreateParameters(IPlatformUrl url, Object id, Action<ExpandoObject> setParams = null, ParametersFlags flags = ParametersFlags.None);
	}

	public interface IModelBlob
	{
		String DataSource { get; }
		String LoadProcedure();
		String Id { get; }
		String Key { get; }
	}

	public interface IModelView: IModelBase
	{
		Boolean Copy { get; }
		String Template { get; }

		Boolean Indirect { get; }
		String Target { get; }
		String TargetId { get; }
		IModelView TargetModel { get; }

		IModelBase Merge { get; }

		String GetView(Boolean bMobile);
		Boolean IsDialog { get; }

		String ExpandProcedure();
		String UpdateProcedure();
		String LoadLazyProcedure(String property);
		String DeleteProcedure(String property);

		IModelView Resolve(IDataModel model);
	}

	public interface IModelInvokeCommand
	{
		Task<IInvokeResult> ExecuteAsync(IModelCommand command, ExpandoObject parameters);
	}

	public interface IModelCommand : IModelBase
	{
		IModelInvokeCommand GetCommandHandler(IServiceProvider serviceProvider);
	}

	public enum ModelReportType
	{
		stimulsoft,
		xml,
		json
	}

	public interface IModelReportHandler
	{
		Task<IInvokeResult> ExportAsync(IModelReport report, ExportReportFormat format, ExpandoObject query, Action<ExpandoObject> setParams);
	}

	public interface IModelReport : IModelBase
	{
		String Name { get; }

		String ReportPath();

		IModelReportHandler GetReportHandler(IServiceProvider serviceProvider);

		ExpandoObject CreateParameters(ExpandoObject query, Action<ExpandoObject> setParams);
		ExpandoObject CreateVariables(ExpandoObject query, Action<ExpandoObject> setParams);
	}

	public interface IModelJsonReader
	{
		Task<IModelView> GetViewAsync(IPlatformUrl url);
		Task<IModelBlob> GetBlobAsync(IPlatformUrl url, String suffix = null);
		Task<IModelCommand> GetCommandAsync(IPlatformUrl url, String command);
		Task<IModelReport> GetReportAsync(IPlatformUrl url);
	}
}
