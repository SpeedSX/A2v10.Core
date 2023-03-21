﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Infrastructure;

namespace A2v10.Xaml;

public class ColorPicker : ValuedControl, ITableControl
{
	public String ?Text { get; set; }
	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (CheckDisabledModel(context))
			return;
		var input = new TagBuilder("a2-color-picker", null, IsInGrid);
		onRender?.Invoke(input);
		MergeAttributes(input, context);
		MergeDisabled(input, context);
		CheckValueType(context, TypeCheckerTypeCode.Date);
		MergeValue(input, context);
		MergeBindingAttributeString(input, context, "text", nameof(Text), Text);
		input.RenderStart(context);
		RenderAddOns(context);
		input.RenderEnd(context);
	}
}
