using FftSharp;
using System;
using System.Collections.Generic;

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
            int index_cutoff = freq_cutoff / data.FreqResolution; ;
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
    }
}