using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using MachineClassLibrary.VideoCapture;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace DicingBlade.Classes
{
    public class USBCamera : IVideoCapture
    {
        private VideoCaptureDevice _localCamera;
        private int _localCameraIndex;
        private int _localCameraCapabilities;
        private bool _isStarted = false;
        public Dictionary<int, (string, string[])> AvaliableDevices { get; private set; }
        private List<VideoCaptureDevice> _videoCaptureDevices;
        private ManagementEventWatcher watcher = new ManagementEventWatcher();
        private const string queryString = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";
        public USBCamera()
        {
            _videoCaptureDevices = GetVideoCaptureDevices();

            var query = new WqlEventQuery(queryString);
            watcher.EventArrived += Watcher_EventArrived;

        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            _videoCaptureDevices = GetVideoCaptureDevices();
            if (_isStarted & !_localCamera.IsRunning)
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
                AvaliableDevices = new();
                for (int i = 0; i < devices.Count; i++)
                {
                    var device = new VideoCaptureDevice(devices[i].MonikerString);
                    videoCaptureDevices.Add(device);
                    var caps = new string[device.VideoCapabilities.Length];
                    for (int n = 0; n < device.VideoCapabilities.Length; n++)
                    {
                        var cap = device.VideoCapabilities[n];
                        caps[n] = $"{cap.FrameSize.Width} X {cap.FrameSize.Height} {cap.FrameRate}fps";
                    }
                    AvaliableDevices.Add(i, (devices[i].MonikerString, caps));
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
            Guard.IsGreaterThan(AvaliableDevices.Count, 0, nameof(_videoCaptureDevices.Count));
            Guard.IsInRange(ind, 0, AvaliableDevices.Count - 1, nameof(ind));
            try
            {
                Guard.IsInRange(capabilitiesInd, 0, AvaliableDevices[ind].Item2.Length - 1, nameof(capabilitiesInd));
            }
            catch (ArgumentOutOfRangeException)
            {
                capabilitiesInd = 0;
            }
            _localCamera = new VideoCaptureDevice(AvaliableDevices[ind].Item1);
            if (!_localCamera.IsRunning)
            {
                _localCamera.VideoResolution = _localCamera.VideoCapabilities[capabilitiesInd]; //8
                _localCamera.NewFrame += HandleNewFrame;
                _localCamera.Start();
                _localCameraIndex = ind;
                _localCameraCapabilities = capabilitiesInd;
            }
            else
            {
                throw new Exception("No device found");
            }
        }
        public int GetVideCapabilitiesCount() => _localCamera?.VideoCapabilities.Length ?? 0;

        public void StopCamera()
        {
            _isStarted = false;
            _localCamera?.Stop();
        }

        public event BitmapHandler OnBitmapChanged;

        public int GetDevicesCount()
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
                OnBitmapChanged?.Invoke(bitmap);
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(40).ConfigureAwait(false);
        }

    }
}