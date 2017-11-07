using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using uPLibrary.Networking.M2Mqtt;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for ConfigPanel.xaml
    /// </summary>
    public partial class ConfigPanel : Window
    {
        public ConnectionSettings ConnSettings;
        public MainWindow _parent;
        public string workingIp = "";


        public ConfigPanel(MainWindow parent)
        {
            ConnSettings = new ConnectionSettings();
            _parent = parent;
            InitializeComponent();
        }

        private void ConfigPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            Left = mainWindow.Left + (mainWindow.Width - ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - ActualHeight) / 2;
        }

        private void BtnContinue_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.BusyIndicatorMainWindow.IsBusy = true;
            try
            {
                Console.WriteLine(workingIp);
                if (workingIp == "")
                {
                    MessageBox.Show("Please enter and test a working IP or domain first.");
                    return;
                }
                ConnSettings.BrokerIp = workingIp;
                ConnSettings.MqttUser = TxtUsername.Text;
                ConnSettings.MqttPassword = TxtPassword.Password;

                _parent.SaveConnectionConfigToJson(ConnSettings);
                _parent._connectionConfig = ConnSettings;
                Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnTestConn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TxtMQTTIP.Text == "")
                {
                    TxtMQTTIP.Text = "localhost";
                }

                MqttClient mqttTest;
                if (TxtMQTTPort.Text.Trim() != "" && TxtMQTTPort.Text != "1883")
                {
                    Console.WriteLine(TxtMQTTPort.Text.Trim());
                    var port = int.Parse(TxtMQTTPort.Text.Trim());
                    mqttTest = new MqttClient(workingIp, port,
                                              // TODO: implement these:
                                              false, null, null, MqttSslProtocols.None);
                }
                else
                {
                    mqttTest = new MqttClient(TxtMQTTIP.Text);
                }
                mqttTest.Connect(Guid.NewGuid().ToString(), TxtUsername.Text, TxtPassword.Password);
                mqttTest.Disconnect();

                var successGreen = ColorFromHex("#00ff00");

                TxtMQTTIP.Foreground = successGreen;
                TxtMQTTPort.Foreground = successGreen;
                BtnTestConn.Background = successGreen;

                BtnContinue.IsEnabled = true;

                workingIp = TxtMQTTIP.Text.Trim();
            }
            catch (Exception ex)
            {
                var errorRed = ColorFromHex("#ff0000");
                Console.WriteLine(ex);
                TxtMQTTIP.Foreground = errorRed;
                TxtMQTTPort.Foreground = errorRed;
                BtnTestConn.Background = errorRed;

                BtnContinue.IsEnabled = false;
                workingIp = "";
            }
        }

        private SolidColorBrush ColorFromHex(string hexCode)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexCode));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void TxtMQTTPort_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }
    }
}