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
using SpecPlus;

namespace SpecPlus.Windows
{
    /// <summary>
    /// Interaction logic for FrequencyDependentShifterWindow.xaml
    /// </summary>
    public partial class FrequencyShifterWindow : Window
    {
        SpecPlusWindow parentRef;
        DispatcherTimer clock;

        public FrequencyShifterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
            this.clock = new DispatcherTimer();
            this.clock.Tick += Clock_Tick;
            this.clock.Start();
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            TextBoxFreqShift.Text = $"{(int)SliderFreqShift.Value} Hz";
            TextBoxOrder.Text = $"{(int)SliderOrder.Value}";
            TextBoxAtten.Text = string.Format("{0:0.00}", SliderThreshold.Value);
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            var processWindow = new FrequencyShifterWindow(parentRef);
            processWindow.Activate();
            processWindow.Show();
            processWindow.Topmost = true;
        }

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e)
        {
            AudioAnalysis.Processing.FrequencyShifter(parentRef.GetSTFT(), (int)SliderFreqShift.Value, parentRef.GetSelectedWindowIndices(), 
                order: (int)SliderOrder.Value, thresh: SliderThreshold.Value);
        }

    }
}
