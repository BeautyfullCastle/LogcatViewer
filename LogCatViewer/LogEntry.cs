using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LogcatViewer
{
    public class LogEntry : INotifyPropertyChanged
    {
        public string DeviceSerial { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string PID { get; set; } = string.Empty;
        public string TID { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public List<string> AdditionalLines { get; } = new List<string>();
        
        private bool _isSearchResult;
        public bool IsSearchResult
        {
            get => _isSearchResult;
            set
            {
                if (_isSearchResult == value) return;
                _isSearchResult = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool HasAdditionalLines => AdditionalLines.Count > 0;
        
        public Visibility ExpanderVisibility => HasAdditionalLines ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddAdditionalLine(string line)
        {
            AdditionalLines.Add(line);
            OnPropertyChanged(nameof(HasAdditionalLines));
            OnPropertyChanged(nameof(ExpanderVisibility));
        }
    }
}