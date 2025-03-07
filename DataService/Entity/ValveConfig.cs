using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataService.Entity
{
    [XmlRoot("ValveConfig")]
    public class ValveConfig
    {
        [XmlElement("ValveGroup")]
        public List<ValveIsland> ValveGroups { get; set; } = new();
    }
}
