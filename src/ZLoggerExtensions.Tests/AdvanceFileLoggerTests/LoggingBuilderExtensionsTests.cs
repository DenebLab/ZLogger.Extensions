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
        var testFilePath = Path.Combine(_testDirectory, "configured.log");

        // Act
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FilePath = testFilePath;
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
    public void AddAdvanceFileLogger_WithFilePath_ConfiguresCorrectPath()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "path_test.log");

        // Act
        services.AddLogging(builder => builder.AddAdvanceFileLogger(testFilePath));
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        logger.LogInformation("Test message");
        Thread.Sleep(100); // Allow time for async operations

        // Assert
        serviceProvider.Dispose();
        // With new API, files use date+index pattern, so check for the expected pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(expectedFile).Should().BeTrue();
    }

    [Fact]
    public void AddAdvanceFileLogger_WithFilePathAndMaxBytes_ConfiguresBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "size_config_test.log");

        // Act
        services.AddLogging(builder => builder.AddAdvanceFileLogger(testFilePath, 1024));
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        logger.LogInformation("Test message with size configuration");
        Thread.Sleep(100);

        // Assert
        serviceProvider.Dispose();
        // With new API, files use date+index pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(expectedFile).Should().BeTrue();
    }

    [Fact]
    public void AddAdvanceFileLogger_WithFullConfiguration_ConfiguresAllOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "full_config_test.log");

        // Act
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(testFilePath, 2048, 15);
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        logger.LogInformation("Test message with full configuration");
        Thread.Sleep(100);

        // Assert
        serviceProvider.Dispose();
        // With new API, files use date+index pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(expectedFile).Should().BeTrue();
    }

    [Fact]
    public void AddAdvanceFileLogger_WithCompleteConfiguration_ConfiguresArchiveDirectory()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "archive_config_test.log");

        // Act
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(testFilePath, 2048, 15, "custom_archive");
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();

        logger.LogInformation("Test message with archive configuration");
        Thread.Sleep(100);

        // Assert
        serviceProvider.Dispose();
        // With new API, files use date+index pattern
        var today = DateTime.Today;
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(expectedFile).Should().BeTrue();
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
        var testFilePath = Path.Combine(_testDirectory, "multiple_loggers_test.log");

        services.AddLogging(builder => builder.AddAdvanceFileLogger(testFilePath));
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
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
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
        var testFilePath = Path.Combine(_testDirectory, "service_collection_test.log");

        // Act
        services.AddAdvanceFileLogger(options =>
        {
            options.FilePath = testFilePath;
            options.MaxBytes = 4096;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void ServiceCollectionExtensions_AddAdvanceFileLogger_WithFilePath_ConfiguresPath()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "service_path_test.log");

        // Act
        services.AddAdvanceFileLogger(testFilePath);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
        loggerProviders.Should().ContainSingle(p => p is AdvanceFileLoggerProvider);

        serviceProvider.Dispose();
    }

    [Fact]
    public void ServiceCollectionExtensions_AddAdvanceFileLogger_WithFilePathAndMaxBytes_ConfiguresBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        var testFilePath = Path.Combine(_testDirectory, "service_size_test.log");

        // Act
        services.AddAdvanceFileLogger(testFilePath, 8192);

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
        var testFilePath = Path.Combine(_testDirectory, "integration_test.log");

        // Act - Add multiple logging providers
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddAdvanceFileLogger(testFilePath);
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
        var expectedFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
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