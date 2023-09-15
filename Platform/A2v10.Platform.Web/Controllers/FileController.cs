﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

using Newtonsoft.Json;

using A2v10.Infrastructure;
using A2v10.Data.Interfaces;

namespace A2v10.Platform.Web.Controllers;

[ExecutingFilter]
[Authorize]
public class FileController : BaseController
{
	private readonly IDataService _dataService;
	private readonly ITokenProvider _tokenProvider;
	private readonly IAppCodeProvider _appCodeProvider;

	public FileController(IApplicationHost host,
		ILocalizer localizer, ICurrentUser currentUser, IProfiler profiler, 
		IDataService dataService, ITokenProvider tokenProvider, IAppCodeProvider appCodeProvider)
		: base(host, localizer, currentUser, profiler)
	{
		_dataService = dataService;
		_tokenProvider = tokenProvider;
		_appCodeProvider = appCodeProvider;
	}

	[Route("_file/{*pathInfo}")]
	[HttpGet]
	public async Task<IActionResult> DefaultGet(String pathInfo)
	{
		try
		{
			var blob = await _dataService.LoadBlobAsync(UrlKind.File, pathInfo, SetSqlQueryParams)
				?? throw new InvalidOperationException("Blob not found");

            if (blob.CheckToken)
                ValidateToken(blob.Token);

            if (blob.Stream == null)
				throw new InvalidReqestExecption($"Blob not found. ({pathInfo})");

			var ar = new WebBinaryActionResult(blob.Stream, blob.Mime ?? MimeTypes.Text.Plain);
			if (Request.Query["export"].Count > 0)
			{
				var cdh = new ContentDispositionHeaderValue("attachment")
				{
					FileNameStar = _localizer.Localize(null, blob.Name)
				};
				ar.AddHeader("Content-Disposition", cdh.ToString());
			}
			else if (MimeTypes.IsImage(blob.Mime))
			{
				ar.EnableCache();
			}
			return ar;
		}
		catch (Exception ex)
		{
			var accept = Request.Headers["Accept"].ToString();
			if (accept != null && accept.Trim().StartsWith("image", StringComparison.OrdinalIgnoreCase))
				return WriteImageException(ex);
			else
				return WriteExceptionStatus(ex);
		}
	}

	[Route("_file/{*pathInfo}")]
	[HttpPost]
	public async Task<IActionResult> DefaultPost(String pathInfo)
	{
		try
		{
			var files = Request.Form.Files;
			if (files.Count == 0)
				throw new InvalidOperationException("No files");
			var file = files[0];

			using var fileStream = file.OpenReadStream();
            var name = Path.GetFileName(file.FileName);

            var result = await _dataService.SaveFileAsync(pathInfo, (blob) =>
				{
					blob.Stream = fileStream;
					blob.Name = name;
					blob.Mime = file.ContentType;
				}, 
				SetSqlQueryParams
			);
            String json = JsonConvert.SerializeObject(result, JsonHelpers.StandardSerializerSettings);
            return Content(json, MimeTypes.Application.Json);
        }
        catch (Exception ex)
		{
			return WriteExceptionStatus(ex);
		}
	}

	void ValidateToken(Guid dbToken)
	{
		var token = Request.Query["token"];
		if (token.Count == 0)
			throw new InvalidReqestExecption("Invalid token");
		var strToken = token[0] ??
			throw new InvalidReqestExecption("Invalid token");
		ValidateToken(dbToken, strToken);
	}

    void ValidateToken(Guid dbToken, String token)
	{
		var generated = _tokenProvider.GenerateToken(dbToken);
		if (generated == token)
			return;
		throw new InvalidReqestExecption("Invalid image token");
	}

    [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Client)]
    [Route("file/{*pathInfo}")]
	[HttpGet]
	public IActionResult LoadFile(String pathInfo)
	{
		try
		{
			Int32 ix = pathInfo.LastIndexOf('-');
			if (ix != -1)
				pathInfo = pathInfo[..ix] + "." + pathInfo[(ix + 1)..];
			if (!new FileExtensionContentTypeProvider().TryGetContentType(pathInfo, out String? contentType))
				contentType = MimeTypes.Application.OctetStream;
            // without using! The FileStreamResult will close stream
            var stream = _appCodeProvider.FileStreamRO(_appCodeProvider.MakePath("_files/", pathInfo))
                ?? throw new FileNotFoundException($"File not found '{pathInfo}'");
			return new FileStreamResult(stream, contentType)
			{
				FileDownloadName = Path.GetFileName(pathInfo)
			};
		}
		catch (Exception ex)
		{
			return WriteHtmlException(ex);
		}
	}
}

