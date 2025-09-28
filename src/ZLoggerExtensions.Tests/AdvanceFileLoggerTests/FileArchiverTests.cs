using Deneblab.ZLoggerExtensions.AdvanceFileLogger;
using FluentAssertions;
using Xunit;

namespace Deneblab.ZLoggerExtensionsTests.AdvanceFileLoggerTests;

public class FileArchiverTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly AdvanceFileLoggerOptions _options;
    private readonly FileNameProvider _fileNameProvider;
    private readonly FileArchiver _archiver;

    public FileArchiverTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileArchiverTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        var logFilePath = Path.Combine(_testDirectory, "test.log");
        _options = new AdvanceFileLoggerOptions
        {
            FilePath = logFilePath,
            MaxArchivedFiles = 3,
            ArchiveDirectory = "archive"
        };

        _fileNameProvider = new FileNameProvider(logFilePath);
        _archiver = new FileArchiver(_fileNameProvider, _options);
    }

    [Fact]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new FileArchiver(_fileNameProvider, _options));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullFileNameProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileArchiver(null!, _options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileArchiver(_fileNameProvider, null!));
    }

    [Fact]
    public void ArchiveFile_WithExistingFile_MovesFileToArchive()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "test.log");
        File.WriteAllText(sourceFile, "Test content");
        var timestamp = new DateTime(2023, 12, 25, 14, 30, 45);

        // Act
        var result = _archiver.ArchiveFile(sourceFile, timestamp);

        // Assert
        result.Should().BeTrue();
        File.Exists(sourceFile).Should().BeFalse();

        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.Exists(archiveDir).Should().BeTrue();

        var archivedFile = Path.Combine(archiveDir, "test_20231225_143045.log");
        File.Exists(archivedFile).Should().BeTrue();
        File.ReadAllText(archivedFile).Should().Be("Test content");
    }

    [Fact]
    public void ArchiveFile_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.log");

        // Act
        var result = _archiver.ArchiveFile(nonExistentFile, DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ArchiveFile_WithInvalidSourcePath_ThrowsArgumentException(string sourcePath)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _archiver.ArchiveFile(sourcePath, DateTime.UtcNow));
    }

    [Fact]
    public void ArchiveCurrentFile_WithExistingFile_ArchivesSuccessfully()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "test.log");
        File.WriteAllText(sourceFile, "Current log content");

        // Act
        var result = _archiver.ArchiveCurrentFile(sourceFile);

        // Assert
        result.Should().BeTrue();
        File.Exists(sourceFile).Should().BeFalse();

        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.Exists(archiveDir).Should().BeTrue();
        Directory.GetFiles(archiveDir).Should().HaveCount(1);
    }

    [Fact]
    public void CleanupOldArchivedFiles_WithExcessFiles_RemovesOldest()
    {
        // Arrange
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        // Create more files than the retention limit
        var files = new[]
        {
            "test_20231220_100000.log",
            "test_20231221_100000.log",
            "test_20231222_100000.log",
            "test_20231223_100000.log",
            "test_20231224_100000.log"
        };

        foreach (var file in files)
        {
            var filePath = Path.Combine(archiveDir, file);
            File.WriteAllText(filePath, "content");
        }

        // Act
        _archiver.CleanupOldArchivedFiles();

        // Assert
        var remainingFiles = Directory.GetFiles(archiveDir);
        remainingFiles.Should().HaveCount(_options.MaxArchivedFiles);

        // Should keep the most recent files
        remainingFiles.Should().Contain(f => f.Contains("20231224"));
        remainingFiles.Should().Contain(f => f.Contains("20231223"));
        remainingFiles.Should().Contain(f => f.Contains("20231222"));
    }

    [Fact]
    public void CleanupOldArchivedFiles_WithinRetentionLimit_DoesNotRemoveFiles()
    {
        // Arrange
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        // Create fewer files than the retention limit
        var files = new[]
        {
            "test_20231222_100000.log",
            "test_20231223_100000.log"
        };

        foreach (var file in files)
        {
            var filePath = Path.Combine(archiveDir, file);
            File.WriteAllText(filePath, "content");
        }

        // Act
        _archiver.CleanupOldArchivedFiles();

        // Assert
        var remainingFiles = Directory.GetFiles(archiveDir);
        remainingFiles.Should().HaveCount(2);
    }

    [Fact]
    public void CleanupOldArchivedFiles_WithMaxArchivedFilesZero_DoesNothing()
    {
        // Arrange
        _options.MaxArchivedFiles = 0;
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        var filePath = Path.Combine(archiveDir, "test_20231225_100000.log");
        File.WriteAllText(filePath, "content");

        // Act
        _archiver.CleanupOldArchivedFiles();

        // Assert
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void GetArchivedFiles_WithExistingFiles_ReturnsFileInfo()
    {
        // Arrange
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        var fileName = "test_20231225_143045.log";
        var filePath = Path.Combine(archiveDir, fileName);
        File.WriteAllText(filePath, "test content");

        // Act
        var archivedFiles = _archiver.GetArchivedFiles(archiveDir).ToList();

        // Assert
        archivedFiles.Should().HaveCount(1);
        var fileInfo = archivedFiles.First();
        fileInfo.FileName.Should().Be(fileName);
        fileInfo.FilePath.Should().Be(filePath);
        fileInfo.Timestamp.Should().Be(new DateTime(2023, 12, 25, 14, 30, 45));
        fileInfo.Size.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetArchivedFiles_WithNonExistentDirectory_ReturnsEmpty()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        // Act
        var archivedFiles = _archiver.GetArchivedFiles(nonExistentDir);

        // Assert
        archivedFiles.Should().BeEmpty();
    }

    [Fact]
    public void GetTotalArchivedSize_WithMultipleFiles_ReturnsCorrectSize()
    {
        // Arrange
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        var content1 = "Content 1";
        var content2 = "Content 2 is longer";

        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_100000.log"), content1);
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_110000.log"), content2);

        var expectedSize = System.Text.Encoding.UTF8.GetByteCount(content1) +
                          System.Text.Encoding.UTF8.GetByteCount(content2);

        // Act
        var totalSize = _archiver.GetTotalArchivedSize();

        // Assert
        totalSize.Should().Be(expectedSize);
    }

    [Fact]
    public void GetArchivedFileCount_WithMultipleFiles_ReturnsCorrectCount()
    {
        // Arrange
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_100000.log"), "content1");
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_110000.log"), "content2");
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_120000.log"), "content3");

        // Act
        var count = _archiver.GetArchivedFileCount();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void CanArchiveMoreFiles_WithinLimit_ReturnsTrue()
    {
        // Arrange
        _options.MaxArchivedFiles = 5;
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        // Create fewer files than limit
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_100000.log"), "content");

        // Act
        var canArchive = _archiver.CanArchiveMoreFiles();

        // Assert
        canArchive.Should().BeTrue();
    }

    [Fact]
    public void CanArchiveMoreFiles_AtLimit_ReturnsFalse()
    {
        // Arrange
        _options.MaxArchivedFiles = 2;
        var archiveDir = Path.Combine(_testDirectory, "archive");
        Directory.CreateDirectory(archiveDir);

        // Create files at the limit
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_100000.log"), "content1");
        File.WriteAllText(Path.Combine(archiveDir, "test_20231225_110000.log"), "content2");

        // Act
        var canArchive = _archiver.CanArchiveMoreFiles();

        // Assert
        canArchive.Should().BeFalse();
    }

    [Fact]
    public void CanArchiveMoreFiles_WithUnlimitedArchiving_ReturnsTrue()
    {
        // Arrange
        _options.MaxArchivedFiles = 0; // Unlimited

        // Act
        var canArchive = _archiver.CanArchiveMoreFiles();

        // Assert
        canArchive.Should().BeTrue();
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