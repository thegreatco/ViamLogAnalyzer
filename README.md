# ViamLogAnalyzer (vlogger)

`vlogger` is a simple log analyzer for Viam logs. Currently, there is only support for logs exported from `systemd` (support for logs exported from app.viam.com will come in a future release).

`vlogger` is compiled for Linux, Windows, and macOS on both amd64 and arm64.

## Installation

Head on over to the releases page and grab the latest download for your OS/arch.

## Usage

`vlogger` has 2 options to run it. An interactive mode and a static command mode. To run in interactive mode, simply run `vlogger` with no parameters.

### Interactive Mode

In interactive mode, you will be guided through the process of using `vlogger`.
There are currently 4 commands available:

1. parse - parse and render a log file
2. analyze - analyze the log file for common information and known/common issues
3. download - connect to a robot, export the `systemd` logs for viam, and download them
4. list files - list previously downloaded files so you can render/analyze them again

Note: the download command stores log files in `/tmp`. These files will get cleaned up by the OS so they won't last forever.

### Static Commands

There are currently 3 commands available:

* parse - parse and render the specified log file
* analyze - analyze the log file for common information and known/common issues
* download - connect to a robot, export the `systemd` logs for viam, and download them

```text
USAGE:
    vlogger [OPTIONS] [COMMAND]

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information

COMMANDS:
    parse <LogFilePath>
    analyze <LogFilePath>
    download <ConnectionString>
```

You can pass `--help` to any of the commands to get more information about what parameters and options they take.

#### Parse

This command will read the specified log file and render it for easier reading. It will highlight error and warning log levels. It also has the ability to filter on loggers, log levels, and start/end times.

|Option|Definition|
|-|-|
|-l, --loggers <MODULES>|The list of loggers to include in the output|
|-i, --ignore-loggers <MODULES>|The list of loggers to exclude from the output|
|-L, --log-levels <LEVELS>|The list of log levels to include in the output|
|-I, --ignore-log-levels <LEVELS>|The list of log levels to exclude from the output|
|-s, --since <SINCE>|The start date for the log entries|
|-u, --until <UNTIL>|The end date for the log entries|

#### Analyze

This command will read the specified log file and run the list of analyzers. If no list is specified, all analyzers are run. Analyzers can be found in [here](Core/Analyzers/)

|Option|Definition|
|-|-|
|-a, --analyzers <ANALYZERS>|The list of analyzers to run|

#### Download

This command will connect to the specified host (using the ssh connection string syntax), export the logs, and download them. You will be prompted for the password on connection. Support for private keys will come in a future release.

## FAQ

### How do I export the logs for use in `vlogger`

```bash
journalctl -b -u viam-agent -u viam-server -o short-iso-precise > viam-logs.log
```
