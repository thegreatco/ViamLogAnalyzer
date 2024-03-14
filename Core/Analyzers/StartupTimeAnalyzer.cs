using Spectre.Console;
using Spectre.Console.Rendering;

using static Core.Analyzers.StartupTimeAnalyzer;


namespace Core.Analyzers
{
    public class StartupTimeAnalyzer(ParseResults results) : IAnalyzer<StartupTimeResult>
    {
        public StartupTimeResult Results { get; protected set; }

        public Task Analyze(CancellationToken cancellationToken = default) => Task.Factory.StartNew(AnalyzeInternal, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        public IRenderable RenderConsoleResults()
        {
            var table = new Table()
                .LeftAligned()
                .AddColumn("Start Time")
                .AddColumn("End Time")
                .AddColumn("Duration");
            table.AddRow(new Text($"{Results.Start:o}"), new Text($"{Results.End:o}"), new Text($"{Results.Duration}"));
            return table;
        }

        private void AnalyzeInternal()
        {
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
                    Results = new (start.Value, end.Value);
                    break;
                }
            }
        }

        public readonly struct StartupTimeResult(DateTimeOffset Start, DateTimeOffset End)
        {
            public DateTimeOffset Start { get; } = Start;
            public DateTimeOffset End { get; } = End;
            public TimeSpan Duration => End - Start;
        }
    }
}
