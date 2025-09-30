using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using LoggingBuilderExtensions = Deneblab.ZLoggerExtensions.AdvanceFileLogger.LoggingBuilderExtensions;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class LoggingBuilderExtensionsTests : IDisposable
{
    private readonly string _testDirectory;

    public LoggingBuilderExtensionsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "LoggingBuilderExtensionsTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void AddAdvanceFileLogger_WithoutConfiguration_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging(builder => builder.AddAdvanceFileLogger());
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void AddAdvanceFileLogger_WithConfiguration_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.LogDirPath = _testDirectory;
                options.AppName = "configured";
                options.MaxBytes = 2048;
                options.MaxArchivedFiles = 10;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        // Assert
        logger.Should().NotBeNull();
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddAdvanceFileLogger_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            LoggingBuilderExtensions.AddAdvanceFileLogger(null!));
    }

    [Fact]
    public void AddAdvanceFileLogger_RegistersProviderOnlyOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Add the provider multiple times
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger();
            builder.AddAdvanceFileLogger();
            builder.AddAdvanceFileLogger();
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should only have one instance
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>()
            .Where(p => p is AdvanceFileLoggerProvider)
            .ToList();

        loggerProviders.Should().HaveCount(1);
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddAdvanceFileLogger_CanCreateMultipleLoggers()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddAdvanceFileLogger(options =>
        {
            options.LogDirPath = _testDirectory;
            options.AppName = "multilogger";
        }));
        var serviceProvider = services.BuildServiceProvider();

        // Act - Create multiple loggers
        var logger1 = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();
        var logger2 = serviceProvider.GetRequiredService<ILogger<string>>();
        var logger3 = serviceProvider.GetRequiredService<ILogger<int>>();

        logger1.LogInformation("Message from logger 1");
        logger2.LogWarning("Message from logger 2");
        logger3.LogError("Message from logger 3");

        Thread.Sleep(100);

        // Assert
        serviceProvider.Dispose();
        // With new API, files use date+index pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"multilogger.{today:yyyy-MM-dd}.00.log");
        File.Exists(expectedFile).Should().BeTrue();

        var content = File.ReadAllText(expectedFile);
        content.Should().Contain("Message from logger 1");
        content.Should().Contain("Message from logger 2");
        content.Should().Contain("Message from logger 3");
    }

    [Fact]
    public void ServiceCollectionExtensions_AddAdvanceFileLogger_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLogging();
        services.AddAdvanceFileLogger();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void ServiceCollectionExtensions_AddAdvanceFileLogger_WithConfiguration_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAdvanceFileLogger(options =>
        {
            options.LogDirPath = _testDirectory;
            options.AppName = "servicecollection";
            options.MaxBytes = 4096;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void ServiceCollectionExtensions_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddAdvanceFileLogger(null!));
    }

    [Fact]
    public void AddAdvanceFileLogger_IntegratesWithOtherLogProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Add multiple logging providers
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddAdvanceFileLogger(options =>
            {
                options.LogDirPath = _testDirectory;
                options.AppName = "integration";
            });
            builder.AddDebug();
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        logger.LogInformation("Integration test message");
        Thread.Sleep(100);

        // Assert
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>().ToList();
        loggerProviders.Should().HaveCountGreaterThan(1);
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
        // With new API, files use date+index pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"integration.{today:yyyy-MM-dd}.00.log");
        File.Exists(expectedFile).Should().BeTrue();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}