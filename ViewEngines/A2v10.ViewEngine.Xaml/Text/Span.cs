﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Xaml;

public enum SpaceMode
{
	None,
	Before,
	After,
	Both
}

[ContentProperty("Content")]
public class Span : Inline
{
	public Object? Content { get; set; }
	public Boolean Small { get; set; }
	public Boolean Big { get; set; }
	public SpaceMode Space { get; set; }
	public UInt32 MaxChars { get; set; }
	public Int32 LineClamp { get; set; }

	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;
		if (Space == SpaceMode.Before || Space == SpaceMode.Both)
			context.Writer.Write("&#160;");
		var span = new TagBuilder("span", null, IsInGrid);
		onRender?.Invoke(span);
		MergeAttributes(span, context);

		var cbind = GetBinding(nameof(Content));
		if (cbind != null)
		{
			span.MergeAttribute("v-text", MaxChars > 0 ? $"$maxChars({cbind.GetPathFormat(context)}, {MaxChars})" : cbind.GetPathFormat(context));
			if (cbind.NegativeRed)
			{
				if (GetBinding(nameof(Color)) != null)
					throw new XamlException("NegativeRed property is not compatible with Color binding");
				span.MergeAttribute(":class", $"$getNegativeRedClass({cbind.GetPath(context)})");
			}
		}
		span.AddCssClassBool(Small, "small");
		span.AddCssClassBool(Big, "text-big");

		if (LineClamp != 0)
		{
			span.AddCssClass("line-clamp");
			span.MergeStyle("-webkit-line-clamp", LineClamp.ToString());
		}

		span.RenderStart(context);
		if (Content is String)
			context.Writer.Write(context.LocalizeCheckApostrophe(Content.ToString()));
		span.RenderEnd(context);
		if (Space == SpaceMode.After || Space == SpaceMode.Both)
			context.Writer.Write("&#160;");
	}
}
