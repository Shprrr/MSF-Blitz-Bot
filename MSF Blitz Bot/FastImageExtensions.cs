using System.Collections.Generic;
using System.Drawing;

namespace MSFBlitzBot
{
    public static class FastImageExtensions
    {
        public static string ReadText(this FImage img, RectangleF area, float fontSize, CharactersImageSet charactersImages, uint color, byte diffDetect = 5, byte diff = 30, float detectionY = -1f, char[] chars = null, int dist1 = 4, int dist2 = 6)
        {
            using FImage extract = img.GetScaledExtract(area, fontSize);
            //extract.Save("C:\\temp\\font.png");
            return extract.DetectText(charactersImages, color, diffDetect, diff, detectionY, chars, dist1, dist2);
        }

        public static string ReadText(this FImage img, CharactersImageSet charactersImages, float scale = 1f, float italic = -0.25f, bool allowMiss = false, int toleranceDist = 5, int toleranceDist2 = 5)
        {
            using FImage fImage = new(img, scale);
            using FImage fImage2 = fImage.Italic(italic);
            string text = string.Empty;
            int num = 0;
            Rectangle rectangle = Rectangle.Empty;
            do
            {
                Rectangle rectangle2 = fImage2.DetectBlack(num, 80);
                if (rectangle2 == Rectangle.Empty)
                {
                    return text;
                }
                if (rectangle != Rectangle.Empty && rectangle2.X - rectangle.Right > 20)
                {
                    text += " ";
                }
                char c = charactersImages.MatchCentered(fImage2, rectangle2, toleranceDist, toleranceDist2);
                if (c == '\0')
                {
                    if (!allowMiss)
                    {
                        return string.Empty;
                    }
                    c = '?';
                }
                switch (c)
                {
                    case 'D':
                    case 'O':
                        c = fImage2.IsSquareCorner(rectangle2) ? 'D' : 'O';
                        break;
                    case '8':
                    case 'B':
                        c = fImage2.IsSquareCorner(rectangle2) ? 'B' : '8';
                        break;
                }
                text += c;
                num = rectangle2.Right;
                rectangle = rectangle2;
            }
            while (num < fImage2.Width);
            return text;
        }

        private static bool IsSquareCorner(this FImage img, Rectangle rect)
        {
            int i;
            for (i = 0; i < rect.Width; i++)
            {
                int j;
                for (j = 0; j < rect.Height && img.GetPixelAlpha(rect.X + i, rect.Y + j) <= 100; j++)
                {
                }
                if (j < rect.Height)
                {
                    break;
                }
            }
            rect.Width -= i;
            rect.X += i;
            int k;
            for (k = 0; k < rect.Height; k++)
            {
                int l;
                for (l = 0; l < rect.Width && img.GetPixelAlpha(rect.X + l, rect.Y + k) <= 100; l++)
                {
                }
                if (l < rect.Width)
                {
                    break;
                }
            }
            rect.Height -= k;
            rect.Y += k;
            while (rect.Width > 0)
            {
                int m;
                for (m = 0; m < rect.Height && img.GetPixelAlpha(rect.Right - 1, rect.Y + m) <= 100; m++)
                {
                }
                if (m < rect.Height)
                {
                    break;
                }
                rect.Width--;
            }
            while (rect.Height > 0)
            {
                int n;
                for (n = 0; n < rect.Width && img.GetPixelAlpha(rect.X + n, rect.Bottom - 1) <= 100; n++)
                {
                }
                if (n < rect.Width)
                {
                    break;
                }
                rect.Height--;
            }
            int num;
            for (num = 0; num < 5; num++)
            {
                int num2 = 0;
                for (int num3 = 0; num3 < rect.Height; num3++)
                {
                    if (img.GetPixelAlpha(rect.X + num, rect.Y + num3) >= 200)
                    {
                        num2++;
                    }
                }
                if ((float)num2 / rect.Height > 0.75f)
                {
                    break;
                }
            }
            int num4;
            for (num4 = rect.Height - 1; num4 > rect.Height - 6; num4--)
            {
                int num5 = 0;
                for (int num6 = 0; num6 < rect.Width; num6++)
                {
                    if (img.GetPixelAlpha(rect.X + num6, rect.Y + num4) >= 200)
                    {
                        num5++;
                    }
                }
                if ((float)num5 / rect.Width > 0.75f)
                {
                    break;
                }
            }
            return img.GetPixelAlpha(rect.X + num, rect.Y + num4) > 128;
        }

        private static Rectangle DetectBlack(this FImage img, int x, byte maxColor = 80)
        {
            Rectangle result = new(-1, 0, img.Width, img.Height);
            while (x < img.Width && result.X < 0)
            {
                for (int i = 0; i < img.Height; i++)
                {
                    var pixel = img.GetPixel(x, i);
                    if (pixel.R > maxColor || pixel.G > maxColor || pixel.B > maxColor)
                    {
                        result.X = x;
                        break;
                    }
                }
                x++;
            }
            if (x == img.Width)
            {
                return Rectangle.Empty;
            }
            while (x < img.Width)
            {
                int j;
                for (j = 0; j < img.Height; j++)
                {
                    var pixel2 = img.GetPixel(x, j);
                    if (pixel2.R > maxColor || pixel2.G > maxColor || pixel2.B > maxColor)
                    {
                        j = img.Height + 1;
                    }
                }
                if (j == img.Height)
                {
                    break;
                }
                x++;
            }
            result.Width = x - result.X;
            int k;
            for (k = 0; k < result.Height; k++)
            {
                int l;
                for (l = 0; l < result.Width; l++)
                {
                    var pixel3 = img.GetPixel(result.X + l, k);
                    if (pixel3.R > maxColor || pixel3.G > maxColor || pixel3.B > maxColor)
                    {
                        break;
                    }
                }
                if (l < result.Width)
                {
                    break;
                }
            }
            result.Height -= k;
            result.Y = k;
            while (result.Height > 0)
            {
                int m;
                for (m = 0; m < result.Width; m++)
                {
                    var pixel4 = img.GetPixel(result.X + m, result.Bottom - 1);
                    if (pixel4.R > maxColor || pixel4.G > maxColor || pixel4.B > maxColor)
                    {
                        break;
                    }
                }
                if (m < result.Width)
                {
                    break;
                }
                result.Height--;
            }
            return result;
        }

        private static string DetectText(this FImage extract, CharactersImageSet characters, uint color, byte diffDetect = 5, byte diff = 30, float detectionY = -1f, char[] chars = null, int dist1 = 4, int dist2 = 6)
        {
            int x = 0;
            string text = "";
            uint[] array = new uint[3] { 4294902015u, 4294967040u, 4278255615u };
            int num = 0;
            while (extract.DetectColor(ref x, out var y, color, detectionY, diffDetect))
            {
                extract.SpreadColor(x, y, color, array[num], out var area, diff);
                char c = characters.Match(extract, area, array[num], chars, dist1, dist2);
                if (c != 0)
                {
                    text += c;
                }
                num = (num + 1) % array.Length;
            }
            return text;
        }

        private static bool DetectColor(this FImage img, ref int x, out int y, uint color, float detectionY = -1f, byte diff = 0, bool rightToLeft = false)
        {
            int num = 0;
            int num2 = img.Height;
            if (detectionY >= 0f)
            {
                num = (int)(img.Height * detectionY);
                num2 = num + 1;
            }
            var c = Color.FromArgb((int)color);
            int num3 = (!rightToLeft) ? (img.Width - 1) : 0;
            int num4 = (!rightToLeft) ? 1 : (-1);
            while (x != num3)
            {
                for (y = num; y < num2; y++)
                {
                    uint pixelU = img.GetPixelU32(x, y);
                    if (diff == 0)
                    {
                        if (pixelU == color)
                        {
                            return true;
                        }
                    }
                    else if (Color.FromArgb((int)pixelU).IsCloseTo(c, diff))
                    {
                        return true;
                    }
                }
                x += num4;
            }
            y = 0;
            return false;
        }

        private static void SpreadColor(this FImage img, int x, int y, uint searchColor, uint fillColor, out Rectangle area, byte diff = 30)
        {
            Stack<Point> stack = new();
            stack.Push(new Point(x, y));
            var c = Color.FromArgb((int)searchColor);
            img.SetPixel(x, y, fillColor);
            int num = x;
            int num2 = x;
            int num3 = y;
            int num4 = y;
            while (stack.Count > 0)
            {
                Point point = stack.Pop();
                if (point.X > 0 && c.IsCloseTo(img.GetPixel(point.X - 1, point.Y), diff))
                {
                    if (point.X == num)
                    {
                        num--;
                    }
                    img.SetPixel(point.X - 1, point.Y, fillColor);
                    stack.Push(new Point(point.X - 1, point.Y));
                }
                if (point.Y > 0 && c.IsCloseTo(img.GetPixel(point.X, point.Y - 1), diff))
                {
                    if (point.Y == num3)
                    {
                        num3--;
                    }
                    img.SetPixel(point.X, point.Y - 1, fillColor);
                    stack.Push(new Point(point.X, point.Y - 1));
                }
                if (point.X < img.Width - 1 && c.IsCloseTo(img.GetPixel(point.X + 1, point.Y), diff))
                {
                    if (point.X == num2)
                    {
                        num2++;
                    }
                    img.SetPixel(point.X + 1, point.Y, fillColor);
                    stack.Push(new Point(point.X + 1, point.Y));
                }
                if (point.Y < img.Height - 1 && c.IsCloseTo(img.GetPixel(point.X, point.Y + 1), diff))
                {
                    if (point.Y == num4)
                    {
                        num4++;
                    }
                    img.SetPixel(point.X, point.Y + 1, fillColor);
                    stack.Push(new Point(point.X, point.Y + 1));
                }
            }
            area = new Rectangle(num, num3, num2 - num + 1, num4 - num3 + 1);
        }

        public static bool HasAlphaCloseTo(this FImage img, int x, int y, int checkDist, byte minAlpha = 150)
        {
            for (int i = y - checkDist; i <= y + checkDist; i++)
            {
                if (i < 0)
                {
                    continue;
                }
                if (i >= img.Height)
                {
                    return false;
                }
                for (int j = x - checkDist; j <= x + checkDist; j++)
                {
                    if (j >= 0)
                    {
                        if (j >= img.Width)
                        {
                            break;
                        }
                        if (img.GetPixelAlpha(j, i) >= minAlpha)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool HasAlphaCloseTo2(this FImage img, int x, int y, int checkDist, byte minAlpha = 150)
        {
            for (int i = y - checkDist; i <= y + checkDist; i++)
            {
                if (i < 0)
                {
                    continue;
                }
                if (i >= img.Height)
                {
                    return false;
                }
                for (int j = x - checkDist; j <= x + checkDist; j++)
                {
                    if (j >= 0)
                    {
                        if (j >= img.Width)
                        {
                            break;
                        }
                        var pixel = img.GetPixel(j, i);
                        if (pixel.A >= minAlpha && pixel.R >= minAlpha && pixel.G >= minAlpha && pixel.B >= minAlpha)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
