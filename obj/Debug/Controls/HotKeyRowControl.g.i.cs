﻿#pragma checksum "..\..\..\Controls\HotKeyRowControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8183929BD75A27461838DC361E8DB127"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using NHotkey.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using Xceed.Wpf.Toolkit;
using mqtt_hotkeys_test.Windows;


namespace mqtt_hotkeys_test.Windows {
    
    
    /// <summary>
    /// HotKeyRowControl
    /// </summary>
    public partial class HotKeyRowControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\..\Controls\HotKeyRowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.WatermarkTextBox TxtHotKey;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\Controls\HotKeyRowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.WatermarkTextBox TxtTopic;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\Controls\HotKeyRowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.IntegerUpDown TxtQos;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Controls\HotKeyRowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.WatermarkTextBox TxtMessage;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/mqtt_hotkeys_test;component/controls/hotkeyrowcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\HotKeyRowControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.TxtHotKey = ((Xceed.Wpf.Toolkit.WatermarkTextBox)(target));
            
            #line 26 "..\..\..\Controls\HotKeyRowControl.xaml"
            this.TxtHotKey.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.TxtHotKey_OnMouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.TxtTopic = ((Xceed.Wpf.Toolkit.WatermarkTextBox)(target));
            return;
            case 3:
            this.TxtQos = ((Xceed.Wpf.Toolkit.IntegerUpDown)(target));
            
            #line 50 "..\..\..\Controls\HotKeyRowControl.xaml"
            this.TxtQos.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TxtQos_OnMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 51 "..\..\..\Controls\HotKeyRowControl.xaml"
            this.TxtQos.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TxtQos_OnMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 4:
            this.TxtMessage = ((Xceed.Wpf.Toolkit.WatermarkTextBox)(target));
            return;
            case 5:
            
            #line 67 "..\..\..\Controls\HotKeyRowControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BtnTest_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

