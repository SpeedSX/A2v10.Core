﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

using A2v10.ReportEngine.Stimulsoft.Controllers;

namespace A2v10.ReportEngine.Stimulsoft
{
	public static class ServicesExtensions
	{
		public static IMvcBuilder AddStimulsoftUI(this IMvcBuilder builder)
		{
			var assembly = typeof(StimulsoftController).Assembly;
			builder.AddApplicationPart(assembly);
			return builder;
		}
	}
}
