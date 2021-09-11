using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace MSFBlitzBot
{
    public static class FontManager
    {
        private static readonly PrivateFontCollection _pfc = new();

        public static void Load(string filename)
        {
            _pfc.AddFontFile(filename);
        }

        public static Font GetFont(string name, float size, FontStyle style = FontStyle.Regular)
        {
            FontFamily[] families = _pfc.Families;
            foreach (FontFamily fontFamily in families)
            {
                if (fontFamily.Name == name)
                {
                    return new Font(fontFamily, size, style, GraphicsUnit.Pixel);
                }
            }
            return null;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
        private static extern unsafe void* CopyMemory(void* dest, void* src, ulong count);

        public static unsafe FImage GetTextImage(string text, string fontName, float size, FontStyle style = FontStyle.Regular, float italic = 0.25f, Color? color = null, bool cleanup = false)
        {
            if (!color.HasValue)
            {
                color = Color.White;
            }
            FImage fImage;
            using (Font font = GetFont(fontName, size, style))
            {
                using StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center
                };
                SizeF sizeF = SizeF.Empty;
                using (Bitmap bitmap = new((int)(text.Length * size * 2f), (int)(size * 2f), PixelFormat.Format32bppArgb))
                {
                    bitmap.SetResolution(96f, 96f);
                    using Graphics graphics = Graphics.FromImage(bitmap);
                    sizeF = graphics.MeasureString(text, font, bitmap.Width, format);
                }
                using Bitmap bitmap2 = new((int)(sizeF.Width + 0.5f) + 2, (int)(size * 2f), PixelFormat.Format32bppArgb);
                bitmap2.SetResolution(96f, 96f);
                using (Graphics graphics2 = Graphics.FromImage(bitmap2))
                {
                    using SolidBrush brush = new(color.Value);
                    graphics2.DrawString(text, font, brush, new RectangleF(0f, font.Height - font.Size, bitmap2.Width, bitmap2.Height), format);
                }
                fImage = new FImage(bitmap2.Width + (int)Math.Ceiling(2f * size * italic), bitmap2.Height, 4);
                fImage.Clear();
                BitmapData bitmapData = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, bitmap2.PixelFormat);
                byte* ptr = (byte*)(void*)bitmapData.Scan0;
                byte* pointer = fImage.Pointer;
                for (int i = 0; i < bitmap2.Height; i++)
                {
                    void* src = ptr + i * bitmapData.Stride;
                    void* dest = pointer + i * fImage.Stride + (int)Math.Round((bitmap2.Height - i) * italic) * fImage.Bpp;
                    CopyMemory(dest, src, (ulong)bitmapData.Stride);
                }
            }
            if (cleanup)
            {
                for (int j = 0; j < fImage.Height; j++)
                {
                    for (int k = 0; k < fImage.Width; k++)
                    {
                        Color pixel = fImage.GetPixel(k, j);
                        if (pixel.A > 0 && (pixel.R != color.Value.R || pixel.G != color.Value.G || pixel.R != color.Value.R))
                        {
                            Color c = Color.FromArgb(pixel.R, color.Value);
                            fImage.SetPixel(k, j, c);
                        }
                    }
                }
            }
            return fImage;
        }
    }
}
