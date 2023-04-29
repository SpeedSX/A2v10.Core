﻿// Copyright © 2015-2023 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace A2v10.Infrastructure;

public interface IAppCodeProvider
{
    String MakePath(String path, String fileName);
    Boolean IsFileExists(String path);
    Stream? FileStreamRO(String path, Boolean primaryOnly = false);

    IEnumerable<String> EnumerateAllFiles(String path, String searchPattern);
    IEnumerable<String> EnumerateWatchedDirs(String path, String searchPattern);

    Boolean HasLicensedModules { get; }
}

