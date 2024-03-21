using Spectre.Console;
using Spectre.Console.Rendering;

namespace Vlogger.Core.Analyzers
{
    public class PipInstallDetectionAnalyzer : IAnalyzer
    {
        public Task<AnalyzerResult> Analyze(ParseResults results, CancellationToken cancellationToken = default) => Task.Factory.StartNew(() => AnalyzeInternal(results), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        public IRenderable RenderConsoleResults(PipInstallDetectionResult? results)
        {
            if (results == null || results.Messages.Count == 0)
                return new Text("No pip install detected", Color.Green);

            var table = new Table()
                .LeftAligned()
                .AddColumn("pip Messages");
            foreach (var message in results.Messages)
            {
                table.AddRow(new Text(message));
            }
            return table;
        }

        private AnalyzerResult AnalyzeInternal(ParseResults results)
        {
            PipInstallDetectionResult? r = null;
            foreach (var e in results.LogEntries)
            {
                if (e.Message?.Contains("pip") == true && e.Message?.Contains("pipe") == false)
                {
                    r ??= new PipInstallDetectionResult();
                    r.Messages.Add($"{e.SystemdTimestamp:o} {e.Message}");
                }

                if (e.Message?.Contains("Installing dependencies") == true)
                {
                    r ??= new PipInstallDetectionResult();
                    // Here we want to strip out superfluous whitespace
                    r.Messages.Add(e.Message!.Replace("  ", " "));
                }
            }
            return new AnalyzerResult(GetType().Name, RenderConsoleResults(r));
        }

        public class PipInstallDetectionResult()
        {
            public IList<string> Messages { get; } = new List<string>();
        }
    }
}
