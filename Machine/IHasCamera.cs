using MachineClassLibrary.VideoCapture;
using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine
{
    public interface IHasCamera
    {
        public event EventHandler<BitmapEventArgs> OnVideoSourceBmpChanged;
        public void StartVideoCapture(int ind);
        public void FreezeVideoCapture();
    }
}