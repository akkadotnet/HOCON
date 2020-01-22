// -----------------------------------------------------------------------
// <copyright file="ArrayTest.cs" company="Akka.NET Project">
//      Copyright (C) 2013 - 2020 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class ArrayTest
    {
        [Fact]
        public void ArrayMerge()
        {
            var hocon1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var hocon2 = @"{
                'ip': {
                    '3': '15.16.17.18'
                }
            }";

            var hoconConfigSource1 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon1)};
            var hoconConfigSource2 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon2)};

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource1);
            configurationBuilder.Add(hoconConfigSource2);
            var config = configurationBuilder.Build();

            Assert.Equal(4, config.GetSection("ip").GetChildren().Count());
            Assert.Equal("1.2.3.4", config["ip:0"]);
            Assert.Equal("7.8.9.10", config["ip:1"]);
            Assert.Equal("11.12.13.14", config["ip:2"]);
            Assert.Equal("15.16.17.18", config["ip:3"]);
        }

        [Fact]
        public void ArrayOfObjects()
        {
            var hocon = @"{
                'ip': [
                    {
                        'address': '1.2.3.4',
                        'hidden': false
                    },
                    {
                        'address': '5.6.7.8',
                        'hidden': true
                    }
                ]
            }";

            var hoconConfigSource = new HoconConfigurationProvider(new HoconConfigurationSource());
            hoconConfigSource.Load(TestStreamHelpers.StringToStream(hocon));

            Assert.Equal("1.2.3.4", hoconConfigSource.Get("ip:0:address"));
            Assert.Equal("false", hoconConfigSource.Get("ip:0:hidden"));
            Assert.Equal("5.6.7.8", hoconConfigSource.Get("ip:1:address"));
            Assert.Equal("true", hoconConfigSource.Get("ip:1:hidden"));
        }

        [Fact]
        public void ArraysAreConvertedToKeyValuePairs()
        {
            var hocon = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var hoconConfigSource = new HoconConfigurationProvider(new HoconConfigurationSource());
            hoconConfigSource.Load(TestStreamHelpers.StringToStream(hocon));

            Assert.Equal("1.2.3.4", hoconConfigSource.Get("ip:0"));
            Assert.Equal("7.8.9.10", hoconConfigSource.Get("ip:1"));
            Assert.Equal("11.12.13.14", hoconConfigSource.Get("ip:2"));
        }

        [Fact]
        public void ArraysAreKeptInFileOrder()
        {
            var hocon = @"{
                'setting': [
                    'b',
                    'a',
                    '2'
                ]
            }";

            var hoconConfigSource = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon)};

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource);
            var config = configurationBuilder.Build();

            var configurationSection = config.GetSection("setting");
            var indexConfigurationSections = configurationSection.GetChildren().ToArray();

            Assert.Equal(3, indexConfigurationSections.Count());
            Assert.Equal("b", indexConfigurationSections[0].Value);
            Assert.Equal("a", indexConfigurationSections[1].Value);
            Assert.Equal("2", indexConfigurationSections[2].Value);
        }

        [Fact]
        public void ExplicitArrayReplacement()
        {
            var hocon1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var hocon2 = @"{
                'ip': {
                    '1': '15.16.17.18'
                }
            }";

            var hoconConfigSource1 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon1)};
            var hoconConfigSource2 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon2)};

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource1);
            configurationBuilder.Add(hoconConfigSource2);
            var config = configurationBuilder.Build();

            Assert.Equal(3, config.GetSection("ip").GetChildren().Count());
            Assert.Equal("1.2.3.4", config["ip:0"]);
            Assert.Equal("15.16.17.18", config["ip:1"]);
            Assert.Equal("11.12.13.14", config["ip:2"]);
        }

        [Fact]
        public void ImplicitArrayItemReplacement()
        {
            var hocon1 = @"{
                'ip': [
                    '1.2.3.4',
                    '7.8.9.10',
                    '11.12.13.14'
                ]
            }";

            var hocon2 = @"{
                'ip': [
                    '15.16.17.18'
                ]
            }";

            var hoconConfigSource1 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon1)};
            var hoconConfigSource2 = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon2)};

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource1);
            configurationBuilder.Add(hoconConfigSource2);
            var config = configurationBuilder.Build();

            Assert.Equal(3, config.GetSection("ip").GetChildren().Count());
            Assert.Equal("15.16.17.18", config["ip:0"]);
            Assert.Equal("7.8.9.10", config["ip:1"]);
            Assert.Equal("11.12.13.14", config["ip:2"]);
        }

        [Fact]
        public void NestedArrays()
        {
            var hocon = @"{
                'ip': [
                    [ 
                        '1.2.3.4',
                        '5.6.7.8'
                    ],
                    [ 
                        '9.10.11.12',
                        '13.14.15.16'
                    ],
                ]
            }";

            var hoconConfigSource = new HoconConfigurationProvider(new HoconConfigurationSource());
            hoconConfigSource.Load(TestStreamHelpers.StringToStream(hocon));

            Assert.Equal("1.2.3.4", hoconConfigSource.Get("ip:0:0"));
            Assert.Equal("5.6.7.8", hoconConfigSource.Get("ip:0:1"));
            Assert.Equal("9.10.11.12", hoconConfigSource.Get("ip:1:0"));
            Assert.Equal("13.14.15.16", hoconConfigSource.Get("ip:1:1"));
        }

        [Fact]
        public void PropertiesAreSortedByNumberOnlyFirst()
        {
            var hocon = @"{
                'setting': {
                    'hello': 'a',
                    'bob': 'b',
                    '42': 'c',
                    '4':'d',
                    '10': 'e',
                    '1text': 'f',
                }
            }";

            var hoconConfigSource = new HoconConfigurationSource
                {FileProvider = TestStreamHelpers.StringToFileProvider(hocon)};

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.Add(hoconConfigSource);
            var config = configurationBuilder.Build();

            var configurationSection = config.GetSection("setting");
            var indexConfigurationSections = configurationSection.GetChildren().ToArray();

            Assert.Equal(6, indexConfigurationSections.Count());
            Assert.Equal("4", indexConfigurationSections[0].Key);
            Assert.Equal("10", indexConfigurationSections[1].Key);
            Assert.Equal("42", indexConfigurationSections[2].Key);
            Assert.Equal("1text", indexConfigurationSections[3].Key);
            Assert.Equal("bob", indexConfigurationSections[4].Key);
            Assert.Equal("hello", indexConfigurationSections[5].Key);
        }
    }
}