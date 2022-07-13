﻿// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using A2v10.Infrastructure;

namespace A2v10.Xaml;

public enum DatePickerView
{
	Day,
	Month
}

public class DatePicker : ValuedControl, ITableControl
{

	public TextAlign Align { get; set; }
	public DropDownPlacement Placement { get; set; }
	public DatePickerView View { get; set; }

	public String? YearCutOff { get; set; }

	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (CheckDisabledModel(context))
			return;
		var input = new TagBuilder("a2-date-picker", null, IsInGrid);
		onRender?.Invoke(input);
		MergeAttributes(input, context);
		MergeDisabled(input, context);
		MergeAlign(input, context, Align);
		input.MergeAttribute("year-cut-off", YearCutOff);
		SetSize(input, nameof(DatePicker));
		if (Placement  != DropDownPlacement.BottomLeft)
			input.AddCssClass("drop-" + Placement.ToString().ToKebabCase());
		CheckValueType(context, TypeCheckerTypeCode.Date);
		MergeValue(input, context);
		input.MergeAttribute("view", View.ToString().ToLowerInvariant());
		input.RenderStart(context);
		RenderAddOns(context);
		input.RenderEnd(context);
	}

	protected override void OnEndInit()
	{
		base.OnEndInit();
		if (Align == TextAlign.Default)
			Align = TextAlign.Center;
	}
}
