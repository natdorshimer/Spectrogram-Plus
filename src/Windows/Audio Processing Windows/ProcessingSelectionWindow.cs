﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using SpecPlus;

namespace Spectrogram_Plus.Windows
{
    public class ProcessingSelectionWindow : Window
    {
        public SpecPlusWindow parentRef;
        public ProcessingSelectionWindow(SpecPlusWindow parentRef)
        {
            this.parentRef = parentRef;
        }

    }
}
