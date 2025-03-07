using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataService.Entity
{
    [XmlRoot("ValveGroups")]
    public class ValveGroupsContainer
    {
        [XmlElement("ValveGroup")]
        public List<ValveGroup> ValveGroups { get; set; } = new();
    }
}
