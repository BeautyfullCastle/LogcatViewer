using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void AutoScrollToggle_Click(object sender, RoutedEventArgs e)
        {
            if (AutoScrollToggle.IsChecked == true)
            {
                // '자동 스크롤'을 켰을 때
                if (_renderingEventHandler == null)
                {
                    // 1. 렌더링 이벤트 핸들러를 연결합니다.
                    _renderingEventHandler = (s, args) =>
                    {
                        // 현재 선택된 탭의 ListView를 찾아서 맨 아래로 내립니다.
                        if (DeviceTabs.SelectedItem is LogcatManager manager &&
                            _managerToListViewMap.TryGetValue(manager, out var lv))
                        {
                            var sv = FindVisualChild<ScrollViewer>(lv);
                            sv?.ScrollToEnd();
                        }
                    };
                    CompositionTarget.Rendering += _renderingEventHandler;
                }
                
                // 2. 현재 탭을 즉시 맨 아래로 스크롤합니다.
                if (DeviceTabs.SelectedItem is LogcatManager selectedManager &&
                    _managerToListViewMap.TryGetValue(selectedManager, out var listView))
                {
                    var scrollViewer = FindVisualChild<ScrollViewer>(listView);
                    scrollViewer?.ScrollToEnd();
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
        
        private void LogListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            var manager = listView?.DataContext as LogcatManager;
            if (manager == null) return;

            _managerToListViewMap[manager] = listView;
            var scrollViewer = FindVisualChild<ScrollViewer>(listView);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += (s, args) => {
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
        
        private void LogListView_Unloaded(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            var manager = listView?.DataContext as LogcatManager;
            if (manager != null && _managerToListViewMap.ContainsKey(manager))
            {
                _managerToListViewMap.Remove(manager);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceTabs.SelectedItem is LogcatManager selectedManager)
            {
                selectedManager.ClearLogs();
            }
        }
        
        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DeviceTabs.SelectedItem is not LogcatManager selectedManager) return;
            if (!_managerToListViewMap.TryGetValue(selectedManager, out var listView) || listView == null) return;
            if (listView.SelectedItems.Count == 0) return;
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
            try { Clipboard.SetText(stringBuilder.ToString()); }
            catch (Exception ex) { Debug.WriteLine($"클립보드 복사 실패: {ex.Message}"); }
        }
        
        private void LogListView_SizeChanged(object sender, SizeChangedEventArgs e)
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
    }
}