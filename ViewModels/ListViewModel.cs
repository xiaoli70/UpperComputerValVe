using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataService.Entity;

namespace EquipmentSignalData.ViewModels
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private ItemModel _selectedItem;

        // 公开的ObservableCollection, 用于绑定到View
        public ObservableCollection<ItemModel> Items { get; set; }

        // 选择的列表项
        public ItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        // 添加命令
        public ICommand AddCommand { get; set; }

        public ICommand EditCommand { get; set; }

        public ICommand DeleteCommand { get; set; }


        private void EditItem(object obj)
        {
            // Edit logic
        }

        private void DeleteItem(object obj)
        {
            // Delete logic
            if (obj is ItemModel item)
            {
                // 逻辑：确保 obj 是 ItemModel 类型
                Items.Remove(item); // 从集合中移除该项目
            }
        }

        public ListViewModel()
        {
            // 初始化集合
            Items = new ObservableCollection<ItemModel>();

            // 初始化命令
            AddCommand = new RelayCommand(AddItem);

            EditCommand = new RelayCommand(EditItem);

            DeleteCommand = new RelayCommand(DeleteItem);

            // 加入一些初始数据
            Items = new ObservableCollection<ItemModel>
        {
            new ItemModel { Index = 1, Name = "电源电压", Model = "VW100", Unit = "kv", Type = "ushort" },
            new ItemModel { Index = 2, Name = "电流", Model = "VW200", Unit = "A", Type = "float" },
            new ItemModel { Index = 3, Name = "温度", Model = "VW300", Unit = "°C", Type = "float" }
        };
        }

        // 添加项的方法
        private void AddItem(object parameter)
        {
            int newId = Items.Count + 1;
            Items.Add(new ItemModel { Index = newId, Name = "温度", Model = "VW300", Unit = "°C", Type = "float" });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
