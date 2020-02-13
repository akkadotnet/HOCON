// -----------------------------------------------------------------------
// <copyright file="DuplicateKeysAndObjectMerging.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Hocon.Tests
{
    public class DuplicateKeysAndObjectMerging
    {
        public DuplicateKeysAndObjectMerging(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        /*
         * If both values are objects, then the objects are merged.
         */

        /*
         * FACT:
         * add fields present in only one of the two objects to the merged object.
         */
        [Fact]
        public void CanMergeObject_DifferentFields()
        {
            var hocon = @"
foo : { a : 42 },
foo : { b : 43 }
";

            var config = HoconParser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
        }

        /*
         * FACT:
         * for object-valued fields present in both objects, the object values should be recursively merged according to these same rules.
         */
        [Fact]
        public void CanMergeObject_RecursiveMerging()
        {
            var hocon = @"
foo
{
  bar {
    a : 42
    b : 43 
  }
},
foo
{ 
  bar
  {
    b : 44
    c : 45
    baz
    {
      a : 9000
    }
  }
}
";

            var config = HoconParser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo.bar.a"));
            Assert.Equal(44, config.GetInt("foo.bar.b"));
            Assert.Equal(45, config.GetInt("foo.bar.c"));
            Assert.Equal(9000, config.GetInt("foo.bar.baz.a"));
        }

        /*
         * FACT:
         * for non-object-valued fields present in both objects, the field found in the second object must be used.
         */
        [Fact]
        public void CanMergeObject_SameField()
        {
            var hocon = @"
foo : { a : 42 },
foo : { a : 43 }
";

            var config = HoconParser.Parse(hocon);
            Assert.Equal(43, config.GetInt("foo.a"));
        }

        [Fact]
        public void CanMixObjectMergeAndSubstitutions_Issue92()
        {
            var hocon =
                @"x={ q : 10 }
y=5

a=1
a.q.r.s=${b}
a=${y}
a=${x}
a={ c : 3 }

b=${x}
b=${y}

// nesting ConfigDelayed inside another one
c=${x}
c={ d : 600, e : ${a}, f : ${b} }";

            var config = HoconParser.Parse(hocon);
            _output.WriteLine(config.PrettyPrint(2));
            Assert.Equal(3, config.GetInt("a.c"));
            Assert.Equal(10, config.GetInt("a.q"));
            Assert.Equal(5, config.GetInt("b"));
            Assert.Equal(600, config.GetInt("c.d"));
            Assert.Equal(3, config.GetInt("c.e.c"));
            Assert.Equal(10, config.GetInt("c.e.q"));
            Assert.Equal(5, config.GetInt("c.f"));
            Assert.Equal(10, config.GetInt("c.q"));
            Assert.Equal(10, config.GetInt("x.q"));
            Assert.Equal(5, config.GetInt("y"));
        }

        /*
         * FACT:
         * duplicate keys that appear later override those that appear earlier, unless both values are objects
         */
        [Fact]
        public void CanOverrideLiteral()
        {
            var hocon = @"
foo : literal
foo : 42
";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo"));
        }

        /*
         * FACT:
         * Assigning an object field to a literal and then to another object would prevent merging
         */
        [Fact]
        public void CanOverrideObject()
        {
            var hocon = @"
{
    foo : { a : 42 },
    foo : null,
    foo : { b : 43 }
}";
            var config = HoconParser.Parse(hocon);
            Assert.False(config.HasPath("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
        }

        /// <summary>
        ///     The `a.b.c` dot syntax may be used to add new fields or override old fields of a HOCON object.
        /// </summary>
        [Fact]
        public void ObjectCanMixBraceAndDotSyntax()
        {
            var hocon = @"
    foo { x = 1 }
    foo { y = 2 }
    foo.z = 32
";
            var config = HoconParser.Parse(hocon);
            Assert.Equal(1, config.GetInt("foo.x"));
            Assert.Equal(2, config.GetInt("foo.y"));
            Assert.Equal(32, config.GetInt("foo.z"));
        }

        // Fix for https://github.com/akkadotnet/HOCON/issues/137
        [Fact]
        public void ObjectsWithArraysShouldMergeCorrectly()
        {
            var hocon = @"
c: { 
    a: [2, 5] 
    a: [6] 
}
";
            var hocon2 = @"
c: { 
    a: [2, 5] [6] 
}
";
            var config = HoconParser.Parse(hocon);
            config.GetIntList("c.a").Should().Equal(6);
            config = HoconParser.Parse(hocon2);
            config.GetIntList("c.a").Should().Equal(2, 5, 6);
        }
    }
}