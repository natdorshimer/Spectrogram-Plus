using AudioAnalysis;
using SpecPlus;
using Spectrogram_Plus.Design;
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

namespace Spectrogram_Plus.Windows
{
    /// <summary>
    /// Interaction logic for ApplyGain.xaml
    /// </summary>
    public partial class ApplyGainWindow : Window
    {
        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            ApplyGainWindow gainWindow = new ApplyGainWindow(parentRef);
            gainWindow.Activate();
            gainWindow.Show();
            gainWindow.Topmost = true;

        }
        private SpecPlusWindow parentRef;
        public ApplyGainWindow()
        {
            InitializeComponent();
        }
        public ApplyGainWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
        }


        public void ApplyGain(double dbGain, bool applyToWindow = false)
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
                Filter.AddGain(stft, dbGain, indices, dB: true);
            }
            else
                Filter.AddGain(stft, dbGain, dB: true);
        }

        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e) =>
            ApplyGain(SliderGain.Value, applyToWindow: false);

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e) =>
            ApplyGain(SliderGain.Value, applyToWindow: true);
        
    }
}
