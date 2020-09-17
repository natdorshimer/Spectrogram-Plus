using System;
using System.Collections.Generic;
using System.Text;
using SpecPlus;
using SpecPlus.Design;
using FftSharp;

namespace AudioAnalysis
{
    
    public static class Filters
    {
        //Provides a set of general purpose filters that act on a Short Time Fourier Transform object

        private static void LowPassFilter(FFTs stft, double cutoff, SelectedWindowIndices indices = null)
        {
            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize / 2);

            List<Complex[]> data = stft.GetFFTs();
            int index_cutoff = (int)(cutoff / stft.FreqResolution);
            for (int n = timeIndex1; n < timeIndex2; n++)
            {
                for (int k = freqIndex1; k < freqIndex2; k++)
                {
                    if (k >= index_cutoff)
                    {
                        data[n][k] = new Complex();
                        data[n][stft.fftSize - 1 - k] = new Complex(); //Mirrored side
                    }
                }
            }
        }

        private static void HighPassFilter(FFTs stft, double cutoff, SelectedWindowIndices indices = null)
        {

            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, stft.Count, 0, stft.fftSize / 2);

            List<Complex[]> data = stft.GetFFTs();
            int index_cutoff = (int)(cutoff / stft.FreqResolution);
            for (int n = timeIndex1; n < timeIndex2; n++)
            {
                for (int k = freqIndex1; k < freqIndex2; k++)
                {
                    if (k <= index_cutoff)
                    {
                        data[n][k] = new Complex();
                        data[n][stft.fftSize - 1 - k] = new Complex(); //Mirrored side
                    }
                }
            }
        }

        public static void WhiteNoiseFilter(FFTs stft, double threshold, SelectedWindowIndices indices = null)
        {
            List<Complex[]> data = stft.GetFFTs();
            (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) =
                indices != null ? indices.Indices() : (0, data.Count, 0, stft.fftSize);

            for (int n = timeIndex1; n < timeIndex2; n++)
            {
                for (int k = freqIndex1; k < freqIndex2; k++)
                {
                    if (data[n][k].Magnitude < threshold)
                    {
                        data[n][k].Real = 0;
                        data[n][k].Imaginary = 0;
                    }
                }
            }
        }

    }
}
