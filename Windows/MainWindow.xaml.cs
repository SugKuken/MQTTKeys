using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using mqtt_hotkeys_test.Controls;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using MessageBox = System.Windows.MessageBox;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isIpConfigured;
        public static MqttClient MqttClient = new MqttClient("localhost");
        public ConnectionSettings ConnectionConfig;
        public static List<MqttTopic> MqttTopics = new List<MqttTopic>();
        public static List<SubscribeHelper> ActiveSubscriptions = new List<SubscribeHelper>();
        public static List<Key> AllHotKeys = new List<Key>();

        public MainWindow()
        {
            InitializeComponent();
            this.Show();
            // Check for configure file and load settings if exists
            ConnectionConfig = LoadConnectionSettingsFromJson();
            var i = 0;

            // Til i fix it
            _isIpConfigured = File.Exists("connectionconfig.json");
            // Keep re-opening config panel til connected successfully.
            while (!MqttClient.IsConnected)
            {
                if (!_isIpConfigured)
                {
                    var cfg = new ConfigPanel(this);
                    cfg.ShowDialog();

                    ConnectionConfig = cfg.ConnSettings;
                    Console.WriteLine(ConnectionConfig.ToString());
                    SaveConnectionConfigToJson(ConnectionConfig);
                }
                try
                {
                    Console.WriteLine($"attempt {i++}");
                    if (_isIpConfigured)
                    {
                        try
                        {
                            ConnectionConfig = LoadConnectionSettingsFromJson();
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
                    ConnectToMqtt(ConnectionConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    // TODO: Change all MessageBoxes to custom popup
                    MessageBox.Show(ex.Message);
                    _isIpConfigured = false;
                }

            }

            // Set title to current IP
            Title += $"  -  Connected to: {ConnectionConfig.BrokerIp}";

            // Load .json file of previous binds if exists
            LoadBindingsFromJson();
            MqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
        }

        //**************************//
        // Config Load/Save Methods //
        //**************************//
        private void LoadBindingsFromJson()
        {
            if (File.Exists("bindingconfig.json"))
            {
                var allBindings =
                    JsonConvert.DeserializeObject<JsonConfigHelper>(File.ReadAllText("bindingconfig.json"));
                var pubBindings = allBindings.PubBindingSettings;
                var subBindings = allBindings.SubBindingSettings;
                // If the json file is empty
                pubBindings.Reverse();
                subBindings.Reverse();
                if (pubBindings.Count != 0)
                {
                    foreach (var binding in pubBindings)
                    {
                        var rowControl = new PubHotKeyRowControl { Binding = binding };
                        PubStackPanel.Children.Insert(0, rowControl);
                        rowControl.UpdateUi();
                    }
                }
                if (subBindings.Count != 0) 
                {
                    foreach (var binding in subBindings)
                    {
                        var rowControl = new SubHotKeyRowControl { Binding = binding };
                        SubStackPanel.Children.Insert(0, rowControl);
                        rowControl.UpdateUi();
                    }
                }

            }
        }

        private ConnectionSettings LoadConnectionSettingsFromJson()
        {
            if (File.Exists("connectionconfig.json"))
            {
                var connectionConfig =
                    JsonConvert.DeserializeObject<ConnectionSettings>(File.ReadAllText("connectionconfig.json"));

                if (connectionConfig == null)
                {
                    _isIpConfigured = false;
                    File.Delete("connectionconfig.json");
                    return new ConnectionSettings();
                }

                // True if IP is correctly configured
                if (connectionConfig.BrokerIp == null) connectionConfig.BrokerIp = "";
                _isIpConfigured = connectionConfig.BrokerIp.Trim() != "";
                return connectionConfig;
            }
            // If file doesn't exist
            _isIpConfigured = false;
            return new ConnectionSettings();
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
            var listOfPubBindingConfigs = new List<PubBindingSettings>();
            var listOfSubBindingConfigs = new List<SubBindingSettings>();
            foreach (var child in PubStackPanel.Children.OfType<PubHotKeyRowControl>())
            {
                var binding = child.Binding;
                // If default topic/message
                if (binding == null)
                    break;
                binding.Message = child.TxtMessage.Text;
                binding.Topic = child.TxtTopic.Text;
                binding.Qos = child.TxtQos.Value ?? 2;
                listOfPubBindingConfigs.Add(binding);
            }
            foreach (var child in SubStackPanel.Children.OfType<SubHotKeyRowControl>())
            {
                var binding = child.Binding;
                // If default topic/message
                if (binding == null)
                    break;
                binding.SubTopic = child.TxtTopic.Text;
                binding.TriggerMessage = child.TxtTrigger.Text;
                binding.Qos = child.TxtQos.Value ?? 2;
                binding.PubTopic = child.TxtReplyTopic.Text;
                binding.ReplyMessage = child.TxtReplyPayload.Text;
                listOfSubBindingConfigs.Add(binding);
            }
            var jsonConfigBoy = new JsonConfigHelper
            {
                PubBindingSettings = listOfPubBindingConfigs,
                SubBindingSettings = listOfSubBindingConfigs
            };
            //File.WriteAllText("bindingconfig.json");
            File.WriteAllText("bindingconfig.json", JsonConvert.SerializeObject(jsonConfigBoy));
        }

        public void ReloadSettings(ConnectionSettings connSettings)
        {
            try
            {
                ConnectToMqtt(connSettings);
            }
            catch (Exception ex)
            {
                // TODO: Change all MessageBoxes to custom popup
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex);
            }
        }

        //*********************//
        // Mqtt Helper Methods //
        //*********************//
        public void ConnectToMqtt(ConnectionSettings connSettings)
        {
            if (MqttClient.IsConnected)
            {
                MqttClient.Disconnect();
            }
            Console.WriteLine( "Connecting with --\n" +
                              $"IP: {connSettings.BrokerIp}\n");
            // Sensitive info, do not debuge with it!!
            //$"User: {connSettings.MqttUser}\n" +
            //$"Pass: {connSettings.MqttPassword}");
            if (connSettings.BrokerIp.Trim() == "")
            {
                return;
            }
            MqttClient = new MqttClient(connSettings.BrokerIp);

            var code = MqttClient.Connect(Guid.NewGuid().ToString(), connSettings.MqttUser, connSettings.MqttPassword);
        }

        public static void AddMqttSub(SubscribeHelper subHelper)
        {
            // TODO: Check if necessary
            MqttTopics = MqttTopics
                .GroupBy(x => x.Topic)
                .Select(x => x.First())
                .ToList();
            MqttClient.Unsubscribe(MqttTopics.Select(x => x.Topic).ToArray());

            MqttClient.Subscribe(MqttTopics.Select(x => x.Topic).ToArray(),
                MqttTopics.Select(x => x.QosLevel).ToArray());
            MqttTopics.ForEach(Console.WriteLine);
            ActiveSubscriptions.Add(subHelper);
        }

        //***************//
        // Misc. Methods //
        //***************//
        public void RemovePubHotKeyAtIndex(int index)
        {
            this.PubStackPanel.Children.RemoveAt(index);
        }

        //*******************************//
        // Window Element Event Handlers //
        //*******************************//
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SaveBindingsToJson();
            Environment.Exit(0);
        }

        private void BtnAddHotKey_OnClick(object sender, RoutedEventArgs e)
        {
            if (TabControlMainWindow.SelectedIndex == 0)
            {
                var rowControl = new PubHotKeyRowControl();
                PubStackPanel.Children.Insert(PubStackPanel.Children.Count - 1, rowControl);
            }
            else if (TabControlMainWindow.SelectedIndex == 1)
            {
                var rowControl = new SubHotKeyRowControl();
                SubStackPanel.Children.Insert(SubStackPanel.Children.Count - 1, rowControl);
            }

        }

        private void MenuItemResetBinds_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Change all MessageBoxes to custom popup
            var mbox = MessageBox.Show("Are you sure?", "Delete", MessageBoxButton.YesNo);
            if (mbox != MessageBoxResult.Yes) return;

            var toRemove = new List<PubHotKeyRowControl>();
            foreach (var child in PubStackPanel.Children.OfType<PubHotKeyRowControl>())
                if ((child.TxtMessage.Text != "") & (child.TxtTopic.Text != ""))
                    toRemove.Add(child);
            foreach (var pubHotKeyRowControl in toRemove)
                PubStackPanel.Children.Remove(pubHotKeyRowControl);
            PubStackPanel.UpdateLayout();
            SaveBindingsToJson();
        }

        private void MenuItemSaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Change all MessageBoxes to custom popup
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

        private void MenuItemMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void HotKeyRowControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //*********************//
        // MQTT Event Handlers //
        //*********************//
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Mqtt handler fired");
            foreach (var subBinding in ActiveSubscriptions)
            {
                Console.WriteLine($"sub topic: {subBinding.SubTopic}\n" +
                                  $"trig msg: {subBinding.TriggerMessage}");
                if (e.Topic == subBinding.SubTopic &&
                    !e.DupFlag &&
                    Encoding.UTF8.GetString(e.Message) == subBinding.TriggerMessage)
                {
                    Console.WriteLine($"Message received on topic [{e.Topic}]: {Encoding.UTF8.GetString(e.Message)}\n" +
                                      $"DupFlag: {e.DupFlag}\n" +
                                      $"QoS: {e.QosLevel}");

                    VirtualKeyCode vKey = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(subBinding.HotKey);
                    // Handle and press hotkey
                    var keyCodes = new List<VirtualKeyCode> { vKey };

                    foreach (var subBindingModifierKey in subBinding.modifierKeys) 
                    {
                        Console.WriteLine(subBindingModifierKey.ToString());
                    }
                    Console.WriteLine(keyCodes.FirstOrDefault());
                    // TODO: When firing mqtt from keystroke, it will continually retrigger. (Potentially fixed?)
                    new InputSimulator().Keyboard.ModifiedKeyStroke(subBinding.modifierKeys, keyCodes);


                    if (subBinding.ReplyMessage.Trim() != "")
                    {
                        if (string.Equals(subBinding.TriggerMessage, subBinding.ReplyMessage,
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            // TODO: Change all MessageBoxes to custom popup
                            MessageBox.Show("Reply payload and trigger payload cannot be the same!");
                            return;
                        }
                        var topic = subBinding.PubTopic.Trim() == "" ? subBinding.SubTopic.Trim() : subBinding.PubTopic.Trim();
                        MqttClient.Publish(topic,
                            Encoding.UTF8.GetBytes(subBinding.ReplyMessage),
                            byte.Parse(subBinding.Qos.ToString()),
                            false);
                    }

                }
            }
        }
    }
}