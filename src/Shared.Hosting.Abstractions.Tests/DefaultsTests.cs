using NSubstitute;
using NUnit.Framework;
using Shared.Hosting.Abstractions;
using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Shared.Hosting.Abstractions.Tests.Logging;

namespace Shared.Hosting.Abstractions.Tests
{
    [TestFixture]
    public class DefaultsTests
    {
        [Test]
        public void Read_Defaults()
        {
            // Arrange
            string[] @args = new
            {
                Defaults = new
                {
                    DbSchema = "TestSchema",
                    LoggingPolicy = new
                    {
                        Section = "section",
                        OutputTemplate = "output-template",
                        Path = "mylog.log",
                        MinimumLevel = new
                        {
                            Default = "Verbose",
                        },
                    }
                },
                ConnectionStrings = new
                {
                    Db = "connectionString",
                }
            }
            .ToConfigurationArgs();
            var configuration = new ConfigurationBuilder().AddCommandLine(@args).Build();

            // Act
            var defaults = Defaults.Read(configuration);

            // Assert
            defaults.DbConnectionString.Should().Be("Db");
            defaults.DbSchema.Should().Be("TestSchema");
            defaults.LoggingPolicy.Path.Should().Be("mylog.log");
            defaults.LoggingPolicy.MinimumLevel.Default.Should().Be(LogEventLevel.Verbose);
            defaults.LoggingPolicy.OutputTemplate.Should().Be("output-template");
            defaults.LoggingPolicy.Section.Should().Be("section");
        }

        [Test]
        public void Read_When_Section_Is_Null()
        {
            // Arrange
            string[] @args = new
                {
                    ConnectionStrings = new
                    {
                        Db = "connectionString",
                    }
                }
                .ToConfigurationArgs();
            var configuration = new ConfigurationBuilder().AddCommandLine(@args).Build();

            // Act
            var defaults = Defaults.Read(configuration);

            // Assert
            defaults.Should().BeEquivalentTo(new Defaults());
        }
    }
}
