using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Spectrogram;
using System.IO;
using System.Globalization;
using Spectrogram_Structures;
using System.CodeDom;

/**
 * To Implement:
 * GUI display for displaying Hz when your mouse is on the spectrogram 
 * Method for pausing and selecting a part of the spectrogram 
 * 
 * Features to reserach and implement :
 * Audio filtering / Transformation on the selected part of the spectrogram
 * GUI interface popup that opens when you want to modify that data
 */
namespace SpecPlus
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SpecPlusWindow : Window
    {
        private Spectrogram.Spectrogram spec; //Spectrogram
        private Colormap[] cmaps;             //Colormaps for spectrogram display
        private DispatcherTimer specTimer;    //Spectrogram clock
        private Listener listener;            //Microphone listener
        private double whiteNoiseMin = 0;
        private void specTimer_tick(object sender, EventArgs e)
        {
            /**
             * Get new audio from the microphone for the spectrogram to process
             */
            double[] newAudio = listener.GetNewAudio();
            spec.Add(newAudio, process: false);
            int size = spec.GetFFTs().Count;
            double brightness = sliderBrightness.Value;

            
            const int rightMargin = 20;
            SpecGrid.MaxWidth = this.ActualWidth - ControlsGrid.ActualWidth - rightMargin;
            SpecGrid.MaxHeight = this.ActualHeight;
            scrollViewerSpec.MaxHeight = this.ActualHeight-60;
            spec.SetFixedWidth((int)SpecGrid.MaxWidth-55);
            if (spec.FftsToProcess > 0)
            {
                spec.Process();
                imageSpec.Source = spec.GetBitmapSource(brightness, dB: false, roll: false, whiteNoiseMin);
            }
        }

        /**
         * Sets up a listener for the selected microphone and initializes a spectrogram display
         */
        public void StartListening()
        {
            int sampleRate = Int32.Parse(SampleRates[cbSampleRate.SelectedIndex]);
            int fftSize = 1 << (9 + cbFFTsize.SelectedIndex);
            int stepSize = fftSize / 20;

            listener?.Dispose();
            listener = new Listener(cbMicInput.SelectedIndex, sampleRate);
            spec = new Spectrogram.Spectrogram(sampleRate, fftSize, stepSize);
        }


        private void SpecInit()
        {

            SampleRates.Add("6000");
            SampleRates.Add("12000");
            SampleRates.Add("24000");
            foreach (string sr in SampleRates)
                cbSampleRate.Items.Add(sr);
            cbSampleRate.SelectedIndex = 1;


            //Init mic inputs into the combo box
            if (NAudio.Wave.WaveIn.DeviceCount == 0)
            {
                MessageBox.Show("No audio input devices found.\n\nThis program will now exit.",
                    "ERROR", MessageBoxButton.OK);
                Close();
            }
            else
            {
                cbMicInput.Items.Clear();
                for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
                    cbMicInput.Items.Add(NAudio.Wave.WaveIn.GetCapabilities(i).ProductName);
                cbMicInput.SelectedIndex = 0;
            }

            //Init fft settings
            for (int i = 9; i < 12; i++)
                cbFFTsize.Items.Add($"2^{i} ({1 << i:N0})");
            cbFFTsize.SelectedIndex = 1;

            //Init colormaps
            cmaps = Colormap.GetColormaps();
            foreach (Colormap cmap in cmaps)
                cbCmaps.Items.Add(cmap.Name);
            cbCmaps.SelectedIndex = cbCmaps.Items.IndexOf("Viridis");


            //Timer used to continously update the spectrogram with new data
            specTimer = new DispatcherTimer();
            specTimer.Interval = TimeSpan.FromMilliseconds(15);
            specTimer.Tick += new EventHandler(specTimer_tick);
            specTimer.Start();
        }

        private List<string> SampleRates = new List<string>();

        public SpecPlusWindow()
        {
            InitializeComponent();
            SpecInit();
        }

        private void cbMicInput_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbFFTsize_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbCmaps_SelectionChanged(object sender, SelectionChangedEventArgs e) => spec.SetColormap(cmaps[cbCmaps.SelectedIndex]);

        private void scrollViewerSpec_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scrollViewerSpec.ScrollToRightEnd();
        }

        private void TextBoxWhiteNoise_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Double.TryParse(TextBoxWhiteNoise.Text, out whiteNoiseMin);
            }
        }

        private void cbSampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartListening();
        }
    }

}
