using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Files
{
    internal class FilesCommand : Command<FilesCommandSettings>
    {
        public override int Execute(CommandContext context, FilesCommandSettings settings)
        {
            var tempPath = Path.GetTempPath();
            var vloggerPath = Path.Combine(tempPath,
                                           "vlogger");

            if (!Directory.Exists(vloggerPath))
                Directory.CreateDirectory(vloggerPath);

            var files = Directory.GetFiles(vloggerPath);
            var prompt = new SelectionPrompt<File>().Title("Files")
                                                    .PageSize(10);

            foreach (var file in files)
            {
                prompt.AddChoice(new File(file));
            }

            AnsiConsole.Prompt(prompt);


            return 0;
        }

        private class File(string filename)
        {
            private readonly FileInfo _fileInfo = new(filename);
            public override string ToString() => _fileInfo.Name;
        }
    }
}
