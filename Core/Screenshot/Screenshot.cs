using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopStreaming.Core.Screenshot
{
    public static class Screenshot
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Cursorinfo
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public Point ptScreenPos;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out Cursorinfo pci);

        [DllImport("user32.dll")]
        static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon,
            int cxWidth, int cyWidth, uint istepIfAniCur, IntPtr hbrFlickerFreeDraw,
            uint diFlags);

        const int CursorShowing = 0x00000001;

        public static IEnumerable<Image> TakeSeriesOfScreenshots(Resolution.Resolutions requiredResolution,
            bool isDisplayCursor)
        {
            var screenSize = new Size(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
            var requiredSize = Resolution.GetResolutionSize(requiredResolution);

            var rawImage = new Bitmap(screenSize.Width, screenSize.Height);
            var rawGraphics = Graphics.FromImage(rawImage);

            bool isNeedToScale = screenSize != requiredSize;

            var image = rawImage;
            var graphics = rawGraphics;

            if (isNeedToScale)
            {
                image = new Bitmap(requiredSize.Width, requiredSize.Height);
                graphics = Graphics.FromImage(image);
            }

            var source = new Rectangle(0, 0, screenSize.Width, screenSize.Height);
            var destination = new Rectangle(0, 0, requiredSize.Width, requiredSize.Height);

            while (true)
            {
                rawGraphics.CopyFromScreen(0, 0, 0, 0, screenSize);

                if (isDisplayCursor)
                {
                    AddCursorToScreenshot(rawGraphics, screenSize, requiredSize);
                }

                if (isNeedToScale)
                {
                    graphics.DrawImage(rawImage, destination, source, GraphicsUnit.Pixel);
                }

                yield return image;
            }
        }

        private static void AddCursorToScreenshot(Graphics graphics, Size screenSize, Size captureSize)
        {
            Cursorinfo cursorInfo = new();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);

            if (GetCursorInfo(out cursorInfo) && cursorInfo.flags == CursorShowing)
            {
                float scaleX = (float)captureSize.Width / screenSize.Width;
                float scaleY = (float)captureSize.Height / screenSize.Height;

                int cursorX = (int)(cursorInfo.ptScreenPos.X * scaleX);
                int cursorY = (int)(cursorInfo.ptScreenPos.Y * scaleY);

                IntPtr hdc = graphics.GetHdc();
                DrawIconEx(hdc, cursorX, cursorY, cursorInfo.hCursor, 0, 0, 0, IntPtr.Zero, 0x0003);
                graphics.ReleaseHdc(hdc);
            }
        }

    }
}