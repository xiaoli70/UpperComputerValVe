using EquipmentSignalData.Command;
using Intetfaces;
using LiveCharts;
using LiveCharts.Wpf;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
    /// Adam6022View.xaml 的交互逻辑
    /// </summary>
    public partial class Adam6022View : UserControl, INotifyPropertyChanged
    {

        public ObservableCollection<string> Labels { get; set; }
        public SeriesCollection ChartSeries { get; set; }

        public ObservableCollection<string> Labels2 { get; set; }
        public SeriesCollection ChartSeries2 { get; set; }

        public ObservableCollection<string> Labels1 { get; set; }
        public SeriesCollection ChartSeries1 { get; set; }


        public ICommand ToggleCommand { get; }
        public ICommand UpdateV { get; }

        private string buttonContent;
        public string ButtonContent
        {
            get => buttonContent;
            set
            {
                buttonContent = value;
                OnPropertyChanged();
            }
        }

        private string button2Content;


        private bool isRunning;


        private double _axisMax;
        public double AxisMax
        {
            get => _axisMax;
            set
            {
                _axisMax = value;
                OnPropertyChanged(nameof(AxisMax));
            }
        }

        private double _axisMin;
        public double AxisMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged(nameof(AxisMin));
            }
        }

        public string Button2Content { get => button2Content; set => button2Content = value; }

        public Adam6022View()
        {
            InitializeComponent();
            buttonContent = "Start";
            button2Content = "Update";
            isRunning = false;
            #region
            Labels = new ObservableCollection<string>();
            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MV",
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                },
                new LineSeries
                {
                    Title = "PV",
                    Values = new ChartValues<double> { 15, 20, 22, 26 }
                },
                new LineSeries
                {
                    Title = "SV",
                    Values = new ChartValues<double> { 10, 15, 18, 20 }
                }
            };
            Labels.Add("10:00");
            Labels.Add("10:01");
            Labels.Add("10:02");
            Labels.Add("10:03");

            Labels2 = new ObservableCollection<string>();
            ChartSeries2 = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MV",
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                },
                new LineSeries
                {
                    Title = "PV",
                    Values = new ChartValues<double> { 15, 20, 22, 26 }
                },
                new LineSeries
                {
                    Title = "SV",
                    Values = new ChartValues<double> { 10, 15, 18, 20 }
                }
            };
            Labels2.Add("10:00");
            Labels2.Add("10:01");
            Labels2.Add("10:02");
            Labels2.Add("10:03");

            Labels1 = new ObservableCollection<string>();
            ChartSeries1 = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MV",
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                },
                new LineSeries
                {
                    Title = "PV",
                    Values = new ChartValues<double> { 15, 20, 22, 26 }
                },
                new LineSeries
                {
                    Title = "SV",
                    Values = new ChartValues<double> { 10, 15, 18, 20 }
                }
            };
            Labels1.Add("10:00");
            Labels1.Add("10:01");
            Labels1.Add("10:02");
            Labels1.Add("10:03");
            if (Labels.Count > 10)
            {
                AxisMax = Labels.Count - 1; // 设置为最新数据的索引
                AxisMin = AxisMax - 9;      // 显示最近的10个数据点
            }
            else
            {
                AxisMax = Labels.Count - 1;
                AxisMin = 0;
            }
            #endregion
            ToggleCommand = new RelayCommand(_ => ToggleButton());

            UpdateV = new RelayCommand(_ => UpdateVButton());
            _ = AddDateLabels();
            this.DataContext = this;
            FZ();
        }



        private void ToggleButton()
        {
            isRunning = !isRunning;
            ButtonContent = isRunning ? "Stop" : "Start";
        }


        private Random random = new Random();
        public async Task AddDateLabels()
        {
            ADAM6022 aDAM6022 = new ADAM6022();
            XMLHelper xMLHelper = new XMLHelper();
            while (true)
            {
                if (!isRunning)
                {
                    await Task.Delay(500);
                    continue;
                }
               string value= LoopCount.Text;
               int Loop=Convert.ToInt32(value);

                var (ipAddress, port) = xMLHelper.GetDeviceConfiguration("ADAM6022");

                aDAM6022.Connter(ipAddress, Convert.ToInt32(port), Loop);
                string result = aDAM6022.Start();
                string[] resuliList = result.Split('*');
                //ADAM6015 aDAM6015 = new ADAM6015();//ADAD6015请求数据
                //aDAM6015.Connter6015("192.168.1.42", 502);
                //aDAM6015.ModBusStart();
                //var (first, second) = aDAM6015.Start();//首先判断second是否位null,null输出日志first

                //ADAM6015 aDAM60152 = new ADAM6015();//ADAD6015请求数据
                //aDAM60152.Connter6015("192.168.1.43", 502);
                //aDAM60152.ModBusStart();
                //var (first2, second2) = aDAM6015.Start();//首先判断second是否位null,null输出日志first

                AddDataPoint(Convert.ToDouble(resuliList[0]), Convert.ToDouble(resuliList[1]), Convert.ToDouble(resuliList[2]), DateTime.Now.ToString("HH:mm:ss"));
                await Task.Delay(3000);
            }

        }


        public void AddDataPoint(double svValue, double pvValue, double mvValue, string label)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChartSeries[0].Values.Add(svValue);
                ChartSeries[1].Values.Add(pvValue);
                ChartSeries[2].Values.Add(mvValue);
                Labels.Add(label);
                if (Labels.Count > 10)
                {
                    AxisMax = Labels.Count - 1; // 设置为最新数据的索引
                    AxisMin = AxisMax - 9;      // 显示最近的10个数据点
                }
                else
                {
                    AxisMax = Labels.Count - 1;
                    AxisMin = 0;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void UpdateVButton()
        {
            try
            {
                string value = LoopCount.Text;
                int Loop = Convert.ToInt32(value);
                string SVValue = SVPIDValue.Text;
                int SV = Convert.ToInt32(SVValue);

                ADAM6022 aDAM6022 = new ADAM6022();
                aDAM6022.Connter("192.168.1.41", 502, Loop);
                aDAM6022.trackBarSV_ValueChanged(SV);
                FZ();
            }
            catch (Exception ex)
            {

             
            }
        }
        public void FZ()
        {
            string value = LoopCount.Text;
            int Loop = Convert.ToInt32(value);
            ADAM6022 aDAM6022 = new ADAM6022();
            aDAM6022.Connter("192.168.1.41", 502, Loop);

            var (cbxLoop, cbxControl, txtSV,PV,MV) = aDAM6022.RefreshPIDStatic();
            SVPIDValue.Text = txtSV;
            PVPIDValue.Text = PV;
            MVPIDValue.Text = MV;
        }
      
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Some logic here
        }
    }
}
