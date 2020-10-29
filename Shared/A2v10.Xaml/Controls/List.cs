﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using Portable.Xaml.Markup;

using A2v10.Infrastructure;

namespace A2v10.Xaml
{
	public enum ListStyle
	{
		List,
		TwoColumnsGrid,
		ThreeColumnsGrid,
		Underlined,
		Chat
	}


	[ContentProperty("Content")]
	public class List : Control, ITableControl
	{
		public Object ItemsSource { get; set; }
		public UIElementCollection Content { get; set; } = new UIElementCollection();
		public AutoSelectMode AutoSelect { get; set; }
		public Boolean Striped { get; set; }
		public Boolean? Select { get; set; }
		public Object Mark { get; set; }
		public RowMarkerStyle MarkerStyle { get; set; }
		public Boolean Border { get; set; }
		public Boolean Flush { get; set; }
		public Boolean Compact { get; set; }
		public BorderStyle BorderStyle { get; set; }

		public Length Height { get; set; }
		public Length MaxHeight { get; set; }
		public BackgroundStyle Background { get; set; }

		public ListStyle Style { get; set; }
		public String GroupBy { get; set; }

		public UIElement EmptyPanel { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder> onRender = null)
		{
			if (SkipRender(context))
				return;
			var ul = new TagBuilder("a2-list", null, IsInGrid);
			onRender?.Invoke(ul);
			var isBind = GetBinding(nameof(ItemsSource));
			ul.AddCssClassBool(Striped, "striped");
			ul.AddCssClassBool(Border, "border");
			ul.AddCssClassBool(Flush, "flush");
			ul.AddCssClassBool(Compact, "compact");
			ul.MergeAttribute("group-by", GroupBy);
			if (BorderStyle != BorderStyle.None)
				ul.AddCssClass($"border-{BorderStyle.ToString().ToKebabCase()}");
			if (MarkerStyle != RowMarkerStyle.None)
				ul.MergeAttribute("mark-style", MarkerStyle.ToString().ToKebabCase());
			if (Select != null)
				ul.MergeAttribute(":selectable", Select.Value.ToString().ToLowerInvariant());
			ul.AddCssClass(Style.ToString().ToKebabCase());
			//ul.MergeAttribute(":command", "()=> $navigate()");

			if (Background != BackgroundStyle.Default)
				ul.AddCssClass("background-" + Background.ToString().ToKebabCase());

			var mbind = GetBinding(nameof(Mark));
			if (mbind != null)
			{
				ul.MergeAttribute("mark", mbind.Path /*without scope, property name*/);
			}
			else if (Mark != null)
			{
				throw new XamlException("The Mark property must be a binding");
			}

			if (isBind != null)
			{
				ul.MergeAttribute(":items-source", isBind.GetPath(context));
				ul.MergeAttribute("v-lazy", isBind.GetPath(context));
			}
			MergeAttributes(ul, context);
			if (Height != null)
				ul.MergeStyle("height", Height.Value);
			if (MaxHeight != null)
				ul.MergeStyle("max-height", MaxHeight.Value);
			if (AutoSelect != AutoSelectMode.None)
				ul.MergeAttribute("auto-select", AutoSelect.ToString().ToKebabCase());
			ul.RenderStart(context);
			RenderBody(context, isBind != null);
			RenderEmptyPanel(context);
			ul.RenderEnd(context);
		}

		void RenderBody(RenderContext context, Boolean dyna)
		{
			if (Content.Count == 0)
				return;
			var tml = new TagBuilder("template");
			if (dyna)
			{
				tml.MergeAttribute("slot", "items");
				tml.MergeAttribute("slot-scope", "listItem");
				tml.RenderStart(context);
				using (new ScopeContext(context, "listItem.item", null))
				{
					foreach (var c in Content)
						c.RenderElement(context);
				}
				tml.RenderEnd(context);
			}
			else
			{
				tml.RenderStart(context);
				foreach (var c in Content)
				{
					var li = new TagBuilder("li", "a2-list-item");
					c.MergeBindingAttributeBool(li, context, "v-if", "If", c.If);
					li.MergeAttribute("tabindex", "1");
					li.RenderStart(context);
					c.RenderElement(context);
					li.RenderEnd(context);
				}
				tml.RenderEnd(context);
			}
		}

		void RenderEmptyPanel(RenderContext context)
		{
			if (EmptyPanel == null)
				return;
			var panel = new TagBuilder("template");
			panel.MergeAttribute("slot", "empty");
			panel.RenderStart(context);
			EmptyPanel.RenderElement(context);
			panel.RenderEnd(context);
		}

	}
}
