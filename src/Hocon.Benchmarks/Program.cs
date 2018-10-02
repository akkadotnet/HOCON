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
