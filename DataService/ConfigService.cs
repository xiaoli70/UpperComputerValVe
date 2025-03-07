using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataService
{
    public class ConfigService
    {
        private static ConfigService? instance;
        private readonly string filePath = "ConfigViewModel.xml";

        public static ConfigService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigService();
                }
                return instance;
            }
        }
        public string LoadConfigKey(string Key)
        {
            if (File.Exists(filePath))
            {
                var doc = XDocument.Load(filePath);

                foreach (var element in doc.Descendants("Setting"))
                {
                    var key = element.Element("Key").Value;
                    var value = element.Element("Value").Value;
                    if (Key == key)
                        return value;
                }
            }

            return "";
        }

        public void UpdateConfigKey(string key, string newValue)
        {
            if (File.Exists(filePath))
            {
                var doc = XDocument.Load(filePath);

                var setting = doc.Descendants("Setting")
                                 .FirstOrDefault(e => e.Element("Key")?.Value == key);

                if (setting != null)
                {
                    setting.Element("Value")?.SetValue(newValue);
                    doc.Save(filePath);
                }
            }
        }
    

    public ObservableCollection<SettingItem> LoadConfig()
        {
            var settings = new ObservableCollection<SettingItem>();

            if (File.Exists(filePath))
            {
                var doc = XDocument.Load(filePath);

                foreach (var element in doc.Descendants("Setting"))
                {
                    var key = element.Element("Key").Value;
                    var value = element.Element("Value").Value;
                    settings.Add(new SettingItem { Key = key, Value = value });
                }
            }

            return settings;
        }

        public void SaveConfig(ObservableCollection<SettingItem> settings)
        {
            var doc = new XDocument(new XElement("Settings",
                settings.Select(s =>
                    new XElement("Setting",
                        new XElement("Key", s.Key),
                        new XElement("Value", s.Value)))));

            doc.Save(filePath);
        }
    }

    public class SettingItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
