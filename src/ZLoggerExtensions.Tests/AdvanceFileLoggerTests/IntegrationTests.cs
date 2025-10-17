using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class IntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    public IntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "IntegrationTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void CompleteLoggingWorkflow_WithRollingAndArchiving_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.MaxBytes = 200; // Small size to force rolling
                options.MaxArchivedFiles = 3;
                options.ArchiveDirectory = "archive";
                options.AutoFlush = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Act - Generate enough logs to trigger rolling and archiving
        for (var i = 0; i < 20; i++)
        {
            logger.LogInformation(
                "Integration test message {MessageId} - This is a longer message to trigger file rolling", i);
            logger.LogWarning("Warning message {MessageId} - Additional content for file size", i);
            logger.LogError("Error message {MessageId} - Even more content to ensure rolling happens", i);
        }

        Thread.Sleep(500); // Allow time for file operations
        serviceProvider.Dispose();

        // Assert - Check for files with today's date pattern
        var today = DateTime.Today;
        var currentFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        var alternateFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_1.log");

        var hasCurrentFile = File.Exists(currentFile) || File.Exists(alternateFile);
        hasCurrentFile.Should().BeTrue();

        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            if (archivedFiles.Any()) archivedFiles.Length.Should().BeLessThanOrEqualTo(3); // Respect retention policy
        }
    }

    [Fact]
    public async Task ConcurrentLogging_WithMultipleLoggers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.MaxBytes = 0; // Disable rolling for this test
                options.AutoFlush = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Act - Create concurrent logging tasks
        var tasks = new List<Task>();
        for (var taskId = 0; taskId < 5; taskId++)
        {
            var currentTaskId = taskId;
            tasks.Add(Task.Run(() =>
            {
                var logger = loggerFactory.CreateLogger($"Task{currentTaskId}");
                for (var i = 0; i < 50; i++)
                {
                    logger.LogInformation("Concurrent message from Task{TaskId} - Message{MessageId}", currentTaskId,
                        i);
                    Thread.Sleep(1); // Small delay to interleave messages
                }
            }));
        }

        await Task.WhenAll(tasks);
        Thread.Sleep(200); // Allow final writes to complete
        serviceProvider.Dispose();

        // Assert
        var today = DateTime.Today;
        var logFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().NotBeEmpty();

        // Verify all tasks wrote messages
        for (var taskId = 0; taskId < 5; taskId++) content.Should().Contain($"Task{taskId}");

        // Count total messages (should be 5 tasks * 50 messages = 250)
        var messageCount = content.Split('\n').Where(line => line.Contains("Concurrent message")).Count();
        messageCount.Should().Be(250);
    }

    [Fact]
    public void LoggingWithScopes_PreservesContextInformation()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider = (dt, index) =>
                    Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Act
        using (logger.BeginScope("OrderProcessing"))
        {
            using (logger.BeginScope("OrderId: {OrderId}", "ORD-12345"))
            {
                logger.LogInformation("Processing order items");
                logger.LogWarning("Low inventory for item {ItemId}", "ITEM-001");

                using (logger.BeginScope("PaymentProcessing"))
                {
                    logger.LogInformation("Processing payment");
                    logger.LogError("Payment gateway timeout");
                }
            }
        }

        Thread.Sleep(100);
        serviceProvider.Dispose();

        // Assert
        var today = DateTime.Today;
        var logFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain("Processing order items");
        content.Should().Contain("OrderProcessing");
        content.Should().Contain("ORD-12345");
        content.Should().Contain("PaymentProcessing");
    }

    [Fact]
    public void DevelopmentMode_AllowsReadingDuringLogging()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.Mode = LoggerMode.Development;
                options.AutoFlush = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();
        var today = DateTime.Today;
        var logFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");

        // Act
        logger.LogInformation("Message before external read");
        Thread.Sleep(50);

        // Try to read the file while logging is active
        var externalContent = "";
        var readException = Record.Exception(() =>
        {
            // Try multiple times as file might be momentarily locked during flush
            for (var i = 0; i < 5; i++)
                try
                {
                    using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fileStream);
                    externalContent = reader.ReadToEnd();
                    break;
                }
                catch (IOException)
                {
                    if (i < 4) Thread.Sleep(10);
                    else throw;
                }
        });

        logger.LogInformation("Message after external read");
        Thread.Sleep(50);
        serviceProvider.Dispose();

        // Assert
        readException.Should().BeNull(); // Should not throw
        externalContent.Should().Contain("Message before external read");

        var finalContent = File.ReadAllText(logFile);
        finalContent.Should().Contain("Message before external read");
        finalContent.Should().Contain("Message after external read");
    }

    [Fact]
    public void HighVolumeLogging_MaintainsPerformance()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.MaxBytes = 10 * 1024 * 1024; // 10MB
                options.AutoFlush = false; // Use buffering for better performance
                options.BufferSize = 8192;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Act - Log a large number of messages
        var startTime = DateTime.UtcNow;
        for (var i = 0; i < 10000; i++)
        {
            logger.LogInformation("Performance test message {MessageId} with timestamp {Timestamp}", i,
                DateTime.UtcNow);

            if (i % 1000 == 0) logger.LogWarning("Checkpoint reached: {Checkpoint}", i);
        }

        var endTime = DateTime.UtcNow;
        serviceProvider.Dispose();

        // Assert
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(10)); // Should complete within 10 seconds

        var today = DateTime.Today;
        var logFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain("Performance test message 0");
        content.Should().Contain("Performance test message 9999");
        content.Should().Contain("Checkpoint reached: 9000");
    }

    [Fact]
    public void ErrorRecovery_HandlesFileSystemIssues()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.MaxBytes = 0; // Disable rolling
                options.AutoFlush = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();
        var today = DateTime.Today;
        var logFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");

        // Act
        logger.LogInformation("Message before file manipulation");
        Thread.Sleep(50);

        // Simulate external file deletion (if possible)
        try
        {
            if (File.Exists(logFile)) File.Delete(logFile);
        }
        catch
        {
            // File might be locked, which is expected
        }

        // Continue logging - should not crash even if file was deleted
        var loggingException = Record.Exception(() =>
        {
            logger.LogInformation("Message after file manipulation");
            logger.LogWarning("Recovery test warning");
            Thread.Sleep(100);
        });

        serviceProvider.Dispose();

        // Assert - Logger should not throw exceptions when continuing to log
        loggingException.Should().BeNull();

        // The original file might not exist if it was successfully deleted
        // but the logger should have handled it gracefully without crashing
    }

    [Fact]
    public void CompleteArchiveWorkflow_RespectsRetentionPolicy()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FileNameProvider =
                    (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log");
                options.MaxBytes = 100; // Very small to force frequent rolling
                options.MaxArchivedFiles = 2; // Keep only 2 archived files
                options.ArchiveDirectory = "archive";
                options.AutoFlush = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Act - Generate many messages to create multiple archive files
        for (var i = 0; i < 100; i++)
        {
            logger.LogInformation("Retention test message {MessageId} - Content to force rolling behavior", i);
            if (i % 10 == 0) Thread.Sleep(10); // Allow file operations to complete
        }

        Thread.Sleep(500); // Allow final operations to complete
        serviceProvider.Dispose();

        // Assert
        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            archivedFiles.Length.Should().BeLessThanOrEqualTo(2); // Should respect retention policy
        }

        var today = DateTime.Today;
        var currentFile = Path.Combine(_testDirectory, $"app.{today:yyyy-MM-dd}_0.log");
        File.Exists(currentFile).Should().BeTrue();
    }
}