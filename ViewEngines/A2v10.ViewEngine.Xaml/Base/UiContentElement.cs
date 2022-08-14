﻿// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Xaml;

[ContentProperty("Content")]
public abstract class UiContentElement : UIElementBase
{
	public Object? Content { get; set; }

	protected virtual void MergeContent(TagBuilder tag, RenderContext context)
	{
		var contBind = GetBinding(nameof(Content));
		if (contBind != null)
		{
			tag.MergeAttribute("v-text", contBind.GetPathFormat(context));
			if (contBind.NegativeRed)
				tag.MergeAttribute(":class", $"$getNegativeRedClass({contBind.GetPath(context)})");
		}
	}

	internal void RenderContent(RenderContext context)
	{
		RenderContent(context, Content);
	}

	public override void OnSetStyles()
	{
		base.OnSetStyles();
		if (Content is XamlElement xamlCont)
			xamlCont.OnSetStyles();
	}

	protected override void OnEndInit()
	{
		base.OnEndInit();
		if (Content is XamlElement xamlCont)
			xamlCont.SetParent(this);
	}
}
