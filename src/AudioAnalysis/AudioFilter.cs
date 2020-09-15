using FftSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace AudioAnalysis
{
    //TODO Populate with types of audio filters
    public enum AudioFilters
    {
        LowPass,
        HighPass,
        NoFilter //todo: testing purposes
    }

    public class Filter
    {
        /// <summary>
        /// Applies a filter of AudioFilters type to an STFT structure
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filter"></param>
        public static void ApplyFilter(FFTs data, AudioFilters filter)
        {
            switch (filter)
            {
                case AudioFilters.HighPass:
                    HighPassFilter(data);
                    break;
                case AudioFilters.LowPass:
                    LowPassFilter(data);
                    break;
                case AudioFilters.NoFilter:
                    break;
            }
        }

        //Non smooth basic low pass filter test
        private static void LowPassFilter(FFTs data)
        {
            const int freq_cutoff = 10000;//hz, test
            int index_cutoff = freq_cutoff / data.FreqResolution; 
            foreach(Complex[] fft in data.GetFFTs())
            {
                for(int i = 0; i < fft.Length; i++)
                {
                    if (i >= index_cutoff) {
                        fft[i].Real = 0; 
                        fft[i].Imaginary = 0;
                    }
                }
            }
        }

        private static void HighPassFilter(FFTs data)
        {
            const int freq_cutoff = 600; //hz, test
            int index_cutoff = freq_cutoff / data.FreqResolution; 
            foreach (Complex[] fft in data.GetFFTs())
            {
                for (int i = 0; i < fft.Length; i++)
                {
                    if (i <= index_cutoff)
                    {
                        fft[i].Real = 0;
                        fft[i].Imaginary = 0;
                    }
                }
            }
        }


        public static void LinearFrequencyShifter(FFTs data, int freq_shift)
        {
            int index_shift = freq_shift / data.FreqResolution;
            if (index_shift < 1)
                return;

            List<Complex[]> ffts = data.GetFFTs();
            foreach (Complex[] fft in ffts)
            {
                for (int i = data.nyquistBin-1; i >= index_shift; i--)
                {
                    fft[i] = fft[i - index_shift];
                    fft[data.fftSize - i-1] = fft[(data.fftSize-i-1) + index_shift];
                }

                for (int i = 0; i < index_shift; i++)
                {
                    fft[i] = new Complex(0, 0);
                    fft[data.fftSize - i - 1] = new Complex(0, 0);
                }
            }
            
        }
    }
}