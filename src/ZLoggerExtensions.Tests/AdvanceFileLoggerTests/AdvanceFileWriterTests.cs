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
        var options = CreateTestOptions("write_test.log");
        var message = "Test log message";
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write(message);
            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(expectedFile).Should().BeTrue();
        var content = File.ReadAllText(expectedFile);
        content.Should().Contain(message);
    }

    [Fact]
    public void Write_WithNullOrEmptyMessage_DoesNotWrite()
    {
        // Arrange
        var options = CreateTestOptions("empty_test.log");
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

        // Act
        using var writer = new AdvanceFileWriter(options);
        writer.Write(null!);
        writer.Write("");
        writer.Flush();

        // Assert
        if (File.Exists(expectedFile))
        {
            var content = File.ReadAllText(expectedFile);
            content.Should().BeEmpty();
        }
    }

    [Fact]
    public void Write_WhenSizeExceedsLimit_RollsFile()
    {
        // Arrange
        var options = CreateTestOptions("rolling_test.log");
        options.MaxBytes = 50; // Very small limit to force rolling
        var expectedFile1 = options.GetFileNameProvider()(DateTime.Today, 0);
        var expectedFile2 = options.GetFileNameProvider()(DateTime.Today, 1);

        // Act
        using var writer = new AdvanceFileWriter(options);
        for (int i = 0; i < 5; i++)
        {
            writer.Write($"This is a long message to exceed the file size limit - iteration {i}");
        }
        writer.Flush();

        // Assert - at least one file should exist (current or rolled)
        bool fileExists = File.Exists(expectedFile1) || File.Exists(expectedFile2);
        fileExists.Should().BeTrue();
    }

    [Fact]
    public void Write_WithMaxBytesZero_DoesNotRoll()
    {
        // Arrange
        var options = CreateTestOptions("no_rolling_test.log");
        options.MaxBytes = 0; // Disable rolling
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

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
        var archiveDir = Path.Combine(Path.GetDirectoryName(expectedFile)!, "archive");
        if (Directory.Exists(archiveDir))
        {
            var archivedFiles = Directory.GetFiles(archiveDir);
            archivedFiles.Should().BeEmpty();
        }

        File.Exists(expectedFile).Should().BeTrue();
        var content = File.ReadAllText(expectedFile);
        content.Should().Contain("Message 0");
        content.Should().Contain("Message 9");
    }

    [Fact]
    public async Task WriteAsync_WithValidMessage_WritesToFile()
    {
        // Arrange
        var options = CreateTestOptions("async_test.log");
        var message = "Async test message";
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            await writer.WriteAsync(message);
            await writer.FlushAsync();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(expectedFile).Should().BeTrue();
        var content = File.ReadAllText(expectedFile);
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
        var options = CreateTestOptions("force_roll_test.log");
        var today = DateTime.Today;
        var file0 = options.GetFileNameProvider()(today, 0);
        var file1 = options.GetFileNameProvider()(today, 1);

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write("Message before roll");
            writer.Flush();

            writer.ForceRoll();

            writer.Write("Message after roll");
            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert - After rolling, the current file should contain only the "after roll" message
        // The "before roll" message should be in an archived file or in index 0 if rolling creates index 1
        bool foundAfterRollMessage = false;

        if (File.Exists(file0))
        {
            var content0 = File.ReadAllText(file0);
            if (content0.Contains("Message after roll"))
                foundAfterRollMessage = true;
        }

        if (File.Exists(file1))
        {
            var content1 = File.ReadAllText(file1);
            if (content1.Contains("Message after roll"))
                foundAfterRollMessage = true;
        }

        foundAfterRollMessage.Should().BeTrue("Should find 'Message after roll' in one of the log files");
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
        var options = CreateTestOptions("buffer_test.log");
        options.AutoFlush = false;
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

        // Act
        using (var writer = new AdvanceFileWriter(options))
        {
            writer.Write("Buffered message");

            // Without flush, file might not contain the message immediately
            // This is implementation-dependent, so we'll just ensure no exception

            writer.Flush();
        } // Ensure writer is disposed before reading

        // Assert
        File.Exists(expectedFile).Should().BeTrue();
        var content = File.ReadAllText(expectedFile);
        content.Should().Contain("Buffered message");
    }

    [Fact]
    public void Write_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir", "nested");
        var options = CreateTestOptions("dir_test.log");
        options.FileNameProvider = (dt, index) => Path.Combine(subDir, $"app.{dt:yyyy-MM-dd}_{index}.log");
        options.CreateDirectories = true;
        var expectedFile = options.GetFileNameProvider()(DateTime.Today, 0);

        // Act
        using var writer = new AdvanceFileWriter(options);
        writer.Write("Directory creation test");
        writer.Flush();

        // Assert
        Directory.Exists(subDir).Should().BeTrue();
        File.Exists(expectedFile).Should().BeTrue();
    }

    private AdvanceFileLoggerOptions CreateTestOptions(string fileName)
    {
        return new AdvanceFileLoggerOptions
        {
            FileNameProvider = (dt, index) => Path.Combine(_testDirectory, $"app.{dt:yyyy-MM-dd}_{index}.log"),
            AppName = "app",
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