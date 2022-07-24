﻿// Copyright © 2022 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.Infrastructure;

public interface IAppStartManager
{
	Task<Int64?> GetRootMenuId();
}
