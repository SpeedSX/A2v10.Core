﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.Infrastructure
{
	public abstract class BaseLocalizer : ILocalizer
	{
		protected abstract IDictionary<String, String> GetLocalizerDictionary(String locale);

		private readonly String _defaultLocale;

		public BaseLocalizer(String defaultLocale)
		{
			_defaultLocale = defaultLocale;
		}

		protected virtual String DefaultLocale => _defaultLocale;

		String GetLocalizedValue(String locale, String key)
		{
			if (locale == null)
				locale = DefaultLocale;
			var dict = GetLocalizerDictionary(locale);
			if (dict.TryGetValue(key, out String value))
				return value;
			return key;
		}

		public String Localize(String locale, String content, Boolean replaceNewLine = true)
		{
			if (content == null)
				return null;
			String s = content;
			if (replaceNewLine)
				s = content.Replace("\\n", "\n");
			var sb = new StringBuilder();
			Int32 xpos = 0;
			String key;
			do
			{
				Int32 start = s.IndexOf("@[", xpos);
				if (start == -1)
					break;
				Int32 end = s.IndexOf("]", start + 2);
				if (end == -1)
					break;
				else
					key = "@" + s.Substring(start + 2, end - start - 2);

				var value = GetLocalizedValue(locale, key);
				sb.Append(s[xpos..start]);
				sb.Append(value);
				xpos = end + 1;
			} while (true);
			// tail!
			sb.Append(s[xpos..]);
			return sb.ToString();
		}
	}
}
