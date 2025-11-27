# Sending and Receiving Video Media

# Getting Started
The classes in the SipLib.Video.Windows use the FFMPEG C DLLs to encode and decode video data. The FFmpeg.AutoGen class library contains C# wrapper classes for the FFMPEG C libraries.

Applications must initialize the C# bindings to the FFMPEG C libraries by calling the static function called `VideoUtils.InitializeFFMPEG()`.

This function must be called once upon application startup.

If `VideoUtils.InitializeFFMPEG()` returns true then the application may use the classes in the SipLib.Video.Windows namespace to process video.

If `VideoUtils.InitializeFFMPEG()` returns false then the application must not use any of the classes in the SipLib.Video.Windows namespace. Attempts to use the classes in this namespace will cause an InvalidOperationException to be thrown.

# <a name="SendingVideo">Sending Video</a>
Applications can use the VideoSender class to send captured video frames over an IP network. The VideoSender class performs the following functions.
1. It receives raw video frames and encodes them
1. Packetizes the encoded video frames.
1. Sends the packetized, encoded video frames over the IP network as RTP packets using an [RtpChannel](https://phrsite.github.io/SipLib/api/SipLib.Rtp.RtpChannel.html) object.

The VideoSender class supports the H.264 and VP8 video codecs.

To use the VideoSender class, an application creates an instance of it, then it passes raw video frames received from the WindowsCameraCapture class's FrameReady event.

The following is the declaration of the constructor of the VideoSender class.
```
public VideoSender(MediaDescription VideoMd, RtpChannel rtpChannel)
```

The [MediaDescription](https://phrsite.github.io/SipLib/api/SipLib.Sdp.MediaDescription.html) object contains the video codec information. The application should pass in the MediaDescription that was used in the SIP OK resopnse to the INVITE request.

The VideoSender class will use the [RtpChannel](https://phrsite.github.io/SipLib/api/SipLib.Rtp.RtpChannel.html) object to send RTP packets to the remote endpoint. The application is responsible for creating the RtpChannel object, starting it by calling its StartListening() method and for calling its Shutdown() method when the media session ends.

The article entitled [The Real Time Protocol](https://phrsite.github.io/SipLib/articles/RealTimeProtocol.html) explains how to create an RtpChannel.

After creating a VideoSender object, the application must call its SendVideoFrame() method each time it receives a raw video frame from the WindowsCameraCapture that it is using. The declaration of the SendVideoFrame() method is:
```
public unsafe void SendVideoFrame(int Width, int Height, int fps, byte[] bytes, 
    AVPixelFormat pixelFormat)
```
This method must be called from within the event handler for the FrameReady event of the WindowsCameraCapture object.

To stop sending video, the application must call the following methods of the VideoSender object it created.
1. Shutdown()
1. Dispose()

**Note: The VideoSender class does not call the Shutdown() method of the RtpChannel object.**

# <a name="ReceivingVideo">Receiving Video</a>
Applications can use the VideoReceiver class to receive video media from the IP network. The VideoReceiver class performs the following functions.
1. Receives fragmented, encoded video frames in RtpPackets from the IP network.
1. De-fragments the RtpPackets to build an complete encoded video frame.
1. Decodes the encoded video frame to create a raw video image.
1. Fires an event that passes a Bitmap to the application.

The VideoReceiver class supports the H.264 and VP8 video codecs.

Follow these steps to use the VideoReceiver class.
1. Construct an instance of it.
1. Hook the FrameReady event of the VideoReceiver object.

The declaration of the constructor for the VideoReceiver class is:
```
public VideoReceiver(MediaDescription VideoMd, RtpChannel rtpChannel);
```
The [MediaDescription](https://phrsite.github.io/SipLib/api/SipLib.Sdp.MediaDescription.html) object contains the video codec information. The application should pass in the MediaDescription that was used in the SIP OK resopnse to the INVITE request.

The VideoReceiver class will use the [RtpChannel](https://phrsite.github.io/SipLib/api/SipLib.Rtp.RtpChannel.html) object to receive RTP packets from the remote endpoint. The application is responsible for creating the RtpChannel object, starting it by calling its StartListening() method and for calling its Shutdown() method when the media session ends.

The article entitled [The Real Time Protocol](https://phrsite.github.io/SipLib/articles/RealTimeProtocol.html) explains how to create an RtpChannel.

The declaration of the delegate type for the FrameReady event is:
```
public delegate void FrameBitmapReadyDelegate(Bitmap bitmap);
```

To stop sending video, the application must call the following methods of the VideoReceiver object it created.
1. Shutdown()
1. Dispose()

**Note: The VideoReceiver class does not call the Shutdown() method of the RtpChannel object.**
