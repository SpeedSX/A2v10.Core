﻿// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

namespace A2v10.Xaml
{
	public class Static : ValuedControl, ITableControl
	{
		public TextAlign Align { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
		{
			if (SkipRender(context))
				return;
			var input = new TagBuilder("static", null, IsInGrid);
			onRender?.Invoke(input);
			MergeAttributes(input, context);
			MergeValue(input, context); // item, prop for validator
			MergeAlign(input, context, Align);
			SetSize(input, nameof(Static));
			var valBind = GetBinding(nameof(Value));
			if (valBind != null)
			{
				input.MergeAttribute(":text", valBind.GetPathFormat(context)); // formatted
				if (valBind.NegativeRed)
					input.MergeAttribute(":class", $"$getNegativeRedClass({valBind.GetPath(context)})");
			}
			input.RenderStart(context);
			RenderAddOns(context);
			input.RenderEnd(context);
		}
	}
}
