using System.IO;
using System.Threading.Tasks;
using App.Commands;
using Lib.Builders;
using Lib.Handlers.Database;
using Lib.Handlers.Disk;
using Lib.Handlers.Password;
using Lib.Handlers.Process;
using Lib.Handlers.Reporting;
using Lib.Handlers.Serialization;
using Lib.Handlers.WebServer;
using Lib.Helpers;
using Lib.Models;
using Lib.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace App
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunCommandLineApplicationAsync<MainSiteCommand>(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder.AddConsoleLogger();
                    loggingBuilder.AddNonGenericLogger();
                    loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddTransient<MainSiteCommand>();
                    services.AddTransient<ListSitesCommand>();
                    services.AddTransient<CreateSiteCommand>();
                    services.AddTransient<DeleteSiteCommand>();
                    services.AddTransient<IXmlHelper, XmlHelper>();
                    services.AddTransient<ISiteHelper, SiteHelper>();
                    services.AddTransient<IDiskHandler, DiskHandler>();
                    services.AddTransient<IConsoleHelper, ConsoleHelper>();
                    services.AddTransient<IRequestBuilder, RequestBuilder>();
                    services.AddTransient<IProcessHandler, ProcessHandler>();
                    services.AddTransient<IPasswordHandler, PasswordHandler>();
                    services.AddTransient<IDatabaseHandler, DatabaseHandler>();
                    services.AddTransient<IWebServerHandler, WebServerHandler>();
                    services.AddTransient<IReportingHandler, ReportingHandler>();
                    services.AddTransient<ICreateSiteValidator, CreateSiteValidator>();
                    services.AddTransient<IDeleteSiteValidator, DeleteSiteValidator>();
                    services.AddTransient<ISerializationHandler, SerializationHandler>();
                    services.AddTransient<IDetailsReportingHandler, DetailsReportingHandler>();
                });

        private static void AddConsoleLogger(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.IncludeScopes = true;
                options.TimestampFormat = "[HH:mm:ss:fff] ";
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            });
        }

        private static void AddNonGenericLogger(this ILoggingBuilder loggingBuilder)
        {
            var services = loggingBuilder.Services;
            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger(Settings.ApplicationName);
            });
        }
    }
}
