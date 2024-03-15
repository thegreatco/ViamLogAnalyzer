using CLI.Commands.Analyze;
using CLI.Commands.Parse;

using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<ParseCommand>("parse");
    config.AddCommand<AnalyzeCommand>("analyze");
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