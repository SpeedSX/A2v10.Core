﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System.IO;

using A2v10.Infrastructure;

namespace A2v10.Xaml;
public class XamlRenderer : IRenderer
{
	private readonly IProfiler _profile;
	private readonly ILocalizer _localizer;
	private readonly IXamlPartProvider _partProvider;

	public XamlRenderer(IProfiler profile, IXamlPartProvider partProvider, ILocalizer localizer)
	{
		_profile = profile;
		_localizer = localizer;
		_partProvider = partProvider;
	}

	public void Render(IRenderInfo info, TextWriter writer)
	{
		if (String.IsNullOrEmpty(info.FileName))
			throw new XamlException("No source for render");
		IProfileRequest request = _profile.CurrentRequest;
		String fileName = String.Empty;
		IXamlElement? uiElem = null;

		var xamlServiceOptions = new XamlServicesOptions(Array.Empty<NamespaceDef>())
		{
			OnCreateReader = (rdr) =>
			{
				rdr.InjectService<IXamlPartProvider>(_partProvider);
			}
		};

		using (request.Start(ProfileAction.Render, $"load: {info.FileTitle}"))
		{
			// XamlServices.Load sets IUriContext
			if (!String.IsNullOrEmpty(info.FileName))
			{
				uiElem = _partProvider.GetXamlPart(info.FileName) as IXamlElement; 
			}
			//else if (!String.IsNullOrEmpty(info.Text))
				//uiElem = _xamlReader.ParseXml(info.Text) as IXamlElement;
			else
				throw new XamlException("Xaml. There must be either a 'FileName' or a 'Text' property");
			if (uiElem == null)
				throw new XamlException("Xaml. Root is not 'IXamlElement'");

			var stylesPart = _partProvider.GetXamlPart("styles.xaml");
			if (stylesPart != null)
			{
				if (stylesPart is not Styles styles)
					throw new XamlException("Xaml. Styles is not 'Styles'");
				if (uiElem is IRootContainer root)
				{
					root.SetStyles(styles);
				}
			}
		}

		using (request.Start(ProfileAction.Render, $"Render: {info.FileTitle}"))
		{
			RenderContext ctx = new(uiElem, info, _localizer, writer)
			{
				RootId = info.RootId,
				Path = info.Path
			};

			if (info.SecondPhase)
			{
				if (uiElem is not ISupportTwoPhaseRendering twoPhaseRender)
					throw new XamlException("The two-phase rendering is not available");
				twoPhaseRender.RenderSecondPhase(ctx);
			}
			else
			{
				uiElem.RenderElement(ctx);
			}
		}

		if (uiElem is IDisposable disp)
		{
			disp.Dispose();
		}
	}
}

