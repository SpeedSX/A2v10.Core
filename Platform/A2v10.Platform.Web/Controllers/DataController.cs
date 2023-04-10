﻿// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using A2v10.Infrastructure;

namespace A2v10.Platform.Web.Controllers;

[Route("_data/[action]")]
[Route("admin/_data/[action]")]
[ExecutingFilter]
[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class DataController : BaseController
{
	private readonly IDataService _dataService;

	public DataController(IApplicationHost host,
		ILocalizer localizer, ICurrentUser currentUser, IProfiler profiler, IDataService dataService)
		: base(host, localizer, currentUser, profiler)
	{
		_dataService = dataService;
	}

	[HttpPost]
	public async Task<IActionResult> Reload()
	{
		return await TryCatch(async () =>
		{ 
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			var baseUrl = eo.Get<String>("baseUrl") 
				?? throw new InvalidReqestExecption(nameof(Reload));
			String data = await _dataService.ReloadAsync(baseUrl, SetSqlQueryParams);
			return new WebActionResult(data);
		});
	}

	[HttpPost]
	public Task<IActionResult> Expand()
	{
		return TryCatch(async () =>
		{
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			var expandData = await _dataService.ExpandAsync(eo, SetSqlQueryParams);

			return new WebActionResult(expandData);
		});
	}

	[HttpPost]
	public Task<IActionResult> LoadLazy()
	{
		return TryCatch(async () =>
		{
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			var lazyData = await _dataService.LoadLazyAsync(eo, SetSqlQueryParams);

			return new WebActionResult(lazyData);
		});
	}

	[HttpPost]
	public Task<IActionResult> Save()
	{
		return TryCatch(async () =>
		{
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			String? baseUrl = eo.Get<String>("baseUrl") 
				?? throw new InvalidReqestExecption(nameof(Save));
			ExpandoObject data = eo.GetNotNull<ExpandoObject>("data");

			var savedData = await _dataService.SaveAsync(baseUrl, data, SetSqlQueryParams);

			return new WebActionResult(savedData);
		});
	}

	[HttpPost]
	public Task<IActionResult> Invoke()
	{
		return TryCatch(async () =>
		{
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			String? baseUrl = eo.Get<String>("baseUrl") 
				?? throw new InvalidReqestExecption(nameof(Invoke));
			String? cmd = eo.Get<String>("cmd") 
				?? throw new InvalidReqestExecption(nameof(Invoke));
			ExpandoObject? data = eo.Get<ExpandoObject>("data");

			var result = await _dataService.InvokeAsync(baseUrl, cmd, data, SetSqlQueryParams);
			return new WebBinaryActionResult(result.Body, result.ContentType);
		});
	}

	[HttpPost]
	public Task<IActionResult> DbRemove()
	{
		return TryCatch(async () =>
		{
			var eo = await Request.ExpandoFromBodyAsync() 
				?? throw new InvalidReqestExecption(Request.Path);
			var baseUrl = eo.Get<String>("baseUrl") 
				?? throw new InvalidReqestExecption(nameof(DbRemove));
			Object id = eo.GetNotNull<Object>("id");
			String propName = eo.GetNotNull<String>("prop");

			await _dataService.DbRemoveAsync(baseUrl, id,  propName, SetSqlQueryParams);

			return new WebActionResult("{\"status\": \"OK\"}");
		});
	}

	[HttpPost]
	public async Task<IActionResult> ExportTo()
	{
		var eo = await Request.ExpandoFromBodyAsync() 
			?? throw new InvalidReqestExecption(Request.Path);
		try
		{
			//String format = eo.Get<String>("format");
			var data = _dataService.Html2Excel(eo.GetNotNull<String>("html"));
			return new WebBinaryActionResult(data, MimeTypes.Application.OctetBinary);
		} 
		catch (Exception ex)
		{
			return WriteExceptionStatus(ex);
		}
	}

	private async Task<IActionResult> TryCatch(Func<Task<IActionResult>> action)
	{
		try
		{
			return await action();
		} 
		catch (Exception ex)
		{
			return WriteExceptionStatus(ex);
		}
	}
}
