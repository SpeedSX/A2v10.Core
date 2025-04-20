﻿// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.Infrastructure;

namespace A2v10.Xaml;

public enum FolderStyle
{
	None,
	Bold,
	Italic
}

[ContentProperty("Columns")]
public class TreeGrid : Control, ITableControl
{
	public Boolean Hover { get; set; }
	public Boolean Striped { get; set; }
	public Boolean StickyHeaders { get; set; }
	public Length? Height { get; set; }
	public GridLinesVisibility GridLines { get; set; }
	public FolderStyle FolderStyle { get; set; }
	public Object? ItemsSource { get; set; }
	public String? ItemsProperty { get; set; }
	public Length? MinWidth { get; set; }
	public Command? DoubleClick { get; set; }
	public DropDownMenu? ContextMenu { get; set; }
	public Object? IsFolder { get; set; }
	public RowMarkerStyle MarkerStyle { get; set; }
	public MarkStyle Mark { get; set; }
	public TreeGridColumnCollection Columns { get; set; } = [];
	public Boolean Sort { get; set; }
	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;

		if (StickyHeaders)
		{
			var outTag = new TagBuilder("div", "a2-sticky-container", IsInGrid);
			MergeAttributes(outTag, context, MergeAttrMode.Visibility);
			if (Height != null)
				outTag.MergeStyle("height", Height.Value);
			outTag.RenderStart(context);
			RenderGrid(context, "tree-grid sticky", false, false, null);
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
			RenderGrid(context, "tree-grid", IsInGrid, true, onRender);
		}

	}

	private void RenderGrid(RenderContext context, String tblClass, Boolean inGrid, Boolean mergeAttrs, Action<TagBuilder>? onRender)
	{
		var treeGrid = new TagBuilder("tree-grid", tblClass, inGrid);
		onRender?.Invoke(treeGrid);
		if (mergeAttrs)
			MergeAttributes(treeGrid, context);

		treeGrid.AddCssClassBool(Hover, "hover");
		treeGrid.AddCssClassBool(Striped, "striped");

		treeGrid.MergeAttribute("folder-style", FolderStyle.ToString().ToLowerInvariant());

		treeGrid.MergeAttribute("grid-lines", MarkGridLines);

		var isfldbind = GetBinding(nameof(IsFolder));

		if (isfldbind != null)
			treeGrid.MergeAttribute("is-folder", isfldbind.GetPath(context));
		else if (IsFolder != null)
			throw new XamlException("TreeGrid. The IsFolder property must be a binding");

		if (MarkerStyle != RowMarkerStyle.None)
			treeGrid.MergeAttribute("mark-style", MarkerStyle.ToString().ToKebabCase());

		var mbind = GetBinding(nameof(Mark));
		if (mbind != null)
			treeGrid.MergeAttribute("mark", mbind.Path); // without context!!!
		else if (Mark != MarkStyle.Default)
			throw new XamlException("TreeGrid. The Mark property must be a binding");

		if (MinWidth != null)
			treeGrid.MergeStyle("min-width", MinWidth.Value);

		String? contextId = null;
		if (ContextMenu != null)
		{
			contextId = $"ctx-{Guid.NewGuid()}";
			treeGrid.MergeAttribute("v-contextmenu", $"'{contextId}'");
		}

		var rootBind = GetBinding(nameof(ItemsSource)) 
			?? throw new XamlException("TreeGrid. ItemsSource must be a Bind");
        treeGrid.MergeAttribute(":root", rootBind.GetPath(context));

		treeGrid.MergeAttribute("item", ItemsProperty);

		var dblClickBind = GetBindingCommand(nameof(DoubleClick));
		if (dblClickBind != null)
		{
			// Function!
			treeGrid.MergeAttribute(":doubleclick", "() => " + dblClickBind.GetCommand(context));
		}


		treeGrid.RenderStart(context);
		var slot = new TagBuilder("template");
		slot.MergeAttribute("v-slot:row", "row");
		slot.RenderStart(context);
		using (new ScopeContext(context, "row.itm", rootBind.Path))
		{
			foreach (var col in Columns)
			{
				col.RenderCell("td", context, SetGridLines);
			}
		}
		slot.RenderEnd(context);
		// render header
		var hdr = new TagBuilder("template");
		hdr.MergeAttribute("v-slot:header", "hdr");
		hdr.RenderStart(context);
		foreach (var col in Columns)
		{
			col.RenderColumn("th", context, Sort, SetGridLines);
		}
		hdr.RenderEnd(context);

		// render columns
		var cols = new TagBuilder("template");
		cols.MergeAttribute("v-slot:columns", "cols");
		cols.RenderStart(context);
		foreach (var col in Columns)
		{
			col.RenderColumnTag(context);
		}
		cols.RenderEnd(context);

		RenderContextMenu(ContextMenu, context, contextId);
		treeGrid.RenderEnd(context);
	}
	private String MarkGridLines =>
		GridLines switch
		{
			GridLinesVisibility.Vertical => "gl-v",
			GridLinesVisibility.Horizontal => "gl-h",
			GridLinesVisibility.Both => "gl-h gl-v",
			_ => ""
		};


	private void SetGridLines(TagBuilder tag)
	{
		switch (GridLines)
		{
			case GridLinesVisibility.Vertical:
				tag.AddCssClass("gl-v");
				return;
			case GridLinesVisibility.Horizontal:
				tag.AddCssClass("gl-h");
				return;
			case GridLinesVisibility.Both:
				tag.AddCssClass("gl-h gl-v");
				return;
		}
	}
	protected override void OnEndInit()
	{
		base.OnEndInit();
		foreach (var col in Columns)
			col.SetParent(this);
		ContextMenu?.InvokeEndInit();
	}

	public override void OnSetStyles(RootContainer root)
	{
		base.OnSetStyles(root);
		foreach (var col in Columns)
			col.OnSetStyles(root);
	}
}
