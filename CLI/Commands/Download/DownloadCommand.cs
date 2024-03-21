using CLI.Commands.Analyze;
using CLI.Commands.Parse;

using Renci.SshNet;

using Spectre.Console;
using Spectre.Console.Cli;

using System.Diagnostics.CodeAnalysis;

namespace CLI.Commands.Download
{
    internal class DownloadCommand(IAnsiConsole console) : AsyncCommand<DownloadCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DownloadCommandSettings settings)
        {
            var token = CancellationToken.None;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                var ssh = console.Prompt(new TextPrompt<string>("SSH connection string (<user>@<hostname>)?"));
                if (string.IsNullOrWhiteSpace(ssh))
                {
                    throw new ArgumentException("SSH connection string cannot be empty");
                }
                settings.ConnectionString = ssh;
            }

            var username = settings.ConnectionString[..settings.ConnectionString.IndexOf("@", StringComparison.CurrentCulture)];
            var hostname = settings.ConnectionString[(settings.ConnectionString.IndexOf("@", StringComparison.CurrentCulture) + 1)..];

            var password = console.Prompt(new TextPrompt<string>("Password?").Secret());

            console.WriteLine($"Connecting to {hostname} with {username}");

            using var client = new SshClient(hostname, username, password);
            await client.ConnectAsync(token);

            using var cs = new MemoryStream();
            await using var csw = new StreamWriter(cs);

            using var os = new MemoryStream();
            using var eos = new MemoryStream();

            var tempPath = Path.GetTempPath();
            var vloggerPath = Path.Combine(tempPath,
                                           "vlogger");

            if (!Directory.Exists(vloggerPath))
                Directory.CreateDirectory(vloggerPath);

            var fileName = Path.Combine(vloggerPath, $"{hostname}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            await using (var fs = File.OpenWrite(fileName))
            await using (var sw = new StreamWriter(fs))
            {
                var shell = client.CreateShell(cs, os, eos);
                shell.Start();
                while (!shell.IsStarted)
                {
                    await Task.Delay(100, token);
                }

                // TODO: Look into using json format
                using var command = client.CreateCommand("journalctl -b -u viam-agent -u viam-server -o short-iso-precise");
                var res = command.Execute();
                await sw.WriteAsync(res);

                //var asyncResult = command.BeginExecute();

                //var readTask = command.OutputStream.CopyToAsync(fs, token);

                //while (!asyncResult.IsCompleted)
                //{
                //    await Task.Delay(100, token);
                //}

                //command.EndExecute(asyncResult);
                //await readTask;
            }

            console.WriteLine($"Logs downloaded to {fileName}");
            while (true)
            {
                var cmd = console.Prompt(new SelectionPrompt<string>()
                                         .Title("Would you like to do?")
                                         .AddChoices([
                                             "render",
                                             "analyze",
                                             "exit"
                                         ]));

                if (cmd == null)
                    return 0;
                switch (cmd)
                {
                    case null:
                    case "":
                    case "exit":
                        return 0;
                    case "render":
                        await new ParseCommand(console).ExecuteAsync(context, new ParseCommandSettings()
                                                                              {
                                                                                  LogFilePath = fileName
                                                                              });

                        break;
                    case "analyze":
                        await new AnalyzeCommand(console).ExecuteAsync(context, new AnalyzeCommandSettings()
                        {
                            LogFilePath = fileName
                        });

                        break;
                    
                }
            }
        }
    }
}
