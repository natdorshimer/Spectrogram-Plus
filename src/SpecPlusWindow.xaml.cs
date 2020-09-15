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
using System.CodeDom;
using NAudio.Wave.SampleProviders;
using AudioAnalysis;
using Microsoft.Win32;
using FftSharp;
using NAudio.Wave;
/**
 * Next Tasks:
 * 1. Implement saving feature to save the select window (or the spectrogram itself)
 * 2. Implement some basic audio filters
 */
namespace SpecPlus
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SpecPlusWindow : System.Windows.Window
    {
        private Spectrogram.Spectrogram spec; //Spectrogram
        private Colormap[] cmaps;             //Colormaps for spectrogram display
        private DispatcherTimer specTimer;    //Spectrogram/Program clock
        private Listener listener;            //Microphone listener


        //Selected Window Data
        private Point selectedWindowStartPoint;
        private Point selectedWindowEndPoint;
        private bool selectedWindowShouldDraw = false; //Determines if the selectedWindowToDraw should continue updating its position 
        private Rectangle selectedWindowToDraw = new Rectangle(); //This is the rectangle that shows the area of the spectrogram selected

        private int freq_resolution => spec.SampleRate / spec.FftSize;
        private double time_resolution => 1d / (double)freq_resolution;

        private double time_scale => time_resolution / zoomFactor;

        private double freq_scale => freq_resolution / zoomFactor;

        //Spectrogram Settings
        private double whiteNoiseMin = 0;     //Basic filter for White Noise
        private double overlap = 0.5;
        private readonly string[] sampleRates = { "5120", "10240", "20480", "40960" }; //Beyond 22khz is essentially pointless, but, options
        private bool specPaused = false;
        private double zoomFactor = 1;

        public SpecPlusWindow()
        {
            InitializeComponent();
            SpecInit();
        }

        private void SpecTimer_tick(object sender, EventArgs e)
        {
            double[] newAudio = listener.GetNewAudio();
            if (!specPaused)
                spec.Add(newAudio, process: false);

            spec.Process();
            DisplaySpectrogram();

            if (selectedWindowShouldDraw)
                UpdateSelectedSpecWindow();
        }

        private void DisplaySpectrogram()
        {
            //Display Settings
            //TODO: Clean up GUI code
            const int rightMargin = 20;
            double brightness = sliderBrightness.Value;
            SpecGrid.MaxWidth = this.ActualWidth - ControlsGrid.ActualWidth - rightMargin;
            SpecGrid.MaxHeight = this.ActualHeight;
            scrollViewerSpec.MaxHeight = this.ActualHeight - 60;

            spec.SetFixedWidth((int)((SpecGrid.MaxWidth - 55) / zoomFactor));
            BitmapSource source = spec.GetBitmapSource(brightness, dB: false, roll: false, whiteNoiseMin);
            TransformedBitmap trans = new TransformedBitmap(source, new ScaleTransform(zoomFactor, zoomFactor));
            imageSpec.Source = trans;
        }

        /**
         * Sets up a listener for the selected microphone and initializes a spectrogram display
         */
        public void StartListening()
        {
            int sampleRate = Int32.Parse(sampleRates[cbSampleRate.SelectedIndex]);
            int fftSize = 1 << (9 + cbFFTsize.SelectedIndex);
            int stepSize = fftSize - (int) (fftSize * overlap); //This can change the quality of the ISTFT signal. Keep an eye on this

            listener?.Dispose();
            listener = new Listener(cbMicInput.SelectedIndex, sampleRate);
            spec = new Spectrogram.Spectrogram(sampleRate, fftSize, stepSize);
        }


        private void TogglePause()
        {
            specPaused = !specPaused;
            if (specPaused)
                PauseButtonText.Text = "Run";
            else
                PauseButtonText.Text = "Pause";
        }

        private void SpecInit()
        {
            foreach (string sr in sampleRates)
                cbSampleRate.Items.Add(sr);
            cbSampleRate.SelectedIndex = 2;


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
            for (int i = 9; i < 14; i++)
                cbFFTsize.Items.Add($"2^{i} ({1 << i:N0})");
            cbFFTsize.SelectedIndex = 1;

            //Init fft settings
            for (int i = 0; i < 10; i++)
                cbOverlap.Items.Add($"{i}/{(i+1)}");
            cbOverlap.SelectedIndex = 1;


            //Init colormaps
            cmaps = Colormap.GetColormaps();
            foreach (Colormap cmap in cmaps)
                cbCmaps.Items.Add(cmap.Name);
            cbCmaps.SelectedIndex = cbCmaps.Items.IndexOf("Viridis");


            //Timer used to continously update the spectrogram with new data
            specTimer = new DispatcherTimer();
            specTimer.Interval = TimeSpan.FromMilliseconds(5);
            specTimer.Tick += new EventHandler(SpecTimer_tick);
            specTimer.Start();
        }

        private void LinearFrequencyShifterMulti(FFTs stft)
        {
            string filename = "C:\\Users\\Natalie\\Documents\\wavs\\multi\\";
            for (int i = 0; i <= 5; i++)
            {
                string end = $"{i}.wav";
                if (i > 0) Filter.LinearFrequencyShifter(stft, 100);
                stft.SaveToWav(filename+end);
            }
        }

        private void SaveSpectrogram()
        {
            if (!specPaused)
                TogglePause();

            SaveFileDialog saveFile = new SaveFileDialog
            {
                Filter = "wav files(*.wav)| *.wav| All files(*.*) | *.* ",
                FilterIndex = 1
            };

            if ((bool)saveFile.ShowDialog())
            {
                
                string filename = saveFile.FileName;
                FFTs stft = new FFTs(spec.GetFFTs(), spec.SampleRate, spec.StepSize, spec.GetWindow());
                LinearFrequencyShifterMulti(stft);
                //stft.SaveToWav(filename);
            }
        }

        private void saveSnippet()
        {

            /**
             * TODO: Bug: the saved snippet isn't entirely accurate. It does save, but the coordinates are sometimes wrong.
             */

            //Get the indices of the STFT
            int fftStartIndex = (int)(selectedWindowStartPoint.X / zoomFactor);
            int fftEndIndex = (int)(selectedWindowEndPoint.X / zoomFactor);
            if (fftStartIndex > fftEndIndex)
            {
                int temp = fftEndIndex;
                fftEndIndex = fftStartIndex;
                fftStartIndex = temp;
            }

            //Make a snippet from those indices  for easy transforming / saving
            FFTs stft = new FFTs(spec.GetFFTs(), spec.SampleRate, spec.StepSize, spec.GetWindow());
            List<Complex[]> ffts = stft.DeepCopyFFTs();
            List<Complex[]> snippet = new List<Complex[]>();
            FFTs stft_snippet = new FFTs(snippet, stft.sampleRate, stft.stepSize, stft.window);
            for (int i = fftStartIndex; i <= fftEndIndex; i++)
                snippet.Add(ffts[i]);
            stft_snippet.SaveToWav("C:\\Users\\Natalie\\Documents\\wavs\\snips\\test.wav");
        }

        private void InitSelectedSpecWindow(Point startPoint)
        {
            //Specify that it should starting drawing the rectangle and initialize a new rectangle onto the painting grid
            selectedWindowShouldDraw = true;
            PaintGrid.Children.Remove(selectedWindowToDraw);

            //Selection window settings for spectrogram
            //Mouse is needed in case mouseleftbutton up happens on the rectangle instead of imagespec
            selectedWindowToDraw = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeThickness = 1.0
            };

            PaintGrid.Children.Add(selectedWindowToDraw);

            selectedWindowStartPoint = startPoint;
            selectedWindowEndPoint = startPoint;
            UpdateSelectedSpecWindow();
        }

        private void UpdateSelectedSpecWindow()
        {
            Rect rectWindow = new Rect(selectedWindowStartPoint, selectedWindowEndPoint);
            selectedWindowToDraw.Arrange(rectWindow);
        }

        private void RemoveSelectedWindow()
        {
            PaintGrid.Children.Remove(selectedWindowToDraw);
            selectedWindowToDraw = new Rectangle();
            selectedWindowShouldDraw = false;
        }

        /**
         * Event Handlers
         */

        private void cbMicInput_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbFFTsize_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbSampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbOverlap_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

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

        private void PaintGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(imageSpec);
            selectedWindowEndPoint = p;
            int freq_resolution = spec.SampleRate / spec.FftSize;
            int freq = (int)(freq_resolution * (imageSpec.ActualHeight-p.Y) / zoomFactor);   //hz
            double time_resolution = 1d / ((double)freq_resolution);
            double time = ((imageSpec.ActualWidth - p.X) * time_resolution / zoomFactor * 1000); //ms

            if(time > 1000)
                MousePosition.Text =  $"{freq} Hz,  {Math.Round(time/1000, 2)} s";
            else
                MousePosition.Text = $"{freq} Hz,  {(int)time} ms";
        }

        private void PaintGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
            => InitSelectedSpecWindow(e.GetPosition(imageSpec));
        
        private void PaintGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedWindowShouldDraw = false;

            /**
             * TODO: Implement the features upon selecting the window
             *   * Implement basic filters to apply to the spectrogram / save the spectrogram
             */
        }

        private void PaintGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TogglePause();

            //TODO: Modify behavior for filters once implemented, open a dialog box with filtering options
        }


        private void PauseButton_Click(object sender, RoutedEventArgs e) => TogglePause();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    TogglePause();
                    break;
                case Key.Delete:
                    RemoveSelectedWindow();
                    break;
                case (Key.S):
                    if ((Keyboard.IsKeyDown(Key.LeftCtrl) || (Keyboard.IsKeyDown(Key.RightCtrl))))
                        SaveSpectrogram();
                    else
                        saveSnippet();
                    break;
            }
        }

        private void TextBoxOverlap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                overlap = cbOverlap.SelectedIndex / (cbOverlap.SelectedIndex + 1);
                StartListening();
            }
        }

        private void PaintGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoomFactor += (double)e.Delta / (1000);
            scrollViewerSpec.ScrollToBottom();
        }
    }
}
