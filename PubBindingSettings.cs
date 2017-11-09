namespace mqtt_hotkeys_test
{
    public class PubBindingSettings
    {
        // Single Letter
        public string HotKey { get; set; }

        // Comma separated list of modifier keys
        public string ModKeys { get; set; }

        // Mqtt message settings
        public string Topic { get; set; }

        public string Message { get; set; }
        public int Qos { get; set; }
    }
}