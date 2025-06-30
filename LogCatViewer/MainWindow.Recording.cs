using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private bool _isRecording = false;
        private Process? _recordingProcess;
        private const string DeviceRecordingPath = "/sdcard/gemini_recording.mp4";

        private void InitializeRecording() // This method should be called from MainWindow constructor
        {
            RecordButton.Click += RecordButton_Click;
        }

        private async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                await StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            var selectedDevice = GetSelectedDeviceSerial();
            if (selectedDevice == null)
            {
                MessageBox.Show("녹화를 시작할 기기를 선택해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Clean up any leftover file from previous failed attempts
            AdbWrapper.DeleteFile(selectedDevice, DeviceRecordingPath);

            _recordingProcess = AdbWrapper.StartScreenRecordingProcess(selectedDevice, DeviceRecordingPath);
            if (_recordingProcess == null || _recordingProcess.HasExited)
            {
                MessageBox.Show("녹화 프로세스를 시작할 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _isRecording = true;
            RecordButton.Content = "녹화 중지";
            Title = "[녹화 중] 로그캣 뷰어";
        }

        private async Task StopRecording()
        {
            if (_recordingProcess == null) return;

            var selectedDevice = GetSelectedDeviceSerial();
            if (selectedDevice == null)
            {
                // This case should ideally not happen if recording was started properly
                _recordingProcess.Kill(); // Kill the process to be safe
                ResetRecordingState();
                return;
            }

            // The screenrecord process is stopped by sending a SIGINT (Ctrl+C)
            // We can't do that easily across platforms, so we just kill the process.
            if (!_recordingProcess.HasExited)
            {
                _recordingProcess.Kill();
                await _recordingProcess.WaitForExitAsync();
            }

            _recordingProcess.Dispose();
            _recordingProcess = null;

            // Ask user where to save the file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MP4 Video (*.mp4)|*.mp4",
                FileName = $"LogcatViewer_Recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string localPath = saveFileDialog.FileName;
                Title = "[파일 저장 중...] 로그캣 뷰어";

                string pullResult = await Task.Run(() => AdbWrapper.PullFile(selectedDevice, DeviceRecordingPath, localPath));

                if (!pullResult.Contains("1 file pulled"))
                {
                    MessageBox.Show($"녹화 파일 가져오기 실패:\n{pullResult}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Clean up the file on the device
                await Task.Run(() => AdbWrapper.DeleteFile(selectedDevice, DeviceRecordingPath));
            }
            else
            {
                // If user cancels save, still delete the file from device
                await Task.Run(() => AdbWrapper.DeleteFile(selectedDevice, DeviceRecordingPath));
            }

            ResetRecordingState();
        }

        private void ResetRecordingState()
        {
            _isRecording = false;
            RecordButton.Content = "녹화 시작";
            Title = "로그캣 뷰어";
        }

        // This method needs to be implemented or already exist in another partial class
        // For now, let's assume it gets the serial from the currently active tab.
        private string? GetSelectedDeviceSerial()
        {
            if (DeviceTabs.SelectedItem is LogcatManager viewModel)
            {
                return viewModel.DeviceSerial;
            }
            return null;
        }

        // Ensure to call ResetRecordingState if the window is closing while recording.
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_isRecording)
            {
                // Stop recording without saving
                if (_recordingProcess != null && !_recordingProcess.HasExited)
                {
                    _recordingProcess.Kill();
                }
                var selectedDevice = GetSelectedDeviceSerial();
                if (selectedDevice != null)
                {
                    AdbWrapper.DeleteFile(selectedDevice, DeviceRecordingPath);
                }
            }
        }
    }
}