﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Xaml;

public enum PropertyGridStyle
{
	Default,
	Compact,
	Small
}

[ContentProperty("Children")]
public class PropertyGrid : UIElement, ITableControl
{

	/* TODO: 
         * 3. Grouping
         */
	public Object? ItemsSource { get; set; }
	public Boolean Compact { get; set; }
	public Boolean Striped { get; set; }
	public String? TestId { get; set; }
	public PropertyGridStyle Style { get; set; }

	public Boolean NoWrapName { get; set; }

	public PropertyGridItems Children { get; set; } = [];
	public GridLinesVisibility GridLines { get; set; }

	public UInt32 NameMaxChars { get; set; }
	public UInt32 ValueMaxChars { get; set; }

	public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		if (SkipRender(context))
			return;
		var table = new TagBuilder("table", "prop-grid", IsInGrid);
		onRender?.Invoke(table);
		table.AddCssClassBool(Compact, "compact");
		SetStyle(table);
		table.AddCssClassBool(Striped, "striped");
		table.AddCssClassBool(NoWrapName, "no-wrap-name");
		if (!String.IsNullOrEmpty(TestId) && context.IsDebugConfiguration)
			table.MergeAttribute("test-id", TestId);
		MergeAttributes(table, context);
		if (GridLines != GridLinesVisibility.None)
			table.AddCssClass($"grid-{GridLines.ToString().ToLowerInvariant()}");

		table.RenderStart(context);
		RenderColumns(context);
		RenderBody(context);
		table.RenderEnd(context);
	}

	static void RenderColumns(RenderContext context)
	{
		var colGroup = new TagBuilder("colgroup").RenderStart(context);
		new TagBuilder("col", "prop-name").Render(context, TagRenderMode.SelfClosing);
		new TagBuilder("col", "prop-value").Render(context, TagRenderMode.SelfClosing);
		colGroup.RenderEnd(context);
	}

	void RenderBody(RenderContext context)
	{
		var tbody = new TagBuilder("tbody").RenderStart(context);
		var isBind = GetBinding(nameof(ItemsSource));
		if (isBind != null)
		{
			if (Children.Count != 1)
				throw new XamlException("For a table with an items source, only one child element is allowed");
			String path = isBind.GetPath(context); // before scope!
			using (new ScopeContext(context, "prop", isBind.Path))
			{
				Children[0].RenderElement(context, (tag) =>
				{
					tag.MergeAttribute("v-for", $"(prop, propIndex) in {path}");
					tag.MergeAttribute(":key", "propIndex");
				});
			}
		}
		else
		{
			Children.Render(context);
		}
		tbody.RenderEnd(context);
	}

	void SetStyle(TagBuilder tag)
	{
		switch (Style)
		{
			case PropertyGridStyle.Compact:
				tag.AddCssClass("compact");
				break;
			case PropertyGridStyle.Small:
				tag.AddCssClass("small");
				break;
		}
	}

    protected override void OnEndInit()
    {
        base.OnEndInit();
        foreach (var c in Children)
            c.SetParent(this);
    }
}
