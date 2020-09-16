﻿using System;
using System.Collections.Generic;
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
            for(int i = 0; i < audioF.Length; i++) audioF[i] = (float)audioD[i];

            return audioF;
        }


        //TODO: Consider a better / more "standard" cloning solution
        public List<Complex[]> CopyFFTs()
        {
            List<Complex[]> newList = new List<Complex[]>();
            foreach (Complex[] fft in ffts)
            {
                Complex[] copy = new Complex[fft.Length];
                for (int i = 0; i < fft.Length; i++)
                    copy[i] = new Complex(fft[i].Real, fft[i].Imaginary);
                newList.Add(copy);
            }
            return newList;
        }

        public void SaveToWav(string filename)
        {
            float[] audio = GetAudioFloat();
            using WaveFileWriter writer = new NAudio.Wave.WaveFileWriter(filename, new WaveFormat(sampleRate, 1));
            writer.WriteSamples(audio, 0, audio.Length);
        }

        public FFTs Copy()
        {
            /**
             * Produces a deep copy
             */
            return new FFTs(CopyFFTs(), sampleRate, stepSize, window);
        }

        public FFTs TimeSlice(int timeIndex1, int timeIndex2)
        {
            List<Complex[]> slice = new List<Complex[]>();
            for (int n = timeIndex1; n < timeIndex2; n++)
                slice.Add(ffts[n]);
            return new FFTs(slice, sampleRate, stepSize, window);
        }
        /**
        public FFTs Index(int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2)
        {
            /**
             * Returns a smaller snapshot of the FFTs contained within those indices
             
            List<Complex[]> slice = new List<Complex[]>();
            for(int t = timeIndex1; t < timeIndex2; t++)
            {
                Complex[] time_slice = new Complex[freqIndex2 - freqIndex1];
                for (int f = freqIndex1; f < freqIndex2; f++)
                    time_slice[f - freqIndex1] = ffts[t][f];
                slice.Add(time_slice);
            }

            return new FFTs(slice, sampleRate, stepSize, window);
        }*/

        public int Count => ffts.Count;

        public void SaveSnippet(string filename, int startIndex, int endIndex)
        {
            List<Complex[]> copy = CopyFFTs();
            List<Complex[]> snippet = new List<Complex[]>();
            for (int i = startIndex; i <= endIndex; i++)
                snippet.Add(copy[i]);
            FFTs stft_snippet = new FFTs(snippet, sampleRate, stepSize, window);
            stft_snippet.SaveToWav(filename);
        }
        /**
         * The purpose of Mirror is to reproduce the first half of the FFT into the second half since it's a real valued signal
         * Ideally would allow me to only modify the single side band, mirror it, and then perfrom an ISTFT instead of having
         * to modify both halves of the STFT before doing ISTFT. 
         * 
         * TODO: The Mirror'ed quality is kind of roboty
         */
        private void Mirror()
        {
            foreach (Complex[] fft in ffts)
            {
                for (int i = 0; i < nyquistBin; i++)
                {
                    Complex val = fft[i];
                    fft[fft.Length - 1 - i].Real = val.Real;
                    fft[fft.Length - 1 - i].Imaginary = -val.Imaginary;
                }
            }
        }

    }
}
