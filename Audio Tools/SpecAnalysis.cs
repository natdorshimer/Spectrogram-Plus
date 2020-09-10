using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FftSharp;
using System.Windows.Threading;
using System.Threading.Tasks;
using Spectrogram;
using NAudio.Wave;
using Microsoft.VisualBasic.CompilerServices;

/**
 * Audio processing tools used for processing of voice. 
 * The heart lies at ISTFT, which allows me to convert from the Fourier Domain back into the Time Domain
 */
namespace SpectrogramAnalysisTools
{
    public class SpecAnalysis
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
            for (int i = 0; i < audio.Length; i++) audio[i] = (float) audioD[i];

            using WaveFileWriter writer = new NAudio.Wave.WaveFileWriter(filename, new WaveFormat(sampleRate, 1));
            writer.WriteSamples(audio, 0, audio.Length);
        }


        /// <param name="ffts">List of ffts with a window block separation of stepSize</param>
        /// <param name="stepSize">The step size the window took when constructing the ffts</param>
        /// <param name="window">Window used to create the ffts originally. Need to ISTFT them back.</param>
        public static double[] ISTFT(List<Complex[]> ffts, int stepSize, double[] window)
        {
            /**
             * Inverse Short Term Fourier Transform
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
             * TODO: The playbacked quality isn't as good as it could be
             * TODO: Implement testing to assert that the deviated quality is within standards
             */

            double[] data = new double[window.Length + ffts.Count * stepSize];
            for(int windowed_block = 0; windowed_block < ffts.Count; windowed_block++)
            {
                Complex[] buffer = ffts[windowed_block];
                Transform.IFFT(buffer); //Get the inverse fourier transform of the buffer
                int data_index = windowed_block * stepSize;
                for (int j = 0; j < buffer.Length; j++)
                    data[data_index + j] += buffer[j].Real * window[j] / data.Length; 
            }
            
            return data;
        }

    }

}