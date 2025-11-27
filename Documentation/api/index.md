# SipLib.Video.Windows Namespaces

## CameraCapture
The CameraCapture namespace contains classes for performing the following functions.
1. Enumerating cameras on a Windows PC
1. Capturing video frames from a selected camera.

The process of enumerating the available cameras involves getting a list of cameras that are attached to a PC and then getting a list of each video format available for each camera. Once enumeration is complete an application can select a camera and a desired video format for capturing video frames from a camera.

The classes in this namespace use the Windows.Media.Capture namespace to work with cameras attached to a Windows PC.

The topic entitled [Enumerating Cameras and Selecting a Camera](~/articles/CapturingVideo.md#EnumeratingCameras) describes how to enumerate camera devices and select one for use.

The topic entitled [Using the WindowsCameraCapture Class](~/articles/CapturingVideo.md#WindowsCameraCapture) describes how to capture and process video frames from a selected camera.

## SipLib.Video.Windows
This namespace contains classes for the performing following functions.
1. Encode video frames captured from a camera and send them to a remote endpoint over the network for SIP based Voice over IP (VoIP) applications.
1. Receive encoded video frames from an IP network and decode them for display.
1. Sending a static image stored in a JPEG file.
