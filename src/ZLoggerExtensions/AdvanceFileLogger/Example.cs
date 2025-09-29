using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Example demonstrating AdvanceFileLogger usage with ZLogger primitives.
/// </summary>
public class Example
{
    /// <summary>
    /// Basic usage example.
    /// </summary>
    public static void BasicUsage()
    {
        // Set up logging with AdvanceFileLogger
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger("logs/example.log", maxBytes: 1024 * 1024); // 1MB
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // Log some messages
        logger.LogInformation("Application started at {StartTime}", DateTime.UtcNow);
        logger.LogDebug("Debug information: Processing item {ItemId}", 123);
        logger.LogWarning("Warning: Low disk space detected");

        try
        {
            throw new InvalidOperationException("Example exception");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing");
        }

        logger.LogInformation("Application finished");

        // Clean up
        serviceProvider.Dispose();
    }

    /// <summary>
    /// Advanced configuration example.
    /// </summary>
    public static void AdvancedUsage()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FilePath = "logs/advanced.log";
                options.MaxBytes = 5 * 1024 * 1024; // 5MB before rolling
                options.MaxArchivedFiles = 10; // Keep 10 archived files
                options.ArchiveDirectory = "archived_logs";
                options.AllowExternalAccess = true; // Allow external tools to read files
                options.AutoFlush = true; // Immediate disk write
                options.CreateDirectories = true; // Auto-create log directories
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // Generate many log messages to demonstrate rolling
        for (int i = 0; i < 1000; i++)
        {
            logger.LogInformation("Processing item {ItemNumber}: {Message}",
                i, $"This is a sample message with some content to fill up the log file - iteration {i}");

            if (i % 100 == 0)
            {
                logger.LogWarning("Checkpoint: Processed {Count} items", i);
            }
        }

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Scoped logging example.
    /// </summary>
    public static void ScopedLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger("logs/scoped.log");
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        using (logger.BeginScope("OrderProcessing"))
        {
            using (logger.BeginScope("OrderId: {OrderId}", "ORD-12345"))
            {
                logger.LogInformation("Starting order processing");
                logger.LogInformation("Validating order items");
                logger.LogInformation("Calculating totals");
                logger.LogInformation("Order processing completed successfully");
            }
        }

        using (logger.BeginScope("PaymentProcessing"))
        {
            logger.LogInformation("Processing payment");
            logger.LogWarning("Payment gateway timeout, retrying...");
            logger.LogInformation("Payment processed successfully");
        }

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Multi-threaded logging example.
    /// </summary>
    public static async Task MultiThreadedLogging()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger("logs/multithreaded.log");
        });

        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Create multiple tasks that log simultaneously
        var tasks = new Task[5];
        for (int taskId = 0; taskId < tasks.Length; taskId++)
        {
            int currentTaskId = taskId;
            tasks[taskId] = Task.Run(() =>
            {
                var logger = loggerFactory.CreateLogger($"Task{currentTaskId}");

                for (int i = 0; i < 100; i++)
                {
                    logger.LogInformation("Task {TaskId} - Message {MessageId}: Processing item {ItemId}",
                        currentTaskId, i, $"ITEM-{currentTaskId}-{i}");

                    Thread.Sleep(10); // Simulate work
                }

                logger.LogInformation("Task {TaskId} completed", currentTaskId);
            });
        }

        await Task.WhenAll(tasks);
        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing ZLogger integration benefits.
    /// </summary>
    public static void ZLoggerIntegration()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.FilePath = "logs/zlogger.log";
                options.MaxBytes = 1024 * 1024; // 1MB
                options.AllowExternalAccess = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // Benefits of ZLogger integration:
        // 1. Memory pooling for reduced allocations
        // 2. Optimized formatting
        // 3. Better performance

        logger.LogInformation("ZLogger integration provides memory pooling");
        logger.LogInformation("Optimized formatting reduces CPU usage");
        logger.LogInformation("Better performance with structured logging");

        // Structured logging with template parameters
        var orderId = "ORD-12345";
        var amount = 99.99m;
        var customerId = 123;

        logger.LogInformation("Order {OrderId} for customer {CustomerId} with amount {Amount:C} processed",
            orderId, customerId, amount);

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing new ZLogger RollingFile-style configuration.
    /// </summary>
    public static void ZLoggerRollingFileStyle()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // New ZLogger-style API: files named like 2025-09-29_0.log, 2025-09-29_1.log, etc.
            builder.AddZLoggerRollingFile((dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log", 1024 * 1024);
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // This will create files like:
        // logs/2025-09-29_0.log (current file for today)
        // logs/2025-09-29_1.log (when file gets too large)
        // logs/2025-09-30_0.log (tomorrow's file)

        for (int i = 0; i < 1000; i++)
        {
            logger.LogInformation("ZLogger RollingFile-style logging: Message {MessageId} at {Timestamp}",
                i, DateTime.UtcNow);
        }

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing advanced ZLogger RollingFile configuration.
    /// </summary>
    public static void AdvancedZLoggerRollingFile()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Advanced configuration with custom path structure and archive settings
            builder.AddZLoggerRollingFile(
                (dt, index) => $"applications/myapp/{dt:yyyy}/{dt:MM}/{dt:yyyy-MM-dd}_{index}.log",
                5 * 1024 * 1024, // 5MB files
                30); // Keep 30 days of logs
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // This creates a hierarchical structure:
        // applications/myapp/2025/09/2025-09-29_0.log
        // applications/myapp/2025/09/2025-09-29_1.log
        // applications/myapp/2025/10/2025-10-01_0.log

        logger.LogInformation("Application started with advanced rolling file configuration");
        logger.LogDebug("Debug message with structured data: {Data}", new { UserId = 123, Action = "Login" });
        logger.LogWarning("Warning: Advanced logging configuration active");

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing AppName usage in rolling file configuration.
    /// </summary>
    public static void AppNameRollingFile()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Simple AppName configuration - creates files like MyWebAPI.2025-09-29_0.log
            builder.AddZLoggerRollingFile("MyWebAPI", 1024 * 1024); // 1MB files
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // This creates files like:
        // MyWebAPI.2025-09-29_0.log (current file)
        // MyWebAPI.2025-09-29_1.log (when file gets too large)
        // MyWebAPI.2025-09-30_0.log (tomorrow's file)

        logger.LogInformation("Web API started with application-specific log files");
        logger.LogInformation("AppName allows easy identification of log files by application");

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing AppName with advanced configuration.
    /// </summary>
    public static void AppNameAdvancedConfiguration()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.AppName = "OrderService";
                options.MaxBytes = 2 * 1024 * 1024; // 2MB files
                options.MaxArchivedFiles = 14; // Keep 2 weeks
                options.ArchiveDirectory = "archived";
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // This creates files like:
        // OrderService.2025-09-29_0.log (current)
        // archived/OrderService.2025-09-15_0.log (old files)

        logger.LogInformation("Order service started with AppName: {AppName}", "OrderService");
        logger.LogInformation("Log files are easily identifiable by service name");

        serviceProvider.Dispose();
    }

    /// <summary>
    /// Example showing custom file naming with AppName access.
    /// </summary>
    public static void CustomFileNamingWithAppName()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(options =>
            {
                options.AppName = "DataProcessor";
                // Custom naming that includes app name in a specific pattern
                options.FileNameProvider = (dt, index) =>
                    $"logs/{options.AppName}/{dt:yyyy-MM}/{options.AppName}_{dt:dd}_{index:D3}.log";
                options.MaxBytes = 5 * 1024 * 1024; // 5MB files
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Example>>();

        // This creates files like:
        // logs/DataProcessor/2025-09/DataProcessor_29_000.log
        // logs/DataProcessor/2025-09/DataProcessor_29_001.log
        // logs/DataProcessor/2025-10/DataProcessor_01_000.log

        logger.LogInformation("Data processor with custom file naming pattern");
        logger.LogInformation("AppName is accessible in custom FileNameProvider functions");

        serviceProvider.Dispose();
    }
}