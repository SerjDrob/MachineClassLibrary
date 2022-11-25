using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace MachineClassLibrary.VideoCapture
{
    public class USBCamera : IVideoCapture
    {
        private VideoCaptureDevice _localCamera;
        private int _localCameraIndex;
        private int _localCameraCapabilities;
        private bool _isStarted = false;
        private string _currentMonikerString;
        public Dictionary<int, (string, string[])> AvaliableVideoCaptureDevices { get; private set; }
        public bool IsVideoCaptureConnected { get; private set; } = false;

        public string VideoCaptureMessage => _errorMessage;

        private string _errorMessage;

        private List<VideoCaptureDevice> _videoCaptureDevices;
        private const string queryString = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";
        private object _eventLocker = new object();
        public USBCamera()
        {
            _videoCaptureDevices = GetVideoCaptureDevices();
            var watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery(queryString);
            watcher.Query = query;
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Start();
        }

        public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Task.Delay(100).Wait();
            _videoCaptureDevices = GetVideoCaptureDevices();

            if (_isStarted & !(_localCamera?.IsRunning ?? false))
            {
                StartCamera(_localCameraIndex, _localCameraCapabilities);
            }
        }
        private List<VideoCaptureDevice> GetVideoCaptureDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var videoCaptureDevices = new List<VideoCaptureDevice>();
            if (devices.Count != 0)
            {
                AvaliableVideoCaptureDevices = new();
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
                        AvaliableVideoCaptureDevices.Add(i, (devices[i].MonikerString, caps));
                    }
                }
            }
            return videoCaptureDevices;
        }
        public void FreezeCameraImage()
        {
            _localCamera.SignalToStop();
        }

        public void StartCamera(int ind, int capabilitiesInd = 0)
        {
            _isStarted = true;
            _localCameraIndex = ind;
            _localCameraCapabilities = capabilitiesInd;
            try
            {
                Guard.IsGreaterThan(AvaliableVideoCaptureDevices?.Count ?? 0, 0, nameof(_videoCaptureDevices.Count));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _errorMessage = $"Device count is 0";
                return;
            }
            Guard.IsInRange(ind, 0, AvaliableVideoCaptureDevices.Count, nameof(ind));
            try
            {
                Guard.IsInRange(capabilitiesInd, 0, AvaliableVideoCaptureDevices[ind].Item2.Length, nameof(capabilitiesInd));
            }
            catch (ArgumentOutOfRangeException)
            {
                capabilitiesInd = 0;
            }
            _currentMonikerString = AvaliableVideoCaptureDevices[ind].Item1;
            _localCamera = new VideoCaptureDevice(_currentMonikerString);
            if (!_localCamera.IsRunning)
            {
                _localCamera.VideoResolution = _localCamera.VideoCapabilities[capabilitiesInd];
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
            return new FilterInfoCollection(FilterCategory.VideoInputDevice).Count;
        }

        private async void HandleNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                var filter = new Mirror(false, false);
                using var img = (Bitmap)eventArgs.Frame.Clone();
                filter.ApplyInPlace(img);

                var ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);

                ms.Seek(0, SeekOrigin.Begin);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(bitmap, _errorMessage));
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(40).ConfigureAwait(false);
        }

    }
}