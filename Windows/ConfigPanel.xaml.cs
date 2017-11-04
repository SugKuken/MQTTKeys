using System;
using System.Net;
using System.Windows;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    ///     Interaction logic for ConfigPanel.xaml
    /// </summary>
    public partial class ConfigPanel : Window
    {
        public ConnectionSettings ConnSettings;
        public MainWindow _parent;


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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _parent.BusyIndicatorMainWindow.IsBusy = true;
            try
            {
                var cleanedDomain = TxtMQTTIP.Text.Trim();
                var hosts = Dns.GetHostEntry(cleanedDomain).AddressList;
                var cleanedIp = TxtMQTTIP.Text.Trim();
                Console.WriteLine(cleanedIp);
                if (!IPAddress.TryParse(cleanedIp, out IPAddress newIp))
                {
                    var result = new SelectHostIp(hosts);
                    result.ShowDialog();
                    ConnSettings.BrokerIp = result.hostIp.ToString();
                    TxtMQTTIP.Text = ConnSettings.BrokerIp;
                }
                else
                {
                    ConnSettings.BrokerIp = cleanedIp;
                }
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
    }
}