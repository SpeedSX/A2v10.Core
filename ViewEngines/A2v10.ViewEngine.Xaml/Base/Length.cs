﻿// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using System.ComponentModel;
using System.Globalization;

using System.Text.RegularExpressions;

namespace A2v10.Xaml;

public enum LengthType
{
	Pixel,
	Percent
}

public enum GridLengthType
{
	Pixel,
	Percent,
	Fraction
}

[TypeConverter(typeof(LengthConverter))]
public record Length
{
	public String? Value;

	public override String? ToString()
	{
		return Value;
	}

	public Boolean IsEmpty => String.IsNullOrEmpty(Value);
	public Boolean IsPixel => (Value != null) && Value.EndsWith("px");

	internal static Boolean IsValidLength(String strVal)
	{
		return (strVal.EndsWith('%') ||
				strVal.EndsWith("px") ||
				strVal.EndsWith("vh") ||
				strVal.EndsWith("vw") ||
				strVal.EndsWith("vmin") ||
				strVal.EndsWith("vmax") ||
				strVal.EndsWith("mm") ||
				strVal.EndsWith("cm") ||
				strVal.EndsWith("pt") ||
				strVal.EndsWith("in") ||
				strVal.EndsWith("ch") ||
				strVal.EndsWith("ex") ||
				strVal.EndsWith("em") ||
				strVal.EndsWith("rem"));
	}

	public static Length? FromStringNull(String? strVal)
	{
		if (String.IsNullOrWhiteSpace(strVal))
			return null;
		return FromString(strVal);
	}


    public static Length FromString(String strVal)
	{
		strVal = strVal.Trim();
		if (strVal == "Auto")
			return new Length() { Value = "auto" };
		if (strVal == "Fit")
			return new Length() { Value = "fit-content" };
		else if (strVal == "0")
			return new Length() { Value = strVal };
		else if (strVal.StartsWith("Calc("))
			return new Length() { Value = strVal };
		else if (IsValidLength(strVal))
			return new Length() { Value = strVal.Replace(" ", "") };
		else if (Double.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out Double _))
			return new Length() { Value = strVal + "px" };
		throw new XamlException($"Invalid length value '{strVal}'");
	}
}

[TypeConverter(typeof(GridLengthConverter))]
public record GridLength
{
	public String? Value;

	public GridLength()
	{
	}

	public GridLength(String value)
	{
		Value = value;
	}

	public override String? ToString()
	{
		return Value;
	}

	public static GridLength Fr1()
	{
		return new GridLength() { Value = "1fr" };
	}

	public static GridLength FromString(String strVal)
	{
		if (strVal.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			return new GridLength("auto");
		else if (strVal.EndsWith("fr"))
			return new GridLength(strVal);
		else if (strVal.StartsWith("MinMax"))
		{
			var pattern = @"MinMax\(([\w\.]+[%\*\.]?);([\w\.]+[%\*\.]?)\)";
			var match = Regex.Match(strVal.Replace(" ", String.Empty), pattern);
			if (match.Groups.Count != 3)
				throw new XamlException($"Invalid grid length value '{strVal}'");
			GridLength gl1 = GridLength.FromString(match.Groups[1].Value);
			GridLength gl2 = GridLength.FromString(match.Groups[2].Value);
			return new GridLength($"minmax({gl1},{gl2})");
		}
		else if (Length.IsValidLength(strVal))
			return new GridLength() { Value = strVal };
		if (strVal.EndsWith('*'))
			return new GridLength(strVal.Trim().Replace("*", "fr"));
		else if (Double.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out Double _))
			return new GridLength(strVal + "px");
		throw new XamlException($"Invalid grid length value '{strVal}'");
	}
}

public class LengthConverter : TypeConverter
{
	public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		if (sourceType == typeof(String))
			return true;
		else if (sourceType == typeof(Length))
			return true;
		return false;
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if (value == null)
			return null;
		if (value is String strVal)
			return Length.FromString(strVal);
		throw new XamlException($"Invalid length value '{value}'");
	}
}

public class GridLengthConverter : TypeConverter
{
	public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		if (sourceType == typeof(String))
			return true;
		else if (sourceType == typeof(GridLength))
			return true;
		return false;
	}

	public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
	{
		if (value == null)
			return null;
		if (value is String strVal)
			return GridLength.FromString(strVal);
		throw new XamlException($"Invalid length value '{value}'");
	}
}
