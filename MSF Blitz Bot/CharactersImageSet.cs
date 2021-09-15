using System;
using System.Collections.Generic;
using System.Drawing;

namespace MSFBlitzBot
{
    public class CharactersImageSet
    {
        public static CharactersImageSet UltimusMed { get; } = new CharactersImageSet("Ultimus Med", 129, new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

        public const float NormalizedFactor = 1.294f;

        public string FontName { get; }

        public float FontSize { get; }

        public float Italic { get; }

        private FontStyle Style { get; }

        public Dictionary<char, FImage> Characters { get; } = new();

        public CharactersImageSet(string fontname, int size, char[] characters, FontStyle style = FontStyle.Regular, float italic = 0.25f, bool cropY = true)
        {
            FontName = fontname;
            FontSize = size * 1.294f;
            Italic = italic;
            Style = style;
            for (int i = 0; i < characters.Length; i++)
            {
                char key = characters[i];
                using FImage fImage = FontManager.GetTextImage(key.ToString(), FontName, FontSize, style, Italic);
                Characters[key] = fImage.Crop(1, cropX: true, cropY);
            }
        }

        public FImage GetText(string text, Color? color = null, bool cleanup = false)
        {
            return FontManager.GetTextImage(text, FontName, FontSize, Style, Italic, color, cleanup);
        }

        public char MatchCentered(FImage img, Rectangle area, int toleranceDist = 5, int toleranceDist2 = 5)
        {
            char result = '\0';
            int num = 0;
            int num2 = area.X + (area.Width >> 1);
            int num3 = area.Y + (area.Height >> 1);
            foreach (char key in Characters.Keys)
            {
                FImage fImage = Characters[key];
                int num4 = fImage.Width >> 1;
                int num5 = fImage.Height >> 1;
                int num6 = Math.Max(fImage.Width, area.Width);
                int num7 = Math.Max(fImage.Height, area.Height);
                int num8 = -(num6 >> 1);
                int num9 = -(num7 >> 1);
                int num10 = 0;
                int num11 = 0;
                bool flag = false;
                int num12 = num5 + num9;
                int num13 = num3 + num9;
                int num14 = 0;
                while (num14 < num7 && !flag)
                {
                    int i = num4 + num8;
                    int j = num2 + num8;
                    for (int k = 0; k < num6 && !flag; k++, i++, j++)
                    {
                        int num15 = 0;
                        if (i >= 0 && i < fImage.Width && num12 >= 0 && num12 < fImage.Height)
                        {
                            Color pixel = fImage.GetPixel(i, num12);
                            num15 = Math.Min(pixel.A, Math.Min(pixel.R, Math.Min(pixel.G, pixel.B)));
                        }
                        int num16 = 0;
                        if (j >= area.X && j < area.Right && num13 >= area.Y && num13 < area.Bottom)
                        {
                            num16 = img.GetPixel(j, num13).B;
                        }
                        bool flag2 = false;
                        if (num15 >= 250)
                        {
                            flag2 = true;
                        }
                        else if (num15 > 100)
                        {
                            continue;
                        }
                        bool flag3 = false;
                        if (num16 >= 250)
                        {
                            flag3 = true;
                        }
                        else if (num16 > 100)
                        {
                            continue;
                        }
                        if (!flag2 && !flag3)
                        {
                            continue;
                        }
                        if (flag2 && flag3)
                        {
                            num10++;
                            num11++;
                            continue;
                        }
                        num11++;
                        if (flag2)
                        {
                            if (!img.HasAlphaCloseTo2(j, num13, toleranceDist, 150))
                            {
                                flag = true;
                            }
                        }
                        else if (!fImage.HasAlphaCloseTo2(i, num12, toleranceDist2, 150))
                        {
                            flag = true;
                        }
                    }
                    num14++;
                    num12++;
                    num13++;
                }
                if (!flag)
                {
                    int num17 = (num11 != 0) ? (num10 * 255 / num11) : 0;
                    if (num17 > num)
                    {
                        num = num17;
                        result = key;
                    }
                }
            }
            return result;
        }

        public char Match(FImage img, Rectangle area, int toleranceDist = 5)
        {
            char result = '\0';
            int num = 0;
            int num2 = area.X + (area.Width >> 1);
            foreach (char key in Characters.Keys)
            {
                FImage fImage = Characters[key];
                int num3 = fImage.Width >> 1;
                int num4 = Math.Max(fImage.Width, area.Width);
                int num5 = -(num4 >> 1);
                int num6 = 0;
                int num7 = 0;
                bool flag = false;
                for (int i = 0; i < img.Height; i++)
                {
                    if (flag)
                    {
                        break;
                    }
                    int j = num3 + num5;
                    int k = num2 + num5;
                    for (int l = 0; l < num4 && !flag; l++, j++, k++)
                    {
                        int num8 = 0;
                        if (j >= 0 && j < fImage.Width && i < fImage.Height)
                        {
                            Color pixel = fImage.GetPixel(j, i);
                            num8 = Math.Min(pixel.A, Math.Min(pixel.R, Math.Min(pixel.G, pixel.B)));
                        }
                        int num9 = (k >= area.X && k < area.Right) ? img.GetPixelAlpha(k, i) : 0;
                        bool flag2 = false;
                        if (num8 >= 250)
                        {
                            flag2 = true;
                        }
                        else if (num8 != 0)
                        {
                            continue;
                        }
                        bool flag3 = false;
                        if (num9 >= 250)
                        {
                            flag3 = true;
                        }
                        else if (num9 != 0)
                        {
                            continue;
                        }
                        if (!flag2 && !flag3)
                        {
                            continue;
                        }
                        if (flag2 && flag3)
                        {
                            num6++;
                            num7++;
                            continue;
                        }
                        num7++;
                        if (flag2)
                        {
                            if (!img.HasAlphaCloseTo(k, i, toleranceDist, 150))
                            {
                                flag = true;
                            }
                        }
                        else if (!fImage.HasAlphaCloseTo(j, i, toleranceDist, 150))
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    int num10 = (num7 != 0) ? (num6 * 255 / num7) : 0;
                    if (num10 > num)
                    {
                        num = num10;
                        result = key;
                    }
                }
            }
            return result;
        }

        public char Match(FImage img, Rectangle area, uint color, char[] characters = null, int dist1 = 4, int dist2 = 6)
        {
            byte b = 0;
            char result = '?';
            if (characters == null)
            {
                characters = new char[Characters.Count];
                Characters.Keys.CopyTo(characters, 0);
            }
            char[] array = characters;
            foreach (char c in array)
            {
                FImage fImage = Characters[c];
                Rectangle rectangle = area;
                int width = area.Width;
                int height = area.Height;
                Rectangle rectangle2 = new(0, 0, fImage.Width, fImage.Height);
                if (rectangle.Width > rectangle2.Width)
                {
                    rectangle2.X -= rectangle.Width + 1 - rectangle2.Width >> 1;
                    width = rectangle.Width;
                }
                else if (rectangle.Width < rectangle2.Width)
                {
                    rectangle.X -= rectangle2.Width + 1 - rectangle.Width >> 1;
                    width = rectangle2.Width;
                }
                if (rectangle.Height > rectangle2.Height)
                {
                    rectangle2.Y -= rectangle.Height + 1 - rectangle2.Height >> 1;
                    height = rectangle.Height;
                }
                else if (rectangle.Height < rectangle2.Height)
                {
                    rectangle.Y -= rectangle2.Height + 1 - rectangle.Height >> 1;
                    height = rectangle2.Height;
                }
                int num = rectangle2.Y;
                int num2 = rectangle.Y;
                int num3 = 0;
                int num4 = 0;
                bool flag = false;
                int num5 = 0;
                while (num5 < height && !flag)
                {
                    int num6 = rectangle2.X;
                    int num7 = rectangle.X;
                    int num8 = 0;
                    while (num8 < width)
                    {
                        int num9 = (num6 >= 0 && num >= 0 && num6 < fImage.Width && num < fImage.Height) ? fImage.GetPixelAlpha(num6, num) : 0;
                        if (num9 is 0 or 255)
                        {
                            bool flag2 = num6 >= 0 && num6 < fImage.Width && num >= 0 && num < fImage.Height && num9 > 128;
                            bool flag3 = num7 >= 0 && num7 < img.Width && num2 >= 0 && num2 < img.Height && img.GetPixelU32(num7, num2) == color;
                            int j = 0;
                            if (flag2 && !flag3)
                            {
                                for (j = 1; j <= dist2 && (num7 - j < 0 || num7 - j >= img.Width || (num2 < 0 || num2 >= img.Height || img.GetPixelU32(num7 - j, num2) != color) && (num2 - j < 0 || num2 - j >= img.Height || img.GetPixelU32(num7 - j, num2 - j) != color) && (num2 + j < 0 || num2 + j >= img.Height || img.GetPixelU32(num7 - j, num2 + j) != color)) && (num7 < 0 || num7 >= img.Width || (num2 - j < 0 || num2 - j >= img.Height || img.GetPixelU32(num7, num2 - j) != color) && (num2 + j < 0 || num2 + j >= img.Height || img.GetPixelU32(num7, num2 + j) != color)) && (num7 + j < 0 || num7 + j >= img.Width || (num2 < 0 || num2 >= img.Height || img.GetPixelU32(num7 + j, num2) != color) && (num2 - j < 0 || num2 - j >= img.Height || img.GetPixelU32(num7 + j, num2 - j) != color) && (num2 + j < 0 || num2 + j >= img.Height || img.GetPixelU32(num7 + j, num2 + j) != color)); j++)
                                {
                                }
                                if (j > dist2)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            else if (!flag2 && flag3)
                            {
                                for (j = 1; j <= dist1 && (num6 - j < 0 || num6 - j >= img.Width || (num < 0 || num >= fImage.Height || fImage.GetPixelAlpha(num6 - j, num) < 200) && (num - j < 0 || num - j >= fImage.Height || fImage.GetPixelAlpha(num6 - j, num - j) < 200) && (num + j < 0 || num + j >= fImage.Height || fImage.GetPixelAlpha(num6 - j, num + j) < 200)) && (num6 < 0 || num6 >= fImage.Width || (num - j < 0 || num - j >= fImage.Height || fImage.GetPixelAlpha(num6, num - j) < 200) && (num + j < 0 || num + j >= fImage.Height || fImage.GetPixelAlpha(num6, num + j) < 200)) && (num6 + j < 0 || num6 + j >= fImage.Width || (num < 0 || num >= fImage.Height || fImage.GetPixelAlpha(num6 + j, num) < 200) && (num - j < 0 || num - j >= fImage.Height || fImage.GetPixelAlpha(num6 + j, num - j) < 200) && (num + j < 0 || num + j >= fImage.Height || fImage.GetPixelAlpha(num6 + j, num + j) < 200)); j++)
                                {
                                }
                                if (j > dist1)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag2 && flag3 || j == 1 || j == 2)
                            {
                                num3++;
                            }
                            if (flag2 || flag3)
                            {
                                num4++;
                            }
                        }
                        num8++;
                        num6++;
                        num7++;
                    }
                    num5++;
                    num++;
                    num2++;
                }
                if (!flag)
                {
                    byte b2 = (byte)Math.Min(255, (num3 << 8) / num4);
                    if (b2 > b)
                    {
                        b = b2;
                        result = c;
                    }
                }
            }
            return result;
        }
    }
}
