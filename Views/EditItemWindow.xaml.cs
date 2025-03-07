using EquipmentSignalData.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using DataService.Entity;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// EditItemWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditItemWindow : Window
    {
        public ItemModel Item { get; private set; }

        public EditItemWindow(ItemModel? item)
        {
            InitializeComponent();

            // 创建项的副本以避免直接修改原始数据，直到用户点击保存
            if (item != null) { 
                Item = new ItemModel
                {
                    Index = item.Index,
                    Name = item.Name,
                    Unit = item.Unit,
                    Type = item.Type
                };

            
            }
            this.DataContext = Item;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 可以在这里添加验证逻辑
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
