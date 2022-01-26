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
    public interface IVideoCapture
    {
        public Dictionary<int, (string, string[])> AvaliableVideoCaptureDevices { get; }
        public bool IsVideoCaptureConnected { get; }

        /// <summary>
        /// Start video capture device
        /// </summary>
        /// <param name="ind">index of device</param>
        public void StartCamera(int ind, int capabilitiesInd = 0);
        public void FreezeCameraImage();
        public void StopCamera();
        public int GetVideoCaptureDevicesCount();
        int GetVideoCapabilitiesCount();
       

        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;

    }
}
