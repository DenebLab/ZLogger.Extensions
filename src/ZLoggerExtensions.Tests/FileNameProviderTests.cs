using FluentAssertions;
using System.Globalization;
using Xunit;
using ZLoggerExtensions.AdvanceFileLogger;

namespace ZLoggerExtensions.Tests;

public class FileNameProviderTests
{
    [Fact]
    public void Constructor_WithValidPath_InitializesCorrectly()
    {
        // Arrange
        var basePath = @"C:\logs\app.log";

        // Act
        var provider = new FileNameProvider(basePath);

        // Assert
        provider.GetCurrentFilePath().Should().Be(basePath);
        provider.GetLogDirectory().Should().Be(@"C:\logs");
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileNameProvider(null!));
    }

    [Fact]
    public void GetCurrentFilePath_ReturnsBasePath()
    {
        // Arrange
        var basePath = "logs/test.log";
        var provider = new FileNameProvider(basePath);

        // Act
        var result = provider.GetCurrentFilePath();

        // Assert
        result.Should().Be(basePath);
    }

    [Fact]
    public void GetRolledFilePath_WithTimestamp_ReturnsFormattedPath()
    {
        // Arrange
        var basePath = "logs/app.log";
        var provider = new FileNameProvider(basePath);
        var timestamp = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var result = provider.GetRolledFilePath(timestamp);

        // Assert
        result.Should().Be(Path.Combine("logs", "app_20231225_143045.log"));
    }

    [Theory]
    [InlineData("app.log", "app_20231225_143045.log")]
    [InlineData("test", "test_20231225_143045.log")]
    [InlineData("my.application.log", "my.application_20231225_143045.log")]
    public void GetRolledFilePath_WithDifferentFileNames_ReturnsCorrectFormat(string fileName, string expected)
    {
        // Arrange
        var provider = new FileNameProvider(fileName);
        var timestamp = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var result = Path.GetFileName(provider.GetRolledFilePath(timestamp));

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetArchivedFilePath_WithValidInputs_ReturnsCorrectPath()
    {
        // Arrange
        var basePath = @"C:\logs\app.log";
        var provider = new FileNameProvider(basePath);
        var fileName = "app_20231225_143045.log";
        var archiveDir = "archive";

        // Act
        var result = provider.GetArchivedFilePath(fileName, archiveDir);

        // Assert
        result.Should().Be(@"C:\logs\archive\app_20231225_143045.log");
    }



    [Fact]
    public void GetArchiveDirectoryPath_ReturnsCorrectPath()
    {
        // Arrange
        var basePath = @"C:\logs\app.log";
        var provider = new FileNameProvider(basePath);

        // Act
        var result = provider.GetArchiveDirectoryPath("archive");

        // Assert
        result.Should().Be(@"C:\logs\archive");
    }

    [Fact]
    public void IsRolledFile_WithValidRolledFileName_ReturnsTrue()
    {
        // Arrange
        var provider = new FileNameProvider("app.log");

        // Act & Assert
        provider.IsRolledFile("app_20231225_143045.log").Should().BeTrue();
        provider.IsRolledFile("app_20240101_000000.log").Should().BeTrue();
        provider.IsRolledFile("app_20231231_235959.log").Should().BeTrue();
    }

    [Fact]
    public void IsRolledFile_WithInvalidRolledFileName_ReturnsFalse()
    {
        // Arrange
        var provider = new FileNameProvider("app.log");

        // Act & Assert
        provider.IsRolledFile("app.log").Should().BeFalse();
        provider.IsRolledFile("other_20231225_143045.log").Should().BeFalse();
        provider.IsRolledFile("app_invalid_timestamp.log").Should().BeFalse();
        provider.IsRolledFile("app_20231225.log").Should().BeFalse();
        provider.IsRolledFile("app_20231225_143045.txt").Should().BeFalse();
        provider.IsRolledFile("").Should().BeFalse();
        provider.IsRolledFile(null!).Should().BeFalse();
    }

    [Theory]
    [InlineData("app.log", "test.log")]
    [InlineData("myapp.log", "otherapp.log")]
    public void IsRolledFile_WithDifferentBaseFileName_ReturnsFalse(string baseName, string testFile)
    {
        // Arrange
        var provider = new FileNameProvider(baseName);
        var baseWithoutExt = Path.GetFileNameWithoutExtension(testFile);
        var rolledFileName = $"{baseWithoutExt}_20231225_143045.log";

        // Act
        var result = provider.IsRolledFile(rolledFileName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExtractTimestampFromRolledFile_WithValidFile_ReturnsTimestamp()
    {
        // Arrange
        var provider = new FileNameProvider("app.log");
        var fileName = "app_20231225_143045.log";

        // Act
        var result = provider.ExtractTimestampFromRolledFile(fileName);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(new DateTime(2023, 12, 25, 14, 30, 45));
    }

    [Theory]
    [InlineData("app.log")]
    [InlineData("other_20231225_143045.log")]
    [InlineData("app_invalid.log")]
    [InlineData("")]
    public void ExtractTimestampFromRolledFile_WithInvalidFile_ReturnsNull(string fileName)
    {
        // Arrange
        var provider = new FileNameProvider("app.log");

        // Act
        var result = provider.ExtractTimestampFromRolledFile(fileName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FileNameProvider_WithFileWithoutExtension_AddsLogExtension()
    {
        // Arrange
        var provider = new FileNameProvider("app");

        // Act
        var rolledPath = provider.GetRolledFilePath(DateTime.UtcNow);

        // Assert
        rolledPath.Should().EndWith(".log");
    }

    [Fact]
    public void FileNameProvider_WithEmptyDirectory_HandlesCorrectly()
    {
        // Arrange
        var provider = new FileNameProvider("app.log");

        // Act
        var logDir = provider.GetLogDirectory();

        // Assert
        logDir.Should().BeEmpty();
    }

    [Fact]
    public void GetRolledFilePath_UsesInvariantCulture()
    {
        // Arrange
        var provider = new FileNameProvider("test.log");
        var timestamp = new DateTime(2023, 1, 1, 1, 1, 1);

        // Save current culture
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            // Set a different culture
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            var result = provider.GetRolledFilePath(timestamp);

            // Assert
            result.Should().EndWith("test_20230101_010101.log");
        }
        finally
        {
            // Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}