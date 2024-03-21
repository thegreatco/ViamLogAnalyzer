using Spectre.Console;
using Spectre.Console.Rendering;

namespace Vlogger.Core.Analyzers
{
    public class TimeGapAnalyzer : IAnalyzer
    {
        public Task<AnalyzerResult> Analyze(ParseResults results, CancellationToken cancellationToken = default) => Task.Factory.StartNew(() => AnalyzeInternal(results), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        public TimeSpan MaxGap { get; set; } = TimeSpan.FromSeconds(2);

        private AnalyzerResult AnalyzeInternal(ParseResults results)
        {
            var r = new List<TimeGap>();
            for (var i = 0; i < results.LogEntries.Count - 1; i++)
            {
                var entry = results.LogEntries[i];
                if (entry.Message?.Contains("serving") == true)
                {
                    return new AnalyzerResult(GetType().Name, RenderConsoleResults(r));
                }
                var nextEntry = results.LogEntries[i + 1];
                if ((nextEntry.SystemdTimestamp - entry.SystemdTimestamp) > MaxGap)
                {
                    r.Add(new(entry.SystemdTimestamp, nextEntry.SystemdTimestamp, entry.Message, nextEntry.Message));
                }
            }

            return new AnalyzerResult(GetType().Name, RenderConsoleResults(r));
        }

        public IRenderable RenderConsoleResults(IList<TimeGap> results)
        {
            if (results.Count == 0)
            {
                return new Text("No time gaps found", Color.Green);
            }
            var table = new Table()
                .LeftAligned()
                .AddColumn("Gap Start")
                .AddColumn("Gap End")
                .AddColumn("Time Gap")
                .AddColumn("Last Log Message");
            foreach (var gap in results)
            {
                var gapStyle = new Style();
                if (gap.Gap > TimeSpan.FromSeconds(5))
                {
                    gapStyle = new Style(Color.Red);
                }
                else if (gap.Gap > TimeSpan.FromSeconds(2))
                {
                    gapStyle = new Style(Color.Yellow);
                }
                table.AddRow(new Text($"{gap.Start:o}", gapStyle), new Text($"{gap.End:o}", gapStyle), new Text($"{gap.Gap}", gapStyle), new Text(gap.StartLogLine ?? string.Empty, gapStyle));
            }
            return table;
        }

        public readonly struct TimeGap(DateTimeOffset start, DateTimeOffset end, string? startLogMessage, string? endLogMessage)
        {
            public DateTimeOffset Start { get; } = start;
            public DateTimeOffset End { get; } = end;
            public TimeSpan Gap => End - Start;
            public string? StartLogLine { get; } = startLogMessage;
            public string? EndLogLine { get; } = endLogMessage;
            public override string ToString() => $"{Start:o} - {End:o} ({Gap})";
        }
    }
}
