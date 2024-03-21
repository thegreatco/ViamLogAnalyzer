using System.Buffers;
using System.Diagnostics;

namespace Vlogger.Core
{
    // TODOS:
    // Parse module version out of the module logger field
    // Fix this log message ending up in the source column
    // 2024-03-20T11:45:27.292110+0000 station-v5-SAU05AD2 viam-agent[596]: 2024-03-20T11:45:27.291Z        ERROR        robot_server.process.tennibot_station_hardware_control_/root/.viam/packages/data/module/db9922d9-0ccd-4886-bd9b-717220d49542-station_hardware_control-1_2_0-any/run.sh.StdErr        pexec/managed_process.go:242
    // 2024-03-20T11:45:27.292110+0000 station-v5-SAU05AD2 viam-agent[596]: \_   WARNING: Retrying (Retry(total=4, connect=None, read=None, redirect=None, status=None)) after connection broken by 'ProtocolError('Connection aborted.', RemoteDisconnected('Remote end closed connection without response'))': /simple/paho-mqtt/paho_mqtt-2.0.0-py3-none-any.whl
    public class LogEntry
    {
        public LogEntry(string line, string? nextLogEntry)
        {
            RawEntry = line;
            //if (RawEntry.Contains("2024-03-14T18:40:27.963248") && Debugger.IsAttached)
            //{
            //    Debugger.Break();
            //}
            var m = line.AsMemory();
            var datetimeLength = m.Span.IndexOf(" ");
            try
            {
                SystemdTimestamp = DateTimeOffset.Parse(m[0..datetimeLength].Span);

                m = m[datetimeLength..].TrimStart();
                var hostnameLength = m.Span.IndexOf(" ");
                Hostname = m[0..hostnameLength].ToString();

                m = m[hostnameLength..].TrimStart();
                var processLength = m.Span.IndexOf(":");
                Process = m[0..processLength].ToString();

                m = m[(processLength + 1)..].TrimStart();
                if (m.Span.IndexOf('\\') == 0)
                {
                    // this is a sub-message, need to handle this as it is part of the previous line?
                    throw new Exception("Unexpected sub-message in primary log message");
                }

                if (Process.Contains("systemd"))
                {
                    Message = m.ToString();
                    return;
                }

                if (m.Span.StartsWith("2") == false)
                {
                    Message = m.ToString();
                    return;
                }

                (m, var ts) = ParseNextField(m);
                ViamTimestamp = DateTimeOffset.Parse(ts.Span);

                (m, var logLevel) = ParseNextField(m);
                LogLevel = logLevel.ToString();

                (m, var logger) = ParseNextField(m);
                if (logger.Span.Contains("StdOut", StringComparison.Ordinal) || logger.Span.Contains("StdErr", StringComparison.Ordinal))
                {
                    // This should remove the robot_server. prefix from the logger
                    logger = logger[(logger.Span.IndexOf(".") + 1)..];
                    // This should remove the process. prefix from the logger
                    logger = logger[(logger.Span.IndexOf(".") + 1)..];

                    // Now find the end of the module name
                    var end = logger.Span.IndexOf("/") - 1;
                    logger = logger[..end];                    
                }
                Logger = logger.ToString();

                (m, var source) = ParseNextField(m);
                Source = source.ToString();

                if (nextLogEntry != null)
                {
                    var n = nextLogEntry.AsMemory();
                    if (n.Span.Contains("\\_", StringComparison.Ordinal))
                    {
                        var messageStart = n.Span.IndexOf("\\_", StringComparison.Ordinal);
                        n = n[(messageStart + 2)..].TrimStart();

                        if (n.Span.StartsWith("2"))
                        {
                            (n, var moduleTimestamp) = ParseNextField(n, " ");
                            // This bit is a little complicated, but it's to handle the case where the time and date are split with a space instead of a "T"
                            if (char.IsAsciiDigit(n.Span[0]))
                            {
                                // So we parse out the next field
                                (n, var secondTimestampPart) = ParseNextField(n, " ");

                                // Then we allocate a new array to hold the two parts and a space
                                var arr = ArrayPool<char>.Shared.Rent(moduleTimestamp.Length + secondTimestampPart.Length + 1);
                                try
                                {
                                    // Copy the date portion into the array
                                    for (var i = 0; i < moduleTimestamp.Length; i++)
                                    {
                                        arr[i] = moduleTimestamp.Span[i];
                                    }

                                    // Add a space in there to reassemble like it was
                                    arr[moduleTimestamp.Length] = ' '; // [0x20 

                                    // Now copy the time portion into the array
                                    for (var i = 0; i < secondTimestampPart.Length; i++)
                                    {
                                        arr[i + moduleTimestamp.Length + 1] = secondTimestampPart.Span[i];
                                    }

                                    // Now we're free to parse the whole thing
                                    ModuleTimestamp = DateTimeOffset.Parse(arr.AsSpan(0, moduleTimestamp.Length + secondTimestampPart.Length + 1));
                                }
                                finally
                                {
                                    // Don't forget to return the memory back to the pool
                                    ArrayPool<char>.Shared.Return(arr);
                                }
                            }
                            else
                            {
                                ModuleTimestamp = DateTimeOffset.Parse(moduleTimestamp.Span);
                            }
                        }

                        (var b, n) = JumpToLog(n, "DEBUG", "INFO", "WARN", "ERROR");
                        if (b)
                        {
                            (n, var moduleLogLevel) = ParseNextField(n);
                            (n, var moduleLogger) = ParseNextField(n, "  ");

                            ModuleLogLevel = moduleLogLevel.ToString();
                            Source = moduleLogger.ToString();
                        }

                        Message = n.ToString();
                    }
                }
                else
                {
                    Message = m.ToString();
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                Console.WriteLine(m.ToString());
                throw;
            }
        }

        private static (bool, ReadOnlyMemory<char>) JumpToLog(ReadOnlyMemory<char> m, params string[] values)
        {
            foreach (var v in values)
            {
                var i = m.Span.IndexOf(v);
                if (i != -1)
                {
                    return (true, m[i..]);
                }
            }

            return (false, m);
        }

        private static (ReadOnlyMemory<char> Line, ReadOnlyMemory<char> Value) ParseNextField(ReadOnlyMemory<char> m, string fieldTerminator = " ")
        {
            var fieldLength = m.Span.IndexOf(fieldTerminator);
            if (fieldLength == -1)
            {
                fieldLength = m.Length;
            }
            var ret = m[..fieldLength];

            return (m[fieldLength..].TrimStart(), ret);

        }

        public DateTimeOffset SystemdTimestamp { get; }
        public string Hostname { get; }
        public string Process { get; }

        public DateTimeOffset? ViamTimestamp { get; }
        public string? LogLevel { get; }
        public string? Logger { get; }
        public string? Source { get; }

        public DateTimeOffset? ModuleTimestamp { get; }
        public string? ModuleLogLevel { get; }

        public string? Message { get; }
        public string RawEntry { get; }
    }
}
