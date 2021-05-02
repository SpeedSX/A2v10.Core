﻿// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Interfaces;
using A2v10.Infrastructure;

namespace A2v10.Services
{
	public static class ServerCommandRegistry
	{
		public static IModelInvokeCommand GetCommand(ModelCommandType command, IServiceProvider serviceProvider)
		{
			return command switch {
				ModelCommandType.sql => new InvokeCommandExecuteSql(
						serviceProvider.GetService<IDbContext>()
					),
				ModelCommandType.clr => throw new DataServiceException("CLR yet not implemented"),
				ModelCommandType.javascript => throw new DataServiceException("javascript command yet not implemented"),
				ModelCommandType.file => throw new DataServiceException("file command yet not implemented"),
				ModelCommandType.xml => throw new DataServiceException("xml command yet not implemented"),
				ModelCommandType.startProcess => throw new DataServiceException("startProcess command yet not implemented"),
				ModelCommandType.resumeProcess => throw new DataServiceException("resumeProcess command yet not implemented"),
				_ => throw new DataServiceException("Server command for '{command}' not found")
			};
		}
	}
}
