using CLI.Commands.Analyze;
using CLI.Commands.Parse;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<ParseCommand>("parse");
    config.AddCommand<AnalyzeCommand>("analyze");
});

return app.Run(args);
