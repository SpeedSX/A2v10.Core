﻿// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using A2v10.Data.Interfaces;

namespace A2v10.Services;

public static class DataModelExtensions
{
    public static String? ResolveDataModel(this IDataModel model, String? source)
    {
        if (model == null || source == null)
            return source;
        if (String.IsNullOrEmpty(source))
            return source;
        if (!source.Contains("{{"))
            return source;
        var ms = Regex.Matches(source, "\\{\\{(.+?)\\}\\}");
        if (ms.Count == 0)
            return source;
        var sb = new StringBuilder(source);
        foreach (Match m in ms.Cast<Match>())
        {
            String key = m.Groups[1].Value;
            var valObj = model.Eval<Object>(key);
            if (ms.Count == 1 && m.Groups[0].Value == source)
                return valObj?.ToString() ?? String.Empty; // single element
            if (valObj is String valStr)
                sb.Replace(m.Value, valStr);
            else if (valObj is ExpandoObject valEo)
                sb.Replace(m.Value, JsonConvert.SerializeObject(valEo));
            else
                sb.Replace(m.Value, valObj?.ToString());

        }
        return sb.ToString();
    }
}
