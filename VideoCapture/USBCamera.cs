using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Toolkit.Diagnostics;

namespace MachineClassLibrary.VideoCapture;

public class USBCamera : WatchableDevice, IVideoCapture
{
    private OpenCvSharp.VideoCapture _localCamera;
    private int _localCameraIndex;
    private int _localCameraCapabilities;
    private bool _isStarted = false;
    private string _currentMonikerString = string.Empty;
    private bool _freezeImage;
    private Bitmap? _avaloniaBitmap;
    private bool _mirrorX;
    private bool _mirrorY;

    public Dictionary<int, (string, string[])> AvailableVideoCaptureDevices { get; private set; }
        = new Dictionary<int, (string Moniker, string[] Capabilities)>();

    public bool IsVideoCaptureConnected { get; private set; } = false;
    public string VideoCaptureMessage => _errorMessage;

    private string _errorMessage = string.Empty;

    private List<OpenCvSharp.VideoCapture> _videoCaptureDevices = new();

    public event EventHandler<VideoCaptureEventArgs>? OnBitmapChanged;
    public event EventHandler<Mat>? OnRawBitmapChanged; // Заменяет OnRawBitmapChanged
    public event EventHandler? CameraPlugged;

    public USBCamera()
    {
        _videoCaptureDevices = GetVideoCaptureDevices();
    }

    private List<OpenCvSharp.VideoCapture> GetVideoCaptureDevices()
    {
        var videoCaptureDevices = new List<OpenCvSharp.VideoCapture>();
        AvailableVideoCaptureDevices.Clear();

        // OpenCvSharp позволяет перебрать устройства через индексы
        for (int i = 0; i < 10; i++) // Проверим первые 10 камер
        {
            var cap = new OpenCvSharp.VideoCapture(i);
            if (cap.IsOpened())
            {
                var caps = GetVideoCapabilities(cap);
                AvailableVideoCaptureDevices.Add(i, (i.ToString(), caps));
                videoCaptureDevices.Add(cap);
                cap.Release(); // Закрываем, т.к. будем открывать заново при старте
            }
            else
            {
                cap.Release();
            }
        }

        return videoCaptureDevices;
    }

    private string[] GetVideoCapabilities(OpenCvSharp.VideoCapture cap)
    {
        // OpenCvSharp не предоставляет список разрешений напрямую
        // Вместо этого, можно задать разрешение и проверить, сработает ли
        var resolutions = new[]
        {
            (640, 480), (800, 600), (1280, 720), (1920, 1080)
        };

        var available = new List<string>();
        foreach (var (w, h) in resolutions)
        {
            cap.Set(VideoCaptureProperties.FrameWidth, w);
            cap.Set(VideoCaptureProperties.FrameHeight, h);
            if (cap.IsOpened())
            {
                var actualW = cap.Get(VideoCaptureProperties.FrameWidth);
                var actualH = cap.Get(VideoCaptureProperties.FrameHeight);
                available.Add($"{(int)actualW} X {(int)actualH} {cap.Get(VideoCaptureProperties.Fps)}fps");
            }
        }

        return available.ToArray();
    }

    public void FreezeCameraImage()
    {
        if (!_freezeImage)
        {
            _freezeImage = true;
        }

        OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_avaloniaBitmap, _errorMessage, _freezeImage));
    }

    public void UnFreezeCamera()
    {
        if (_freezeImage)
        {
            _freezeImage = false;
        }
    }

    public void StartCamera(int ind, int capabilitiesInd = 0)
    {
        _freezeImage = false;
        _isStarted = true;
        _localCameraIndex = ind;
        _localCameraCapabilities = capabilitiesInd;

        try
        {
            Guard.IsGreaterThan(AvailableVideoCaptureDevices.Count, 0, nameof(AvailableVideoCaptureDevices.Count));
        }
        catch (ArgumentOutOfRangeException)
        {
            _errorMessage = "Device count is 0";
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

        // Открываем камеру
        _localCamera = new OpenCvSharp.VideoCapture(ind);

        if (_localCamera.IsOpened())
        {
            // Устанавливаем разрешение (если поддерживается)
            var resolution = ParseResolution(AvailableVideoCaptureDevices[ind].Item2[capabilitiesInd]);
            _localCamera.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
            _localCamera.Set(VideoCaptureProperties.FrameHeight, resolution.Height);

            // Запускаем захват
            _ = Observable.Interval(TimeSpan.FromMilliseconds(33)) // ~30 FPS
                .Subscribe(_ => CaptureFrame());

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

    private (int Width, int Height) ParseResolution(string cap)
    {
        var parts = cap.Split(' ');
        var wh = parts[0].Split('X');
        return (int.Parse(wh[0]), int.Parse(wh[1]));
    }

    private void CaptureFrame()
    {
        if (_freezeImage || _localCamera == null || !_localCamera.IsOpened())
            return;

        Mat frame = _localCamera.RetrieveMat();

        if (frame.Empty())
            return;

        OnRawMatChanged?.Invoke(this, frame);

        var processed = ProcessFrame(frame);
        _avaloniaBitmap = ConvertMatToAvaloniaBitmap(processed);// processed.ToAvaloniaBitmap();

        OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_avaloniaBitmap, _errorMessage, _freezeImage));

        frame.Dispose();
        processed.Dispose();
    }
    private Bitmap? ConvertMatToAvaloniaBitmap(Mat mat)
    {
        if (mat.Empty())
            return null;
        using var ms = mat.ToMemoryStream(ext:".png");
        return new Bitmap(ms);
    }
    private Mat ProcessFrame(Mat input)
    {
        Mat output = input.Clone();

        // Применяем зеркалирование
        if (_mirrorX || _mirrorY)
        {
            var flipCode = _mirrorX && _mirrorY ? FlipMode.XY :
                           _mirrorX ? FlipMode.X :
                           FlipMode.Y;
            Cv2.Flip(output, output, flipCode);
        }

        // Применяем контраст (заменяет ContrastCorrection)
        Cv2.ConvertScaleAbs(output, output, alpha: 1.2, beta: 10); // Пример контраста

        // Обрезка (если нужна)
        if (AdjustWidthToHeight)
        {
            var width = output.Width;
            var height = output.Height;
            var x1 = (width - height) / 2;
            var rect = new Rect(x1, 0, height, height);
            output = new Mat(output, rect);
        }

        return output;
    }

    public void StopCamera()
    {
        _isStarted = false;
        _localCamera?.Release();
        _localCamera?.Dispose();
        _localCamera = null;
    }

    public int GetVideoCapabilitiesCount() => AvailableVideoCaptureDevices.ContainsKey(_localCameraIndex) ?
        AvailableVideoCaptureDevices[_localCameraIndex].Item2.Length : 0;

    public int GetVideoCaptureDevicesCount() => AvailableVideoCaptureDevices.Count;

    public bool AdjustWidthToHeight { get; set; }

    public void SetCameraMirror(bool mirrorX, bool mirrorY) => (_mirrorX, _mirrorY) = (mirrorX, mirrorY);

    public void InvokeSettings()
    {
        // OpenCvSharp не предоставляет UI-окно настроек
        // Оставляем как заглушку
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
/*
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Toolkit.Diagnostics;


namespace MachineClassLibrary.VideoCapture;

public class USBCamera : WatchableDevice, IVideoCapture
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

    public USBCamera() 
    {
        _videoCaptureDevices = GetVideoCaptureDevices();
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
        if (!_freezeImage)
        {
            _freezeImage = true;
            _localCamera.NewFrame -= HandleNewFrame;
        }
        OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_bitmap, _errorMessage, _freezeImage));
    }

    public void UnFreezeCamera()
    {
        if (_freezeImage)
        {
            _freezeImage = false;
            _localCamera.NewFrame += HandleNewFrame;
        }
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
        catch (ArgumentOutOfRangeException)
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
        OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(null, _errorMessage, _freezeImage));
    }

    public int GetVideoCapabilitiesCount() => _localCamera?.VideoCapabilities.Length ?? 0;

    public void StopCamera()
    {
        _isStarted = false;
        _localCamera.SignalToStop();
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
            if (_bitmap is not null) OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(_bitmap, _errorMessage, _freezeImage));
        }
        catch (Exception)
        {
        }

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
*/
