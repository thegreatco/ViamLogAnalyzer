using Spectre.Console.Cli;

using System.ComponentModel;
using Spectre.Console;

namespace CLI.Commands.Analyze
{
    internal class AnalyzeCommandSettings : CommandSettings
    {
        [Description("The file to parse")]
        [CommandArgument(0, "<LogFilePath>")]
        public string? LogFilePath { get; set; }

        [Description("The list of analyzers to run")]
        [CommandOption("-a|--analyzers <ANALYZERS>")]
        public string[]? Analyzers { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrEmpty(LogFilePath)) 
                return ValidationResult.Error("log file cannot be empty");
            if (Path.Exists(LogFilePath) == false)
                return ValidationResult.Error("log file not found");

            return ValidationResult.Success();
        }
    }
}
