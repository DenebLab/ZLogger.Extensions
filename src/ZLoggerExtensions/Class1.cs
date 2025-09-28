
using ZLogger;
using Microsoft.Extensions.Logging;
namespace ZLoggerExtensions;

public class Class1
{

    public ZLoggerOptions MyClass()
    {

        using var factory = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);

            // Add ZLogger provider to ILoggingBuilder
            logging.AddZLoggerConsole();

            // Output Structured Logging, setup options
            // logging.AddZLoggerConsole(options => options.UseJsonFormatter());
        });
        var aa = new ZLogger.ZLoggerOptions();
        return aa;
    }
}