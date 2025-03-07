using EquipmentSignalData.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using EquipmentSignalData.Models;
using System.IO;
using System.Xml.Serialization;
using DataService.Entity;

namespace EquipmentSignalData.ViewModels
{
    public class ValveViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Valve> AllValves { get; set; } = new();
        public ObservableCollection<Valve> AllValves2 { get; set; } = new();
        public ObservableCollection<ValveGroup> ValveGroups { get; set; } = new();
        public ObservableCollection<ValveGroup> ValveGroups2 { get; set; } = new();

        private ValveGroup selectedGroup;
        public ValveGroup SelectedGroup
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

        private ValveGroup selectedGroup2;
        public ValveGroup SelectedGroup2
        {
            get => selectedGroup2;
            set
            {
                if (selectedGroup2 != value)
                {
                    selectedGroup2 = value;
                    OnPropertyChanged(nameof(SelectedGroup2));
                    //AssignValveToGroupCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand AddGroupCommand { get; }
        public RelayCommandT<Valve> AssignValveToGroupCommand { get; }
        public RelayCommand SaveGroupsCommand { get; }
        public RelayCommand LoadGroupsCommand { get; }
        public RelayCommandT<Valve> RemoveValveFromGroupCommand { get; }

        public RelayCommand DeleteCommand2 { get; }
        public RelayCommand AddGroupCommand2 { get; }
        public RelayCommandT<Valve> AssignValveToGroupCommand2 { get; }
        public RelayCommand SaveGroupsCommand2 { get; }
        public RelayCommand LoadGroupsCommand2 { get; }
        public RelayCommandT<Valve> RemoveValveFromGroupCommand2 { get; }
        public ValveViewModel()
        {
            AddGroupCommand = new RelayCommand(_ => AddGroup());
            AssignValveToGroupCommand = new RelayCommandT<Valve>(AssignValveToGroup, CanAssignValveToGroup);
            SaveGroupsCommand = new RelayCommand(_ => SaveGroups());
            LoadGroupsCommand = new RelayCommand(_ => LoadGroups());
            RemoveValveFromGroupCommand = new RelayCommandT<Valve>(RemoveValveFromGroup, CanRemoveValve);

            //AddGroupCommand2 = new RelayCommand(_ => AddGroup2());
            AssignValveToGroupCommand2 = new RelayCommandT<Valve>(AssignValveToGroup2, CanAssignValveToGroup2);
            SaveGroupsCommand2 = new RelayCommand(_ => SaveGroups2());
            LoadGroupsCommand2 = new RelayCommand(_ => LoadGroups2());
            RemoveValveFromGroupCommand2 = new RelayCommandT<Valve>(RemoveValveFromGroup2, CanRemoveValve2);

            for (int i = 0; i < 13; i++)
            {
                ValveGroups2.Add(new ValveGroup { GroupId = i, GroupName = i < 9 ? $"FC10{i + 1}" : $"FC1{i + 1}" });
            }
            
            DeleteCommand = new RelayCommand(_=>DeleteSelectedGroup(),_=>CanDelete());

            AllValves = App.SharedValves;
            AllValves2 = App.SharedValves;
            LoadGroups();
            LoadGroups2();

        }
        private bool CanRemoveValve(Valve? valve)
        {
            // 确保 valve 和 SelectedGroup 都不为空
            return valve != null && SelectedGroup != null && SelectedGroup.Valves.Contains(valve);
        }
        private void DeleteSelectedGroup()
        {
            if (SelectedGroup != null)
            {
                ValveGroups.Remove(SelectedGroup);
                SelectedGroup = null; // 可选：清空选中项
            }
        }

        private bool CanDelete()
        {
            return SelectedGroup != null; // 只有在有选中项时才允许删除
        }
        private void RemoveValveFromGroup(Valve? valve)
        {
            if (SelectedGroup != null && valve != null)
            {
                SelectedGroup.Valves.Remove(valve);
            }
        }
        private bool CanAssignValveToGroup(Valve valve)
        {
            //bool boll = valve != null && !ValveGroups.Any(group => group.Valves.Contains(valve));
            // 如果 valve 已经在某个组中，则返回 false
            return valve != null && !ValveGroups.Any(group => group.Valves.Any(val=>val.Name==valve.Name)); //Contains(valve)
        }

        private void AssignValveToGroup(Valve valve)
        {
            if (SelectedGroup == null || valve == null) return;

            // 从其他组中移除阀门
            foreach (var group in ValveGroups)
            {
                if (group.Valves.Contains(valve))
                {
                    group.Valves.Remove(valve);
                    valve.GroupId = 0;
                }
            }

            // 将阀门添加到当前选定的组
            SelectedGroup.Valves.Add(valve);
            valve.GroupId = SelectedGroup.GroupId;
            //AssignValveToGroupCommand.RaiseCanExecuteChanged();
        }
        private void InitializeValves()
        {

            AllValves = App.SharedValves;
            //for (int i = 1; i <= 54; i++)
            //{
            //    AllValves.Add(new Valve { Id = i, Name = $"Valve {i}" });
            //}
        }

        private void AddGroup()
        {
            int maxGroupId = ValveGroups.Any() ? ValveGroups.Max(g => g.GroupId) : 0;
            ValveGroups.Add(new ValveGroup { GroupId = maxGroupId + 1, GroupName = $"Group {maxGroupId + 1}" });
        }

        private void SaveGroups()
        {
            // 这里编写保存到XML的逻辑
            try
            {
                var valveGroupsContainer = new ValveGroupsContainer { ValveGroups = new List<ValveGroup>(ValveGroups) };

                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var writer = new StreamWriter("ValveGroups.xml"))
                {
                    serializer.Serialize(writer, valveGroupsContainer);
                }
                
                MessageBox.Show("编组已保存到 XML 文件。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }

        private void LoadGroups()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var reader = new StreamReader("ValveGroups.xml"))
                {
                    var valveGroupsContainer = (ValveGroupsContainer)serializer.Deserialize(reader);
                    ValveGroups.Clear();

                    // 复制加载的阀门组
                    foreach (var group in valveGroupsContainer.ValveGroups)
                    {
                        ValveGroups.Add(group);
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

//流量计
        private bool CanRemoveValve2(Valve? valve)
        {
            // 确保 valve 和 SelectedGroup 都不为空
            return valve != null && SelectedGroup2 != null && SelectedGroup2.Valves.Contains(valve);
        }
        //private void DeleteSelectedGroup2()
        //{
        //    if (SelectedGroup != null)
        //    {
        //        ValveGroups.Remove(SelectedGroup);
        //        SelectedGroup = null; // 可选：清空选中项
        //    }
        //}

        
        //private bool CanDelete2()
        //{
        //    return SelectedGroup2 != null; // 只有在有选中项时才允许删除
        //}
        private void RemoveValveFromGroup2(Valve? valve)
        {
            if (SelectedGroup2 != null && valve != null)
            {
                SelectedGroup2.Valves.Remove(valve);
            }
        }
        private bool CanAssignValveToGroup2(Valve valve)
        {
            //bool boll = valve != null && !ValveGroups.Any(group => group.Valves.Contains(valve));
            // 如果 valve 已经在某个组中，则返回 false
            return valve != null && !ValveGroups2.Any(group => group.Valves.Any(val => val.Name == valve.Name)); //Contains(valve)
        }

        private void AssignValveToGroup2(Valve valve)
        {
            if (SelectedGroup2 == null || valve == null) return;

            // 从其他组中移除阀门
            foreach (var group in ValveGroups2)
            {
                if (group.Valves.Contains(valve))
                {
                    group.Valves.Remove(valve);
                    valve.GroupId = 0;
                }
            }

            // 将阀门添加到当前选定的组
            SelectedGroup2.Valves.Add(valve);
            valve.GroupId = SelectedGroup2.GroupId;
            //AssignValveToGroupCommand.RaiseCanExecuteChanged();
        }
       

        //private void AddGroup2()
        //{
        //    int maxGroupId = ValveGroups.Any() ? ValveGroups.Max(g => g.GroupId) : 0;
        //    ValveGroups.Add(new ValveGroup { GroupId = maxGroupId + 1, GroupName = $"Group {maxGroupId + 1}" });
        //}

        private void SaveGroups2()
        {
            // 这里编写保存到XML的逻辑
            try
            {
                var valveGroupsContainer = new ValveGroupsContainer { ValveGroups = new List<ValveGroup>(ValveGroups2) };

                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var writer = new StreamWriter("ValveFlowMeterGroup.xml"))
                {
                    serializer.Serialize(writer, valveGroupsContainer);
                }

                MessageBox.Show("编组已保存到 XML 文件。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }

        private void LoadGroups2()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var reader = new StreamReader("ValveFlowMeterGroup.xml"))
                {
                    var valveGroupsContainer = (ValveGroupsContainer)serializer.Deserialize(reader);
                    ValveGroups2.Clear();
                
                    // 复制加载的阀门组
                    foreach (var group in valveGroupsContainer.ValveGroups)
                    {
                        
                        ValveGroups2.Add(group);
                        
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


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    
}
