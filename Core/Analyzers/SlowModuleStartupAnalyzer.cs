using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

using static Core.Analyzers.SlowModuleStartupAnalyzer;

namespace Core.Analyzers
{
    // "my-module-that-does-stuff" slow startup detected. Elapsed 7.02 seconds
    public class SlowModuleStartupAnalyzer(ParseResults results) : IAnalyzer<SlowModuleStartupResult>
    {
        public Task Analyze(CancellationToken cancellationToken = default) =>
            Task.Factory.StartNew(AnalyzeInternal,
                                  cancellationToken,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Current);
        
        public SlowModuleStartupResult? Results { get; private set; }

        public IRenderable RenderConsoleResults()
        {
            var table = new Table().LeftAligned()
                                   .AddColumn("Module Name")
                                   .AddColumn("Start Time");

            if (Results != null)
            {
                foreach (var row in Results.SlowStarts)
                {
                    table.AddRow(new Text(row.Key), new Text(row.Value.ToString("g")));
                }
            }

            return table;
        }

        private void AnalyzeInternal()
        {
            var r = new SlowModuleStartupResult();
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

            Results = r;
        }

        public class SlowModuleStartupResult
        {
            public Dictionary<string, TimeSpan> SlowStarts { get; } = new();
        }
    }
}
