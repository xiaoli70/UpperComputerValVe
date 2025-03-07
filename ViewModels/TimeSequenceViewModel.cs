using DataService;
using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EquipmentSignalData.ViewModels
{

    public class TimeSequenceViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Item> items;
        private Item dragItem;

        private int dragIndex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Item> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ICommand DropCommand { get; }
        public ICommand DragCommand { get; }

        public TimeSequenceViewModel()
        {
            Items = new ObservableCollection<Item>();
            LoadConfig();

            
        }

        private void LoadConfig()
        {
            string itemstr = new ConfigService().LoadConfigKey("TimeSequence");
            string[] itemArray = itemstr.Split(",");

            foreach (var item in itemArray)
            {
                Items.Add(new Item { Name = item });
            }
        }

        public void SaveConfig()
        {
            string concatenatedNames = String.Join(",", Items.Select(i => i.Name));
            new ConfigService().UpdateConfigKey("TimeSequence", concatenatedNames);
        }



        public void OnItemMouseDown(Item clickedItem)
        {
            if (clickedItem != null)
            {
                dragItem = clickedItem;
                dragIndex = Items.IndexOf(dragItem as Item);
            }
        }

        public void OnItemDrop(int dropIndex)
        {
            if (dragItem != null)
            {
                Items.Remove(dragItem as Item);
                Items.Insert(dropIndex, dragItem as Item);
                new ConfigService().UpdateConfigKey("TimeSequence", string.Join(",", Items.Select(i => i.Name)));
            }
        }

        public void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Item)))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true; // 标记事件为已处理
        }


        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }


    public class Item
    {
        public string Name { get; set; }
        public string index { get; set; }
    }
}
