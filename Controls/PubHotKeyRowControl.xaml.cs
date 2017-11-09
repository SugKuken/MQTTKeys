using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NHotkey;
using NHotkey.Wpf;
using uPLibrary.Networking.M2Mqtt;
using Xceed.Wpf.AvalonDock.Controls;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for HotKeyRowControl.xaml
    /// </summary>
    public partial class PubHotKeyRowControl : UserControl
    {
        public PubBindingSettings Binding;

        public PubHotKeyRowControl()
        {
            InitializeComponent();
        }

        public void UpdateUi()
        {
            if (Binding != null)
            {
                TxtTopic.Text = Binding.Topic;
                TxtMessage.Text = Binding.Message;
                TxtQos.Value = Binding.Qos;
                TxtQos.Text = Binding.Qos.ToString();
                if (Enum.TryParse(Binding.ModKeys, true, out ModifierKeys modifierKeys) &&
                    Enum.TryParse(Binding.HotKey, true, out Key keyLtr))
                {
                    var modKeysString = CleanModifierKeysString(modifierKeys.ToString());
                    TxtHotKey.Text = $"{modKeysString} + {keyLtr}";
                    SetUpHotkey(keyLtr, modifierKeys);
                }
            }
        }

        private void BtnTest_OnClick(object sender, RoutedEventArgs e)
        {
            PublishMqttMessage();
        }

        private void TxtHotKey_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // "Garbage collect" when user sets a different key
            try
            {
                MainWindow.AllHotKeys.Remove((Key)Enum.Parse(typeof(Key), Binding.HotKey));
                Console.WriteLine($"Removing {(Key)Enum.Parse(typeof(Key), Binding.HotKey)}");
            }
            catch (Exception ex)
            {
                // Suppressed
            }

            var window = new SelectHotKey(false) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {

                var modKeysString = window.ModKeys.ToString();
                var modKeysForConfig = modKeysString;
                
                var hotKeyLetter = window.HotKey.ToString();
                modKeysString = CleanModifierKeysString(modKeysString);
                TxtHotKey.Text = $"{modKeysString} + {window.HotKey}";
                SetUpHotkey(window.HotKey, window.ModKeys);

                // Create object for config
                Binding = new PubBindingSettings
                {
                    HotKey = hotKeyLetter,
                    ModKeys = modKeysForConfig
                };

                MainWindow.AllHotKeys.Add(window.HotKey);
            }
        }

        private static string CleanModifierKeysString(string modKeysString)
        {
            modKeysString = modKeysString.Replace("Windows", "Win");
            modKeysString = modKeysString.Replace(",", " +");
            return modKeysString;
        }

        private void SetUpHotkey(Key key, ModifierKeys modKeys)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(TxtTopic.Text + TxtHotKey.Text, key, modKeys, true, HandleHotKey);
            }
            catch (Exception ex)
            {
                TxtHotKey.Text = "";
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex);
            }
        }

        private void HandleHotKey(object sender, HotkeyEventArgs e)
        {
            //this.Dispatcher.Invoke(PublishMqttMessage);
            Console.WriteLine("Hotkey handler fired");
            PublishMqttMessage();
        }

        private void PublishMqttMessage()
        {
            if (TxtMessage.Text == "" || TxtTopic.Text == "")
            {
                MessageBox.Show("Topic or message cannot be blank", "Invalid selections", MessageBoxButton.OK);
                return;
            }

            // TODO: implement retained pubs?
            try
            {
                MainWindow._mqttClient.Publish(TxtTopic.Text,
                   Encoding.UTF8.GetBytes(TxtMessage.Text),
                   byte.Parse(TxtQos.Text),
                   false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // TODO: click and drag to set QoS
        public Point mouseStartPos;
        public Point mouseEndPos;

        private void TxtQos_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseStartPos = e.GetPosition(TxtQos);
            Console.WriteLine(mouseStartPos);
        }

        private void TxtQos_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseEndPos = e.GetPosition(TxtQos);
            Console.WriteLine(mouseEndPos);
        }

        private void BtnRemoveHotkey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var parent = Window.GetWindow(this);
                var stackPanel = parent.FindLogicalChildren<StackPanel>().FirstOrDefault(x => string.Equals(x.Name, "PubStackPanel", StringComparison.InvariantCultureIgnoreCase));
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