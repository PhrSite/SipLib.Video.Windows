/////////////////////////////////////////////////////////////////////////////////////
//  File:   VideoSender.cs                                          17 Apr 24 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace SipLib.Video.Windows;

using SipLib.Rtp;
using SipLib.Video;
using SipLib.Sdp;

using SIPSorceryMedia.FFmpeg;
using FFmpeg.AutoGen;
using SipLib.Logging;

/// <summary>
/// Class for sending video to an endpoint using an RtpChannel. This class can be used to send H264 or VP8
/// video frames to a remote endpoint using an RtpChannel.
/// </summary>
public class VideoSender
{
    private RtpChannel m_RtpChannel;
    private AVCodecID m_CodecID = AVCodecID.AV_CODEC_ID_FIRST_UNKNOWN;
    private FFmpegVideoEncoder m_VideoEncoder;

    private int m_EncodingErrors = 0;
    private const int MAX_ENCODING_ERRORS = 10;

    private VideoRtpSender? m_RtpSender = null;

    private bool m_Shutdown = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="VideoMd">MediaDescription that specifies the negotiated video codec to use to
    /// encode video frames.</param>
    /// <param name="rtpChannel">RtpChannel to use to send encoded video data packets.</param>
    public VideoSender(MediaDescription VideoMd, RtpChannel rtpChannel)
    {
        m_RtpChannel = rtpChannel;

        m_VideoEncoder = new FFmpegVideoEncoder();

        // Determine the video encoder and the RTP packetizer type to use.
        if (VideoMd.RtpMapAttributes.Count > 0)
        {
            RtpMapAttribute Rma = VideoMd.RtpMapAttributes[0];
            string EncoderName = Rma.EncodingName!.ToUpper();
            if (EncoderName == "H264")
            {
                m_CodecID = AVCodecID.AV_CODEC_ID_H264;
                m_RtpSender = new H264RtpSender(Rma.PayloadType, 30, m_RtpChannel.Send);
            }
            else if (EncoderName == "VP8")
            {
                m_CodecID = AVCodecID.AV_CODEC_ID_VP8;
                m_RtpSender = new VP8RtpSender(Rma.PayloadType, 30, m_RtpChannel.Send);
            }
            else
            {   // Unknown video codec
                SipLogger.LogError($"Unknown video codec: {EncoderName}");
            }
        }
    }

    /// <summary>
    /// This method must be called before the RtpChannel has been shut down. After this method is called,
    /// shutdown the RtpChannel, then call the Dispose() method.
    /// </summary>
    public void Shutdown()
    {
        m_Shutdown = true;
    }

    /// <summary>
    /// Releases resources held by the encoder. 
    /// </summary>
    public void Dispose()
    {
        m_VideoEncoder.Dispose();
    }

    /// <summary>
    /// Sends a video frame on the RtpChannel by encoding it and packaging it into RTP packets.
    /// </summary>
    /// <param name="Width">The width in pixels of the video frame to send.</param>
    /// <param name="Height">The height in pixels of the video frame to send.</param>
    /// <param name="fps">The number of frames per second of the captured video.</param>
    /// <param name="bytes">A byte array containing the raw video data. The format of this array depends on the
    /// pixelFormat parameter.</param>
    /// <param name="pixelFormat">The pixel format of the video frame</param>
    public unsafe void SendVideoFrame(int Width, int Height, int fps, byte[] bytes, AVPixelFormat pixelFormat)
    {
        if (m_CodecID == AVCodecID.AV_CODEC_ID_FIRST_UNKNOWN)
            return;

        if (m_Shutdown == true)
            return;

        byte[]? EncodedBytes = null;
        try
        {
            fixed (byte* pFrame = bytes)
            {
                EncodedBytes = m_VideoEncoder.Encode(m_CodecID, pFrame, Width, Height, fps, false, pixelFormat);
            }
        }
        catch (Exception ex)
        {
            m_EncodingErrors += 1;
            if (m_EncodingErrors < MAX_ENCODING_ERRORS)
                SipLogger.LogError(ex, "Failed to encode a video frame");
        }

        if (EncodedBytes == null)
            return;

        if (m_RtpSender != null)
            m_RtpSender.SendEncodedFrame(EncodedBytes);
    }
}
