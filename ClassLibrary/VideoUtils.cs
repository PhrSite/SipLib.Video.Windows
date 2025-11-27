/////////////////////////////////////////////////////////////////////////////////////
//  File:   VideoUtils.cs                                           19 Nov 25 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace SipLib.Video.Windows;
using SIPSorceryMedia.FFmpeg;


/// <summary>
/// This class provides utility functions for working with video.
/// </summary>
public class VideoUtils
{
    /// <summary>
    /// Initializes the FFMPEG libraries. This method must be called by the consumer of the FFMPEG libraries before
    /// attempting to use any of the FFMPEG C# wrapper classes or any of the classes in SipLib.Video.Windows namespace.
    /// </summary>
    /// <returns>Returns true if successful or false if not successful. If this method returns false then do not 
    /// attempt to use any FFMPEG related classes.</returns>
    public static bool InitializeFFMPEG()
    {
        bool Success = true;
        try
        {
            FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, @".\");
        }
        catch
        {
            Success = false;
        }

        return Success;
    }
}
