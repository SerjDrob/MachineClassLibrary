using System;

public sealed class VideoCaptureEventArgs : EventArgs
{
    public VideoFrame? Frame { get; }
    public string Message { get; }
    public bool ImageFreezed { get; }

    public VideoCaptureEventArgs(
        VideoFrame? frame,
        string message,
        bool imageFreezed)
    {
        Frame = frame;
        Message = message;
        ImageFreezed = imageFreezed;
    }
}
