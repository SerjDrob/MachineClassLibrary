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
using MachineClassLibrary.Machine.Machines;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Toolkit.Diagnostics;
using OpenCvSharp;


namespace MachineClassLibrary.VideoCapture
{
    public class USBCamera : WatchableDevice, /*PlugMeWatcher,*/ IVideoCapture
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
        private bool _mirrorX;
        private bool _mirrorY;

        public USBCamera() //: base("VID_AA47", "PID_1301")
        {
            _videoCaptureDevices = GetVideoCaptureDevices();
            //WaitAndPlugMe(() =>
            //{
            //    _videoCaptureDevices = GetVideoCaptureDevices();
            //    //StartCamera(_localCameraIndex, _localCameraCapabilities);
            //});
            //DevicePlugged += USBCamera_DevicePlugged;
        }

        private void USBCamera_DevicePlugged(object sender, EventArgs e) => CameraPlugged?.Invoke(sender, e);

        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
        public event EventHandler<Bitmap> OnRawBitmapChanged;
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
                            caps[n] = $"{cap.FrameSize.Width} X {cap.FrameSize.Height} {cap.AverageFrameRate}fps";
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
                DeviceOK(this);
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
            HasHealthProblem(_errorMessage, null, this);
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
        public void SetCameraMirror(bool mirrorX, bool mirrorY) => (_mirrorX, _mirrorY) = (mirrorX, mirrorY);
        private async void HandleNewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            var filter = new ContrastCorrection();
            var mirror = new Mirror(_mirrorX, _mirrorY);



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
                    try
                    {
                        using var img = ApplyAdjustWidthIfEnable((Bitmap)eventArgs.Frame.Clone());
                        filter.ApplyInPlace(img);
                        mirror.ApplyInPlace(img);
                        OnRawBitmapChanged?.Invoke(this, img);
                        var ms = new MemoryStream();
                        img.Save(ms, ImageFormat.Bmp);

                        ms.Seek(0, SeekOrigin.Begin);

                        _bitmap = new BitmapImage();
                        _bitmap.BeginInit();
                        _bitmap.StreamSource = ms;
                        _bitmap.EndInit();
                        _bitmap.Freeze();

                    }
                    catch (AccessViolationException ex)
                    {

                        Console.WriteLine(ex.Message);
                    }



                }
                if (_bitmap is not null) OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_bitmap, _errorMessage));
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(40).ConfigureAwait(false);
        }

        public float GetBlurIndex()
        {
            var src = OpenCvSharp.Extensions.BitmapConverter.ToMat(BitmapImage2Bitmap(_bitmap));
            return calcBlurriness(src);
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        static float calcBlurriness(Mat src)
        {
            Mat Gx = new Mat();
            Mat Gy = new Mat();
            Cv2.Sobel(src, Gx, MatType.CV_32F, 1, 0);
            Cv2.Sobel(src, Gy, MatType.CV_32F, 0, 1);
            double normGx = Cv2.Norm(Gx);
            double normGy = Cv2.Norm(Gy);
            double sumSq = normGx * normGx + normGy * normGy;
            return (float)(1.0 / (sumSq / (src.Size().Height * src.Size().Width) + 1e-6));
        }

        public void InvokeSettings()
        {
            _localCamera?.DisplayPropertyPage(IntPtr.Zero);
        }

        public override void CureDevice()
        {
            _videoCaptureDevices = GetVideoCaptureDevices();
            StartCamera(_localCameraIndex, _localCameraCapabilities);
        }

        public override void AskHealth()
        {
            throw new NotImplementedException();
        }
    }
}
