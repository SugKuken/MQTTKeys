using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public static MqttClient _mqttClient = new MqttClient("localhost");
        public ConnectionSettings _connectionConfig;
        public static List<MqttTopic> MqttTopics = new List<MqttTopic>();

        public MainWindow()
        {
            InitializeComponent();
            this.Show();
            // Check for configure file and load settings if exists
            _connectionConfig = LoadConnectionSettingsFromJson();
            var i = 0;

            // Til i fix it
            _isIpConfigured = File.Exists("connectionconfig.json");
            // Keep re-opening config panel til connected successfully.
            while (!_mqttClient.IsConnected)
            {
                if (!_isIpConfigured)
                {
                    var cfg = new ConfigPanel(this);
                    cfg.ShowDialog();

                    _connectionConfig = cfg.ConnSettings;
                    Console.WriteLine(_connectionConfig.ToString());
                    SaveConnectionConfigToJson(_connectionConfig);
                }
                try
                {
                    Console.WriteLine($"attempt {i++}");
                    if (_isIpConfigured)
                    {
                        try
                        {
                            _connectionConfig = LoadConnectionSettingsFromJson();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        // TODO: Replace with custom checkbox window
                        //var mbox = MessageBox.Show("Save this username & password for future use?", "Save User", MessageBoxButton.YesNo);
                        //if (mbox == MessageBoxResult.Yes)
                        //{

                        //}
                        //mbox = MessageBox.Show("Save this IP for future use?", "Save IP", MessageBoxButton.YesNo);
                        //if (mbox == MessageBoxResult.Yes)
                        //{

                        //}

                    }
                    // TODO: Show loading icon
                    ConnectToMqtt(_connectionConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show(ex.Message);
                    _isIpConfigured = false;
                }

            }

            // Set title to current IP
            Title += $"  -  Connected to: {_connectionConfig.BrokerIp}";

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
                    var rowControl = new PubHotKeyRowControl {Binding = binding};
                    MainStackPanel.Children.Insert(0, rowControl);
                    rowControl.UpdateUi();
                }
            }
        }

        public void ConnectToMqtt(ConnectionSettings connSettings)
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

            var code = _mqttClient.Connect(Guid.NewGuid().ToString(), connSettings.MqttUser, connSettings.MqttPassword);
        }

        private void BtnAddThing_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("clicc");
            if (MainStackPanel.Children.OfType<PubHotKeyRowControl>().Last().TxtMessage.Text == "" ||
                MainStackPanel.Children.OfType<PubHotKeyRowControl>().Last().TxtTopic.Text == "")
                return;
            var rowControl = new PubHotKeyRowControl();
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
            foreach (var child in MainStackPanel.Children.OfType<PubHotKeyRowControl>())
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

            var toRemove = new List<PubHotKeyRowControl>();
            foreach (var child in MainStackPanel.Children.OfType<PubHotKeyRowControl>())
                if ((child.TxtMessage.Text != "") & (child.TxtTopic.Text != ""))
                    toRemove.Add(child);
            foreach (var pubHotKeyRowControl in toRemove)
                MainStackPanel.Children.Remove(pubHotKeyRowControl);
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
            try
            {
                ConnectToMqtt(connSettings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex);
            }
        }

        private void MenuItemMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void HotKeyRowControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}