# The SipLib.Video.Windows Class Library
This class library provides classes that provide the following functionality for using video in SIP calls.
1. Enumerate video capture devices (i.e. cameras) on a Windows computer. 
1. A class for capturing video from a camera.
1. A VideoSender class that encodes video frames using a H.264 or a VP8 encoder and sends fragmented video frames to a remote endpoint using the Real Time Protocol (RTP).
1. A VideoReceiver class that receives fragmented encoded video frames from an IP network using RTP, defragments the RTP packets into encoded frames, decodes the frames and provides video image data to an application that can display it.

This class library can be used with the [SipLib](https://github.com/PhrSite/SipLib) class library to build Voice over IP (VoIP) applications requiring video support in the Windows environment. It may be used by applications or other class libraries on Windows 10, Windows 11 or Windows Server. 

See the article entitled [Capturing Video](~/articles/CapturingVideo.md) for a description of how to caputure video frames and use them.

See the article entitled [Sending and Receiving Video Media](~/articles/SendingAndReceivingVideo.md) for topics such as library initialization, sending video and receiving video.
