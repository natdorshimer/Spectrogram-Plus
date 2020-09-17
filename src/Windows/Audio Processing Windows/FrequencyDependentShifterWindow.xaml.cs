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
using SpecPlus;

namespace SpecPlus.Windows
{
    /// <summary>
    /// Interaction logic for FrequencyDependentShifterWindow.xaml
    /// </summary>
    public partial class FrequencyDependentShifterWindow : Window
    {
        SpecPlusWindow parentRef;
        public FrequencyDependentShifterWindow(SpecPlusWindow parentRef)
        {
            InitializeComponent();
            this.parentRef = parentRef;
        }

        public static void OpenWindow(SpecPlusWindow parentRef)
        {
            var processWindow = new FrequencyDependentShifterWindow(parentRef);
            processWindow.Activate();
            processWindow.Show();
            processWindow.Topmost = true;
        }
    }
}
