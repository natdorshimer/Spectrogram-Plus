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

        public static void FrequencyShifter(FFTs stft, int freq_shift, SelectedWindowIndices indices = null, bool isFreqDependent= false)
        {
            int indexShift = freq_shift / stft.FreqResolution;
            if (indexShift == 0)
                return;

            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);


            //Shift map tells how far each index is going to shift
            //shift_mod gives a scaling factor to each shifted index
            int[] shift_map = createFrequencyShiftMap(stft, indexShift, indices, isFreqDependent);
            double[] shift_mod = createFreqShiftModulation(stft, indexShift, indices, isFreqDependent);


            List<Complex[]> ffts = stft.GetFFTs();
            for(int n = timeIndex1; n < timeIndex2; n++)
            {
                Complex[] fft = ffts[n];
                Complex[] shifted_fft = new Complex[fft.Length];
                Array.Fill(shifted_fft, new Complex(0, 0));
                for (int k = 0; k < fft.Length; k++)
                    if(k + shift_map[k] >= 0 && k + shift_map[k] < fft.Length) //Ignore the ones that get shifted too far
                        shifted_fft[k + shift_map[k]] += fft[k]*shift_mod[k];

                Array.Copy(shifted_fft, fft, fft.Length);
            }
        }

        private static int[] createFrequencyShiftMap(FFTs stft, int indexShift, SelectedWindowIndices indices = null, bool freqDependent = false)
        {
            /**
             * Maps each FFT index to how many indices it is going to shift. 
             * out[5]=2 says that the 5th index of the FFT will shift 2 indices upwards
             */
            if (freqDependent)
                return createFreqDependentShiftMap(stft, indexShift, indices);
            return createLinearShiftMap(stft, indexShift, indices);
        }

        private static int[] createLinearShiftMap(FFTs stft, int indexShift, SelectedWindowIndices indices = null)
        {
            /**
             * Creates a shift map that corresponds to how much index k is supposed to shift
             * linear shift map applies the same shift to every value (or just the values in the window)
             * There will be clipping if you shift it into frequency bands that have values, hence a smarter frequency dependent shifter
             * 
             * This also provides mapping to the mirrored side which removes complexity on having to handle it in other processes
             */

            (_, _, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);

            int[] shift_map = new int[stft.fftSize];
            for (int i = 0; i < shift_map.Length / 2; i++)
            {
                if (i >= freqIndex1 && i < freqIndex2)
                {
                    shift_map[i] = indexShift; //first side
                    shift_map[shift_map.Length - i - 1] = -indexShift; //mirrored side
                }
                else
                    shift_map[i] = 0;
            }
            return shift_map;
        }

        private static int[] createFreqDependentShiftMap(FFTs stft, int indexShift, SelectedWindowIndices indices = null)
        {
            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize);

            int centerIndex = (freqIndex2 + freqIndex1) / 2;

            //TODO: Implement this. not gonna be easy
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