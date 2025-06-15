using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LogcatViewer
{
    public class LogEntry : INotifyPropertyChanged
    {
        public string DeviceSerial { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public string PID { get; set; }
        public string TID { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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