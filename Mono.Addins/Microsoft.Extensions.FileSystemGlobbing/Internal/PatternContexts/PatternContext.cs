// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts
{
    abstract class PatternContext<TFrame> : IPatternContext
    {
        private Stack<TFrame> _stack = new Stack<TFrame>();
        protected TFrame Frame;

        public virtual void Declare(Action<IPathSegment, bool> declare) { }

        public abstract PatternTestResult Test(FileInfo file);

        public abstract bool Test(DirectoryInfo directory);

        public abstract void PushDirectory(DirectoryInfo directory);

        public virtual void PopDirectory()
        {
            Frame = _stack.Pop();
        }

        protected void PushDataFrame(TFrame frame)
        {
            _stack.Push(Frame);
            Frame = frame;
        }

        protected bool IsStackEmpty()
        {
            return _stack.Count == 0;
        }
    }
}