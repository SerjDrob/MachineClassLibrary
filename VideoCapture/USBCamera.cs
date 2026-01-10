using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MachineClassLibrary.Miscellaneous;
using OpenCvSharp;

namespace MachineClassLibrary.VideoCapture;

public sealed class USBCamera : WatchableDevice, IVideoCapture, ICameraPropertyControl
{
    private static readonly (int w, int h)[] CommonResolutions =
    {
        (640, 480),
        (800, 600),
        (1280, 720),
        (1920, 1080)
    };
    private static readonly Dictionary<CameraProperty, VideoCaptureProperties>
    _propertyMap = new()
    {
        { CameraProperty.Brightness,   VideoCaptureProperties.Brightness },
        { CameraProperty.Contrast,     VideoCaptureProperties.Contrast },
        { CameraProperty.Saturation,   VideoCaptureProperties.Saturation },
        { CameraProperty.Hue,          VideoCaptureProperties.Hue },
        { CameraProperty.Gain,         VideoCaptureProperties.Gain },
        { CameraProperty.Exposure,     VideoCaptureProperties.Exposure },
        { CameraProperty.Focus,        VideoCaptureProperties.Focus },
        { CameraProperty.Zoom,         VideoCaptureProperties.Zoom },
        { CameraProperty.Sharpness,    VideoCaptureProperties.Sharpness },
        { CameraProperty.Gamma,        VideoCaptureProperties.Gamma },
        { CameraProperty.WhiteBalance, VideoCaptureProperties.WhiteBalanceBlueU }
    };
    private static readonly int[] CommonFps = { 15, 30, 60 };
    private readonly MemoryPool<byte> _memoryPool = MemoryPool<byte>.Shared;
    private OpenCvSharp.VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private readonly Stopwatch _frameTimer = new();

    private bool _freeze;
    private bool _mirrorX;
    private bool _mirrorY;

    private int _currentDeviceIndex;
    private int _targetFps = 30;
    private string _message = string.Empty;

    public Dictionary<int, (string name, string[] capabilities)> AvailableVideoCaptureDevices { get; } = new();

    public bool IsVideoCaptureConnected { get; private set; }
    public string VideoCaptureMessage => _message;
    public bool AdjustWidthToHeight { get; set; }

    public IReadOnlyCollection<CameraProperty> SupportedProperties
    {
        get
        {
            if (_capture is null || !_capture.IsOpened())
                return Array.Empty<CameraProperty>();

            var list = new List<CameraProperty>();

            foreach (var kv in _propertyMap)
            {
                double value = _capture.Get(kv.Value);

                // OpenCV возвращает 0 или -1 если свойство не поддерживается
                if (!double.IsNaN(value) && value != -1)
                    list.Add(kv.Key);
            }

            return list;
        }
    }


    public event EventHandler<VideoCaptureEventArgs>? OnBitmapChanged;
    public event EventHandler? CameraPlugged;

    public USBCamera()
    {
        RefreshDevices();
    }

    // ------------------------------------------------------------

    public int GetVideoCaptureDevicesCount()
        => AvailableVideoCaptureDevices.Count;

    public int GetVideoCapabilitiesCount()
        => AvailableVideoCaptureDevices.TryGetValue(_currentDeviceIndex, out var d)
            ? d.capabilities.Length
            : 0;

    public void StartCamera(int deviceIndex, int capabilityIndex = 0)
    {
        StopCamera();
        _currentDeviceIndex = deviceIndex;

        _capture = new OpenCvSharp.VideoCapture(deviceIndex);
        if (!_capture.IsOpened())
        {
            _message = "Camera not found";
            RaiseFrame(null);
            return;
        }

        ApplyCapability(deviceIndex, capabilityIndex);

        _cts = new CancellationTokenSource();
        IsVideoCaptureConnected = true;
        _message = string.Empty;

        Task.Run(() => CaptureLoop(_cts.Token));
        DeviceOK(this);
    }

    public void StopCamera()
    {
        _cts?.Cancel();
        _capture?.Release();
        _capture?.Dispose();
        _capture = null;

        IsVideoCaptureConnected = false;
    }

    public void FreezeCameraImage() => _freeze = true;
    public void UnFreezeCamera() => _freeze = false;

    public void SetCameraMirror(bool mirrorX, bool mirrorY)
        => (_mirrorX, _mirrorY) = (mirrorX, mirrorY);

    public void InvokeSettings()
    {
        // intentionally empty (no cross-platform equivalent)
    }

    // ------------------------------------------------------------
    // Device discovery & capabilities
    // ------------------------------------------------------------

    private void RefreshDevices()
    {
        AvailableVideoCaptureDevices.Clear();

        for (int camIndex = 0; camIndex < 10; camIndex++)
        {
            using var cap = new OpenCvSharp.VideoCapture(camIndex);
            if (!cap.IsOpened())
                continue;

            var caps = new HashSet<string>();

            foreach (var (w, h) in CommonResolutions)
            {
                foreach (var fps in CommonFps)
                {
                    cap.Set(VideoCaptureProperties.FrameWidth, w);
                    cap.Set(VideoCaptureProperties.FrameHeight, h);
                    cap.Set(VideoCaptureProperties.Fps, fps);

                    int rw = (int)cap.Get(VideoCaptureProperties.FrameWidth);
                    int rh = (int)cap.Get(VideoCaptureProperties.FrameHeight);
                    int rfps = (int)cap.Get(VideoCaptureProperties.Fps);

                    if (rw == w && rh == h)
                        caps.Add($"{rw} x {rh} {rfps}fps");
                }
            }

            if (caps.Count == 0)
                caps.Add("Default");

            AvailableVideoCaptureDevices[camIndex] =
                ($"Camera {camIndex}", caps.ToArray());
        }
    }

    private void ApplyCapability(int deviceIndex, int capabilityIndex)
    {
        if (!AvailableVideoCaptureDevices.TryGetValue(deviceIndex, out var d))
            return;

        var caps = d.capabilities;
        if (capabilityIndex < 0 || capabilityIndex >= caps.Length)
            return;

        var cap = caps[capabilityIndex];
        if (cap == "Default")
            return;

        // "1280 x 720 30fps"
        var parts = cap.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var size = parts[0].Split('x');

        int w = int.Parse(/*size[0]*/parts[0]);
        int h = int.Parse(/*size[1]*/parts[2]);
        int fps = int.Parse(parts[3].Replace("fps", ""));

        _capture!.Set(VideoCaptureProperties.FrameWidth, w);
        _capture!.Set(VideoCaptureProperties.FrameHeight, h);
        _capture!.Set(VideoCaptureProperties.Fps, fps);
        _targetFps = fps;
    }

    // ------------------------------------------------------------
    // Capture loop
    // ------------------------------------------------------------

    private void CaptureLoop(CancellationToken token)
    {
        using var mat = new Mat();
        _frameTimer.Restart();

        while (!token.IsCancellationRequested)
        {
            long frameStart = _frameTimer.ElapsedMilliseconds;

            if (!_capture!.Read(mat) || mat.Empty())
            {
                HandleDisconnect();
                break;
            }

            if (_freeze)
                continue;

            ProcessFrame(mat);

            var frame = CreateVideoFrame(mat);
            RaiseFrame(frame);

            LimitFps(frameStart, token);
        }
    }

    private Mat ProcessFrame(Mat src)
    {
        Mat mat = src;

        if (_mirrorX)
            Cv2.Flip(mat, mat, FlipMode.Y);
        if (_mirrorY)
            Cv2.Flip(mat, mat, FlipMode.X);

        if (AdjustWidthToHeight)
        {
            int size = Math.Min(mat.Width, mat.Height);
            var roi = new Rect(
                (mat.Width - size) / 2,
                0,
                size,
                size);

            mat = new Mat(mat, roi);
        }

        return mat;
    }

    private unsafe VideoFrame CreateVideoFrame(Mat mat)
    {
        if (!mat.IsContinuous())
            mat = mat.Clone();

        int length = (int)(mat.Total() * mat.ElemSize());
        var owner = _memoryPool.Rent(length);

        fixed (byte* dst = owner.Memory.Span)
        {
            Buffer.MemoryCopy(
                (void*)mat.Data,
                dst,
                length,
                length);
        }

        return new VideoFrame(
            mat.Width,
            mat.Height,
            VideoPixelFormat.Bgr24,
            owner,
            length);
    }

    private void LimitFps(long frameStart, CancellationToken token)
    {
        long elapsed = _frameTimer.ElapsedMilliseconds - frameStart;
        long target = 1000 / Math.Max(1, _targetFps);

        if (elapsed < target)
            Task.Delay((int)(target - elapsed), token).Wait(token);
    }

    private void HandleDisconnect()
    {
        IsVideoCaptureConnected = false;
        _message = "Camera disconnected";

        RaiseFrame(null);

        Task.Delay(1000).ContinueWith(_ =>
        {
            RefreshDevices();
            if (AvailableVideoCaptureDevices.ContainsKey(_currentDeviceIndex))
                StartCamera(_currentDeviceIndex);
        });
    }

    private void RaiseFrame(VideoFrame? frame)
    {
        OnBitmapChanged?.Invoke(
            this,
            new VideoCaptureEventArgs(frame, _message, _freeze));
    }

    // ------------------------------------------------------------

    public override void CureDevice()
    {
        StartCamera(_currentDeviceIndex);
    }

    public override void AskHealth()
    {
    }

    public bool TrySet(CameraProperty property, double value)
    {
        if (_capture is null || !_capture.IsOpened())
            return false;

        if (!_propertyMap.TryGetValue(property, out var cvProp))
            return false;

        try
        {
            return _capture.Set(cvProp, value);
        }
        catch
        {
            return false;
        }
    }

    public bool TryGet(CameraProperty property, out double value)
    {
        value = default;

        if (_capture is null || !_capture.IsOpened())
            return false;

        if (!_propertyMap.TryGetValue(property, out var cvProp))
            return false;

        try
        {
            value = _capture.Get(cvProp);
            return !double.IsNaN(value) && value != -1;
        }
        catch
        {
            return false;
        }
    }

}

