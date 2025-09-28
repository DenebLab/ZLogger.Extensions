using System.Text;
using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using FluentAssertions;
using Xunit;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class AdvanceFileLoggerOptionsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()

    {
        // Arrange & Act
        var options = new AdvanceFileLoggerOptions();

        // Assert
        options.FilePath.Should().Be("logs/app.log");
        options.MaxBytes.Should().Be(50 * 1024 * 1024); // 50 MB
        options.ArchiveDirectory.Should().Be("archive");
        options.MaxArchivedFiles.Should().Be(7);
        options.CreateDirectories.Should().BeTrue();
        options.Encoding.Should().Be(Encoding.UTF8);
        options.AutoFlush.Should().BeTrue();
        options.BufferSize.Should().Be(4096);
        options.AllowExternalAccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var options = new AdvanceFileLoggerOptions
        {
            FilePath = "test.log",
            MaxBytes = 1024,
            MaxArchivedFiles = 5,
            ArchiveDirectory = "archive",
            BufferSize = 2048
        };

        // Act & Assert
        var exception = Record.Exception(() => options.Validate());
        exception.Should().BeNull();
    }

    

   


    [Fact]
    public void MaxBytes_SetToZero_DisablesRolling()
    {
        // Arrange
        var options = new AdvanceFileLoggerOptions();

        // Act
        options.MaxBytes = 0;

        // Assert
        options.MaxBytes.Should().Be(0);
        var exception = Record.Exception(() => options.Validate());
        exception.Should().BeNull();
    }

    [Fact]
    public void MaxArchivedFiles_SetToZero_DisablesArchiving()
    {
        // Arrange
        var options = new AdvanceFileLoggerOptions();

        // Act
        options.MaxArchivedFiles = 0;

        // Assert
        options.MaxArchivedFiles.Should().Be(0);
        var exception = Record.Exception(() => options.Validate());
        exception.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new AdvanceFileLoggerOptions();
        var customEncoding = Encoding.ASCII;

        // Act
        options.FilePath = "custom/path.log";
        options.MaxBytes = 1000000;
        options.ArchiveDirectory = "custom_archive";
        options.MaxArchivedFiles = 15;
        options.CreateDirectories = false;
        options.Encoding = customEncoding;
        options.AutoFlush = false;
        options.BufferSize = 8192;
        options.AllowExternalAccess = false;

        // Assert
        options.FilePath.Should().Be("custom/path.log");
        options.MaxBytes.Should().Be(1000000);
        options.ArchiveDirectory.Should().Be("custom_archive");
        options.MaxArchivedFiles.Should().Be(15);
        options.CreateDirectories.Should().BeFalse();
        options.Encoding.Should().Be(customEncoding);
        options.AutoFlush.Should().BeFalse();
        options.BufferSize.Should().Be(8192);
        options.AllowExternalAccess.Should().BeFalse();
    }
}