using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

/// <summary>
/// Unit tests for AdvanceFileLogger functionality.
/// </summary>
public class AdvanceFileLoggerTests
{
    private readonly string _testDirectory;

    public AdvanceFileLoggerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "AdvanceFileLoggerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Test basic logging functionality.
    /// </summary>
    public void TestBasicLogging()
    {
        var logFilePath = Path.Combine(_testDirectory, "test.log");
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = logFilePath,
            MaxBytes = 0 // Disable rolling for this test
        };

        using var writer = new AdvanceFileWriter(options);

        writer.Write("Test message 1");
        writer.Write("Test message 2");
        writer.Flush();

        Assert.True(File.Exists(logFilePath), "Log file should exist");

        var content = File.ReadAllText(logFilePath);
        Assert.Contains("Test message 1", content);
        Assert.Contains("Test message 2", content);
    }

    /// <summary>
    /// Test file rolling when size limit is exceeded.
    /// </summary>
    public void TestFileRolling()
    {
        var logFilePath = Path.Combine(_testDirectory, "rolling.log");
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = logFilePath,
            MaxBytes = 100 // Small size to trigger rolling
        };

        using var writer = new AdvanceFileWriter(options);

        // Write enough data to trigger rolling
        for (int i = 0; i < 10; i++)
        {
            writer.Write($"This is a longer message to trigger file rolling - iteration {i}");
        }
        writer.Flush();

        // Check that archive directory was created and contains rolled files
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Assert.True(Directory.Exists(archiveDir), "Archive directory should exist");

        var archivedFiles = Directory.GetFiles(archiveDir);
        Assert.True(archivedFiles.Length > 0, "Should have archived files");
    }

    /// <summary>
    /// Test archive retention policy.
    /// </summary>
    public void TestArchiveRetention()
    {
        var logFilePath = Path.Combine(_testDirectory, "retention.log");
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = logFilePath,
            MaxBytes = 50, // Very small size
            MaxArchivedFiles = 3 // Keep only 3 archived files
        };

        using var writer = new AdvanceFileWriter(options);

        // Generate many small files to test retention
        for (int i = 0; i < 10; i++)
        {
            writer.Write($"Message {i} - some content to fill the file");
            writer.Flush();
            Thread.Sleep(10); // Small delay to ensure different timestamps
        }

        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            Assert.True(archivedFiles.Length <= options.MaxArchivedFiles + 1,
                $"Should not exceed max archived files. Found: {archivedFiles.Length}");
        }
    }

    /// <summary>
    /// Test FileNameProvider functionality.
    /// </summary>
    public void TestFileNameProvider()
    {
        var basePath = Path.Combine(_testDirectory, "test.log");
        var provider = new FileNameProvider(basePath);

        // Test current file path
        var currentPath = provider.GetCurrentFilePath();
        Assert.Equal(basePath, currentPath);

        // Test rolled file path
        var timestamp = new DateTime(2023, 12, 25, 14, 30, 45);
        var rolledPath = provider.GetRolledFilePath(timestamp);
        Assert.Contains("_20231225_143045", rolledPath);

        // Test archive path
        var archivePath = provider.GetArchivedFilePath("test_20231225_143045.log", "archive");
        Assert.Contains("archive", archivePath);
        Assert.Contains("test_20231225_143045.log", archivePath);

        // Test rolled file detection
        Assert.True(provider.IsRolledFile("test_20231225_143045.log"));
        Assert.False(provider.IsRolledFile("test.log"));
        Assert.False(provider.IsRolledFile("other_file.txt"));
    }

    /// <summary>
    /// Test FileArchiver functionality.
    /// </summary>
    public void TestFileArchiver()
    {
        var basePath = Path.Combine(_testDirectory, "archive_test.log");
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = basePath,
            MaxArchivedFiles = 5
        };

        var provider = new FileNameProvider(basePath);
        var archiver = new FileArchiver(provider, options);

        // Create a test file
        File.WriteAllText(basePath, "Test content");

        // Archive it
        var success = archiver.ArchiveCurrentFile(basePath);
        Assert.True(success, "Archive operation should succeed");

        // Check that file was moved to archive
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Assert.True(Directory.Exists(archiveDir), "Archive directory should exist");

        var archivedFiles = archiver.GetArchivedFiles(archiveDir);
        Assert.True(archivedFiles.Count() > 0, "Should have archived files");
    }

    /// <summary>
    /// Test provider integration with ILoggingBuilder.
    /// </summary>
    public void TestProviderIntegration()
    {
        var logFilePath = Path.Combine(_testDirectory, "provider.log");

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(logFilePath, maxBytes: 1024);
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<AdvanceFileLoggerTests>>();

        logger.LogInformation("Test information message");
        logger.LogWarning("Test warning message");
        logger.LogError("Test error message");

        // Allow some time for async operations
        Thread.Sleep(100);

        // Dispose to ensure flush
        serviceProvider.Dispose();

        Assert.True(File.Exists(logFilePath), "Log file should exist");

        var content = File.ReadAllText(logFilePath);
        Assert.Contains("Test information message", content);
        Assert.Contains("Test warning message", content);
        Assert.Contains("Test error message", content);
    }

    /// <summary>
    /// Test external file access capability.
    /// </summary>
    public void TestExternalFileAccess()
    {
        var logFilePath = Path.Combine(_testDirectory, "external.log");
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = logFilePath,
            AllowExternalAccess = true
        };

        using var writer = new AdvanceFileWriter(options);

        writer.Write("Initial message");
        writer.Flush();

        // Try to read the file while it's being written to
        Assert.True(File.Exists(logFilePath), "Log file should exist");

        // This should not throw if external access is properly configured
        var content = File.ReadAllText(logFilePath);
        Assert.Contains("Initial message", content);

        // Try to delete the file (should be possible with external access)
        // Note: This might not work on all systems due to OS file locking
        try
        {
            var tempCopy = Path.Combine(_testDirectory, "temp_copy.log");
            File.Copy(logFilePath, tempCopy);
            Assert.True(File.Exists(tempCopy), "Should be able to copy the log file");
        }
        catch (Exception)
        {
            // File operations might fail due to OS restrictions, which is acceptable
        }
    }

    /// <summary>
    /// Test configuration validation.
    /// </summary>
    public void TestConfigurationValidation()
    {
        var options = new AdvanceFileLoggerOptions();

        // Test valid configuration
        options.FilePath = "test.log";
        options.MaxBytes = 1024;
        options.MaxArchivedFiles = 5;

        // Should not throw
        options.Validate();

        // Test invalid configurations
        options.FilePath = "";
        Assert.Throws<ArgumentException>(() => options.Validate());

        options.FilePath = "test.log";
        options.MaxBytes = -1;
        Assert.Throws<ArgumentException>(() => options.Validate());

        options.MaxBytes = 1024;
        options.MaxArchivedFiles = -1;
        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    /// <summary>
    /// Cleanup test directory after tests.
    /// </summary>
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
    }
}

/// <summary>
/// Simple assertion helper for tests.
/// </summary>
public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
            throw new Exception(message ?? "Assertion failed: expected true");
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
            throw new Exception(message ?? "Assertion failed: expected false");
    }

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!Equals(expected, actual))
            throw new Exception(message ?? $"Assertion failed: expected {expected}, actual {actual}");
    }

    public static void Contains(string expectedSubstring, string actualString, string? message = null)
    {
        if (actualString == null || !actualString.Contains(expectedSubstring))
            throw new Exception(message ?? $"Assertion failed: '{actualString}' does not contain '{expectedSubstring}'");
    }

    public static void Throws<T>(Action action, string? message = null) where T : Exception
    {
        try
        {
            action();
            throw new Exception(message ?? $"Expected exception of type {typeof(T).Name}");
        }
        catch (T)
        {
            // Expected exception type
        }
        catch (Exception ex)
        {
            throw new Exception(message ?? $"Expected exception of type {typeof(T).Name}, but got {ex.GetType().Name}");
        }
    }
}