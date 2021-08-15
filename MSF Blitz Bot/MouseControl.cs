using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace MSFBlitzBot
{
    internal static class MouseControl
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x2,
            LeftUp = 0x4,
            MiddleDown = 0x20,
            MiddleUp = 0x40,
            Move = 0x1,
            Absolute = 0x8000,
            RightDown = 0x8,
            RightUp = 0x10
        }

        private enum VirtualKeysCodes
        {
            VK_LBUTTON = 1,
            VK_RBUTTON = 2,
            VK_MBUTTON = 4
        }

        public enum MouseState
        {
            Press,
            Pressed,
            Release,
            ReleaseScroll,
            Released
        }

        public struct POINT
        {
            public int X;

            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        private static MouseState _lmstate = MouseState.Released;

        private static bool _lmmoved;

        private static PointF _lmpos = PointF.Empty;

        private static bool _wasPressing;

        private static bool _wasPress;

        private static Point _lastClick = Point.Empty;

        public static MouseState CurrentMouseState
        {
            get => _lmstate;
            set
            {
                _lmstate = value;
                RaiseCurrentMouseChanged();
            }
        }

        public static PointF CurrentMousePos
        {
            get => _lmpos;
            set
            {
                _lmpos = value;
                RaiseCurrentMouseChanged();
            }
        }
        public delegate void OnCurrentMouseHandler();
        public static event OnCurrentMouseHandler OnCurrentMouseChanged;
        private static void RaiseCurrentMouseChanged() => OnCurrentMouseChanged?.Invoke();

        [DllImport("user32")]
        private static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int virtualKeyCode);

        public static void UpdateMouseState()
        {
            switch (CurrentMouseState)
            {
                case MouseState.Press:
                    CurrentMouseState = MouseState.Pressed;
                    break;
                case MouseState.Release:
                case MouseState.ReleaseScroll:
                    CurrentMouseState = MouseState.Released;
                    break;
            }
            byte[] bytes = BitConverter.GetBytes(GetAsyncKeyState(1));
            bool flagIsPress = (bytes[1] & 0x80) == 0x80;
            bool flagWasPressed = bytes[0] == 1;
            if (flagIsPress != _wasPressing || flagWasPressed != _wasPress)
            {
                _wasPressing = flagIsPress;
                _wasPress = flagWasPressed;
            }
            if (CurrentMouseState == MouseState.Released && !flagIsPress && !flagWasPressed)
            {
                return;
            }
            PointF mouseGamePos = GetMouseGamePos();
            if ((mouseGamePos.X < 0f || mouseGamePos.X > 1f || mouseGamePos.Y < 0f || mouseGamePos.Y > 1f) && (CurrentMouseState == MouseState.Released || !flagIsPress || flagWasPressed))
            {
                CurrentMouseState = MouseState.Released;
            }
            else if (flagWasPressed)
            {
                CurrentMousePos = new PointF(mouseGamePos.X, mouseGamePos.Y);
                CurrentMouseState = flagIsPress ? MouseState.Press : MouseState.Release;
                _lmmoved = false;
            }
            else if (CurrentMouseState == MouseState.Pressed)
            {
                if (!_lmmoved && Math.Abs(CurrentMousePos.X - mouseGamePos.X) > 0.01 || Math.Abs(CurrentMousePos.Y - mouseGamePos.Y) > 0.02)
                {
                    _lmmoved = true;
                }
                if (!flagIsPress)
                {
                    CurrentMousePos = mouseGamePos;
                    CurrentMouseState = _lmmoved ? MouseState.ReleaseScroll : MouseState.Release;
                }
            }
        }

        public static PointF GetMouseGamePos()
        {
            Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
            GetCursorPos(out var lpPoint);
            return new PointF((float)(lpPoint.X - gameScreenArea.X) / gameScreenArea.Width, (float)(lpPoint.Y - gameScreenArea.Y) / gameScreenArea.Height);
        }

        public static void Scroll(PointF point1, PointF point2)
        {
            Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
            Point point3 = new Point((int)((float)gameScreenArea.X + point1.X * (float)gameScreenArea.Width), (int)((float)gameScreenArea.Y + point1.Y * (float)gameScreenArea.Height));
            Point point4 = new Point((int)((float)gameScreenArea.X + point2.X * (float)gameScreenArea.Width), (int)((float)gameScreenArea.Y + point2.Y * (float)gameScreenArea.Height));
            Point point5 = new Point(point4.X - point3.X, point4.Y - point3.Y);
            SetCursorPos(point3.X, point3.Y);
            mouse_event(2, point3.X, point3.Y, 0, 0);
            DateTime now = DateTime.Now;
            int num = RandomManager.Get(300, 350);
            for (int num2 = 0; num2 < num; num2 = (int)(DateTime.Now - now).TotalMilliseconds)
            {
                Point point6 = new Point(point3.X + point5.X * num2 / num, point3.Y + point5.Y * num2 / num);
                SetCursorPos(point6.X, point6.Y);
            }
            SetCursorPos(point4.X, point4.Y);
            mouse_event(4, point4.X, point4.Y, 0, 0);
        }

        //public static void VerticalScroll(PointF source, float distance)
        //{
        //    Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
        //    Point pos = new Point((int)(gameScreenArea.X + source.X * gameScreenArea.Width), (int)(gameScreenArea.Y + source.Y * gameScreenArea.Height));
        //    Point point = new Point((int)((float)gameScreenArea.X + (source.X + (float)UserDataManager.UserData.ScrollLateralDistance / 1920f) * (float)gameScreenArea.Width), (int)(gameScreenArea.Y + source.Y * gameScreenArea.Height));
        //    Point pos2 = new Point((int)(gameScreenArea.X + source.X * gameScreenArea.Width), (int)(gameScreenArea.Y + (source.Y + distance) * gameScreenArea.Height));
        //    SetCursorPos(pos.X, pos.Y);
        //    Thread.Sleep(UserDataManager.UserData.ScrollInitDelay);
        //    mouse_event(2, pos.X, pos.Y, 0, 0);
        //    Slide(pos, point, (float)UserDataManager.UserData.ScrollLateralSpeed / 100f);
        //    Thread.Sleep(UserDataManager.UserData.ScrollMidDelay);
        //    Slide(point, pos2, (float)UserDataManager.UserData.ScrollSpeed / 100f);
        //    Thread.Sleep(UserDataManager.UserData.ScrollStabilizationDelay);
        //    mouse_event(4, point.X, point.Y, 0, 0);
        //}

        private static void Slide(Point pos1, Point pos2, float speed = 1f)
        {
            DateTime now = DateTime.Now;
            int num = pos2.X - pos1.X;
            int num2 = pos2.Y - pos1.Y;
            float num3 = (int)Math.Sqrt(num * num + num2 * num2) / speed;
            int num4 = 0;
            Point point = new Point(pos2.X - pos1.X, pos2.Y - pos1.Y);
            while (num4 < num3)
            {
                Point point2 = new Point((int)(pos1.X + point.X * num4 / num3), (int)(pos1.Y + point.Y * num4 / num3));
                SetCursorPos(point2.X, point2.Y);
                num4 = (int)(DateTime.Now - now).TotalMilliseconds;
            }
            SetCursorPos(pos2.X, pos2.Y);
        }

        public static void Click(float x, float y)
        {
            Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
            bool cursorPos = GetCursorPos(out POINT lpPoint);
            _lastClick = new Point((int)(gameScreenArea.X + x * gameScreenArea.Width), (int)(gameScreenArea.Y + y * gameScreenArea.Height));
            SetCursorPos(_lastClick.X, _lastClick.Y);
            mouse_event(2, _lastClick.X, _lastClick.Y, 0, 0);
            Thread.Sleep(100);
            mouse_event(4, _lastClick.X, _lastClick.Y, 0, 0);
            if (cursorPos)
            {
                SetCursorPos(lpPoint.X, lpPoint.Y);
            }
            Thread.Sleep(100);
        }

        public static void ClickArea(RectangleF zone, bool restorePos = true)
        {
            Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
            bool cursorPos = GetCursorPos(out POINT lpPoint);
            if (!restorePos && cursorPos)
            {
                _lastClick = lpPoint;
            }
            Rectangle rectangle = new Rectangle((int)(gameScreenArea.X + zone.X * gameScreenArea.Width), (int)(gameScreenArea.Y + zone.Y * gameScreenArea.Height), (int)(zone.Width * gameScreenArea.Width), (int)(zone.Height * gameScreenArea.Height));
            bool flag = _lastClick.X >= rectangle.X && _lastClick.Y >= rectangle.Y && _lastClick.X <= rectangle.Right && _lastClick.Y <= rectangle.Bottom;
            Point point = default;
            if (flag)
            {
                point = _lastClick;
            }
            else
            {
                PointF pointF = new PointF(zone.X + zone.Width / 2f, zone.Y + zone.Height / 2f);
                PointF pointF2 = new PointF(pointF.X - zone.X, pointF.Y - zone.Y);
                PointF pointF3 = new PointF(RandomManager.Get(-900, 900) / 1000f, RandomManager.Get(-900, 900) / 1000f);
                pointF3.X *= pointF3.X;
                pointF3.Y *= pointF3.Y;
                PointF pointF4 = new PointF(pointF.X + pointF3.X * pointF2.X, pointF.Y + pointF3.Y * pointF2.Y);
                Point lastClick = new Point((int)(gameScreenArea.X + gameScreenArea.Width * pointF4.X), (int)(gameScreenArea.Y + gameScreenArea.Height * pointF4.Y));
                if (flag)
                {
                    _lastClick.X = (_lastClick.X * 8 + lastClick.X * 2) / 10;
                    _lastClick.Y = (_lastClick.Y * 8 + lastClick.Y * 2) / 10;
                }
                else
                {
                    _lastClick = lastClick;
                }
            }
            SetCursorPos(_lastClick.X, _lastClick.Y);
            mouse_event(2, point.X, point.Y, 0, 0);
            Thread.Sleep(RandomManager.Get(75, 100));
            mouse_event(4, point.X, point.Y, 0, 0);
            if (cursorPos && restorePos)
            {
                SetCursorPos(lpPoint.X, lpPoint.Y);
            }
            Thread.Sleep(100);
        }

        public static bool GetPos(out float x, out float y)
        {
            if (!GetCursorPos(out var lpPoint))
            {
                x = y = -1f;
                return false;
            }
            Rectangle gameScreenArea = Emulator.GetGameScreenArea(useDpi: false);
            x = (float)(lpPoint.X - gameScreenArea.X) / gameScreenArea.Width;
            y = (float)(lpPoint.Y - gameScreenArea.Y) / gameScreenArea.Height;
            if (x >= 0f && x <= 1f && y >= 0f)
            {
                return y <= 1f;
            }
            return false;
        }
    }
}
