﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System;

namespace A2v10.Infrastructure;

public enum ProfileAction
{
	Sql,
	Render,
	Workflow,
	Script,
	Report,
	Exception
};

public interface IProfileRequest
{
	IDisposable? Start(ProfileAction kind, String description);
	void Stop();
}

public interface IProfiler
{
	Boolean Enabled { get; set; }

	IProfileRequest? BeginRequest(String address, String? session);
	IProfileRequest CurrentRequest { get; }
	void EndRequest(IProfileRequest? request);

	String? GetJson();
}
