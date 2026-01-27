using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Windows.Data;
using System.ComponentModel;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void AutoScrollToggle_Click(object? sender, RoutedEventArgs e)
        {
            if (AutoScrollToggle.IsChecked == true)
            {
                // '자동 스크롤'을 켰을 때
                if (_renderingEventHandler == null)
                {
                    // 1. 렌더링 이벤트 핸들러를 연결합니다.
                    _renderingEventHandler = (s, args) => 
                    { 
                        // 현재 선택된 탭의 LogcatManager의 ScrollViewer를 사용
                        if (DeviceTabs.SelectedItem is LogcatManager manager)
                        {
                            manager.ScrollViewer?.ScrollToBottom();
                        }
                    };
                    CompositionTarget.Rendering += _renderingEventHandler;
                }

                // 현재 선택된 탭의 ScrollViewer를 스크롤
                if (DeviceTabs.SelectedItem is LogcatManager selectedManager)
                {
                    selectedManager.ScrollViewer?.ScrollToBottom();
                }
            }
            else
            {
                // '자동 스크롤'을 껐을 때, 연결했던 렌더링 이벤트 핸들러를 제거합니다.
                if (_renderingEventHandler != null)
                {
                    CompositionTarget.Rendering -= _renderingEventHandler;
                    _renderingEventHandler = null;
                }
            }
        }

        private void LogListView_Loaded(object? sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;
            var manager = listView.DataContext as LogcatManager;
            if (manager == null) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(listView);
            // 각 LogcatManager 인스턴스에 ScrollViewer를 저장 (static 제거)
            manager.ScrollViewer = scrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += (s, args) =>
                {
                    // 사용자가 스크롤을 '위로' 올렸을 때 (e.VerticalChange가 음수일 때)
                    // 그리고 자동 스크롤이 켜져 있을 때만 반응합니다.
                    if (args.VerticalChange < 0 && AutoScrollToggle.IsChecked == true)
                    {
                        // 자동 스크롤 체크를 풀어버리고, 렌더링 이벤트도 해제합니다.
                        AutoScrollToggle.IsChecked = false;
                        if (_renderingEventHandler != null)
                        {
                            CompositionTarget.Rendering -= _renderingEventHandler;
                            _renderingEventHandler = null;
                        }
                    }
                };
            }
        }

        private void LogListView_Unloaded(object? sender, RoutedEventArgs e)
        {
            // Unloaded 시 ScrollViewer를 null로 만들지 않음 - 탭 전환 시에도 유지
            // 각 LogcatManager 인스턴스가 자신의 ScrollViewer를 계속 보유
        }

        private void ClearLogButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DeviceTabs.SelectedItem is LogcatManager selectedManager)
            {
                selectedManager.ClearLogs();
            }
        }

        private void CopyCommand_Executed(object? sender, ExecutedRoutedEventArgs e)
        {
            // 현재 선택된 탭에서 ListView를 가져옴
            if (DeviceTabs.SelectedItem is not LogcatManager selectedManager) return;
            
            // TabControl에서 현재 선택된 탭의 ContentPresenter를 찾아 ListView를 가져옴
            var tabContent = DeviceTabs.Template.FindName("PART_SelectedContentHost", DeviceTabs) as ContentPresenter;
            var listView = tabContent != null ? FindVisualChild<ListView>(tabContent) : null;
            
            if (listView == null || listView.SelectedItems.Count == 0) return;
            
            var stringBuilder = new StringBuilder();
            foreach (var selectedItem in listView.SelectedItems)
            {
                if (selectedItem is LogEntry log)
                {
                    string headerLine = $"{log.Time} {log.PID} {log.TID} {log.Level} {log.Tag}: {log.Message}";
                    stringBuilder.AppendLine(headerLine);
                    if (log.HasAdditionalLines)
                    {
                        foreach (string additionalLine in log.AdditionalLines)
                        {
                            stringBuilder.AppendLine($"\t{additionalLine}");
                        }
                    }
                }
            }

            try
            {
                Clipboard.SetText(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"클립보드 복사 실패: {ex.Message}");
            }
        }

        private void LogListView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (sender is not ListView listView) return;
            if (listView.View is not GridView gridView) return;
            if (gridView.Columns.Count == 0) return;
            double availableWidth = e.NewSize.Width;
            double otherColumnsWidth = 0;
            for (int i = 0; i < gridView.Columns.Count - 1; i++)
            {
                otherColumnsWidth += gridView.Columns[i].ActualWidth;
            }

            double newLastColumnWidth = availableWidth - otherColumnsWidth - 25;
            if (newLastColumnWidth > 0)
            {
                gridView.Columns.Last().Width = newLastColumnWidth;
            }
        }

        private async void SaveLogsButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DeviceTabs.SelectedItem is not LogcatManager selectedManager) return;

            string formattedDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var saveFileDialog = new SaveFileDialog
            {
                FileName = $"logcat_{selectedManager.DeviceSerial}_{formattedDateTime}.txt",
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var stringBuilder = new StringBuilder();
                    ICollectionView filteredLogsView = CollectionViewSource.GetDefaultView(selectedManager.LogEntries);
                    foreach (var item in filteredLogsView)
                    {
                        if (item is not LogEntry log) continue;
                        stringBuilder.AppendLine($"{log.Time} {log.PID} {log.TID} {log.Level} {log.Tag}: {log.Message}");
                        if (log.HasAdditionalLines)
                        {
                            foreach (string additionalLine in log.AdditionalLines)
                            {
                                stringBuilder.AppendLine($"\t{additionalLine}");
                            }
                        }
                    }
                    await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, stringBuilder.ToString());
                    MessageBox.Show("로그가 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"로그 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}