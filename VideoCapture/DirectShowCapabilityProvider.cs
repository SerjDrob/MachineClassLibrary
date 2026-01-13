using System.Collections.Generic;
using AForge.Video.DirectShow;

namespace MachineClassLibrary.VideoCapture;

public sealed class DirectShowCapabilityProvider
    : IVideoCapabilityProvider
{
    public IReadOnlyList<VideoCapability> GetCapabilities(int cameraIndex)
    {
        var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        var device = new VideoCaptureDevice(devices[cameraIndex].MonikerString);

        var list = new List<VideoCapability>();

        foreach (var cap in device.VideoCapabilities)
        {
            list.Add(new VideoCapability(
                cap.FrameSize.Width,
                cap.FrameSize.Height,
                cap.AverageFrameRate));
        }

        return list;
    }
}

