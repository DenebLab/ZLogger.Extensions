# AdvanceFileLogger

A high-performance file logging provider for ZLogger extensions with advanced features including size-based rolling, automatic archiving, and external process support.

## Features

- **Size-based rolling**: Configurable file size limit (default: 50MB)
- **Automatic archiving**: Move rolled files to archive subdirectory
- **Retention policy**: Configurable number of archived files to keep (default: 7)
- **External process support**: Allow reading/deleting log files during logging
- **ZLogger integration**: Full compatibility with ZLogger architecture
- **ILoggingBuilder support**: Easy integration with .NET logging infrastructure

## Quick Start

### Basic Usage

```csharp
using Microsoft.Extensions.Logging;
using ZLoggerExtensions.AdvanceFileLogger;

// Using ILoggingBuilder
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger("logs/app.log");
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started");
logger.LogWarning("This is a warning");
logger.LogError("This is an error");
```

### Advanced Configuration

```csharp
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.FilePath = "logs/myapp.log";
        options.MaxBytes = 100 * 1024 * 1024; // 100MB
        options.MaxArchivedFiles = 10;
        options.ArchiveDirectory = "archive";
        options.AllowExternalAccess = true;
        options.AutoFlush = true;
    });
});
```

### Configuration via appsettings.json

```json
{
  "Logging": {
    "AdvanceFile": {
      "FilePath": "logs/app.log",
      "MaxBytes": 52428800,
      "MaxArchivedFiles": 7,
      "ArchiveDirectory": "archive",
      "AllowExternalAccess": true,
      "AutoFlush": true
    }
  }
}
```

## Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FilePath` | string | "logs/app.log" | Base file path for log files |
| `MaxBytes` | long | 50MB | File size limit for rolling (0 disables) |
| `MaxArchivedFiles` | int | 7 | Maximum archived files to retain |
| `ArchiveDirectory` | string | "archive" | Archive directory name |
| `AllowExternalAccess` | bool | true | Allow external process file access |
| `AutoFlush` | bool | true | Flush after each write |
| `CreateDirectories` | bool | true | Create directories if missing |
| `Encoding` | Encoding | UTF8 | File encoding |
| `BufferSize` | int | 4096 | File buffer size |

## File Naming Convention

The AdvanceFileLogger follows ZLogger's rolling file naming convention:

- Current file: `app.log`
- Rolled files: `app_20231225_143045.log`
- Archived files: `archive/app_20231225_143045.log`

## Architecture

The AdvanceFileLogger consists of several components:

- **AdvanceFileLoggerProvider**: Main provider implementing ILoggerProvider
- **AdvanceFileWriter**: Core file writing with rolling logic
- **FileNameProvider**: Handles ZLogger-style file naming
- **FileArchiver**: Manages archiving and retention policy
- **AdvanceFileLoggerOptions**: Configuration class

## External Process Support

When `AllowExternalAccess` is enabled, the logger uses file sharing modes that allow:

- Reading log files while logging is active
- Deleting/moving log files by external processes
- Log rotation by external tools

## Thread Safety

All components are thread-safe and can be used in multi-threaded applications without additional synchronization.

## Error Handling

The logger is designed to be resilient:

- Failed writes attempt recovery by reopening files
- Archive failures don't stop logging
- Invalid configurations are validated at startup

## Performance

- Configurable buffer size for optimal I/O performance
- Optional auto-flush for immediate persistence
- Efficient file size monitoring
- Minimal memory allocation during logging

## Testing

Comprehensive test coverage includes:

- Basic logging functionality
- File rolling behavior
- Archive retention policy
- Configuration validation
- Provider integration
- External file access
- Thread safety

Run tests with:
```bash
dotnet test
```