using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spectrogram
{
    public static class Image
    {
        //For use in WPF
        public static BitmapSource GetBitmapSource(IList<FftSharp.Complex[]> ffts, Colormap cmap, int sampleRate, double intensity = 1, bool dB = false, bool roll = false, int rollOffset = 0, double whiteNoiseMin = 0)
        {
            int resolution = sampleRate / ffts[0].Length;
            int maxFreq = 7000;
            int maxBin = maxFreq / resolution;
            if (ffts.Count == 0)
                throw new ArgumentException("This Spectrogram contains no FFTs (likely because no signal was added)");


            int Width = ffts.Count;
            int Height = Math.Min(maxBin, ffts[0].Length/2); //No point in showing beyond nyquist frequency
           

            var pixelFormat = System.Windows.Media.PixelFormats.Indexed8;
            WriteableBitmap bit = new WriteableBitmap(Width, Height, 96, 96, pixelFormat, cmap.GetBitmapPalette());
            
            //Lock the bits to make sure that no one can read them while we are writing to its memory
            //Then we use a pointer to the backbuffer to write to the memory of the bitmap directly to avoid having to create an array and copy into it
            bit.Lock();
            unsafe
            {
                int stride = bit.BackBufferStride;
                int bytesPerPixel = pixelFormat.BitsPerPixel / 8;
                byte* bytes = (byte*)bit.BackBuffer; //Pointer to the raw color data of the bitmap
                Parallel.For(0, Width, col =>
                {
                    int sourceCol = col;
                    if (roll)
                    {
                        sourceCol += Width - rollOffset % Width;
                        if (sourceCol >= Width)
                            sourceCol -= Width;
                    }

                    for (int row = 0; row < Height; row++)
                    {
                        double value = ffts[sourceCol][row].Magnitude;
                        if (value <= whiteNoiseMin)
                            value = 0;
                        if (dB)
                            value = 20 * Math.Log10(value + 1);
                        value *= intensity;
                        value = Math.Min(value, 255);
                        int bytePosition = (Height - 1 - row) * stride + col*bytesPerPixel;
                        bytes[bytePosition] = (byte)value;
                    }
                });
            }

            bit.AddDirtyRect(new Int32Rect(0, 0, Width, Height)); //Specifies the region of the bitmap that has changed so that it can update the image
            bit.Unlock();
            return bit;

        }


        //For use in Windows Forms
        public static Bitmap GetBitmap(IList<double[]> ffts, Colormap cmap, double intensity = 1, bool dB = false, bool roll = false, int rollOffset = 0)
        {
            if (ffts.Count == 0)
                throw new ArgumentException("This Spectrogram contains no FFTs (likely because no signal was added)");

            int Width = ffts.Count;
            int Height = ffts[0].Length;

            var pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

            Bitmap bmp = new Bitmap(Width, Height, pixelFormat);
            cmap.Apply(bmp);

            var lockRect = new Rectangle(0, 0, Width, Height);
            BitmapData bitmapData = bmp.LockBits(lockRect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                int stride = bitmapData.Stride;
                byte* bytes = (byte*)bitmapData.Scan0; //Pointer to the raw color data of the bitmap
                Parallel.For(0, Width, col =>
                {
                    int sourceCol = col;
                    if (roll)
                    {
                        sourceCol += Width - rollOffset % Width;
                        if (sourceCol >= Width)
                            sourceCol -= Width;
                    }

                    for (int row = 0; row < Height; row++)
                    {
                        double value = ffts[sourceCol][row];
                        if (dB)
                            value = 20 * Math.Log10(value + 1);
                        value *= intensity;
                        value = Math.Min(value, 255);
                        int bytePosition = (Height - 1 - row) * stride + col;
                        bytes[bytePosition] = (byte)value;
                    }
                });
            }

            bmp.UnlockBits(bitmapData);
            return bmp;
        }
    }
}
