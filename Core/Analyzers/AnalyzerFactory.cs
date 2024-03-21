namespace Vlogger.Core.Analyzers
{
    public static class AnalyzerFactory
    {
        public static IAnalyzer[] GetAnalyzers() => [
            new StartupTimeAnalyzer(),
            new PipInstallDetectionAnalyzer(),
            new SlowModuleStartupAnalyzer(),
            new TimeGapAnalyzer()
            ];
    }
}
