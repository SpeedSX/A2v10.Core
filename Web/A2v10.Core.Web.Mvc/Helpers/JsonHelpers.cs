﻿// Copyright © 2019-2020 Alex Kukhtin. All rights reserved.

using Newtonsoft.Json;

namespace A2v10.Core.Web.Mvc
{
	public class JsonHelpers
	{
		public static readonly JsonSerializerSettings StandardSerializerSettings =
			new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				StringEscapeHandling = StringEscapeHandling.EscapeHtml,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};

		public static readonly JsonSerializerSettings ReleaseSerializerSettings =
			new JsonSerializerSettings()
			{
				Formatting = Formatting.None,
				StringEscapeHandling = StringEscapeHandling.EscapeHtml,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};

		public static JsonSerializerSettings ConfigSerializerSettings(bool bDebug)
		{
			return bDebug ? StandardSerializerSettings : ReleaseSerializerSettings;
		}
	}
}
