using FftSharp;
using SpecPlus.Design;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace AudioAnalysis
{

    public static class Processing
    {

        public static void AddGain(FFTs stft, double gain = 0, SelectedWindowIndices indices = null, bool dB = true)
        {
            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);

            //Defaults to dB
            //Converts to linear gain for multiplying gain to data
            gain = dB ? Math.Pow(10, gain/20) : gain;

            List<Complex[]> data = stft.GetFFTs();
            for (int n = timeIndex1; n < timeIndex2; n++)
            {
                for (int k = freqIndex1; k < freqIndex2; k++)
                {
                    data[n][k].Real *= gain;
                    data[n][k].Imaginary *= gain;
                }
            }
        }

        public static void FrequencyShifter(FFTs stft, int freq_shift, SelectedWindowIndices indices = null, int order=0, double thresh=0.9)
        {
            int indexShift = freq_shift / stft.FreqResolution;
            if (indexShift == 0)
                return;

            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);


            //Shift map tells how far each index is going to shift
            //Order of 0 results in a linear shifter
            //shift_mod gives a scaling factor to each shifted index
            int[] shift_map = createFreqShiftMap(stft, indexShift, indices, order, thresh);
            double[] shift_mod = createFreqShiftModulation(stft, indexShift, indices);


            List<Complex[]> ffts = stft.GetFFTs();
            for(int n = timeIndex1; n < timeIndex2; n++)
            {
                Complex[] fft = ffts[n];
                Complex[] shifted_fft = new Complex[fft.Length];
                for (int k = 0; k < fft.Length; k++)
                    if(k + shift_map[k] >= 0 && k + shift_map[k] < fft.Length) //Ignore the ones that get shifted too far
                        shifted_fft[k + shift_map[k]] += fft[k]*shift_mod[k];
                Array.Copy(shifted_fft, fft, fft.Length);
            }
        }

        private static int[] createFreqShiftMap(FFTs stft, int indexShift, SelectedWindowIndices indices = null, int order = 0, double thresh = 0.9)
        {
            (_, _, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);

            /**
             * Explanation of algorithm: 
             * We basically want one half cycle of a cos(ang_freq * (centerIndex-i))^(order) 
             * First we calculation the attenuation we want at the window boundaries, ie
             *      cos(ang_freq*(centerIndex-freq1))^(order) = thresh
             *      Then we solve for ang_freq to find the angular frequency of the window
             *      Then we use that angular frequency for arbitrary values of i
             *      
             * And then, depending on the shift direction, we don't care about shifts on the opposite side of the direction
             * 
             * If order is zero, it results in a Linear Frequency Shifter
             */
            int centerIndex = (freqIndex2 + freqIndex1) / 2;
            int[] shift_map = new int[stft.fftSize];
            for (int i = 0; i < shift_map.Length / 2; i++)
            {
                double angular_freq = Math.Acos(Math.Pow(thresh, (1d / ((double)order)))) / (centerIndex - freqIndex1);
                double angle = angular_freq * (i - centerIndex);
                double mod = Math.Pow(Math.Cos(angle), order);

                //Don't care about values outside of the half period window
                if (Math.Abs(angle) >= Math.PI / 2) 
                    mod = 0;

                //Don't care about values past the window in the direction it's not shifting
                else if (indexShift > 0 && i < freqIndex1) 
                    mod = 0;
                else if(indexShift < 0 && i > freqIndex2)
                    mod = 0;
                
                int shift = (int)(indexShift * mod);
                if (i >= freqIndex1 && i < freqIndex2)
                {
                    shift_map[i] = indexShift; //first side
                    shift_map[shift_map.Length - i - 1] = -indexShift; //mirrored side
                }
                else
                {
                    shift_map[i] = shift;
                }
            }


            return shift_map;
        }

        private static int getNextBandIndex(Complex[] fft, int currentPosition, bool increasing = true)
        {
            /**
             *  Returns -1 if there is no band beyond the current one
             */

            int threshold = 10; //todo: completely arbitary fix later

            if (increasing)
            {
                for (int i = currentPosition; i < fft.Length; i++)
                    if (fft[i].Magnitude > threshold)
                        return i;
            }
            else
            {
                for (int i = currentPosition; i >= 0; i--)
                    if (fft[i].Magnitude > threshold)
                        return i;
            }

            return -1;
        }

        private static int[] createRecursiveBandShiftMap(FFTs stft, int indexShift, SelectedWindowIndices indices = null)
        {
            throw new NotImplementedException();
        }

        private static double[] createFreqShiftModulation(FFTs stft, int shiftIndex, SelectedWindowIndices indices, bool freqDependent = false)
        {
            if (freqDependent) ; //todo, implement createFreqDependentShiftMap first

            double[] shift_mod = new double[stft.fftSize];
            Array.Fill(shift_mod, 1);
            return shift_mod;
        }


    }
}