//-----------------------------------------------------------------------
// <copyright file="ApiTest.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Immutable.Tests
{
    public class ApiTest
    {
        private readonly ITestOutputHelper _output;

        public ApiTest(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> ObjectArrayData =>
            new List<object[]>
            {
                new object[] {"a=[{a=1, b=2}, {c=3,d=4}, {e=5,f=6}, ]"},
                new object[]{@"a=[
  {
    a=1,
    b=2
  }, 
  {
    c=3,
    d=4
  }, 
  {
    e=5,
    f=6
  },
]"},
                new object[]{@"a=[
{a=1,b=2}, 
{c=3,d=4}, 
{e=5,f=6},]"},
                new object[]{@"a=[
{a=1,b=2}, 
{c=3,d=4}, 
{e=5,f=6}]"}
            };
        [Theory]
        [MemberData(nameof(ObjectArrayData))]
        public void CanIntelligentlyUseIndexerTypeToAccessMembers(string hocon)
        {
            var config = Parser.Parse(hocon).ToHoconImmutable();

            Assert.Equal(1, config["a"][0]["a"].GetInt());
            Assert.Equal(2, config["a"][0]["b"].GetInt());

            Assert.Equal(3, config["a"][1]["c"].GetInt());
            Assert.Equal(4, config["a"][1]["d"].GetInt());

            Assert.Equal(5, config["a"][2]["e"].GetInt());
            Assert.Equal(6, config["a"][2]["f"].GetInt());
        }


        [Fact]
        public void CanAutomaticallyCastValuesToPrimitives()
        {
            var hocon = @"{
  root : {
    bool : true

    byte : 255,
    sbyte : -128,

    short : -32768,
    ushort : 65535,

    int : -2147483648,
    uint : 4294967295,

    long : -9223372036854775808,
    ulong : 18446744073709551615,

    double : 1.23

    string : foo
    quoted-string = ""foo""
    unquoted-string = bar
    concat-string = foo bar

    object : {
      hasContent : false
    }

    array : [1,2,3]
    float-array : [1.0,2.0,3.0]
    array-concat : [[1,2] [3,4]]
    array-single-element : [1 2 3 4]
    array-newline-element : [
        1
        2
        3
        4
    ]
    array-array : [[1,2],[3,4]]
    string-array: [""1"",""2"",""3""]

    timespan: 1m
    null : null
  },

  root_2 : 1234
}";
            var config = Parser.Parse(hocon).ToHoconImmutable();
            Assert.True(config["root.bool"].GetBoolean());
            Assert.True(config["root.bool"].Get<bool>());
            Assert.True(config["root.bool"]);

            Assert.False(config["root.object.hasContent"].GetBoolean());
            Assert.False(config["root.object.hasContent"].Get<bool>());
            Assert.False(config["root.object.hasContent"]);

            Assert.Equal(byte.MaxValue, config["root.byte"].GetByte());
            Assert.Equal(byte.MaxValue, config["root.byte"].Get<byte>());
            Assert.Equal(byte.MaxValue, config["root.byte"]);

            Assert.Equal(sbyte.MinValue, config["root.sbyte"].GetSByte());
            Assert.Equal(sbyte.MinValue, config["root.sbyte"].Get<sbyte>());
            Assert.Equal(sbyte.MinValue, config["root.sbyte"]);

            Assert.Equal(short.MinValue, config["root.short"].GetShort());
            Assert.Equal(short.MinValue, config["root.short"].Get<short>());
            Assert.Equal(short.MinValue, config["root.short"]);

            Assert.Equal(ushort.MaxValue, config["root.ushort"].GetUShort());
            Assert.Equal(ushort.MaxValue, config["root.ushort"].Get<ushort>());
            Assert.Equal(ushort.MaxValue, config["root.ushort"]);

            Assert.Equal(int.MinValue, config["root.int"].GetInt());
            Assert.Equal(int.MinValue, config["root.int"].Get<int>());
            Assert.Equal(int.MinValue, config["root.int"]);

            Assert.Equal(uint.MaxValue, config["root.uint"].GetUInt());
            Assert.Equal(uint.MaxValue, config["root.uint"].Get<uint>());
            Assert.Equal(uint.MaxValue, config["root.uint"]);

            Assert.Equal(long.MinValue, config["root.long"].GetLong());
            Assert.Equal(long.MinValue, config["root.long"].Get<long>());
            Assert.Equal(long.MinValue, config["root.long"]);

            Assert.Equal(ulong.MaxValue, config["root.ulong"].GetULong());
            Assert.Equal(ulong.MaxValue, config["root.ulong"].Get<ulong>());
            Assert.Equal(ulong.MaxValue, config["root.ulong"]);

            Assert.Equal(1.23f, config["root.double"].GetFloat());
            Assert.Equal(1.23f, config["root.double"].Get<float>());
            Assert.Equal(1.23f, config["root.double"]);

            Assert.Equal(1.23, config["root.double"].GetDouble());
            Assert.Equal(1.23, config["root.double"].Get<double>());
            Assert.Equal(1.23, config["root.double"]);

            Assert.Null(config["root.null"].GetString());
            Assert.Null(config["root.null"].Get<string>());

            Assert.Equal("foo", config["root.string"].GetString());
            Assert.Equal("foo", config["root.string"].Get<string>());
            Assert.Equal("foo", config["root.string"]);

            Assert.Equal("foo", config["root.quoted-string"]);
            Assert.Equal("bar", config["root.unquoted-string"]);
            Assert.Equal("foo bar", config["root.concat-string"]);


            Assert.Equal(TimeSpan.FromMinutes(1), config["root.timespan"].GetTimeSpan());
            Assert.Equal(TimeSpan.FromMinutes(1), config["root.timespan"].Get<TimeSpan>());
            Assert.Equal(TimeSpan.FromMinutes(1), config["root.timespan"]);

            Assert.True(new byte[] {1, 2, 3}.SequenceEqual(config["root.array"].ToByteArray()));
            Assert.True(new byte[] {1, 2, 3}.SequenceEqual(config["root.array"].Get<byte[]>()));
            Assert.True(new byte[] {1, 2, 3}.SequenceEqual((byte[]) config["root.array"]));
            Assert.Equal((byte) 1, config["root.array"][0].Get<byte>());
            Assert.Equal((byte) 1, config["root.array"][0]);

            Assert.True(new[] {1, 2, 3}.SequenceEqual(config["root.array"].ToIntArray()));
            Assert.True(new[] {1, 2, 3}.SequenceEqual(config["root.array"].Get<int[]>()));
            Assert.True(new[] {1, 2, 3}.SequenceEqual((int[]) config["root.array"]));
            Assert.Equal(1, config["root.array"][0].Get<int>());
            Assert.Equal(1, config["root.array"][0]);

            Assert.True(new uint[] {1, 2, 3}.SequenceEqual(config["root.array"].Get<uint[]>()));
            Assert.True(new uint[] {1, 2, 3}.SequenceEqual((uint[]) config["root.array"]));
            Assert.Equal((uint) 1, config["root.array"][0].Get<uint>());
            Assert.Equal((uint) 1, config["root.array"][0]);

            Assert.True(new ushort[] {1, 2, 3}.SequenceEqual(config["root.array"].Get<ushort[]>()));
            Assert.True(new ushort[] {1, 2, 3}.SequenceEqual((ushort[]) config["root.array"]));
            Assert.Equal((ushort) 1, config["root.array"][0].Get<ushort>());
            Assert.Equal((ushort) 1, config["root.array"][0]);

            Assert.True(new ulong[] {1, 2, 3}.SequenceEqual(config["root.array"].Get<ulong[]>()));
            Assert.True(new ulong[] {1, 2, 3}.SequenceEqual((ulong[]) config["root.array"]));
            Assert.Equal((ulong) 1, config["root.array"][0].Get<ulong>());
            Assert.Equal((ulong) 1, config["root.array"][0]);

            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual((int[]) config["root.array-newline-element"]));
            Assert.True(new[] {"1 2 3 4"}.SequenceEqual((string[]) config["root.array-single-element"]));

            Assert.True(new float[] {1, 2, 3}.SequenceEqual(config["root.float-array"].ToFloatArray()));
            Assert.True(new float[] {1, 2, 3}.SequenceEqual(config["root.float-array"].Get<float[]>()));
            Assert.True(new float[] {1, 2, 3}.SequenceEqual((float[]) config["root.float-array"]));
            Assert.Equal(1f, config["root.float-array"][0].Get<float>());
            Assert.Equal(1f, config["root.float-array"][0]);

            Assert.True(new double[] {1, 2, 3}.SequenceEqual(config["root.float-array"].ToDoubleArray()));
            Assert.True(new double[] {1, 2, 3}.SequenceEqual(config["root.float-array"].Get<double[]>()));
            Assert.True(new double[] {1, 2, 3}.SequenceEqual((double[]) config["root.float-array"]));
            Assert.Equal(1.0, config["root.float-array"][0].Get<double>());
            Assert.Equal(1.0, config["root.float-array"][0]);

            Assert.True(new decimal[] {1, 2, 3}.SequenceEqual(config["root.float-array"].ToDecimalArray()));
            Assert.True(new decimal[] {1, 2, 3}.SequenceEqual(config["root.float-array"].Get<decimal[]>()));
            Assert.True(new decimal[] {1, 2, 3}.SequenceEqual((decimal[]) config["root.float-array"]));
            Assert.Equal(1M, config["root.float-array"][0].Get<decimal>());
            Assert.Equal(1M, config["root.float-array"][0]);

            Assert.True(new[] {"1", "2", "3"}.SequenceEqual(config["root.string-array"].ToStringArray()));
            Assert.True(new[] {"1", "2", "3"}.SequenceEqual(config["root.string-array"].Get<string[]>()));
            Assert.True(new[] {"1", "2", "3"}.SequenceEqual((string[]) config["root.string-array"]));
            Assert.Equal("1", config["root.string-array"][0].Get<string>());
            Assert.Equal("1", config["root.string-array"][0]);

            Assert.True(new[] {1, 2}.SequenceEqual((int[]) config["root.array-array"][0]));
            Assert.True(new[] {3, 4}.SequenceEqual((int[]) config["root.array-array"][1]));

            Assert.Equal("1234", config["root_2"].GetString());
            Assert.Equal("1234", config["root_2"].Get<string>());
            Assert.Equal("1234", config["root_2"]);
        }
    }
}
