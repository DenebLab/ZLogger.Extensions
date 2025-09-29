using FluentAssertions;
using System.Globalization;
using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using Xunit;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class FileNameProviderTests
{
    [Fact]
    public void Constructor_WithValidProvider_InitializesCorrectly()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log";

        // Act
        var provider = new FileNameProvider(nameProvider);

        // Assert
        provider.GetCurrentFilePath().Should().StartWith("logs/");
        provider.GetLogDirectory().Should().Be("logs");
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileNameProvider(null!));
    }

    [Fact]
    public void GetCurrentFilePath_WithDateAndIndex_ReturnsFormattedPath()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var date = new DateTime(2023, 12, 25);
        var index = 2;

        // Act
        var result = provider.GetCurrentFilePath(date, index);

        // Assert
        result.Should().Be("logs/2023-12-25_2.log");
    }

    [Fact]
    public void GetCurrentFilePath_WithoutParameters_UsesTodayWithIndexZero()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var today = DateTime.UtcNow.Date;

        // Act
        var result = provider.GetCurrentFilePath();

        // Assert
        result.Should().Be($"{today:yyyy-MM-dd}_0.log");
    }

    [Theory]
    [InlineData(0, "2023-12-25_0.log")]
    [InlineData(1, "2023-12-25_1.log")]
    [InlineData(5, "2023-12-25_5.log")]
    public void GetCurrentFilePath_WithDifferentIndices_ReturnsCorrectFormat(int index, string expected)
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, idx) => $"{dt:yyyy-MM-dd}_{idx}.log";
        var provider = new FileNameProvider(nameProvider);
        var date = new DateTime(2023, 12, 25);

        // Act
        var result = provider.GetCurrentFilePath(date, index);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetNextFilePath_WithCurrentFile_ReturnsIncrementedIndex()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var currentPath = "logs/2023-12-25_2.log";

        // Act
        var result = provider.GetNextFilePath(currentPath);

        // Assert
        result.Should().Be("logs/2023-12-25_3.log");
    }

    [Fact]
    public void GetNextFilePath_WithInvalidFile_ReturnsFallbackPath()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var today = DateTime.UtcNow.Date;

        // Act
        var result = provider.GetNextFilePath("invalid-file.log");

        // Assert
        result.Should().Be($"{today:yyyy-MM-dd}_1.log");
    }

    [Fact]
    public void GetArchiveDirectoryPath_WithValidDirectory_ReturnsCorrectPath()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);

        // Act
        var result = provider.GetArchiveDirectoryPath("archive");

        // Assert
        result.Should().Be(Path.Combine("logs", "archive"));
    }

    [Fact]
    public void GetArchiveDirectoryPath_WithNullDirectory_ThrowsArgumentException()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"logs/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => provider.GetArchiveDirectoryPath(null!));
    }

    [Theory]
    [InlineData("2023-12-25_0.log", true)]
    [InlineData("2023-12-25_1.log", true)]
    [InlineData("2023-12-25_999.log", true)]
    [InlineData("invalid.log", false)]
    [InlineData("2023-12-25.log", false)]
    [InlineData("", false)]
    public void IsRolledFile_WithVariousFileNames_ReturnsExpectedResult(string fileName, bool expected)
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);

        // Act
        var result = provider.IsRolledFile(fileName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsCurrentDateFile_WithTodaysFile_ReturnsTrue()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var today = DateTime.UtcNow.Date;
        var todaysFile = $"{today:yyyy-MM-dd}_0.log";

        // Act
        var result = provider.IsCurrentDateFile(todaysFile);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentDateFile_WithOldFile_ReturnsFalse()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var oldFile = "2020-01-01_0.log";

        // Act
        var result = provider.IsCurrentDateFile(oldFile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentDateFile_WithSpecificDate_ReturnsCorrectResult()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var testDate = new DateTime(2023, 12, 25);
        var testFile = "2023-12-25_5.log";

        // Act
        var result = provider.IsCurrentDateFile(testFile, testDate);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("2023-12-25_0.log", "2023-12-25", 0)]
    [InlineData("2023-01-01_999.log", "2023-01-01", 999)]
    [InlineData("logs/2023-12-25_5.log", "2023-12-25", 5)]
    public void TryParseFilePath_WithValidFiles_ExtractsDateAndIndex(string filePath, string expectedDateStr, int expectedIndex)
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var expectedDate = DateTime.ParseExact(expectedDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Act
        var result = provider.TryParseFilePath(filePath, out var actualDate, out var actualIndex);

        // Assert
        result.Should().BeTrue();
        actualDate.Date.Should().Be(expectedDate.Date);
        actualIndex.Should().Be(expectedIndex);
    }

    [Theory]
    [InlineData("invalid.log")]
    [InlineData("2023-12-25.log")]
    [InlineData("")]
    [InlineData("not-a-date_0.log")]
    public void TryParseFilePath_WithInvalidFiles_ReturnsFalse(string filePath)
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);

        // Act
        var result = provider.TryParseFilePath(filePath, out _, out _);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHighestIndexForDate_WithExistingFiles_ReturnsHighestIndex()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"temp/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var testDate = new DateTime(2023, 12, 25);

        // Create a temporary directory with test files
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test files
            File.WriteAllText(Path.Combine(tempDir, "2023-12-25_0.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, "2023-12-25_2.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, "2023-12-25_5.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, "2023-12-24_0.log"), "test"); // Different date

            // Create provider that points to temp directory
            Func<DateTime, int, string> tempNameProvider = (dt, index) => Path.Combine(tempDir, $"{dt:yyyy-MM-dd}_{index}.log");
            var tempProvider = new FileNameProvider(tempNameProvider);

            // Act
            var result = tempProvider.GetHighestIndexForDate(testDate);

            // Assert
            result.Should().Be(5);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetHighestIndexForDate_WithNoFiles_ReturnsMinusOne()
    {
        // Arrange
        Func<DateTime, int, string> nameProvider = (dt, index) => $"nonexistent/{dt:yyyy-MM-dd}_{index}.log";
        var provider = new FileNameProvider(nameProvider);
        var testDate = new DateTime(2023, 12, 25);

        // Act
        var result = provider.GetHighestIndexForDate(testDate);

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public void GetOldLogFiles_WithVariousFiles_ReturnsOnlyOldFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var twoDaysAgo = today.AddDays(-2);
            var threeDaysAgo = today.AddDays(-3);

            Func<DateTime, int, string> nameProvider = (dt, index) => Path.Combine(tempDir, $"{dt:yyyy-MM-dd}_{index}.log");
            var provider = new FileNameProvider(nameProvider);

            // Create test files
            File.WriteAllText(Path.Combine(tempDir, $"{today:yyyy-MM-dd}_0.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, $"{yesterday:yyyy-MM-dd}_0.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, $"{twoDaysAgo:yyyy-MM-dd}_0.log"), "test");
            File.WriteAllText(Path.Combine(tempDir, $"{threeDaysAgo:yyyy-MM-dd}_0.log"), "test");

            // Act
            var result = provider.GetOldLogFiles(2); // Keep 2 days

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().Contain($"{threeDaysAgo:yyyy-MM-dd}_0.log");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}