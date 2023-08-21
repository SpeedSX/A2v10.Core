﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using DocumentFormat.OpenXml;

namespace A2v10.Services.Interop;

internal static class ExcelFormats
{
	//// https://msdn.microsoft.com/en-GB/library/documentformat.openxml.spreadsheet.numberingformat(v=office.14).aspx
	private static readonly Dictionary<UInt32, String> DateFormatDictionary = new()
	{
		[14] = "dd/MM/yyyy",
		[15] = "d-MMM-yy",
		[16] = "d-MMM",
		[17] = "MMM-yy",
		[18] = "h:mm AM/PM",
		[19] = "h:mm:ss AM/PM",
		[20] = "h:mm",
		[21] = "h:mm:ss",
		[22] = "M/d/yy h:mm",
		[30] = "M/d/yy",
		[34] = "yyyy-MM-dd",
		[45] = "mm:ss",
		[46] = "[h]:mm:ss",
		[47] = "mmss.0",
		[51] = "MM-dd",
		[52] = "yyyy-MM-dd",
		[53] = "yyyy-MM-dd",
		[55] = "yyyy-MM-dd",
		[56] = "yyyy-MM-dd",
		[58] = "MM-dd",
		[165] = "M/d/yy",
		[166] = "dd MMMM yyyy",
		[167] = "dd/MM/yyyy",
		[168] = "dd/MM/yy",
		[169] = "d.M.yy",
		[170] = "yyyy-MM-dd",
		[171] = "dd MMMM yyyy",
		[172] = "d MMMM yyyy",
		[173] = "M/d",
		[174] = "M/d/yy",
		[175] = "MM/dd/yy",
		[176] = "d-MMM",
		[177] = "d-MMM-yy",
		[178] = "dd-MMM-yy",
		[179] = "MMM-yy",
		[180] = "MMMM-yy",
		[181] = "MMMM d, yyyy",
		[182] = "M/d/yy hh:mm t",
		[183] = "M/d/y HH:mm",
		[184] = "MMM",
		[185] = "MMM-dd",
		[186] = "M/d/yyyy",
		[187] = "d-MMM-yyyy"
	};

	private static readonly Dictionary<UInt32, String> NumberFormatDictionary = new()
	{
		[0] = "General",
		[1] = "0",
		[2] = "0.00",
		[3] = "#,##0",
		[4] = "#,##0.00",
		[11] = "0.00E+00",
		[37] = "#,##0 ;(#,##0)",
		[38] = "#,##0 ;[Red](#,##0)",
		[39] = "#,##0.00;(#,##0.00)",
		[40] = "#,##0.00;[Red](#,##0.00)",
		[48] = "##0.0E+0"
	};

	public static String? GetDateTimeFormat(UInt32Value? numberFormatId)
	{
		if (numberFormatId is null)
			return null;
		if (DateFormatDictionary.TryGetValue(numberFormatId.Value, out String? format))
			return format;
		return null;
	}

	public static Boolean IsNumberFormat(UInt32Value? numberFormatId)
	{
		if (numberFormatId is null) 
			return false;
		return NumberFormatDictionary.ContainsKey(numberFormatId.Value);
	}
}
