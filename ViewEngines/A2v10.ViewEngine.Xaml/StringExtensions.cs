﻿// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Linq;

using A2v10.Infrastructure;

namespace A2v10.ViewEngine.Xaml;

internal static class StringExtensions
{
    private static String ToJsonString(ExpandoObject o)
    {
        var e = String.Join(',', o.Select(kv => $"{kv.Key}: '{kv.Value}'"));
        return $"{{{e}}}";
    }
    public static (String?, String?) ParseUrlQuery(this String? urlQuery)
    {
        var url = urlQuery;
        ExpandoObject? query = null;
        if (url != null && url.Contains('?'))
        {
            var urlSplit = url.Split('?');
            url = urlSplit[0];

            var qs = urlSplit[1].Split('&').ToDictionary(c => c.Split('=')[0], c => c.Split('=')[1]);
            query = new ExpandoObject();
            foreach (var (k, v) in qs)
            {
                if (k != null && v != null)
                    query.Add(k, v);
            }
            if (query.IsEmpty())
                query = null;
        }
        return (url, query != null ? ToJsonString(query) : null);
    }
}
