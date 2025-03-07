using DataService.Entity;
using EquipmentSignalData.Command;
using EquipmentSignalData.Converter;
using EquipmentSignalData.Models;
using Intetfaces;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Overall.xaml 的交互逻辑
    /// </summary>
    public partial class Overall : UserControl,INotifyPropertyChanged
    {
        public ObservableCollection<Valve> Valves { get; set; }

        public ObservableCollection<FlowMeterModel> FlowMeterModels { get; set; }
        public ObservableCollection<FlowMeterModel> Temperatures { get; set; }
        public ICommand SkipCommand { get; }

        public ICommand ConfirmCommand { get; }
        public ICommand ConfirmCommandWD { get; }

        XMLHelper xml = new XMLHelper();

        private readonly System.Timers.Timer updateTimer;
        public Overall()
        {
            InitializeComponent();
            Valves = App.SharedValves;
            SkipCommand = new RelayCommand(ApplyFilterAsync);

            ConfirmCommand = new RelayCommand(ConfirmAction);
            ConfirmCommandWD = new RelayCommand(ConfirmActionWendu);

            FlowMeterModels = new ObservableCollection<FlowMeterModel>();
            for (int i = 1; i < 14; i++)
            {
                FlowMeterModels.Add(new FlowMeterModel { Name = i < 10 ? $"FC10{i}" : $"FC1{i}", SV = 0, PV = 0 });
            }

            Temperatures = new ObservableCollection<FlowMeterModel>();

            for (int i = 1; i < 27; i++)
            {
                Temperatures.Add(new FlowMeterModel { Id=i, Name = i < 10 ? $"TS10{i}" : $"TS1{i}", SV = 0, PV = 0 });
            }


            updateTimer = new System.Timers.Timer(1000000); // 每秒更新一次
            updateTimer.Elapsed += RealTimeUpdate;
            updateTimer.Start();

            this.DataContext = this;
        }
        Random random = new Random();

        private async void RealTimeUpdate(object sender, ElapsedEventArgs e)
        {
            List<FlowMeter> flowMeter = xml.GetFlowMeterIP();
            ReadModBus readModBus = new ReadModBus();

            // 使用并行任务来提高效率
            var tasks = new List<Task>();
            for (int i = 0; i < 13; i++)
            {
                int index = i; // 捕获索引以防止闭包问题
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100 * index); // 可以加延时，避免全并发压力过大
                        readModBus.Start(flowMeter[index].Ip, 6000, 32);
                        List<string> Seven = await Task.Run(() => readModBus.ReadSeven(20));

                        if (Seven != null && Seven.Count > 3)  // 确保数据有效
                        {
                            FlowMeterModels[index].PV = Convert.ToInt32(Seven[3]);
                        }
                        else
                        {
                            AlarmManager.Instance.ShowError("流量计返回数据无效！" + flowMeter[index]);
                        }
                    }
                    catch (Exception ex)
                    {
                        //AlarmManager.Instance.ShowError("流量计连接失败！" + flowMeter[index] + " 错误：" + ex.Message);
                    }
                }));
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);
            
        }
        ReadModBus readModBus = new ReadModBus();

        private async void ConfirmAction(object parameter)
        {
            var flowMeter = parameter as FlowMeterModel;
            if (flowMeter == null) return;

            // 弹出确认框
            var result = MessageBox.Show(
                $"确定要写入设定值为 {flowMeter.SV} 到设备: {flowMeter.Name} 吗？",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 执行写入操作
                FlowMeter flowMetermod = xml.GetFlowMeter(flowMeter.Name);
                readModBus.Start(flowMetermod.Ip, flowMetermod.Port, 1);
                readModBus.Write(0, Convert.ToInt32(flowMeter.SV));
                //AlarmManager.Instance.ShowError("错误");
                AlarmManager.Instance.ShowInfo("成功写入");

            }
        }
        public string[] SVList = { "Hx000", "Hx001", "Hx002", "Hx003", "Hx004", "Hx005", "Hx006", "Hx007" };
        private async void ConfirmActionWendu(object parameter)
        {
            var flowMeter = parameter as FlowMeterModel;
            if (flowMeter == null) return;

            // 弹出确认框
            var result = MessageBox.Show(
                $"确定要写入设定值为 {flowMeter.SV} 到设备: {flowMeter.Name} 吗？",
                "确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 执行写入操作
                FlowMeter flowMetermod = xml.GetFlowMeter(flowMeter.Name);
                string key = RemoveNotNumber(SVList[flowMeter.Id]);
                int decValue = Convert.ToInt32(key, 16);


                readModBus.Start("192.168.1.50", 502, 1);
                readModBus.Write(decValue, Convert.ToInt32(flowMeter.SV));

            }
        }
        private readonly Dictionary<string, Func<SubItem>> _pageMappings = new()
        {
            { "阀岛循环", () => new SubItem("阀岛循环", () => new UserControlHomePage()) },
            { "主流程", () => new SubItem("时序控制", () => new RecipeManagerView()) },
            { "阀门控制", () => new SubItem("阀门控制", () => new UserControlManualControlPage()) },
            { "阀门互锁", () => new SubItem("阀门互锁", () => new UserControlInterlockConfig()) }
        };

        private void ApplyFilterAsync(object parameter)
        {
            if (parameter is string pageName)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                if (_pageMappings.TryGetValue(pageName, out var createPageFunc))
                {
                    var subItem = createPageFunc();
                    mainWindow.SwitchScreen(subItem);
                }
                else
                {
                    Console.WriteLine("未知页面");
                }
            }
        }
        public string RemoveNotNumber(string key)
        {
            string str1 = System.Text.RegularExpressions.Regex.Replace(key, @"[A-Za-z]", "");
            return str1;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
