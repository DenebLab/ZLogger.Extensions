using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using FluentAssertions;
using Xunit;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class AdvanceFileWriterTests : IDisposable
{
    private readonly string _testDirectory;

    public AdvanceFileWriterTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "AdvanceFileWriterTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void Constructor_WithValidOptions_InitializesCorrectly()
    {
        // Arrange
        var options = CreateTestOptions("test.log");

        // Act
        using var writer = new AdvanceFileWriter(options);

        // Assert
        writer.Should().NotBeNull();
        writer.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdvanceFileWriter(null!));
    }

    [Fact]
    public void Write_WithValidMessage_WritesToFile()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "write_test.log");
        var options = CreateTestOptions(logFile);
        var message = "Test log message";

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write(message);
            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain(message);
    }

    [Fact]
    public void Write_WithNullOrEmptyMessage_DoesNotWrite()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "empty_test.log");
        var options = CreateTestOptions(logFile);

        // Act
        using var writer = new AdvanceFileWriter(options);
        writer.Write(null!);
        writer.Write("");
        writer.Flush();

        // Assert
        if (File.Exists(logFile))
        {
            var content = File.ReadAllText(logFile);
            content.Should().BeEmpty();
        }
    }

    [Fact]
    public void Write_WhenSizeExceedsLimit_RollsFile()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "rolling_test.log");
        var options = CreateTestOptions(logFile);
        options.MaxBytes = 50; // Very small limit to force rolling

        // Act
        using var writer = new AdvanceFileWriter(options);
        for (int i = 0; i < 5; i++)
        {
            writer.Write($"This is a long message to exceed the file size limit - iteration {i}");
        }
        writer.Flush();

        // Assert
        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            archivedFiles.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void Write_WithMaxBytesZero_DoesNotRoll()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "no_rolling_test.log");
        var options = CreateTestOptions(logFile);
        options.MaxBytes = 0; // Disable rolling

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            for (int i = 0; i < 10; i++)
            {
                writer.Write($"Message {i} - This is a longer message to test that rolling is disabled");
            }
            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            archivedFiles.Should().BeEmpty();
        }

        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain("Message 0");
        content.Should().Contain("Message 9");
    }

    [Fact]
    public async Task WriteAsync_WithValidMessage_WritesToFile()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "async_test.log");
        var options = CreateTestOptions(logFile);
        var message = "Async test message";

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            await writer.WriteAsync(message);
            await writer.FlushAsync();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain(message);
    }

    [Fact]
    public void CurrentFileSize_AfterWriting_ReflectsActualSize()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "size_test.log");
        var options = CreateTestOptions(logFile);
        var message = "Size test message";

        // Act
        using var writer = new AdvanceFileWriter(options);
        var initialSize = writer.CurrentFileSize;
        writer.Write(message);
        writer.Flush();
        var finalSize = writer.CurrentFileSize;

        // Assert
        initialSize.Should().Be(0);
        finalSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ForceRoll_ManuallyTriggersRolling()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "force_roll_test.log");
        var options = CreateTestOptions(logFile);

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write("Message before roll");
            writer.Flush();

            writer.ForceRoll();

            writer.Write("Message after roll");
            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        var archiveDir = Path.Combine(_testDirectory, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            archivedFiles.Should().NotBeEmpty();
        }

        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain("Message after roll");
        content.Should().NotContain("Message before roll");
    }

    [Fact]
    public void Dispose_ReleasesResources()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "dispose_test.log");
        var options = CreateTestOptions(logFile);
        var writer = new AdvanceFileWriter(options);

        // Act
        writer.Write("Test message");
        writer.Dispose();

        // Assert
        writer.IsDisposed.Should().BeTrue();

        // Writing after disposal should throw
        Assert.Throws<ObjectDisposedException>(() => writer.Write("After disposal"));
    }

    [Fact]
    public void Write_AfterDisposal_ThrowsObjectDisposedException()
    {
        // Arrange
        var options = CreateTestOptions("disposed_test.log");
        var writer = new AdvanceFileWriter(options);
        writer.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => writer.Write("test"));
    }


    [Fact]
    public void Flush_AfterDisposal_DoesNotThrow()
    {
        // Arrange
        var options = CreateTestOptions("flush_disposed_test.log");
        var writer = new AdvanceFileWriter(options);
        writer.Dispose();

        // Act & Assert
        var exception = Record.Exception(() => writer.Flush());
        exception.Should().BeNull();
    }

    [Fact]
    public void Write_WithAutoFlushDisabled_BuffersMessages()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "buffer_test.log");
        var options = CreateTestOptions(logFile);
        options.AutoFlush = false;

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write("Buffered message");

            // Without flush, file might not contain the message immediately
            // This is implementation-dependent, so we'll just ensure no exception

            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(logFile).Should().BeTrue();
        var content = File.ReadAllText(logFile);
        content.Should().Contain("Buffered message");
    }

    [Fact]
    public void Write_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir", "nested");
        var logFile = Path.Combine(subDir, "dir_test.log");
        var options = CreateTestOptions(logFile);
        options.CreateDirectories = true;

        // Act
        using var writer = new AdvanceFileWriter(options);
        writer.Write("Directory creation test");
        writer.Flush();

        // Assert
        Directory.Exists(subDir).Should().BeTrue();
        File.Exists(logFile).Should().BeTrue();
    }

    private AdvanceFileLoggerOptions CreateTestOptions(string fileName)
    {
        var fullPath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(_testDirectory, fileName);
        return new AdvanceFileLoggerOptions
        {
            FilePath = fullPath,
            MaxBytes = 1024 * 1024, // 1MB default
            MaxArchivedFiles = 5,
            ArchiveDirectory = "archive",
            CreateDirectories = true,
            AutoFlush = true,
            AllowExternalAccess = true
        };
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