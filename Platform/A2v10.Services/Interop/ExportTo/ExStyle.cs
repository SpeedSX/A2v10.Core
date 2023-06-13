﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Services.Interop.ExportTo;

public enum RowRole
{
	None,
	Title,
	Parameter,
	LastParameter,
	Header,
	LightHeader,
	Total,
	Body,
	Footer,
	Divider
}

public struct Style
{
	public HorizontalAlign Align;
	public VerticalAlign VAlign;
	public Boolean Bold;
	public DataType DataType;
	public Boolean Wrap;
	public RowRole RowRole;
	public RowKind RowKind;
	public UInt32 Indent;
	public Boolean Underline;

	public readonly Boolean HasBorder => RowKind == RowKind.Body || RowRole == RowRole.Header || RowRole == RowRole.Footer || RowRole == RowRole.Total;
	public readonly Boolean HasAlignment => Align != HorizontalAlign.NotSet || DataType == DataType.DateTime || DataType == DataType.Date;
}

public class StylesDictionary
{
	private readonly Dictionary<Style, UInt32> _hash = new();
	public List<Style> List { get; } = new List<Style>();

	public StylesDictionary()
	{
		var d = new Style();
		// default style (zero)
		GetOrCreate(d);
	}

	public UInt32 GetOrCreate(Style style)
	{
		if (_hash.TryGetValue(style, out UInt32 index))
			return index;
		List.Add(style);
		var newIndex = (UInt32)List.Count - 1;
		_hash.Add(style, newIndex);
		return newIndex;
	}
}
