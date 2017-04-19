// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NuGet.ProjectModel;

namespace Microsoft.DotNet.Cli.Utils
{
    public interface IPackagedCommandSpecFactory
    {
        //  Code review TODO: Is it OK to make breaking changes to the CLI Utils API surface?

        CommandSpec CreateCommandSpecFromLibrary(
            LockFileTargetLibrary toolLibrary,
            string commandName,
            IEnumerable<string> commandArguments,
            IEnumerable<string> allowedExtensions,
            LockFile lockFile,
            CommandResolutionStrategy commandResolutionStrategy,
            string depsFilePath,
            string runtimeConfigPath);
    }
}
