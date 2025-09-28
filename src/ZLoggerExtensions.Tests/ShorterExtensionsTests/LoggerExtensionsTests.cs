using Deneblab.ZLoggerExtensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ZLogger;

namespace Deneblab.ZLoggerExtensionsTests.ShorterExtensionsTests;

public class LoggerExtensionsTests
{
    [Fact]
    public void Trace_GenericLogger_CallsZLogTrace()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Trace($"Test trace message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Debug_GenericLogger_CallsZLogDebug()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Debug($"Test debug message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Info_GenericLogger_CallsZLogInformation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Info($"Test info message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Warn_GenericLogger_CallsZLogWarning()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Warn($"Test warn message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Error_GenericLogger_CallsZLogError()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Error($"Test error message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Error_GenericLogger_WithException_CallsZLogError()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();
        var testException = new InvalidOperationException("Test exception");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Error(testException, $"Test error with exception"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Critical_GenericLogger_CallsZLogCritical()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Critical($"Test critical message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Critical_GenericLogger_WithException_CallsZLogCritical()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();
        var testException = new InvalidOperationException("Test exception");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Critical(testException, $"Test critical with exception"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Trace_NonGenericLogger_CallsZLogTrace()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Trace($"Test trace message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Debug_NonGenericLogger_CallsZLogDebug()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Debug($"Test debug message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Info_NonGenericLogger_CallsZLogInformation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Info($"Test info message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Warn_NonGenericLogger_CallsZLogWarning()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Warn($"Test warn message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Error_NonGenericLogger_CallsZLogError()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Error($"Test error message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Error_NonGenericLogger_WithException_CallsZLogError()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");
        var testException = new InvalidOperationException("Test exception");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Error(testException, $"Test error with exception"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Critical_NonGenericLogger_CallsZLogCritical()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Critical($"Test critical message"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void Critical_NonGenericLogger_WithException_CallsZLogCritical()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestLogger");
        var testException = new InvalidOperationException("Test exception");

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() => { logger.Critical(testException, $"Test critical with exception"); });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void AllMethods_WorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsole());
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerExtensionsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() =>
        {
            logger.Trace($"Trace message");
            logger.Debug($"Debug message");
            logger.Info($"Info message");
            logger.Warn($"Warn message");
            logger.Error($"Error message");
            logger.Critical($"Critical message");
        });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }
}