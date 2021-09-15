using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MSFBlitzBot
{
    public class FImage : IDisposable
    {
        public enum MatchColorMode
        {
            Color,
            Grayscale,
            GrayscaleWithRed
        }

        private unsafe byte* _rawData = null;
        private unsafe byte* _ptr;
        private bool _disposed;

        private Color _areaBoxColor;
        private byte _areaBoxProximity;
        private Rectangle _areaBox = Rectangle.Empty;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Size Size => new(Width, Height);

        public int Stride { get; private set; }

        public int Bpp { get; private set; }

        public unsafe byte* Pointer { get; private set; } = null;

        public unsafe IntPtr IntPtr => (IntPtr)Pointer;

        public unsafe bool IsValid => Pointer != null && Width > 0 && Height > 0;

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
        private static extern unsafe void* CopyMemory(void* dest, void* src, ulong count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
        private static extern IntPtr MemSet(IntPtr dest, int c, int byteCount);

        public unsafe FImage()
        {
            Pointer = null;
        }

        public unsafe byte[] GetArray()
        {
            int num = Stride * Height;
            byte[] array = new byte[num];
            Marshal.Copy((IntPtr)_rawData, array, 0, num);
            return array;
        }

        public unsafe FImage(FImage source)
        {
            Bpp = source.Bpp;
            Height = source.Height;
            Width = source.Width;
            Stride = source.Stride;
            int num = Height * Stride;
            Pointer = (byte*)(void*)MemoryManager.Alloc(num);
            _ptr = Pointer;
            _rawData = Pointer;
            source.ResetPos();
            CopyMemory(_ptr, source._ptr, (ulong)num);
        }

        public unsafe FImage(int width, int height, int bpp)
        {
            Bpp = bpp;
            Height = height;
            Width = width;
            Stride = Width * Bpp;
            Pointer = (byte*)(void*)MemoryManager.Alloc(Height * Stride);
            _ptr = Pointer;
            _rawData = Pointer;
        }

        public unsafe FImage(string filename, float scale = 1f, int bpp = -1)
        {
            using Bitmap bitmap = new(filename);
            Initialize(bitmap, (int)Math.Round(bitmap.Width * scale), (int)Math.Round(bitmap.Height * scale), bpp);
        }

        public unsafe FImage(string filename, int width, int height, int bpp = -1)
        {
            using Bitmap source = new(filename);
            Initialize(source, width, height, bpp);
        }

        public unsafe FImage(Bitmap source, float scale = 1f)
        {
            Initialize(source, scale);
        }

        public unsafe FImage(FImage source, Rectangle area)
        {
            Bpp = source.Bpp;
            Height = area.Height;
            Width = area.Width;
            Stride = Width * Bpp;
            Pointer = (byte*)(void*)MemoryManager.Alloc(Height * Stride);
            _ptr = Pointer;
            _rawData = Pointer;
            source.SetPos(area.X, area.Y);
            SetPos(0, 0);
            for (int i = 0; i < Height; i++)
            {
                CopyMemory(_ptr, source._ptr, (ulong)Stride);
                _ptr += Stride;
                source._ptr += source.Stride;
            }
        }

        public FImage(FImage source, float xmin = 0f, float ymin = 0f, float xmax = 1f, float ymax = 1f)
            : this(source, new Rectangle((int)(xmin * source.Width), (int)(ymin * source.Height), (int)((xmax - xmin) * source.Width), (int)((ymax - ymin) * source.Height)))
        {
        }

        public unsafe FImage(FImage source, float scale)
        {
            using Bitmap source2 = source.GetBitmap();
            Initialize(source2, scale);
        }

        public unsafe FImage(FImage source, int newWidth, int newHeight, int bpp = -1)
        {
            using Bitmap source2 = source.GetBitmap();
            Initialize(source2, newWidth, newHeight, bpp);
        }

        public FImage(FImage source, RectangleF area)
            : this(source, new Rectangle((int)(area.X * source.Width), (int)(area.Y * source.Height), (int)(area.Width * source.Width), (int)(area.Height * source.Height)))
        {
        }

        public void Dispose()
        {
            Dispose(_: true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool _)
        {
            if (!_disposed)
            {
                MemoryManager.Free((IntPtr)Pointer);
                Pointer = null;
                _disposed = true;
            }
        }

        ~FImage()
        {
            Dispose(_: false);
        }

        public unsafe void Initialize(Bitmap source)
        {
            if (Pointer != null && (source.Width != Width || source.Height != Height))
            {
                MemoryManager.Free((IntPtr)Pointer);
                Pointer = null;
            }
            BitmapData bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, source.PixelFormat);
            if (Pointer == null)
            {
                Bpp = Image.GetPixelFormatSize(source.PixelFormat) >> 3;
                Height = bitmapData.Height;
                Width = bitmapData.Width;
                Stride = Width * Bpp;
                Pointer = (byte*)(void*)MemoryManager.Alloc(Height * Stride);
            }
            byte* PtrFirstPixel = (byte*)(void*)bitmapData.Scan0;
            _rawData = Pointer;
            _ptr = Pointer;
            Parallel.For(0, Height, delegate (int y)
            {
                void* src = PtrFirstPixel + y * bitmapData.Stride;
                void* dest = _rawData + y * Stride;
                CopyMemory(dest, src, (ulong)Stride);
            });
            source.UnlockBits(bitmapData);
        }

        private unsafe void Initialize(Bitmap source, float scale)
        {
            int x = 0;
            Bitmap bitmap = source;
            if (scale != 1f)
            {
                bitmap = new Bitmap(source, new Size((int)(source.Width * scale), (int)(source.Height * scale)));
            }
            int width = bitmap.Width;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(x, 0, width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Bpp = Image.GetPixelFormatSize(bitmap.PixelFormat) >> 3;
            Height = bitmapData.Height;
            Width = bitmapData.Width;
            Stride = Width * Bpp;
            byte* PtrFirstPixel = (byte*)(void*)bitmapData.Scan0;
            Pointer = (byte*)(void*)MemoryManager.Alloc(Height * Stride);
            _rawData = Pointer;
            _ptr = Pointer;
            Parallel.For(0, Height, delegate (int y)
            {
                void* src = PtrFirstPixel + y * bitmapData.Stride;
                void* dest = _rawData + y * Stride;
                CopyMemory(dest, src, (ulong)Stride);
            });
            bitmap.UnlockBits(bitmapData);
            if (bitmap != source)
            {
                bitmap.Dispose();
            }
        }

        private unsafe void Initialize(Bitmap source, int width, int height, int bpp = -1)
        {
            Bitmap bitmap = (width != source.Width || height != source.Height) ? new Bitmap(source, new Size(width, height)) : new Bitmap(source);
            PixelFormat pixelFormat = bitmap.PixelFormat;
            switch (bpp)
            {
                case 3:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    break;
                case 4:
                    pixelFormat = PixelFormat.Format32bppArgb;
                    break;
            }
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, pixelFormat);
            Bpp = Image.GetPixelFormatSize(pixelFormat) >> 3;
            Height = bitmapData.Height;
            Width = bitmapData.Width;
            Stride = Width * Bpp;
            byte* ptrFirstPixel = (byte*)(void*)bitmapData.Scan0;
            Pointer = (byte*)(void*)MemoryManager.Alloc(Height * Stride);
            _rawData = Pointer;
            _ptr = Pointer;
            if (pixelFormat == bitmap.PixelFormat)
            {
                Parallel.For(0, Height, delegate (int y)
                {
                    void* src = ptrFirstPixel + y * bitmapData.Stride;
                    void* dest = _rawData + y * Stride;
                    CopyMemory(dest, src, (ulong)Stride);
                });
            }
            else
            {
                Parallel.For(0, Height, delegate (int y)
                {
                    byte* ptr = ptrFirstPixel + y * bitmapData.Stride;
                    byte* ptr2 = _rawData + y * Stride;
                    for (int i = 0; i < Width; i++)
                    {
                        if (Bpp == 4)
                        {
                            *ptr2++ = byte.MaxValue;
                        }
                        else
                        {
                            ptr++;
                        }
                        CopyMemory(ptr2, ptr, 3uL);
                        ptr2 += 3;
                        ptr += 3;
                    }
                });
            }
            bitmap.UnlockBits(bitmapData);
            if (bitmap != source)
            {
                bitmap.Dispose();
            }
        }

        public unsafe FImage GetCenteredScale(int width, int height)
        {
            int num = width;
            int num2 = height;
            int num3 = 0;
            int num4 = 0;
            if (Height * width / Width > height)
            {
                num = Width * height / Height;
                num3 = width - num >> 1;
            }
            else
            {
                num2 = Height * width / Width;
                num4 = height - num2 >> 1;
            }
            if (num3 == 0 && num4 == 0)
            {
                return new FImage(this, width, height);
            }
            Bitmap bitmap;
            using (Bitmap original = GetBitmap())
            {
                bitmap = new Bitmap(original, num, num2);
            }
            FImage img = new(width, height, Bpp);
            img.Clear();
            int num5 = Image.GetPixelFormatSize(bitmap.PixelFormat) >> 3;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            _ = bitmapData.Height;
            int width2 = bitmapData.Width;
            ulong sstride = (ulong)(width2 * num5);
            byte* sPtrFirstPixel = (byte*)(void*)bitmapData.Scan0;
            byte* ptr = img.Pointer + num3 * img.Bpp + num4 * img.Stride;
            Parallel.For(0, bitmap.Height, delegate (int py)
            {
                void* src = sPtrFirstPixel + py * bitmapData.Stride;
                void* dest = ptr + py * img.Stride;
                CopyMemory(dest, src, sstride);
            });
            bitmap.Dispose();
            return img;
        }

        public FImage GetScaledExtract(RectangleF area, float fontSize)
        {
            float scale = 108000f / (fontSize * Height);
            using FImage fImage = new(this, area);
            using Bitmap source = fImage.GetBitmap();
            fImage.Dispose();
            return new FImage(source, scale);
        }

        public FImage GetScaledExtract(RectangleF area, int width, int height)
        {
            using FImage source = new(this, area);
            return new FImage(source, width, height);
        }

        public FImage GetScaledExtract(Rectangle area, int width, int height)
        {
            using FImage source = new(this, area);
            return new FImage(source, width, height);
        }

        public FImage GetScaledExtract(float x0, float y0, float x1, float y1, int width, int height)
        {
            using FImage source = new(this, new RectangleF(x0, y0, x1 - x0, y1 - y0));
            return new FImage(source, width, height);
        }

        public unsafe Bitmap GetBitmap()
        {
            Bitmap bitmap = new(Width, Height, (Bpp == 4) ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte* ptrFirstPixel = (byte*)(void*)bitmapData.Scan0;
            Parallel.For(0, Height, delegate (int y)
            {
                void* src = _rawData + y * Stride;
                void* dest = ptrFirstPixel + y * bitmapData.Stride;
                CopyMemory(dest, src, (ulong)bitmapData.Stride);
            });
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public bool IsBlank(byte minAlpha = 1)
        {
            Rectangle boundingBox = GetBoundingBox(minAlpha);
            return boundingBox.Width == 0 || boundingBox.Height == 0;
        }

        public void Save(string filename)
        {
            using Bitmap bitmap = GetBitmap();
            bitmap.SetResolution(96f, 96f);
            bitmap.Save(filename);
        }

        public unsafe void SetPos(int x, int y)
        {
            _ptr = _rawData + y * Stride + x * Bpp;
        }

        public unsafe void ResetPos()
        {
            _ptr = _rawData;
        }

        public unsafe void KeepColor(Color color, Color newColor, byte diff = 5)
        {
            int num = Height * Width;
            byte* ptr = _rawData;
            for (int i = 0; i < num; i++, ptr += Bpp)
            {
                if (*ptr != color.B || ptr[1] != color.G || ptr[2] != color.R)
                {
                    int val = Math.Abs(ptr[2] - color.R);
                    int val2 = Math.Abs(ptr[1] - color.G);
                    int val3 = Math.Abs(*ptr - color.B);
                    int num2 = Math.Max(val, Math.Max(val2, val3));
                    if (num2 == 0)
                    {
                        continue;
                    }
                    if (num2 < diff)
                    {
                        int num3 = (num2 << 8) / diff;
                        if (num3 < 255)
                        {
                            byte b = (byte)(255 - num3);
                            *ptr = (byte)(newColor.B * b >> 8);
                            ptr[1] = (byte)(newColor.G * b >> 8);
                            ptr[2] = (byte)(newColor.R * b >> 8);
                            continue;
                        }
                    }
                    byte* intPtr = ptr;
                    byte b2;
                    ptr[1] = b2 = ptr[2] = 0;
                    *intPtr = b2;
                }
                else
                {
                    *ptr = newColor.B;
                    ptr[1] = newColor.G;
                    ptr[2] = newColor.R;
                }
            }
        }

        public unsafe void KeepColor(Color color, byte diff = 5, Color? newColor = null)
        {
            int num = Height * Width;
            byte* ptr = _rawData;
            for (int i = 0; i < num; i++, ptr += Bpp)
            {
                if (*ptr == color.B && ptr[1] == color.G && ptr[2] == color.R)
                {
                    continue;
                }
                int val = Math.Abs(ptr[2] - color.R);
                int val2 = Math.Abs(ptr[1] - color.G);
                int val3 = Math.Abs(*ptr - color.B);
                int num2 = Math.Max(val, Math.Max(val2, val3));
                if (num2 == 0)
                {
                    continue;
                }
                if (num2 < diff)
                {
                    int num3 = (num2 << 8) / diff;
                    if (num3 < 255)
                    {
                        byte b = (byte)(255 - num3);
                        *ptr = (byte)(color.B * b >> 8);
                        ptr[1] = (byte)(color.G * b >> 8);
                        ptr[2] = (byte)(color.R * b >> 8);
                        continue;
                    }
                }
                byte* intPtr = ptr;
                byte b2;
                ptr[1] = b2 = ptr[2] = 0;
                *intPtr = b2;
            }
        }

        public unsafe void BlackBackground()
        {
            int num = Height * Width;
            byte* ptr = _rawData;
            int num2 = 0;
            while (num2 < num)
            {
                byte b = ptr[3];
                if (b != byte.MaxValue)
                {
                    *ptr = (byte)(*ptr * b >> 8);
                    ptr[1] = (byte)(ptr[1] * b >> 8);
                    ptr[2] = (byte)(ptr[2] * b >> 8);
                    ptr[3] = byte.MaxValue;
                }
                num2++;
                ptr += Bpp;
            }
        }

        public unsafe Color GetPixel()
        {
            var result = Color.FromArgb((Bpp == 4) ? _ptr[3] : byte.MaxValue, _ptr[2], _ptr[1], *_ptr);
            _ptr += Bpp;
            return result;
        }

        public unsafe Color GetPixelStay()
        {
            return Color.FromArgb((Bpp == 4) ? _ptr[3] : byte.MaxValue, _ptr[2], _ptr[1], *_ptr);
        }

        public Color GetPixel(int x, int y)
        {
            GetPixel(x, y, out var r, out var g, out var b, out var a);
            return Color.FromArgb(a, r, g, b);
        }

        public Color GetPixel(float x, float y)
        {
            GetPixel((int)(x * Width), (int)(y * Height), out var r, out var g, out var b, out var a);
            return Color.FromArgb(a, r, g, b);
        }

        public void GetPixel(float x, float y, out byte r, out byte g, out byte b)
        {
            GetPixel((int)(x * Width), (int)(y * Height), out r, out g, out b);
        }

        public unsafe void GetPixel(int x, int y, out byte r, out byte g, out byte b)
        {
            byte* ptr = _rawData + y * Stride + x * Bpp;
            b = *ptr;
            g = ptr[1];
            r = ptr[2];
        }

        public unsafe void GetPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
        {
            byte* ptr = _rawData + y * Stride + x * Bpp;
            b = *ptr;
            g = ptr[1];
            r = ptr[2];
            a = ptr[3];
        }

        public uint GetPixelU32(int x, int y)
        {
            GetPixel(x, y, out var r, out var g, out var b, out var a);
            return (uint)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public uint GetPixelU32(float x, float y)
        {
            return GetPixelU32((int)(x * Width), (int)(y * Height));
        }

        public unsafe byte GetPixelAlpha(int x, int y)
        {
            return (_rawData + y * Stride + x * Bpp)[3];
        }

        public byte GetPixelAlphaExt(int x, int y)
        {
            return x < 0 || x >= Width || y < 0 || y >= Height ? (byte)0 : GetPixelAlpha(x, y);
        }

        public unsafe byte GetPixelAlpha()
        {
            byte result = _ptr[3];
            _ptr += 4;
            return result;
        }

        public unsafe void SetPixel(Color c)
        {
            *_ptr++ = c.B;
            *_ptr++ = c.G;
            *_ptr++ = c.R;
            if (Bpp == 4)
            {
                *_ptr++ = c.A;
            }
        }

        public unsafe void SetPixel(uint c)
        {
            if (Bpp == 4)
            {
                uint* ptr = (uint*)_ptr;
                *ptr = c;
                _ptr += 4;
                return;
            }
            throw new NotImplementedException();
        }

        public unsafe void NextPixel()
        {
            _ptr += Bpp;
        }

        public unsafe void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            byte* ptr = _rawData + y * Stride + x * Bpp;
            *ptr = b;
            ptr[1] = g;
            ptr[2] = r;
            if (Bpp == 4)
            {
                ptr[3] = a;
            }
        }

        public unsafe void SetPixel(int x, int y, Color c)
        {
            byte* ptr = _rawData + y * Stride + x * Bpp;
            *ptr = c.B;
            ptr[1] = c.G;
            ptr[2] = c.R;
            if (Bpp == 4)
            {
                ptr[3] = c.A;
            }
        }

        public unsafe void SetPixel(int x, int y, uint color)
        {
            byte* ptr = _rawData + y * Stride + x * Bpp;
            *ptr = (byte)(color & 0xFFu);
            ptr[1] = (byte)(color >> 8);
            ptr[2] = (byte)(color >> 16);
            if (Bpp == 4)
            {
                ptr[3] = (byte)(color >> 24);
            }
        }

        public unsafe void SetPixel(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            *_ptr++ = b;
            *_ptr++ = g;
            *_ptr++ = r;
            if (Bpp == 4)
            {
                *_ptr++ = a;
            }
        }

        public unsafe void GetPixel(out byte r, out byte g, out byte b)
        {
            b = *_ptr++;
            g = *_ptr++;
            r = *_ptr++;
            if (Bpp == 4)
            {
                _ptr++;
            }
        }

        public unsafe void GetPixel(out byte r, out byte g, out byte b, out byte a)
        {
            b = *_ptr++;
            g = *_ptr++;
            r = *_ptr++;
            a = Bpp == 4 ? *_ptr++ : byte.MaxValue;
        }

        public unsafe bool HasColor(RectangleF area, uint color)
        {
            int x = (int)(area.X * Width);
            int num = (int)(area.Y * Height);
            int num2 = (int)(area.Width * Width);
            int num3 = (int)(area.Height * Height);
            for (int i = 0; i < num3; i++)
            {
                SetPos(x, num + i);
                uint* ptr = (uint*)_ptr;
                for (int j = 0; j < num2; j++)
                {
                    if (*ptr == color)
                    {
                        return true;
                    }
                    ptr++;
                }
            }
            return false;
        }

        public Color GetAverageColor(RectangleF area)
        {
            return GetAverageColor(new Rectangle((int)(area.X * Width), (int)(area.Y * Height), (int)(area.Width * Width), (int)(area.Height * Height)));
        }

        public Color GetAverageColor(Rectangle area)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            for (int i = area.Y; i < area.Bottom; i++)
            {
                SetPos(area.X, i);
                for (int j = area.X; j < area.Right; j++)
                {
                    Color pixel = GetPixel();
                    num += pixel.R;
                    num2 += pixel.G;
                    num3 += pixel.B;
                }
            }
            int num4 = area.Width * area.Height;
            return Color.FromArgb((byte)(num / num4), (byte)(num2 / num4), (byte)(num3 / num4));
        }

        public unsafe FImage Italic(float dist)
        {
            FImage fImage = new(Width, Height, Bpp);
            for (int i = 0; i < Height; i++)
            {
                float num = (i - Height / 2f) * dist;
                int num2 = (num > 0f) ? ((int)(num + 0.5f)) : ((int)(num - 0.5f));
                SetPos(num2, i);
                for (int j = 0; j < Width; j++)
                {
                    if (j + num2 < 0 || j + num2 >= Width - 1)
                    {
                        fImage.SetPixel(0, 0, 0, 0);
                    }
                    else
                    {
                        fImage.SetPixel(_ptr[2], _ptr[1], *_ptr, _ptr[3]);
                    }
                    _ptr += Bpp;
                }
            }
            return fImage;
        }

        public unsafe Rectangle GetBoundingBox(byte alpha)
        {
            Rectangle empty = Rectangle.Empty;
            ResetPos();
            bool flag = false;
            int num = Stride - Width * Bpp;
            int i;
            for (i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    GetPixel(out _, out _, out _, out var a);
                    if (a >= alpha)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num;
            }
            if (i == Height)
            {
                return empty;
            }
            flag = false;
            SetPos(0, Height - 1);
            int num2;
            for (num2 = Height - 1; num2 > i; num2--)
            {
                for (int k = 0; k < Width; k++)
                {
                    GetPixel(out _, out _, out _, out var a2);
                    if (a2 >= alpha)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num - (Stride << 1);
            }
            num2 -= i;
            flag = false;
            int l;
            for (l = 0; l < Width; l++)
            {
                for (int m = i; m < i + num2; m++)
                {
                    GetPixel(l, m, out _, out _, out _, out var a3);
                    if (a3 >= alpha)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            flag = false;
            int num3;
            for (num3 = Width - 1; num3 > l; num3--)
            {
                for (int n = i; n < i + num2; n++)
                {
                    GetPixel(num3, n, out _, out _, out _, out var a4);
                    if (a4 >= alpha)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            num3 -= l;
            return new Rectangle(l, i, num3, num2);
        }

        public unsafe Rectangle GetBoundingBoxBlack(byte tolerance = 50)
        {
            Rectangle empty = Rectangle.Empty;
            ResetPos();
            bool flag = false;
            int num = Stride - Width * Bpp;
            int i;
            for (i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    GetPixel(out var r, out var g, out var b, out _);
                    if (r > tolerance || g > tolerance || b > tolerance)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num;
            }
            if (i == Height)
            {
                return empty;
            }
            flag = false;
            SetPos(0, Height - 1);
            int num2;
            for (num2 = Height - 1; num2 > i; num2--)
            {
                for (int k = 0; k < Width; k++)
                {
                    GetPixel(out var r2, out var g2, out var b2, out _);
                    if (r2 > tolerance || g2 > tolerance || b2 > tolerance)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num - (Stride << 1);
            }
            num2 -= i;
            flag = false;
            int l;
            for (l = 0; l < Width; l++)
            {
                for (int m = i; m < i + num2; m++)
                {
                    GetPixel(l, m, out var r3, out var g3, out var b3, out _);
                    if (r3 > tolerance || g3 > tolerance || b3 > tolerance)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            flag = false;
            int num3;
            for (num3 = Width - 1; num3 > l; num3--)
            {
                for (int n = i; n < i + num2; n++)
                {
                    GetPixel(num3, n, out var r4, out var g4, out var b4, out _);
                    if (r4 > tolerance || g4 > tolerance || b4 > tolerance)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            num3 -= l;
            return new Rectangle(l, i, num3, num2);
        }

        public unsafe void Clear()
        {
            if (Bpp != 4)
            {
                throw new NotImplementedException();
            }
            MemSet((IntPtr)_rawData, 0, Height * Stride);
        }

        public unsafe FImage ItalicAndCrop(float dist)
        {
            Rectangle area = GetBoundingBox(200);
            FImage result = new(area.Width + (int)Math.Ceiling(area.Height * dist), area.Height, Bpp);
            MemSet((IntPtr)result._rawData, 0, result.Height * result.Stride);
            Parallel.For(0, area.Height, delegate (int y)
            {
                void* src = _rawData + area.X * Bpp + (area.Y + y) * Stride;
                void* dest = result._rawData + (int)(dist * (result.Height - y - 1)) * Bpp + y * result.Stride;
                CopyMemory(dest, src, (ulong)(area.Width * Bpp));
            });
            return result;
        }

        public unsafe FImage Crop(byte minalpha = 1, bool cropX = true, bool cropY = true)
        {
            Rectangle area = GetBoundingBox(minalpha);
            if (!cropX)
            {
                area.X = 0;
                area.Width = Width;
            }
            if (!cropY)
            {
                area.Y = 0;
                area.Height = Height;
            }
            FImage result = new(area.Width, area.Height, Bpp);
            Parallel.For(0, area.Height, delegate (int y)
            {
                void* src = _rawData + area.X * Bpp + (area.Y + y) * Stride;
                void* dest = result._rawData + y * result.Stride;
                CopyMemory(dest, src, (ulong)(area.Width * Bpp));
            });
            return result;
        }

        public unsafe FImage CropBlack(byte tolerance = 50)
        {
            Rectangle area = GetBoundingBoxBlack(tolerance);
            FImage result = new(area.Width, area.Height, Bpp);
            Parallel.For(0, area.Height, delegate (int y)
            {
                void* src = _rawData + area.X * Bpp + (area.Y + y) * Stride;
                void* dest = result._rawData + y * result.Stride;
                CopyMemory(dest, src, (ulong)(area.Width * Bpp));
            });
            return result;
        }

        public unsafe byte Match(FImage displayImage, byte proximity = 30, byte alphaThreshold = 100)
        {
            if (Width != displayImage.Width || Height != displayImage.Height)
            {
                return 0;
            }
            byte* ptr = _rawData;
            byte* ptr2 = displayImage._rawData;
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (Bpp == 3 || ptr[3] >= alphaThreshold)
                    {
                        if (Math.Abs(*ptr2 - *ptr) <= proximity && Math.Abs(ptr2[1] - ptr[1]) <= proximity && Math.Abs(ptr2[2] - ptr[2]) <= proximity)
                        {
                            num++;
                        }
                        num2++;
                    }
                    ptr += Bpp;
                    ptr2 += displayImage.Bpp;
                }
            }
            if (num2 == 0)
            {
                return 0;
            }
            int val = (num << 8) / num2;
            return (byte)Math.Min(255, val);
        }

        public unsafe byte MatchItem(FImage displayImage, byte proximity = 30, byte alphaThreshold = 200)
        {
            if (Width != displayImage.Width || Height != displayImage.Height)
            {
                return 0;
            }
            byte* ptr = _rawData;
            byte* ptr2 = displayImage._rawData;
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (ptr[3] >= alphaThreshold)
                    {
                        if (Math.Abs(*ptr2 - *ptr) <= proximity && Math.Abs(ptr2[1] - ptr[1]) <= proximity && Math.Abs(ptr2[2] - ptr[2]) <= proximity)
                        {
                            num++;
                        }
                        num2++;
                    }
                    else if (ptr[3] == 0)
                    {
                        if (*ptr2 <= proximity && ptr2[1] <= proximity && ptr2[2] <= proximity)
                        {
                            num++;
                        }
                        num2++;
                    }
                    ptr += Bpp;
                    ptr2 += displayImage.Bpp;
                }
            }
            if (num2 == 0)
            {
                return 0;
            }
            int val = (num << 8) / num2;
            return (byte)Math.Min(255, val);
        }

        public unsafe float MatchF(FImage pattern, float pivotx, float pivoty, int posx, int posy, float topratio = 1f, byte proximity = 30, byte alphaThreshold = 100)
        {
            int num = (int)(posx - pivotx * pattern.Width);
            int num2 = (int)(posy - pivoty * pattern.Height);
            if (num + pattern.Width <= 0)
            {
                return 0f;
            }
            if (num2 + pattern.Height <= 0)
            {
                return 0f;
            }
            if (num >= Width)
            {
                return 0f;
            }
            if (num2 >= Height)
            {
                return 0f;
            }
            byte* ptr = _rawData + num2 * Stride + num * Bpp;
            byte* ptr2 = pattern._rawData;
            int num3 = Stride - pattern.Width * Bpp;
            int num4 = (int)(pattern.Height * topratio);
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            while (num7 < num4 && num2 + num7 < Height)
            {
                if (num2 + num7 < 0)
                {
                    ptr += Stride;
                    ptr2 += Stride;
                }
                else
                {
                    int num8 = 0;
                    if (num < 0)
                    {
                        num8 = -num;
                        ptr += Bpp * num8;
                        ptr2 += pattern.Bpp * num8;
                    }
                    while (num8 < pattern.Width)
                    {
                        if (num + num8 >= Width)
                        {
                            ptr += Bpp * (pattern.Width - num8);
                            ptr2 += Bpp * (pattern.Width - num8);
                            break;
                        }
                        if (Bpp == 3 || ptr2[3] >= alphaThreshold)
                        {
                            if (Math.Abs(*ptr - *ptr2) <= proximity && Math.Abs(ptr[1] - ptr2[1]) <= proximity && Math.Abs(ptr[2] - ptr2[2]) <= proximity)
                            {
                                num5++;
                            }
                            num6++;
                        }
                        num8++;
                        ptr += Bpp;
                        ptr2 += pattern.Bpp;
                    }
                }
                num7++;
                ptr += num3;
            }
            return num5 / (float)num6;
        }

        public unsafe byte Match(FImage pattern, float posx, float posy, float topRatio = 1f, byte proximity = 30, byte alphaThreshold = 100, MatchColorMode mode = MatchColorMode.Color, string debugFilename = null)
        {
            int num = (int)Math.Round(posx * Width);
            int num2 = (int)Math.Round(posy * Height);
            int width = pattern.Width;
            _ = pattern.Height;
            byte* ptr = pattern._rawData;
            byte* ptr2 = _rawData + num * Bpp + num2 * Stride;
            int num3 = 0;
            int num4 = 0;
            int num5 = Stride - pattern.Width * Bpp;
            if (num2 < 0)
            {
                ptr += -num2 * pattern.Stride;
                ptr2 += -num2 * Stride;
                num2 = 0;
            }
            FImage fImage = null;
            FImage fImage2 = null;
            FImage fImage3 = null;
            if (debugFilename != null)
            {
                fImage = new FImage(pattern.Width, pattern.Height, pattern.Bpp);
                fImage2 = new FImage(pattern.Width, pattern.Height, pattern.Bpp);
                fImage3 = new FImage(pattern.Width, pattern.Height, pattern.Bpp);
            }
            int num6 = (int)(pattern.Height * topRatio);
            int num7 = 0;
            while (num7 < num6 && num2 + num7 < Height)
            {
                int num8 = 0;
                if (num < 0)
                {
                    num8 = -num;
                    ptr2 += Bpp * num8;
                    ptr += pattern.Bpp * num8;
                }
                while (num8 < width)
                {
                    if (num + num8 >= Width)
                    {
                        ptr2 += Bpp * (pattern.Width - num8);
                        ptr += Bpp * (pattern.Width - num8);
                        break;
                    }
                    if (debugFilename != null && ptr[3] < alphaThreshold)
                    {
                        fImage?.SetPixel(num8, num7, byte.MaxValue, byte.MaxValue, byte.MaxValue, 127);
                        fImage2?.SetPixel(num8, num7, byte.MaxValue, byte.MaxValue, byte.MaxValue, 127);
                        fImage3?.SetPixel(num8, num7, byte.MaxValue, byte.MaxValue, byte.MaxValue, 127);
                    }
                    fImage3?.SetPixel(num8, num7, ptr2[2], ptr2[1], *ptr2, ptr2[3]);
                    if (ptr[3] >= alphaThreshold)
                    {
                        switch (mode)
                        {
                            case MatchColorMode.Grayscale:
                                {
                                    byte b5 = (byte)Math.Min(255f, *ptr * 0.07f + ptr[1] * 0.72f + ptr[2] * 0.21f);
                                    fImage2?.SetPixel(num8, num7, b5, b5, b5, byte.MaxValue);
                                    if (Math.Abs(*ptr2 - b5) <= proximity)
                                    {
                                        num3++;
                                        fImage?.SetPixel(num8, num7, 0, byte.MaxValue, 0, byte.MaxValue);
                                    }
                                    else
                                    {
                                        fImage?.SetPixel(num8, num7, byte.MaxValue, 0, 0, byte.MaxValue);
                                    }
                                    break;
                                }
                            case MatchColorMode.GrayscaleWithRed:
                                {
                                    bool flag = false;
                                    if (ptr2[2] > 120 && Math.Abs(ptr2[2] - *ptr2) > 10 && Math.Abs(ptr2[1] - *ptr2) < 5)
                                    {
                                        if (ptr2[-2] - ptr2[-4] < 5 || ptr2[6] - ptr2[4] < 5 || ptr2[2 - Stride] - ptr2[-Stride] < 5 || ptr2[2 + Stride] - ptr2[Stride] < 5)
                                        {
                                            fImage?.SetPixel(num8, num7, 0, 0, 0, 0);
                                            fImage2?.SetPixel(num8, num7, 0, 0, 0, 0);
                                            break;
                                        }
                                        flag = true;
                                    }
                                    byte b2 = (byte)Math.Min(255f, *ptr * 0.07f + ptr[1] * 0.72f + ptr[2] * 0.21f);
                                    if (flag)
                                    {
                                        byte b3 = (byte)((38760 + b2 * 65) / 255);
                                        byte b4 = (byte)((2280 + b2 * 65) / 255);
                                        fImage2?.SetPixel(num8, num7, b3, b4, b4, byte.MaxValue);
                                        if (Math.Abs(*ptr2 - b4) <= proximity && Math.Abs(ptr2[1] - b4) <= proximity && Math.Abs(ptr2[2] - b3) <= proximity)
                                        {
                                            num3++;
                                            fImage?.SetPixel(num8, num7, 0, byte.MaxValue, 0, byte.MaxValue);
                                        }
                                        else
                                        {
                                            fImage?.SetPixel(num8, num7, byte.MaxValue, 0, 0, byte.MaxValue);
                                        }
                                    }
                                    else
                                    {
                                        fImage2?.SetPixel(num8, num7, b2, b2, b2, byte.MaxValue);
                                        if (Math.Abs(*ptr2 - b2) <= proximity)
                                        {
                                            num3++;
                                            fImage?.SetPixel(num8, num7, 0, byte.MaxValue, 0, byte.MaxValue);
                                        }
                                        else
                                        {
                                            fImage?.SetPixel(num8, num7, byte.MaxValue, 0, 0, byte.MaxValue);
                                        }
                                    }
                                    break;
                                }
                            case MatchColorMode.Color:
                                if (debugFilename != null)
                                {
                                    byte b = (byte)Math.Max(Math.Abs(*ptr2 - *ptr), Math.Max(Math.Abs(ptr2[1] - ptr[1]), Math.Abs(ptr2[2] - ptr[2])));
                                    if (b <= proximity)
                                    {
                                        fImage2?.SetPixel(num8, num7, 0, (byte)(255 - b), 0, byte.MaxValue);
                                    }
                                    else
                                    {
                                        fImage2?.SetPixel(num8, num7, b, 0, 0, byte.MaxValue);
                                    }
                                }
                                if (Math.Abs(*ptr2 - *ptr) <= proximity && Math.Abs(ptr2[1] - ptr[1]) <= proximity && Math.Abs(ptr2[2] - ptr[2]) <= proximity)
                                {
                                    num3++;
                                    fImage?.SetPixel(num8, num7, 0, byte.MaxValue, 0, byte.MaxValue);
                                }
                                else
                                {
                                    fImage?.SetPixel(num8, num7, byte.MaxValue, 0, 0, byte.MaxValue);
                                }
                                break;
                        }
                        num4++;
                    }
                    else if (debugFilename != null)
                    {
                        fImage?.SetPixel(num8, num7, 0, 0, 0, 0);
                        fImage2?.SetPixel(num8, num7, 0, 0, 0, 0);
                    }
                    num8++;
                    ptr2 += Bpp;
                    ptr += pattern.Bpp;
                }
                num7++;
                ptr2 += num5;
            }
            if (debugFilename != null)
            {
                fImage?.Save("C:/temp/Debug_" + debugFilename + "_1.png");
                fImage2?.Save("C:/temp/Debug_" + debugFilename + "_2.png");
                fImage3?.Save("C:/temp/Debug_" + debugFilename + "_3.png");
                pattern.Save("C:/temp/Debug_" + debugFilename + "_Pattern.png");
                fImage?.Dispose();
                fImage2?.Dispose();
                fImage3?.Dispose();
            }
            if (num4 == 0)
            {
                return 0;
            }
            int val = (num3 << 8) / num4;
            return (byte)Math.Min(255, val);
        }

        public unsafe byte Match(FImage pattern, float pivotx, float pivoty, int posx, int posy, float topratio = 1f, byte proximity = 30, byte alphaThreshold = 100, bool grayscale = false)
        {
            int num = (int)(posx - pivotx * pattern.Width);
            int num2 = (int)(posy - pivoty * pattern.Height);
            if (num + pattern.Width <= 0)
            {
                return 0;
            }
            if (num2 + pattern.Height <= 0)
            {
                return 0;
            }
            if (num >= Width)
            {
                return 0;
            }
            if (num2 >= Height)
            {
                return 0;
            }
            byte* ptr = _rawData + num2 * Stride + num * Bpp;
            byte* ptr2 = pattern._rawData;
            int num3 = Stride - pattern.Width * Bpp;
            int num4 = (int)(pattern.Height * topratio);
            int num5 = 0;
            int num6 = 0;
            if (grayscale)
            {
                int num7 = 0;
                while (num7 < num4 && num2 + num7 < Height)
                {
                    if (num2 + num7 < 0)
                    {
                        ptr += Stride;
                        ptr2 += Stride;
                    }
                    else
                    {
                        int num8 = 0;
                        if (num < 0)
                        {
                            num8 = -num;
                            ptr += Bpp * num8;
                            ptr2 += pattern.Bpp * num8;
                        }
                        while (num8 < pattern.Width)
                        {
                            if (num + num8 >= Width)
                            {
                                ptr += Bpp * (pattern.Width - num8);
                                ptr2 += Bpp * (pattern.Width - num8);
                                break;
                            }
                            if (Bpp == 3 || ptr2[3] >= alphaThreshold)
                            {
                                byte b = (byte)Math.Min(255f, *ptr2 * 0.07f + ptr2[1] * 0.72f + ptr2[2] * 0.21f);
                                if (Math.Abs(*ptr - b) <= proximity)
                                {
                                    num5++;
                                }
                                num6++;
                            }
                            num8++;
                            ptr += Bpp;
                            ptr2 += pattern.Bpp;
                        }
                    }
                    num7++;
                    ptr += num3;
                }
            }
            else
            {
                int num9 = 0;
                while (num9 < num4 && num2 + num9 < Height)
                {
                    if (num2 + num9 < 0)
                    {
                        ptr += Stride;
                        ptr2 += Stride;
                    }
                    else
                    {
                        int num10 = 0;
                        if (num < 0)
                        {
                            num10 = -num;
                            ptr += Bpp * num10;
                            ptr2 += pattern.Bpp * num10;
                        }
                        while (num10 < pattern.Width)
                        {
                            if (num + num10 >= Width)
                            {
                                ptr += Bpp * (pattern.Width - num10);
                                ptr2 += Bpp * (pattern.Width - num10);
                                break;
                            }
                            if (Bpp == 3 || ptr2[3] >= alphaThreshold)
                            {
                                if (Math.Abs(*ptr - *ptr2) <= proximity && Math.Abs(ptr[1] - ptr2[1]) <= proximity && Math.Abs(ptr[2] - ptr2[2]) <= proximity)
                                {
                                    num5++;
                                }
                                num6++;
                            }
                            num10++;
                            ptr += Bpp;
                            ptr2 += pattern.Bpp;
                        }
                    }
                    num9++;
                    ptr += num3;
                }
            }
            return (byte)((num6 != 0) ? Math.Min(255, (num5 << 8) / num6) : 0);
        }

        public unsafe byte Find(FImage pattern, out Point location, Rectangle? searchArea = null, Rectangle? patternArea = null, byte proximity = 30, byte alphaThreshold = 100)
        {
            FImage pattern2 = pattern;
            if (patternArea.HasValue)
            {
                pattern2.SetPos(patternArea.Value.X, patternArea.Value.Y);
            }
            else
            {
                patternArea = new Rectangle(0, 0, pattern2.Width, pattern2.Height);
                pattern2.ResetPos();
            }
            searchArea = searchArea.HasValue
                ? new Rectangle(searchArea.Value.X, searchArea.Value.Y, Math.Min(searchArea.Value.Width, Width - searchArea.Value.X - patternArea.Value.Width), Math.Min(searchArea.Value.Height, Height - searchArea.Value.Y - patternArea.Value.Height))
                : new Rectangle(0, 0, Width - pattern2.Width, Height - pattern2.Height);
            if (searchArea.Value.Height <= 0 || searchArea.Value.Width <= 0)
            {
                location = default;
                return 0;
            }
            int num = searchArea.Value.Width * searchArea.Value.Height;
            int[] total = new int[num];
            int[] count = new int[num];
            Parallel.For(0, num, delegate (int pos)
            {
                int num5 = pos % searchArea.Value.Width;
                int num6 = pos / searchArea.Value.Width;
                byte* ptr = pattern2._rawData + patternArea.Value.Y * pattern2.Stride + patternArea.Value.X * pattern2.Bpp;
                byte* ptr2 = _rawData + (num6 + searchArea.Value.Y) * Stride + (num5 + searchArea.Value.X) * Bpp;
                int num7 = Stride - patternArea.Value.Width * Bpp;
                int num8 = pattern2.Stride - patternArea.Value.Width * pattern2.Bpp;
                for (int j = 0; j < patternArea.Value.Height; j++)
                {
                    for (int k = 0; k < patternArea.Value.Width; k++)
                    {
                        if (pattern2.Bpp == 3 || ptr[3] >= alphaThreshold)
                        {
                            if (Math.Abs(*ptr2 - *ptr) <= proximity && Math.Abs(ptr2[1] - ptr[1]) <= proximity && Math.Abs(ptr2[2] - ptr[2]) <= proximity)
                            {
                                total[pos]++;
                            }
                            count[pos]++;
                        }
                        ptr += pattern2.Bpp;
                        ptr2 += Bpp;
                    }
                    ptr += num8;
                    ptr2 += num7;
                }
            });
            int num2 = 0;
            int num3 = 0;
            for (int i = 0; i < num; i++)
            {
                if (count[i] != 0)
                {
                    int num4 = (total[i] << 8) / count[i];
                    if (num2 < num4)
                    {
                        num2 = num4;
                        num3 = i;
                    }
                }
            }
            location = new Point(searchArea.Value.X + num3 % searchArea.Value.Width, searchArea.Value.Y + num3 / searchArea.Value.Width);
            return (byte)Math.Min(255, num2);
        }

        public void ChangeWidth(int width)
        {
            _areaBox = Rectangle.Empty;
            Width = width;
        }

        public unsafe Rectangle GetColorAreaBox(Color color, byte proximity = 0, byte minAlpha = 200)
        {
            if (_areaBox != Rectangle.Empty && _areaBoxColor == color && _areaBoxProximity == proximity)
            {
                return _areaBox;
            }
            ResetPos();
            int num = Stride - Width * Bpp;
            bool flag = false;
            int i;
            int j;
            for (i = 0; i < Height; i++)
            {
                for (j = 0; j < Width; j++)
                {
                    GetPixel(out var r, out var g, out var b, out var a);
                    if (a >= minAlpha && Math.Abs(r - color.R) <= proximity && Math.Abs(g - color.G) <= proximity && Math.Abs(b - color.B) <= proximity)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num;
            }
            if (i == Height)
            {
                _areaBox = Rectangle.Empty;
                return _areaBox;
            }
            flag = false;
            SetPos(0, Height - 1);
            int num2;
            for (num2 = Height - 1; num2 > i; num2--)
            {
                for (j = 0; j < Width; j++)
                {
                    GetPixel(out var r2, out var g2, out var b2, out var a2);
                    if (a2 >= minAlpha && Math.Abs(r2 - color.R) <= proximity && Math.Abs(g2 - color.G) <= proximity && Math.Abs(b2 - color.B) <= proximity)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
                _ptr += num - (Stride << 1);
            }
            num2 -= i;
            flag = false;
            for (j = 0; j < Width; j++)
            {
                for (int k = i; k < i + num2; k++)
                {
                    GetPixel(j, k, out var r3, out var g3, out var b3, out var a3);
                    if (a3 >= minAlpha && Math.Abs(r3 - color.R) <= proximity && Math.Abs(g3 - color.G) <= proximity && Math.Abs(b3 - color.B) <= proximity)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            flag = false;
            int num3;
            for (num3 = Width - 1; num3 > j; num3--)
            {
                for (int l = i; l < i + num2; l++)
                {
                    GetPixel(num3, l, out var r4, out var g4, out var b4, out var a4);
                    if (a4 >= minAlpha && Math.Abs(r4 - color.R) <= proximity && Math.Abs(g4 - color.G) <= proximity && Math.Abs(b4 - color.B) <= proximity)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            num3 -= j;
            _areaBoxColor = color;
            _areaBoxProximity = proximity;
            _areaBox = new Rectangle(j, i, num3, num2);
            return _areaBox;
        }
    }
}
