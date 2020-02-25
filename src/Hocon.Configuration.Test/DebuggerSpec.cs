// -----------------------------------------------------------------------
// <copyright file="DebuggerSpec.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace Hocon.Configuration.Tests
{
    public class DebuggerSpec
    {
        [Fact]
        public void Should_dump_stand_alone_HOCON()
        {
            var myHocon = HoconConfigurationFactory.ParseString(@"akka{
                actor.provider = cluster
                deployment{
                    /foo {
                        dispatcher = bar
                    }
                }
            }");

            var dump = myHocon.DumpConfig();
            dump.Should().Contain(myHocon.PrettyPrint(2));
        }

        [Fact]
        public void Should_dump_HOCON_with_1_Fallback()
        {
            var myHocon1 = HoconConfigurationFactory.ParseString(@"akka{
                actor.provider = cluster
                deployment{
                    /foo {
                        dispatcher = bar
                    }
                }
            }");

            var myHocon2 = HoconConfigurationFactory.ParseString(@"akka{
                debug{
                    received  = on
                }
                actor.provider = cluster
                deployment{
                    /foo {
                        dispatcher = foo
                    }
                    /bar {
                        mailbox = ""myFoo""
                    }
                }
            }");

            var fullHocon = myHocon1.WithFallback(myHocon2);

            var dump = fullHocon.DumpConfig();
            dump.Should().Contain(myHocon1.PrettyPrint(2));
            dump.Should().Contain(myHocon2.PrettyPrint(2));
        }

        [Fact]
        public void Should_dump_HOCON_with_2_Fallback()
        {
            var myHocon1 = HoconConfigurationFactory.ParseString(@"akka{
                actor.provider = cluster
                deployment{
                    /foo {
                        dispatcher = bar
                    }
                }
            }");

            var myHocon2 = HoconConfigurationFactory.ParseString(@"akka{
                debug{
                    received  = on
                }
                actor.provider = cluster
                deployment{
                    /foo {
                        dispatcher = foo
                    }
                    /bar {
                        mailbox = ""myFoo""
                    }
                }
            }");

            var myHocon3 = HoconConfigurationFactory.ParseString(@"akka{
                remote{
                    transport = ""foo""
                }
            }");

            var fullHocon = myHocon1
                .SafeWithFallback(myHocon2)
                .SafeWithFallback(myHocon3);

            fullHocon.GetString("akka.remote.transport").Should().Be("foo");

            var dump = fullHocon.DumpConfig();
            dump.Should().Contain(myHocon1.PrettyPrint(2));
            dump.Should().Contain(myHocon2.PrettyPrint(2));
            dump.Should().Contain(myHocon3.PrettyPrint(2));
        }
    }
}
