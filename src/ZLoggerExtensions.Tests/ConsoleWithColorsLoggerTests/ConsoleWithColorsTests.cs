using Deneblab.ZLoggerExtensions.ConsoleWithColorsLogger;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ZLogger;
using ZLogger.Providers;

namespace Deneblab.ZLoggerExtensionsTests.ConsoleWithColorsLoggerTests;

public class ConsoleWithColorsTests
{
    [Fact]
    public void AddZLoggerConsoleWithColors_WithoutConfiguration_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging(builder => builder.AddZLoggerConsoleWithColors());
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is ZLoggerConsoleLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void AddZLoggerConsoleWithColors_WithActionConfiguration_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging(builder =>
        {
            builder.AddZLoggerConsoleWithColors(options =>
            {
                options.LogVerbosity = LogVerbosity.DataTimeUtcLogLevelCategory;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Assert
        logger.Should().NotBeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddZLoggerConsoleWithColors_WithOptionsConfiguration_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new AddZLoggerConsoleWithColorsOptions
        {
            LogVerbosity = LogVerbosity.LogLevelLineColor
        };

        // Act
        services.AddLogging(builder => builder.AddZLoggerConsoleWithColors(options));
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Assert
        logger.Should().NotBeNull();
        serviceProvider.Dispose();
    }

    [Theory]
    [InlineData(LogVerbosity.TimeOnlyLocalLogLevel)]
    [InlineData(LogVerbosity.DataTimeUtcLogLevelCategory)]
    [InlineData(LogVerbosity.LogLevelLineColor)]
    public void AddZLoggerConsoleWithColors_WithDifferentVerbosityLevels_ConfiguresCorrectly(LogVerbosity verbosity)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging(builder =>
        {
            builder.AddZLoggerConsoleWithColors(options =>
            {
                options.LogVerbosity = verbosity;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Assert
        logger.Should().NotBeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void LogVerbosity_CanLogWithoutErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddZLoggerConsoleWithColors(options =>
            {
                options.LogVerbosity = LogVerbosity.TimeOnlyLocalLogLevel;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Act & Assert - Should not throw exceptions
        var exception = Record.Exception(() =>
        {
            logger.LogTrace("Test trace message");
            logger.LogDebug("Test debug message");
            logger.LogInformation("Test information message");
            logger.LogWarning("Test warning message");
            logger.LogError("Test error message");
            logger.LogCritical("Test critical message");
            Thread.Sleep(100); // Allow async operations to complete
        });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void ZLoggerWithColorsProvider_CanBeCreatedDirectly()
    {
        // Arrange
        var options = new ZLoggerWithColorsProviderOptions
        {
            LogVerbosity = LogVerbosity.TimeOnlyLocalLogLevel,
            OutputEncodingToUtf8 = true,
            ConfigureEnableAnsiEscapeCode = true
        };

        // Act
        using var provider = new ZLoggerWithColorsProvider(options);
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        logger.Should().NotBeNull();
        provider.Should().BeOfType<ZLoggerWithColorsProvider>();
    }

    [Fact]
    public void ZLoggerWithColorsProviderOptions_DefaultValues_AreSetCorrectly()
    {
        // Act
        var options = new ZLoggerWithColorsProviderOptions();

        // Assert
        options.OutputEncodingToUtf8.Should().BeTrue();
        options.ConfigureEnableAnsiEscapeCode.Should().BeFalse();
        options.LogToStandardErrorThreshold.Should().Be(LogLevel.None);
        options.LogVerbosity.Should().Be(LogVerbosity.TimeOnlyLocalLogLevel);
    }

    [Theory]
    [InlineData(LogVerbosity.TimeOnlyLocalLogLevel)]
    [InlineData(LogVerbosity.DataTimeUtcLogLevelCategory)]
    [InlineData(LogVerbosity.LogLevelLineColor)]
    public void ZLoggerWithColorsProviderOptions_WithDifferentVerbosity_ConfiguresCorrectly(LogVerbosity verbosity)
    {
        // Act
        var options = new ZLoggerWithColorsProviderOptions
        {
            LogVerbosity = verbosity
        };

        // Assert
        options.LogVerbosity.Should().Be(verbosity);
    }


    [Fact]
    public void AddZLoggerConsoleWithColors_IntegratesWithOtherLogProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Add multiple logging providers
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddZLoggerConsoleWithColors();
            builder.AddDebug();
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Assert - Should not throw exceptions
        var exception = Record.Exception(() =>
        {
            logger.LogInformation("Integration test message");
            Thread.Sleep(100);
        });

        exception.Should().BeNull();

        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>().ToList();
        loggerProviders.Should().HaveCountGreaterThan(1);
        loggerProviders.Should().ContainSingle(p => p is ZLoggerConsoleLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void AddZLoggerConsoleWithColors_CanCreateMultipleLoggers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddZLoggerConsoleWithColors());
        var serviceProvider = services.BuildServiceProvider();

        // Act - Create multiple loggers
        var logger1 = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();
        var logger2 = serviceProvider.GetRequiredService<ILogger<string>>();
        var logger3 = serviceProvider.GetRequiredService<ILogger<int>>();

        // Assert - Should not throw exceptions when logging
        var exception = Record.Exception(() =>
        {
            logger1.LogInformation("Message from logger 1");
            logger2.LogWarning("Message from logger 2");
            logger3.LogError("Message from logger 3");
            Thread.Sleep(100);
        });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void DefaultOptions_LogVerbosity_IsTimeOnlyLocalLogLevel()
    {
        // Act
        var options = new AddZLoggerConsoleWithColorsOptions();

        // Assert
        options.LogVerbosity.Should().Be(LogVerbosity.TimeOnlyLocalLogLevel);
    }

    [Fact]
    public void LogVerbosity_EnumValues_AreValid()
    {
        // Act & Assert - All enum values should be valid
        var timeOnly = LogVerbosity.TimeOnlyLocalLogLevel;
        var dateTimeUtc = LogVerbosity.DataTimeUtcLogLevelCategory;
        var lineColor = LogVerbosity.LogLevelLineColor;

        // Ensure enum values are as expected
        ((int)timeOnly).Should().Be(0);
        ((int)dateTimeUtc).Should().Be(1);
        ((int)lineColor).Should().Be(2);
    }

    [Theory]
    [InlineData(LogVerbosity.TimeOnlyLocalLogLevel)]
    [InlineData(LogVerbosity.DataTimeUtcLogLevelCategory)]
    [InlineData(LogVerbosity.LogLevelLineColor)]
    public void AllVerbosityLevels_CanLogAllLogLevels_WithoutExceptions(LogVerbosity verbosity)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddZLoggerConsoleWithColors(options =>
            {
                options.LogVerbosity = verbosity;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ConsoleWithColorsTests>>();

        // Act & Assert - Should not throw exceptions for any log level
        var exception = Record.Exception(() =>
        {
            logger.LogTrace("Trace message with verbosity {Verbosity}", verbosity);
            logger.LogDebug("Debug message with verbosity {Verbosity}", verbosity);
            logger.LogInformation("Information message with verbosity {Verbosity}", verbosity);
            logger.LogWarning("Warning message with verbosity {Verbosity}", verbosity);
            logger.LogError("Error message with verbosity {Verbosity}", verbosity);
            logger.LogCritical("Critical message with verbosity {Verbosity}", verbosity);
            Thread.Sleep(100);
        });

        exception.Should().BeNull();
        serviceProvider.Dispose();
    }
}