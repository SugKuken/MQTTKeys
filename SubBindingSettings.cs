namespace mqtt_hotkeys_test
{
    public class SubBindingSettings
    {
        // Single Letter
        public string HotKey { get; set; }

        // Comma separated list of modifier keys
        public string ModKeys { get; set; }

        // Mqtt message settings
        public string SubTopic { get; set; }
        public string TriggerMessage { get; set; }

        public string PubTopic { get; set; }
        public string ReplyMessage { get; set; }
        public int Qos { get; set; }
    }
}