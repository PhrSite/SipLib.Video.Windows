# Capturing Video

# <a name="EnumeratingCameras">Enumerating Cameras and Selecting a Camera</a>
A Windows PC may have one or more cameras attached to it and each camera may support multiple video frame formats. In order to capture video frames from a camera an application must be able to select a camera and an appropriate video format to use.

Applications can use the GetVideoFrameSources() method of the VideoDeviceEnumerator class to get information about the video capabilities of a PC. The declaration of this function is:

```
static public async Task<Dictionary<string, List<VideoDeviceFormat>>> 
    GetVideoFrameSources();
```

This method returns a dictionary. The key is the name of a camera the the value of the dictionary if a list of available video device image formats supported by the camera.

The dictionary will be empty if the PC does not have a camera.

The [VideoDeviceFormat](~/api/CameraCapture.VideoDeviceFormat.yml) class contains properties that describe an image format. These properties include the format SubType (RGB, NV12, YUY2, etc), the image Width and Height and the Framerate (for example: 30 frames/sec.).

The properties of the VideoDeviceFormat class determine the image size and the bitrate of the video frames that will be captured from the camera and send over the IP network.

Applications can save the selected camera name and the selected video format in the class called [VideoSourceSettings](~/api/CameraCapture.VideoSourceSettings.yml). This class can be used with the WindowsCameraCapture class to configure image capture.

# <a name="WindowsCameraCapture">Using the WindowsCameraCapture Class</a>
Applications can use the WindowsCameraCapture class to capture video frames. The steps involved are:
1. Construct an instance of the WindowsCameraCaptrure class. The constructor takes an instance of the [VideoSourceSettings](~/api/CameraCapture.VideoSourceSettings.yml) class.
1. Hook the events of the WindowsCameraCapture object.
1. Call the StartCapture() method of the WindowsCameraCapture object.

The declaration of the StartCapture() method is:
```
public async Task<bool> StartCapture();
```

The StartCapture() method verifies that the video format specified in the VideoSourceSettings object passed in the constructor of the WindowsCameraCapture object exists. If it does not, then this method returns false to indicate that an error occurred. Otherwise, this method returns true to indicate the the capture process has started.

To stop capturing video frames, the application must perform the following steps.
1. Unhook the events of the WindowsCameraCapture object.
1. Call the StopCapture() method of the WindowsCameraCapture object.

The WindowsCameraCapture class provides two events that an application can hook to get video frame data.

The FrameBitmapReady event is fired when a System.Drawing.Bitmap object is available for display. The purpose of this event is to allow a WinForms application to display a preview of the image of the captured video frame. The declaration of the delegate type for this event is:
```
public delegate void FrameBitmapReadyDelegate(Bitmap bitmap);
```
The FrameReady event is fired when a raw image frame is available. The declaration of the delegate type for this event is:
```
public delegate void FrameReadyDelegate(int Width, int Height, int fps, byte[] bytes, 
    AVPixelFormat pixelFormat);
```
The application can used the data provided by this event to encode and send video frames over the IP network using an instance of the [VideoSender](~/api/SipLib.Video.Windows.VideoSender.yml) class.

The article entitled [Sending Video](~/articles/SendingAndReceivingVideo.md#SendingVideo) describes how to use the VideoSender class.
