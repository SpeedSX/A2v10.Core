﻿// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using A2v10.Infrastructure;
using System.Text;

namespace A2v10.Xaml;

public abstract class UIElement : UIElementBase
{
	public Boolean? Bold { get; set; }
	public Boolean? Italic { get; set; }
	public String? CssClass { get; set; }
	public Boolean? UserSelect { get; set; }
	public Boolean? Print { get; set; }


	public override void MergeAttributes(TagBuilder tag, RenderContext context, MergeAttrMode mode = MergeAttrMode.All)
	{
		base.MergeAttributes(tag, context, mode);

		var boldBind = GetBinding(nameof(Bold));
		var italicBind = GetBinding(nameof(Italic));
		var cssBind = GetBinding(nameof(CssClass));
		if (cssBind != null)
		{
			tag.MergeAttribute(":class", cssBind.GetPath(context));
			if (boldBind != null || italicBind != null)
				throw new XamlException("CssClass binding is incompatible with Bold and Italic bindings");
		}
		else if (boldBind != null || italicBind != null)
		{
			var sb = new StringBuilder("{");
			if (boldBind != null)
				sb.Append($"bold: {boldBind.GetPath(context)}, ");
			if (italicBind != null)
				sb.Append($"italic: {italicBind.GetPath(context)}, ");
			sb.RemoveTailComma();
			sb.Append('}');
			tag.MergeAttribute(":class", sb.ToString());
		}
		tag.AddCssClassBoolNo(Bold, "bold");
		tag.AddCssClassBoolNo(Italic, "italic");
		tag.AddCssClassBoolNo(UserSelect, "user-select");
		tag.AddCssClassBoolNo(Print, "print");
		tag.AddCssClass(CssClass);
	}
}
