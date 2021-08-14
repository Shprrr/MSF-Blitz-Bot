using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MSFBlitzBot
{
    internal class WinCapture
    {
        private struct Rect
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }

        public enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
#pragma warning disable CA1069 // Les valeurs enum ne doivent pas être dupliquées
            ShowMaximized = 3,
#pragma warning restore CA1069 // Les valeurs enum ne doivent pas être dupliquées
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hWnd);

        public static int GetWinDpi(IntPtr hWnd)
        {
            try
            {
                return GetDpiForWindow(hWnd);
            }
            catch (Exception)
            {
                return 96;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        public static Rectangle GetWindowArea(IntPtr hWnd, int dpi = 96)
        {
            Rect rect = default;
            if (!GetWindowRect(hWnd, ref rect)) return Rectangle.Empty;

            return new Rectangle(rect.Left * 96 / dpi, rect.Top * 96 / dpi, (rect.Right - rect.Left) * 96 / dpi, (rect.Bottom - rect.Top) * 96 / dpi);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        public static IEnumerable<IntPtr> GetWindowChildren(IntPtr parent)
        {
            List<IntPtr> list = new List<IntPtr>();
            GCHandle value = GCHandle.Alloc(list);
            IntPtr lParam = GCHandle.ToIntPtr(value);
            try
            {
                EnumWindowProc callback = EnumWindow;
                EnumChildWindows(parent, callback, lParam);
                return list;
            }
            finally
            {
                value.Free();
            }
        }

        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gCHandle = GCHandle.FromIntPtr(lParam);
            if (gCHandle.Target == null)
            {
                return false;
            }
            (gCHandle.Target as List<IntPtr>).Add(hWnd);
            return true;
        }

        public static bool CaptureWindow(IntPtr captureWnd, IntPtr areaWnd, FImage img)
        {
            if (areaWnd == IntPtr.Zero || captureWnd == IntPtr.Zero)
            {
                return false;
            }
            Rectangle windowArea = GetWindowArea(areaWnd);
            Rectangle windowArea2 = GetWindowArea(captureWnd);
            if (windowArea.Width <= 0 || windowArea.Height <= 0)
            {
                return false;
            }
            try
            {
                using Bitmap bitmap = new Bitmap(windowArea.Width, windowArea.Height);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = graphics.GetHdc();
                    IntPtr dC = GetDC(captureWnd);
                    BitBlt(hdc, 0, 0, bitmap.Width, bitmap.Height, dC, windowArea.X - windowArea2.X, windowArea.Y - windowArea2.Y, 13369376);
                    graphics.ReleaseHdc(hdc);
                    ReleaseDC(captureWnd, dC);
                }
                img.Initialize(bitmap);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool CaptureWindow(IntPtr hWnd, FImage img)
        {
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }
            Rectangle windowArea = GetWindowArea(hWnd);
            if (windowArea.Width <= 0 || windowArea.Height <= 0)
            {
                return false;
            }
            try
            {
                using Bitmap bitmap = new Bitmap(windowArea.Width, windowArea.Height);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = graphics.GetHdc();
                    IntPtr dC = GetDC(hWnd);
                    BitBlt(hdc, 0, 0, bitmap.Width, bitmap.Height, dC, 0, 0, 13369376);
                    graphics.ReleaseHdc(hdc);
                    ReleaseDC(hWnd, dC);
                }
                img.Initialize(bitmap);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static FImage CaptureWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return null;
            }
            Rectangle windowArea = GetWindowArea(hWnd);
            if (windowArea.Width <= 0 || windowArea.Height <= 0)
            {
                return null;
            }
            using Bitmap bitmap = new Bitmap(windowArea.Width, windowArea.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                IntPtr hdc = graphics.GetHdc();
                IntPtr dC = GetDC(hWnd);
                BitBlt(hdc, 0, 0, bitmap.Width, bitmap.Height, dC, 0, 0, 13369376);
                graphics.ReleaseHdc(hdc);
                ReleaseDC(hWnd, dC);
            }
            return new FImage(bitmap);
        }
    }
}
