using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FftSharp;

namespace Spectrogram_Plus
{
    /// <summary>
    /// Interaction logic for FFTPlot.xaml
    /// </summary>
    public partial class FFTPlot : System.Windows.Window
    {
        public FFTPlot()
        {
            InitializeComponent();
        }

        public void PlotFFT(Complex[] fft, int sampleRate = 0, bool ssb = true)
        {
            int length = (ssb == false ? fft.Length : fft.Length / 2);

            double[] magnitude = new double[length];
            double[] phase = new double[length];
            double[] real = new double[length];
            double[] imag = new double[length];
            double[] scale = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (sampleRate == 0)
                    scale[i] = i;
                else
                    scale[i] = sampleRate / fft.Length * i;

                magnitude[i] = fft[i].Magnitude;
                phase[i] = Math.Atan(fft[i].Imaginary / fft[i].Real);
                real[i] = fft[i].Real;
                imag[i] = fft[i].Imaginary;
            }

            FftPlotMagnitude.plt.PlotScatter(scale, magnitude);
            FftPlotMagnitude.plt.Title("Magnitude of FFT");
            FftPlotMagnitude.plt.XLabel($"{(sampleRate == 0 ? "Bins" : "Frequency (Hz)")}");
            FftPlotMagnitude.Render();

            FftPlotPhase.plt.PlotScatter(scale, phase);
            FftPlotPhase.plt.Title("Phase of FFT");
            FftPlotPhase.plt.XLabel($"{(sampleRate == 0 ? "Bins" : "Frequency (Hz)")}");
            FftPlotPhase.Render();

            FftPlotReal.plt.PlotScatter(scale, real);
            FftPlotReal.plt.Title("Real part of FFT");
            FftPlotReal.plt.XLabel($"{(sampleRate == 0 ? "Bins" : "Frequency (Hz)")}");
            FftPlotReal.Render();

            FftPlotImaginary.plt.PlotScatter(scale, imag);
            FftPlotImaginary.plt.Title("Imag part of FFT");
            FftPlotImaginary.plt.XLabel($"{(sampleRate == 0 ? "Bins" : "Frequency (Hz)")}");
            FftPlotImaginary.Render();
        }
    }
}
