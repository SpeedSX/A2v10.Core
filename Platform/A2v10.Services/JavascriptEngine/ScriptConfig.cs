﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Services.Javascript;
public class ScriptConfig
{
	private readonly IApplicationHost _host;

	public ScriptConfig(IApplicationHost host)
	{
		_host = host;
	}

#pragma warning disable IDE1006 // Naming Styles
	public ExpandoObject appSettings(String name)
#pragma warning restore IDE1006 // Naming Styles
	{
		return _host.GetEnvironmentObject(name);
	}
}

