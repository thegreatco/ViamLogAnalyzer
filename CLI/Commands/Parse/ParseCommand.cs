using System.Diagnostics;

using Core.Parsers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Parse
{
    internal class ParseCommand : AsyncCommand<ParseCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, ParseCommandSettings settings)
        {
            var parser = new FileParser(settings.LogFilePath!);
            var res = await parser.Parse();
            var table = new Table().LeftAligned();
            AnsiConsole.Live(table)
                       .Start(ctx =>
                       {
                           table.AddColumns("Timestamp",
                                            "Hostname",
                                            "Process",
                                            "Log Level",
                                            "Logger",
                                            "Source",
                                            "Message");
                           ctx.Refresh();
                           var a = 0;
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
                               if (a % 100 == 0)
                                   ctx.Refresh();
                               a++;
                           }
                       });
            return 0;
        }
    }
}
