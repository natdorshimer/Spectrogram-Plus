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

namespace Spectrogram_Plus.Windows
{
    /// <summary>
    /// Interaction logic for FrequencyShifterWindow.xaml
    /// </summary>
    public partial class FrequencyShifterWindow : Window
    {
        SpecPlusWindow parentRef;
        public FrequencyShifterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            var processWindow = new FrequencyShifterWindow(parentRef);
            processWindow.Activate();
            processWindow.Show();
            processWindow.Topmost = true;
            
        }
    }
}
