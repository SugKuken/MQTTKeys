namespace mqtt_hotkeys_test
{
    public class MqttTopic
    {
        public string Topic { get; set; }
        public byte QosLevel { get; set; }

        public override string ToString()
        {
            return Topic;
        }
    }
}
