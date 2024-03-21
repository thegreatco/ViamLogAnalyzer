using CLI.Commands.Analyze;
using CLI.Commands.Download;
using CLI.Commands.Files;
using CLI.Commands.Parse;

using Spectre.Console;
using Spectre.Console.Cli;

using System.Diagnostics.CodeAnalysis;

namespace CLI.Commands.Default
{
    internal class DefaultCommand(IAnsiConsole console) : AsyncCommand<DefaultCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DefaultCommandSettings settings)
        {
            var token = CancellationToken.None;
            while (true)
            {
                var cmd = console.Prompt(new SelectionPrompt<string>()
                                         .Title("What would you like to do?")
                                         .AddChoices([
                                             "parse",
                                             "analyze",
                                             "download",
                                             "previous files",
                                             "exit"
                                         ]));

                switch (cmd)
                {
                    case null:
                    case "":
                    case "exit":
                        return 0;
                    case "download":
                        await new DownloadCommand(console).ExecuteAsync(context, DownloadCommandSettings.Empty);
                        break;
                    case "previous files":
                        new FilesCommand(console).Execute(context, FilesCommandSettings.Empty);
                        break;
                    case "parse":
                        await new ParseCommand(console).ExecuteAsync(context, ParseCommandSettings.Empty);
                        break;
                    case "analyze":
                        await new AnalyzeCommand(console).ExecuteAsync(context, AnalyzeCommandSettings.Empty);
                        break;
                }
            }
        }
    }
}
