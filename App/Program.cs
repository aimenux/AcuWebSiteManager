﻿using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using App.Commands;
using Lib.Builders;
using Lib.Handlers.Database;
using Lib.Handlers.Disk;
using Lib.Handlers.Password;
using Lib.Handlers.Process;
using Lib.Handlers.Reporting;
using Lib.Handlers.Serialization;
using Lib.Handlers.Site;
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
                    config.SetBasePath(GetDirectoryPath());
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
                    services.AddTransient<SwitchDbCommand>();
                    services.AddTransient<ImportDbCommand>();
                    services.AddTransient<ExportDbCommand>();
                    services.AddTransient<IXmlHelper, XmlHelper>();
                    services.AddTransient<ISiteHelper, SiteHelper>();
                    services.AddTransient<IFileHelper, FileHelper>();
                    services.AddTransient<ISiteHandler, SiteHandler>();
                    services.AddTransient<IDiskHandler, DiskHandler>();
                    services.AddTransient<IConsoleHelper, ConsoleHelper>();
                    services.AddTransient<IDatabaseHelper, DatabaseHelper>();
                    services.AddTransient<IRequestBuilder, RequestBuilder>();
                    services.AddTransient<IProcessHandler, ProcessHandler>();
                    services.AddTransient<IPasswordHandler, PasswordHandler>();
                    services.AddTransient<IDatabaseHandler, DatabaseHandler>();
                    services.AddTransient<IWebServerHandler, WebServerHandler>();
                    services.AddTransient<IReportingHandler, ReportingHandler>();
                    services.AddTransient<ISwitchDbValidator, SwitchDbValidator>();
                    services.AddTransient<IImportDbValidator, ImportDbValidator>();
                    services.AddTransient<IExportDbValidator, ExportDbValidator>();
                    services.AddTransient<ICreateSiteValidator, CreateSiteValidator>();
                    services.AddTransient<IDeleteSiteValidator, DeleteSiteValidator>();
                    services.AddTransient<ISerializationHandler, SerializationHandler>();
                    services.AddTransient<IImportDatabaseHandler, ImportDatabaseHandler>();
                    services.AddTransient<IExportDatabaseHandler, ExportDatabaseHandler>();
                    services.AddTransient<IDetailsReportingHandler, DetailsReportingHandler>();
                    services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
                });

        public static string GetSettingFilePath() => Path.GetFullPath(Path.Combine(GetDirectoryPath(), @"appsettings.json"));

        private static void AddConsoleLogger(this ILoggingBuilder loggingBuilder)
        {
            if (!File.Exists(GetSettingFilePath()))
            {
                loggingBuilder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.IncludeScopes = true;
                    options.UseUtcTimestamp = true;
                    options.TimestampFormat = "[HH:mm:ss:fff] ";
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                });
            }
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

        private static string GetDirectoryPath()
        {
            try
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
}
