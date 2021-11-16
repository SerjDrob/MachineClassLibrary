using MachineClassLibrary.Laser;
using MachineClassLibrary.Laser.Entities;
using MachineClassLibrary.Machine.MotionDevices;
using MachineClassLibrary.VideoCapture;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.Machine.Machines
{    
    public class LaserMachine : PCI124XXMachine, IHasCamera, IMarkLaser
    {
        private readonly IMarkLaser _markLaser;
        private readonly IVideoCapture _videoCapture;

        public LaserMachine(ExceptionsAgregator exceptionsAgregator, MotionDevicePCI1240U motionDevice, IMarkLaser markLaser, IVideoCapture videoCapture) : base(exceptionsAgregator, motionDevice)
        {
            Guard.IsNotNull(markLaser, nameof(markLaser));
            Guard.IsNotNull(videoCapture, nameof(videoCapture));
            _markLaser = markLaser;
            _videoCapture = videoCapture;
            _videoCapture.OnBitmapChanged += _videoCapture_OnBitmapChanged;
        }

        private void _videoCapture_OnBitmapChanged(BitmapImage bitmapImage)
        {
            OnVideoSourceBmpChanged?.Invoke(this, new BitmapEventArgs(bitmapImage));
        }

        public bool IsMarkDeviceInit => _markLaser.IsMarkDeviceInit;

        public event EventHandler<BitmapEventArgs> OnVideoSourceBmpChanged;

        public void CloseMarkDevice()
        {
            _markLaser.CloseMarkDevice();
        }

        public void FreezeVideoCapture()
        {
            _videoCapture.FreezeCameraImage();
        }

        public void InitMarkDevice()
        {
            _markLaser.InitMarkDevice();
        }

        public async Task<bool> PierceObjectAsync(IPerforatorBuilder perforatorBuilder)
        {
            return await _markLaser.PierceObjectAsync(perforatorBuilder);
        }

        public void SetMarkDeviceParams()
        {
            _markLaser.SetMarkDeviceParams();
        }

        public void StartVideoCapture(int ind)
        {
            _videoCapture.StartCamera(ind);
        }
    }
}
