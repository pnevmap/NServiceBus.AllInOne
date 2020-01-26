using NUnit.Framework;
using Shared.Hosting.Abstractions.Logging;
using System;
using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Serilog;
using Serilog.Configuration;

namespace Shared.Hosting.Abstractions.Tests.Logging
{
    [TestFixture]
    public class LoggingPolicyTests
    {
        [TestCase("DefaultLoggingPolicy", "")]
        [TestCase("", @"C:\Logs")]
        [TestCase("DefaultLoggingPolicy", @"C:\Logs")]

        public void IsDefined_When_Well_Defined(string section, string path)
        {
            // Arrange
            var loggingPolicy = new LoggingPolicy { Section = section, Path = path };

            // Act
            // Assert
            loggingPolicy.IsDefined().Should().BeTrue();
        }
        [TestCase(null)]
        [TestCase("")]
        public void IsDefined_When_Misdefined(string path)
        {
            // Arrange
            var loggingPolicy = new LoggingPolicy { Path = path };

            // Act
            // Assert
            loggingPolicy.IsDefined().Should().BeFalse();
        }
    }
}
