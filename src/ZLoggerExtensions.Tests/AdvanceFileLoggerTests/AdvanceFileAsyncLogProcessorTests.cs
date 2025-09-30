using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using Xunit;
using ZLogger;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class AdvanceFileAsyncLogProcessorTests : IDisposable
{
    private readonly string _testDirectory;

    public AdvanceFileAsyncLogProcessorTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "AdvanceFileAsyncLogProcessorTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void Constructor_WithValidOptions_InitializesCorrectly()
    {
        // Arrange
        var options = CreateTestOptions("test.log");

        // Act
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Assert
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
        processor.CurrentFileSize.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdvanceFileAsyncLogProcessor(null!));
    }

    [Fact]
    public void Post_WithNullLogEntry_HandlesGracefully()
    {
        // Arrange
        var options = CreateTestOptions("null_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => processor.Post(null!));
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Post_AfterDisposal_HandlesGracefully()
    {
        // Arrange
        var options = CreateTestOptions("disposed_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);
        await processor.DisposeAsync();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => processor.Post(null!));
        exception.Should().BeNull();
        processor.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ReleasesResources()
    {
        // Arrange
        var options = CreateTestOptions("dispose_async_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act
        await processor.DisposeAsync();

        // Assert
        processor.IsDisposed.Should().BeTrue();
        processor.CurrentFileSize.Should().Be(0);
    }

    [Fact]
    public void ForceRoll_BeforeDisposal_ExecutesSuccessfully()
    {
        // Arrange
        var options = CreateTestOptions("force_roll_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => processor.ForceRoll());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task ForceRoll_AfterDisposal_HandlesGracefully()
    {
        // Arrange
        var options = CreateTestOptions("force_roll_disposed_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);
        await processor.DisposeAsync();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => processor.ForceRoll());
        exception.Should().BeNull();
    }

    [Fact]
    public void Flush_BeforeDisposal_ExecutesSuccessfully()
    {
        // Arrange
        var options = CreateTestOptions("flush_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => processor.Flush());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task FlushAsync_BeforeDisposal_ExecutesSuccessfully()
    {
        // Arrange
        var options = CreateTestOptions("flush_async_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => processor.FlushAsync());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task FlushAsync_AfterDisposal_HandlesGracefully()
    {
        // Arrange
        var options = CreateTestOptions("flush_async_disposed_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);
        await processor.DisposeAsync();

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => processor.FlushAsync());
        exception.Should().BeNull();
    }

    [Fact]
    public void CurrentFileSize_InitiallyZero()
    {
        // Arrange
        var options = CreateTestOptions("size_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert
        processor.CurrentFileSize.Should().Be(0);
    }

    [Fact]
    public async Task CurrentFileSize_AfterDisposal_ReturnsZero()
    {
        // Arrange
        var options = CreateTestOptions("size_disposed_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);
        await processor.DisposeAsync();

        // Act & Assert
        processor.CurrentFileSize.Should().Be(0);
    }

    [Fact]
    public void IsDisposed_InitiallyFalse()
    {
        // Arrange
        var options = CreateTestOptions("disposed_state_test.log");
        using var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act & Assert
        processor.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public async Task IsDisposed_AfterDisposal_ReturnsTrue()
    {
        // Arrange
        var options = CreateTestOptions("disposed_state_after_test.log");
        var processor = new AdvanceFileAsyncLogProcessor(options);

        // Act
        await processor.DisposeAsync();

        // Assert
        processor.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Processor_WithRollingConfiguration_HandlesCorrectly()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "rolling_test.log");
        var options = CreateTestOptions(logFile);
        options.MaxBytes = 100; // Small size to test rolling configuration

        // Act & Assert - Should not throw during construction
        using var processor = new AdvanceFileAsyncLogProcessor(options);
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Processor_WithArchivingConfiguration_HandlesCorrectly()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "archiving_test.log");
        var options = CreateTestOptions(logFile);
        options.MaxArchivedFiles = 5;
        options.ArchiveDirectory = "custom_archive";

        // Act & Assert - Should not throw during construction
        using var processor = new AdvanceFileAsyncLogProcessor(options);
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Processor_WithExternalAccessConfiguration_HandlesCorrectly()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "external_access_test.log");
        var options = CreateTestOptions(logFile);
        options.AllowExternalAccess = true;

        // Act & Assert - Should not throw during construction
        using var processor = new AdvanceFileAsyncLogProcessor(options);
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Processor_WithBufferingConfiguration_HandlesCorrectly()
    {
        // Arrange
        var logFile = Path.Combine(_testDirectory, "buffering_test.log");
        var options = CreateTestOptions(logFile);
        options.AutoFlush = false;
        options.BufferSize = 8192;

        // Act & Assert - Should not throw during construction
        using var processor = new AdvanceFileAsyncLogProcessor(options);
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Processor_WithDirectoryCreation_HandlesCorrectly()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir", "nested");
        var logFile = Path.Combine(subDir, "directory_test.log");
        var options = CreateTestOptions(logFile);
        options.CreateDirectories = true;

        // Act & Assert - Should not throw during construction
        using var processor = new AdvanceFileAsyncLogProcessor(options);
        processor.Should().NotBeNull();
        processor.IsDisposed.Should().BeFalse();
    }

    private AdvanceFileLoggerOptions CreateTestOptions(string fileName)
    {
        return new AdvanceFileLoggerOptions
        {
            LogDirPath = _testDirectory,
            AppName = Path.GetFileNameWithoutExtension(fileName),
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