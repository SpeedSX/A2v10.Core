﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Xaml
{
	[ContentProperty("Content")]
	public class Repeater : UIElement
	{
		public Object? ItemsSource { get; set; }
		public UIElementBase? Content { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
		{
			if (SkipRender(context))
				return;
			var isBind = GetBinding(nameof(ItemsSource));
			if (isBind == null)
				return;
			var div = new TagBuilder("template", null, IsInGrid);
			onRender?.Invoke(div);
			MergeAttributes(div, context);
			div.MergeAttribute("v-for", $"(elem, elemIndex) in {isBind.GetPath(context)}");
			div.RenderStart(context);
			if (Content != null)
			{
				using (new ScopeContext(context, "elem", isBind.Path))
				{
					Content.RenderElement(context, (tag)=> 
					{
						onRender?.Invoke(tag);
						tag.MergeAttribute(":key", "elemIndex");
					});
				}
			}
			div.RenderEnd(context);
		}

		protected override void OnEndInit()
		{
			base.OnEndInit();
			Content?.SetParent(this);
		}

		public override void OnSetStyles()
		{
			base.OnSetStyles();
			Content?.OnSetStyles();
		}
	}
}
