using Lib.Helpers;
using Lib.Models;
using McMaster.Extensions.CommandLineUtils;

namespace App.Commands
{
    [Command(Name = "MainSite", FullName = "Manage Acumatica Sites", Description = "Manage Acumatica Site (database, files).")]
    [HelpOption]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand(typeof(CreateSiteCommand), typeof(DeleteSiteCommand), typeof(ListSitesCommand))]
    public class MainSiteCommand
    {
        private readonly IConsoleHelper _consoleHelper;

        public MainSiteCommand(IConsoleHelper consoleHelper)
        {
            _consoleHelper = consoleHelper;
        }

        public void OnExecute(CommandLineApplication app) => ShowHelp(app);

        private void ShowHelp(CommandLineApplication app)
        {
            const string title = Settings.ApplicationName;
            _consoleHelper.RenderTitle(title);
            app.ShowHelp();
        }

        private static string GetVersion() => typeof(MainSiteCommand).GetVersion();
    }
}
