using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using uPLibrary.Networking.M2Mqtt;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for HotKeyRowControl.xaml
    /// </summary>
    public partial class HotKeyRowControl : UserControl
    {
        public BindingSettings Binding;

        public HotKeyRowControl()
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
            var window = new SelectHotKey { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                var modKeysString = window.ModKeys.ToString();
                var modKeysForConfig = modKeysString;

                var hotKeyLetter = window.HotKey.ToString();
                modKeysString = CleanModifierKeysString(modKeysString);
                TxtHotKey.Text = $"{modKeysString} + {window.HotKey}";
                SetUpHotkey(window.HotKey, window.ModKeys);

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

        private void SetUpHotkey(Key key, ModifierKeys modKeys)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(TxtTopic.Text + TxtHotKey.Text, key, modKeys, HandleHotKey);
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
            if (TxtMessage.Text == "" || TxtTopic.Text == "")
            {
                MessageBox.Show("Topic or message cannot be blank", "Invalid selections", MessageBoxButton.OK);
                return;
            }
            MainWindow._mqttClient.Publish(TxtTopic.Text,
                       Encoding.UTF8.GetBytes(TxtMessage.Text),
                       byte.Parse(TxtQos.Text),
                       false);
        }

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
    }
}