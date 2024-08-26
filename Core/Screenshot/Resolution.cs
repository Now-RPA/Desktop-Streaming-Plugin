using System;
using System.Drawing;

namespace DesktopStreaming.Core.Screenshot
{
    /// <summary>
    /// Provides methods for working with image resolutions.
    /// </summary>
    public static class Resolution
    {
        #region Screen resolutions

        private const int QuadHDWidth = 2560;
        private const int QuadHDHeight = 1440;

        private const int FullHDWidth = 1920;
        private const int FullHDHeight = 1080;

        private const int HDWidth = 1280;
        private const int HDHeight = 720;

        #endregion

        /// <summary>
        /// Possible screen resolutions.
        /// </summary>
        public enum Resolutions
        {
            QuadHD,
            FullHD,
            HD
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
                Resolutions.QuadHD => new Size(QuadHDWidth, QuadHDHeight),
                Resolutions.FullHD => new Size(FullHDWidth, FullHDHeight),
                Resolutions.HD => new Size(HDWidth, HDHeight),
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
                Resolutions.QuadHD => "Quad HD (2560x1440)",
                Resolutions.FullHD => "Full HD (1920x1080)",
                Resolutions.HD => "HD (1280x720)",
                _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null)
            };
        }
    }
}