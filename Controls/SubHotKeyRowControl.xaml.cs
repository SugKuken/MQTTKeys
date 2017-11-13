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