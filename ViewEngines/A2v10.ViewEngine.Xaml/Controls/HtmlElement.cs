﻿// Copyright © 2021-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Xaml;

public class HtmlAttribute : XamlElement
{
	public String? Name { get; set; }
	public Object? Value { get; set; }
}

public class HtmlAttributeCollection : List<HtmlAttribute>
{
}

[ContentProperty("Children")]
public class HtmlElement : UIElementBase
{
	public HtmlAttributeCollection Attributes { get; set; } = [];
	public String? TagName { get; set; }
	public UIElementCollection Children { get; set; } = [];
    public Length? Width { get; set; }
    public Length? Height { get; set; }

    public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
	{
		var tag = new TagBuilder(TagName, null, IsInGrid);
		onRender?.Invoke(tag);
		MergeAttributes(tag, context);

        if (Height != null)
            tag.MergeStyle("height", Height.Value);
        if (Width != null)
            tag.MergeStyle("width", Width.Value);

        foreach (var attr in Attributes)
		{
			var valBind = attr.GetBinding("Value");
			if (valBind != null)
				tag.MergeAttribute($":{attr.Name}", valBind.GetPathFormat(context));
			else if (attr.Value != null && attr.Name != null)
				tag.MergeAttribute(attr.Name, attr.Value.ToString());
		}
		tag.RenderStart(context);
		foreach (var c in Children)
			c.RenderElement(context);
		tag.RenderEnd(context);
	}

	protected override void OnEndInit()
	{
		base.OnEndInit();
		foreach (var c in Children)
			c.SetParent(this);
	}

	public override void OnSetStyles(RootContainer root)
	{
		base.OnSetStyles(root);
		foreach (var c in Children)
			c.OnSetStyles(root);
	}
}
