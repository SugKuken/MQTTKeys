using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mqtt_hotkeys_test
{
    class JsonConfigHelper
    {
        public List<PubBindingSettings> PubBindingSettings { get; set; }
        public List<SubBindingSettings> SubBindingSettings { get; set; }
    }
}
