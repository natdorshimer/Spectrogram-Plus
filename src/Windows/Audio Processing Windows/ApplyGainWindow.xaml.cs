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
    /// Interaction logic for ApplyGain.xaml
    /// </summary>
    public partial class ApplyGainWindow : Window
    {
        private SpecPlusWindow parentRef;
        private DispatcherTimer clock;

        public ApplyGainWindow()
        {
            InitializeComponent();
        }
        public ApplyGainWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
            this.clock = new DispatcherTimer();
            this.clock.Tick += Clock_Tick;
            this.clock.Start();
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            TextBoxGain.Text = $"Gain: {(int)SliderGain.Value} dB";
      
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
                Processing.AddGain(stft, dbGain, indices, dB: true);
            }
            else
                Processing.AddGain(stft, dbGain, dB: true);
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            ApplyGainWindow gainWindow = new ApplyGainWindow(parentRef);
            gainWindow.Activate();
            gainWindow.Show();
            gainWindow.Topmost = true;

        }

        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e) =>
            ApplyGain(SliderGain.Value, applyToWindow: false);

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e) =>
            ApplyGain(SliderGain.Value, applyToWindow: true);
        
    }
}
