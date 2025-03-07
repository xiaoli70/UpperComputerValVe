using DataService;
using DataService.Entity;
using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using EquipmentSignalData.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EquipmentSignalData.ViewModels
{
    public class ConfigViewModel : INotifyPropertyChanged
    {

        public ConfigService ConfigService;
        private SettingItem selectedSetting;

        public ObservableCollection<ValveIsland> Valvess { get; set; }

        public SettingItem SelectedSetting
        {
            get { return selectedSetting; }
            set
            {
                selectedSetting = value;
                OnPropertyChanged(nameof(SelectedSetting));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public ConfigViewModel()
        {
            Valvess = new ObservableCollection<ValveIsland>();
            int index = 0;
            //for (int i = 0; i < 4; i++)
            //{

            //    var Valveis = new ValveIsland { IP = $"192.168.10{i}" };
            //    for (int j = 0; j < 16; j++)
            //    {
            //        Valve valve = new Valve();
            //        valve.Id = index;
            //        valve.Name = "Valve " + index;
            //        valve.Address = "20000";
            //        valve.DataType = "INT";
            //        valve.Region = "R";
            //        Valveis.Valves.Add(valve);
            //        index++;
            //    }

            //    Valvess.Add(Valveis); ;
            //}

            SaveCommand = new RelayCommand(SaveConfig);
            LoadGroups();
            //for (int i = 0; i < 4; i++)
            //{

            //    var Valveis = new ValveIsland {  };
            //    for (int j = 0; j < 16; j++)
            //    {
            //        Valve valve = Valvess[i].Valves[j];
            //        //valve.Id = index;
            //        valve.ProcessNum = index < 9 ? $"V10{index + 1}" : $"V1{index + 1}";

            //        index++;
            //    }

            //    ; ;
            //}
        }

        private void LoadGroups()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ValveConfig));

                using (var reader = new StreamReader("ValveConfig.xml"))
                {
                    var valveGroupsContainer = (ValveConfig)serializer.Deserialize(reader);
                    Valvess.Clear();

                    // 复制加载的阀门组
                    foreach (var group in valveGroupsContainer.ValveGroups)
                    {
                        Valvess.Add(group);
                    }
                }
                //AssignValveToGroupCommand.RaiseCanExecuteChanged();
                //MessageBox.Show("编组已从 XML 文件加载。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}");
            }
        }
        // 保存配置文件
        private void SaveConfig(object obj)
        {
            try
            {
                var valveGroupsContainer = new ValveConfig { ValveGroups = new List<ValveIsland>(Valvess) };

                var serializer = new XmlSerializer(typeof(ValveConfig));

                using (var writer = new StreamWriter("ValveConfig.xml"))
                {
                    serializer.Serialize(writer, valveGroupsContainer);
                }

                MessageBox.Show("已保存到 XML 文件。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }

        //// 增加设置项
        //private void AddSetting()
        //{
        //    Settings.Add(new SettingItem { Key = "NewKey", Value = "NewValue" });
        //}

        //// 删除设置项
        //private void DeleteSetting(object obj)
        //{

        //    if (obj is SettingItem setting && setting != null)
        //    {
        //        Settings.Remove(setting);
        //    }
        //}

        public ICommand LoadCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand AddSettingCommand { get; set; }
        public ICommand DeleteSettingCommand { get; set; }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}
