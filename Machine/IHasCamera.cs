using MachineClassLibrary.VideoCapture;
using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine
{
    public interface IHasCamera:IVideoCapture
    {
        //public event EventHandler<BitmapEventArgs> OnVideoSourceBmpChanged;
        //public void StartVideoCapture(int ind, int capabilitiesInd = 0);
        //public void FreezeVideoCapture();
        //public void StopVideoCapture();
        //public int GetCamerasCount();
        //public int GetCameraCapabilitiesCount();
    }
}