using System.Collections.Generic;
using System.Windows.Input;
using WindowsInput.Native;

namespace mqtt_hotkeys_test
{
    public class SubscribeHelper
    {
        // Single Letter
        public Key HotKey { get; set; }
    
        // Comma separated list of modifier keys
        public List<VirtualKeyCode> modifierKeys = new List<VirtualKeyCode>();
    
        // Mqtt message settings
        public string SubTopic { get; set; }
    
        public string TriggerMessage { get; set; }
    
        public string PubTopic { get; set; }
        public string ReplyMessage { get; set; }
        public int Qos { get; set; }

    }
}