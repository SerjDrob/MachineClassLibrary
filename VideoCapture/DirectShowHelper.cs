using System;
using AForge.Video.DirectShow;

namespace MachineClassLibrary.VideoCapture;

internal static class DirectShowHelper
{
    public static VideoCaptureDevice CreateDirectShowDevice(int cameraIndex)
    {
        var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

        if (devices.Count == 0)
            throw new InvalidOperationException("No DirectShow video devices found.");

        if (cameraIndex < 0 || cameraIndex >= devices.Count)
            throw new ArgumentOutOfRangeException(
                nameof(cameraIndex),
                $"Camera index {cameraIndex} is out of range. Found {devices.Count} devices.");

        var deviceInfo = devices[cameraIndex];

        var device = new VideoCaptureDevice(deviceInfo.MonikerString);

        return device;
    }
}


