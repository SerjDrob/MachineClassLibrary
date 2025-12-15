using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MachineClassLibrary.Machine.Machines;
using MachineClassLibrary.Miscellaneous;
using Microsoft.Toolkit.Diagnostics;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MachineClassLibrary.VideoCapture;

public class USBCamera2 : WatchableDevice, IVideoCapture // PlugMeWatcher
{
    private OpenCvSharp.VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private WriteableBitmap? _writeableBitmap;
    private Mat _currentFrame = new();
    private bool _isStarted;
    private bool _freezeImage;
    private bool _mirrorX, _mirrorY;
    private int _deviceIndex;
    private int _formatIndex;

    // Кэш устройств: Moniker отсутствует в OpenCV, используем индексы + имена
    public Dictionary<int, (string, string[])> AvailableVideoCaptureDevices { get; private set; } = [];
    public bool IsVideoCaptureConnected => _capture?.IsOpened() == true && _isStarted;
    public string VideoCaptureMessage => _errorMessage;
    private string _errorMessage = string.Empty;

    // События
    public event EventHandler<VideoCaptureEventArgs> OnBitmapChanged;
    public event EventHandler<System.Drawing.Bitmap> OnRawBitmapChanged; // для совместимости — можно заменить на OpenCvSharp.Mat при рефакторинге
    public event EventHandler CameraPlugged;

    public USBCamera2()
    {
        RefreshDeviceList();
    }

    // === Реализация IVideoCapture ===

    public bool AdjustWidthToHeight { get; set; }

    public void StartCamera(int ind, int capabilitiesInd = 0)
    {
        Guard.IsGreaterThanOrEqualTo(AvailableVideoCaptureDevices.Count, 1, nameof(AvailableVideoCaptureDevices));
        Guard.IsInRange(ind, 0, AvailableVideoCaptureDevices.Count - 1, nameof(ind));
        var (name, formats) = AvailableVideoCaptureDevices[ind];
        Guard.IsInRange(capabilitiesInd, 0, formats.Length - 1, nameof(capabilitiesInd));

        StopCamera(); // остановить предыдущую сессию

        try
        {
            _deviceIndex = ind;
            _formatIndex = capabilitiesInd;

            _capture = new OpenCvSharp.VideoCapture(ind, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
                throw new InvalidOperationException($"Не удалось открыть камеру #{ind}");

            // Применяем выбранный формат (например: "1280x720 MJPEG 30fps")
            ParseAndApplyFormat(formats[capabilitiesInd]);

            // Запуск фонового захвата
            _cts = new CancellationTokenSource();
            _isStarted = true;
            _ = Task.Run(() => CaptureLoop(_cts.Token));

            DeviceOK(this);
            //IsVideoCaptureConnected = true;
            _errorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Ошибка запуска камеры: {ex.Message}";
            HasHealthProblem(_errorMessage, ex, this);
            _capture?.Release();
            _capture = null;
            throw;
        }
    }

    public void StopCamera()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _capture?.Release();
        _capture?.Dispose();
        _capture = null;

        _isStarted = false;
        //IsVideoCaptureConnected = false;
    }

    public void FreezeCameraImage()
    {
        _freezeImage = true;
        // Кадры больше не обрабатываются, но источник не останавливается (чтобы не переподключать)
        RaiseBitmapChanged();
    }

    public void UnFreezeCamera()
    {
        _freezeImage = false;
    }

    public int GetVideoCaptureDevicesCount() => AvailableVideoCaptureDevices.Count;

    public int GetVideoCapabilitiesCount() =>
        AvailableVideoCaptureDevices.TryGetValue(_deviceIndex, out var dev) ? dev.Item2.Length : 0;

    public void SetCameraMirror(bool mirrorX, bool mirrorY) =>
        (_mirrorX, _mirrorY) = (mirrorX, mirrorY);

    public void InvokeSettings()
    {
        // OpenCV не поддерживает PropertyPage напрямую.
        // Возвращаемся к AForge *только для настроек* (легковесно и безопасно)
        try
        {
            var aforgeDevices = new AForge.Video.DirectShow.FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            if (_deviceIndex < aforgeDevices.Count)
            {
                var aforgeCam = new AForge.Video.DirectShow.VideoCaptureDevice(aforgeDevices[_deviceIndex].MonikerString);
                aforgeCam.DisplayPropertyPage(IntPtr.Zero);
                //aforgeCam?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Не удалось открыть настройки камеры: {ex.Message}";
        }
    }

    public float GetBlurIndex()
    {
        //if (_currentFrame.Empty())
            return 0f;

        //using var gray = new Mat();
        //Cv2.CvtColor(_currentFrame, gray, ColorConversionCodes.BGR2GRAY);

        //// Вариант: Laplacian variance (стандартный метод)
        //using var laplacian = new Mat();
        //Cv2.Laplacian(gray, laplacian, MatType.CV_64F);
        //var variance = Cv2.MeanStdDev(laplacian).Item2[0];
        //return (float)(variance * variance);
    }

    public override void CureDevice()
    {
        RefreshDeviceList();
        try
        {
            StartCamera(_deviceIndex, _formatIndex);
        }
        catch
        {
            // ошибка логируется внутри StartCamera
        }
    }

    public override void AskHealth()
    {
        // нет "health" в OpenCV, но можно проверить isOpened + попытаться прочитать кадр
        if (_capture?.IsOpened() == true)
        {
            using var testFrame = new Mat();
            if (_capture.Read(testFrame) && !testFrame.Empty())
            {
                DeviceOK(this);
                return;
            }
        }
        HasHealthProblem("Камера не отвечает", null, this);
    }

    // === Внутренние методы ===

    private void RefreshDeviceList()
    {
        AvailableVideoCaptureDevices.Clear();
        var deviceCount = GetDeviceCount(); // эвристика

        for (int i = 0; i < deviceCount; i++)
        {
            using var cap = new OpenCvSharp.VideoCapture(i, VideoCaptureAPIs.DSHOW);
            if (!cap.IsOpened()) continue;

            var name = GetDeviceName(i) ?? $"Camera {i}";
            var formats = GetAvailableFormats(cap).ToArray();
            if (formats.Length > 0)
                AvailableVideoCaptureDevices[i] = (name, formats);
        }

        // Опционально: триггер CameraPlugged при изменении списка
        // CameraPlugged?.Invoke(this, EventArgs.Empty);
    }

    private static int GetDeviceCount()
    {
        int count = 0;
        for (int i = 0; i < 10; i++) // обычно не > 5 камер
        {
            using var cap = new OpenCvSharp.VideoCapture(i, VideoCaptureAPIs.DSHOW);
            if (cap.IsOpened())
            {
                count = i + 1;
            }
            else
            {
                break; // предполагаем нумерацию без пропусков
            }
        }
        return count;
    }

    private static string? GetDeviceName(int index)
    {
        // OpenCV не даёт имя, но можно через Windows API или AForge (только чтение)
        try
        {
            var devices = new AForge.Video.DirectShow.FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            return devices.Count > index ? devices[index].Name : null;
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<string> GetAvailableFormats(OpenCvSharp.VideoCapture cap)
    {
        var candidates = new[]
        {
        (1920, 1080, "MJPEG"),
        (1280, 720,  "MJPEG"),
        (1024, 768,  "MJPEG"),
        (640,  480,  "MJPEG"),
        (1920, 1080, "YUY2"),
        (1280, 720,  "YUY2"),
        (640,  480,  "YUY2"),
        (320,  240,  "YUY2"),
    };

        foreach (var (w, h, fmt) in candidates)
        {
            string? formatString = null;

            try
            {
                // Сохраняем текущие настройки, чтобы не повредить cap
                var origW = cap.Get(VideoCaptureProperties.FrameWidth);
                var origH = cap.Get(VideoCaptureProperties.FrameHeight);
                var origFourCC = cap.Get(VideoCaptureProperties.FourCC);

                // Пробуем установить разрешение
                cap.Set(VideoCaptureProperties.FrameWidth, w);
                cap.Set(VideoCaptureProperties.FrameHeight, h);

                var actualW = (int)cap.Get(VideoCaptureProperties.FrameWidth);
                var actualH = (int)cap.Get(VideoCaptureProperties.FrameHeight);

                if (actualW == w && actualH == h)
                {
                    if (fmt == "MJPEG")
                    {
                        cap.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
                        var fourcc = cap.Get(VideoCaptureProperties.FourCC);
                        if ((int)fourcc == VideoWriter.FourCC('M', 'J', 'P', 'G'))
                            formatString = $"{w}x{h} MJPEG";
                        else
                            formatString = $"{w}x{h} YUY2";
                    }
                    else
                    {
                        // Для YUY2 явно сбрасываем в raw (не MJPEG)
                        cap.Set(VideoCaptureProperties.FourCC, 0); // 0 = default (чаще YUY2)
                        formatString = $"{w}x{h} YUY2";
                    }
                }

                // Восстанавливаем исходные параметры
                cap.Set(VideoCaptureProperties.FrameWidth, origW);
                cap.Set(VideoCaptureProperties.FrameHeight, origH);
                cap.Set(VideoCaptureProperties.FourCC, origFourCC);
            }
            catch
            {
                // пропускаем, formatString остаётся null
            }

            if (formatString != null)
                yield return formatString;
        }
    }

    private void ParseAndApplyFormat(string format)
    {
        // формат: "1280x720 MJPEG" или "640x480 YUY2"
        var parts = format.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1) return;

        var wh = parts[0].Split('x');
        if (wh.Length != 2) return;

        int w = int.Parse(wh[0]), h = int.Parse(wh[1]);
        _capture!.Set(VideoCaptureProperties.FrameWidth, w);
        _capture!.Set(VideoCaptureProperties.FrameHeight, h);

        if (parts.Length > 1 && parts[1].StartsWith("MJ"))
        {
            _capture.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
        }
        // FPS пока не трогаем — можно добавить, если нужно
    }

    private async void CaptureLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _capture?.IsOpened() == true)
        {
            if (!_capture.Read(_currentFrame) || _currentFrame.Empty())
                break;

            if (!_freezeImage)
            {
                // Обработка: контраст + зеркало (OpenCV-native)
                var processed = _currentFrame.Clone();

                try
                {

                    // Контраст (аналог ContrastCorrection)
                    Cv2.ConvertScaleAbs(processed, processed, alpha: 1.2, beta: 10); // alpha > 1 — ярче/контрастнее

                    // Зеркало
                    if (_mirrorX || _mirrorY)
                    {
                        FlipMode flipMode = (_mirrorX, _mirrorY) switch
                        {
                            (true, false) => FlipMode.Y,   // ← горизонтальное зеркало (UI: mirrorX)
                            (false, true) => FlipMode.X,   // ← вертикальное зеркало (UI: mirrorY)
                            (true, true) => FlipMode.XY,
                        };
                        Cv2.Flip(processed, processed, flipMode);
                    }

                    // Crop (AdjustWidthToHeight)
                    using var cropped = new Mat();
                    if (AdjustWidthToHeight && processed.Width > processed.Height)
                    {
                        int size = processed.Height;
                        int x = (processed.Width - size) / 2;
                        Cv2.GetRectSubPix(processed, new OpenCvSharp.Size(size, size), new Point2f(x + size / 2f, size / 2f), cropped);
                        processed = cropped;
                    }

                    // Конвертация в WPF (WriteableBitmap)
                    EnsureWriteableBitmap(processed.Width, processed.Height);
                    UpdateWriteableBitmap(processed);

                    // События
                    try
                    {
                        // Для совместимости — конвертируем в System.Drawing.Bitmap (осторожно: GC!)
                        using var bitmap = processed.ToBitmap(); // BitmapConverter: OpenCvSharp.Extensions
                        OnRawBitmapChanged?.Invoke(this, bitmap);
                    }
                    catch { /* ignore — fallback to WPF-only */ }
                }
                finally 
                {
                    processed?.Dispose();
                }
                RaiseBitmapChanged();
            }

            await Task.Delay(10, ct).ConfigureAwait(false); // ~100 FPS max; уменьшите при нагрузке (33 → 30 FPS)
        }

        // Выход: ошибка/отключение
        _errorMessage = "Поток захвата завершён";
        //IsVideoCaptureConnected = false;
        RaiseBitmapChanged();
    }

    private void EnsureWriteableBitmap(int width, int height)
    {
        if (_writeableBitmap is null ||
            _writeableBitmap.PixelWidth != width ||
            _writeableBitmap.PixelHeight != height)
        {
            _writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                // Внешний код (например, XAML) должен слушать изменение источника
                // Здесь — просто обновляем источник один раз при смене разрешения
                // Если Preview — Image: нужно передавать его сюда или через событие
            });
        }
    }

    private byte[]? _pixelBuffer;
    private readonly object _bufferLock = new();

    private void UpdateWriteableBitmap(Mat bgrMat)
    {
        Guard.IsNotNull(bgrMat, nameof(bgrMat));
        Guard.IsTrue(bgrMat.Channels() is 3 or 4, "Ожидался BGR (3) или BGRA (4)");

        int width = bgrMat.Width;
        int height = bgrMat.Height;
        EnsureWriteableBitmap(width, height);

        Mat? workingMat = null; // ← инициализируем null (обязательно!)

        try
        {
            if (bgrMat.Channels() == 4)
            {
                // Используем копию, чтобы иметь право Dispose (bgrMat — внешний)
                workingMat = bgrMat.Clone();
            }
            else
            {
                workingMat = new Mat();
                Cv2.CvtColor(bgrMat, workingMat, ColorConversionCodes.BGR2BGRA);
            }

            // Убеждаемся, что continuous
            if (!workingMat.IsContinuous())
            {
                var old = workingMat;
                workingMat = old.Clone();
                old.Dispose(); // old — точно не null (мы его создали/клонировали)
            }

            byte[] pixels = workingMat.ToBytes();

            _writeableBitmap!.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels,
                _writeableBitmap.BackBufferStride,
                0);
        }
        finally
        {
            // ✅ workingMat объявлена как null, поэтому workingMat?.Dispose() — безопасно
            workingMat?.Dispose();
        }
    }

    private void RaiseBitmapChanged()
    {
        // Конвертируем в BitmapImage — безопасно, freeze-нуто, для UI
        var bitmapImage = ConvertToBitmapImage(_writeableBitmap);

        // Отправляем в UI-поток
        Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            OnBitmapChanged?.Invoke(this, new VideoCaptureEventArgs(
                image: bitmapImage ?? new BitmapImage(), // или null, если конструктор допускает
                errorMessage: _errorMessage,
                imageFreezed: _freezeImage));
        }, System.Windows.Threading.DispatcherPriority.Background);
    }
    private BitmapImage? ConvertToBitmapImage(WriteableBitmap? wb)
    {
        if (wb == null)
            return null;

        try
        {
            // Создаём BitmapImage
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();

            // Используем MemoryStream как промежуточный буфер
            using var ms = new MemoryStream();

            // Кодируем как PNG (потеряless, поддержка alpha)
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wb));
            encoder.Save(ms);

            // Сбрасываем позицию
            ms.Position = 0;

            // Загружаем из потока
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // ← кэшируем в памяти, можно освободить StreamSource
            bitmapImage.EndInit();

            // Обязательно Freeze(), чтобы можно было использовать из любого потока
            bitmapImage.Freeze();

            // После Freeze() StreamSource можно отпустить — данные уже скопированы
            return bitmapImage;
        }
        catch (Exception ex)
        {
            // Логирование при необходимости
            // Console.WriteLine($"Ошибка конвертации: {ex}");
            return null;
        }
    }
    // IDisposable — при необходимости добавьте
}
