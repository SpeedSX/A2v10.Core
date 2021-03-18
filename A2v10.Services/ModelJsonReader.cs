﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

using A2v10.Infrastructure;

namespace A2v10.Services
{

	public class ModelJsonReader : IModelJsonReader
	{
		private readonly IAppCodeProvider _appCodeProvider;
		private readonly IUserStateManager _userStateManager;

		private readonly RedirectModule _redirect;

		public ModelJsonReader(IAppCodeProvider appCodeProvider, IAppConfiguration appConig, IUserStateManager userStateManager)
		{
			_appCodeProvider = appCodeProvider;
			_userStateManager = userStateManager;
			var redPath = _appCodeProvider.MakeFullPath(String.Empty, "redirect.json", false);
			if (appCodeProvider.FileExists(redPath))
				_redirect = new RedirectModule(redPath, appConig.Watch);
		}

		public async Task<IModelView> GetViewAsync(IPlatformUrl url)
		{
			var rm = await Load(url);
			return url.Kind switch
			{
				UrlKind.Page => rm.GetAction(url.Action),
				UrlKind.Dialog => rm.GetDialog(url.Action),
				UrlKind.Popup => rm.GetPopup(url.Action),
				_ => null,
			};
		}

		public async Task<IModelCommand> GetCommandAsync(IPlatformUrl url, String command)
		{
			var rm = await Load(url);
			return rm.GetCommand(command);
		}

		public async Task<IModelReport> GetReportAsync(IPlatformUrl url)
		{
			var rm = await Load(url);
			return rm.GetReport(url.Action);
		}

		public async Task<IModelBlob> GetBlobAsync(IPlatformUrl url, String suffix = null)
		{
			var rm = await Load(url);
			return url.Kind switch
			{
				UrlKind.Image => rm.GetBlob(url.Action, suffix),
				_ => null
			};
		}

		public async Task<ModelJson> Load(IPlatformUrl url)
		{
			var localPath = _redirect.Redirect(url.LocalPath);
			url.Redirect(localPath);
			String json = await _appCodeProvider.ReadTextFileAsync(url.LocalPath, "model.json", _userStateManager.IsAdmin);
			var rm = JsonConvert.DeserializeObject<ModelJson>(json, JsonHelpers.CamelCaseSerializerSettings);
			rm.OnEndInit(url);
			return rm;
		}
	}
}
