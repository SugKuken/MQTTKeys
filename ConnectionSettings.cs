using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mqtt_hotkeys_test
{
    public class ConnectionSettings
    {
        public string BrokerIp { get; set; }
        public string MqttUser { get; set; }
        public string MqttPassword { get; set; }

        public ConnectionSettings()
        {
            BrokerIp = "";
            MqttUser = "";
            MqttPassword = "";
        }

        public ConnectionSettings(string ip, string user, string pass)
        {
            BrokerIp = ip;
            MqttUser = user;
            MqttPassword = pass;
        }
    }
}
