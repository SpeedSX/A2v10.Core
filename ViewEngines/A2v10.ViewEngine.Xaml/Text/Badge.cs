﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Xaml;

[ContentProperty("Content")]
public class Badge : Inline
{
	public Object? Content { get; set; }
	public Boolean Small { get; set; }

	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;
		var span = new TagBuilder("span", "a2-badge", IsInGrid);
		onRender?.Invoke(span);
		MergeAttributes(span, context);

		var cbind = GetBinding(nameof(Content));
		if (cbind != null)
		{
			span.MergeAttribute("v-text", cbind.GetPathFormat(context));
			if (cbind.NegativeRed)
				span.MergeAttribute(":class", $"$getNegativeRedClass({cbind.GetPath(context)})");
		}
		span.AddCssClassBool(Small, "small");

		span.RenderStart(context);
		if (Content is String)
			context.Writer.Write(context.LocalizeCheckApostrophe(Content.ToString()));
		span.RenderEnd(context);
	}
}

