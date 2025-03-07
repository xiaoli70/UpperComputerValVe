using DataService.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;

namespace EquipmentSignalData.Command
{
    internal class XMLHelper
    {
        public Valve ReadNameXml(string valueName)
        {
            XDocument xmlDoc = XDocument.Load("ValveConfig.xml");


            var valve = xmlDoc.Descendants("Valve")
                .Where(valve => (string)valve.Element("ProcessNum") == valueName)
                .Select(valve => new Valve
                {
                    Name = (string)valve.Element("ProcessNum"),
                    Address = (string)valve.Element("Address"),
                    Region = (string)valve.Element("Region"),
                    DataType = (string)valve.Element("DataType"),
                    IpAddress = (string)valve.Parent.Parent.Element("Ip")
                })
                .FirstOrDefault() ?? new Valve(); 


            return valve;
        }

        public (string IpAddress, string Port) GetDeviceConfiguration(string deviceName)
        { 
            var doc = XDocument.Load("Configurations.xml");
            var deviceElement = doc.Root?.Element(deviceName);

            if (deviceElement != null)
            {
                string ipAddress = deviceElement.Element("IP")?.Value ?? "192.168.0.1"; // 默认值
                string port = deviceElement.Element("Port")?.Value ?? "502"; // 默认值
                return (ipAddress, port);
            }
            return ("192.168.0.1", "502"); // 返回默认值，如果未找到设备
        }

        public string ReadXmlToFilowGroup(string GroupName) {

            XDocument xmlDoc = XDocument.Load("ValveFlowMeterGroup.xml");
            
            var valveNames = xmlDoc.Descendants("ValveGroup")
                                .Where(vg => vg.Element("GroupName").Value == GroupName)
                                .SelectMany(vg => vg.Descendants("Valve").Select(v => v.Element("Name").Value))
                                .ToList();

            string result = string.Join(", ", valveNames);
            return result;
        }

        public FlowMeter GetFlowMeter(string Name)
        {
            XDocument xmlDoc = XDocument.Load("FlowGroup.xml");

            // 查找 Name 为 "流量计 2" 的 FlowMeter 对象
            var flowMeter = xmlDoc.Descendants("FlowMeter")
                                .Where(fm => (string)fm.Element("Name") == Name)
                                .Select(fm => new FlowMeter
                                {
                                    Id = (int)fm.Element("Id"),
                                    Name = (string)fm.Element("Name"),
                                    SelectedValveIslands = (string)fm.Element("SelectedValveIslands"),
                                    AlarmValue = (double?)fm.Element("AlarmValue")
                                })
                                .FirstOrDefault();

            return flowMeter;

        }
        public List<FlowMeter> GetFlowMeterIP()
        {
            XDocument xmlDoc = XDocument.Load("FlowGroup.xml");

            // 查找 Name 为 "流量计 2" 的 FlowMeter 对象
            var flowMeter = xmlDoc.Descendants("FlowMeter")

                                .Select(fm => new FlowMeter
                                {
                                    
                                    Ip = (string)fm.Element("Ip"),
                                    Name = (string)fm.Element("Name"),
                                   
                                })

                                .ToList();

            return flowMeter;

        }
        public List<FlowMeter> GetFlowMeterI2P()
        {
            XDocument xmlDoc = XDocument.Load("FlowGroup.xml");

            // 查找 Name 为 "流量计 2" 的 FlowMeter 对象
            var flowMeter = xmlDoc.Descendants("FlowMeter")
                               
                                .Select(fm => new FlowMeter
                                {
                                    Id = (int)fm.Element("Id"),
                                    Ip = (string)fm.Element("Ip"),
                                    Name = (string)fm.Element("Name"),
                                    SelectedValveIslands = (string)fm.Element("SelectedValveIslands"),
                                    AlarmValue = (double?)fm.Element("AlarmValue")
                                })
                                .ToList();

            return flowMeter;

        }

    }
}
