using System;
using System.ComponentModel;
using System.Windows.Data;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void ApplyFilter()
        {
            if (DeviceTabs.SelectedItem is not LogcatManager selectedManager) return;
            ICollectionView view = CollectionViewSource.GetDefaultView(selectedManager.LogEntries);
            if (view == null) return;
            
            view.Filter = item => {
                if (item is not LogEntry log) return false;
                bool levelMatch = log.Level switch {
                    "V" => VerboseToggle.IsChecked == true, "D" => DebugToggle.IsChecked == true,
                    "I" => InfoToggle.IsChecked == true, "W" => WarningToggle.IsChecked == true,
                    "E" => ErrorToggle.IsChecked == true, _ => true };
                if (!levelMatch) return false;
                string filterText = FilterTextBox.Text;
                if (string.IsNullOrWhiteSpace(filterText)) return true; 
                return log.Tag.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                       log.Message.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            };
        }
    }
}