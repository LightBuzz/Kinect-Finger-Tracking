using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectFingerTracking
{
    internal class InfraredBitmapGenerator
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public ushort[] InfraredData { get; protected set; }

        public byte[] Pixels { get; protected set; }

        public WriteableBitmap Bitmap { get; protected set; }

        public void Update(InfraredFrame frame)
        {
            if (Bitmap == null)
            {
                Width = frame.FrameDescription.Width;
                Height = frame.FrameDescription.Height;
                InfraredData = new ushort[Width * Height];
                Pixels = new byte[Width * Height * 4];
                Bitmap = new WriteableBitmap(Width, Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            }

            frame.CopyFrameDataToArray(InfraredData);
            
            int colorIndex = 0;

            for (int infraredIndex = 0; infraredIndex < InfraredData.Length; infraredIndex++)
            {
                ushort ir = InfraredData[infraredIndex];
                
                byte intensity = (byte)(ir >> 6);

                Pixels[colorIndex++] = intensity; // Blue
                Pixels[colorIndex++] = intensity; // Green   
                Pixels[colorIndex++] = intensity; // Red
                
                colorIndex++;
            }

            Bitmap.Lock();

            Marshal.Copy(Pixels, 0, Bitmap.BackBuffer, Pixels.Length);
            Bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

            Bitmap.Unlock();
        }
    }


    internal static class BitmapExtensions
    {
        private static InfraredBitmapGenerator _bitmapGenerator = new InfraredBitmapGenerator();

        public static WriteableBitmap ToBitmap(this InfraredFrame frame)
        {
            _bitmapGenerator.Update(frame);

            return _bitmapGenerator.Bitmap;
        }
    }
}
