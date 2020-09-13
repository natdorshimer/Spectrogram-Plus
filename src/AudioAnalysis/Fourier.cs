using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FftSharp;
using NAudio.Wave;
using Microsoft.VisualBasic.CompilerServices;
using System.Windows;
using System.Threading.Tasks;

namespace AudioAnalysis
{

    /**
    * Audio processing tools used for processing of voice. 
    * The heart lies at ISTFT, which allows me to convert from the Fourier Domain back into the Time Domain after doing filtering
    */
    public static class Fourier
    {

        public static void SaveFFTsToWav(string filename, List<Complex[]> ffts, int sampleRate, int stepSize, double[] window)
        {
            //Deep copy the complex ffts into a new buffer to safely transform
            List<Complex[]> buffers = new List<Complex[]>();
            foreach (Complex[] arr in ffts)
            {
                Complex[] arr_copy = new Complex[arr.Length];
                Array.Copy(arr, 0, arr_copy, 0, arr.Length);
                buffers.Add(arr_copy);
            }

            double[] audioD = ISTFT(ffts, stepSize, window);
            float[] audio = new float[audioD.Length];
            for (int i = 0; i < audio.Length; i++) audio[i] = (float)audioD[i];

            using WaveFileWriter writer = new NAudio.Wave.WaveFileWriter(filename, new WaveFormat(sampleRate, 1));
            writer.WriteSamples(audio, 0, audio.Length);
        }

        public static void SaveFFTsToWav(string filename, FFTs stft)
        {
            float[] audio = stft.GetAudioFloat();
            using WaveFileWriter writer = new NAudio.Wave.WaveFileWriter(filename, new WaveFormat(stft.sampleRate, 1));
            writer.WriteSamples(audio, 0, audio.Length);
        }


        /// <param name="ffts">List of ffts with a window block separation of stepSize</param>
        /// <param name="stepSize">The step size the window took when constructing the ffts</param>
        /// <param name="window">Window used to create the ffts originally. Needed to ISTFT them back.</param>
        public static double[] ISTFT(List<Complex[]> ffts, int stepSize, double[] window)
        {
            /**
             * Inverse Short Time Fourier Transform
             * See: http://eeweb.poly.edu/iselesni/EL713/STFT/stft_inverse.pdf
             * 
             * In our case, the window length N is also the size of each ffts array.
             * The step size is also not N/2, the step size is whatever the user sets it to be. 
             * 
             * Below is an example where the step size is 3 with a window size (FftSize) of FftSize
             *
             * ffts[1] = [1,2,3...FftSize] + 
             * ffts[2] =       [4,5,6...FftSize+3] + 
             * ffts[3] =           [7,8,9...FftSize+6] +
             *                       
             * data =    [1,2,3,........................FftSize+bufferCount*stepSize]
             * 
             * So the length of the data array will be FftSize + ffts.Count*stepSize
             * 
             * 1. Modulate each Complex[] fft array with the window 
             * 2. Summate each array into one final double array
             * 3. The length of the data array will the total number of elements in ffts divided by the stepsize, ffts.Count*FftSize/stepSize
             * 
             * 
             * TODO: The playback quality isn't as good as it could be
             * TODO: Implement testing to assert that the deviated quality is within standards
             */

            double[] data = new double[window.Length + ffts.Count * stepSize];
            for (int windowed_block = 0; windowed_block < ffts.Count; windowed_block++)
            {
                Complex[] buffer = ffts[windowed_block];
                Transform.IFFT(buffer); 
                int data_index = windowed_block * stepSize;
                for (int j = 0; j < buffer.Length; j++)
                    data[data_index + j] += buffer[j].Real * window[j] / window.Length * 2 ;
            }

            return data;
        }

        public static List<Complex[]> STFT(double[] audio, int stepSize, double[] window)
        {
            /**
             * Short Time Fourier Transform 
             * We do normalization (1/N) on the inverse transform instead of the forward transform. 
             */


            int num_blocks = (audio.Length - window.Length) / stepSize;

            //window too large or stepSize too large to get a single windowed block out of it
            if (num_blocks < 1)
                return null; 

            List<Complex[]> ffts = new List<Complex[]>();

            Parallel.For(0, num_blocks, windowed_block =>
            {
                FftSharp.Complex[] buffer = new FftSharp.Complex[window.Length];
                int sourceIndex = windowed_block * stepSize;
                for (int i = 0; i < window.Length; i++)
                    buffer[i].Real = audio[sourceIndex + i] * window[i];

                FftSharp.Transform.FFT(buffer);
                ffts.Add(buffer);
            });

            return ffts;
        }

        public static class Window
        {
            public static double[] RootHann(int windowSize)
            {
                double[] window = FftSharp.Window.Hanning(windowSize);
                for (int i = 0; i < windowSize; i++)
                    window[i] = Math.Sin((i + 0.5) * Math.PI / windowSize);
                return window;
            }

            public static double[] Hann(int windowSize)
            {
                double[] window = FftSharp.Window.Hanning(windowSize);
                for (int i = 0; i < windowSize; i++)
                    window[i] = Math.Pow(Math.Sin((i + 0.5) * Math.PI / windowSize), 2);
                return window;
            }
        }

    }

}