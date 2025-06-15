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
                // IsAutoScrollEnabled 속성 설정 코드가 완전히 제거되었습니다.
                var manager = new LogcatManager(deviceSerial);
                
                manager.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
                _logcatManagers.Add(manager);
                DeviceTabs.SelectedItem = manager;
                manager.Start();
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
        
        // ★★★ 수정된 최종 버전 ★★★
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