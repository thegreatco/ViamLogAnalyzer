using Spectre.Console.Cli;

using System.ComponentModel;
using Spectre.Console;

namespace CLI.Commands.Download
{
    internal class DownloadCommandSettings : CommandSettings
    {
        public static DownloadCommandSettings Empty { get; set; } = new DownloadCommandSettings();

        [Description("The machine to connect to in the form <user>@<hostname>")]
        [CommandArgument(0, "<ConnectionString>")]
        public string? ConnectionString { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString) == false && ConnectionString.Contains('@') == false)
            {
                return ValidationResult.Error("Connection string does not appear to contain a username and hostname");
            }

            return ValidationResult.Success();
        }
    }
}
