//-----------------------------------------------------------------------
// <copyright file="DuplicateKeysAndObjectMerging.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hocon.Tests
{
    public class DuplicateKeysAndObjectMerging
    {
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
            var config = Parser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo"));
        }

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

            var config = Parser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
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

            var config = Parser.Parse(hocon);
            Assert.Equal(43, config.GetInt("foo.a"));
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

            var config = Parser.Parse(hocon);
            Assert.Equal(42, config.GetInt("foo.bar.a"));
            Assert.Equal(44, config.GetInt("foo.bar.b"));
            Assert.Equal(45, config.GetInt("foo.bar.c"));
            Assert.Equal(9000, config.GetInt("foo.bar.baz.a"));
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
            var config = Parser.Parse(hocon);
            Assert.False(config.HasPath("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
        }

        /// <summary>
        /// The `a.b.c` dot syntax may be used to add new fields or override old fields of a HOCON object.
        /// </summary>
        [Fact]
        public void ObjectCanMixBraceAndDotSyntax()
        {
            var hocon = @"
    foo { x = 1 }
    foo { y = 2 }
    foo.z = 32
";
            var config = Parser.Parse(hocon);
            Assert.Equal(1, config.GetInt("foo.x"));
            Assert.Equal(2, config.GetInt("foo.y"));
            Assert.Equal(32, config.GetInt("foo.z"));
        }
    }
}
