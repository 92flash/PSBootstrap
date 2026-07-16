using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Interface;
using PSBootstrap.Shared.Template;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Service;

public class LogService(LogPath logPath) : ILogService
{
    private readonly LogPath _logPath = logPath;
    private LogStreamState _isOpen = LogStreamState.NotOpened;
    private FileStream _fileStream;
    private readonly object _lock = new();
    private static readonly ConcurrentDictionary<string, bool> HeaderConfirmed = new();

    // Clears the log file by truncating its contents to zero length.
    public void Clear()
    {
        lock (_lock)
        {
            if (!_logPath.Exists()) return;

            Open();

            try
            {
                _fileStream.SetLength(0);
                _fileStream.Seek(0, SeekOrigin.Begin);
                _fileStream.Flush();
            }
            catch (Exception ex)
            {
                throw new BootstrapException($"The log file could not be cleared.", ex);
            }   
        }
    }

    // Close the filestream to release the file lock and allow other processes to access the log file.
    public void Close()
    {
        lock (_lock)
        {
            if (null == _fileStream)
            {
                _isOpen = LogStreamState.Closed;
                return;
            }

            _fileStream.Close();
            _isOpen = LogStreamState.Closed;
        }
    }

    // Creates the log file and its parent directory if they do not exist.
    private void Create()
    {
        if (_logPath.Exists()) return;

        string directory = Path.GetDirectoryName(_logPath.ToString());
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Dispose will do the same as Close, releasing the file lock and allowing other processes to access the log file.
    public void Dispose() => Close();

    // Appends a log entry to the log file with the specified message, log type, and optional details like type and timestamp.
    public void Append(string message, LogType logType, bool addDetails)
    {
        lock (_lock)
        {
            Open();
            WriteHeaderIfNeeded();

            try
            {
                // Build the log entry and convert it to bytes for writing to the file.
                string logEntry = addDetails ? LogTemplate.LineDetails(message, logType) : message;
                var bytes = System.Text.Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);

                // Append the log entry to the end of the file and flush to ensure it's written.
                _fileStream.Seek(0, SeekOrigin.End);
                _fileStream.Write(bytes, 0, bytes.Length);
                _fileStream.Flush();
            }
            catch (Exception ex)
            {
                throw new BootstrapException($"Line could not be appended to the log.", ex);
            }
        }
    }

    // Opens the filestream for reading and writing and creating the log file if it does not exist.
    private void Open()
    {
        lock (_lock)
        {
            if (_isOpen == LogStreamState.Opened && null != _fileStream && _fileStream.Name == _logPath.ToString()) return;

            if (_isOpen == LogStreamState.Closed) throw new BootstrapException("Cannot reopen the closed filestream. Please create a new object.");

            Create();

            try
            {
                _fileStream = new(_logPath.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                _isOpen = LogStreamState.Opened;
            }
            catch (Exception ex)
            {
                _isOpen = LogStreamState.NotOpened;
                throw new BootstrapException($"Could not open the log file, probably because it's locked.", ex);
            }
        }
    }

    bool ILogService.IsFileEmpty() => new FileInfo(_logPath.ToString()).Length == 0;

    // Searches the log file for the specified matches within the last tailBytes of the file.
    bool ILogService.FindMatch(string[] matches, int tailBytes)
    {
        lock (_lock)
        {
            Open();

            try
            {
                if (!_logPath.Exists()) return false;

                // Ensure all written data is visible to readers.
                _fileStream.Flush();

                // Remember current position (likely end for append), then seek to start to read.
                long originalPosition = _fileStream.Position;
                long start = Math.Max(0, _fileStream.Length - tailBytes);
                _fileStream.Seek(start, SeekOrigin.Begin);

                // Read the last tailBytes of the file into a string for searching.
                using StreamReader reader = new(_fileStream, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true);
                string content = reader.ReadToEnd();

                // Restore the original position so writers aren't affected.
                _fileStream.Seek(originalPosition, SeekOrigin.Begin);

                // Check if all specified matches are present in the log
                foreach (string match in matches)
                {
                    if (!content.Contains(match)) return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    // Writes the log header to the log file if it has not already been written.
    private void WriteHeaderIfNeeded()
    {
        string key = _logPath.ToString();
        if (HeaderConfirmed.GetOrAdd(key, false)) return;

        string[] header = LogTemplate.LogHeader();
        if (!((ILogService)this).FindMatch(header))
        {
            string logEntry = string.Join(Environment.NewLine, header) + Environment.NewLine;
            var bytes = System.Text.Encoding.UTF8.GetBytes(logEntry);
            _fileStream.Seek(0, SeekOrigin.End);
            _fileStream.Write(bytes, 0, bytes.Length);
            _fileStream.Flush();
        }
        HeaderConfirmed[key] = true;
    }

    // Deletes the log file if they exist.
    public void Delete()
    {
        lock (_lock)
        {
            Close();

            if (_logPath.Exists())
            {
                try
                {
                    File.Delete(_logPath.ToString());
                }
                catch (Exception ex)
                {
                    throw new BootstrapException($"The log file could not be deleted.", ex);
                }
            }
        }
    }

    // Retrieve the log file and convert it into a list of Log objects based on the provided header and log entry regex patterns.
    public List<Log> Get(Regex headerMatch, Regex logEntryMatch)
    {
        lock (_lock)
        {
            Open();

            if (!_logPath.Exists()) return [];


            long originalPosition = _fileStream.Position;
            _fileStream.Seek(0, SeekOrigin.Begin);

            List<Log> logs = [];
            string fileContents;
            using (StreamReader reader = new(_fileStream, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                fileContents = reader.ReadToEnd();
            }

            _fileStream.Seek(originalPosition, SeekOrigin.Begin);

            string computerName = "";
            string username = "";
            foreach (string line in fileContents.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))
            {
                // If the line either matches the Computername or Username header, save the value to its variable
                Match header = headerMatch.Match(line);
                if (header.Success)
                {
                    if (header.Groups[1].Value == "Computername") computerName = header.Groups[2].Value;
                    if (header.Groups[1].Value == "Username") username = header.Groups[2].Value;
                    continue;
                }

                // Split the log line into DateTime, LogType and Message to create a new Log object and add it to the list of logs
                Match entry = logEntryMatch.Match(line);
                if (entry.Success)
                {
                    DateTime timestamp = DateTime.ParseExact(entry.Groups[1].Value, "dd-MM-yyyy HH:mm:ss", null);
                    LogType logType = Enum.TryParse(entry.Groups[2].Value, out LogType parsedLogType) ? parsedLogType : LogType.Attention;
                    logs.Add(new Log(timestamp, logType, entry.Groups[3].Value, computerName, username));
                }
            }

            return [.. logs.OrderBy(log => log.Timestamp)];
        }
    }
}