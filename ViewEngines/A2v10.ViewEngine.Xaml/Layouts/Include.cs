﻿// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Xaml
{
	public class Include : UIElementBase
	{
		public String Source { get; set; }
		public Object Argument { get; set; }
		public Object Data { get; set; }

		public Boolean FullHeight { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder> onRender = null)
		{
			if (SkipRender(context))
				return;
			var div = new TagBuilder("a2-include");
			MergeAttributes(div, context);
			var src = GetBinding(nameof(Source));
			if (src != null)
				div.MergeAttribute(":source", src.GetPathFormat(context));
			else if (Source != null)
				div.MergeAttribute("source", Source);
			else
				throw new XamlException("Partial. Source must be specified");
			var arg =  GetBinding(nameof(Argument));
			if (arg != null)
				div.MergeAttribute(":arg", arg.GetPathFormat(context));
			else if (Argument != null)
				div.MergeAttribute("arg", Argument.ToString());
			var dat = GetBinding(nameof(Data));
			if (dat != null)
				div.MergeAttribute(":dat", dat.GetPathFormat(context));
			div.AddCssClassBool(FullHeight, "full-height");
			div.RenderStart(context);
			div.RenderEnd(context);
		}
	}
}
