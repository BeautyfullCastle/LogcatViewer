using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace LogcatViewer
{
    public enum ApkInstallState { None, InProgress, Success, Failure }

    public class LogcatManager : INotifyPropertyChanged
    {
        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();
        public string DeviceSerial => _deviceSerial;
        public bool IsUserAtBottom { get; set; } = true;

        private ApkInstallState _apkInstallState;
        public ApkInstallState ApkInstallState
        {
            get => _apkInstallState;
            set { if (_apkInstallState == value) return; _apkInstallState = value; OnPropertyChanged(); }
        }

        private readonly string _deviceSerial;
        private Process _logcatProcess;
        private readonly List<LogEntry> _logBuffer = new List<LogEntry>();
        private readonly DispatcherTimer _batchUpdateTimer;
        private LogEntry _currentBuildingLog = null;
        private readonly object _bufferLock = new object();

        private static readonly Regex LogRegex = new Regex(
            @"^(?<time>\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s+(?<pid>\d+)\s+(?<tid>\d+)\s+(?<level>[VDIWEF])\s+(?<tag>.*?)\s*:" +
            @"(?<message>.*)$", RegexOptions.Compiled);

        public LogcatManager(string deviceSerial)
        {
            _deviceSerial = deviceSerial;
            _batchUpdateTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _batchUpdateTimer.Tick += BatchUpdateTimer_Tick;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Start()
        {
            AdbWrapper.ExecuteCommand($"-s {_deviceSerial} logcat -c");
            _batchUpdateTimer.Start();
            
            var command = $"-s {_deviceSerial} logcat -v threadtime";
            var processInfo = new ProcessStartInfo(AdbWrapper.AdbPath, command) {
                CreateNoWindow = true, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
            };
            _logcatProcess = new Process { StartInfo = processInfo, EnableRaisingEvents = true };
            _logcatProcess.OutputDataReceived += OnOutputDataReceived;
            _logcatProcess.ErrorDataReceived += OnOutputDataReceived;
            _logcatProcess.Start();
            _logcatProcess.BeginOutputReadLine();
            _logcatProcess.BeginErrorReadLine();
        }

        public void Stop()
        {
            _batchUpdateTimer.Stop();
            lock(_bufferLock) { FlushBuildingLog(); }
            if (_logcatProcess != null && !_logcatProcess.HasExited) _logcatProcess.Kill();
            _logcatProcess?.Dispose();
        }
        
        public void ClearLogs()
        {
            lock (_bufferLock)
            {
                _logBuffer.Clear();
                _currentBuildingLog = null;
            }
            LogEntries.Clear();
        }

        private void BatchUpdateTimer_Tick(object sender, EventArgs e)
        {
            List<LogEntry> itemsToAdd;
            lock (_bufferLock)
            {
                FlushBuildingLog();
                if (_logBuffer.Count == 0) return;
                itemsToAdd = new List<LogEntry>(_logBuffer);
                _logBuffer.Clear();
            }
            if (itemsToAdd.Count > 0)
            {
                LogEntries.AddRange(itemsToAdd);
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_bufferLock)
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                var match = LogRegex.Match(e.Data);
                if (match.Success)
                {
                    var newLog = new LogEntry {
                        DeviceSerial = _deviceSerial, Time = match.Groups["time"].Value,
                        PID = match.Groups["pid"].Value.Trim(), TID = match.Groups["tid"].Value.Trim(),
                        Level = match.Groups["level"].Value.Trim(), Tag = match.Groups["tag"].Value.Trim(),
                        Message = match.Groups["message"].Value.Trim()
                    };
                    bool isContinuation = false;
                    if (_currentBuildingLog != null && _currentBuildingLog.PID == newLog.PID)
                    {
                        string trimmedMessage = newLog.Message.Trim();
                        if (trimmedMessage.StartsWith("at ") || trimmedMessage.StartsWith("Caused by:") || Regex.IsMatch(trimmedMessage, @"^\.\.\. \d+ more$")) { isContinuation = true; }
                        else if (_currentBuildingLog.Tag == "Unity" && _currentBuildingLog.Time == newLog.Time && _currentBuildingLog.TID == newLog.TID)
                        {
                            int colonIndex = newLog.Message.IndexOf(':');
                            int openParenIndex = newLog.Message.IndexOf('(');
                            if (colonIndex > 0 && openParenIndex > colonIndex && newLog.Message.EndsWith(")")) { isContinuation = true; }
                        }
                    }
                    if (isContinuation) { _currentBuildingLog.AddAdditionalLine(newLog.Message); }
                    else { FlushBuildingLog(); _currentBuildingLog = newLog; }
                }
                else { if (_currentBuildingLog != null) { _currentBuildingLog.AddAdditionalLine(e.Data); } }
            }
        }

        private void FlushBuildingLog()
        {
            if (_currentBuildingLog != null) { _logBuffer.Add(_currentBuildingLog); }
            _currentBuildingLog = null;
        }
    }
}