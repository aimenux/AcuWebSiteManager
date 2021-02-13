using System.Collections.Generic;
using Lib.Models;
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
                .Border(TableBorder.Square)
                .BorderColor(Color.White)
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
                .Border(TableBorder.Square)
                .BorderColor(Color.White)
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