//-----------------------------------------------------------------------
// <copyright file="HoconConfigurationTest.cs" company="Hocon Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hocon.Extensions.Configuration.Tests
{
    public class HoconConfigurationTest
    {
        private HoconConfigurationProvider LoadProvider(string hocon)
        {
            var p = new HoconConfigurationProvider(new HoconConfigurationSource { Optional = true });
            p.Load(TestStreamHelpers.StringToStream(hocon));
            return p;
        }

        [Fact]
        public void LoadKeyValuePairsFromValidHocon()
        {
            var hocon = @"
{
    'firstname': 'test',
    'test.last.name': 'last.name',
        'residential.address': {
            'street.name': 'Something street',
            'zipcode': '12345'
        }
}";
            var hoconConfigSrc = LoadProvider(hocon);

            Assert.Equal("test", hoconConfigSrc.Get("firstname"));
            Assert.Equal("last.name", hoconConfigSrc.Get("test.last.name"));
            Assert.Equal("Something street", hoconConfigSrc.Get("residential.address:STREET.name"));
            Assert.Equal("12345", hoconConfigSrc.Get("residential.address:zipcode"));
        }

        [Fact]
        public void LoadMethodCanHandleEmptyValue()
        {
            var hocon = @"
{
    'name': ''
}";
            var hoconConfigSrc = LoadProvider(hocon);
            Assert.Equal(string.Empty, hoconConfigSrc.Get("name"));
        }

        [Fact]
        public void LoadWithCulture()
        {
            var previousCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                var hocon = @"
{
    'number': 3.14
}";
                var hoconConfigSrc = LoadProvider(hocon);
                Assert.Equal("3.14", hoconConfigSrc.Get("number"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public void NonObjectRootIsInvalid()
        {
            var hocon = @"'test'";

            var exception = Assert.Throws<FormatException>(
                () => LoadProvider(hocon));

            Assert.NotNull(exception.Message);
        }

        [Fact]
        public void SupportAndIgnoreComments()
        {
            var hocon = @" // Comments
                {// Comments
                ""name"": ""test"", // Comments
                ""address"": {
                    ""street"": ""Something street"", // Comments
                    ""zipcode"": ""12345""
                }
            }";
            var hoconConfigSrc = LoadProvider(hocon);
            Assert.Equal("test", hoconConfigSrc.Get("name"));
            Assert.Equal("Something street", hoconConfigSrc.Get("address:street"));
            Assert.Equal("12345", hoconConfigSrc.Get("address:zipcode"));
        }

        [Fact]
        public void ThrowExceptionWhenUnexpectedEndFoundBeforeFinishParsing()
        {
            var hocon = @"{
                'name': 'test',
                'address': {
                    'street': 'Something street',
                    'zipcode': '12345'
                }
            // Missing a right brace here";
            var exception = Assert.Throws<FormatException>(() => LoadProvider(hocon));
            Assert.NotNull(exception.Message);
        }

        [Fact]
        public void ThrowExceptionWhenMissingCurlyBeforeFinishParsing()
        {
            var hocon = @"
            {
              'Data': {
            ";

            var exception = Assert.Throws<FormatException>(() => LoadProvider(hocon));
            Assert.Contains("Could not parse the HOCON file.", exception.Message);
        }

        [Fact]
        public void ThrowExceptionWhenPassingNullAsFilePath()
        {
            var expectedMsg = new ArgumentException(Resources.Error_InvalidFilePath, "path").Message;

            var exception = Assert.Throws<ArgumentException>(() => new ConfigurationBuilder().AddHoconFile(path: null));

            Assert.Equal(expectedMsg, exception.Message);
        }

        [Fact]
        public void ThrowExceptionWhenPassingEmptyStringAsFilePath()
        {
            var expectedMsg = new ArgumentException(Resources.Error_InvalidFilePath, "path").Message;

            var exception = Assert.Throws<ArgumentException>(() => new ConfigurationBuilder().AddHoconFile(string.Empty));

            Assert.Equal(expectedMsg, exception.Message);
        }

        [Fact]
        public void HoconConfiguration_Throws_On_Missing_Configuration_File()
        {
            var config = new ConfigurationBuilder().AddHoconFile("NotExistingConfig.hocon", optional: false);
            var exception = Assert.Throws<FileNotFoundException>(() => config.Build());

            // Assert
            Assert.StartsWith("The configuration file 'NotExistingConfig.hocon' was not found and is not optional. The physical path is '", exception.Message);
        }

        [Fact]
        public void HoconConfiguration_Does_Not_Throw_On_Optional_Configuration()
        {
            var config = new ConfigurationBuilder().AddHoconFile("NotExistingConfig.hocon", optional: true).Build();
        }

        [Fact]
        public void ThrowFormatExceptionWhenFileIsEmpty()
        {
            var exception = Assert.Throws<FormatException>(() => LoadProvider(@""));
        }
    }
}
