using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using EquipmentSignalData.ViewModels;
using EquipmentSignalData.Views;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using DataService.Entity;
using System.Windows.Shapes;

namespace EquipmentSignalData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private GridLength _sideBarWidth = new GridLength(0); // 默认宽度为250


        public GridLength SideBarWidth
        {
            get { return _sideBarWidth; }
            set
            {
                _sideBarWidth = value;
                OnPropertyChanged(nameof(SideBarWidth));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            //LogStartupAsync();
            var menuSchedule = new List<SubItem>
            {
                new SubItem("Modbus TCP", () => new UserControlModbusPage()),
                new SubItem("Ethernet/IP", () => new UserControlEthernetIpPage()),
            };
            var menuRegister = new List<SubItem>
            {
                new SubItem("设备日志", () => new UserControlEquipmentLog()),
                //new SubItem("时序控制", () => new UserControlTimeSequence()),
                new SubItem("阀岛循环", () => new UserControlHomePage()),
                
                new SubItem("时序控制", () => new RecipeManagerView()),

                new SubItem("Valve Recipe", () => new ValveRecipe()),
                new SubItem("Adam6022", () => new Adam6022View()),
                new SubItem("Adam6015", () => new Adam6015()),
                new SubItem("七星流量计Chart", () => new FlowMeterPage()),
                new SubItem("压力规Chart", () => new UserControlYaLiGui()),
                new SubItem("台达 温度", () => new TaiDaWend()),
                
                new SubItem("阀岛配置", () => new UserControlConfigXML()),

                new SubItem("七星流量计配置", () => new FlowMeterGroup()),

                new SubItem("阀门编组", () => new ValveControl()),
                new SubItem("阀门互锁", () => new UserControlInterlockConfig()),
                new SubItem("阀门控制", () => new UserControlManualControlPage()),
                //new SubItem("压力规", () => new UserControlSampling()),
                new SubItem("设备配置", () => new UserControlEthernetIpPage()),

                //new SubItem("通讯", null,menuSchedule)
            };

            var menuRegister2 = new List<SubItem>
            {
                new SubItem("首页", () => new Overall()),
            };

            var item6 = new ItemMenu("首页", menuRegister2, PackIconKind.Selfie);
            var item2 = new ItemMenu("设备信息", menuRegister, PackIconKind.Monitor);

            //var item1 = new ItemMenu("通讯", menuSchedule, PackIconKind.Radio);


            Menu.Children.Add(new UserControlMenuItem(item6, this));
            Menu.Children.Add(new UserControlMenuItem(item2, this));
            //Menu.Children.Add(new UserControlMenuItem(item1, this));
            
            DataContext = this;
            ToggleButton_Click(this, new RoutedEventArgs());
            
        }
        private async void LogStartupAsync()
        {
            new ExcelLogger().LogOperationAsync(new OperationLog() { OperationName = "系统启动" });
        }

        private System.Windows.Controls.UserControl currentView;
        public System.Windows.Controls.UserControl CurrentView
        {
            get => currentView;
            set
            {
                currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }
      
        internal void SwitchScreen(SubItem subItem)
        {
            var screen = subItem.GetControl();

            if (screen != null)
            {
                CurrentView = screen;
            }

            foreach (var menu in Menu.Children.OfType<UserControlMenuItem>())
            {
                var listViewMenu = menu.FindName("ListViewMenu") as System.Windows.Controls.ListView;
                if (listViewMenu != null)
                {
                    if (listViewMenu.SelectedItem != subItem)
                    {
                        listViewMenu.SelectedItem = null;
                    }
                    //else
                    //{
                    //    listViewMenu.SelectedItem = subItem;  
                    //}
                }
            }
        }


        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

            var animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut } 
            };

            if (SideBarWidth.Value > 0) 
            {
                animation.From = SideBar.ActualWidth;
                animation.To = 0; 
                animation.Completed += (s, _) => SideBarWidth = new GridLength(0); 
            }
            else 
            {
                animation.From = SideBar.ActualWidth;
                animation.To = 200; 
                animation.Completed += (s, _) => SideBarWidth = new GridLength(200);
            }

        
            SideBar.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            await new ExcelLogger().LogOperationAsync(new OperationLog() { OperationName = "系统关闭" });


        }


    }
}