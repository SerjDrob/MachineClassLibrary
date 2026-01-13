using System;
using AForge.Video.DirectShow;

namespace MachineClassLibrary.VideoCapture;

public sealed class DirectShowCameraNativeSettings :
    ICameraNativeSettings
{
    private readonly VideoCaptureDevice _device;

    public DirectShowCameraNativeSettings(VideoCaptureDevice device)
    {
        _device = device;
    }

    public bool CanShowNativeDialog => true;

    public void ShowNativeDialog(IntPtr parent)
    {
        _device.DisplayPropertyPage(parent);
    }
}

