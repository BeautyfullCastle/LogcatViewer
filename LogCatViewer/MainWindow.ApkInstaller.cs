using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void ApkPathTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effects = DragDropEffects.Copy; }
            else { e.Effects = DragDropEffects.None; }
            e.Handled = true;
        }

        private void ApkPathTextBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string firstFile = files[0];
                if (Path.GetExtension(firstFile).Equals(".apk", StringComparison.OrdinalIgnoreCase))
                {
                    ApkPathTextBox.Text = firstFile;
                }
                else
                {
                    MessageBox.Show("APK 파일만 드래그 앤 드롭 할 수 있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ApkPathTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Android Package (*.apk)|*.apk" };
            string currentPath = ApkPathTextBox.Text;
            if (File.Exists(currentPath))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(currentPath);
                openFileDialog.FileName = Path.GetFileName(currentPath);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ApkPathTextBox.Text = openFileDialog.FileName;
            }
            e.Handled = true;
        }

        private async void InstallApkButton_Click(object sender, RoutedEventArgs e)
        {
            string apkPath = ApkPathTextBox.Text;
            if (!File.Exists(apkPath))
            {
                MessageBox.Show("APK 파일 경로가 잘못되었습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var targetManagers = _logcatManagers.ToList();
            if (targetManagers.Count == 0)
            {
                MessageBox.Show("연결된 기기가 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            InstallApkButton.IsEnabled = false;
            InstallApkButton.Content = "작업 중...";

            if (ReinstallCheckBox.IsChecked == true)
            {
                string? packageName = await Task.Run(() => AdbWrapper.GetPackageNameFromApk(apkPath));
                if (string.IsNullOrEmpty(packageName))
                {
                    MessageBox.Show("APK에서 패키지 이름을 가져오는데 실패했습니다. '삭제 후 재설치'를 끄고 다시 시도해보세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    InstallApkButton.IsEnabled = true;
                    InstallApkButton.Content = "연결된 모든 기기에 설치";
                    return;
                }
                
                targetManagers.ForEach(m => m.ApkInstallState = ApkInstallState.InProgress);
                var uninstallTasks = targetManagers.Select(manager => Task.Run(() => AdbWrapper.UninstallPackage(manager.DeviceSerial, packageName))).ToList();
                await Task.WhenAll(uninstallTasks);
            }

            targetManagers.ForEach(m => m.ApkInstallState = ApkInstallState.InProgress);
            var installTasks = targetManagers.Select(manager => Task.Run(() =>
            {
                string result = AdbWrapper.InstallApk(manager.DeviceSerial, apkPath);
                bool success = result.Contains("Success", StringComparison.OrdinalIgnoreCase);
                return new { Manager = manager, Success = success, ResultString = result };
            })).ToList();

            var results = await Task.WhenAll(installTasks);

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine("설치 작업이 완료되었습니다.\n");

            foreach (var res in results)
            {
                res.Manager.ApkInstallState = res.Success ? ApkInstallState.Success : ApkInstallState.Failure;
                resultBuilder.AppendLine($"[{res.Manager.DeviceSerial}]: {res.ResultString.Trim()}");

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                timer.Tick += (s, args) =>
                {
                    res.Manager.ApkInstallState = ApkInstallState.None;
                    timer.Stop();
                };
                timer.Start();
            }

            InstallApkButton.IsEnabled = true;
            InstallApkButton.Content = "연결된 모든 기기에 설치";
            MessageBox.Show(resultBuilder.ToString(), "작업 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}