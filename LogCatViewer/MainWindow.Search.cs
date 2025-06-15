using System;
using System.Linq;
using System.Windows.Controls;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DeviceTabs.SelectedItem is not LogcatManager selectedManager) return;
            string searchText = SearchTextBox.Text;
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                AutoScrollToggle.IsChecked = false;
            }

            foreach (var logEntry in selectedManager.LogEntries)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    if (logEntry.IsSearchResult) logEntry.IsSearchResult = false;
                    continue;
                }

                bool match = (logEntry.Tag != null && logEntry.Tag.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                             (logEntry.Message != null && logEntry.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                
                logEntry.IsSearchResult = match;
            }
        }

        private void ClearSearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchTextBox.Clear();
        }
    }
}