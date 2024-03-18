# ViamLogAnalyzer (vlogger)

vlogger is a simple log analyzer for Viam logs. Currently, there is only support for logs exported from `systemd` (support for logs exported from app.viam.com will come in a future release).

vlogger is compiled for Linux, Windows, and macOS on both amd64 and arm64.


## Installation

Head on over to the releases page for the latest download.

## Usage

vlogger has 2 options to run it. An interactive mode and a static command mode. To run in interactive mode, simply run `vlogger` with no parameters.

### Static Commands

There are currently 3 commands available:

* parse - parse and render the specified log file
* analyze - analyze the log file for common information and known/common issues
* download - connect to a robot, export the `systemd` logs for viam, and download them

```text
USAGE:
    vlogger.dll [OPTIONS] [COMMAND]

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information

COMMANDS:
    parse <LogFilePath>
    analyze <LogFilePath>
    download <ConnectionString>
```

You can pass `--help` to any of the commands to get more information about what parameters and options they take.

### Interactive Mode

In interactive mode, you will be guided through the process of using `vlogger`.
There are currently 4 commands available:

1. parse - parse and render a log file
2. analyze - analyze the log file for common information and known/common issues
3. download - connect to a robot, export the `systemd` logs for viam, and download them
4. list files - list previously downloaded files so you can render/analyze them again

Note: the download command stores log files in `/tmp`. These files will get cleaned up by the OS so they won't last forever.
