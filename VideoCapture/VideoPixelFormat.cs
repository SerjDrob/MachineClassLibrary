public enum VideoPixelFormat
{
    Bgr24,   // OpenCV default
    Rgb24,
    Gray8
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
