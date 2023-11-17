﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace A2v10.Xaml
{
	public enum RowStyle
	{
		Default,
		Title,
		Parameter,
		LastParameter,
		Header,
		LightHeader,
		Footer,
		Total,
		NoBorder,
		PageHeader,
		Divider
	}

	[ContentProperty("Cells")]
	public class SheetRow : UIElement
	{
		public SheetCells Cells { get; } = [];

		public RowStyle Style { get; set; }
		public TextAlign? Align { get; set; }

		public MarkStyle Mark { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
		{
			if (SkipRender(context))
				return;
			var tr = new TagBuilder("tr");
			onRender?.Invoke(tr);
			MergeAttributes(tr, context);

			var markBind = GetBinding(nameof(Mark));
			if (markBind != null)
			{
				if (GetBinding(nameof(Bold)) != null)
					throw new XamlException("The Bold and Mark bindings cannot be used at the same time");
				var class2 = tr.GetAttribute(":class");
				if (!String.IsNullOrEmpty(class2))
				{
					tr.MergeAttribute(":class", $"row.rowCssClass({markBind.GetPathFormat(context)})", replaceExisting: true);
				}
				else
					tr.MergeAttribute(":class", markBind.GetPathFormat(context));
			}
			else if (Mark != MarkStyle.Default)
				tr.AddCssClass(Mark.ToString().ToKebabCase());

			if (Style != RowStyle.Default)
				tr.AddCssClass("row-" + Style.ToString().ToKebabCase());
			if (Align != null)
				tr.AddCssClass("text-" + Align.ToString()!.ToLowerInvariant());
			tr.RenderStart(context);
			foreach (var c in Cells)
				c.RenderElement(context);
			tr.RenderEnd(context);
		}

		protected override void OnEndInit()
		{
			base.OnEndInit();
			foreach (var c in Cells)
				c.SetParent(this);
		}

		public override void OnSetStyles(RootContainer root)
		{
			base.OnSetStyles(root);
			foreach (var c in Cells)
				c.OnSetStyles(root);
		}
	}

	[TypeConverter(typeof(SheetRowsConverter))]
	public class SheetRows : List<SheetRow>
	{
	}

	public class SheetRowsConverter : TypeConverter
	{
		public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			if (sourceType == typeof(String))
				return true;
			return false;
		}

		public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
		{
			if (value == null)
				return null;
			if (value is String strVal)
			{
				var rows = new SheetRows();
				var row = new SheetRow();
				rows.Add(row);
				foreach (var s in strVal.Split(','))
				{
					row.Cells.Add(new SheetCell() { Content = s.Trim() });
				}
				return rows;
			}
			throw new XamlException($"Invalid SheetRows value '{value}'");
		}
	}
}
