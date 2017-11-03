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
        public string MqttIp;
        public string Username;
        public string Password;

        public ConfigPanel()
        {
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
            var hosts = Dns.GetHostEntry(TxtMQTTIP.Text).AddressList;
            var cleanedIp = TxtMQTTIP.Text.Trim();
            if (!IPAddress.TryParse(cleanedIp, out IPAddress newIp))
            {
                var result = new SelectHostIp(hosts).ShowDialog();
                MessageBox.Show("You need to enter a valid IP address or domain");
                return;
            }
            Console.WriteLine($"IP: {newIp}");
            MqttIp = cleanedIp;
            Username = TxtUsername.Text;
            Password = TxtPassword.Password;
            Close();
        }
    }
}