﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Infrastructure
{

	public interface IReportEngineProvider
	{
		IReportEngine FindReportEngine(String name);
	}
}
