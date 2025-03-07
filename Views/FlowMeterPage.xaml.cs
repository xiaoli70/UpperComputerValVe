using DataService.Entity;
using EquipmentSignalData.Command;
using Intetfaces;
using LiveCharts;
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
    /// FlowMeterPage.xaml 的交互逻辑
    /// </summary>
    public partial class FlowMeterPage : UserControl, INotifyPropertyChanged
    {
        private readonly System.Timers.Timer updateTimer;
        private readonly int maxDataPoints = 20; // 控制显示的最大点数

        public ChartValues<double> InstantFlowValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> TemperatureValues { get; set; } = new ChartValues<double>();
        public ObservableCollection<string> TimeLabels { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<SensorData> SensorDataList { get; set; } = new ObservableCollection<SensorData>();

        XMLHelper xml = new XMLHelper();

        public ObservableCollection<string> IP { get; set; }

        ElectromagneticValveController ElectromagneticValveController { get; set; }

        private string _selectedIp;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SelectedIp
        {
            get { return _selectedIp; }
            set
            {
                if (_selectedIp != value)
                {
                    _selectedIp = value;
                    OnPropertyChanged(nameof(SelectedIp));
                }
            }
        }
        public ICommand StartCommand { get; }
        public FlowMeterPage()
        {
            InitializeComponent();

            StartCommand = new RelayCommand(_ => ApplyFilterAsync());

            ElectromagneticValveController = new ElectromagneticValveController();
            foreach (var value in new double[] { 1.0, 1.2, 1.5, 1.7, 2.0 })
            {
                InstantFlowValues.Add(value);
            }
            
            foreach (var value in new double[] { 20, 21, 22, 23, 24 })
            {
                TemperatureValues.Add(value);
            }
           
            foreach (var value in new string[] { "10:00", "10:10", "10:20", "10:30", "10:40" })
            {
                TimeLabels.Add(value);
            }
            IP = new ObservableCollection<string>
            {};

            var ipList = xml.GetFlowMeterIP(); 

            IP.Clear();

            foreach (var FlowMeter in ipList)
            {
                IP.Add(FlowMeter.Name);
            }
            SelectedIp = IP.FirstOrDefault();
            SensorDataList.Add(new SensorData { Name = "温度", Value = "54" });
            SensorDataList.Add(new SensorData { Name = "流量计", Value = "1.24" });
            SensorDataList.Add(new SensorData { Name = "压力传感器", Value = "101 kPa" });
            SensorDataList.Add(new SensorData { Name = "湿度传感器", Value = "40%" });
            SensorDataList.Add(new SensorData { Name = "振动传感器", Value = "0.05 g" });

            // 定时器动态更新数据
            updateTimer = new System.Timers.Timer(5000); // 每秒更新一次
            updateTimer.Elapsed += UpdateData;
            updateTimer.Start();

            this.DataContext = this;
        }

        private async Task ApplyFilterAsync()
        {
            var ss = SelectedIp;
        }
        private async void UpdateData(object sender, ElapsedEventArgs e)
        {

            //FlowMeter flowMeter2 = xml.GetFlowMeter("流量计 1");
            //if (flowMeter2.AlarmValue > 20.1)
            //{ //guan

            //    await ElectromagneticValveController.CloseValveAsync(flowMeter2.SelectedValveIslands.Split(","));
            //}
            //else
            //{
            //    await ElectromagneticValveController.OpenValveAsync(flowMeter2.SelectedValveIslands.Split(","));
            //}

            //var random = new Random();
            FlowMeter flowMeter = xml.GetFlowMeter(SelectedIp);
            ReadModBus readModBus = new ReadModBus();
            readModBus.Start(flowMeter.Ip, flowMeter.Port, 32);
            List<string> Seven =null;
            try
            {
                Seven = readModBus.ReadSeven(20);
            }
            catch (Exception)
            {
                updateTimer.Stop();
                return;
                //throw;
            }
           
            

            // 添加新数据
            double newInstantFlow = Convert.ToDouble(Seven[3]);// 模拟流量数据 random.Next(0, 100); 
            double newTemperature = Convert.ToDouble(Seven[15]); // 模拟温度数据

            if (flowMeter.AlarmValue > newInstantFlow)
            { //guan

                await ElectromagneticValveController.CloseValveAsync(flowMeter.SelectedValveIslands.Split(","));
            }
            else {
                await ElectromagneticValveController.OpenValveAsync(flowMeter.SelectedValveIslands.Split(","));
            }


            string newTimeLabel = DateTime.Now.ToString("HH:mm:ss");
            SensorData sensorDataToModify = SensorDataList.FirstOrDefault(sd => sd.Name == "温度");
            SensorData sensorDataToLiuLJ = SensorDataList.FirstOrDefault(sd => sd.Name == "流量计");
            App.Current.Dispatcher.Invoke(() =>
            {
                
                sensorDataToLiuLJ.Value = newInstantFlow.ToString();
                sensorDataToModify.Value = newTemperature.ToString();

                // 更新集合
                InstantFlowValues.Add(newInstantFlow);
                TemperatureValues.Add(newTemperature);
                TimeLabels.Add(newTimeLabel);

                // 如果超出最大点数，移除最早的数据
                if (InstantFlowValues.Count > maxDataPoints)
                {
                    InstantFlowValues.RemoveAt(0);
                    TemperatureValues.RemoveAt(0);
                    TimeLabels.RemoveAt(0);
                }
            });
        }
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
    public class SensorData: INotifyPropertyChanged
    {
        private string value;
        public string Name { get; set; }
        

        public string Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
