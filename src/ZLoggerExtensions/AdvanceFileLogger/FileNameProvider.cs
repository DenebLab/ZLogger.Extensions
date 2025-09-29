using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Provides file naming functionality following ZLogger rolling file conventions with date+index pattern.
/// </summary>
public class FileNameProvider
{
    private readonly Func<DateTime, int, string> _fileNameProvider;
    private readonly string _baseDirectory;

    /// <summary>
    /// Initializes a new instance of the FileNameProvider class.
    /// </summary>
    /// <param name="fileNameProvider">The function that generates file names based on date and index.</param>
    public FileNameProvider(Func<DateTime, int, string> fileNameProvider)
    {
        _fileNameProvider = fileNameProvider ?? throw new ArgumentNullException(nameof(fileNameProvider));

        // Extract base directory from a sample file name to determine where logs should be stored
        var samplePath = _fileNameProvider(DateTime.UtcNow, 0);
        _baseDirectory = Path.GetDirectoryName(samplePath) ?? string.Empty;
    }

    /// <summary>
    /// Gets the current log file path for the specified date and index.
    /// </summary>
    /// <param name="date">The date for the log file.</param>
    /// <param name="index">The index for the log file.</param>
    /// <returns>The current log file path.</returns>
    public string GetCurrentFilePath(DateTime date, int index)
    {
        return _fileNameProvider(date, index);
    }

    /// <summary>
    /// Gets the current log file path for today with index 0.
    /// </summary>
    /// <returns>The current log file path.</returns>
    public string GetCurrentFilePath()
    {
        return GetCurrentFilePath(DateTime.UtcNow.Date, 0);
    }

    /// <summary>
    /// Gets the next file path when rolling due to size limit.
    /// </summary>
    /// <param name="currentFilePath">The current file path.</param>
    /// <returns>The next file path with incremented index.</returns>
    public string GetNextFilePath(string currentFilePath)
    {
        if (TryParseFilePath(currentFilePath, out var date, out var index))
        {
            return GetCurrentFilePath(date, index + 1);
        }

        // Fallback: use current date with index 1
        return GetCurrentFilePath(DateTime.UtcNow.Date, 1);
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

        return Path.Combine(_baseDirectory, archiveDirectory);
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

        var archivePath = GetArchiveDirectoryPath(archiveDirectory);
        return Path.Combine(archivePath, originalFileName);
    }

    /// <summary>
    /// Gets the directory path for log files.
    /// </summary>
    /// <returns>The directory path.</returns>
    public string GetLogDirectory()
    {
        return _baseDirectory;
    }

    /// <summary>
    /// Checks if a file name matches the rolling pattern for this logger.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file matches the rolling pattern.</returns>
    public bool IsRolledFile(string fileName)
    {
        return TryParseFilePath(fileName, out _, out _);
    }

    /// <summary>
    /// Checks if a file path matches the current date pattern.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if the file matches the current date.</returns>
    public bool IsCurrentDateFile(string filePath)
    {
        return IsCurrentDateFile(filePath, DateTime.UtcNow.Date);
    }

    /// <summary>
    /// Checks if a file path matches the specified date pattern.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="date">The date to check against.</param>
    /// <returns>True if the file matches the specified date.</returns>
    public bool IsCurrentDateFile(string filePath, DateTime date)
    {
        if (TryParseFilePath(filePath, out var fileDate, out _))
        {
            return fileDate.Date == date.Date;
        }
        return false;
    }

    /// <summary>
    /// Gets all log files that match the current naming pattern.
    /// </summary>
    /// <returns>Array of file paths that match the pattern.</returns>
    public string[] GetAllLogFiles()
    {
        if (string.IsNullOrEmpty(_baseDirectory) || !Directory.Exists(_baseDirectory))
            return Array.Empty<string>();

        return Directory.GetFiles(_baseDirectory, "*")
            .Where(IsRolledFile)
            .OrderBy(f => f)
            .ToArray();
    }

    /// <summary>
    /// Gets log files older than the specified number of days.
    /// </summary>
    /// <param name="daysToKeep">Number of days to keep.</param>
    /// <returns>Array of file paths older than the specified days.</returns>
    public string[] GetOldLogFiles(int daysToKeep)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysToKeep);

        return GetAllLogFiles()
            .Where(filePath =>
            {
                if (TryParseFilePath(filePath, out var fileDate, out _))
                {
                    return fileDate.Date < cutoffDate;
                }
                return false;
            })
            .ToArray();
    }

    /// <summary>
    /// Tries to parse a file path to extract date and index information.
    /// </summary>
    /// <param name="filePath">The file path to parse.</param>
    /// <param name="date">The extracted date.</param>
    /// <param name="index">The extracted index.</param>
    /// <returns>True if parsing was successful.</returns>
    public bool TryParseFilePath(string filePath, out DateTime date, out int index)
    {
        date = default;
        index = 0;

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            // Generate a sample file to understand the pattern
            var samplePath = _fileNameProvider(new DateTime(2025, 09, 29), 42);
            var sampleFileName = Path.GetFileName(samplePath);

            // Create a regex pattern from the sample
            // Replace the date part with a regex group and index with another group
            var datePattern = @"(\d{4}-\d{2}-\d{2})";
            var indexPattern = @"(\d+)";

            // Build pattern by replacing known values in sample with regex groups
            var pattern = sampleFileName
                .Replace("2025-09-29", datePattern)
                .Replace("42", indexPattern);

            // Escape other regex special characters
            pattern = Regex.Escape(pattern)
                .Replace(Regex.Escape(datePattern), datePattern)
                .Replace(Regex.Escape(indexPattern), indexPattern);

            var fileName = Path.GetFileName(filePath);
            var match = Regex.Match(fileName, pattern);

            if (match.Success && match.Groups.Count >= 3)
            {
                if (DateTime.TryParseExact(match.Groups[1].Value, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out date) &&
                    int.TryParse(match.Groups[2].Value, out index))
                {
                    return true;
                }
            }
        }
        catch
        {
            // If pattern matching fails, return false
        }

        return false;
    }

    /// <summary>
    /// Gets the highest index for files of the current date.
    /// </summary>
    /// <param name="date">The date to check for.</param>
    /// <returns>The highest index found, or -1 if no files exist for that date.</returns>
    public int GetHighestIndexForDate(DateTime date)
    {
        var maxIndex = -1;
        var allFiles = GetAllLogFiles();

        foreach (var filePath in allFiles)
        {
            if (TryParseFilePath(filePath, out var fileDate, out var index) &&
                fileDate.Date == date.Date &&
                index > maxIndex)
            {
                maxIndex = index;
            }
        }

        return maxIndex;
    }
}