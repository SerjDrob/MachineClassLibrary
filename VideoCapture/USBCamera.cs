using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Toolkit.Diagnostics;


namespace MachineClassLibrary.VideoCapture
{
    public class USBCamera : PlugMeWatcher, IVideoCapture
    {
        private VideoCaptureDevice _localCamera;
        private int _localCameraIndex;
        private int _localCameraCapabilities;
        private bool _isStarted = false;
        private string _currentMonikerString;
        public Dictionary<int, (string, string[])> AvailableVideoCaptureDevices
        {
            get; private set;
        }
        public bool IsVideoCaptureConnected { get; private set; } = false;

        public string VideoCaptureMessage => _errorMessage;

        private string _errorMessage;

        private List<VideoCaptureDevice> _videoCaptureDevices;
        private bool _freezeImage;
        private BitmapImage _bitmap;

        public USBCamera() : base("VID_AA47", "PID_1301")
        {
            _videoCaptureDevices = GetVideoCaptureDevices();
            WaitAndPlugMe(() =>
            {
                _videoCaptureDevices = GetVideoCaptureDevices();
                //StartCamera(_localCameraIndex, _localCameraCapabilities);
            });
            DevicePlugged += USBCamera_DevicePlugged;
        }

        private void USBCamera_DevicePlugged(object sender, EventArgs e) => CameraPlugged?.Invoke(sender,e);

        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
        public event EventHandler CameraPlugged;
        private List<VideoCaptureDevice> GetVideoCaptureDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var videoCaptureDevices = new List<VideoCaptureDevice>();
            if (devices.Count != 0)
            {
                AvailableVideoCaptureDevices = new();
                for (int i = 0; i < devices.Count; i++)
                {
                    var device = new VideoCaptureDevice(devices[i].MonikerString);
                    if (device.VideoCapabilities?.Length > 0)
                    {
                        videoCaptureDevices.Add(device);
                        var caps = new string[device.VideoCapabilities.Length];
                        for (int n = 0; n < device.VideoCapabilities.Length; n++)
                        {
                            var cap = device.VideoCapabilities[n];
                            caps[n] = $"{cap.FrameSize.Width} X {cap.FrameSize.Height} {cap.FrameRate}fps";
                        }
                        AvailableVideoCaptureDevices.Add(i, (devices[i].MonikerString, caps));
                    }
                }
            }
            return videoCaptureDevices;
        }
        public void FreezeCameraImage()
        {
            _localCamera.SignalToStop();
            _localCamera.NewFrame -= HandleNewFrame;
            _freezeImage = true;
        }

        public void StartCamera(int ind, int capabilitiesInd = 0)
        {
            _freezeImage = false;
            _isStarted = true;
            _localCameraIndex = ind;
            _localCameraCapabilities = capabilitiesInd;
            try
            {
                Guard.IsGreaterThan(AvailableVideoCaptureDevices?.Count ?? 0, 0, nameof(_videoCaptureDevices.Count));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _errorMessage = $"Device count is 0";
                return;
            }
            Guard.IsInRange(ind, 0, AvailableVideoCaptureDevices.Count, nameof(ind));
            try
            {
                Guard.IsInRange(capabilitiesInd, 0, AvailableVideoCaptureDevices[ind].Item2.Length, nameof(capabilitiesInd));
            }
            catch (ArgumentOutOfRangeException)
            {
                capabilitiesInd = 0;
            }
            _currentMonikerString = AvailableVideoCaptureDevices[ind].Item1;
            _localCamera = new VideoCaptureDevice(_currentMonikerString);
            if (!_localCamera.IsRunning)
            {
                _localCamera.VideoResolution = _localCamera.VideoCapabilities[capabilitiesInd];
                //_localCamera.SetCameraProperty(CameraControlProperty.Exposure,100, CameraControlFlags.Manual);
                _localCamera.PlayingFinished += _localCamera_PlayingFinished;
                _localCamera.NewFrame += HandleNewFrame;
                _localCamera.Start();

                IsVideoCaptureConnected = true;
                _errorMessage = string.Empty;
                _localCameraIndex = ind;
                _localCameraCapabilities = capabilitiesInd;
            }
            else
            {
                throw new Exception("No device found");
            }
        }

        private void _localCamera_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            _localCamera.NewFrame -= HandleNewFrame;
            IsVideoCaptureConnected = false;
            _errorMessage = "Device has been switched off";
            OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(null, _errorMessage));
        }

        public int GetVideoCapabilitiesCount() => _localCamera?.VideoCapabilities.Length ?? 0;

        public void StopCamera()
        {
            _isStarted = false;
            _localCamera.SignalToStop();
            _localCamera.WaitForStop();//.Stop();
        }

        public int GetVideoCaptureDevicesCount()
        {
            var collection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return collection.Count;
        }

        public bool AdjustWidthToHeight
        {
            get; set;
        }

        private async void HandleNewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            var filter = new ContrastCorrection();

            Bitmap ApplyAdjustWidthIfEnable(Bitmap bitmap)
            {
                if (!AdjustWidthToHeight) return bitmap;
                var width = eventArgs.Frame.Width;
                var height = eventArgs.Frame.Height;
                var x1 = (width - height) / 2;
                var crop = new Crop(new Rectangle(x1, 0, height, height));
                return crop.Apply(bitmap);
            }

            try
            {
                if (!_freezeImage)
                {
                    using var img = ApplyAdjustWidthIfEnable((Bitmap)eventArgs.Frame.Clone());
                    filter.ApplyInPlace(img);
                    var ms = new MemoryStream();
                    img.Save(ms, ImageFormat.Bmp);

                    ms.Seek(0, SeekOrigin.Begin);

                    _bitmap = new BitmapImage();
                    _bitmap.BeginInit();
                    _bitmap.StreamSource = ms;
                    _bitmap.EndInit();
                    _bitmap.Freeze();
                }
                if (_bitmap is not null) OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_bitmap, _errorMessage));
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(40).ConfigureAwait(false);
        }

        public void InvokeSettings()
        {
            _localCamera?.DisplayPropertyPage(IntPtr.Zero);
        }
    }
}