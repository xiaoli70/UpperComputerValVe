using EquipmentSignalData.ViewModels;
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

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// DeviceWindows.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceWindows : Window
    {
        public DeviceWindows()
        {
            InitializeComponent();
            this.DataContext = new ListViewModel();
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).Content.ToString() == "串口")
            {
                this.cb1.Visibility = Visibility.Visible;
                this.cb2.Visibility = Visibility.Visible;
                this.cb3.Visibility = Visibility.Visible;
                this.cb4.Visibility = Visibility.Visible;
                this.cb5.Visibility = Visibility.Visible;
                this.tb1.Visibility = Visibility.Collapsed;
                this.tb2.Visibility = Visibility.Collapsed;
            }
            else if ((sender as RadioButton).Content.ToString() == "网口")
            {
                this.cb1.Visibility = Visibility.Collapsed;
                this.cb2.Visibility = Visibility.Collapsed;
                this.cb3.Visibility = Visibility.Collapsed;
                this.cb4.Visibility = Visibility.Collapsed;
                this.cb5.Visibility = Visibility.Collapsed;
                this.tb1.Visibility = Visibility.Visible;
                this.tb2.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
