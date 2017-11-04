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
        private bool _isIpConfigured;
        private MqttClient _mqttClient = new MqttClient("localhost"); /* = new MqttClient("192.168.0.6");*/
        private static ConnectionSettings _connectionConfig;

        public MainWindow()
        {
            // Check for configure file and load settings if exists
            _connectionConfig = LoadConnectionSettingsFromJson();
            string mqttIp = _connectionConfig.BrokerIp,
                user = _connectionConfig.MqttUser,
                pass = "";


            InitializeComponent();

            // Keep re-opening config panel til connected successfully.
            while (!_mqttClient.IsConnected)
                try
                {
                    if (!_isIpConfigured)
                    {
                        var cfg = new ConfigPanel(this);
                        cfg.ShowDialog();

                        _connectionConfig = cfg.ConnSettings;

                        // TODO: Replace with custom checkbox window
                        var mbox = MessageBox.Show("Save this username & password for future use?", "Save User", MessageBoxButton.YesNo);
                        if (mbox == MessageBoxResult.Yes)
                        {
                            _connectionConfig.MqttUser = user;
                            _connectionConfig.MqttPassword = pass;
                        }
                        mbox = MessageBox.Show("Save this IP for future use?", "Save IP", MessageBoxButton.YesNo);
                        if (mbox == MessageBoxResult.Yes)
                        {
                            _connectionConfig.BrokerIp = mqttIp;
                        }
                        SaveConnectionConfigToJson(_connectionConfig);
                    }

                    // If blank, try to connect anyways
                    ConnectToMqtt(_connectionConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show(ex.Message);
                    _isIpConfigured = false;
                }

            // Set title to current IP
            Title += $"  -  Connected to: {mqttIp}";

            // Load .json file of previous binds if exists
            LoadBindingsFromJson();
        }

        private ConnectionSettings LoadConnectionSettingsFromJson()
        {
            if (File.Exists("connectionconfig.json"))
            {
                var connectionConfig =
                    JsonConvert.DeserializeObject<ConnectionSettings>(File.ReadAllText("connectionconfig.json"));
                // True if IP is correctly configured
                if (connectionConfig.BrokerIp == null) connectionConfig.BrokerIp = "";
                _isIpConfigured = connectionConfig.BrokerIp.Trim() != "";
                return connectionConfig;
            }
            // If file doesn't exist
            _isIpConfigured = false;
            return new ConnectionSettings();
        }

        private void LoadBindingsFromJson()
        {
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

        private void ConnectToMqtt(ConnectionSettings connSettings)
        {
            if (_mqttClient.IsConnected)
            {
                _mqttClient.Disconnect();
            }
            Console.WriteLine($"Connecting with --\n" +
                              $"IP: {connSettings.BrokerIp}\n" +
                              $"User: {connSettings.MqttUser}\n" +
                              $"Pass: {connSettings.MqttPassword}");
            if (connSettings.BrokerIp.Trim() == "")
            {
                return;
            }
            _mqttClient = new MqttClient(connSettings.BrokerIp);

            try
            {
                var code = _mqttClient.Connect(Guid.NewGuid().ToString(), connSettings.MqttUser, connSettings.MqttPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.Message);
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

        public void SaveConnectionConfigToJson(ConnectionSettings connectionConfig)
        {
            if (connectionConfig != null)
            {
                File.WriteAllText("connectionconfig.json", JsonConvert.SerializeObject(connectionConfig));
            }
            else
            {
                Console.WriteLine("Invalid conn config");
            }
        }

        public void SaveBindingsToJson()
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
            var cfg = new ConfigPanel(this);
            cfg.ShowDialog();
            ReloadSettings(cfg.ConnSettings);
        }

        public void ReloadSettings(ConnectionSettings connSettings)
        {
            ConnectToMqtt(connSettings);
        }
    }
}