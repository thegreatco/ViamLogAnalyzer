using Spectre.Console.Cli;

using Core.Analyzers;
using Spectre.Console;
using Core.Parsers;

namespace CLI.Commands.Analyze
{
    internal class AnalyzeCommand : AsyncCommand<AnalyzeCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeCommandSettings settings)
        {
            var parser = new FileParser(settings.LogFilePath!);
            var results = await parser.Parse();

            var analyzerTasks = new List<Task>();

            // TODO: Figure out a way to dynamically load analyzers
            var timeGapAnalyzer = new TimeGapAnalyzer(results, TimeSpan.FromSeconds(2));
            analyzerTasks.Add(timeGapAnalyzer.Analyze());

            var startupTimeAnalyzer = new StartupTimeAnalyzer(results);
            analyzerTasks.Add(startupTimeAnalyzer.Analyze());

            await Task.WhenAll(analyzerTasks);
            var table = new Table().LeftAligned().AddColumn("Analyzer").AddColumn("Results");
            AnsiConsole.Live(table).Start(ctx =>
            {
                table.AddRow(new Text(typeof(TimeGapAnalyzer).Name), timeGapAnalyzer.RenderConsoleResults());
                ctx.Refresh();
                table.AddRow(new Text(typeof(StartupTimeAnalyzer).Name), startupTimeAnalyzer.RenderConsoleResults());
                ctx.Refresh();
            });
            return 0;
        }
    }
}
