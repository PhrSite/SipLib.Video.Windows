/////////////////////////////////////////////////////////////////////////////////////
//  File:   VideoSourceSettings.cs                                  7 Feb 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CameraCapture;

/// <summary>
/// Settings class for the selected video source (capture devices)
/// </summary>
public class VideoSourceSettings
{
    /// <summary>
    /// Name of the selected video capture device. A value of null means that no video device has
    /// been selected.
    /// </summary>
    public string? SelectedDeviceName { get; set; } = null;
    /// <summary>
    /// Video format settings for the video device.
    /// </summary>
    public VideoDeviceFormat DeviceFormat { get; set; } = new VideoDeviceFormat();

    /// <summary>
    /// Constructor
    /// </summary>
    public VideoSourceSettings()
    {
    }
}

/// <summary>
/// Video format for the video capture device.
/// </summary>
public class VideoDeviceFormat
{
    /// <summary>
    /// Sub-Type such as RGB, NV12 or YUY2
    /// </summary>
    public string SubType { get; set; } = string.Empty;
    /// <summary>
    /// Frame with in pixels
    /// </summary>
    public uint Width { get; set; } = 0;
    /// <summary>
    /// Frame height in pixels.
    /// </summary>
    public uint Height { get; set; } = 0;
    /// <summary>
    /// Frame rate in frames per second.
    /// </summary>
    public uint Framerate { get; set; } = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    public VideoDeviceFormat()
    {
    }
}