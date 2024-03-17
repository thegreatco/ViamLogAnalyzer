using CLI.Commands.Analyze;
using CLI.Commands.Download;
using CLI.Commands.Files;
using CLI.Commands.Parse;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Default
{
    internal class DefaultCommand : AsyncCommand<DefaultCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DefaultCommandSettings settings)
        {
            var token = CancellationToken.None;
            while (true)
            {
                var cmd = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                             .Title("What would you like to do?")
                                             .AddChoices([
                                                 "parse",
                                                 "analyze",
                                                 "download",
                                                 "previous files",
                                                 "exit"
                                             ]));

                if (cmd == null)
                {
                    return -1;
                }

                switch (cmd)
                {
                    case "exit":
                        return 0;
                    case "download":
                        await new DownloadCommand().ExecuteAsync(context, DownloadCommandSettings.Empty);
                        break;
                    case "previous files":
                        new FilesCommand().Execute(context, FilesCommandSettings.Empty);
                        break;
                    case "parse":
                        await new ParseCommand().ExecuteAsync(context, ParseCommandSettings.Empty);
                        break;
                    case "analyze":
                        await new AnalyzeCommand().ExecuteAsync(context, AnalyzeCommandSettings.Empty);
                        break;
                }

                if (cmd == "download")
                {
                    
                }
            }

            return 0;
        }
    }
}
