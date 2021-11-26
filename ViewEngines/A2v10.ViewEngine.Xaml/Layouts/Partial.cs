﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using A2v10.Infrastructure;
using System;

namespace A2v10.Xaml
{
	public class Partial : RootContainer
	{
		public BackgroundStyle Background { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
		{
			if (SkipRender(context))
				return;
			var page = new TagBuilder("div", "page partial");
			page.MergeAttribute("id", context.RootId);
			MergeAttributes(page, context, MergeAttrMode.Margin);

			if (Background != BackgroundStyle.Default)
				page.AddCssClass("background-" + Background.ToString().ToKebabCase());
			page.AddCssClass(CssClass);

			page.RenderStart(context);
			RenderChildren(context);
			RenderAccelCommands(context);
			page.RenderEnd(context);
		}
	}
}
