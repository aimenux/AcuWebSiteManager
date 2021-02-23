using System.Collections.Generic;
using System.IO;
using Lib.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace Lib.Helpers
{
    public class ConsoleHelper : IConsoleHelper
    {
        public void RenderTitle(string text)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new FigletText(text).LeftAligned());
            AnsiConsole.WriteLine();
        }

        public void RenderFile(string filepath)
        {
            var name = Path.GetFileName(filepath);
            var json = File.ReadAllText(filepath);
            var formattedJson = JToken.Parse(json).ToString(Formatting.Indented);
            var header = new Rule($"[yellow]({name})[/]");
            header.Centered();
            var footer = new Rule($"[yellow]({filepath})[/]");
            footer.Centered();

            AnsiConsole.WriteLine();
            AnsiConsole.Render(header);
            AnsiConsole.WriteLine(formattedJson);
            AnsiConsole.Render(footer);
            AnsiConsole.WriteLine();
        }

        public void RenderTable(Request request)
        {
            var site = request.SiteVirtualDirectoryName;
            var pool = request.AppPoolName;
            var server = request.ServerName;
            var database = request.DatabaseName;
            var password = request.Password;
            var directory = request.SiteDirectoryPath;

            var link = $"http://localhost/{site}";
            var siteMarkup = $"[green][link={link}]{site}[/][/]";

            var table = new Table()
                .BorderColor(Color.White)
                .Border(TableBorder.Square)
                .AddColumn(new TableColumn("[u]Site[/]").Centered())
                .AddColumn(new TableColumn("[u]Pool[/]").Centered())
                .AddColumn(new TableColumn("[u]Server[/]").Centered())
                .AddColumn(new TableColumn("[u]Database[/]").Centered())
                .AddColumn(new TableColumn("[u]Password[/]").Centered())
                .AddColumn(new TableColumn("[u]Directory[/]").Centered())
                .AddRow(siteMarkup, pool, server, database, password, directory);

            AnsiConsole.WriteLine();
            AnsiConsole.Render(table);
            AnsiConsole.WriteLine();
        }

        public void RenderTable(ICollection<SiteDetails> details)
        {
            var table = new Table()
                .BorderColor(Color.White)
                .Border(TableBorder.Square)
                .Title($"[yellow]Found {details.Count} site(s)[/]")
                .AddColumn(new TableColumn("[u]Site[/]").Centered())
                .AddColumn(new TableColumn("[u]AppPool[/]").Centered())
                .AddColumn(new TableColumn("[u]VirtualDirectory[/]").Centered());

            foreach (var detail in details)
            {
                var site = detail.SiteFriendlyName;
                var link = $"http://localhost/{site}";
                var siteMarkup = $"[green][link={link}]{site}[/][/]";
                var pool = detail.Application.ApplicationPoolName;
                var virDir = detail.VirtualDirectory.PhysicalPath;
                table = table.AddRow(siteMarkup, pool, virDir);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(table);
            AnsiConsole.WriteLine();
        }
    }
}