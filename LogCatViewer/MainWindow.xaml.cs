using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; // CompositionTarget을 위해 추가
using System.Windows.Threading;

namespace LogcatViewer
{
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand CopyLogCommand = new(
            "Copy Log", "CopyLogCommand", typeof(MainWindow));

        private readonly DispatcherTimer _deviceCheckTimer;
        private readonly ObservableCollection<LogcatManager> _logcatManagers = new();
        
        private EventHandler _renderingEventHandler;

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
                SearchTextBox_TextChanged(SearchTextBox, null);
            };
            ClearLogButton.Click += ClearLogButton_Click;
            
            AutoScrollToggle.Click += AutoScrollToggle_Click;
            
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            ClearSearchButton.Click += ClearSearchButton_Click;
            
            InstallApkButton.Click += InstallApkButton_Click;

            _deviceCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _deviceCheckTimer.Tick += DeviceCheckTimer_Tick;
            _deviceCheckTimer.Start();

            this.Closing += (s, e) => {
                foreach (var manager in _logcatManagers) manager.Stop();
                if (_renderingEventHandler != null)
                {
                    CompositionTarget.Rendering -= _renderingEventHandler;
                }
            };
            
            // 앱 시작 시 자동 스크롤이 켜져있도록 초기화
            AutoScrollToggle_Click(null, null);
        }
    }
}