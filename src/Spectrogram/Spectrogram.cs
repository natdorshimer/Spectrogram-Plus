using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using FftSharp;
using AudioAnalysis;
using NAudio.Wave;

namespace Spectrogram
{

    public class Spectrogram
    {
        public int Width { get { return ffts.Count; } }
        public int Height { get { return settings.Height; } }
        public int FftSize { get { return settings.FftSize; } }
        public double HzPerPx { get { return settings.HzPerPixel; } }
        public double SecPerPx { get { return settings.StepLengthSec; } }
        public int FftsToProcess { get { return (newAudio.Count - settings.FftSize) / settings.StepSize; } }
        public int FftsProcessed { get; private set; }
        public int NextColumnIndex { get { return (FftsProcessed + rollOffset) % Width; } }
        public int OffsetHz { get { return settings.OffsetHz; } set { settings.OffsetHz = value; } }
        public int SampleRate { get { return settings.SampleRate; } }
        public int StepSize { get { return settings.StepSize; } }
        public double FreqMax { get { return settings.FreqMax; } }
        public int NyquistBin => FftSize / 2;
        public int FreqNyquist => SampleRate / 2;
        public double FreqMin { get { return settings.FreqMin; } }

        private readonly Settings settings;
        private List<Complex[]> ffts;
        private List<FftSharp.Complex[]> ffts_complex;
        private readonly List<double> newAudio = new List<double>();
        private Colormap cmap = Colormap.Viridis;


        public Spectrogram(int sampleRate, int fftSize, int stepSize,
            double minFreq = 0, double maxFreq = double.PositiveInfinity,
            int? fixedWidth = null, int offsetHz = 0)
        {
            ffts = new List<Complex[]>();
            ffts_complex = new List<FftSharp.Complex[]>();
            this.SetColormap(Colormap.Viridis);
            settings = new Settings(sampleRate, fftSize, stepSize, minFreq, maxFreq, offsetHz);
            
            if (fixedWidth.HasValue)
                SetFixedWidth(fixedWidth.Value);
        }

        public override string ToString()
        {
            double processedSamples = ffts.Count * settings.StepSize + settings.FftSize;
            double processedSec = processedSamples / settings.SampleRate;
            string processedTime = (processedSec < 60) ? $"{processedSec:N2} sec" : $"{processedSec / 60.0:N2} min";

            return $"Spectrogram ({Width}, {Height})" +
                   $"\n  Vertical ({Height} px): " +
                   $"{settings.FreqMin:N0} - {settings.FreqMax:N0} Hz, " +
                   $"FFT size: {settings.FftSize:N0} samples, " +
                   $"{settings.HzPerPixel:N2} Hz/px" +
                   $"\n  Horizontal ({Width} px): " +
                   $"{processedTime}, " +
                   $"window: {settings.FftLengthSec:N2} sec, " +
                   $"step: {settings.StepLengthSec:N2} sec, " +
                   $"overlap: {settings.StepOverlapFrac * 100:N0}%";
        }



        public void SetColormap(Colormap cmap)
        {
            this.cmap = cmap ?? this.cmap;
        }

        public double[] GetWindow(){ return settings.Window; }

        public void SetWindow(double[] newWindow)
        {
            if (newWindow.Length > settings.FftSize)
                throw new ArgumentException("window length cannot exceed FFT size");

            for (int i = 0; i < settings.FftSize; i++)
                settings.Window[i] = 0;

            int offset = (settings.FftSize - newWindow.Length) / 2;
            Array.Copy(newWindow, 0, settings.Window, offset, newWindow.Length);
        }


        public void Add(double[] audio, bool process = true)
        {
            newAudio.AddRange(audio);
            if (process)
                Process();
        }

        private int rollOffset = 0;

        public void RollReset(int offset = 0)
        {
            rollOffset = -FftsProcessed + offset;
        }


        //Modified to be equivalent to Fourier.STFT code.
        public Complex[][] Process()
        {
            if (FftsToProcess < 1)
                return null;
            int newFftCount = FftsToProcess;

            Complex[][] newFfts = new Complex[newFftCount][];
            Parallel.For(0, newFftCount, newFftIndex =>
            {
                FftSharp.Complex[] buffer = new FftSharp.Complex[settings.FftSize];
                int sourceIndex = newFftIndex * settings.StepSize;
                for (int i = 0; i < settings.FftSize; i++)
                    buffer[i].Real = newAudio[sourceIndex + i] * settings.Window[i] / settings.FftSize;

                FftSharp.Transform.FFT(buffer);
                newFfts[newFftIndex] = new Complex[settings.FftSize];
                for (int i = 0; i < settings.FftSize; i++) {
                    newFfts[newFftIndex][i] = buffer[i];
                }
            });



            for (int i = 0; i < newFftCount; i++)
            {
                ffts.Add(newFfts[i]);
                ffts_complex.Add(newFfts[i]);
            }
            
            FftsProcessed += newFfts.Length;
            newAudio.RemoveRange(0, newFftCount * settings.StepSize);

            PadOrTrimForFixedWidth();
            return newFfts;
        }


        public List<Complex[]> GetMelFFTs(int melBinCount)
        {
            if (settings.FreqMin != 0)
                throw new InvalidOperationException("cannot get Mel spectrogram unless minimum frequency is 0Hz");

            
            var fftsMel = new List<Complex[]>();

            /**
            foreach(var fft in ffts)
                fftsMel.Add(FftSharp.Transform.MelScale(fft, SampleRate, melBinCount));
            **/
            throw new NotImplementedException("Only using Complex[] ffts and not double[] ffts which currently isn't supported");

            //return fftsMel;
        }

   
        public BitmapSource GetBitmapSource(double intensity = 1, bool dB = false, bool roll = false, double whiteNoiseMin=0) =>
            Image.GetBitmapSource(ffts, cmap, settings.SampleRate, intensity, dB, roll, NextColumnIndex, whiteNoiseMin);


        public BitmapSource GetBitmapSourceMel(int melBinCount = 25, double intensity = 1, bool dB = false, bool roll = false) =>
            Image.GetBitmapSource(GetMelFFTs(melBinCount), cmap, settings.SampleRate, intensity, dB, roll, NextColumnIndex);


        /**
         * sTODO: Requires testing
         */
        public void SaveImage(string fileName, double intensity = 1, bool dB = false, bool roll = false)
        {
            if (ffts.Count == 0)
                throw new InvalidOperationException("Spectrogram contains no data. Use Add() to add signal data.");

            string extension = Path.GetExtension(fileName).ToLower();

            BitmapEncoder encoder;
            if (extension == ".bmp")
                encoder = new BmpBitmapEncoder();
            else if (extension == ".png")
                encoder = new PngBitmapEncoder();
            else if (extension == ".jpg" || extension == ".jpeg")
                encoder = new JpegBitmapEncoder();
            else if (extension == ".gif")
                encoder = new GifBitmapEncoder();
            else
                throw new ArgumentException("unknown file extension");


            BitmapSource image = Image.GetBitmapSource(ffts, cmap, settings.SampleRate, intensity, dB, roll, NextColumnIndex);
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public BitmapSource GetBitmapMax(double intensity = 1, bool dB = false, bool roll = false, int reduction = 4)
        {
            List<Complex[]> ffts2 = new List<Complex[]>();
            for (int i = 0; i < ffts.Count; i++)
            {
                Complex[] d1 = ffts[i];
                Complex[] d2 = new Complex[d1.Length / reduction];
                for (int j = 0; j < d2.Length; j++)
                    for (int k = 0; k < reduction; k++)
                        d2[j] = d2[j].Magnitude > d1[j * reduction + k].Magnitude ? d2[j] : d1[j * reduction + k];
                ffts2.Add(d2);
            }

            return Image.GetBitmapSource(ffts2, cmap, settings.SampleRate, intensity, dB, roll, NextColumnIndex);
        }

        
        public void SaveData(string filePath, int melBinCount = 0)
        {
            throw new NotImplementedException("If using, will implement a method to store Complex data and not just SFF");
        }

        private int fixedWidth = 0;
        public void SetFixedWidth(int width)
        {
            fixedWidth = width;
            PadOrTrimForFixedWidth();
        }

        private void PadOrTrimForFixedWidth()
        {
            if (fixedWidth > 0)
            {
                int overhang = Width - fixedWidth;
                if (overhang > 0) {
                    ffts.RemoveRange(0, overhang);
                    ffts_complex.RemoveRange(0, overhang);
                }

                while (ffts.Count < fixedWidth) {
                    ffts.Insert(0, new Complex[settings.FftSize]);
                    ffts_complex.Insert(0, new FftSharp.Complex[settings.FftSize]);
                }
            }
        }

        /**
         * TODO: This still needs to be converted to WPF
         */
        public BitmapSource GetVerticalScale(int width, int offsetHz = 0, int tickSize = 3, int reduction = 1)
        {
            throw new NotImplementedException();
            //return Scale.Vertical(width, settings, offsetHz, tickSize, reduction);
        }

        public int PixelY(double frequency, int reduction = 1)
        {
            int pixelsFromZeroHz = (int)(settings.PxPerHz * frequency / reduction);
            int pixelsFromMinFreq = pixelsFromZeroHz - settings.FftIndex1 / reduction + 1;
            int pixelRow = settings.Height / reduction - 1 - pixelsFromMinFreq;
            return pixelRow - 1;
        }

        public List<Complex[]> GetFFTs()
        {
            return ffts;
        }

        public List<Complex[]> CopyFFTs()
        {
            List<Complex[]> copy = new List<Complex[]>();
            foreach (Complex[] fft in ffts) 
            {
                Complex[] fft_copy = new Complex[fft.Length];
                for (int i = 0; i < fft.Length; i++)
                    fft_copy[i] = new Complex(fft[i].Real, fft[i].Imaginary);
                copy.Add(fft_copy);
            }
            return copy;
        }

        
        public (double freqHz, double magRms) GetPeak(bool latestFft = true)
        {
            if (ffts.Count == 0)
                return (double.NaN, double.NaN);

            if (latestFft == false)
                throw new NotImplementedException("peak of mean of all FFTs not yet supported");

            Complex[] freqs = ffts[ffts.Count - 1];

            int peakIndex = 0;
            double peakMagnitude = 0;
            for (int i = 0; i < freqs.Length; i++)
            {
                if (freqs[i].Magnitude > peakMagnitude)
                {
                    peakMagnitude = freqs[i].Magnitude;
                    peakIndex = i;
                }
            }

            double maxFreq = SampleRate / 2;
            double peakFreqFrac = peakIndex / (double)freqs.Length;
            double peakFreqHz = maxFreq * peakFreqFrac;

            return (peakFreqHz, peakMagnitude);
        }
    }
    
}
