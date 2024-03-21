namespace Vlogger.Core
{
    public class ParseResults
    {
        public IList<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public void AddLogEntry(string logEntry, string? nextLogEntry) => LogEntries.Add(new LogEntry(logEntry, nextLogEntry));
    }
}
