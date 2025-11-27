/////////////////////////////////////////////////////////////////////////////////////
//  File:   VideoReceiver.cs                                        19 Apr 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

using SipLib.Rtp;
using SipLib.Sdp;
using SIPSorceryMedia.FFmpeg;
using SIPSorceryMedia.Abstractions;

using FFmpeg.AutoGen;
using SipLib.Logging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SipLib.Video.Windows;

/// <summary>
/// Class that processes received RTP packets containing fragmented encoded video frame data. This class decodes
/// the data and fires an event when a Bitmap for a full frame of video has been received.
/// </summary>
public class VideoReceiver
{
    /// <summary>
    /// This event is fired when a Bitmap for a full video frame is ready.
    /// </summary>
    public event FrameBitmapReadyDelegate? FrameReady = null;

    private VideoRtpReceiver? m_VideoRtpReceiver = null;
    private AVCodecID m_CodecID = AVCodecID.AV_CODEC_ID_FIRST_UNKNOWN;
    private FFmpegVideoEncoder m_VideoEncoder;
    private RtpChannel m_RtpChannel;
    private bool m_Shutdown = false;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="VideoMd">MediaDescription that describes the negotiated video codec.</param>
    /// <param name="rtpChannel">RtpChannel that the video data packets will be received on.</param>
    public VideoReceiver(MediaDescription VideoMd, RtpChannel rtpChannel)
    {
        m_RtpChannel = rtpChannel;
        m_VideoEncoder = new FFmpegVideoEncoder();

        // Determine the video decoder and the RTP depacketizer type to use.
        if (VideoMd.RtpMapAttributes.Count > 0)
        {
            RtpMapAttribute Rma = VideoMd.RtpMapAttributes[0];
            string EncoderName = Rma.EncodingName!.ToUpper();
            if (EncoderName == "H264")
            {
                m_CodecID = AVCodecID.AV_CODEC_ID_H264;
                m_VideoRtpReceiver = new H264RtpReceiver();
            }
            else if (EncoderName == "VP8")
            {
                m_CodecID = AVCodecID.AV_CODEC_ID_VP8;
                m_VideoRtpReceiver = new VP8RtpReceiver();
            }
            else
            {   // Unknown video codec
                SipLogger.LogError($"Unknown video codec: {EncoderName}");
            }
        }

        m_RtpChannel.RtpPacketReceived += OnRtpPacketReceived;
    }

    private void OnRtpPacketReceived(RtpPacket packet)
    {
        if (m_Shutdown == true)
            return;

        if (m_VideoRtpReceiver != null)
        {
            byte[]? encodedFrame = m_VideoRtpReceiver.ProcessRtpPacket(packet);
            if (encodedFrame != null)
            {
                int Width, Height;
                List<RawImage>? images = m_VideoEncoder.DecodeFaster(m_CodecID, encodedFrame, out Width, out Height);
                if (images != null && images.Count > 0)
                {
                    foreach (RawImage image in images )
                    {
                        ProcessRawImage(image, Width, Height);
                    }
                }
            }
        }
    }

    private void ProcessRawImage(RawImage image, int Width, int Height)
    {
        byte[] rawImageBytes = image.GetBuffer();
        PixelFormat pxFormat = PixelFormat.Format24bppRgb;
        Bitmap bmp = new Bitmap(Width,Height, pxFormat);
        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, pxFormat);

        IntPtr pNative = bmpData.Scan0;
        Marshal.Copy(rawImageBytes, 0, pNative, rawImageBytes.Length);
        bmp.UnlockBits(bmpData);

        FrameReady?.Invoke(bmp);
    }

    /// <summary>
    /// This method must be called before the RtpChannel has been shut down. After this method is called,
    /// shutdown the RtpChannel, then call the Dispose() method.
    /// </summary>
    public void Shutdown()
    {
        m_Shutdown = true;
        m_RtpChannel.RtpPacketReceived -= OnRtpPacketReceived;
    }

    /// <summary>
    /// Releases resources held by the decoder.
    /// </summary>
    public void Dispose()
    {
        m_VideoEncoder.Dispose();
    }
}
