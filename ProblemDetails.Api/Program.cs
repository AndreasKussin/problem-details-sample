using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;


namespace ProblemDetails.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Running ProblemDetails API on the host now..");
            host.Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, options) =>
                {
                    // Delete all logging providers first
                    options.ClearProviders();

                    // Add additional provider for local debugging
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        options.AddDebug();
                        options.AddConsole();
                    }

                    // Read log level from configuration
                    var logLevel = GetLogLevel(hostingContext.Configuration["LogLevel"]);

                    // Add application insights logging provider.
                    // Providing an instrumentation key here is required if you're using
                    // standalone package Microsoft.Extensions.Logging.ApplicationInsights
                    // or if you want to capture logs from early in the application startup 
                    // pipeline from Startup.cs or Program.cs itself.
                    options.AddApplicationInsights(hostingContext.Configuration["AppInsightsKey"]);

                    // Optional: Apply filters to control what logs are sent to Application Insights.
                    // The following configures LogLevel for all categories.
                    options.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);

                    // Adding the filter below to ensure logs from Program.cs is sent to ApplicationInsights.
                    options.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, logLevel);

                    // Adding the filter below to ensure logs of from Startup.cs is sent to ApplicationInsights.
                    options.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, logLevel);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // Remove default ASP.NET header
                        options.AddServerHeader = false;
                    })
                    .UseStartup<Startup>();
                });


        private static LogLevel GetLogLevel(string logLevel)
        {
            return logLevel switch
            {
                "trace" => LogLevel.Trace,
                "debug" => LogLevel.Debug,
                "info" => LogLevel.Information,
                "warn" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "critical" => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }
    }
}
