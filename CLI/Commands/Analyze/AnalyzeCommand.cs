using Spectre.Console.Cli;
using Spectre.Console;


using Vlogger.Core.Analyzers;
using Vlogger.Core.Parsers;

namespace CLI.Commands.Analyze
{
    internal class AnalyzeCommand(IAnsiConsole console) : AsyncCommand<AnalyzeCommandSettings>
    {
        // TODO: Figure out a way to dynamically load analyzers
        private static readonly IAnalyzer[] Analyzers = AnalyzerFactory.GetAnalyzers();

        public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeCommandSettings settings)
        {
            var parser = new FileParser(settings.LogFilePath!);
            var results = await parser.Parse();
            var analyzerTasks = Analyzers.Select(analyzer => analyzer.Analyze(results));

            var r = await Task.WhenAll(analyzerTasks);
            var table = new Table().LeftAligned().AddColumn("Analyzer").AddColumn("Results");
            foreach (var result in r)
            {
                table.AddRow(new Text(result.AnalyzerName), result.RenderedResults);
            }

            console.Write(table);
            return 0;
        }
    }
}
