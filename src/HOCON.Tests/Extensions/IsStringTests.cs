// -----------------------------------------------------------------------
// <copyright file="IsStringTests.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace Hocon.Tests.Extensions
{
    public class IsStringTests
    {
        public readonly string RawTestHocon = @"
            root{
                foo {
                    bar = str
                    baz = 1 # int
                    biz = 2m # time
                    boz = 4.0 # float
                    byz = #empty
                    boy = ""quoted-string""
                }
            }
        ";

        public HoconRoot TestHocon => HoconParser.Parse(RawTestHocon);

        [Fact]
        public void IsString_should_detect_String_literals()
        {
            TestHocon.GetObject("root").Type.Should().NotBe(HoconType.String);
            TestHocon.GetObject("root.foo").Type.Should().NotBe(HoconType.String);
            var values = TestHocon.GetObject("root.foo");
            values["bar"].Type.Should().Be(HoconType.String);
        }
    }
}