﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace A2v10.Services.Javascript;
public static class FetchCommand
{
	static void SetHeaders(HttpWebRequest wr, ExpandoObject? headers)
	{
		if (headers == null)
			return;
		var d = headers as IDictionary<String, Object>;
		foreach (var hp in d)
			wr.Headers.Add(hp.Key, hp.Value.ToString());
	}

	static void AddAuthorization(HttpWebRequest wr, ExpandoObject? auth)
	{
		if (auth == null)
			return;
		var type = auth.Get<String>("type");
		switch (type)
		{
			case "apiKey":
				var apiKey = auth.Get<String>("apiKey");
				wr.Headers.Add("Authorization", $"ApiKey {apiKey}");
				break;
			case "basic":
				var name = auth.Get<String>("name");
				var password = auth.Get<String>("password");
				var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(name + ":" + password));
				wr.Headers.Add("Authorization", $"Basic {encoded}");
				break;
			case "bearer":
				var token = auth.Get<String>("token");
				wr.Headers.Add("Authorization", $"Bearer {token}");
				break;
			default:
				throw new InvalidOperationException($"Invalid Authorization type ({type})");
		}
	}

	static String CreateQueryString(ExpandoObject? query)
	{
		if (query == null || query.IsEmpty())
			return String.Empty;
		var elems = (query as IDictionary<String, Object>)
			.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value.ToString()!)}");
		var ts = String.Join("&", elems);
		if (String.IsNullOrEmpty(ts))
			return String.Empty;
		return "?" + ts;
	}

	static ExpandoObject? GetResponseHeaders(WebHeaderCollection? headers)
	{
		if (headers == null)
			return null;
		var eo = new ExpandoObject();
		foreach (var key in headers.AllKeys)
		{
			eo.Set(key, headers[key]);
		}
		return eo;
	}

	public static FetchResponse Execute(String url, ExpandoObject? prms)
	{
		try
		{
			var httpWebRequest = WebRequest.CreateHttp(url + CreateQueryString(prms?.Get<ExpandoObject>("query")));

			String mtd = prms?.Get<String>("method")?.ToUpperInvariant() ?? "GET";
			AddAuthorization(httpWebRequest, prms?.Get<ExpandoObject>("authorization"));
			SetHeaders(httpWebRequest, prms?.Get<ExpandoObject>("headers"));

			if (mtd == "POST")
			{
				httpWebRequest.Method = mtd;
				var bodyObj = prms?.Get<Object>("body");
				String? bodyStr = null;

				switch (bodyObj)
				{
					case String strObj:
						bodyStr = strObj;
						break;
					case ExpandoObject eoObj:
						bodyStr = JsonConvert.SerializeObject(eoObj, new JsonDoubleConverter());
						httpWebRequest.ContentType = MimeTypes.Application.Json;
						break;
				}

				if (bodyStr != null)
				{
					var bytes = Encoding.GetEncoding("UTF-8").GetBytes(bodyStr);
					using var rqs = httpWebRequest.GetRequestStream();
					rqs.Write(bytes, 0, bytes.Length);
				}
			}

			using var respRaw = httpWebRequest.GetResponse();
			if (respRaw is not HttpWebResponse resp)
				throw new InvalidProgramException("Invalid response type");
			var contentType = resp.ContentType;
			var headers = resp.Headers;
			using var rs = resp.GetResponseStream();
			using var ms = new StreamReader(rs);
			String strResult = ms.ReadToEnd();
			return new FetchResponse(
				resp.StatusCode,
				contentType,
				strResult,
				GetResponseHeaders(headers),
				resp.StatusDescription);
		}
		catch (WebException wex)
		{
			if (wex.Response != null && wex.Response is HttpWebResponse webResp)
			{
				using var rs = new StreamReader(wex.Response.GetResponseStream());
				String strError = rs.ReadToEnd();
				var headers = wex.Response.Headers;
				// set headers
				return new FetchResponse(
					webResp.StatusCode,
					wex.Response.ContentType,
					strError,
					GetResponseHeaders(headers),
					webResp.StatusDescription);
			}
			else
				throw;
		}
	}
}

