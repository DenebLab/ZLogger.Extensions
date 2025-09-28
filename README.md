# ZLogger Extensions

[![CI/CD](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Deneblab.ZLoggerExtensions)](https://www.nuget.org/packages/Deneblab.ZLoggerExtensions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Extensions and advanced features for [ZLogger](https://github.com/Cysharp/ZLogger), including advanced file logging with archiving, rotation, and enhanced console logging capabilities.

## Features Overview

- **🚀 Advanced File Logger**: Enterprise-grade file logging with automatic rolling, archiving, and retention policies
- **🎨 Console Logger with Colors**: ANSI color-coded console output with multiple formatting modes  
- **📦 Easy Integration**: Seamless integration with .NET's `ILoggingBuilder` infrastructure
- **⚙️ Flexible Configuration**: Support for both code-based and `appsettings.json` configuration
- **🔒 Thread-Safe**: All components are thread-safe for multi-threaded applications
- **🔧 External Process Support**: Files can be accessed by external tools during active logging
- **🌐 Cross-Platform**: Works on Windows, Linux, and macOS

## Available Loggers

This library provides the following logger implementations:

### 🚀 Advanced File Logger (`AdvanceFileLogger`)

Enterprise-grade file logging with advanced features:

**Key Features:**
- **Size-based Rolling**: Automatic file rotation based on configurable size limits
- **Automatic Archiving**: Move rolled files to organized archive directories
- **Retention Policies**: Configurable cleanup of old archived files
- **External Process Support**: Files can be read/deleted by external processes during logging
- **Thread-Safe Operations**: Safe for concurrent use in multi-threaded applications
- **Cross-Platform**: Works on Windows, Linux, and macOS

**Usage:**
```csharp
// Basic usage
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger("logs/app.log");
});

// Advanced configuration
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.FilePath = "logs/myapp.log";
        options.MaxBytes = 100 * 1024 * 1024; // 100MB
        options.MaxArchivedFiles = 10;
        options.ArchiveDirectory = "archive";
        options.AllowExternalAccess = true;
    });
});
```

### 🎨 Console Logger with Colors (`ConsoleWithColorsLogger`)

Enhanced console logging with color-coded log levels and multiple formatting options:

**Key Features:**
- **Color-Coded Log Levels**: Visual distinction for different log severities
- **Multiple Verbosity Modes**: Choose from 3 different formatting styles
- **ANSI Escape Code Support**: Full terminal color support
- **UTF-8 Output**: Proper encoding for international characters
- **Customizable Formatting**: Flexible timestamp and category display options

**Verbosity Modes:**
1. **`TimeOnlyLocalLogLevel`**: Shows local time and colored log levels
   ```
   14:30:15 [INFO] Application started
   14:30:16 [WARN] Low disk space
   14:30:17 [ERROR] Connection failed
   ```

2. **`DataTimeUtcLogLevelCategory`**: Full UTC datetime with category information
   ```
   2023-12-25T14:30:15.123Z|INFO|MyApp.Services.UserService|User login successful
   2023-12-25T14:30:16.456Z|WARN|MyApp.Data.Repository|Cache miss detected
   ```

3. **`LogLevelLineColor`**: Entire log line colored by severity level
   ```
   [INFO] Application ready to serve requests
   [WARN] Performance threshold exceeded
   [ERROR] Database connection timeout
   ```

**Usage:**
```csharp
// Basic usage with default settings
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors();
});

// Custom verbosity mode
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors(options =>
    {
        options.LogVerbosity = LogVerbosity.DataTimeUtcLogLevelCategory;
    });
});
```

**Color Scheme:**
- **Trace**: Light gray (`\u001b[38;2;200;200;200m`)
- **Debug**: Default terminal color (no coloring)
- **Information**: Cyan (`\u001B[36m`)
- **Warning**: Yellow (`\u001b[33m`)
- **Error/Critical**: Red (`\u001b[31m`)

### 📋 Logger Comparison

| Feature | Advanced File Logger | Console with Colors |
|---------|---------------------|-------------------|
| **Output Target** | Files on disk | Console/Terminal |
| **Rolling/Rotation** | ✅ Size-based | ❌ N/A |
| **Archiving** | ✅ Automatic | ❌ N/A |
| **Colors** | ❌ Plain text | ✅ ANSI colors |
| **Formatting Options** | ✅ Structured | ✅ Multiple modes |
| **External Access** | ✅ Configurable | ❌ N/A |
| **Performance** | ✅ High (buffered) | ✅ High (direct) |
| **Thread Safety** | ✅ Yes | ✅ Yes |
| **Cross-Platform** | ✅ Yes | ✅ Yes |

### 🔧 Combined Usage

You can use both loggers together for comprehensive logging:

```csharp
services.AddLogging(builder =>
{
    // File logging for persistence
    builder.AddAdvanceFileLogger(options =>
    {
        options.FilePath = "logs/app.log";
        options.MaxBytes = 50 * 1024 * 1024; // 50MB
        options.MaxArchivedFiles = 7;
    });
    
    // Console logging for development
    builder.AddZLoggerConsoleWithColors(options =>
    {
        options.LogVerbosity = LogVerbosity.TimeOnlyLocalLogLevel;
    });
});
```

This setup provides:
- **Persistent logging** to files with automatic rotation and archiving
- **Real-time console feedback** with color-coded severity levels
- **Development-friendly** output for debugging
- **Production-ready** file management for monitoring

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Deneblab.ZLoggerExtensions
```

Or via Package Manager Console:

```powershell
Install-Package Deneblab.ZLoggerExtensions
```

## Quick Start

### Advanced File Logger

```csharp
using Microsoft.Extensions.Logging;
using ZLoggerExtensions.AdvanceFileLogger;

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

### Console Logger with Colors

```csharp
using Microsoft.Extensions.Logging;
using Deneblab.ZLoggerExtensions.ConsoleWithColorsLogger;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors();
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Colorful console logging!");
logger.LogWarning("This appears in yellow");
logger.LogError("This appears in red");
```

## Quick Reference

### Logger Methods

| Logger Type | Extension Method | Primary Use Case |
|-------------|------------------|------------------|
| **Advanced File** | `AddAdvanceFileLogger()` | Production logging with file management |
| **Console Colors** | `AddZLoggerConsoleWithColors()` | Development and debugging with visual feedback |

### Common Configuration Patterns

```csharp
// Production setup: File + Console
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger("logs/app.log", maxBytes: 100_000_000);
    builder.AddZLoggerConsoleWithColors();
});

// Development setup: Console only with detailed output
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors(opts => 
        opts.LogVerbosity = LogVerbosity.DataTimeUtcLogLevelCategory);
});

// High-volume production: File with aggressive archiving
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(opts =>
    {
        opts.FilePath = "logs/app.log";
        opts.MaxBytes = 50_000_000; // 50MB
        opts.MaxArchivedFiles = 20;
        opts.AutoFlush = false; // Better performance
    });
});
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

## Advanced File Logger

The Advanced File Logger provides enterprise-grade file logging capabilities with the following features:

### Key Features

- **Size-based Rolling**: Configurable file size limit (default: 50MB)
- **Automatic Archiving**: Move rolled files to archive subdirectory
- **Retention Policy**: Configurable number of archived files to keep (default: 7)
- **External Process Support**: Allow reading/deleting log files during logging
- **ZLogger Integration**: Full compatibility with ZLogger architecture

### Configuration Options

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

### File Naming Convention

The AdvanceFileLogger follows ZLogger's rolling file naming convention:

- Current file: `app.log`
- Rolled files: `app_20231225_143045.log`
- Archived files: `archive/app_20231225_143045.log`

## Console Logger with Colors

Enhanced console logging with color support for different log levels, providing better visual distinction in console applications.

## Architecture

The library consists of several well-designed components:

### Advanced File Logger Components

- **AdvanceFileLoggerProvider**: Main provider implementing `ILoggerProvider`
- **AdvanceFileWriter**: Core file writing with rolling logic
- **FileNameProvider**: Handles ZLogger-style file naming
- **FileArchiver**: Manages archiving and retention policy
- **AdvanceFileLoggerOptions**: Configuration class

## Requirements

- **.NET 8.0** or later
- **ZLogger 2.5.10** or later

## Dependencies

- **ZLogger**: High-performance structured logging library
- **Microsoft.Extensions.Logging**: .NET logging abstractions
- **Microsoft.Extensions.DependencyInjection**: Dependency injection framework

## Performance

The library is designed for high performance:

- Configurable buffer size for optimal I/O performance
- Optional auto-flush for immediate persistence
- Efficient file size monitoring
- Minimal memory allocation during logging
- Thread-safe operations without blocking

## Error Handling

The logger is designed to be resilient:

- Failed writes attempt recovery by reopening files
- Archive failures don't stop logging
- Invalid configurations are validated at startup
- Graceful degradation when external processes interfere

## Testing

The library includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test coverage includes:
- Basic logging functionality
- File rolling behavior
- Archive retention policy
- Configuration validation
- Provider integration
- External file access scenarios
- Thread safety verification

## Contributing

We welcome contributions! Please see our contributing guidelines for details on how to:

- Report bugs
- Suggest enhancements
- Submit pull requests
- Follow coding standards

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [ZLogger](https://github.com/Cysharp/ZLogger) - The foundational logging library this extends
- [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) - .NET logging infrastructure

## Support

- 📖 [Documentation](https://github.com/DenebLab/ZLogger.Extensions/wiki)
- 🐛 [Issues](https://github.com/DenebLab/ZLogger.Extensions/issues)
- 💬 [Discussions](https://github.com/DenebLab/ZLogger.Extensions/discussions)

---

Made with ❤️ by [DenebLab](https://github.com/DenebLab)