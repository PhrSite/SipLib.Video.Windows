# Introduction
This class library provides classes that provide the following functionality for using video in SIP calls.
1. Enumerate video capture devices (i.e. cameras) on a Windows computer. 
1. A class for capturing video from a camera.
1. A VideoSender class that encodes video frames using a H.264 or a VP8 encoder and sends fragmented video frames to a remote endpoint using the Real Time Protocol (RTP).
1. A VideoReceiver class that receives fragmented encoded video frames from an IP network using RTP, defragments the RTP packets into encoded frames, decodes the frames and provides video image data to an application that can display it.

This class library can be used with the [SipLib](https://github.com/PhrSite/SipLib) class library to build Voice over IP (VoIP) applications requiring video support in the Windows environment. It may be used by applications or other class libraries on Windows 10, Windows 11 or Windows Server. 

This class library uses the H.264 and VP8 codecs from the FFMPEG libraries. The NuGet package for this class library distributes the required C language DLL files. It uses the C# wrapper classes provided by the [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen) class library.

# Documentation
The documentation pages are located at https://phrsite.github.io/SipLib.Video.Windows. The documentation web site includes class documentation and articles that explain the usage of the classes in this library.

# Installation
This class library is available on NuGet.

To install it from the .NET CLI, type:

```
dotnet add package SipLib.Video.Windows --version X.X.X
```
"X.X.X" is the version number of the packet to add.

To install using the NuGET Package Manager Command window, type:

```
NuGet\Install-Package SipLib.Video.Windows --version X.X.X
```
Or, you can install it from the Visual Studio GUI.

1. Right click on a project
2. Select Manage NuGet Packages
3. Search for SipLib.Video.Windows
4. Click on Install

# Dependancies
This project has direct dependencies on the following NuGet packages.

1. FFmpeg.AutoGen (7.0.0)
1. SipLib (0.0.4 or later)
1. SIPSorceryMedia.FFmpeg (8.0.10)
1. SIPSorceryMedia.Abstractions (8.0.7)

# Included FFMPEG DLLs
This class library distributes the following FFMPEG DLLs. These files are located in the FFMPEG directory.

1. avcodec-61.dll
1. avdevice-61.dll
1. avfilter-10.dll
1. avformat-61.dll
1. avutil-59.dll
1. postproc-58.dll
1. swresample-5.dll
1. swscale-8.dll

If the version of FFmpeg.AutoGen or the version of SIPSorceryMedia.FFmeg is changed, then these files must be updated to use the appropriate versions used by these NuGet packages.

The GitHub respository for FFmpeg.AutoGen contains the latest versions of these DLLs in the FFmpeg/bin/x64 folder. You can download the latest versions of the DLLs by cloning the FFmpeg.AutoGen repository to your local development machine.

