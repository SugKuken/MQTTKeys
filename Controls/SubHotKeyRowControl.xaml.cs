using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using static WindowsInput.InputSimulator;
using NHotkey;
using NHotkey.Wpf;
using uPLibrary.Networking.M2Mqtt;
using Xceed.Wpf.AvalonDock.Controls;
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
        public SubBindingSettings Binding;

        public SubHotKeyRowControl()
        {
            InitializeComponent();
        }

        public void UpdateUi()
        {
            if (Binding != null)
            {
                TxtTopic.Text = Binding.SubTopic;
                TxtTrigger.Text = Binding.TriggerMessage;
                TxtReplyTopic.Text = Binding.PubTopic;
                TxtReplyPayload.Text = Binding.ReplyMessage;
                if (Enum.TryParse(Binding.ModKeys, true, out ModifierKeys modifierKeys) &&
                    Enum.TryParse(Binding.HotKey, true, out Key keyLtr))
                {
                    var modKeysString = CleanModifierKeysString(modifierKeys.ToString());
                    TxtHotKey.Text = $"{modKeysString} + {keyLtr}";
                    //SetUpKeyPress(keyLtr, modifierKeys);
                }
            }
        }

        private void BtnSub_OnClick(object sender, RoutedEventArgs e)
        {
            if (TxtTopic.Text == "" ||
                TxtTrigger.Text == "" ||
                TxtHotKey.Text == "")
            {
                // TODO: Change all MessageBoxes to custom popup
                MessageBox.Show("You need to set a topic, trigger payload, and hotkey!");
                return;
            }

            MainWindow.MqttTopics.Add(new MqttTopic
            {
                QosLevel = byte.Parse(TxtQos.Text),
                Topic = TxtTopic.Text
            });

            // TODO: Check if necessary
            MainWindow._mqttClient.Unsubscribe(MainWindow.MqttTopics.Select(x => x.Topic).ToArray());

            MainWindow._mqttClient.Subscribe(MainWindow.MqttTopics.Select(x=>x.Topic).ToArray(), 
                                             MainWindow.MqttTopics.Select(x=>x.QosLevel).ToArray());

            // TODO: Do this on init instead of per control?
            MainWindow._mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;

        }

        private void _mqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            // TODO: Figure out why this fires multiple times?
            this.Dispatcher.Invoke(() =>
            {
                //if (MainWindow.AllHotKeys.Any(Keyboard.IsKeyDown))
                //{
                //    Console.WriteLine("Key is still pressed, ignoring");
                //    return;
                //}
                if (e.Topic == TxtTopic.Text && 
                    !e.DupFlag && 
                    Encoding.UTF8.GetString(e.Message) == TxtTrigger.Text)
                {
                    Console.WriteLine($"Message received on topic [{e.Topic}]: {Encoding.UTF8.GetString(e.Message)}\n" +
                                      $"DupFlag: {e.DupFlag}\n" +
                                      $"QoS: {e.QosLevel}");

                    VirtualKeyCode vKey = (VirtualKeyCode) KeyInterop.VirtualKeyFromKey(hotKey);
                    // Handle and press hotkey
                    var keyCodes = new List<VirtualKeyCode> {vKey};

                    new InputSimulator().Keyboard.ModifiedKeyStroke(modifierKeys, keyCodes);
                    if (TxtReplyPayload.Text.Trim() != "")
                    {
                        if (string.Equals(TxtTrigger.Text, TxtReplyPayload.Text,
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageBox.Show("Reply payload and trigger payload cannot be the same!");
                            return;
                        }
                        var topic = TxtReplyTopic.Text.Trim() == "" ? TxtTopic.Text.Trim() : TxtReplyTopic.Text.Trim();
                        MainWindow._mqttClient.Publish(topic,
                            Encoding.UTF8.GetBytes(TxtReplyPayload.Text),
                            byte.Parse(TxtQos.Text),
                            false);
                    }
                }
            });
        }

        private void TxtHotKey_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var window = new SelectHotKey(true) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var modKeysString = window.ModKeys.ToString();
                var modKeysForConfig = modKeysString;
                var modKeysList = modKeysForConfig.Split(new [] {", "}, StringSplitOptions.None);
                modifierKeys.Clear();
                foreach (var keyStr in modKeysList)
                {
                    switch (keyStr.ToLower())
                    {
                        // TODO: (If not keyboard interface) Add L&R Control/shift support (Tyler add buttons)
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
                // Create object for config
                Binding = new SubBindingSettings
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

        //private void SetUpKeyPress(Key key, ModifierKeys modKeys)
        //{
        //    try
        //    {
        //        // TODO handler

        //    }
        //    catch (Exception ex)
        //    {
        //        TxtHotKey.Text = "Double click to set key";
        //        MessageBox.Show(ex.Message);
        //        Console.WriteLine(ex);
        //    }
        //}

        private void HandleHotKey(object sender, HotkeyEventArgs e)
        {
            PublishMqttMessage();
        }

        private void PublishMqttMessage()
        {
            if (TxtReplyPayload.Text == "" || TxtTopic.Text == "")
            {
                MessageBox.Show("Topic or message cannot be blank", "Invalid selections", MessageBoxButton.OK);
                return;
            }
            MainWindow._mqttClient.Publish(TxtTopic.Text,
                       Encoding.UTF8.GetBytes(TxtReplyPayload.Text),
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


        private void BtnRemoveHotkey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var parent = Window.GetWindow(this);
                var stackPanel = parent.FindLogicalChildren<StackPanel>().FirstOrDefault(x => string.Equals(x.Name, "SubStackPanel", StringComparison.InvariantCultureIgnoreCase));
                var index = stackPanel.Children.IndexOf(this);
                if (stackPanel.Children.Count == 1) return;
                stackPanel.Children.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}