using Serilog;
using Serilog.Core;

namespace Monitoring;


public static class MonitorService
{
    public static Serilog.ILogger Log => Serilog.Log.Logger;

    static MonitorService()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();
    }
}
