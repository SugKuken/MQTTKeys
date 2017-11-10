using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using NHotkey;
using Xceed.Wpf.AvalonDock.Controls;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace mqtt_hotkeys_test.Controls
{
    /// <summary>
    ///     Interaction logic for HotKeyRowControl.xaml
    /// </summary>
    public partial class SubHotKeyRowControl : UserControl
    {

        public List<VirtualKeyCode> ModifierKeys = new List<VirtualKeyCode>();
        public Key HotKey = 0;
        public SubBindingSettings Binding;
        public SubscribeHelper SubHelper;

        public SubHotKeyRowControl()
        {
            InitializeComponent();
        }

        public void UpdateUi()
        {
            try
            {
                if (Binding != null)
                {
                    TxtTopic.Text = Binding.SubTopic;
                    TxtTrigger.Text = Binding.TriggerMessage;
                    TxtReplyTopic.Text = Binding.PubTopic;
                    TxtReplyPayload.Text = Binding.ReplyMessage;
                    if (Enum.TryParse(Binding.HotKey, true, out Key keyLtr))
                    {
                        HotKey = keyLtr;
                        var modKeysString = CleanModifierKeysString(Binding.ModKeys);
                        foreach (var keyStr in modKeysString.Split(new[] { ", " }, StringSplitOptions.None))
                        {
                            switch (keyStr.ToLower())
                            {
                                // TODO: (If not keyboard interface) Add L&R Control/shift/Win support (Tyler add buttons)
                                case "control":
                                    ModifierKeys.Add(VirtualKeyCode.CONTROL);
                                    break;
                                case "windows":
                                    ModifierKeys.Add(VirtualKeyCode.LWIN);
                                    break;
                                case "alt":
                                    ModifierKeys.Add((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(Key.LeftAlt));
                                    break;
                                case "shift":
                                    ModifierKeys.Add(VirtualKeyCode.SHIFT);
                                    break;
                            }
                        }
                        BtnHotKey.Content = $"{modKeysString} + {keyLtr}";
                        //SetUpKeyPress(keyLtr, modifierKeys);
                    }
                    // Resub if loaded from config
                    BtnSub_OnClick(this, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void BtnSub_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Subscribed");
            if (TxtTopic.Text == "" ||
                TxtTrigger.Text == "" ||
                // TODO: Check if this still works (changed from textbox.text -> button.content)
                (string)BtnHotKey.Content == "")
            {
                // TODO: Change all MessageBoxes to custom popup
                MessageBox.Show("You need to set a topic, trigger payload, and hotkey!");
                return;
            }

            Windows.MainWindow.MqttTopics.Add(new MqttTopic
            {
                QosLevel = byte.Parse(TxtQos.Text),
                Topic = TxtTopic.Text
            });

            SubHelper = new SubscribeHelper
            {
                HotKey = HotKey,
                modifierKeys = ModifierKeys,
                PubTopic = TxtReplyTopic.Text,
                Qos = TxtQos.Value ?? 0,
                ReplyMessage = TxtReplyPayload.Text,
                SubTopic = TxtTopic.Text,
                TriggerMessage = TxtTrigger.Text

            };

            Windows.MainWindow.AddMqttSub(SubHelper);


            //MainWindow._mqttClient.Unsubscribe(MainWindow.MqttTopics.Select(x => x.Topic).ToArray());

            //MainWindow._mqttClient.Subscribe(MainWindow.MqttTopics.Select(x=>x.Topic).ToArray(), 
            //                                 MainWindow.MqttTopics.Select(x=>x.QosLevel).ToArray());

            // TODO: Do this on init instead of per control? (Move to MainWindow.xaml.cs?)
            //MainWindow._mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;

        }

        //TODO: Move to MainWindow.xaml.cs? (DONE, testing)
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

                    VirtualKeyCode vKey = (VirtualKeyCode) KeyInterop.VirtualKeyFromKey(HotKey);
                    // Handle and press hotkey
                    var keyCodes = new List<VirtualKeyCode> {vKey};

                    new InputSimulator().Keyboard.ModifiedKeyStroke(ModifierKeys, keyCodes);
                    if (TxtReplyPayload.Text.Trim() != "")
                    {
                        if (string.Equals(TxtTrigger.Text, TxtReplyPayload.Text,
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            MessageBox.Show("Reply payload and trigger payload cannot be the same!");
                            return;
                        }
                        var topic = TxtReplyTopic.Text.Trim() == "" ? TxtTopic.Text.Trim() : TxtReplyTopic.Text.Trim();
                        Windows.MainWindow.MqttClient.Publish(topic,
                            Encoding.UTF8.GetBytes(TxtReplyPayload.Text),
                            byte.Parse(TxtQos.Text),
                            false);
                    }
                }
            });
        }

        private void BtnHotKey_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var window = new Windows.SelectHotKey(true) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var modKeysString = window.ModKeys.ToString();
                var modKeysForConfig = modKeysString;
                var modKeysList = modKeysForConfig.Split(new [] {", "}, StringSplitOptions.None);
                ModifierKeys.Clear();
                foreach (var keyStr in modKeysList)
                {
                    switch (keyStr.ToLower())
                    {
                        // TODO: (If not keyboard interface) Add L&R Control/shift/Win support (Tyler add buttons)
                        case "control":
                            ModifierKeys.Add(VirtualKeyCode.CONTROL);
                            break;
                        case "windows":
                            ModifierKeys.Add(VirtualKeyCode.LWIN);
                            break;
                        case "alt":
                            ModifierKeys.Add((VirtualKeyCode)KeyInterop.VirtualKeyFromKey(Key.LeftAlt));
                            break;
                        case "shift":
                            ModifierKeys.Add(VirtualKeyCode.SHIFT);
                            break;
                    }
                }
                HotKey = window.HotKey;
                var hotKeyLetter = window.HotKey.ToString();
                modKeysString = CleanModifierKeysString(modKeysString);
                BtnHotKey.Content = $"{modKeysString}+{window.HotKey}";
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
            modKeysString = modKeysString.Replace(",", "+");
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
            Windows.MainWindow.MqttClient.Publish(TxtTopic.Text,
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