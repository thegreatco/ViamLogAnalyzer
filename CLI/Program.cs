using CLI.Commands.Analyze;
using CLI.Commands.Default;
using CLI.Commands.Download;
using CLI.Commands.Files;
using CLI.Commands.Parse;

using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp<DefaultCommand>();

app.Configure(config =>
{
    config.AddCommand<ParseCommand>("parse");
    config.AddCommand<AnalyzeCommand>("analyze");
    config.AddCommand<FilesCommand>("files");
    config.AddCommand<DownloadCommand>("download");
});

try
{
    return app.Run(args);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return -1;
}