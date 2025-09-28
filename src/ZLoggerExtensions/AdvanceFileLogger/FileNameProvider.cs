using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Provides file naming functionality following ZLogger rolling file conventions.
/// </summary>
public class FileNameProvider
{
    private readonly string _baseFilePath;
    private readonly string _directory;
    private readonly string _fileNameWithoutExtension;
    private readonly string _extension;

    /// <summary>
    /// Initializes a new instance of the FileNameProvider class.
    /// </summary>
    /// <param name="baseFilePath">The base file path for log files.</param>
    public FileNameProvider(string baseFilePath)
    {
        _baseFilePath = baseFilePath ?? throw new ArgumentNullException(nameof(baseFilePath));

        _directory = Path.GetDirectoryName(_baseFilePath) ?? string.Empty;
        _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_baseFilePath);
        _extension = Path.GetExtension(_baseFilePath);

        if (string.IsNullOrEmpty(_extension))
        {
            _extension = ".log";
        }
    }

    /// <summary>
    /// Gets the current log file path.
    /// </summary>
    /// <returns>The current log file path.</returns>
    public string GetCurrentFilePath()
    {
        return _baseFilePath;
    }

    /// <summary>
    /// Gets a rolled file path with timestamp suffix.
    /// </summary>
    /// <param name="timestamp">The timestamp for the rolled file.</param>
    /// <returns>The rolled file path.</returns>
    public string GetRolledFilePath(DateTime timestamp)
    {
        var timestampSuffix = timestamp.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var rolledFileName = $"{_fileNameWithoutExtension}_{timestampSuffix}{_extension}";

        return Path.Combine(_directory, rolledFileName);
    }

    /// <summary>
    /// Gets an archived file path in the archive directory.
    /// </summary>
    /// <param name="originalFileName">The original file name to archive.</param>
    /// <param name="archiveDirectory">The archive directory name.</param>
    /// <returns>The archived file path.</returns>
    public string GetArchivedFilePath(string originalFileName, string archiveDirectory)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name cannot be null or empty.", nameof(originalFileName));

        if (string.IsNullOrWhiteSpace(archiveDirectory))
            throw new ArgumentException("Archive directory cannot be null or empty.", nameof(archiveDirectory));

        var archivePath = Path.Combine(_directory, archiveDirectory);
        return Path.Combine(archivePath, originalFileName);
    }

    /// <summary>
    /// Gets the archive directory path.
    /// </summary>
    /// <param name="archiveDirectory">The archive directory name.</param>
    /// <returns>The full archive directory path.</returns>
    public string GetArchiveDirectoryPath(string archiveDirectory)
    {
        if (string.IsNullOrWhiteSpace(archiveDirectory))
            throw new ArgumentException("Archive directory cannot be null or empty.", nameof(archiveDirectory));

        return Path.Combine(_directory, archiveDirectory);
    }

    /// <summary>
    /// Gets the directory path for log files.
    /// </summary>
    /// <returns>The directory path.</returns>
    public string GetLogDirectory()
    {
        return _directory;
    }

    /// <summary>
    /// Checks if a file name matches the rolling pattern for this logger.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file matches the rolling pattern.</returns>
    public bool IsRolledFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        // Check if extension matches
        if (!string.Equals(extension, _extension, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if it starts with our base file name followed by underscore and timestamp
        var expectedPrefix = _fileNameWithoutExtension + "_";
        if (!fileNameWithoutExt.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check if the suffix looks like a timestamp (yyyyMMdd_HHmmss)
        var timestampPart = fileNameWithoutExt.Substring(expectedPrefix.Length);
        return timestampPart.Length == 15 &&
               timestampPart[8] == '_' &&
               IsNumeric(timestampPart.Substring(0, 8)) &&
               IsNumeric(timestampPart.Substring(9));
    }

    /// <summary>
    /// Extracts the timestamp from a rolled file name.
    /// </summary>
    /// <param name="fileName">The rolled file name.</param>
    /// <returns>The timestamp if extraction successful, null otherwise.</returns>
    public DateTime? ExtractTimestampFromRolledFile(string fileName)
    {
        if (!IsRolledFile(fileName))
            return null;

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var expectedPrefix = _fileNameWithoutExtension + "_";
        var timestampPart = fileNameWithoutExt.Substring(expectedPrefix.Length);

        if (DateTime.TryParseExact(timestampPart, "yyyyMMdd_HHmmss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
        {
            return timestamp;
        }

        return null;
    }

    private static bool IsNumeric(string str)
    {
        return !string.IsNullOrEmpty(str) && str.All(char.IsDigit);
    }
}