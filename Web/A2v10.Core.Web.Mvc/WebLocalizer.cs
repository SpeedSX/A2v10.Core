﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

using A2v10.Data.Interfaces;
using A2v10.Infrastructure;

namespace A2v10.Core.Web.Mvc
{
	public class WebLocalizer : BaseLocalizer, IDataLocalizer
	{
		private readonly ILocalizerDictiorany _dictionary;

		public WebLocalizer(ILocalizerDictiorany dictiorany, IUserLocale userLocale)
			:base(userLocale)
		{
			_dictionary = dictiorany;
		}

		#region IDataLocalizer
		public String Localize(String content)
		{
			return Localize(null, content, true);
		}
		#endregion

		protected override IDictionary<String, String> GetLocalizerDictionary(String locale)
		{
			return _dictionary.GetLocalizerDictionary(locale);
		}
	}
}
