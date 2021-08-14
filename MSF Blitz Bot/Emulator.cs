using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace MSFBlitzBot
{
    public static class Emulator
    {
        public enum EmulatorId
        {
            None = -1,
            BlueStacks,
            Nox,
            MEmu,
            dnplayer,
            NemuPlayer,
            HDPlayer,
            Count
        }

        public static bool IsValid => Id != EmulatorId.None;

        public static EmulatorId Id { get; private set; } = EmulatorId.None;


        private static int Dpi { get; set; } = 96;


        private static IntPtr GameHandle { get; set; } = IntPtr.Zero;


        public static IntPtr AppHandle { get; private set; } = IntPtr.Zero;


        public static FImage GameImage { get; } = new FImage();


        public static FImage WindowImage => WinCapture.CaptureWindow(AppHandle);

        public static void Close()
        {
            Id = EmulatorId.None;
        }

        public static void Initialize()
        {
            IntPtr intPtr = IntPtr.Zero;
            EmulatorId id = EmulatorId.None;
            Rectangle rectangle = Rectangle.Empty;
            for (Id = EmulatorId.BlueStacks; Id < EmulatorId.Count; Id++)
            {
                Process[] processesByName = Process.GetProcessesByName((Id == EmulatorId.HDPlayer) ? "HD-Player" : Id.ToString());
                for (int i = 0; i < processesByName.Length; i++)
                {
                    AppHandle = processesByName[i].MainWindowHandle;
                    Dpi = WinCapture.GetWinDpi(AppHandle);
                    if (Dpi != 0)
                    {
                        Rectangle windowArea = WinCapture.GetWindowArea(AppHandle, Dpi);
                        if (windowArea.Width > rectangle.Width)
                        {
                            intPtr = AppHandle;
                            rectangle = windowArea;
                            id = Id;
                        }
                    }
                }
            }
            if (intPtr != IntPtr.Zero && WinCapture.IsIconic(intPtr))
            {
                WinCapture.ShowWindow(intPtr, WinCapture.ShowWindowCommands.Restore);
                Dpi = WinCapture.GetWinDpi(intPtr);
                rectangle = WinCapture.GetWindowArea(intPtr, Dpi);
            }
            if (rectangle == Rectangle.Empty)
            {
                AppHandle = IntPtr.Zero;
                Id = EmulatorId.None;
                return;
            }
            Point point = new Point(rectangle.X + (rectangle.Width >> 1), rectangle.Y + (rectangle.Height >> 1));
            AppHandle = intPtr;
            Id = id;
            Dpi = WinCapture.GetWinDpi(AppHandle);
            IEnumerable<IntPtr> windowChildren = WinCapture.GetWindowChildren(AppHandle);
            int num = 0;
            IntPtr intPtr2 = IntPtr.Zero;
            int num2 = rectangle.Width * rectangle.Height;
            foreach (IntPtr item in windowChildren)
            {
                num++;
                int winDpi = WinCapture.GetWinDpi(item);
                Rectangle windowArea2 = WinCapture.GetWindowArea(item, winDpi);
                if (windowArea2.X <= point.X && windowArea2.Y <= point.Y && windowArea2.Right >= point.X && windowArea2.Bottom >= point.Y && windowArea2.Width >= rectangle.Width >> 1 && windowArea2.Height >= rectangle.Height >> 1)
                {
                    int num3 = windowArea2.Width * windowArea2.Height;
                    if (num3 <= num2)
                    {
                        intPtr2 = item;
                        num2 = num3;
                    }
                }
            }
            GameHandle = (intPtr2 == IntPtr.Zero) ? AppHandle : intPtr2;
        }

        public static void BringToFront()
        {
            WinCapture.SetForegroundWindow(AppHandle);
        }

        public static FImage UpdateGameImage()
        {
            return WinCapture.CaptureWindow(AppHandle, GameHandle, GameImage) ? GameImage : null;
        }

        public static Rectangle GetGameScreenArea(bool useDpi = true)
        {
            return WinCapture.GetWindowArea(GameHandle, useDpi ? Dpi : 96);
        }
    }
}
