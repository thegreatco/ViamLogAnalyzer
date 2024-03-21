using Spectre.Console;
using Spectre.Console.Rendering;

namespace Vlogger.Core.Analyzers
{
    // "my-module-that-does-stuff" slow startup detected. Elapsed 7.02 seconds
    public class SlowModuleStartupAnalyzer : IAnalyzer
    {
        public Task<AnalyzerResult> Analyze(ParseResults results, CancellationToken cancellationToken = default) =>
            Task.Factory.StartNew(() => AnalyzeInternal(results),
                                  cancellationToken,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Current);
        
        public SlowModuleStartupResult? Results { get; private set; }

        public IRenderable RenderConsoleResults(SlowModuleStartupResult? results)
        {
            if (results == null || results.SlowStarts.Count == 0)
            {
                return new Text("No slow startups detected", Color.Green);
            }
            var table = new Table().LeftAligned()
                                   .AddColumn("Module Name")
                                   .AddColumn("Start Time");

            if (results != null)
            {
                foreach (var row in results.SlowStarts)
                {
                    table.AddRow(new Text(row.Key), new Text(row.Value.ToString("g")));
                }
            }

            return table;
        }

        private AnalyzerResult AnalyzeInternal(ParseResults results)
        {
            var r = new SlowModuleStartupResult(new Dictionary<string, TimeSpan>());
            foreach (var e in results.LogEntries)
            {
                if (e.Message?.Contains("slow startup detected") != true)
                    continue;

                var m = e.Message.AsSpan();
                var start = m.IndexOf("\"");
                // remove the first quote
                m = m[(start + 1)..];

                // now we can get the closing quote
                var end = m.IndexOf("\"");
                var moduleName = m[..end].ToString();

                // Now slice out the module name and the quote
                m = m[(end + 1)..];

                // Now advance up to the actual time
                start = m.IndexOf("Elapsed ") + "Elapsed ".Length;
                m = m[start..];

                end = m.IndexOf(" ");
                var timespan = TimeSpan.FromSeconds(double.Parse(m[..end]));
                r.SlowStarts[moduleName] = timespan;
            }
            return new AnalyzerResult(GetType().Name, RenderConsoleResults(r));
        }

        public record class SlowModuleStartupResult(Dictionary<string, TimeSpan> SlowStarts);
    }
}
