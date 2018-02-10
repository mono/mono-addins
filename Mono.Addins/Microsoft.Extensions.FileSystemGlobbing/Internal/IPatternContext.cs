// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal
{
    interface IPatternContext
    {
        void Declare(Action<IPathSegment, bool> onDeclare);

		bool Test(DirectoryInfo directory);

        PatternTestResult Test(FileInfo file);

        void PushDirectory(DirectoryInfo directory);

        void PopDirectory();
    }
}
