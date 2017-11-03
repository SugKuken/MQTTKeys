using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using mqtt_hotkeys_test.Properties;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly bool _isIpConfigured;
        private readonly MqttClient _mqttClient = new MqttClient("localhost"); /* = new MqttClient("192.168.0.6");*/

        public MainWindow()
        {
            var mqttIp = (string) Settings.Default["BrokerIp"];
            string user = (string)Settings.Default["MqttUser"], 
                   pass = "";
            if (mqttIp.Contains(" "))
                _isIpConfigured = true;
            InitializeComponent();
            while (!_mqttClient.IsConnected)
                try
                {
                    if (!_isIpConfigured)
                    {
                        var cfg = new ConfigPanel();
                        cfg.ShowDialog();
                        mqttIp = cfg.MqttIp;
                        user = cfg.Username;
                        pass = cfg.Password;
                        if (mqttIp == "") Environment.Exit(0);
                        var mbox = MessageBox.Show("Save this username for future use?", "Save User", MessageBoxButton.YesNo);
                        if (mbox == MessageBoxResult.Yes)
                        {
                            Settings.Default["MqttUser"] = mqttIp;
                            Settings.Default.Save();
                        }
                    }
                    // TODO: Add username and password to config panel.
                    // If blank, try to connect anyways
                    _mqttClient = new MqttClient(mqttIp);
                    var code = _mqttClient.Connect(Guid.NewGuid().ToString(), user, pass);
                    if (!_isIpConfigured)
                    {
                        var mbox = MessageBox.Show("Save this IP for future use?", "Save IP", MessageBoxButton.YesNo);
                        if (mbox == MessageBoxResult.Yes)
                        {
                            Settings.Default["BrokerIp"] = mqttIp;
                            Settings.Default.Save();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            Title += $"  -  Connected to: {mqttIp}";

            // Load .json file of previous binds if exists
            if (File.Exists("bindingconfig.json"))
            {
                var bindings =
                    JsonConvert.DeserializeObject<List<BindingSettings>>(File.ReadAllText("bindingconfig.json"));
                bindings.Reverse();
                foreach (var binding in bindings)
                {
                    var rowControl = new HotKeyRowControl {Binding = binding};
                    MainStackPanel.Children.Insert(0, rowControl);
                    rowControl.UpdateUi();
                }
            }
        }

        private void BtnAddThing_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainStackPanel.Children.OfType<HotKeyRowControl>().Last().TxtMessage.Text == "" ||
                MainStackPanel.Children.OfType<HotKeyRowControl>().Last().TxtTopic.Text == "")
                return;
            var rowControl = new HotKeyRowControl();
            MainStackPanel.Children.Insert(MainStackPanel.Children.Count - 1, rowControl);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SaveBindingsToJson();
            Environment.Exit(0);
        }

        private void SaveBindingsToJson()
        {
            var listOfBindingConfigs = new List<BindingSettings>();
            foreach (var child in MainStackPanel.Children.OfType<HotKeyRowControl>())
            {
                var binding = child.Binding;
                // If default topic/message
                if (binding == null)
                    break;
                binding.Message = child.TxtMessage.Text;
                binding.Topic = child.TxtTopic.Text;
                binding.Qos = child.TxtQos.Value ?? 2;
                listOfBindingConfigs.Add(binding);
            }
            File.WriteAllText("bindingconfig.json", JsonConvert.SerializeObject(listOfBindingConfigs));
        }

        private void MenuItemResetBinds_OnClick(object sender, RoutedEventArgs e)
        {
            var mbox = MessageBox.Show("Are you sure?", "Delete", MessageBoxButton.YesNo);
            if (mbox != MessageBoxResult.Yes) return;

            var toRemove = new List<HotKeyRowControl>();
            foreach (var child in MainStackPanel.Children.OfType<HotKeyRowControl>())
                if ((child.TxtMessage.Text != "") & (child.TxtTopic.Text != ""))
                    toRemove.Add(child);
            foreach (var hotKeyRowControl in toRemove)
                MainStackPanel.Children.Remove(hotKeyRowControl);
            MainStackPanel.UpdateLayout();
            SaveBindingsToJson();
        }

        private void MenuItemSaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var mbox = MessageBox.Show("Are you sure?\nThis will overwrite and previous binding configs.", "Delete",
                MessageBoxButton.YesNo);
            if (mbox != MessageBoxResult.Yes) return;
            SaveBindingsToJson();
        }

        private void MenuItemClose_OnClick(object sender, RoutedEventArgs e)
        {
            SaveBindingsToJson();
            Environment.Exit(0);
        }

        private void MenuItemConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var cfg = new ConfigPanel();
            cfg.ShowDialog();
            var mqttIp = cfg.MqttIp;
        }
    }
}