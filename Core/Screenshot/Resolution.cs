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
                Resolutions.QuadHd => "Quad HD (2560x1440)",
                Resolutions.FullHd => "Full HD (1920x1080)",
                Resolutions.Hd => "HD (1280x720)",
                _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null)
            };
        }
    }
}