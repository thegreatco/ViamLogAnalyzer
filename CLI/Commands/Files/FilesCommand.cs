using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands.Files
{
    internal class FilesCommand(IAnsiConsole console) : Command<FilesCommandSettings>
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
                                                    .UseConverter(Render)
                                                    .PageSize(10);

            foreach (var file in files)
            {
                prompt.AddChoice(new File(file));
            }

            console.Prompt(prompt);


            return 0;
        }

        private static string Render(File arg) => arg.Name;

        private class File(string filename)
        {
            private readonly FileInfo _fileInfo = new(filename);
            public string Name => _fileInfo.Name;
            public string Size => _fileInfo.Length.Bytes().Humanize();
            public override string ToString() => _fileInfo.Name;
        }
    }
}
