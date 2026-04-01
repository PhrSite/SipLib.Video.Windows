/////////////////////////////////////////////////////////////////////////////////////
//  File:   WindowsCameraCapture.cs                                 4 Apr 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Capture.Frames;

using System.Diagnostics;
using SIPSorceryMedia.FFmpeg;
using FFmpeg.AutoGen;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using SipLib.Video.Windows;
using SipLib.Logging;

namespace CameraCapture;

/// <summary>
/// Class for capturing video frames from a camera on a Windows computer.
/// </summary>
public class WindowsCameraCapture : IVideoCapture
{
    private MediaCapture m_mediaCapture;
    private MediaCaptureInitializationSettings m_captureInitSettings;
    private VideoSourceSettings m_VideoSourceSettings;
    private bool m_Started = false;
    private int m_FramesPerSecond = 30;

    private VideoFrameConverter? m_BitmapFrameConverter = null;
    private VideoFrameConverter? m_FrameConverter = null;
    private MediaFrameReader? m_MediaFrameReader = null;

    // Storage for the a Buffer to use for building a Bitmap object
    private Windows.Storage.Streams.Buffer m_Buf;

    private AVPixelFormat m_InputPixelFormat = AVPixelFormat.AV_PIX_FMT_NONE;

    private const AVPixelFormat OutputPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;

    /// <summary>
    /// Event that is fired when a full video frame Bitmap object is ready for display.
    /// </summary>
    public event FrameBitmapReadyDelegate? FrameBitmapReady = null;

    /// <summary>
    /// Event that is fired when a full frame has been captured.
    /// </summary>
    public event FrameReadyDelegate? FrameReady;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="videoSourceSettings">Video device and format settings.</param>
    public WindowsCameraCapture(VideoSourceSettings videoSourceSettings)
    {
        m_VideoSourceSettings = videoSourceSettings;
        m_mediaCapture = new MediaCapture();

        m_captureInitSettings = new MediaCaptureInitializationSettings()
        {
            SharingMode = MediaCaptureSharingMode.SharedReadOnly,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu,
            StreamingCaptureMode = StreamingCaptureMode.Video
        };
        
        // Assume a worst case of 3 bytes/pixel
        uint BytesPerFrame = videoSourceSettings.DeviceFormat.Width * videoSourceSettings.DeviceFormat.Height * 3;
        m_Buf = new Windows.Storage.Streams.Buffer(BytesPerFrame);
    }

    /// <summary>
    /// Starts capturing video frames.
    /// </summary>
    /// <returns>Returns true if the capture was successfully started, or false if an error occurred.</returns>
    public async Task<bool> StartCapture()
    {
        if (m_Started == true)
            return true;

        VideoDeviceFormat Vdf = m_VideoSourceSettings.DeviceFormat;
        try
        {
            await m_mediaCapture.InitializeAsync(m_captureInitSettings);
        }
        catch (Exception ex)
        {
            SipLogger.LogError(ex, "Failed to initialize the MediaCapture object");
            return false;
        }

        MediaFrameSource? chosenMfs = null;
        MediaFrameFormat? chosenFormat = null;
        foreach (MediaFrameSource Mfs in m_mediaCapture.FrameSources.Values)
        {
            foreach (MediaFrameFormat format in Mfs.SupportedFormats)
            {
                if (format.MajorType == "Video")
                {
                    if (format.Subtype == Vdf.SubType && format.VideoFormat.Width == Vdf.Width &&
                        format.VideoFormat.Height == Vdf.Height)
                    {
                        chosenMfs = Mfs;
                        chosenFormat = format;
                    }
                }
            }
        }

        if (chosenMfs == null || chosenFormat == null)
        {
            SipLogger.LogError("Unable to find the configured Media Frame Source or Media Frame Format");
            return false;   // Error condition
        }

        int Width = (int)chosenFormat.VideoFormat.Width;
        int Height = (int)chosenFormat.VideoFormat.Height;
        m_FramesPerSecond = (int) (chosenFormat.FrameRate.Numerator / chosenFormat.FrameRate.Denominator);
        BitmapSize Bs = new BitmapSize(chosenFormat.VideoFormat.Width, chosenFormat.VideoFormat.Height);

        m_InputPixelFormat = SubtypeStringToAVPixelFormat(chosenFormat.Subtype);
        string mediaSubType = SubtypeStringToMediaEncodingSubtype(chosenFormat.Subtype);

        m_BitmapFrameConverter = new VideoFrameConverter(Width, Height, m_InputPixelFormat, Width, Height, 
            AVPixelFormat.AV_PIX_FMT_BGR24);
        if (m_InputPixelFormat != OutputPixelFormat)
            m_FrameConverter = new VideoFrameConverter(Width, Height, m_InputPixelFormat, Width, Height,
                OutputPixelFormat);

        m_MediaFrameReader = await m_mediaCapture.CreateFrameReaderAsync(chosenMfs, mediaSubType, Bs);

        m_MediaFrameReader.FrameArrived += FrameArrived;
        await m_MediaFrameReader.StartAsync();

        m_Started = true;
        return true;
    }

    private AVPixelFormat SubtypeStringToAVPixelFormat(string subType)
    {
        AVPixelFormat format = AVPixelFormat.@AV_PIX_FMT_NONE;
        switch (subType)
        {
            case "NV12":
                format = AVPixelFormat.AV_PIX_FMT_NV12;
                break;
            case "YUY2":
                format = AVPixelFormat.AV_PIX_FMT_YUYV422;
                break;
        }

        return format;
    }

    private string SubtypeStringToMediaEncodingSubtype(string subType)
    {
        string mediaSubtype = MediaEncodingSubtypes.Nv12;
        switch (subType)
        {
            case "NV12":
                mediaSubtype = MediaEncodingSubtypes.Nv12;
                break;
            case "YUY2":
                mediaSubtype = MediaEncodingSubtypes.Yuy2;
                break;
        }

        return mediaSubtype;
    }

    /// <summary>
    /// Stops the capture process.
    /// </summary>
    /// <returns></returns>
    public async Task StopCapture()
    {
        if (m_MediaFrameReader != null)
        {
            m_MediaFrameReader.FrameArrived -= FrameArrived;
            await m_MediaFrameReader.StopAsync();
            m_MediaFrameReader.Dispose();
            m_MediaFrameReader = null;
        }

        if (m_mediaCapture != null)
        {
            m_mediaCapture.Dispose();
            m_mediaCapture = null!;
        }
    }

    private void FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        MediaFrameReference Mfr = sender.TryAcquireLatestFrame();
        if (Mfr == null)
            return;

        VideoMediaFrame Vmf = Mfr.VideoMediaFrame;
        SoftwareBitmap Sb = Vmf.SoftwareBitmap;
        if (Sb != null)
        {
            Sb.CopyToBuffer(m_Buf);
            byte[] bytes = m_Buf.ToArray();

            byte[] pOutputBytes;
            if (m_FrameConverter != null)
            {
                try
                {
                    pOutputBytes = m_FrameConverter.ConvertToBuffer(bytes);
                }
                catch (Exception ex)
                {
                    SipLogger.LogError(ex, "Exception in call to VideoFrameConverter.ConvertToBuffer()");
                    return;
                }
            }
            else
                pOutputBytes = bytes;

            FrameReady?.Invoke(Sb.PixelWidth, Sb.PixelHeight, m_FramesPerSecond, pOutputBytes, OutputPixelFormat);

            byte[] RgbBytes = m_BitmapFrameConverter!.ConvertToBuffer(bytes);
            Bitmap Bm = CreateBitmap(RgbBytes, Sb.PixelWidth, Sb.PixelHeight);
            FrameBitmapReady?.Invoke(Bm);

            Sb.Dispose();
        }
    }

    private Bitmap CreateBitmap(byte[] RGBFrame, int width, int height)
    {
        PixelFormat pxFormat = PixelFormat.Format24bppRgb;
        Bitmap bmp = new Bitmap(width, height, pxFormat);
        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pxFormat);

        IntPtr pNative = bmpData.Scan0;
        Marshal.Copy(RGBFrame, 0, pNative, RGBFrame.Length);
        bmp.UnlockBits(bmpData);
        return bmp;
    }
}
