using System;
using Avalonia.Media.Imaging;

public class VideoCaptureEventArgs : EventArgs
{
    public Bitmap? Image { get; }
    public string Message { get; }
    public bool ImageFreezed { get; }

    public VideoCaptureEventArgs(Bitmap? image, string message, bool imageFreezed)
    {
        Image = image;
        Message = message;
        ImageFreezed = imageFreezed;
    }
}


/*
using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.VideoCapture
{
    public class VideoCaptureEventArgs : EventArgs
    {
        public VideoCaptureEventArgs(BitmapImage image, string errorMessage, bool imageFreezed)
        {
            Image = image;
            ErrorMessage = errorMessage;
            ImageFreezed = imageFreezed;
        }

        public BitmapImage Image { get; init; }
        public string ErrorMessage { get; init; }
        public bool ImageFreezed { get; init; }
    }
}
*/
