using System;
using System.Management;

namespace MachineClassLibrary.Miscellaneous
{
    public /*abstract*/ class PlugMeWatcher
    {
        private readonly string VID;// = "VID_AA47";
        private readonly string PID;// = "PID_1301";
        private Action _plugAction;
        private const string queryString = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";

        public PlugMeWatcher(string vid, string pid) => (VID, PID) = (vid, pid);

        public void WaitAndPlugMe(Action plugAction)
        {
            _plugAction = plugAction;
            using var watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery(queryString);
            watcher.Query = query;
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Start();
        }
        public event EventHandler DevicePlugged;
        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%" + VID + "%'"))
            {
                try
                {
                    var deviceCollection = searcher.Get();
                    foreach (var device in deviceCollection)
                    {
                        var deviceID = (string)device.GetPropertyValue("DeviceID");
                        if (
                                deviceID.Contains(PID)
                           )
                        {
                            _plugAction?.Invoke();
                            DevicePlugged?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
