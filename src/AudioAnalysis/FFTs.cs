using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FftSharp;
using NAudio.Wave;

namespace AudioAnalysis
{
    public class FFTs
    {
        /**
         * See: http://eeweb.poly.edu/iselesni/EL713/STFT/stft_inverse.pdf
         * FFts contains information about the Short-Time-Fourier-Transform representation of a signal
         * It is essentially the fourier transform of overlapping, windowed blocks of a signal
         * 
         * This allows us to obtain a fourier transform as a function of frequency AND time, S(w, t) instead of being solely dependent on frequency. 
         *      This representation of audio is most akin to how our ears actually interperet audio. They work by sampling the FFT of the sound it hears, which is a function of time
         *      This also allows us to easily perform signal processing on small windows of time.
         */

        private List<Complex[]> ffts;
        public double[] window { get; private set; }
        public int sampleRate { get; private set; }
        public int stepSize { get; private set; }
        public int FreqNyquist => sampleRate / 2;
        public int nyquistBin => fftSize / 2;
        public int fftSize => window.Length;
        public int FreqResolution => sampleRate / window.Length;
        public int Count => ffts.Count;

        private const int default_Fftsize = 1024;
        private const int default_overlap = 20; //stepSizes per Window length
        private double[] default_window = Fourier.Window.RootHann(default_Fftsize);



        public FFTs(double[] audio, int sampleRate, int stepSize, double[] window)
        {
            this.sampleRate = sampleRate;
            this.stepSize = stepSize;
            this.window = window;
            this.ffts = Fourier.STFT(audio, stepSize, window);
            this.window = window;
        }
        public FFTs(double[] audio, int sampleRate)
        {
            this.sampleRate = sampleRate;
            this.window = Fourier.Window.RootHann(default_Fftsize);
            this.stepSize = this.window.Length / default_overlap;
            this.ffts = Fourier.STFT(audio, stepSize, window);
        }

        public FFTs(List<Complex[]> ffts, int sampleRate, int stepSize, double[] window)
        {
            this.ffts = ffts;
            this.sampleRate = sampleRate;
            this.stepSize = stepSize;
            this.window = window;
        }

        public List<Complex[]> GetFFTs() => ffts;

        public double[] GetAudioDouble()
        {
            List<Complex[]> ffts_copy = CopyFFTs();
            return Fourier.ISTFT(ffts_copy, stepSize, window);
        }

        public float[] GetAudioFloat()
        {
            double[] audioD = GetAudioDouble();
            float[] audioF = new float[audioD.Length];
            for (int i = 0; i < audioD.Length; i++) audioF[i] = (float)audioD[i];
            return audioF;
        }

        public List<Complex[]> CopyFFTs() => CopyFFTs(0, ffts.Count);

        public List<Complex[]> CopyFFTs(int startIndex, int count)
        {
            List<Complex[]> newList = new List<Complex[]>();
            for(int i = startIndex; i < startIndex+count; i++)
            {
                Complex[] copy = new Complex[ffts[i].Length];
                Array.Copy(ffts[i], copy, ffts[i].Length); //Complex is a struct and passed by value
                newList.Add(copy);
            }
            return newList;
        }


        public FFTs Copy() => Copy(0, ffts.Count);

        public FFTs Copy(int startIndex, int count)
        {
            //Deep copy
            return new FFTs(CopyFFTs(startIndex, count), sampleRate, stepSize, window);
        }

        public void SaveToWav(string filename)
        {
            float[] audio = GetAudioFloat();
            using WaveFileWriter writer = new NAudio.Wave.WaveFileWriter(filename, new WaveFormat(sampleRate, 1));
            writer.WriteSamples(audio, 0, audio.Length);
        }

        public void SaveSnippet(string filename, int startIndex, int endIndex)
        {
            List<Complex[]> snippet = CopyFFTs(startIndex, endIndex - startIndex);
            FFTs stft_snippet = new FFTs(snippet, sampleRate, stepSize, window);
            stft_snippet.SaveToWav(filename);
        }


        private void Mirror()
        {
            /**
             * The purpose of Mirror is to reproduce the first half of the FFT into the second half since it's a real valued signal
             * Ideally would allow me to only modify the single side band, mirror it, and then perfrom an ISTFT instead of having
             * to modify both halves of the STFT before doing ISTFT. 
             * 
             * TODO: The Mirror'ed quality is kind of roboty
             */
            foreach (Complex[] fft in ffts)
                for (int i = 0; i < nyquistBin; i++)
                    fft[fft.Length - 1 - i] = fft[i];

        }

    }
}
