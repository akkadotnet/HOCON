// -----------------------------------------------------------------------
// <copyright file="Substitution.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class Substitution
    {
        public Substitution(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Theory]
        [InlineData("[${foo}]", new[] {"hello"})]
        [InlineData("[${foo},world]", new[] {"hello", "world"})]
        public void SimpleSubstitutionBetweenBracketsShouldResolveToArray(string bar, string[] expected)
        {
            var hocon = $@"foo = hello
bar = {bar}";

            var config = HoconParser.Parse(hocon);
            Assert.Equal(expected, config.GetStringList("bar").ToArray());
        }

        /*
         * FACT:
         * A substitution is replaced with any value type (number, object, string, array, true, false, null)
         */

        [Fact]
        public void CanAssignSubstitutionToField()
        {
            var hocon = @"a{
    int = 1
    number = 10.0
    string = string
    boolean = true
    null = null

    b = ${a.number}
    c = ${a.string}
    d = ${a.boolean}
    e = ${a.null}
    f = ${a.int}
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(10.0, config.GetFloat("a.b"));
            Assert.Equal("string", config.GetString("a.c"));
            Assert.True(config.GetBoolean("a.d"));
            Assert.Null(config.GetString("a.e"));
            Assert.Equal(1, HoconParser.Parse(hocon).GetInt("a.f"));
        }

        [Fact]
        public void CanConcatenateArray_1()
        {
            var hocon = @"a {
  b = [1,2,3]
  c = ${a.b} [4,5,6]
}";
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {1, 2, 3, 4, 5, 6}.SequenceEqual(config.GetIntList("a.c")));
        }

        [Fact]
        public void CanConcatenateArray_2()
        {
            var hocon = @"a {
  b = [4,5,6]
  c = [1,2,3] ${a.b}
}";
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {1, 2, 3, 4, 5, 6}.SequenceEqual(config.GetIntList("a.c")));
        }

        /*
         * FACT:
         * or can use value concatenation of a substitution and a quoted string.
         */
        [Fact]
        public void CanConcatenateQuotedString()
        {
            var hocon = @"a {
  name = Roger
  c = ""Hello my name is ""${a.name}
}";
            Assert.Equal("Hello my name is Roger", HoconParser.Parse(hocon).GetString("a.c"));
        }

        /*
         * FACT:
         * or can use value concatenation of a substitution and a quoted string.
         */
        [Fact]
        public void CanConcatenateQuotedStringOfArrayElement()
        {
            var hocon = @"a {
  name = Roger
  c = [""Hello my name is ""${a.name}]
}";
            Assert.Equal("Hello my name is Roger", HoconParser.Parse(hocon).GetStringList("a.c")[0]);
        }

        /*
         * FACT:
         * To get a string containing a substitution, 
         * you must use value concatenation with the substitution in the unquoted portion
         */
        [Fact]
        public void CanConcatenateUnquotedString()
        {
            var hocon = @"a {
  name = Roger
  c = Hello my name is ${a.name}
}";
            Assert.Equal("Hello my name is Roger", HoconParser.Parse(hocon).GetString("a.c"));
        }

        /*
         * FACT:
         * To get a string containing a substitution as an array element, 
         * you must use value concatenation with the substitution in the unquoted portion
         */
        [Fact]
        public void CanConcatenateUnquotedStringOfArrayElement()
        {
            var hocon = @"a {
  name = Roger
  c = [Hello my name is ${a.name}]
}";
            Assert.Equal("Hello my name is Roger", HoconParser.Parse(hocon).GetStringList("a.c")[0]);
        }

        [Fact]
        public void CanCSubstituteObject()
        {
            var hocon = @"a {
  b {
      foo = hello
      bar = 123
  }
  c {
     d = xyz
     e = ${a.b}
  }  
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("hello", config.GetString("a.c.e.foo"));
            Assert.Equal(123, config.GetInt("a.c.e.bar"));
        }

        [Fact]
        public void CanResolveSubstitutesInInclude()
        {
            var hocon = @"a {
    b { 
        include ""foo""
    }
}";
            var includeHocon = @"
x = 123
y = ${x}
";

            Task<string> IncludeCallback(HoconCallbackType t, string s)
            {
                return Task.FromResult(includeHocon);
            }

            var config = HoconParser.Parse(hocon, IncludeCallback);

            Assert.Equal(123, config.GetInt("a.b.x"));
            Assert.Equal(123, config.GetInt("a.b.y"));
        }

        [Fact]
        public void CanResolveSubstitutesInNestedIncludes()
        {
            var hocon = @"a.b.c {
    d { 
        include ""hocon1""
    }
}";
            var includeHocon = @"
f = 123
e {
      include ""hocon2""
}";

            var includeHocon2 = @"
x = 123
y = ${x}
";

            Task<string> Include(HoconCallbackType t, string s)
            {
                switch (s)
                {
                    case "hocon1":
                        return Task.FromResult(includeHocon);
                    case "hocon2":
                        return Task.FromResult(includeHocon2);
                    default:
                        return Task.FromResult("{}");
                }
            }

            var config = HoconParser.Parse(hocon, Include);

            Assert.Equal(123, config.GetInt("a.b.c.d.e.x"));
            Assert.Equal(123, config.GetInt("a.b.c.d.e.y"));
        }

        /*
         * FACT:
         * For substitutions which are not found in the configuration tree, 
         * implementations may try to resolve them by looking at other external sources of configuration.
         */
        // TODO: Create external config file lookup and loading implementation

        /*
         * FACT:
         * Substitutions are not parsed inside quoted strings.
         */
        [Fact]
        public void DoNotParseSubstitutionInsideQuotedString()
        {
            var hocon = @"a{
    b = 5
    c = ""I have ${a.b} Tesla car(s).""
}";
            Assert.Equal("I have ${a.b} Tesla car(s).", HoconParser.Parse(hocon).GetString("a.c"));
        }

        /*
         * FACT:
         * If the substitution is the only part of a value, then the type is preserved. Otherwise, it is value-concatenated to form a string.
         */
        [Fact]
        public void FieldSubstitutionShouldPreserveType()
        {
            var hocon = @"a{
    b = 1
    c = ${a.b}23
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(1, config.GetInt("a.b"));
            Assert.Equal(123, config.GetInt("a.c"));
        }

        [Fact]
        public void FieldSubstitutionWithDifferentTypesShouldConcatenateToString()
        {
            var hocon = @"a{
    b = 1
    c = ${a.b}foo
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(1, config.GetInt("a.b"));
            Assert.Equal("1foo", config.GetString("a.c"));
        }

        [Fact]
        public void NullValueQuestionMarkSubstitutionShouldNotLookUpExternalSource()
        {
            var hocon = @"
MY_ENV_VAR = null
a {
  b = ${?MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Null(HoconParser.Parse(hocon).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        /*
         * FACT:
         * If a key has been specified more than once, 
         * the substitution will always evaluate to its latest-assigned value 
         * (that is, it will evaluate to the merged object, or the last non-object value that was set, 
         * in the entire document being parsed including all included files).
         */
        // TODO: Need test implementation.

        /*
         * FACT:
         * If a configuration sets a value to null then it should not be looked up in the external source.
         */
        [Fact]
        public void NullValueSubstitutionShouldNotLookUpExternalSource()
        {
            var hocon = @"
MY_ENV_VAR = null
a {
  b = ${MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Null(HoconParser.Parse(hocon).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        [Fact]
        public void QuestionMarkShouldFallbackToEnvironmentVariables()
        {
            var hocon = @"a {
  b = ${?MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Equal(value, HoconParser.Parse(hocon).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        /*
         * FACT:
         * For substitutions which are not found in the configuration tree, 
         * implementations may try to resolve them by looking at system environment variables.
         */
        [Fact]
        public void ShouldFallbackToEnvironmentVariables()
        {
            var hocon = @"a {
  b = ${MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Equal(value, HoconParser.Parse(hocon).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        /*
         * FACT:
         * Substitutions are resolved by looking up the path in the configuration. 
         * The path begins with the root configuration object, i.e. it is "absolute" rather than "relative."
         */
        // TODO: Is this test-able?

        /*
         * FACT:
         * Substitution processing is performed as the last parsing step, 
         * so a substitution can look forward in the configuration. 
         * If a configuration consists of multiple files, 
         * it may even end up retrieving a value from another file.
         */
        [Fact]
        public void SubstitutionShouldLookForwardInConfiguration()
        {
            var hocon = @"a{
    b = ${a.c}
    c = 42
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(42, config.GetInt("a.b"));
        }

        /*
         * FACT:
         * The syntax is ${pathexpression} or ${?pathexpression}
         */

        /*
         * FACT:
         * The two characters ${ must be exactly like that, grouped together.
         */
        [Fact]
        public void ThrowsOnInvalidSubstitutionStartToken()
        {
            var hocon = @"a{
    b = 1
    c = $ {a.b}
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /*
         * FACT:
         * - The ? in ${?pathexpression} must not have whitespace before it
         * - The three characters ${? must be exactly like that, grouped together.
         */
        [Fact]
        public void ThrowsOnInvalidSubstitutionWithQuestionMarkStartToken_1()
        {
            var hocon = @"a{
    b = 1
    c = ${ ?a.b}
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnInvalidSubstitutionWithQuestionMarkStartToken_2()
        {
            var hocon = @"a{
    b = 1
    c = $ {?a.b}
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /*
         * Substitutions are not allowed in keys or nested inside other substitutions (path expressions)
         */
        [Fact]
        public void ThrowsOnSubstitutionInKeys()
        {
            var hocon = @"a{
    b = 1
    c = b
    ${a.c} = 2;
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnSubstitutionInSubstitution()
        {
            var hocon = @"a{
    bar = foo
    foo = ${?a.${?bar}}
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnSubstitutionWithQuestionMarkInKeys()
        {
            var hocon = @"a{
    b = 1
    c = b
    ${?a.c} = 2;
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /*
         * FACT:
         * If a substitution does not match any value present in the configuration 
         * and is not resolved by an external source, then it is undefined. 
         * An undefined substitution with the ${foo} syntax is invalid and should generate an error.
         */
        [Fact]
        public void ThrowsWhenSubstituteIsUndefined()
        {
            var hocon = @"a{
    b = ${foo}
}";

            var ex = Record.Exception(() => HoconParser.Parse(hocon));
            Assert.NotNull(ex);
            Assert.IsType<HoconParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void TwoUndefinedQuestionMarkSubstitutionShouldNotCreateField()
        {
            var hocon = @"a{
    foo = ${?bar}${?baz}
}";
            var config = HoconParser.Parse(hocon);
            Assert.False(config.HasPath("a.foo"));
        }

        [Fact]
        public void UndefinedQuestionMarkShouldFailSilently()
        {
            var hocon = @"a {
  b = ${?a.c}
}";
            HoconParser.Parse(hocon);
        }

        /*
         * FACT:
         * foo : ${?bar} would avoid creating field foo if bar is undefined.
         * foo : ${?bar}${?baz} would also avoid creating the field if both bar and baz are undefined.
         */
        [Fact]
        public void
            UndefinedQuestionMarkSubstitutionAndResolvedQuestionMarkSubstitutionShouldResolveToTheResolvedSubtitude()
        {
            var hocon = @"
bar : { a : 42 },
foo : ${?bar}${?baz}
";

            var config = HoconParser.Parse(hocon);
            Assert.NotNull(config.GetValue("foo"));
            Assert.Equal(42, config.GetInt("foo.a"));
        }

        /*
         * FACT:
         * If a substitution with the ${?foo} syntax is undefined:
         * If it is an array element then the element should not be added.
         */
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotAddArrayElement()
        {
            var hocon = @"a{
    b = [ 1, ${?foo}, 3, 4 ]
}";
            var config = HoconParser.Parse(hocon);
            Assert.True(new[] {1, 3, 4}.SequenceEqual(config.GetIntList("a.b")));
        }

        /*
         * FACT:
         * If a substitution with the ${?foo} syntax is undefined:
         * If the field would have overridden a previously-set value for the same field, 
         * then the previous value remains.
         */
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotChangeFieldValue()
        {
            var hocon = @"a{
    b = 2
    b = ${?foo}
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(2, config.GetInt("a.b"));
        }

        /*
         * FACT:
         * If a substitution with the ${?foo} syntax is undefined:
         * If it is the value of an object field then the field should not be created.
         */
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotCreateField()
        {
            var hocon = @"a{
    b = 1
    c = ${?foo}
}";
            var config = HoconParser.Parse(hocon);
            Assert.False(config.HasPath("a.c"));
        }

        /*
         * FACT:
         * If a substitution with the ${?foo} syntax is undefined:
         * if part of a value concatenation with an object or array it should become an empty object or array.
         */
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyArray()
        {
            var hocon = @"a {
  c = ${?a.b} [4,5,6]
}";
            Assert.True(new[] {4, 5, 6}.SequenceEqual(HoconParser.Parse(hocon).GetIntList("a.c")));
        }

        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyObject()
        {
            var hocon = @"
foo : { a : 42 },
foo : ${?bar}
";

            var config = HoconParser.Parse(hocon);
            Assert.NotNull(config.GetValue("foo"));
            Assert.Equal(42, config.GetInt("foo.a"));
        }

        /*
         * FACT:
         * If a substitution with the ${?foo} syntax is undefined:
         * if it is part of a value concatenation with another string then it should become an empty string
         */
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyString()
        {
            var hocon = @"a{
    b = My name is ${?foo}
}";
            var config = HoconParser.Parse(hocon);
            Assert.Equal("My name is ", config.GetString("a.b"));
        }
    }
}