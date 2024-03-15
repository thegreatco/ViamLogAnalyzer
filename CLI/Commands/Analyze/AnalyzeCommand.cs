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

            var pipInstallDetectionAnalyzer = new PipInstallDetectionAnalyzer(results);
            analyzerTasks.Add(pipInstallDetectionAnalyzer.Analyze());

            var slowModuleStartupAnalyzer = new SlowModuleStartupAnalyzer(results);
            analyzerTasks.Add(slowModuleStartupAnalyzer.Analyze());

            await Task.WhenAll(analyzerTasks);
            var table = new Table().LeftAligned().AddColumn("Analyzer").AddColumn("Results");
            AnsiConsole.Live(table).Start(ctx =>
            {
                table.AddRow(new Text(nameof(TimeGapAnalyzer)), timeGapAnalyzer.RenderConsoleResults());
                ctx.Refresh();
                table.AddRow(new Text(nameof(StartupTimeAnalyzer)), startupTimeAnalyzer.RenderConsoleResults());
                ctx.Refresh();
                table.AddRow(new Text(nameof(PipInstallDetectionAnalyzer)), pipInstallDetectionAnalyzer.RenderConsoleResults());
                ctx.Refresh();
                table.AddRow(new Text(nameof(SlowModuleStartupAnalyzer)), slowModuleStartupAnalyzer.RenderConsoleResults());
                ctx.Refresh();
            });
            return 0;
        }
    }
}
