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
using AudioAnalysis;

namespace Spectrogram_Plus.Windows
{
    /// <summary>
    /// Interaction logic for WhiteNoiseFilterWindow.xaml
    /// </summary>
    public partial class WhiteNoiseFilterWindow : Window
    {
        SpecPlusWindow parentRef; 
        public WhiteNoiseFilterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
        }


        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e)
        {
            parentRef.WhiteNoiseFilterAll(SliderWhiteNoiseThreshold.Value);
        }

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e)
        {
            parentRef.WhiteNoiseFilterSelectedWindow(SliderWhiteNoiseThreshold.Value);
        }
    }
}
