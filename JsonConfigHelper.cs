using System.Collections.Generic;

namespace mqtt_hotkeys_test
{
    class JsonConfigHelper
    {
        public List<PubBindingSettings> PubBindingSettings { get; set; }
        public List<SubBindingSettings> SubBindingSettings { get; set; }
    }
}
