using AudioAnalysis;
using SpecPlus;
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
using SpecPlus.Design;
using System.Windows.Threading;

namespace SpecPlus.Windows
{
    /// <summary>
    /// Interaction logic for WhiteNoiseFilterWindow.xaml
    /// </summary>
    public partial class WhiteNoiseFilterWindow : Window
    {
        private SpecPlusWindow parentRef;
        private DispatcherTimer clock;
        public WhiteNoiseFilterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
            clock = new DispatcherTimer();
            clock.Tick += Clock_Tick;
            clock.Start();
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            TextBoxWhiteNoise.Text = $"Threshold: {(int) SliderWhiteNoiseThreshold.Value} dB";
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            var processWindow = new WhiteNoiseFilterWindow(parentRef);
            processWindow.Activate();
            processWindow.Show();
            processWindow.Topmost = true;
        }

        public void ApplyWhiteNoiseFilter(double threshold, bool applyToWindow = false)
        {
            FFTs stft = parentRef.GetSTFT();
            SelectedWindowIndices indices = parentRef.GetSelectedWindowIndices();
            if (applyToWindow)
            {
                if (!indices.Exists())
                {
                    MessageBox.Show("There is no window selected!");
                    return;
                }
                Filters.WhiteNoiseFilter(stft, threshold, indices);
            }
            else
                Filters.WhiteNoiseFilter(stft, threshold);
        }

        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e) =>
            ApplyWhiteNoiseFilter(SliderWhiteNoiseThreshold.Value, applyToWindow: false);
        

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e) =>
            ApplyWhiteNoiseFilter(SliderWhiteNoiseThreshold.Value, applyToWindow: true);

        private void ButtonRealTimeFiltering_Click(object sender, RoutedEventArgs e)
        {
            parentRef.ToggleRealTimeWhiteNoiseFilter(SliderWhiteNoiseThreshold.Value);
        }

        private void SliderWhiteNoiseThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(e.NewValue - e.OldValue > 0 && parentRef != null)
                parentRef.SetWhiteNoiseFilterValue(SliderWhiteNoiseThreshold.Value);
        }
    }
}
