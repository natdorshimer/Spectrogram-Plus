﻿#pragma checksum "..\..\..\SpecWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ADF890C402103C77560D0074BF484B30AE191A2E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SpecPlus;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SpecPlus {
    
    
    /// <summary>
    /// SpecPlusWindow
    /// </summary>
    public partial class SpecPlusWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ControlsGrid;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbMicInput;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbFFTsize;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cbCmaps;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider sliderBrightness;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid SpecGrid;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer scrollViewerSpec;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\SpecWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imageSpec;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Spectrogram Plus;component/specwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\SpecWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ControlsGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.cbMicInput = ((System.Windows.Controls.ComboBox)(target));
            
            #line 31 "..\..\..\SpecWindow.xaml"
            this.cbMicInput.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cbMicInput_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cbFFTsize = ((System.Windows.Controls.ComboBox)(target));
            
            #line 38 "..\..\..\SpecWindow.xaml"
            this.cbFFTsize.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cbFFTsize_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cbCmaps = ((System.Windows.Controls.ComboBox)(target));
            
            #line 45 "..\..\..\SpecWindow.xaml"
            this.cbCmaps.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cbCmaps_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.sliderBrightness = ((System.Windows.Controls.Slider)(target));
            
            #line 52 "..\..\..\SpecWindow.xaml"
            this.sliderBrightness.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.sliderBrightness_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.SpecGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.scrollViewerSpec = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 8:
            this.imageSpec = ((System.Windows.Controls.Image)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

