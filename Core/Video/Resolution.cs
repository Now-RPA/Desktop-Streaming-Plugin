using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DesktopStreaming.Core.Video;

/// <summary>
/// Provides methods for working with image resolutions.
/// </summary>
public static class Resolution
{
    #region Screen resolutions

    private const int QuadHdWidth = 2560;
    private const int QuadHdHeight = 1440;

    private const int FullHdWidth = 1920;
    private const int FullHdHeight = 1080;

    private const int HdWidth = 1280;
    private const int HdHeight = 720;

    #endregion

    /// <summary>
    /// Possible screen resolutions.
    /// </summary>
    public enum Resolutions
    {
        Current,
        QuadHd,
        FullHd,
        Hd
    }

    /// <summary>
    /// Provides width and height for the required screen resolution.
    /// </summary>
    /// <param name="resolution">The resolution of the screen whose size you need to know.</param>
    /// <returns>Width and height of resolution.</returns>
    public static Size GetResolutionSize(Resolutions resolution)
    {
        return resolution switch
        {
            Resolutions.Current => GetDisplayResolution(),
            Resolutions.QuadHd => new Size(QuadHdWidth, QuadHdHeight),
            Resolutions.FullHd => new Size(FullHdWidth, FullHdHeight),
            Resolutions.Hd => new Size(HdWidth, HdHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null)
        };
    }

    /// <summary>
    /// Gets the descriptive name of the resolution.
    /// </summary>
    /// <param name="resolution">The resolution enum value.</param>
    /// <returns>A string describing the resolution.</returns>
    public static string GetResolutionDescription(Resolutions resolution)
    {
        return resolution switch
        {
            Resolutions.Current => $"Current Resolution ({GetDisplayResolution().Width}x{GetDisplayResolution().Height})",
            Resolutions.QuadHd => "Quad HD (2560x1440)",
            Resolutions.FullHd => "Full HD (1920x1080)",
            Resolutions.Hd => "HD (1280x720)",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null)
        };
    }

    #region Display Resolution

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
    public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

    public enum DeviceCap
    {
        DESKTOPVERTRES = 117,
        DESKTOPHORZRES = 118
    }

    public static Size GetDisplayResolution()
    {
        // Create Graphics object from the current windows handle
        Graphics graphicsObject = Graphics.FromHwnd(IntPtr.Zero);
        // Get Handle to the device context associated with this Graphics object
        IntPtr deviceContextHandle = graphicsObject.GetHdc();

        // Get the physical screen width and height directly from GetDeviceCaps
        int physicalScreenWidth = GetDeviceCaps(deviceContextHandle, (int)DeviceCap.DESKTOPHORZRES);
        int physicalScreenHeight = GetDeviceCaps(deviceContextHandle, (int)DeviceCap.DESKTOPVERTRES);

        // Release the Handle and Dispose of the GraphicsObject object
        graphicsObject.ReleaseHdc(deviceContextHandle);
        graphicsObject.Dispose();

        // Return the screen size
        return new Size(physicalScreenWidth, physicalScreenHeight);
    }

    #endregion
}