namespace DesktopStreaming.Core.Screenshot
{
    /// <summary>
    /// Represents the frames per second for streaming.
    /// </summary>
    public class Fps
    {
        /// <summary>
        /// Gets the delay in milliseconds between frames.
        /// </summary>
        public int Delay { get; }

        /// <summary>
        /// Gets the frames per second.
        /// </summary>
        public int FramesPerSecond { get; }

        /// <summary>
        /// Initializes a new instance of the Fps class.
        /// </summary>
        /// <param name="framesPerSecond">The desired frames per second.</param>
        private Fps(int framesPerSecond)
        {
            FramesPerSecond = framesPerSecond;
            Delay = (int)(1000 / framesPerSecond);
        }

        public static Fps CreateInstance(int framesPerSecond)
        {
            return new Fps(framesPerSecond);
        }
    }
}