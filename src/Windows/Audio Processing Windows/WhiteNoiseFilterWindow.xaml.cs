﻿using AudioAnalysis;
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
using Spectrogram_Plus.Design;

namespace Spectrogram_Plus.Windows
{
    /// <summary>
    /// Interaction logic for WhiteNoiseFilterWindow.xaml
    /// </summary>
    public partial class WhiteNoiseFilterWindow : Window
    {
        private SpecPlusWindow parentRef;
        public WhiteNoiseFilterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
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
                Filter.WhiteNoiseFilter(stft, threshold, indices);
            }
            else
                Filter.WhiteNoiseFilter(stft, threshold);
        }

        private void ButtonApplyToWhole_Click(object sender, RoutedEventArgs e) =>
            ApplyWhiteNoiseFilter(SliderWhiteNoiseThreshold.Value, applyToWindow: false);
        

        private void ButtonApplyToWindow_Click(object sender, RoutedEventArgs e) =>
            ApplyWhiteNoiseFilter(SliderWhiteNoiseThreshold.Value, applyToWindow: true);
        
    }
}