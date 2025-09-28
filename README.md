# ZLogger Extensions

[![CI/CD](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Deneblab.ZLoggerExtensions)](https://www.nuget.org/packages/Deneblab.ZLoggerExtensions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Extensions and advanced features for [ZLogger](https://github.com/Cysharp/ZLogger), including advanced file logging with archiving, rotation, and enhanced console logging capabilities.

## Features

- **🚀 Advanced File Logger**: High-performance file logging with size-based rolling, automatic archiving, and retention policies
- **🎨 Console Logger with Colors**: Enhanced console logging with color support for different log levels
- **📦 Easy Integration**: Seamless integration with .NET's logging infrastructure via `ILoggingBuilder`
- **⚙️ Flexible Configuration**: Support for both code-based and `appsettings.json` configuration
- **🔒 Thread-Safe**: All components are thread-safe for multi-threaded applications
- **🔧 External Process Support**: Allow reading/deleting log files during active logging

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