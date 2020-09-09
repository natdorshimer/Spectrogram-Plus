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
using NAudio.Wave.SampleProviders;

/**
 * To Implement
 * See: ProjectMap
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

        private double whiteNoiseMin = 0;     //Basic filter for White Noise
        private string[] sampleRates = { "5500", "11000", "22000", "44000" }; //Beyond 22khz is essentially pointless, but, options


        //Selected Window Data
        private Point selectedWindowStartPoint;
        private Point selectedWindowEndPoint;               
        private bool selectedWindowShouldDraw = false; //Determines if the selectedWindowToDraw should continue updating its position 
        private Rectangle selectedWindowToDraw = new Rectangle(); //This is the rectangle that shows the area of the spectrogram selected

        private void specTimer_tick(object sender, EventArgs e)
        {
            /**
             * Get new audio from the microphone for the spectrogram to process
             */
            double[] newAudio = listener.GetNewAudio();
            spec.Add(newAudio, process: false);

            if (spec.FftsToProcess > 0)
                ProcessAndDisplaySpectrogram();
            if(selectedWindowShouldDraw)
                UpdateSelectedSpecWindow();

        }

        private void  UpdateSelectedSpecWindow()
        {
            Rect rectWindow = new Rect(selectedWindowStartPoint, selectedWindowEndPoint);
            selectedWindowToDraw.Width = rectWindow.Width;
            selectedWindowToDraw.Height = rectWindow.Height;

            //TODO: this is basically just because the mousevents won't work if the rectangle is below the mouse. Find a fix
            const int offset = 5; 
            rectWindow.Location = new Point(rectWindow.X - offset, rectWindow.Y - offset);

            selectedWindowToDraw.Arrange(rectWindow);
            selectedWindowToDraw.Stroke = Brushes.White;
            selectedWindowToDraw.StrokeThickness = 1.0;
        }


        private void ProcessAndDisplaySpectrogram()
        {
            //Display Settings
            const int rightMargin = 20;
            int size = spec.GetFFTs().Count;
            double brightness = sliderBrightness.Value;
            SpecGrid.MaxWidth = this.ActualWidth - ControlsGrid.ActualWidth - rightMargin;
            SpecGrid.MaxHeight = this.ActualHeight;
            scrollViewerSpec.MaxHeight = this.ActualHeight - 60;
            spec.SetFixedWidth((int)SpecGrid.MaxWidth - 55);

            spec.Process();
            imageSpec.Source = spec.GetBitmapSource(brightness, dB: false, roll: false, whiteNoiseMin);
        }

        /**
         * Sets up a listener for the selected microphone and initializes a spectrogram display
         */
        public void StartListening()
        {
            int sampleRate = Int32.Parse(sampleRates[cbSampleRate.SelectedIndex]);
            int fftSize = 1 << (9 + cbFFTsize.SelectedIndex);
            int stepSize = fftSize / 20;

            listener?.Dispose();
            listener = new Listener(cbMicInput.SelectedIndex, sampleRate);
            spec = new Spectrogram.Spectrogram(sampleRate, fftSize, stepSize);
        }

 

        public SpecPlusWindow()
        {
            InitializeComponent();
            SpecInit();
        }


        /**
         * Event Handlers
         */

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
                Double.TryParse(TextBoxWhiteNoise.Text, out whiteNoiseMin);
        }

        private void cbSampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartListening();
        }


        private void imageSpec_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(imageSpec);
            selectedWindowEndPoint = p;
            MousePosition.Text = (new Point((int)p.X, (int)p.Y)).ToString();
        }


        private void imageSpec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Release the window if the user clicks again
            if (selectedWindowShouldDraw)
                selectedWindowShouldDraw = false;
            else
            {
                selectedWindowShouldDraw = true;
                PaintingGrid.Children.Remove(selectedWindowToDraw);
                selectedWindowToDraw = new Rectangle();
                PaintingGrid.Children.Add(selectedWindowToDraw);
                selectedWindowStartPoint = e.GetPosition(imageSpec);
            }
        }

        private void imageSpec_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /**
             * TODO: Implement the features upon selecting the window
             * 
             * 1. Implement pause functionality on the spectrogram
             * 2. Implement saving feature to save the window
             * 3. Implement basic filters to apply to the spectrogram / save the spectrogram
             * 4. Make a gui for filters
             */
        }

        //TODO: Modify behavior for filters once implemented
        private void imageSpec_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PaintingGrid.Children.Remove(selectedWindowToDraw);
            selectedWindowToDraw = new Rectangle();
            selectedWindowShouldDraw = false;
        }

        
        private void SpecInit()
        {
            foreach (string sr in sampleRates)
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
    }
}
