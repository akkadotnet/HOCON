using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class UnitFormats
    {
        [Theory]
        [InlineData("", null)]
        [InlineData("11b", 11L)]
        [InlineData("11byte", 11L)]
        [InlineData("11bytes", 11L)]

        [InlineData("11 b", 11L)]
        [InlineData("11 byte", 11L)]
        [InlineData("11 bytes", 11L)]

        [InlineData("11kB", 11L * 1000L)]
        [InlineData("11kilobyte", 11L * 1000L)]
        [InlineData("11kilobytes", 11L * 1000L)]
        [InlineData("11K", 11L * 1024L)]
        [InlineData("11k", 11L * 1024L)]
        [InlineData("11Ki", 11L * 1024L)]
        [InlineData("11KiB", 11L * 1024L)]
        [InlineData("11kibibyte", 11L * 1024L)]
        [InlineData("11kibibytes", 11L * 1024L)]

        [InlineData("11 kB", 11L * 1000L)]
        [InlineData("11 kilobyte", 11L * 1000L)]
        [InlineData("11 kilobytes", 11L * 1000L)]
        [InlineData("11 K", 11L * 1024L)]
        [InlineData("11 k", 11L * 1024L)]
        [InlineData("11 Ki", 11L * 1024L)]
        [InlineData("11 KiB", 11L * 1024L)]
        [InlineData("11 kibibyte", 11L * 1024L)]
        [InlineData("11 kibibytes", 11L * 1024L)]

        [InlineData("11MB", 11L * 1000L * 1000L)]
        [InlineData("11megabyte", 11L * 1000L * 1000L)]
        [InlineData("11megabytes", 11L * 1000L * 1000L)]
        [InlineData("11M", 11L * 1024L * 1024L)]
        [InlineData("11m", 11L * 1024L * 1024L)]
        [InlineData("11Mi", 11L * 1024L * 1024L)]
        [InlineData("11MiB", 11L * 1024L * 1024L)]
        [InlineData("11mebibyte", 11L * 1024L * 1024L)]
        [InlineData("11mebibytes", 11L * 1024L * 1024L)]

        [InlineData("11 MB", 11L * 1000L * 1000L)]
        [InlineData("11 megabyte", 11L * 1000L * 1000L)]
        [InlineData("11 megabytes", 11L * 1000L * 1000L)]
        [InlineData("11 M", 11L * 1024L * 1024L)]
        [InlineData("11 m", 11L * 1024L * 1024L)]
        [InlineData("11 Mi", 11L * 1024L * 1024L)]
        [InlineData("11 MiB", 11L * 1024L * 1024L)]
        [InlineData("11 mebibyte", 11L * 1024L * 1024L)]
        [InlineData("11 mebibytes", 11L * 1024L * 1024L)]

        [InlineData("11GB", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gigabyte", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gigabytes", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11G", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11g", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11Gi", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11GiB", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11gibibyte", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11gibibytes", 11L * 1024L * 1024L * 1024L)]

        [InlineData("11 GB", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 gigabyte", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 gigabytes", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 G", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 g", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 Gi", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 GiB", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 gibibyte", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 gibibytes", 11L * 1024L * 1024L * 1024L)]

        [InlineData("11 TB", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 terabyte", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 terabytes", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 T", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 t", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 Ti", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 TiB", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 tebibyte", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 tebibytes", 11L * 1024L * 1024L * 1024L * 1024L)]

        [InlineData("11TB", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11terabyte", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11terabytes", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11T", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11t", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11Ti", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11TiB", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11tebibyte", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11tebibytes", 11L * 1024L * 1024L * 1024L * 1024L)]

        [InlineData("11PB", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11petabyte", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11petabytes", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11P", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11p", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11Pi", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11PiB", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11pebibyte", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11pebibytes", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]

        [InlineData("11 PB", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 petabyte", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 petabytes", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 P", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 p", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 Pi", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 PiB", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 pebibyte", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 pebibytes", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]

        [InlineData("1EB", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1exabyte", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1exabytes", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1E", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1e", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1Ei", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1EiB", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1exbibyte", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1exbibytes", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]

        [InlineData("1 EB", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1 exabyte", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1 exabytes", 1L * 1000L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("1 E", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1 e", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1 Ei", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1 EiB", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1 exbibyte", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("1 exbibytes", 1L * 1024L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        public void Byte_sizes_are_understood(string value, long? expected)
        {
            var hocon = $"byte-size = {value}";
            var config = Parser.Parse(hocon);
            var actual = config.GetByteSize("byte-size");
            if (expected.HasValue)
                Assert.True(expected.Equals(actual), $"'{value}' is {expected} bytes");
            else
                Assert.True(null == actual, $"'{value}' should be null");
        }


        [Fact]
        public void Can_parse_nanoseconds()
        {
            var expected = TimeSpan.FromTicks(12);
            var value = "1234 ns";
            var hocon = $"timespan = {value}";

            var res = Parser.Parse(hocon).GetTimeSpan("timespan");
            Assert.True(expected.Equals(res), $"'{value}' should rounds to 12 ticks");
        }

        [Fact]
        public void Can_parse_microseconds()
        {
            var expected = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * 0.123));
            var value = "123 microseconds";
            var hocon = $"timespan = {value}";

            var res = Parser.Parse(hocon).GetTimeSpan("timespan");
            Assert.True(expected.Equals(res), $"'{value}' is {expected}");
        }

    }
}
