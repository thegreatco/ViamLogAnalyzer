using Spectre.Console;
using Spectre.Console.Rendering;

using static Core.Analyzers.PipInstallDetectionAnalyzer;

namespace Core.Analyzers
{
    public class PipInstallDetectionAnalyzer(ParseResults results) : IAnalyzer<PipInstallDetectionResult>
    {
        public PipInstallDetectionResult? Results { get; private set; }

        public Task Analyze(CancellationToken cancellationToken = default) => Task.Factory.StartNew(AnalyzeInternal, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        public IRenderable RenderConsoleResults()
        {
            if (Results == null)
                return new Table();

            var table = new Table()
                .LeftAligned()
                .AddColumn("Detected")
                .AddColumn("Messages");
            var messagesTable = new Table().LeftAligned().AddColumn("Messages").HideHeaders();
            foreach (var message in Results.Messages)
            {
                messagesTable.AddRow(new Text(message));
            }
            table.AddRow(new Text(Results.Detected.ToString(), Results.Detected ? new Style(Color.Red) : null), new Text(string.Join(Environment.NewLine, Results.Messages)));
            return Results.Detected ? table : new Table();
        }

        private void AnalyzeInternal()
        {
            foreach (var e in results.LogEntries)
            {
                if (e.Message?.Contains("pip") == true)
                {
                    Results ??= new PipInstallDetectionResult(true);
                    Results.Messages.Add($"{e.SystemdTimestamp:o} {e.Message}");
                }

                if (e.Message?.Contains("Installing dependencies") == true)
                {
                    Results ??= new PipInstallDetectionResult(true);
                    // Here we want to strip out superfluous whitespace
                    Results.Messages.Add(e.RawEntry.Replace("  ", " "));
                }
            }
        }

        public class PipInstallDetectionResult(bool detected)
        {
            public bool Detected { get; } = detected;
            public IList<string> Messages { get; } = new List<string>();
        }
    }
}
