using Spectre.Console;
using Spectre.Console.Rendering;

namespace Vlogger.Core.Analyzers
{
    public class StartupTimeAnalyzer : IAnalyzer
    {
        public Task<AnalyzerResult> Analyze(ParseResults results, CancellationToken cancellationToken = default) => Task.Factory.StartNew(() => AnalyzeInternal(results), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        public IRenderable RenderConsoleResults(StartupTimeResult results)
        {
            var table = new Table()
                .LeftAligned()
                .AddColumn("Start Time")
                .AddColumn("End Time")
                .AddColumn("Duration");
            table.AddRow(new Text($"{results.Start:o}"), new Text($"{results.End:o}"), new Text($"{results.Duration}"));
            return table;
        }

        private AnalyzerResult AnalyzeInternal(ParseResults results)
        {
            var r = new StartupTimeResult();
            DateTimeOffset? start = null;
            DateTimeOffset? end = null;
            for (var i = 0; i < results.LogEntries.Count - 1; i++)
            {
                var entry = results.LogEntries[i];
                if (start == null && entry.Process == "systemd[1]")
                    start = entry.SystemdTimestamp;
                if (entry.Message?.Contains("serving") == true)
                {
                    end = entry.SystemdTimestamp;
                }
                if (start != null && end != null)
                {
                    r = new (start.Value, end.Value);
                    break;
                }
            }

            return new AnalyzerResult(GetType().Name, RenderConsoleResults(r));
        }

        public readonly record struct StartupTimeResult(DateTimeOffset Start, DateTimeOffset End)
        {
            public TimeSpan Duration => End - Start;
        }
    }
}
