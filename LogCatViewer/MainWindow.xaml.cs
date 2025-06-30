using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; // CompositionTarget을 위해 추가
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace LogcatViewer
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand CopyLogCommand = new(
            "Copy Log", "CopyLogCommand", typeof(MainWindow));

        private readonly DispatcherTimer _deviceCheckTimer;
        private readonly ObservableCollection<LogcatManager> _logcatManagers = new();
        
        private EventHandler? _renderingEventHandler;

        public MainWindow()
        {
            InitializeComponent();
            DeviceTabs.ItemsSource = _logcatManagers;

            // 이벤트 핸들러 연결
            FilterTextBox.TextChanged += (s, e) => ApplyFilter();
            VerboseToggle.Click += (s, e) => ApplyFilter();
            DebugToggle.Click += (s, e) => ApplyFilter();
            InfoToggle.Click += (s, e) => ApplyFilter();
            WarningToggle.Click += (s, e) => ApplyFilter();
            ErrorToggle.Click += (s, e) => ApplyFilter();
            DeviceTabs.SelectionChanged += (s, e) => 
            {
                LogcatManager.ScrollViewer?.ScrollToBottom();
                ApplyFilter();
                SearchTextBox_TextChanged(SearchTextBox, new TextChangedEventArgs(e.RoutedEvent, UndoAction.None));
            };
            ClearLogButton.Click += ClearLogButton_Click;
            
            AutoScrollToggle.Click += AutoScrollToggle_Click;
            
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            ClearSearchButton.Click += ClearSearchButton_Click;
            
            InstallApkButton.Click += InstallApkButton_Click;
            ShowApkInfoButton.Click += ShowApkInfoButton_Click;
            ScreenshotButton.Click += ScreenshotButton_Click;

            _deviceCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _deviceCheckTimer.Tick += DeviceCheckTimer_Tick;
            _deviceCheckTimer.Start();

            this.Closing += (s, e) => {
                foreach (var manager in _logcatManagers) manager.Stop();
                if (_renderingEventHandler != null)
                {
                    CompositionTarget.Rendering -= _renderingEventHandler;
                }
                MainWindow_Closing(s, e);
            };
            
            // 앱 시작 시 자동 스크롤이 켜져있도록 초기화
            AutoScrollToggle_Click(null, new RoutedEventArgs());

            InitializeRecording();
        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            LogcatManager? selectedManager = null;

            if (_logcatManagers.Count == 0)
            {
                MessageBox.Show("연결된 기기가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (_logcatManagers.Count == 1)
            {
                selectedManager = _logcatManagers[0];
            }
            else // 여러 기기가 연결된 경우
            {
                selectedManager = DeviceTabs.SelectedItem as LogcatManager;
                if (selectedManager == null)
                {
                    MessageBox.Show("스크린샷을 캡처할 기기를 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "캡처가 완료되었습니다.\n스크린샷을 어떻게 처리하시겠습니까?\n\n[예] 버튼: 파일로 저장\n[아니오] 버튼: 클립보드에 복사\n[취소] 버튼: 작업 취소",
                    "스크린샷 캡처 완료",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes,
                    MessageBoxOptions.DefaultDesktopOnly);

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (result == MessageBoxResult.Yes) // 파일로 저장
                {
                    string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    string saveFolder = Path.Combine(picturesPath, "LogcatViewer_Screenshots");
                    Directory.CreateDirectory(saveFolder); 

                    string fileName = $"{selectedManager.DeviceSerial}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    string savePath = Path.Combine(saveFolder, fileName);

                    string? error = AdbWrapper.TakeScreenshotAndSaveToFile(selectedManager.DeviceSerial, savePath);

                    if (error != null)
                    {
                        MessageBox.Show($"스크린샷 저장에 실패했습니다.\n{error}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBoxResult openFolderResult = MessageBox.Show(
                            $"스크린샷이 다음 경로에 저장되었습니다:\n{savePath}\n폴더를 여시겠습니까?",
                            "성공",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (openFolderResult == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", saveFolder);
                        }
                    }
                }
                else if (result == MessageBoxResult.No) // 클립보드에 복사
                {
                    string? tempFilePath = null;
                    try
                    {
                        string? error = AdbWrapper.TakeScreenshotToTempFile(selectedManager.DeviceSerial, out tempFilePath);
                        if (error != null)
                        {
                            MessageBox.Show($"스크린샷 클립보드 복사에 실패했습니다.\n{error}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (tempFilePath != null && File.Exists(tempFilePath))
                        {
                            var image = new System.Windows.Media.Imaging.BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(tempFilePath);
                            image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            image.EndInit();
                            Clipboard.SetImage(image);
                            MessageBox.Show("스크린샷이 클립보드에 복사되었습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("임시 스크린샷 파일을 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    finally
                    {
                        if (tempFilePath != null && File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"스크린샷 처리 중 예외가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowApkInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string apkPath = ApkPathTextBox.Text.Trim();

            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
            {
                MessageBox.Show("유효한 APK 파일을 선택하거나 경로를 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ApkInfo? apkInfo = AdbWrapper.GetApkDetailInfo(apkPath);

            if (apkInfo == null)
            {
                MessageBox.Show("APK 정보를 가져오는 데 실패했습니다. 파일이 손상되었거나 유효한 APK 파일이 아닙니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"패키지 이름: {apkInfo.PackageName}");
            sb.AppendLine($"앱 이름: {apkInfo.AppLabel}");
            sb.AppendLine($"버전 이름: {apkInfo.VersionName}");
            sb.AppendLine($"버전 코드: {apkInfo.VersionCode}");
            sb.AppendLine($"아이콘 경로: {apkInfo.IconPath}");
            sb.AppendLine($"최소 SDK 버전: {apkInfo.MinSdkVersion}");
            sb.AppendLine($"대상 SDK 버전: {apkInfo.TargetSdkVersion}");
            sb.AppendLine($"\n권한:\n{apkInfo.Permissions}");

            ApkDetailWindow detailWindow = new ApkDetailWindow(sb.ToString());
            detailWindow.Owner = this;
            detailWindow.ShowDialog();
        }
    }
}