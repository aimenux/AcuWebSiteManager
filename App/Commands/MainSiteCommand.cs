using Lib.Helpers;
using Lib.Models;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands
{
    [Command(Name = "MainSite", FullName = "Manage Acumatica Sites", Description = "Manage Acumatica Site (database, files).")]
    [Subcommand(typeof(CreateSiteCommand), typeof(DeleteSiteCommand), typeof(ListSitesCommand))]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [SuppressDefaultHelpOption]
    public class MainSiteCommand
    {
        private readonly IConsoleHelper _consoleHelper;

        public MainSiteCommand(IConsoleHelper consoleHelper)
        {
            _consoleHelper = consoleHelper;
        }

        [Option("-h|--help", "Show help information.", CommandOptionType.NoValue)]
        public bool ShowHelp { get; set; }

        [Option("-s|--settings", "Show settings information.", CommandOptionType.NoValue)]
        public bool ShowSettings { get; set; }

        public void OnExecute(CommandLineApplication app)
        {
            if (ShowSettings)
            {
                var filepath = Program.GetSettingFilePath();
                _consoleHelper.RenderFile(filepath);
                return;
            }

            const string title = Settings.ApplicationName;
            _consoleHelper.RenderTitle(title);
            app.ShowHelp();
        }

        private static string GetVersion() => typeof(MainSiteCommand).GetVersion();
    }
}
