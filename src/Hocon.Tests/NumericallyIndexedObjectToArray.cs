// -----------------------------------------------------------------------
// <copyright file="NumericallyIndexedObjectToArray.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using FluentAssertions;
using Xunit;

namespace Hocon.Tests
{
    public class NumericallyIndexedObjectToArray
    {
        /*
         * FACT:
         * the conversion should not occur if the object is empty or has no keys which parse as positive integers.
         */
        [Theory]
        [InlineData("a:{}")]
        [InlineData("a:{-1:{foo:a}, -2:{bar:a}}")]
        public void WithEmptyOrNonPositiveIndicesShouldThrow(string hocon)
        {
            var config = HoconParser.Parse(hocon);
            var ex = Assert.Throws<HoconValueException>(() => { config.GetObjectList("a"); });
            ex.GetBaseException().Should().BeOfType<HoconException>();
            ex.FailPath.Should().Be("a");
        }

        /*
         * FACT:
         * the conversion should ignore any keys which do not parse as positive integers.
         */
        [Theory]
        [InlineData("a:{0:a, ignored:b, 1:c}")]
        [InlineData("a:{0:a, -1:b, 1:c}")]
        public void ShouldIgnoreAnyKeysUnparseableToPositiveInt(string hocon)
        {
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {"a", "c"}.SequenceEqual(config.GetStringList("a")));
        }

        /*
         * FACT:
         * the conversion should sort by the integer value of each key and then build the array;
         * if the integer keys are "0" and "2" then the resulting array would have indices "0" and "1",
         * i.e. missing indices in the object are eliminated.
         */
        [Theory]
        [InlineData("a:{2:c, 0:a, 1:b}")]
        [InlineData("a:{22:c, 5:a, 12:b}")]
        public void ShouldSortBasedOnIndicesAndProduceNonSparseArray(string hocon)
        {
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {"a", "b", "c"}.SequenceEqual(config.GetStringList("a")));
        }

        /*
         * FACT:
         * Numerically-indexed object can be converted into an array of a specific type
         */
        [Fact]
        public void CanConvertToBooleanList()
        {
            var hocon = @"a={0:true, 1:false, 2:true, 3:false}";
            var config = HoconParser.Parse(hocon);

            Assert.True(config.GetBoolean("a.0"));
            Assert.False(config.GetBoolean("a.1"));
            Assert.True(config.GetBoolean("a.2"));
            Assert.False(config.GetBoolean("a.3"));
            Assert.True(new[] {true, false, true, false}.SequenceEqual(config.GetBooleanList("a")));
        }

        [Fact]
        public void CanConvertToByteList()
        {
            var hocon = @"a={0:1, 1:2, 2:3, 3:4}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal((byte) 1, config.GetByte("a.0"));
            Assert.Equal((byte) 2, config.GetByte("a.1"));
            Assert.Equal((byte) 3, config.GetByte("a.2"));
            Assert.Equal((byte) 4, config.GetByte("a.3"));
            Assert.True(new[] {(byte) 1, (byte) 2, (byte) 3, (byte) 4}.SequenceEqual(config.GetByteList("a")));
        }

        [Fact]
        public void CanConvertToDecimalList()
        {
            var hocon = @"a={0:1.0, 1:2.0, 2:3.0, 3:4.0}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal(1.0M, config.GetDecimal("a.0"));
            Assert.Equal(2.0M, config.GetDecimal("a.1"));
            Assert.Equal(3.0M, config.GetDecimal("a.2"));
            Assert.Equal(4.0M, config.GetDecimal("a.3"));
            Assert.True(new[] {1.0M, 2.0M, 3.0M, 4.0M}.SequenceEqual(config.GetDecimalList("a")));
        }

        [Fact]
        public void CanConvertToDoubleList()
        {
            var hocon = @"a={0:1.0, 1:2.0, 2:3.0, 3:4.0}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal(1.0, config.GetDouble("a.0"));
            Assert.Equal(2.0, config.GetDouble("a.1"));
            Assert.Equal(3.0, config.GetDouble("a.2"));
            Assert.Equal(4.0, config.GetDouble("a.3"));
            Assert.True(new[] {1.0, 2.0, 3.0, 4.0}.SequenceEqual(config.GetDoubleList("a")));
        }

        [Fact]
        public void CanConvertToFloatList()
        {
            var hocon = @"a={0:1.0, 1:2.0, 2:3.0, 3:4.0}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal(1.0f, config.GetFloat("a.0"));
            Assert.Equal(2.0f, config.GetFloat("a.1"));
            Assert.Equal(3.0f, config.GetFloat("a.2"));
            Assert.Equal(4.0f, config.GetFloat("a.3"));
            Assert.True(new[] {1.0f, 2.0f, 3.0f, 4.0f}.SequenceEqual(config.GetFloatList("a")));
        }

        [Fact]
        public void CanConvertToIntList()
        {
            var hocon = @"a={0:1, 1:2, 2:3, 3:4}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal(1, config.GetInt("a.0"));
            Assert.Equal(2, config.GetInt("a.1"));
            Assert.Equal(3, config.GetInt("a.2"));
            Assert.Equal(4, config.GetInt("a.3"));
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(config.GetIntList("a")));
        }

        [Fact]
        public void CanConvertToLongList()
        {
            var hocon = @"a={0:1, 1:2, 2:3, 3:4}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal(1L, config.GetLong("a.0"));
            Assert.Equal(2L, config.GetLong("a.1"));
            Assert.Equal(3L, config.GetLong("a.2"));
            Assert.Equal(4L, config.GetLong("a.3"));
            Assert.True(new[] {1L, 2L, 3L, 4L}.SequenceEqual(config.GetLongList("a")));
        }

        [Fact]
        public void CanConvertToObjectList()
        {
            var hocon = @"
a: {
  0: {
    a: {
      foo: bar
    }
  }, 
  1:{
    a: larry
    b: moe
  }, 
  2:{
    a: ben
    b: jerry
  }, 
}";
            var config = HoconParser.Parse(hocon);

            var objectList = config.GetObjectList("a");
            Assert.Equal("bar", objectList[0]["a.foo"].GetString());
            Assert.Equal("larry", objectList[1]["a"].GetString());
            Assert.Equal("moe", objectList[1]["b"].GetString());
            Assert.Equal("ben", objectList[2]["a"].GetString());
            Assert.Equal("jerry", objectList[2]["b"].GetString());
        }

        [Fact]
        public void CanConvertToStringList()
        {
            var hocon = @"a={0:a, 1:b, 2:c, 3:d}";
            var config = HoconParser.Parse(hocon);

            Assert.Equal("a", config.GetString("a.0"));
            Assert.Equal("b", config.GetString("a.1"));
            Assert.Equal("c", config.GetString("a.2"));
            Assert.Equal("d", config.GetString("a.3"));
            Assert.True(new[] {"a", "b", "c", "d"}.SequenceEqual(config.GetStringList("a")));
        }

        [Fact]
        public void WithMixedFieldValuesShouldThrow()
        {
            var hocon = @"
a: {
  0: {
    a: {
      foo: bar
    }
  }, 
  1: b 
  2: 1 
}";
            var config = HoconParser.Parse(hocon);

            Assert.Throws<HoconValueException>(() => { config.GetObjectList("a"); });
            Assert.Throws<HoconValueException>(() => { config.GetStringList("a"); });
            Assert.Throws<HoconValueException>(() => { config.GetIntList("a"); });
        }
    }
}