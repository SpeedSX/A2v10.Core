﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using A2v10.Infrastructure;
using A2v10.System.Xaml;

namespace A2v10.Xaml
{

	public enum ToolbarStyle
	{
		Default,
		Transparent,
		Light
	}

	public enum ToolbarBorderStyle
	{
		None,
		Bottom,
		BottomShadow
	}

	[AttachedProperties("Align")]
	public class Toolbar : Container
	{

		public enum ToolbarAlign
		{
			Left,
			Right
		}


		private readonly IAttachedPropertyManager _attachedPropertyManager;

		public Toolbar(IServiceProvider serviceProvider)
		{
			_attachedPropertyManager = serviceProvider.GetService(typeof(IAttachedPropertyManager)) as IAttachedPropertyManager;
		}

		#region Attached Properties

		public ToolbarAlign GetAlgin(Object obj)
		{
			return _attachedPropertyManager.GetProperty<ToolbarAlign>("Toolbar.Align", obj);
		}

		#endregion

		public ToolbarStyle Style { get; set; }
		public ToolbarBorderStyle Border { get; set; }
		public AlignItems AlignItems { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder> onRender = null)
		{
			if (SkipRender(context))
				return;
			var tb = new TagBuilder("div", "toolbar", IsInGrid);
			onRender?.Invoke(tb);
			if (Style != ToolbarStyle.Default)
				tb.AddCssClass(Style.ToString().ToKebabCase());
			if (AlignItems != AlignItems.Default)
				tb.AddCssClass("align-" + AlignItems.ToString().ToLowerInvariant());
			MergeAttributes(tb, context);
			if (Border != ToolbarBorderStyle.None)
				tb.AddCssClass("tb-border-" + Border.ToString().ToKebabCase());
			tb.RenderStart(context);
			RenderChildren(context);
			tb.RenderEnd(context);
		}

		public override void RenderChildren(RenderContext context, Action<TagBuilder> onRenderStatic = null)
		{
			var rightList = new List<UIElementBase>();
			Boolean bFirst = true;
			foreach (var ch in Children)
			{
				if (GetAlgin(ch) == ToolbarAlign.Right)
					rightList.Add(ch);
				else
				{
					if (bFirst)
						ch.RenderElement(context, (tag) => tag.AddCssClass("first-elem"));
					else
						ch.RenderElement(context);
					bFirst = false;
				}
			}
			if (rightList.Count == 0)
				return;

			// aligner
			new TagBuilder("div", "aligner").Render(context);

			// right elements
			foreach (var ch in rightList)
				ch.RenderElement(context);
		}
	}
}
