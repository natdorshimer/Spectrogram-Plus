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
using Spectrogram_Plus;
using System.Windows.Controls.Primitives;
using Spectrogram_Plus.Design;



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
        private SelectedSpecWindow selectedWindow = new SelectedSpecWindow();

        private int freq_resolution => spec.SampleRate / spec.FftSize;
        private double time_resolution => 1d / (double)freq_resolution;

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

        //What the program does every clock cycle
        private void SpecTimer_tick(object sender, EventArgs e)
        {
            double[] newAudio = listener.GetNewAudio();
            if (!specPaused)
                spec.Add(newAudio, process: false);

            spec.Process();
            DisplaySpectrogram();
        }

        private void DisplaySpectrogram()
        {
            //Display Settings
            double brightness = sliderBrightness.Value;
            double specGridWidth = this.ActualWidth - ControlsGrid.ActualWidth;
            ScrollBar scrollBar = (ScrollBar)scrollViewerSpec.Template.FindName("PART_VerticalScrollBar", scrollViewerSpec);
            double scrollBarSpace = scrollBar.ActualWidth*2 + scrollViewerSpec.Margin.Left;
            SpecGrid.MaxWidth = specGridWidth;
            spec.SetFixedWidth((int)((specGridWidth - scrollBarSpace)));
            
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
                Filter.LFSMultiSave(stft);
            }
        }


        private void InitSelectedSpecWindow(Point startPoint)
        {
            PaintGrid.Children.Remove(selectedWindow.windowToDraw);
            selectedWindow = new SelectedSpecWindow(startPoint);
            PaintGrid.Children.Add(selectedWindow.windowToDraw);
        }

        private void RemoveSelectedSpecWindow()
        {
            PaintGrid.Children.Remove(selectedWindow.windowToDraw);
            selectedWindow = new SelectedSpecWindow();
        }

        private (int t_index, int f_index) PositionToIndices(Point p)
        {
            int f_index = (int)((imageSpec.ActualHeight - p.Y) / zoomFactor);

            int maxPossibleFFTs = (int)(imageSpec.ActualWidth / zoomFactor);
            int t_index = (int)((p.X - (maxPossibleFFTs - spec.GetFFTs().Count)) / zoomFactor);
            t_index = t_index < 0 ? 0 : t_index; //If there's not enough data and the point is out of bounds, just start it at 0

            return (t_index, f_index);
        }

        private void SaveSnippet()
        {
            //Get the indices of the STFT
            int fftStartIndex = PositionToIndices(selectedWindow.startPoint).t_index;
            int fftEndIndex = PositionToIndices(selectedWindow.endPoint).t_index;
            if (fftStartIndex > fftEndIndex)
                (fftStartIndex, fftEndIndex) = (fftEndIndex, fftStartIndex);

            FFTs stft = new FFTs(spec.CopyFFTs(), spec.SampleRate, spec.StepSize, spec.GetWindow());
            stft.SaveSnippet("C:\\Users\\Natalie\\Documents\\wavs\\snips\\test.wav", fftStartIndex, fftEndIndex); //todo: Testing only
        }

        private void OpenPlotAt(Point p)
        {
            //Opens a window that plots information of the FFT at the point where the user double clicked

            int fft_index = PositionToIndices(p).t_index;
            Complex[] fft = spec.GetFFTs()[fft_index];
            FFTPlot plotwin = new FFTPlot();
            plotwin.PlotFFT(fft, spec.SampleRate);
            plotwin.Activate();
            plotwin.Show();
            plotwin.Topmost = true;
        }

        private void TogglePause()
        {
            specPaused = !specPaused;
            if (specPaused)
                PauseButtonText.Text = "Run";
            else
                PauseButtonText.Text = "Pause";
        }


        /**
         * Event Handlers
         */

        private void cbMicInput_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbFFTsize_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbSampleRate_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbOverlap_SelectionChanged(object sender, SelectionChangedEventArgs e) => StartListening();

        private void cbCmaps_SelectionChanged(object sender, SelectionChangedEventArgs e) => spec.SetColormap(cmaps[cbCmaps.SelectedIndex]);

        private void scrollViewerSpec_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    TogglePause();
                    break;
                case Key.Delete:
                    RemoveSelectedSpecWindow();
                    break;
                case (Key.S):
                    if ((Keyboard.IsKeyDown(Key.LeftCtrl) || (Keyboard.IsKeyDown(Key.RightCtrl))))
                        SaveSpectrogram();
                    else
                        SaveSnippet();
                    break;
            }
        }

        private void TextBoxWhiteNoise_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Double.TryParse(TextBoxWhiteNoise.Text, out whiteNoiseMin);
        }

        private void PaintGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(imageSpec);

            //Make sure the selected window is being updated properly
            selectedWindow.UpdateShape(p);

            //Update mouse position info for frequency and time location
            (int t_index, int f_index) = PositionToIndices(p);

            double time = (t_index * time_resolution * 1000); //ms
            int freq = (int)(freq_resolution * f_index);   //hz

            if(time > 1000)
                MousePosition.Text =  $"{freq} Hz,  {Math.Round(time/1000, 2)} s";
            else
                MousePosition.Text = $"{freq} Hz,  {(int)time} ms";
        }


        private void PaintGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                OpenPlotAt(e.GetPosition(imageSpec));
            else
                InitSelectedSpecWindow(e.GetPosition(imageSpec));
        }

        private void PaintGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedWindow.FinishDrawing();

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

        private void PaintGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoomFactor += (double)e.Delta / (1000);
            if (zoomFactor < 1)
                zoomFactor = 1;
            scrollViewerSpec.ScrollToRightEnd();
            scrollViewerSpec.ScrollToBottom();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e) => TogglePause();

    }
}
