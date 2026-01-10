using System;
using System.Collections.Generic;

namespace MachineClassLibrary.VideoCapture;

public interface IVideoCapture
{
    Dictionary<int, (string name, string[] capabilities)> AvailableVideoCaptureDevices { get; }
    bool IsVideoCaptureConnected { get; }
    string VideoCaptureMessage { get; }
    bool AdjustWidthToHeight { get; set; }
    void StartCamera(int deviceIndex, int capabilityIndex = 0);
    void StopCamera();
    void FreezeCameraImage();
    void UnFreezeCamera();
    int GetVideoCaptureDevicesCount();
    int GetVideoCapabilitiesCount();
    void SetCameraMirror(bool mirrorX, bool mirrorY);
    void InvokeSettings();
    /// <summary>
    /// Main video stream event
    /// </summary>
    event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
    event EventHandler CameraPlugged;
}
