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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlMenuItem.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlMenuItem : UserControl
    {
        MainWindow _context;
        ItemMenu _itemMenu;
        public UserControlMenuItem(ItemMenu itemMenu, MainWindow context)
        {

            InitializeComponent();
            _context = context;
            _itemMenu=itemMenu;
            ExpanderMenu.Visibility = itemMenu.SubItems == null ? Visibility.Collapsed : Visibility.Visible;
            ListViewItemMenu.Visibility = itemMenu.SubItems == null ? Visibility.Visible : Visibility.Collapsed;

            this.DataContext = itemMenu;


            //if (itemMenu.Header == "首页")
            //{
            //    ExpanderMenu.IsExpanded = true;
            //}
            if (itemMenu.SubItems != null && itemMenu.SubItems.Any())
            {
                var homeItem = itemMenu.SubItems.FirstOrDefault(s => s.Name == "首页");
                if (homeItem != null)
                {
                    ListViewMenu.SelectedItem = homeItem;  
                    _context.SwitchScreen(homeItem);    
                }
            }
            

        }


        private void ExpanderMenu_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander != null && expander.Header != null && expander.Header.ToString() == "首页")
            {

                expander.IsExpanded = false;

                OpenHomePage();
            }
        }

        private void OpenHomePage()
        {
            if (_itemMenu.SubItems != null)
            {
                var homeItem = _itemMenu.SubItems.FirstOrDefault(s => s.Name == "首页");
                if (homeItem != null)
                {
                    _context.SwitchScreen(homeItem); 
                }
                else
                {
                    MessageBox.Show("无法找到首页");
                }
            }
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var listView = sender as ListView;  // sender 是 ListView
            

            var subItem = listView?.SelectedItem as SubItem;  // 从 ListView 获取 SelectedItem
            if (listView?.Items.Count == 2)
            {
                listView.SelectedItem=null;
            }

            if (subItem != null)
            {
                _context.SwitchScreen(subItem);  // 使用 SubItem 切换屏幕
            }

            
        }
        
    }
}
