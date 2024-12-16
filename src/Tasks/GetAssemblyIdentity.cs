// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Shared;
using Microsoft.Build.Utilities;
using Mono.Cecil;

#nullable disable

namespace Microsoft.Build.Tasks
{
    /// <summary>
    /// Sniffs input files for their assembly identities, and outputs a set of items with the identity information.
    /// </summary>
    /// <comment>
    ///  Input:  Assembly Include="foo.exe"
    ///  Output: Identity Include="Foo, Version=1.0.0.0", Name="Foo, Version="1.0.0.0".
    /// </comment>
    public class GetAssemblyIdentity : TaskExtension
    {
        private ITaskItem[] _assemblyFiles;

        [Required]
        public ITaskItem[] AssemblyFiles
        {
            get
            {
                ErrorUtilities.VerifyThrowArgumentNull(_assemblyFiles, nameof(AssemblyFiles));
                return _assemblyFiles;
            }
            set => _assemblyFiles = value;
        }

        [Output]
        public ITaskItem[] Assemblies { get; set; }

        private static string ByteArrayToHex(Byte[] a)
        {
            if (a == null)
            {
                return null;
            }

            var s = new StringBuilder(a.Length * 2);
            foreach (Byte b in a)
            {
                s.Append(b.ToString("X02", CultureInfo.InvariantCulture));
            }

            return s.ToString();
        }

        public override bool Execute()
        {
            var list = new List<ITaskItem>();
            foreach (ITaskItem item in AssemblyFiles)
            {
                try
                {
                    using (var assembly = AssemblyDefinition.ReadAssembly(item.ItemSpec, new ReaderParameters { ReadingMode = ReadingMode.Deferred }))
                    {
                        ITaskItem newItem = new TaskItem(assembly.FullName);
                        newItem.SetMetadata("Name", assembly.Name.Name);
                        if (assembly.Name.Version != null)
                        {
                            newItem.SetMetadata("Version", assembly.Name.Version.ToString());
                        }

                        if (assembly.Name.PublicKeyToken != null)
                        {
                            newItem.SetMetadata("PublicKeyToken", ByteArrayToHex(assembly.Name.PublicKeyToken));
                        }

                        if (assembly.Name.Culture != null)
                        {
                            newItem.SetMetadata("Culture", assembly.Name.Culture);
                        }

                        item.CopyMetadataTo(newItem);
                        list.Add(newItem);
                    }
                }
                catch (BadImageFormatException e)
                {
                    Log.LogErrorWithCodeFromResources("GetAssemblyIdentity.CouldNotGetAssemblyName", item.ItemSpec, e.Message);
                    continue;
                }
                catch (Exception e) when (ExceptionHandling.IsIoRelatedException(e))
                {
                    Log.LogErrorWithCodeFromResources("GetAssemblyIdentity.CouldNotGetAssemblyName", item.ItemSpec, e.Message);
                    continue;
                }
            }

            Assemblies = list.ToArray();

            return !Log.HasLoggedErrors;
        }
    }
}
