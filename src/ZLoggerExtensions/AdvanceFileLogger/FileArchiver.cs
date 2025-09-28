using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Handles archiving of log files and retention policy management.
/// </summary>
public class FileArchiver
{
    private readonly FileNameProvider _fileNameProvider;
    private readonly AdvanceFileLoggerOptions _options;

    /// <summary>
    /// Initializes a new instance of the FileArchiver class.
    /// </summary>
    /// <param name="fileNameProvider">The file name provider.</param>
    /// <param name="options">The logger options.</param>
    public FileArchiver(FileNameProvider fileNameProvider, AdvanceFileLoggerOptions options)
    {
        _fileNameProvider = fileNameProvider ?? throw new ArgumentNullException(nameof(fileNameProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Archives a file by moving it to the archive directory.
    /// </summary>
    /// <param name="sourceFilePath">The source file path to archive.</param>
    /// <param name="timestamp">The timestamp for the archived file name.</param>
    /// <returns>True if archiving was successful, false otherwise.</returns>
    public bool ArchiveFile(string sourceFilePath, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFilePath));

        if (!File.Exists(sourceFilePath))
            return false;

        try
        {
            // Ensure archive directory exists
            var archiveDirectory = _fileNameProvider.GetArchiveDirectoryPath(_options.ArchiveDirectory);
            if (_options.CreateDirectories && !Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            // Generate rolled file name with timestamp
            var sourceFileName = Path.GetFileName(sourceFilePath);
            var baseFileName = Path.GetFileNameWithoutExtension(sourceFileName);
            var extension = Path.GetExtension(sourceFileName);

            // Create timestamped archive file name
            var timestampSuffix = timestamp.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            var archivedFileName = $"{baseFileName}_{timestampSuffix}{extension}";
            var destinationPath = Path.Combine(archiveDirectory, archivedFileName);

            // Move file to archive
            File.Move(sourceFilePath, destinationPath);

            // Clean up old archived files
            CleanupOldArchivedFiles();

            return true;
        }
        catch (Exception)
        {
            // Log the exception if needed, but don't throw to avoid breaking logging
            return false;
        }
    }

    /// <summary>
    /// Archives the current log file by creating a rolled version.
    /// </summary>
    /// <param name="currentFilePath">The current log file path.</param>
    /// <returns>True if archiving was successful, false otherwise.</returns>
    public bool ArchiveCurrentFile(string currentFilePath)
    {
        return ArchiveFile(currentFilePath, DateTime.UtcNow);
    }

    /// <summary>
    /// Cleans up old archived files based on the retention policy.
    /// </summary>
    public void CleanupOldArchivedFiles()
    {
        if (_options.MaxArchivedFiles <= 0)
            return;

        try
        {
            var archiveDirectory = _fileNameProvider.GetArchiveDirectoryPath(_options.ArchiveDirectory);
            if (!Directory.Exists(archiveDirectory))
                return;

            var archivedFiles = GetArchivedFiles(archiveDirectory)
                .OrderByDescending(f => f.Timestamp)
                .ToList();

            // Keep only the most recent files up to MaxArchivedFiles
            var filesToDelete = archivedFiles.Skip(_options.MaxArchivedFiles);

            foreach (var fileInfo in filesToDelete)
            {
                try
                {
                    File.Delete(fileInfo.FilePath);
                }
                catch (Exception)
                {
                    // Continue with other files if one fails
                }
            }
        }
        catch (Exception)
        {
            // Don't break logging if cleanup fails
        }
    }

    /// <summary>
    /// Gets information about archived files in the archive directory.
    /// </summary>
    /// <param name="archiveDirectory">The archive directory path.</param>
    /// <returns>A collection of archived file information.</returns>
    public IEnumerable<ArchivedFileInfo> GetArchivedFiles(string archiveDirectory)
    {
        if (!Directory.Exists(archiveDirectory))
            return Enumerable.Empty<ArchivedFileInfo>();

        try
        {
            return Directory.GetFiles(archiveDirectory)
                .Where(file => _fileNameProvider.IsRolledFile(Path.GetFileName(file)))
                .Select(file =>
                {
                    var fileName = Path.GetFileName(file);
                    var timestamp = _fileNameProvider.ExtractTimestampFromRolledFile(fileName);
                    return new ArchivedFileInfo
                    {
                        FilePath = file,
                        FileName = fileName,
                        Timestamp = timestamp ?? File.GetCreationTimeUtc(file),
                        Size = new FileInfo(file).Length
                    };
                })
                .ToList();
        }
        catch (Exception)
        {
            return Enumerable.Empty<ArchivedFileInfo>();
        }
    }

    /// <summary>
    /// Gets the total size of all archived files.
    /// </summary>
    /// <returns>The total size in bytes.</returns>
    public long GetTotalArchivedSize()
    {
        var archiveDirectory = _fileNameProvider.GetArchiveDirectoryPath(_options.ArchiveDirectory);
        return GetArchivedFiles(archiveDirectory).Sum(f => f.Size);
    }

    /// <summary>
    /// Gets the count of archived files.
    /// </summary>
    /// <returns>The count of archived files.</returns>
    public int GetArchivedFileCount()
    {
        var archiveDirectory = _fileNameProvider.GetArchiveDirectoryPath(_options.ArchiveDirectory);
        return GetArchivedFiles(archiveDirectory).Count();
    }

    /// <summary>
    /// Checks if the archive directory has space for more files.
    /// </summary>
    /// <returns>True if more files can be archived, false if at limit.</returns>
    public bool CanArchiveMoreFiles()
    {
        if (_options.MaxArchivedFiles <= 0)
            return true;

        return GetArchivedFileCount() < _options.MaxArchivedFiles;
    }
}

/// <summary>
/// Information about an archived file.
/// </summary>
public class ArchivedFileInfo
{
    /// <summary>
    /// Gets or sets the full file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp extracted from the file name or file creation time.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long Size { get; set; }
}