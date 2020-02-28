// -----------------------------------------------------------------------
// <copyright file="SelfReferentialSubstitution.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class SelfReferentialSubstitution
    {
        public SelfReferentialSubstitution(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Theory]
        [InlineData(@"{
  a {
    b: ""a:b:c""
  }
  a.b: ${a.b} "":d""
}")]
        [InlineData(@"{
  a {
    b: ""a:b:c""
  }
  a {
    b: ${a.b} "":d""
  }
}")]
        [InlineData(@"{
  a.b: ""a:b:c""
  a.b: ${a.b} "":d""
}")]
        [InlineData(@"{
  a.b: ""a:b:c""
  a {
    b: ${a.b} "":d""
  }
}")]
        public void CanValueConcatenateOlderValueInsideObject_Issue_95_97(string hocon)
        {
            var config = HoconParser.Parse(hocon);
            Assert.Equal("a:b:c:d", config.GetString("a.b"));
        }

        [Theory]
        [InlineData(@"{
  a {
    b: [1, 2]
  }
  a {
    b: ${a.b} [3, 4]
  }
}")]
        [InlineData(@"{
  a {
    b: [1, 2]
  }
  a.b: ${a.b} [3, 4]
}")]
        [InlineData(@"{
  a.b: [1, 2]
  a {
    b: ${a.b} [3, 4]
  }
}")]
        [InlineData(@"{
  a.b: [1, 2]
  a.b: ${a.b} [3, 4]
}")]
        public void CanValueConcatenateOlderArrayInsideObject_Issue_95_97(string hocon)
        {
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {1, 2, 3, 4}.SequenceEqual(config.GetIntList("a.b")));
        }

        /*
         * FACT:
         * A cyclic or circular loop substitution should be detected as invalid.
         */
        [Theory]
        [InlineData(@"
bar : ${foo}
foo : ${bar}")]
        [InlineData(@"
a : ${b}
b : ${c}
c : ${a}")]
        [InlineData(@"
a : 1
b : 2
a : ${b}
b : ${a}")]
        public void ThrowsOnCyclicSubstitutionDetection(string hocon)
        {
            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            Assert.Contains("cyclic", ex.Message);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /*
         * FACT:
         * When merging two objects, the self-reference in the overriding field refers to the overridden field.
         */
        [Fact]
        public void CanReferToOverriddenField()
        {
            var hocon = @"
foo : { a : 1 }
foo : ${foo}";

            var config = HoconParser.Parse(hocon);
            Assert.Equal("1", config.GetString("foo.a"));
        }

        [Fact]
        public void CanValueConcatenateOlderArray()
        {
            var hocon = @"
path : [ /usr/etc, /usr/home ]
path : ${path} [ /usr/bin ]";

            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {"/usr/etc", "/usr/home", "/usr/bin"}.SequenceEqual(config.GetStringList("path")));
        }

        /*
         * FACT:
         * Allow a new value for a field to be based on the older value
         */
        [Fact]
        public void CanValueConcatenateOlderValue()
        {
            var hocon = @"
path : ""a:b:c""
path : ${path} "":d""";

            var config = HoconParser.Parse(hocon);
            Assert.Equal("a:b:c:d", config.GetString("path"));
        }

        /*
         * FACT:
         * If a substitution is hidden by a value that could not be merged with it
         * (by a non-object value) then it is never evaluated and no error will be reported
         */
        [Fact]
        public void HiddenSubstitutionShouldNeverBeEvaluated()
        {
            var hocon = @"
foo : ${does-not-exist}
foo : 42
bar : ${yet-another-to-ignore}
bar : [42]
";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);
            Assert.Equal(42, config.GetInt("foo"));
        }

        /*
         * FACT:
         * A self-reference resolves to the value "below" even if it's part of a path expression.
         * Here, ${foo.a} would refer to { c : 1 } rather than 2 and so the final merge would be { a : 2, c : 1 }
         */
        [Fact]
        public void MergedSubstitutionShouldAlwaysResolveToOlderValue()
        {
            var hocon = @"
foo : { a : { c : 1 } }
foo : ${foo.a}
foo : { a : 2 }";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);

            Assert.Equal(2, config.GetInt("foo.a"));
            Assert.Equal(1, config.GetInt("foo.c"));
            Assert.False(config.HasPath("foo.a.c"));
        }

        /*
         * FACT:
         * Mutually-referring objects should also work, and are not self-referential
         */
        [Fact]
        public void MutuallyReferringObjectsAreResolvedNormally()
        {
            var hocon = @"
// bar.a should end up as 4
bar : { a : ${foo.d}, b : 1 }
bar.b = 3
// foo.c should end up as 3
foo : { c : ${bar.b}, d : 2 }
foo.d = 4";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);

            Assert.Equal(4, config.GetInt("bar.a"));
            Assert.Equal(3, config.GetInt("foo.c"));
        }

        /*
         * FACT:
         *  the optional substitution syntax ${?foo} does not create a cycle
         */
        [Fact]
        public void OptionalSubstitutionCycleShouldBeIgnored()
        {
            var hocon = "foo : ${?foo}";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);
            // should not create a field
            Assert.False(config.HasPath("foo"));
        }

        /*
         * FACT:
         * Fields may have += as a separator rather than : or =. A field with += transforms into an optional self-referential array concatenation 
         * {a += b} becomes {a = ${?a} [b]}
         */
        [Fact]
        public void PlusEqualOperatorShouldExpandToSelfReferencingArrayConcatenation()
        {
            var hocon = @"
a = [ 1, 2 ]
a += 3
a += ${b}
b = [ 4, 5 ]
";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);
            var array = config.GetValue("a").GetArray();
            Assert.Equal(1, array[0].GetInt());
            Assert.Equal(2, array[1].GetInt());
            Assert.Equal(3, array[2].GetInt());
            Assert.True(new[] {4, 5}.SequenceEqual(array[3].GetIntList()));
        }

        /*
         * FACT:
         * Value concatenated optional substitution should be ignored and dropped silently
         */
        [Fact]
        public void SelfReferenceOptionalSubstitutionInValueConcatenationShouldBeIgnored()
        {
            var hocon = "a = ${?a}foo";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);

            Assert.Equal("foo", config.GetString("a"));
        }

        /*
         * FACT:
         * Implementations must be careful to allow objects to refer to paths within themselves.
         * The test below is NOT a self reference nor a cycle, the final value for bar.foo 
         * and bar.baz should be 43 (forward checking)
         */
        [Fact]
        public void SubstitutionToAnotherMemberOfTheSameObjectAreResolvedNormally()
        {
            var hocon = @"
bar : { foo : 42,
        baz : ${bar.foo}
      }
bar : { foo : 43 }";

            HoconRoot config = null;
            var ex = Record.Exception(() => config = HoconParser.Parse(hocon));
            Assert.Null(ex);

            Assert.Equal(43, config.GetInt("bar.foo"));
            Assert.Equal(43, config.GetInt("bar.baz"));
        }

        /*
         * FACT:
         * In isolation (with no merges involved), a self-referential field is an error because the substitution cannot be resolved.
         */
        [Fact]
        public void ThrowsWhenThereAreNoOldValue()
        {
            var hocon = "foo : ${foo}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /*
         * FACT:
         * It would be an error if these two fields were reversed
         */
        [Fact]
        public void ThrowsWhenThereAreNoOverriddenValueAvailable()
        {
            var hocon = @"
foo : ${foo}
foo : { a : 1 }
";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
    }
}