using AudioAnalysis;
using SpecPlus;
using SpecPlus.Design;
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
using System.Windows.Threading;

namespace SpecPlus.Windows
{
    /// <summary>
    /// Interaction logic for FrequencyShifterWindow.xaml
    /// </summary>
    public partial class FrequencyShifterWindow : Window
    {
        SpecPlusWindow parentRef;
        DispatcherTimer clock;
        public FrequencyShifterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
            
            clock = new DispatcherTimer();
            clock.Tick += Clock_UpdateText;
            clock.Start();
        }

        private void Clock_UpdateText(object sender, EventArgs e)
        {
            this.TextBlockFreq.Text = $"Frequency Shift: {(int)SliderFreqShift.Value} Hz";
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            var processWindow = new FrequencyShifterWindow(parentRef);
            processWindow.Activate();
            processWindow.Show();
            processWindow.Topmost = true;
        }

        public void ApplyFrequencyShift(int freqShift, bool applyToWindow = false)
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
                Processing.FrequencyShifter(stft, (int)SliderFreqShift.Value, indices);
            }
            else
                Processing.FrequencyShifter(stft, (int)SliderFreqShift.Value);
        }

        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e) =>
            ApplyFrequencyShift((int)SliderFreqShift.Value, false);
        

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e) =>
            ApplyFrequencyShift((int)SliderFreqShift.Value, true);
    }
}
