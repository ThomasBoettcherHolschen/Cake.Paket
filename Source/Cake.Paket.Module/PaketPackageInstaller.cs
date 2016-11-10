﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Packaging;
using Cake.NuGet;

namespace Cake.Paket.Module
{
    /// <summary>
    /// Installer for paket URI resources.
    /// </summary>
    public sealed class PaketPackageInstaller : IPackageInstaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaketPackageInstaller"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="contentResolver">The content resolver.</param>
        /// <param name="log">The log.</param>
        public PaketPackageInstaller(ICakeEnvironment environment, INuGetContentResolver contentResolver, ICakeLog log)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (contentResolver == null)
            {
                throw new ArgumentNullException(nameof(contentResolver));
            }

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            Environment = environment;
            ContentResolver = contentResolver;
            Log = log;
        }

        private ICakeEnvironment Environment { get; }

        private INuGetContentResolver ContentResolver { get; }

        private ICakeLog Log { get; }

        /// <summary>
        /// Determines whether this instance can install the specified resource.
        /// </summary>
        /// <param name="package">The package reference.</param>
        /// <param name="type">The package type.</param>
        /// <returns><c>true</c> if this installer can install the specified resource; otherwise <c>false</c>.</returns>
        public bool CanInstall(PackageReference package, PackageType type)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (package.Scheme.Equals("nuget", StringComparison.OrdinalIgnoreCase))
            {
                throw new CakeException("nuget is not supported. Perhaps you need to include the schema?");
            }

            return package.Scheme.Equals("paket", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Installs the specified resource at the given location.
        /// </summary>
        /// <param name="package">The package reference.</param>
        /// <param name="type">The package type.</param>
        /// <param name="path">The location where to install the package.</param>
        /// <returns>The installed files.</returns>
        public IReadOnlyCollection<IFile> Install(PackageReference package, PackageType type, DirectoryPath path)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var packagePath = GetPackagePath(path, package);
            var result = ContentResolver.GetFiles(packagePath, package, type);
            if (result.Count != 0)
            {
                return result;
            }

            if (type == PackageType.Addin)
            {
                var framework = Environment.Runtime.TargetFramework;
                Log.Warning($"Could not find any assemblies compatible with {framework.FullName}. Perhaps you need an include parameter?");
            }
            else if (type == PackageType.Tool)
            {
                Log.Warning($"Could not find any relevant files for tool '{package.Package}'. Perhaps you need an include parameter?");
            }

            return result;
        }

        private static DirectoryPath GetFromGroup(DirectoryPath path, PackageReference package)
        {
            const string key = "group";
            const string packages = "packages";

            var parameters = package.Parameters;

            if (parameters.ContainsKey(key))
            {
                var group = parameters[key].Single();
                var folders = path.Segments;
                var newFolders = new List<string>();
                foreach (var f in folders)
                {
                    if (f.Equals(packages))
                    {
                        break;
                    }

                    newFolders.Add(f);
                }

                newFolders.Add(packages);

                var packageDirectory = DirectoryPath.FromString(string.Join("/", newFolders));
                var groupDirectory = DirectoryPath.FromString(group);
                return packageDirectory.Combine(groupDirectory);
            }

            return null;
        }

        private static DirectoryPath GetFromDefaultPath(DirectoryPath path, PackageReference package)
        {
            return path.Combine(package.Package);
        }

        private DirectoryPath GetPackagePath(DirectoryPath path, PackageReference package)
        {
            path = path.MakeAbsolute(Environment);

            var packagePath = GetFromGroup(path, package) ?? GetFromDefaultPath(path, package);

            return packagePath;
        }
    }
}
