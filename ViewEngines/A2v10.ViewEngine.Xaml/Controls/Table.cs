﻿// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.Infrastructure;

namespace A2v10.Xaml;

public enum TableBackgroundStyle
{
	None,
	Paper,
	Yellow,
	Cyan,
	Rose,
	WhiteSmoke,
	White,
	Primary
}

public enum CellSpacingMode
{
	None,
	Small,
	Medium,
	Large
}


[ContentProperty("Rows")]
public class Table : Control, ITableControl
{
	public GridLinesVisibility GridLines { get; set; }

	public TableRowCollection Rows { get; set; } = [];

	public Boolean Border { get; set; }
	public Boolean Compact { get; set; }
	public Boolean Hover { get; set; }
	public Boolean Striped { get; set; }

	public TableBackgroundStyle Background { get; set; }
	public CellSpacingMode CellSpacing { get; set; }

	public Boolean StickyHeaders { get; set; }
	public Length? Height { get; set; }

	public TableRowCollection Header
	{
		get
		{
			_header ??= [];
			return _header;
		}
		set
		{
			_header = value;
		}
	}

	public TableRowCollection Footer
	{
		get
		{
			_footer ??= [];
			return _footer;
		}
		set { _footer = value; }
	}

	public TableColumnCollection Columns
	{
		get
		{
			_columns ??= [];
			return _columns;
		}
		set
		{
			_columns = value;
		}
	}


	TableRowCollection? _header;
	TableRowCollection? _footer;
	TableColumnCollection? _columns;

	public Object? ItemsSource { get; set; }

	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;
		if (StickyHeaders)
		{
			var outTag = new TagBuilder("div", "a2-sticky-container", IsInGrid);
			MergeAttributes(outTag, context, MergeAttrMode.Visibility | MergeAttrMode.Margin);
			if (Height != null)
				outTag.MergeStyle("height", Height.Value);
			outTag.RenderStart(context);
			RenderTable(context, "a2-table sticky", false, false, null);
			outTag.RenderEnd(context);
			/*
			var sb = new TagBuilder("div", "a2-sticky-bottom");
			if (Width != null)
				sb.MergeStyle("width", Width.Value);
			sb.Render(context);
			*/
		}
		else
		{
			RenderTable(context, "a2-table", IsInGrid, true, onRender);
		}
	}

	private void RenderTable(RenderContext context, String tblClass, Boolean inGrid, Boolean mergeAttrs, Action<TagBuilder>? onRender)
	{
		var table = new TagBuilder("table", tblClass, inGrid);
		onRender?.Invoke(table);
		if (mergeAttrs)
			MergeAttributes(table, context);
		if (Background != TableBackgroundStyle.None)
			table.AddCssClass("bk-" + Background.ToString().ToKebabCase());
		if (CellSpacing != CellSpacingMode.None)
		{
			table.AddCssClass("table-separate-border");
			table.AddCssClass("table-cell-spacing-" + CellSpacing.ToString().ToLowerInvariant());
		}
		if (GridLines != GridLinesVisibility.None)
			table.AddCssClass($"grid-{GridLines.ToString().ToLowerInvariant()}");

		table.AddCssClassBool(Border, "bordered");
		table.AddCssClassBool(Compact, "compact");
		table.AddCssClassBool(Hover, "hover");
		table.AddCssClassBool(Striped, "striped");

		Bind? isBind = GetBinding(nameof(ItemsSource));
		if (isBind != null)
			table.MergeAttribute("v-lazy", isBind.GetPath(context));

		table.RenderStart(context);

		if (_columns != null)
			Columns.Render(context);

		RenderHeader(context);

		RenderBody(context);
		RenderFooter(context);

		table.RenderEnd(context);
	}

	void RenderHeader(RenderContext context)
	{
		if (_header == null)
			return;
		var thead = new TagBuilder("thead").RenderStart(context);
		foreach (var h in Header)
			h.RenderElement(context);
		thead.RenderEnd(context);
	}

	void RenderBody(RenderContext context)
	{
		if (Rows.Count == 0)
			return;
		var tbody = new TagBuilder("tbody").RenderStart(context);
		Bind? isBind = GetBinding(nameof(ItemsSource));
		if (isBind != null)
		{
			var repeatAttr = $"(row, rowIndex) in {isBind.GetPath(context)}";
			using (new ScopeContext(context, "row", isBind.Path))
			{
				if (Rows.Count == 1)
				{
					Rows[0].RenderElement(context, (tag) =>
					{
						tag.MergeAttribute("v-for", repeatAttr);
					});
				}
				else
				{
					var tml = new TagBuilder("template");
					tml.MergeAttribute("v-for", repeatAttr);
					tml.RenderStart(context);
					using (var cts = new ScopeContext(context, "row", isBind.Path))
					{
						var rNo = 0;
						foreach (var row in Rows)
						{
							row.RenderElement(context, (tag) => tag.MergeAttribute(":key", $"'r{rNo}:' + rowIndex"));
							rNo += 1;
						}
					}
					tml.RenderEnd(context);
				}
			}
		}
		else
		{
			foreach (var row in Rows)
				row.RenderElement(context);
		}
		tbody.RenderEnd(context);
	}

	void RenderFooter(RenderContext context)
	{
		if (_footer == null)
			return;
		var tfoot = new TagBuilder("tfoot").RenderStart(context);
		foreach (var f in Footer)
			f.RenderElement(context);
		tfoot.RenderEnd(context);
	}

	protected override void OnEndInit()
	{
		base.OnEndInit();
		foreach (var c in Rows)
			c.SetParent(this);
		if (_header != null)
			foreach (var h in Header)
				h.SetParent(this);
		if (_footer != null)
			foreach (var f in Footer)
				f.SetParent(this);
		if (_columns != null)
			foreach (var c in Columns)
				c.SetParent(this);
	}

	public override void OnSetStyles(RootContainer root)
	{
		base.OnSetStyles(root);
		foreach (var c in Rows)
			c.OnSetStyles(root);
		if (_header != null)
			foreach (var h in Header)
				h.OnSetStyles(root);
		if (_footer != null)
			foreach (var f in Footer)
				f.OnSetStyles(root);
		if (_columns != null)
			foreach (var c in Columns)
				c.OnSetStyles(root);
	}
}
