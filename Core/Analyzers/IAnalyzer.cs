using Spectre.Console.Rendering;

namespace Core.Analyzers
{
    public interface IAnalyzer<T>
    {
        public Task Analyze(CancellationToken cancellationToken = default);
        public T? Results { get; }
        public IRenderable RenderConsoleResults();
    }
}
