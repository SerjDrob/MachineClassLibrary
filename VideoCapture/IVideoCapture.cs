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
        Dictionary<int, (string, string[])> AvaliableVideoCaptureDevices { get; }
        bool IsVideoCaptureConnected { get; }
        string VideoCaptureMessage { get; }
        /// <summary>
        /// Start video capture device
        /// </summary>
        /// <param name="ind">index of device</param>
        void StartCamera(int ind, int capabilitiesInd = 0);
        void FreezeCameraImage();
        void StopCamera();
        int GetVideoCaptureDevicesCount();
        int GetVideoCapabilitiesCount();
        void InvokeSettings();
        event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;

    }
}
