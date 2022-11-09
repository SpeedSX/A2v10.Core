﻿// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

namespace A2v10.Xaml
{

	[ContentProperty("Sections")]
	public partial class Sheet : UIElement
	{

		public SheetSections Sections { get; set; } = new SheetSections();

		SheetRows? _header;
		SheetRows? _footer;
		SheetColumnCollection? _columns;
		public SheetRows Header
		{
			get
			{
				_header ??= new SheetRows();
				return _header;
			}
			set
			{
				_header = value;
			}
		}

		public SheetRows Footer
		{
			get
			{
				_footer ??= new SheetRows();
				return _footer;
			}
			set
			{
				_footer = value;
			}
		}

		public SheetColumnCollection Columns
		{
			get
			{
				_columns ??= new SheetColumnCollection();
				return _columns;
			}
			set
			{
				_columns = value;
			}
		}

		public GridLinesVisibility GridLines { get; set; }
		public Boolean Hover { get; set; }
		public Boolean Striped { get; set; }
		public Boolean? Border { get; set; }
		public Length? Width { get; set; }
		public Boolean Compact { get; set; }
		public Boolean FitWidth { get; set; }

		public SheetAutoGenerate? AutoGenerate { get; set; }

		public override void RenderElement(RenderContext context, Action<TagBuilder>? onRender = null)
		{
			if (SkipRender(context))
				return;
			GenerateSheet(context);
			var sheet = new TagBuilder("a2-sheet", null, IsInGrid);
			onRender?.Invoke(sheet);
			MergeAttributes(sheet, context);
			if (GridLines != GridLinesVisibility.None)
				sheet.AddCssClass($"grid-{GridLines.ToString().ToLowerInvariant()}");
			sheet.AddCssClassBool(Hover, "hover");
			sheet.AddCssClassBool(Striped, "striped");
			sheet.AddCssClassBoolNo(Border, "border");
			sheet.AddCssClassBool(Compact, "compact");
			sheet.AddCssClassBool(FitWidth, "fit-width");
			if (Width != null)
				sheet.MergeStyle("width", Width.Value);
			sheet.RenderStart(context);
			RenderColumns(context);
			RenderBody(context);
			RenderColumnsShadow(context);
			RenderHeader(context);
			RenderFooter(context);
			sheet.RenderEnd(context);
		}

		void RenderColumnsShadow(RenderContext context)
		{
			// for export to excel column widths
			if (_columns == null)
				return;
			var cs = new TagBuilder("template");
			cs.MergeAttribute("slot", "col-shadow");
			cs.RenderStart(context);
			var tb = new TagBuilder("tbody", "col-shadow");
			tb.RenderStart(context);
			var tr = new TagBuilder("tr");
			tr.RenderStart(context);
			foreach (var c in _columns)
				c.RenderShadow(context);
			tr.RenderEnd(context);
			tb.RenderEnd(context);
			cs.RenderEnd(context);
		}

		void RenderColumns(RenderContext context)
		{
			if (_columns == null)
				return;
			var cols = new TagBuilder("template");
			cols.MergeAttribute("slot", "columns");
			cols.RenderStart(context);
			_columns.Render(context);
			cols.RenderEnd(context);
		}

		void RenderHeader(RenderContext context)
		{
			if (_header == null)
				return;
			var thead = new TagBuilder("template");
			thead.MergeAttribute("slot", "header");
			thead.RenderStart(context);
			foreach (var h in Header)
				h.RenderElement(context);
			thead.RenderEnd(context);
		}

		void RenderBody(RenderContext context)
		{
			var tbody = new TagBuilder("template");
			tbody.MergeAttribute("slot", "body");
			tbody.RenderStart(context);
			foreach (var s in Sections)
				s.RenderElement(context, null);
			tbody.RenderEnd(context);
		}

		void RenderFooter(RenderContext context)
		{
			if (_footer == null)
				return;
			var tfoot = new TagBuilder("template");
			tfoot.MergeAttribute("slot", "footer");
			tfoot.RenderStart(context);
			foreach (var f in Footer)
				f.RenderElement(context);
			tfoot.RenderEnd(context);
		}

		protected override void OnEndInit()
		{
			base.OnEndInit();
			if (_header != null)
				foreach (var h in Header)
					h.SetParent(this);
			if (_footer != null)
				foreach (var f in Footer)
					f.SetParent(this);
			foreach (var s in Sections)
				s.SetParent(this);
		}

		public override void OnSetStyles(RootContainer root)
		{
			base.OnSetStyles(root);
			if (_header != null)
				foreach (var h in Header)
					h.OnSetStyles(root);
			if (_footer != null)
				foreach (var f in Footer)
					f.OnSetStyles(root);
			foreach (var s in Sections)
				s.OnSetStyles(root);
		}
	}
}
