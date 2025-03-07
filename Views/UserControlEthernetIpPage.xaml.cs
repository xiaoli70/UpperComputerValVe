using EquipmentSignalData.Command;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace EquipmentSignalData.Views
{
    public partial class UserControlEthernetIpPage : UserControl,INotifyPropertyChanged
    {
        public ObservableCollection<DeviceConfig> Devices { get; set; } = new ObservableCollection<DeviceConfig>();
        public ICommand SaveAllCommand { get; }

        public UserControlEthernetIpPage()
        {
            InitializeComponent();

            // 初始化设备配置
            InitializeDevices();

            // 绑定保存全部命令
            SaveAllCommand = new RelayCommand(_ => SaveAll());

            // 加载配置
            LoadConfigurations();

            // 设置数据上下文
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void InitializeDevices()
        {
            Devices.Add(new DeviceConfig("ADAM6022", "192.168.0.1", "502"));
            Devices.Add(new DeviceConfig("ADAM6015-1", "192.168.0.2", "503"));
            Devices.Add(new DeviceConfig("ADAM6015-2", "192.168.0.3", "504"));
            Devices.Add(new DeviceConfig("CDG025D压力硅", "192.168.0.4", "505"));
            Devices.Add(new DeviceConfig("复合柜检测FPG550", "192.168.0.5", "506"));
        }

        private void SaveAll()
        {
            foreach (var device in Devices)
            {
                SaveConfiguration(device.DeviceName, device.IpAddress, device.Port);
            }
            MessageBox.Show("所有配置已保存！", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveConfiguration(string deviceName, string ip, string port)
        {
            string filePath = "Configurations.xml";
            XDocument doc;

            if (!File.Exists(filePath))
            {
                // 如果文件不存在，创建一个带根节点的 XML 文件
                doc = new XDocument(new XElement("Configurations"));
                doc.Save(filePath);
            }
            else
            {
                // 如果文件存在，加载文件
                doc = XDocument.Load(filePath);
            }

            // 查找或创建设备配置节点
            var deviceElement = doc.Root?.Element(deviceName);
            if (deviceElement == null)
            {
                deviceElement = new XElement(deviceName);
                doc.Root?.Add(deviceElement);
            }

            // 设置 IP 和端口值
            deviceElement.SetElementValue("IP", ip);
            deviceElement.SetElementValue("Port", port);

            // 保存更新后的 XML
            doc.Save(filePath);
        }

        private void LoadConfigurations()
        {
            string filePath = "Configurations.xml";

            if (File.Exists(filePath))
            {
                var doc = XDocument.Load(filePath);
                foreach (var device in Devices)
                {
                    var deviceElement = doc.Root?.Element(device.DeviceName);
                    if (deviceElement != null)
                    {
                        device.IpAddress = deviceElement.Element("IP")?.Value ?? device.IpAddress;
                        device.Port = deviceElement.Element("Port")?.Value ?? device.Port;
                    }
                }
            }
        }
    }

    public class DeviceConfig
    {
        public string DeviceName { get; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public ICommand SaveCommand { get; }

        public DeviceConfig(string deviceName, string defaultIp, string defaultPort)
        {
            DeviceName = deviceName;
            IpAddress = defaultIp;
            Port = defaultPort;

            // 初始化保存命令
            SaveCommand = new RelayCommand(_ => SaveConfiguration());
        }

        private void SaveConfiguration()
        {
            var filePath = "Configurations.xml";
            XDocument doc;

            if (!File.Exists(filePath))
            {
                doc = new XDocument(new XElement("Configurations"));
                doc.Save(filePath);
            }
            else
            {
                doc = XDocument.Load(filePath);
            }

            var deviceElement = doc.Root?.Element(DeviceName);
            if (deviceElement == null)
            {
                deviceElement = new XElement(DeviceName);
                doc.Root?.Add(deviceElement);
            }

            deviceElement.SetElementValue("IP", IpAddress);
            deviceElement.SetElementValue("Port", Port);
            doc.Save(filePath);

            MessageBox.Show($"{DeviceName} 配置已保存！", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
