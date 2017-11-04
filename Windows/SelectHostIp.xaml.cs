using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    /// Interaction logic for SelectHostIp.xaml
    /// </summary>
    public partial class SelectHostIp : Window
    {

        public IPAddress hostIp;
        private RadioButton btns;
        public SelectHostIp(IPAddress[] ips)
        {
            
            InitializeComponent();
            foreach (var ip in ips)
            {
                var chk = new RadioButton
                {
                    Content = ip.ToString(),
                    Name = "RadioBtnIp",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9000")),
                    FontWeight = FontWeights.Bold
                };
                chk.Click += Chk_Click;
                MainGrid.Children.Insert(0, chk);
            }
        }

        private void Chk_Click(object sender, RoutedEventArgs e)
        {
            var chk = (RadioButton)sender;
            hostIp=IPAddress.Parse(chk.Content.ToString());
        }

        private void BtnConfirm_OnClick(object sender, RoutedEventArgs e)
        {
            // If checked. If unsure = false
            if (!btns?.IsChecked??false)
            {
                MessageBox.Show("No IPs selected.");
                return;
            }
            this.Close();
        }
    }
}
