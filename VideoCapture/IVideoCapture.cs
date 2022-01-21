using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.VideoCapture
{
    public delegate void BitmapHandler(BitmapImage bitmapImage);
    public interface IVideoCapture
    {
        Dictionary<int, (string, string[])> AvaliableDevices { get; }

        /// <summary>
        /// Start video capture device
        /// </summary>
        /// <param name="ind">index of device</param>
        public void StartCamera(int ind, int capabilitiesInd = 0);
        public void FreezeCameraImage();
        public void StopCamera();
        public int GetDevicesCount();
        int GetVideCapabilitiesCount();

        public event BitmapHandler OnBitmapChanged;

    }
}
