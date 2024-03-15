using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Parse
{
    internal class ParseCommandSettings : CommandSettings
    {
        [Description("The file to parse")]
        [CommandArgument(0, "[LogFilePath]")]
        public string? LogFilePath { get; set; }

        [Description("The list of loggers to include in the output")]
        [CommandOption("-l|--loggers <MODULES>")]
        public string[]? Loggers { get; set; }

        [Description("The list of loggers to exclude from the output")]
        [CommandOption("-i|--ignore-loggers <MODULES>")]
        public string[]? IgnoreLoggers { get; set; }

        [Description("The list of log levels to include in the output")]
        [CommandOption("-L|--log-levels <LEVELS>")]
        public string[]? LogLevels { get; set; }

        [Description("The list of log levels to exclude from the output")]
        [CommandOption("-I|--ignore-log-levels <LEVELS>")]
        public string[]? IgnoreLogLevels { get; set; }

        [Description("The start date for the log entries")]
        [CommandOption("-s|--since <SINCE>")]
        public DateTimeOffset? Since { get; set; }

        [Description("The end date for the log entries")]
        [CommandOption("-u|--until <UNTIL>")]
        public DateTimeOffset? Until { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrEmpty(LogFilePath)) 
                return ValidationResult.Error("log file cannot be empty");
            if (Path.Exists(LogFilePath) == false)
                return ValidationResult.Error("log file not found");

            if (LogLevels != null)
            {
                foreach(var level in LogLevels)
                {
                    if (level != "ERROR" && level != "WARN" && level != "INFO" && level != "DEBUG" && level != "TRACE")
                        return ValidationResult.Error("log level must be one of ERROR, WARN, INFO, DEBUG, or TRACE");
                }
            }
            return ValidationResult.Success();
        }
    }
}
