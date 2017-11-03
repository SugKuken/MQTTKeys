using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace mqtt_hotkeys_test.Windows
{
    /// <summary>
    /// Interaction logic for SelectHostIp.xaml
    /// </summary>
    public partial class SelectHostIp : Window
    {
        public SelectHostIp(IPAddress[] ips)
        {
            InitializeComponent();
            foreach (var ip in ips)
            {
                var chk = new RadioButton()
                {
                    Content = ip.ToString(), Name = "RadioBtnIp"
                };
                MainGrid.Children.Insert(0, chk);
            }
        }
    }
}
