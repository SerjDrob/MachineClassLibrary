using System;
using System.Buffers;

public readonly struct VideoFrame : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public VideoPixelFormat PixelFormat { get; }
    public ReadOnlyMemory<byte> Data { get; }

    private readonly IMemoryOwner<byte>? _owner;

    public VideoFrame(
        int width,
        int height,
        VideoPixelFormat pixelFormat,
        IMemoryOwner<byte> owner,
        int length)
    {
        Width = width;
        Height = height;
        PixelFormat = pixelFormat;
        _owner = owner;
        Data = owner.Memory.Slice(0, length);
    }

    public void Dispose()
    {
        _owner?.Dispose();
    }
}


/*
using System;
using System.Windows.Media.Imaging;

namespace MachineClassLibrary.VideoCapture
{
    public class VideoCaptureEventArgs : EventArgs
    {
        public VideoCaptureEventArgs(BitmapImage image, string errorMessage, bool imageFreezed)
        {
            Image = image;
            ErrorMessage = errorMessage;
            ImageFreezed = imageFreezed;
        }

        public BitmapImage Image { get; init; }
        public string ErrorMessage { get; init; }
        public bool ImageFreezed { get; init; }
    }
}
*/
