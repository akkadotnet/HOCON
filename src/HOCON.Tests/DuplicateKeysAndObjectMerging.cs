﻿using System;
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
         * duplicate keys that appear later override those that appear earlier, unless both values are objects
         */
        [Fact]
        public void CanOverrideLiteral()
        {
            var hocon = @"
foo : literal
foo : 42
";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.Equal(42, config.GetInt("foo"));
        }

        /*
         * If both values are objects, then the objects are merged.
         */

        // add fields present in only one of the two objects to the merged object.
        [Fact]
        public void CanMergeObject_DifferentFields()
        {
            var hocon = @"
foo : { a : 42 },
foo : { b : 43 }
";

            var config = ConfigurationFactory.ParseString(hocon);
            Assert.Equal(42, config.GetInt("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
        }

        // for non-object-valued fields present in both objects, the field found in the second object must be used.
        [Fact]
        public void CanMergeObject_SameField()
        {
            var hocon = @"
foo : { a : 42 },
foo : { a : 43 }
";

            var config = ConfigurationFactory.ParseString(hocon);
            Assert.Equal(43, config.GetInt("foo.a"));
        }

        // for object-valued fields present in both objects, the object values should be recursively merged according to these same rules.
        [Fact]
        public void CanMergeObject_RecursiveMerging()
        {
            var hocon = @"
foo
{
  bar 
  {
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

            var config = ConfigurationFactory.ParseString(hocon);
            Assert.Equal(42, config.GetInt("foo.bar.a"));
            Assert.Equal(44, config.GetInt("foo.bar.b"));
            Assert.Equal(45, config.GetInt("foo.bar.c"));
            Assert.Equal(9000, config.GetInt("foo.bar.baz.a"));
        }

        // Assigning an object field to a literal and then to another object would prevent merging
        [Fact]
        public void CanOverrideObject()
        {
            var hocon = @"
{
    foo : { a : 42 },
    foo : null,
    foo : { b : 43 }
}";
            var config = ConfigurationFactory.ParseString(hocon);
            Assert.False(config.HasPath("foo.a"));
            Assert.Equal(43, config.GetInt("foo.b"));
        }

    }
}
