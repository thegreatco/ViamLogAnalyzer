using Spectre.Console.Rendering;

namespace Vlogger.Core.Analyzers
{
    public interface IAnalyzer
    {
        public Task<AnalyzerResult> Analyze(ParseResults results, CancellationToken cancellationToken = default);
    }

    public record class AnalyzerResult(string AnalyzerName, IRenderable RenderedResults);
}
