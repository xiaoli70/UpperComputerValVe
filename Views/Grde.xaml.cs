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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// Grde.xaml 的交互逻辑
    /// </summary>
    public partial class Grde : UserControl
    {
        public Grde()
        {
            InitializeComponent();
            this.DataContext = new GrdeViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DeviceWindows dw = new DeviceWindows();
            dw.ShowDialog();
        }
    }
}
