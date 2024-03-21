using System.Globalization;
using Vlogger.Core.Parsers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Parse
{
    internal class ParseCommand(IAnsiConsole console) : AsyncCommand<ParseCommandSettings>
    {
        private ParseCommandSettings PromptForSettings()
        {
            var settings = new ParseCommandSettings();
            settings.LogFilePath = console.Ask<string>("File to parse:");

            console.WriteLine("separate multiple values with a comma");

            var loggers = console.Ask<string>("Loggers to include", CultureInfo.InvariantCulture);
            var loggersArray =
                loggers.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (loggersArray.Length > 0)
                settings.Loggers = loggersArray;

            var ignoreLoggers = AnsiConsole.Ask("Loggers to exclude", string.Empty);
            var ignoreLoggersArray =
                ignoreLoggers.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ignoreLoggersArray.Length > 0)
                settings.IgnoreLoggers = ignoreLoggersArray;

            var logLevels = AnsiConsole.Ask("Log levels to include", string.Empty);
            var logLevelsArray =
                logLevels.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (logLevelsArray.Length > 0)
                settings.LogLevels = logLevelsArray;

            var ignoreLogLevels = AnsiConsole.Ask("Log levels to exclude", string.Empty);
            var ignoreLogLevelsArray =
                ignoreLogLevels.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ignoreLogLevelsArray.Length > 0)
                settings.IgnoreLogLevels = ignoreLoggersArray;

            return settings;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ParseCommandSettings settings)
        {
            if (settings == ParseCommandSettings.Empty)
                settings = PromptForSettings();

            var parser = new FileParser(settings.LogFilePath!);
            var res = await parser.Parse();
            var table = new Table().LeftAligned().AddColumns("Timestamp",
                                            "Hostname",
                                            "Process",
                                            "Log Level",
                                            "Logger",
                                            "Source",
                                            "Message");
            var start = settings.Since.GetValueOrDefault(DateTimeOffset.MinValue);
            var end = settings.Until.GetValueOrDefault(DateTimeOffset.MaxValue);
            foreach (var i in res.LogEntries)
            {
                if (i.SystemdTimestamp < start || i.SystemdTimestamp > end)
                    continue;
                if (settings.Loggers?.Contains(i.Logger) == false)
                    continue;
                if (settings.LogLevels?.Contains(i.LogLevel) == false)
                    continue;
                if (settings.IgnoreLoggers?.Contains(i.Logger) == true)
                    continue;
                if (settings.IgnoreLogLevels?.Contains(i.LogLevel) == true)
                    continue;
                var style = i.LogLevel switch
                {
                    "ERROR" => new Style(Color.Red),
                    "WARN" => new Style(Color.Yellow),
                    _ => new Style()
                };
                table.AddRow(new Text(i.SystemdTimestamp.ToString("o"), style),
                             new Text(i.Hostname?.EscapeMarkup() ?? string.Empty, style),
                             new Text(i.Process?.EscapeMarkup() ?? string.Empty, style),
                             new Text(i.LogLevel?.EscapeMarkup() ?? string.Empty, style),
                             new Text(i.Logger?.EscapeMarkup() ?? string.Empty, style),
                             new Text(i.Source?.EscapeMarkup() ?? string.Empty, style),
                             new Text(i.Message?.EscapeMarkup() ?? string.Empty, style));
            }
            AnsiConsole.Write(table);
            return 0;
        }
    }
}
