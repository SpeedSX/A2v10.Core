﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Infrastructure
{
	public interface IDataLoadResult
	{
		public IDataModel Model { get; }
		public IModelView View { get; }
	}

	public interface IBlobInfo
	{
		String Mime { get; }
		String Name { get; }
		Guid Token { get; }
		Byte[] Stream { get; }
		String BlobName { get; }
	}

	public interface IInvokeResult
	{
		Byte[] Body { get; }
		String ContentType { get; }
	}

	public interface IDataService
	{
		Task<IDataLoadResult> Load(UrlKind kind, String baseUrl, Action<ExpandoObject> setParams);
		Task<IDataLoadResult> Load(String baseUrl, Action<ExpandoObject> setParams);

		Task<String> ReloadAsync(String baseUrl, Action<ExpandoObject> setParams);
		Task<String> LoadLazyAsync(String baseUrl, Object Id, String propertyName, Action<ExpandoObject> setParams);
		Task<String> ExpandAsync(String baseUrl, Object id, Action<ExpandoObject> setParams);
		Task<String> SaveAsync(String baseUrl, ExpandoObject data, Action<ExpandoObject> setParams);
		Task DbRemoveAsync(String baseUrl, Object Id, String propertyName, Action<ExpandoObject> setParams);

		Task<IInvokeResult> InvokeAsync(String baseUrl, String command, ExpandoObject data, Action<ExpandoObject> setParams);

		Task<IBlobInfo> LoadBlobAsync(UrlKind kind, String baseUrl, Action<ExpandoObject> setParams, String suffix = null);
		Task<IBlobInfo> StaticImage(String baseUrl);
	}
}