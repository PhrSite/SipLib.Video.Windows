/////////////////////////////////////////////////////////////////////////////////////
//  File:   VideoDeviceEnumerator.cs                                7 Feb 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace SipLib.Video.Windows;
using SipLib.Logging;
using CameraCapture;

/// <summary>
/// Class that enumerates the available video devices available on a Windows computer.
/// </summary>
public class VideoDeviceEnumerator
{
    /// <summary>
    /// Gets a dictionary of the available video devices (video sources) and the list of supported
    /// formats for each device.
    /// </summary>
    /// <returns>The string key is the name of the video capture device. The dictionary value is a
    /// list of available video formats that the device supports. The dictionary will be empty if there
    /// is no camera or another error occurred.</returns>
    static public async Task<Dictionary<string, List<VideoDeviceFormat>>> GetVideoFrameSources()
    {
        Dictionary<string, List<VideoDeviceFormat>> Devices = new Dictionary<string, List<VideoDeviceFormat>>();
        MediaCapture mediaCapture = new MediaCapture();
        MediaCaptureInitializationSettings captureInitSettings = new MediaCaptureInitializationSettings()
        {
            SharingMode = MediaCaptureSharingMode.SharedReadOnly,
            StreamingCaptureMode = StreamingCaptureMode.Video
        };

        try
        {
            await mediaCapture.InitializeAsync(captureInitSettings);
        }
        catch (Exception ex)
        {
            SipLogger.LogError(ex, "Failed to initialize the MediaCapture object");
            return Devices;
        }

        foreach (MediaFrameSource Mfs in mediaCapture.FrameSources.Values)
        {
            List<VideoDeviceFormat> formats = new List<VideoDeviceFormat>();
            foreach (MediaFrameFormat format in Mfs.SupportedFormats)
            {
                if (format.MajorType == "Video")
                {
                    if (format.Subtype != "MJPG")
                    {
                        VideoDeviceFormat Vdf = new VideoDeviceFormat();
                        Vdf.SubType = format.Subtype;
                        Vdf.Width = format.VideoFormat.Width;
                        Vdf.Height = format.VideoFormat.Height;
                        Vdf.Framerate = format.FrameRate.Numerator / format.FrameRate.Denominator;
                        formats.Add(Vdf);
                    }
                }
            }

            if (formats.Count > 0)
                Devices.Add(Mfs.Info.DeviceInformation.Name, formats);
        }

        mediaCapture.Dispose();
        return Devices;
    }
}
