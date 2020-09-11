using System;
using System.Collections.Generic;
using System.Text;
using FftSharp;

namespace SpectrogramAnalysis
{
    public class FFTs
    {
        /**
         * See: http://eeweb.poly.edu/iselesni/EL713/STFT/stft_inverse.pdf
         * FFts contains information about the Short-Term-Fourier-Transform representation of a signal
         * It is essentially the fourier transform of overlapping, windowed blocks of a signal
         * 
         * This allows us to obtain a fourier transform as a function of frequency AND time, S(w, t) instead of being solely dependent on frequency. 
         *      This representation of audio is most akin to how our ears actually interperet audio. They work by sampling the FFT of the sound it hears, which is a function of time
         *      This also allows us to perform signal processing on small windows of time.
         */
        private List<Complex[]> ffts;
        public double[] window { get; private set; }
        public int sampleRate { get; private set; }
        public int stepSize { get; private set; }

        private const int default_Fftsize = 1024;

        private const int default_overlap = 20; //stepSizes per Window length

        private double[] default_window = FftSharp.Window.Hanning(default_Fftsize);

        public FFTs(List<Complex[]> fft_list, double[] window, int sampleRate, int stepSize)
        {
            ffts = fft_list;
            this.window = window;
            this.sampleRate = sampleRate;
            this.stepSize = stepSize;
        }

        public double[] GetAudio()
        {
            return SpecAnalysis.ISTFT(ffts, stepSize, window);
        }

        public List<Complex[]> GetFFTs => ffts;

        //Produces a deep copy of the FFTs
        public List<Complex[]> DeepCopyFFTs()
        {
            List<Complex[]> newList = new List<Complex[]>();
            foreach (Complex[] fft in ffts)
            {
                Complex[] copy = new Complex[fft.Length];
                for (int i = 0; i < fft.Length; i++)
                    copy[i] = new Complex(fft[i].Real, fft[i].Imaginary);
            }
            return newList;
        }

        //Converts an audio stream into a list of FFTs 
        public FFTs(double[] audio, int sampleRate, int stepSize, double[] window)
        {
            this.sampleRate = sampleRate;
            this.stepSize = stepSize;
            this.window = window;
            this.ffts = SpecAnalysis.STFT(audio, stepSize, window);
            this.window = window;
        }

        //Uses default hanning window and default stepsize calculated from the default amount of overlapping
        public FFTs(double[] audio, int sampleRate)
        {
            this.sampleRate = sampleRate;
            this.window = FftSharp.Window.Hanning(default_Fftsize);
            this.stepSize = this.window.Length / default_overlap;
            this.ffts = SpecAnalysis.STFT(audio, stepSize, window);
        }

        public FFTs(List<Complex[]> ffts, int sampleRate, int stepSize, double[] window)
        {
            this.ffts = ffts;
            this.sampleRate = sampleRate;
            this.stepSize = stepSize;
            this.window = window;
        }
    }
}
