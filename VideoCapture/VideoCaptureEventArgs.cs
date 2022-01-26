using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.VideoCapture
{
    public class VideoCaptureEventArgs : EventArgs
    {
        public VideoCaptureEventArgs(BitmapImage image, string errorMessage)
        {
            Image = image;
            ErrorMessage = errorMessage;
        }

        public BitmapImage Image { get; init; }
        public string ErrorMessage { get; init; }
    }
}
