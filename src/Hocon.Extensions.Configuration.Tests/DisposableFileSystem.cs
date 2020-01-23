﻿// -----------------------------------------------------------------------
// <copyright file="DisposableFileSystem.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Hocon.Extensions.Configuration.Tests
{
    public class DisposableFileSystem : IDisposable
    {
        public DisposableFileSystem()
        {
            RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(RootPath);
            DirectoryInfo = new DirectoryInfo(RootPath);
        }

        public string RootPath { get; }

        public DirectoryInfo DirectoryInfo { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(RootPath, true);
            }
            catch
            {
                // Don't throw if this fails.
            }
        }

        public DirectoryInfo GetDirectory(string path)
        {
            return new DirectoryInfo(Path.Combine(RootPath, path));
        }

        public FileInfo GetFile(string path)
        {
            return new FileInfo(Path.Combine(RootPath, path));
        }

        public DisposableFileSystem CreateFolder(string path)
        {
            Directory.CreateDirectory(Path.Combine(RootPath, path));
            return this;
        }

        public DisposableFileSystem CreateFile(string path, string content = "test")
        {
            File.WriteAllText(Path.Combine(RootPath, path), content);
            return this;
        }

        public DisposableFileSystem CreateFile(FileInfo fileInfo, string content = "test")
        {
            using (var stream = fileInfo.Create())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }

            return this;
        }

        public DisposableFileSystem CreateFiles(params string[] fileRelativePaths)
        {
            foreach (var path in fileRelativePaths)
            {
                var fullPath = Path.Combine(RootPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                File.WriteAllText(
                    fullPath,
                    string.Format("Automatically generated for testing on {0:yyyy}/{0:MM}/{0:dd} {0:hh}:{0:mm}:{0:ss}",
                        DateTime.UtcNow));
            }

            return this;
        }
    }
}