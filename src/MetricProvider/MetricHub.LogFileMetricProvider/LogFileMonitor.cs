using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace MetricHub.MetricProvider
{
    public class LogFileMonitor
    {
        private ReaderWriterLock _rwLock;
        private TimeSpan _lockTimeout = TimeSpan.FromMilliseconds(1000);
        private AutoResetEvent _stopEvent;
        private FileSystemWatcher _watcher;
        private Dictionary<string, LogFileInfo> _fileStreamDict;

        public int MaxRetries { get; set; } = 5;

        public bool IsStopped { get; set; } = false;

        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(1000);

        public event EventHandler<string> LogHandler;

        public LogFileMonitor(string path, string filter, bool includeSubdirectories)
        {
            _watcher = new FileSystemWatcher(path, filter)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = includeSubdirectories
            };
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnCreated);
            _watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);
            _watcher.Error += new ErrorEventHandler(OnError);

            _fileStreamDict = new Dictionary<string, LogFileInfo>();
            _rwLock = new ReaderWriterLock();
            _stopEvent = new AutoResetEvent(false);
        }

        public void Start()
        {
            SearchOption searchOption = _watcher.IncludeSubdirectories == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] fileNames = Directory.GetFiles(_watcher.Path, _watcher.Filter, searchOption);

            foreach (var fileName in fileNames)
            {
                ReloadFile(fileName);
            }

            _stopEvent.WaitOne();

            IsStopped = true;
        }

        public void Stop()
        {
            _stopEvent.Set();
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            Log($"{e.FullPath} was created.");

            ReloadFile(e.FullPath);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            Log($"{e.FullPath} was deleted.");
            RemoveFile(e.FullPath);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Log($"File {e.OldFullPath} was renamed to {e.FullPath}");

            bool fStopMonitorOldFile = MatchFilter(e.OldName, _watcher.Filter) && MatchDirectory(e.OldFullPath, _watcher.Path);
            bool fStartMonitorNewFile = MatchFilter(e.Name, _watcher.Filter) && MatchDirectory(e.FullPath, _watcher.Path);

            if (fStopMonitorOldFile == true && fStartMonitorNewFile == false)
            {
                RemoveFile(e.OldFullPath);
            }
            else if (fStopMonitorOldFile == true && fStartMonitorNewFile == true)
            {
                RenameFile(e.OldFullPath, e.FullPath);
            }
            else if (fStopMonitorOldFile == false && fStartMonitorNewFile == true)
            {
                ReloadFile(e.FullPath);
            }
            else
            {
                throw new Exception($"Unexpected: neither {e.OldFullPath} nor {e.FullPath} matches the monitoring rule.");
            }
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            Log($"Error in monitoring: {e.GetException().ToString()}");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //Loge($"{e.FullPath} was changed.");

            ReloadFile(e.FullPath);
        }

        private void RemoveFile(string fullPath)
        {
            try
            {
                _rwLock.AcquireWriterLock(_lockTimeout);

                if (_fileStreamDict.ContainsKey(fullPath))
                {
                    _fileStreamDict.Remove(fullPath);
                }

                _rwLock.ReleaseWriterLock();
            }
            catch (ApplicationException ex)
            {
                Log($"RemoveFile: AcquireWriterLock timeout: {ex.ToString()}");
                return;
            }
        }

        private void RenameFile(string oldfullPath, string newFullPath)
        {
            var newLogFileInfo = new LogFileInfo()
            {
                FullPath = newFullPath
            };

            try
            {
                _rwLock.AcquireWriterLock(_lockTimeout);

                if (_fileStreamDict.TryGetValue(oldfullPath, out LogFileInfo oldLogFileInfo))
                {
                    newLogFileInfo.LastFileSize = oldLogFileInfo.LastFileSize;
                    _fileStreamDict.Remove(oldfullPath);
                }

                _fileStreamDict[newFullPath] = newLogFileInfo;

                _rwLock.ReleaseWriterLock();
            }
            catch (ApplicationException ex)
            {
                Log($"RenameFile: AcquireWriterLock timeout: {ex.ToString()}");
                return;
            }
        }

        private void ReloadFile(string fullPath)
        {
            bool fExist = false;
            LogFileInfo logFileInfo = null;

            try
            {
                _rwLock.AcquireReaderLock(_lockTimeout);

                fExist = _fileStreamDict.TryGetValue(fullPath, out logFileInfo);

                _rwLock.ReleaseReaderLock();
            }
            catch (ApplicationException ex)
            {
                Log($"ReloadFile: AcquireReaderLock timeout: {ex.ToString()}");
                return;
            }

            if (!fExist)
            {
                logFileInfo = new LogFileInfo()
                {
                    FullPath = fullPath,
                };
            }

            LogFileInfo newLogFileInfo = ReloadFile(logFileInfo);

            try
            {
                _rwLock.AcquireWriterLock(_lockTimeout);

                if (_fileStreamDict.ContainsKey(fullPath))
                {
                    _fileStreamDict.Remove(fullPath);
                }

                _fileStreamDict[fullPath] = newLogFileInfo;

                _rwLock.ReleaseWriterLock();
            }
            catch (ApplicationException ex)
            {
                Log($"ReloadFile: AcquireWriterLock timeout: {ex.ToString()}");
                return;
            }
        }

        private LogFileInfo ReloadFile(LogFileInfo logFileInfo)
        {
            string newFileLines = null;
            long newLength = 0;

            int count = 0;
            bool fSuccess = false;
            while (count < MaxRetries && !fSuccess)
            {
                try
                {
                    using (FileStream fs = File.Open(logFileInfo.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        newLength = fs.Length;
                        if (newLength >= logFileInfo.LastFileSize)
                        {
                            fs.Position = logFileInfo.LastFileSize;  // Only read in new lines added
                        }

                        using (StreamReader reader = new StreamReader(fs))
                        {
                            newFileLines = reader.ReadToEnd();
                        }
                    }

                    fSuccess = true;
                }
                catch (IOException)
                {
                    Thread.Sleep(RetryInterval);
                }

                ++count;
            }

            if (0 != newFileLines.Length)
            {
                // See if this log file has proper cr/lf and if not convert
                int lastCr = newFileLines.LastIndexOf('\n');
                if (-1 != lastCr && 0 < lastCr)
                {
                    if (newFileLines[lastCr - 1] != '\r')
                    {
                        // OK, this file only has Cr and we need to convert to CrLf
                        newFileLines = newFileLines.Replace("\n", "\r\n");
                    }
                }
            }

            Log(newFileLines);

            return new LogFileInfo()
            {
                FullPath = logFileInfo.FullPath,
                LastFileSize = newLength
            };
        }

        private static bool MatchFilter(string fileName, string filter)
        {
            Regex mask = new Regex(
                '^' +
                filter
                    .Replace(".", "[.]")
                    .Replace("*", ".*")
                    .Replace("?", ".")
                + '$',
                RegexOptions.IgnoreCase);
            return mask.IsMatch(fileName);
        }

        private static bool MatchDirectory(string fullPath, string directoryPath)
        {
            return fullPath.StartsWith(directoryPath);
        }

        private void Log(string data)
        {
            LogHandler?.Invoke(this, data);
        }
    }
}
