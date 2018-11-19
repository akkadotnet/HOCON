#region copyright
// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace Hocon.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).RunAll();
        }
    }
}
