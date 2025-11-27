/////////////////////////////////////////////////////////////////////////////////////
//  File:   IVideoCapture.cs                                        15 Apr 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

using FFmpeg.AutoGen;

namespace SipLib.Video.Windows;

/// <summary>
/// Delegate definition for the FrameBitmapReady event of classes that implement the IVideoCapture interface.
/// </summary>
/// <param name="bitmap">System.Drawing.Bitmap object that WinForms applications can use to display a video frame.</param>
public delegate void FrameBitmapReadyDelegate(Bitmap bitmap);

/// <summary>
/// Delegate definition for the FrameReady event of classes that implement the IVideoCapture interface.
/// </summary>
/// <param name="Width">Width of the video frame in pixels</param>
/// <param name="Height">Height of the video frame in pixels</param>
/// <param name="fps">Frame rate in frames per second</param>
/// <param name="bytes">Raw frame bytes. The format of the data in this array depends on th pixelFormat parameter</param>
/// <param name="pixelFormat">Specifies the pixel format.</param>
public delegate void FrameReadyDelegate(int Width, int Height, int fps, byte[] bytes, AVPixelFormat pixelFormat);

/// <summary>
/// Interface that video capture classes must implement.
/// </summary>
public interface IVideoCapture
{
    /// <summary>
    /// This event is fired when a frame bitmap is available for preview display.
    /// </summary>
    public event FrameBitmapReadyDelegate? FrameBitmapReady;

    /// <summary>
    /// This event is fired when a full frame is ready for encoding and transmission to a remote endpoint.
    /// </summary>
    public event FrameReadyDelegate? FrameReady;

    /// <summary>
    /// Starts the video capture process. Hook the events before calling this method.
    /// </summary>
    public Task<bool> StartCapture();

    /// <summary>
    /// Stops the video capture process. Unhook the events before calling this method.
    /// </summary>
    public Task StopCapture();
}
