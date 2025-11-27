/////////////////////////////////////////////////////////////////////////////////////
//  File:   StaticImageCapture.cs                                   15 Apr 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

using SipLib.Logging;
using SIPSorceryMedia.FFmpeg;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using System.Diagnostics;

namespace SipLib.Video.Windows;

/// <summary>
/// Class for reading a static image from a JPEG file and sending the image bytes to a remote endpoint
/// </summary>
public class StaticImageCapture : IVideoCapture
{
    private Bitmap? m_Bitmap = null;
    private byte[]? m_FrameBytes = null;
    private int m_FramesPerSecond;
    private System.Threading.Timer? m_Timer;
    private bool m_Started = false;

    private int m_Width;
    private int m_Height;

    private const AVPixelFormat OutputPixelFormat = AVPixelFormat.AV_PIX_FMT_YUV420P;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="jpegFile">The file path to the JPEG file containing the static image to read.</param>
    /// <param name="framesPerSecond">Frames per second. Must be between 1 and 60. If outside of this range,
    /// then a default value of 30 frames/second will be used.</param>
    /// <param name="Width">The desired image width in pixels</param>
    /// <param name="Height">The desired image height in pixels</param>
    public StaticImageCapture(string jpegFile, int framesPerSecond, int Width, int Height)
    {
        if (framesPerSecond < 1 || framesPerSecond > 60)
            framesPerSecond = 30;

        m_Width = Width;
        m_Height = Height;

        m_FramesPerSecond = framesPerSecond;
        if (File.Exists(jpegFile) == true)
        {
            try
            {
                Bitmap bitmap = new Bitmap(jpegFile);
                if (bitmap.Width != Width || bitmap.Height != Height)
                    m_Bitmap = new Bitmap(bitmap, Width, Height);
                else
                    m_Bitmap = bitmap;

                if (m_Bitmap.PixelFormat == PixelFormat.Format24bppRgb || m_Bitmap.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    AVPixelFormat pixelFormat = m_Bitmap.PixelFormat == PixelFormat.Format24bppRgb ?
                        AVPixelFormat.AV_PIX_FMT_BGR24 : AVPixelFormat.AV_PIX_FMT_BGRA;
                    m_FrameBytes = GetFrameBytes(m_Bitmap, pixelFormat);
                }
                else
                    SipLogger.LogError($"The Bitmap pixel format is not 24 bit RGB or 32 bit ARGB for file: {jpegFile}");
            }
            catch (Exception ex)
            {
                SipLogger.LogError(ex, $"Unable to read the bitmap image file: {jpegFile}");
            }
        }
    }

    // See: the Example at https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.lockbits?view=dotnet-plat-ext-8.0
    private byte[]? GetFrameBytes(Bitmap bmp, AVPixelFormat pixelFormat)
    {
        byte[]? bytes = null;
        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
        // Get the address of the first line.
        IntPtr ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap.
        int size = Math.Abs(bmpData.Stride) * bmp.Height;
        bytes = new byte[size];

        // Copy the RGB values into the array.
        Marshal.Copy(ptr, bytes, 0, size);

        bmp.UnlockBits(bmpData);
        byte[]? Yuv420PBytes = null;
        VideoFrameConverter FrameConverter;
        try
        {
            
            FrameConverter = new VideoFrameConverter(bmp.Width, bmp.Height, pixelFormat, bmp.Width, 
                bmp.Height, OutputPixelFormat);
            Yuv420PBytes = FrameConverter.ConvertToBuffer(bytes);
        }
        catch (Exception ex)
        {
            SipLogger.LogError(ex, "Unable to convert the RGB image to YUV420P");
        }

        return Yuv420PBytes;
    }

    /// <summary>
    /// This event is fired periodically at the specified frame rate.
    /// </summary>
    public event FrameBitmapReadyDelegate? FrameBitmapReady = null;

    /// <summary>
    /// This event is fired periodically at the specified frame rate.
    /// </summary>
    public event FrameReadyDelegate? FrameReady = null;

    /// <summary>
    /// Starts the capture process
    /// </summary>
    #pragma warning disable CS1998
    public async Task<bool> StartCapture()
    {
        if (m_Started == true)
            return true;

        m_Timer = new System.Threading.Timer(OnTimerElapsed, null, 0, 1000 / m_FramesPerSecond);
        m_Started = true;

        return true;
    }
    #pragma warning restore CS1998

    /// <summary>
    /// Stops the capture process
    /// </summary>
    public Task StopCapture()
    {
        if (m_Started == false)
            return Task.CompletedTask; ;

        if (m_Timer != null)
        {
            m_Timer.Dispose();
            m_Timer = null;
        }

        m_Started = false;
        return Task.CompletedTask;
    }



    private void OnTimerElapsed(object? state)
    {
        if (m_Bitmap == null)
            return;

        FrameBitmapReady?.Invoke(m_Bitmap);

        if (m_FrameBytes != null)
        {
            try
            {
                FrameReady?.Invoke(m_Width, m_Height, m_FramesPerSecond, m_FrameBytes,
                    AVPixelFormat.AV_PIX_FMT_YUV420P);
            }
            catch (InvalidOperationException Ioe)
            {
                Debug.WriteLine(Ioe.ToString());
            }
        }
    }
}
