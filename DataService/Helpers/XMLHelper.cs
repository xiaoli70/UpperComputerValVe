using DataService.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataService.Helpers
{
    internal class XMLHelper
    {
        public Valve ReadNameXml(string valueName)
        {
            XDocument xmlDoc = XDocument.Load("ValveConfig.xml");


            var valve = xmlDoc.Descendants("Valve")
                .Where(valve => (string)valve.Element("Name") == "Valve64")
                .Select(valve => new Valve
                {
                    Name = (string)valve.Element("Name"),
                    Address = (string)valve.Element("Address"),
                    Region = (string)valve.Element("Region"),
                    DataType = (string)valve.Element("DataType"),
                })
                .FirstOrDefault() ?? new Valve();


            return valve;
        }
    }
}
