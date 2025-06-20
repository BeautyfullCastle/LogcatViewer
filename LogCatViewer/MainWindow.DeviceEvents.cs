using System;
using System.Collections.Specialized;
using System.Linq;

namespace LogcatViewer
{
    public partial class MainWindow
    {
        private void DeviceCheckTimer_Tick(object sender, EventArgs e)
        {
            var connectedDevices = AdbWrapper.GetConnectedDevices();
            var currentSerials = _logcatManagers.Select(m => m.DeviceSerial).ToList();
            var newDevices = connectedDevices.Except(currentSerials).ToList();
            foreach (var deviceSerial in newDevices) 
            {
                var manager = new LogcatManager(deviceSerial);
                
                manager.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
                _logcatManagers.Add(manager);
                DeviceTabs.SelectedItem = manager;
                manager.Start();
                
                // 새 기기가 연결되었을 때, 오토 스크롤을 다시 켭니다.
                if (AutoScrollToggle.IsChecked == false)
                {
                    AutoScrollToggle.IsChecked = true;
                }
                // 오토 스크롤 클릭 이벤트를 수동으로 호출하여 
                // 렌더링 핸들러를 다시 연결하고 즉시 스크롤을 수행합니다.
                AutoScrollToggle_Click(null, null);
            }
            var disconnectedDevices = currentSerials.Except(connectedDevices).ToList();
            foreach (var deviceSerial in disconnectedDevices) 
            {
                var managerToRemove = _logcatManagers.FirstOrDefault(m => m.DeviceSerial == deviceSerial);
                if (managerToRemove != null) {
                    managerToRemove.LogEntries.CollectionChanged -= LogEntries_CollectionChanged;
                    managerToRemove.Stop();
                    _logcatManagers.Remove(managerToRemove);
                }
            }
        }
        
        private void LogEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 로그가 비워졌을 때 (ClearLog 버튼 클릭 시)
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // 검색창을 비워서, TextChanged 이벤트를 통해 하이라이트가 지워지도록 유도합니다.
                SearchTextBox.Clear();
            }
        }
    }
}