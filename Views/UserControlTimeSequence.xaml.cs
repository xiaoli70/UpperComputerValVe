using DataService;
using EquipmentSignalData.Models;
using EquipmentSignalData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlTimeSequence.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlTimeSequence : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Item> items;

        public ObservableCollection<Item> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        private object dragItem;
        private int dragIndex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public UserControlTimeSequence()
        {
            InitializeComponent();
            Items = new ObservableCollection<Item>();
            string itemstr = new ConfigService().LoadConfigKey("TimeSequence");
            string[] item = itemstr.Split(",");
            for (int i = 0; i < item.Length; i++)
            {
                Items.Add(new Item { Name = item[i] });
            }
            DataContext = this; // 设置数据上下文
            //DataContext = new TimeSequenceViewModel(); // 设置数据上下文
        }




        private void ListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            var clickedItem = GetItemAtMousePosition(listView, e.GetPosition(listView));
            if (clickedItem != null)
            {
                dragItem = clickedItem;
                dragIndex = Items.IndexOf(dragItem as Item);
            }
        }

        private void ListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && dragItem != null)
            {
                DragDrop.DoDragDrop(listViewItems, dragItem, DragDropEffects.Move);
            }
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Item)))
            {
                var listView = sender as ListView;
                // 使用鼠标位置来获取目标索引
                var mousePos = e.GetPosition(listView);
                var dropIndex = -1;

                for (int i = 0; i < listView.Items.Count; i++)
                {
                    var item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                    if (item != null)
                    {
                        // 判断鼠标是否在当前项的上方
                        if (mousePos.Y < item.TranslatePoint(new Point(0, 0), listView).Y + item.ActualHeight / 2)
                        {
                            dropIndex = i;
                            break;
                        }
                    }
                }

                // 如果 dropIndex 仍然是 -1，表示放在最后
                if (dropIndex == -1)
                {
                    dropIndex = listView.Items.Count - 1;
                }

                // 移动项
                Items.Remove(dragItem as Item);
                Items.Insert(dropIndex, dragItem as Item);
                new ConfigService().UpdateConfigKey("TimeSequence", String.Join(",", Items.Select(i => i.Name)));
            }
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Item)))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        private Item GetItemAtMousePosition(ListView listView, Point mousePosition)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                var item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null && item.IsMouseOver)
                {
                    return (Item)listView.Items[i];
                }
            }
            return null;
        }





        protected void OnPropertyChanged(string propertyName) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    
}
