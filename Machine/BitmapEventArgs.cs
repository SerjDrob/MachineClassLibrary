using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine
{
    public class BitmapEventArgs : EventArgs
    {
        public BitmapImage Image  { get; init; }

        public BitmapEventArgs(BitmapImage image)
        {
            Image = image;
        }
    }
}