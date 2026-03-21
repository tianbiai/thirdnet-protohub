using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ProtoHub.Identity.API
{
    /// <summary>
    /// 程序启动
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = BuildWebHost(args);
            await host.RunAsync();
        }


        public static IHost BuildWebHost(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                    })
                    .UseWindowsService()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>().UseKestrel();
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    })
                    .UseNLog();

            return builder.Build();
        }
    }
}
