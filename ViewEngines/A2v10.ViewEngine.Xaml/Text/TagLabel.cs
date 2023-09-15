﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Xaml;


public enum TagLabelStyle
{
	Default,
	Success,
	Green,
	Warning,
	Orange,
	Info,
	Cyan,
	Danger,
	Red,
	Error,
	Purple,
	Pink,
	Gold,
	Blue,
	Salmon,
	Seagreen,
	Tan,
	Magenta,
	LightGray,
	Olive,
	Teal
}

[ContentProperty("Content")]
public class TagLabel : Inline
{
	public String? Content { get; set; }
	public TagLabelStyle Style { get; set; }
        public Boolean Outline { get; set; }

        public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;
		var span = new TagBuilder("span", "tag-label", IsInGrid);
		MergeAttributes(span, context);
            span.AddCssClassBool(Outline, "outline");

            var cbind = GetBinding(nameof(Content));
		if (cbind != null)
			span.MergeAttribute("v-text", cbind.GetPathFormat(context));

		var cStyle = GetBinding(nameof(Style));
		if (cStyle != null)
		{
			span.MergeAttribute(":class", cStyle.GetPathFormat(context));
		}
		else if (Style != TagLabelStyle.Default)
		{
			span.AddCssClass(Style.ToString().ToLowerInvariant());
		}

		span.RenderStart(context);
		if (Content is String strCont)
			context.Writer.Write(context.LocalizeCheckApostrophe(strCont));
		span.RenderEnd(context);
	}
}
