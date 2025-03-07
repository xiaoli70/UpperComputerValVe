using EquipmentSignalData.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentSignalData.ViewModels
{
    internal class MainWindowModel : INotifyPropertyChanged
    {

        private GridLength _sideBarWidth = new GridLength(250); // 默认宽度为250

        public GridLength SideBarWidth
        {
            get { return _sideBarWidth; }
            set
            {
                _sideBarWidth = value;
                OnPropertyChanged(nameof(SideBarWidth));
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) //侧边栏
        {
            // 如果当前宽度大于0，则收起，否则展开
            if (SideBarWidth.Value > 0)
            {
                SideBarWidth = new GridLength(0);
            }
            else
            {
                SideBarWidth = new GridLength(250); // 恢复为原始宽度
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
