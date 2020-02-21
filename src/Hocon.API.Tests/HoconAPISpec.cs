//-----------------------------------------------------------------------
// <copyright file="HoconAPISpec.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2019 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using Hocon.Extensions.Configuration;
using ApprovalTests;
using Xunit;
using static PublicApiGenerator.ApiGenerator;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hocon.API.Tests
{
    public class HoconAPISpec
    {
        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveCore()
        {
            var publicApi = Filter(GeneratePublicApi(typeof(HoconObject).Assembly));
            Approvals.Verify(publicApi);
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveConfiguration()
        {
            var publicApi = Filter(GeneratePublicApi(typeof(Config).Assembly));
            Approvals.Verify(publicApi);
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveExtensionConfiguration()
        {
            var publicApi = Filter(GeneratePublicApi(typeof(HoconConfigurationSource).Assembly));
            Approvals.Verify(publicApi);
        }

        static string Filter(string text)
        {
            return string.Join(Environment.NewLine, text.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.StartsWith("[assembly: ReleaseDateAttribute("))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                );
        }
    }
}
