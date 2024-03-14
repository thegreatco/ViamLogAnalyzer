using Spectre.Console;
using Spectre.Console.Rendering;

using static Core.Analyzers.TimeGapAnalyzer;

namespace Core.Analyzers
{
    public class TimeGapAnalyzer(ParseResults results, TimeSpan maxGap) : IAnalyzer<IList<TimeGap>>
    {
        public Task Analyze(CancellationToken cancellationToken = default) => Task.Factory.StartNew(AnalyzeInternal, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        public IList<TimeGap>? Results => _timeGapList;

        private readonly IList<TimeGap> _timeGapList = new List<TimeGap>();

        private void AnalyzeInternal()
        {
            for (var i = 0; i < results.LogEntries.Count - 1; i++)
            {
                var entry = results.LogEntries[i];
                if (entry.Message?.Contains("serving") == true)
                {
                    return;
                }
                var nextEntry = results.LogEntries[i + 1];
                if ((nextEntry.SystemdTimestamp - entry.SystemdTimestamp) > maxGap)
                {
                    _timeGapList.Add(new(entry.SystemdTimestamp, nextEntry.SystemdTimestamp, entry.Message, nextEntry.Message));
                }
            }
        }

        public IRenderable RenderConsoleResults()
        {
            var table = new Table()
                .LeftAligned()
                .AddColumn("Gap Start")
                .AddColumn("Gap End")
                .AddColumn("Time Gap")
                .AddColumn("Last Log Message");
            foreach (var gap in _timeGapList)
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
