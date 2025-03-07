using DataService.Entity;
using EquipmentSignalData.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlManualControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlManualControlPage : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<Valve> Valves { get; set; }
        Intetfaces.PLCRead pLCRead = new Intetfaces.PLCRead();
        public ICommand OpenValveCommand { get; }
        public ICommand CloseValveCommand { get; }
        public ICommand OpenSelectedValvesCommand { get; }
        public ICommand CloseSelectedValvesCommand { get; }
        public ICommand SelectAllCommand { get; }

        private bool isAllSelected;
        public bool IsAllSelected
        {
            get => isAllSelected;
            set
            {
                if (isAllSelected != value)
                {
                    isAllSelected = value;
                    OnPropertyChanged(nameof(IsAllSelected));
                    SelectAllValves(isAllSelected);
                }
            }
        }
        public UserControlManualControlPage()
        {
            InitializeComponent();
            //Valves = new ObservableCollection<Valve>{};
            Valves = App.SharedValves;

            OpenValveCommand = new RelayCommandT<Valve>(OpenValve);
            CloseValveCommand = new RelayCommandT<Valve>(CloseValve);
            OpenSelectedValvesCommand = new RelayCommand(_=>OpenSelectedValves());
            CloseSelectedValvesCommand = new RelayCommand(_=>CloseSelectedValves());
            SelectAllCommand = new RelayCommand(_=>SelectAllValves(IsAllSelected));
            this.DataContext = this;
        }
        private void OpenValve(Valve valve)
        {
            if (valve == null) return;

            // 获取当前所有已打开的阀门
            var openedValves = Valves.Where(v => v.IndicatorColor == "Green").Select(v => v.Name).ToList();

            // 读取互锁配置
            var interlockRules = LoadInterlockRulesFromXml("InterlockConfig.xml");

            // 检查是否与当前打开的阀门有互锁冲突
            if (HasInterlockConflict(valve.Name, openedValves, interlockRules))
            {
                MessageBox.Show($"无法打开阀门 {valve.Name}：与已打开的阀门触发互锁机制。", "互锁警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Valve valve1 = new XMLHelper().ReadNameXml(valve.Name);
            _=new ExcelLogger().LogOperationAsync(new OperationLog() { OperationName = "阀门开启" ,ValveName=valve.Name}); //写入日志
            pLCRead.ReadDate(valve1.IpAddress, valve1.Address,true);
            // 没有互锁冲突，打开阀门
            valve.IndicatorColor = "Green";
        }

        private void CloseValve(Valve valve)
        {
            if (valve != null)
            {
                Valve valve1 = new XMLHelper().ReadNameXml(valve.Name);
                pLCRead.ReadDate(valve1.IpAddress, valve1.Address, false);
                _ = new ExcelLogger().LogOperationAsync(new OperationLog() { OperationName = "阀门关闭", ValveName = valve.Name });
                valve.IndicatorColor = "Gray";
            }
        }

        private void OpenSelectedValves()
        {
            // 获取当前所有已打开的阀门
            var openedValves = Valves.Where(v => v.IndicatorColor == "Green").Select(v => v.Name).ToList();

            // 读取互锁配置
            var interlockRules = LoadInterlockRulesFromXml("InterlockConfig.xml");

            // 获取当前选择的阀门
            var selectedValves = Valves.Where(v => v.IsSelected).ToList();

            // 遍历每个被选中的阀门，逐个检查互锁冲突
            foreach (var valve in selectedValves)
            {
                var allOpenedValves = openedValves.Concat(selectedValves.Where(v => v.Name != valve.Name).Select(v => v.Name)).ToList();

                if (HasInterlockConflict(valve.Name, allOpenedValves, interlockRules))
                {
                    MessageBox.Show($"无法打开阀门：阀门 {valve.Name} 与其他阀门触发了互锁机制。", "互锁警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // 打开阀门
                valve.IndicatorColor = "Green";
                Valve valve1 = new XMLHelper().ReadNameXml(valve.Name);
                pLCRead.ReadDate(valve1.IpAddress, valve1.Address, true);
            }
        }

        private bool HasInterlockConflict(string valveName, List<string> activeValves, List<InterlockRule> interlockRules)
        {
            foreach (var rule in interlockRules)
            {
                if (rule.InterlockedValves.Contains(valveName) &&
                    rule.InterlockedValves.Any(activeValves.Contains))
                {
                    return true;
                }
            }
            return false;
        }

        private List<InterlockRule> LoadInterlockRulesFromXml(string filePath)
        {
            var interlockRules = new List<InterlockRule>();

            if (!File.Exists(filePath))
                return interlockRules;

            var xml = XElement.Load(filePath);
            foreach (var ruleElement in xml.Elements("Rule"))
            {
                var valveIds = ruleElement.Attribute("Valves").Value.Split(',').ToList();
                interlockRules.Add(new InterlockRule { InterlockedValves = valveIds });
            }

            return interlockRules;
        }
        private void SelectAllValves(bool isSelected)
        {
            foreach (var valve in Valves)
            {
                valve.IsSelected = isSelected;
            }
        }
        private void CloseSelectedValves()
        {
            foreach (var valve in Valves.Where(v => v.IsSelected))
            {
                Valve valve1 = new XMLHelper().ReadNameXml(valve.Name);
                pLCRead.ReadDate(valve1.IpAddress, valve1.Address, false);

                valve.IndicatorColor = "Gray";
                
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
