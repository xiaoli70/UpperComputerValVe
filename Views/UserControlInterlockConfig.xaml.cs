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
using System.Xml.Serialization;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlInterlockConfig.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlInterlockConfig : UserControl
    {
        public ObservableCollection<Valve> Valves { get; set; }
        public ObservableCollection<InterlockRule> InterlockRules { get; set; }

        public RelayCommand CreateInterlockCommand { get; set; }
        public RelayCommand SaveConfigCommand { get; set; }
        public RelayCommand LoadConfigCommand { get; set; }
        public RelayCommand DeleteCommand { get; }
        private InterlockRule selectedGroup;
        public InterlockRule SelectedGroup
        {
            get => selectedGroup;
            set
            {
                if (selectedGroup != value)
                {
                    selectedGroup = value;
                    OnPropertyChanged(nameof(SelectedGroup));
                    //AssignValveToGroupCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public UserControlInterlockConfig()
        {
            InitializeComponent();

            Valves = new ObservableCollection<Valve>();
            InterlockRules = new ObservableCollection<InterlockRule>();
            CreateInterlockCommand = new RelayCommand(CreateInterlock, CanCreateInterlock);
            SaveConfigCommand = new RelayCommand(SaveConfig);
            LoadConfigCommand = new RelayCommand(_=>LoadConfig());
            DeleteCommand = new RelayCommand(_ => DeleteSelectedGroup(), _ => CanDelete());
            // 初始化54个阀门
            Valves = App.SharedValves;
            LoadConfig();
            this.DataContext = this;
        }
        private bool CanCreateInterlock(object parameter)
        {
            return Valves.Count(v => v.IsSelected) > 1;
        }

        private void CreateInterlock(object parameter)
        {
            var selectedValves = Valves.Where(v => v.IsSelected).Select(v => v.Name).ToList();
            InterlockRules.Add(new InterlockRule { InterlockedValves = selectedValves });

            // 清空选择状态
            foreach (var valve in Valves) valve.IsSelected = false;
        }

        private void DeleteSelectedGroup()
        {
            if (SelectedGroup != null)
            {
                InterlockRules.Remove(SelectedGroup);
                SelectedGroup = null; // 可选：清空选中项
            }
        }

        private bool CanDelete()
        {
            return SelectedGroup != null; // 只有在有选中项时才允许删除
        }
        private void SaveConfig(object parameter)
        {
            XElement root = new XElement("Interlocks",
                InterlockRules.Select(r =>
                    new XElement("Rule", new XAttribute("Valves", string.Join(",", r.InterlockedValves)))));

            root.Save("InterlockConfig.xml");
        }

        private void LoadConfig()
        {
            InterlockRules.Clear();
            var xml = XElement.Load("InterlockConfig.xml");
            foreach (var ruleElement in xml.Elements("Rule"))
            {
                //var valveIds = ruleElement.Attribute("Valves").Value.Split(',').Select(int.Parse).ToList();
                var valveIds = ruleElement.Attribute("Valves").Value.Split(',').ToList();
                InterlockRules.Add(new InterlockRule { InterlockedValves = valveIds });
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class InterlockRule
    {
        public List<string> InterlockedValves { get; set; } = new List<string>();
        public string DisplayText
        {
            get { return "\uD83D\uDD12 " + string.Join(", ", InterlockedValves); }
        }
    }
}

