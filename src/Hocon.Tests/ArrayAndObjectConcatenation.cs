﻿//-----------------------------------------------------------------------
// <copyright file="ArrayAndObjectConcatenation.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class ArrayAndObjectConcatenation
    {
        private readonly ITestOutputHelper _output;

        public ArrayAndObjectConcatenation(ITestOutputHelper output)
        {
            _output = output;
        }

        /*
         * FACT:
         * Arrays can be concatenated with arrays, and objects with objects, but it is an error if they are mixed.
         */
        [Fact]
        public void CanConcatenateArray()
        {
            var hocon = @"a=[1,2] [3,4]";
            Assert.True(new[] { 1, 2, 3, 4 }.SequenceEqual(Parser.Parse(hocon).GetIntList("a")));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_1()
        {
            var hocon = "a : { b : 1 } { c : 2 }";
            var config = Parser.Parse(hocon);
            Assert.Equal(1, config.GetInt("a.b"));
            Assert.Equal(2, config.GetInt("a.c"));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_2()
        {
            var hocon = @"
data-center-generic = { cluster-size = 6 }
data-center-east = ${data-center-generic} { name = ""east"" }";

            var config = Parser.Parse(hocon);

            Assert.Equal(6, config.GetInt("data-center-generic.cluster-size"));

            Assert.Equal(6, config.GetInt("data-center-east.cluster-size"));
            Assert.Equal("east", config.GetString("data-center-east.name"));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_3()
        {
            var hocon = @"
data-center-generic = { cluster-size = 6 }
data-center-east = { name = ""east"" } ${data-center-generic}";

            var config = Parser.Parse(hocon);

            Assert.Equal(6, config.GetInt("data-center-generic.cluster-size"));

            Assert.Equal(6, config.GetInt("data-center-east.cluster-size"));
            Assert.Equal("east", config.GetString("data-center-east.name"));
        }

        [Fact]
        public void CanConcatenateObjectsWhenMerged()
        {
            var hocon = @"
a : { b : 1 } 
a : { c : 2 }";

            var config = Parser.Parse(hocon);
            Assert.Equal(1, config.GetInt("a.b"));
            Assert.Equal(2, config.GetInt("a.c"));
        }

        #region Array and object concatenation exception spec
        [Fact]
        public void ThrowsWhenArrayAndObjectAreConcatenated_1()
        {
            var hocon = @"a : [1,2] { c : 2 }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");

        }

        [Fact]
        public void ThrowsWhenArrayAndObjectAreConcatenated_2()
        {
            var hocon = @"a : { c : 2 } [1,2]";
            
            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndObjectSubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : { c : 2 }
b : [1,2] ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndObjectSubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : { c : 2 }
b : ${a} [1,2]";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndArraySubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : [1,2]
b : { c : 2 } ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndArraySubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : [1,2]
b : ${a} { c : 2 }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
        #endregion

        #region String  and object concatenation exception spec
        [Fact]
        public void ThrowsWhenStringAndObjectAreConcatenated_1()
        {
            var hocon = @"a : literal { c : 2 }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectAreConcatenated_2()
        {
            var hocon = @"a : { c : 2 } literal";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectSubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : { c : 2 }
b : literal ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectSubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : { c : 2 }
b : ${a} literal";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndStringSubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : literal
b : ${a} { c : 2 }";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndStringSubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : literal
b : { c : 2 } ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
        #endregion

        #region String and array concatenation exception spec
        [Fact]
        public void ThrowsWhenArrayAndStringAreConcatenated_1()
        {
            var hocon = @"a : [1,2] literal";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringAreConcatenated_2()
        {
            var hocon = @"a : literal [1,2]";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringSubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : literal
b : ${a} [1,2]";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringSubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : literal
b : [1,2] ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndArraySubstitutionAreConcatenated_1()
        {
            var hocon = @"
a : [1,2]
b : ${a} literal";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndArraySubstitutionAreConcatenated_2()
        {
            var hocon = @"
a : [1,2]
b : literal ${a}";

            var ex = Record.Exception(() => Parser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
        #endregion
    }
}
