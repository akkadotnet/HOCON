using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Hocon.Configuration.Tests
{
    public class ConcurrentImmutablitySpec
    {

        private static Config fb2 = HoconConfigurationFactory.ParseString(@"akka.actor{
                dispatchers{
                    foo.test = bar
                    foo2.test = baz
                }
            }");

        private static Config fb1 = HoconConfigurationFactory.ParseString(@"akka.actor{
                deployment{
                    /foo{
                        dispatcher = akka.actor.dispatchers.foo
                    }
                }
                dispatchers{
                    foo2.test = biz
                }
            }");

        private static Config fb3 = HoconConfigurationFactory.ParseString(@"akka.actor{
            deployment{
                /harmless{
                    throughput = 12
                }
            }
            dispatchers{
                foo3.test = boz
            }
        }");

        private Config C { get; set; } = fb1;

        [Fact]
        public void ConcurrentConfig_modification_should_be_immutable()
        {
            C = C.SafeWithFallback(fb2);

            var r = Parallel.ForEach(Enumerable.Range(1, 1000), i =>
            {
                /*
                 * Modify the Config that is being concurrently read with a fallback
                 * that doesn't modify any of the properties actually being read.
                 *
                 * This operation should not throw - every other thread should be reading its
                 * own immutable copy of the object.
                 */
                if (i % 2 == 0)
                {
                    C = C.SafeWithFallback(fb3);
                }

                var actorConfig = C.GetConfig("akka.actor.deployment./foo");
                var dispatcherName = actorConfig.GetString("dispatcher");
                var dispatcherConfig = C.GetConfig(dispatcherName);
                dispatcherConfig.GetString("test").Should().Be("bar");
            });
        }
    }
}
