using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using static WindowsInput.InputSimulator;
using NHotkey;
using NHotkey.Wpf;
using uPLibrary.Networking.M2Mqtt;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for HotKeyRowControl.xaml
    /// </summary>
    public partial class SubHotKeyRowControl : UserControl
    {

        public List<VirtualKeyCode> modifierKeys = new List<VirtualKeyCode>();
        public Key hotKey = 0;
        public BindingSettings Binding;

        public SubHotKeyRowControl()
        {
            InitializeComponent();
        }

        public void UpdateUi()
        {
            if (Binding != null)
            {
                TxtTopic.Text = Binding.Topic;
                if (Enum.TryParse(Binding.ModKeys, true, out ModifierKeys modifierKeys) &&
                    Enum.TryParse(Binding.HotKey, true, out Key keyLtr))
                {
                    var modKeysString = CleanModifierKeysString(modifierKeys.ToString());
                    TxtHotKey.Text = $"{modKeysString} + {keyLtr}";
                    SetUpKeyPress(keyLtr, modifierKeys);
                }
            }
        }

        private void BtnSub_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.MqttTopics.Add(new MqttTopic
            {
                QosLevel = byte.Parse(TxtQos.Text),
                Topic = TxtTopic.Text
            });
            MainWindow._mqttClient.Subscribe(MainWindow.MqttTopics.Select(x=>x.Topic).ToArray(), 
                                             MainWindow.MqttTopics.Select(x=>x.QosLevel).ToArray());
            MainWindow._mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;

        }

        private void _mqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.Topic == TxtTopic.Text)
                {
                    if (Encoding.UTF8.GetString(e.Message) == TxtTrigger.Text)
                    {
                        VirtualKeyCode vKey = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(hotKey);
                        // Handle and press hotkey
                        var keyCodes = new List<VirtualKeyCode>();
                        keyCodes.Add(vKey);
                        foreach (var virtualKeyCode in keyCodes)
                        {
                            Console.WriteLine("ZONIKS SCOOB"+virtualKeyCode.ToString());
                        }
                        foreach (var virtualKeyCode in modifierKeys)
                        {
                            Console.WriteLine("ZONIKS SCOOB" + virtualKeyCode.ToString());
                        }


                        var test = new InputSimulator().Keyboard.ModifiedKeyStroke(modifierKeys, keyCodes);

                        //keyCodes.Add((VirtualKeyCode)vKey);
                        Console.WriteLine("keypressed");
                    }
                }
            });

        }


        private void TxtHotKey_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var window = new SelectHotKey { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var modKeysString = window.ModKeys.ToString();
                var modKeysForConfig = modKeysString;
                var modKeysList = modKeysForConfig.Split(new [] {", "}, StringSplitOptions.None);

                foreach (var keyStr in modKeysList)
                {
                    switch (keyStr.ToLower())
                    {
                        // TODO: Add L&R Control/shift support (Tyler add buttons)
                        case "control":
                            modifierKeys.Add(VirtualKeyCode.CONTROL);
                            break;
                        case "windows":
                            modifierKeys.Add(VirtualKeyCode.LWIN);
                            break;
                        case "alt":
                            modifierKeys.Add(VirtualKeyCode.MENU);
                            break;
                        case "shift":
                            modifierKeys.Add(VirtualKeyCode.SHIFT);
                            break;
                    }
                }



                var hotKeyLetter = window.HotKey.ToString();
                
                modKeysString = CleanModifierKeysString(modKeysString);
                TxtHotKey.Text = $"{modKeysString} + {window.HotKey}";
                SetUpKeyPress(window.HotKey, window.ModKeys);
                Console.WriteLine(window.HotKey.ToString());
                // Create object for config
                Binding = new BindingSettings
                {
                    HotKey = hotKeyLetter,
                    ModKeys = modKeysForConfig
                };
            }
        }

        private static string CleanModifierKeysString(string modKeysString)
        {
            modKeysString = modKeysString.Replace("Windows", "Win");
            modKeysString = modKeysString.Replace(",", " +");
            return modKeysString;
        }

        private void SetUpKeyPress(Key key, ModifierKeys modKeys)
        {
            try
            {
                // TODO handler

            }
            catch (Exception ex)
            {
                TxtHotKey.Text = "Double click to set key";
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex);
            }
        }

        private void HandleHotKey(object sender, HotkeyEventArgs e)
        {
            PublishMqttMessage();
        }

        private void PublishMqttMessage()
        {
            if (TxtReply.Text == "" || TxtTopic.Text == "")
            {
                MessageBox.Show("Topic or message cannot be blank", "Invalid selections", MessageBoxButton.OK);
                return;
            }
            MainWindow._mqttClient.Publish(TxtTopic.Text,
                       Encoding.UTF8.GetBytes(TxtReply.Text),
                       byte.Parse(TxtQos.Text),
                       false);
        }

        public Point mouseStartPos;
        public Point mouseEndPos;

        private void TxtQos_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TxtQos_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}