using EquipmentSignalData.Command;
using Intetfaces;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// UserControlYaLiGui.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlYaLiGui : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<string> Labels { get; set; }
        public SeriesCollection ChartSeries { get; set; }

        public ObservableCollection<string> Labels2 { get; set; }
        public SeriesCollection ChartSeries2 { get; set; }

        public ObservableCollection<string> Labels1 { get; set; }
        public SeriesCollection ChartSeries1 { get; set; }


        private double latestValueDevice1=105.2;
        public double LatestValueDevice1
        {
            get => latestValueDevice1;
            set
            {
                latestValueDevice1 = value;
                OnPropertyChanged(nameof(LatestValueDevice1));
            }
        }

        private double latestValueDevice2=21.0;
        public double LatestValueDevice2
        {
            get => latestValueDevice2;
            set
            {
                latestValueDevice2 = value;
                OnPropertyChanged(nameof(LatestValueDevice2));
            }
        }

        private double latestValueDevice3=34.2;
        public double LatestValueDevice3
        {
            get => latestValueDevice3;
            set
            {
                latestValueDevice3 = value;
                OnPropertyChanged(nameof(LatestValueDevice3));
            }
        }


        public ICommand ToggleCommand { get; }


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
        public UserControlYaLiGui()
        {
            InitializeComponent();

            buttonContent = "Start";
            isRunning = true;
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
                
            };
            Labels2.Add("10:00");
            Labels2.Add("10:01");
            Labels2.Add("10:02");
            Labels2.Add("10:03");

           
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
            _ = AddDateLabels();
            this.DataContext = this;
        }
        private void ToggleButton()
        {
            

            isRunning = !isRunning;
            ButtonContent = isRunning ? "Stop" : "Start";

        }

        public bool istrue = true;
        private Random random = new Random();
        public async Task AddDateLabels()
        {
            XMLHelper xMLHelper = new XMLHelper();
            ReadModBus readModBus = new ReadModBus();
            while (istrue)
            {
                if (!isRunning)
                {
                    await Task.Delay(500);
                    continue;
                }
                await Task.Run(() =>
                {
                    var (ipAddress, port) = xMLHelper.GetDeviceConfiguration("CDG025D压力硅");
                    var (ipAddress2, port2) = xMLHelper.GetDeviceConfiguration("复合柜检测FPG550");
                    //Debug.Write($"ip:{ipAddress}--{port}   -----ip2:{ipAddress2}__{port2}");
                    readModBus.Start(ipAddress, Convert.ToInt32(port), 1);
                    ushort[] w = readModBus.Read(2);
                    if (w == null) {
                        istrue = false;
                        return;
                        };
                    readModBus.Start(ipAddress2, Convert.ToInt32(port2), 1);
                    ushort[] e = readModBus.Read(3);
                    AddDataPoint(e[0], e[1], e[2], w[0], w[1], DateTime.Now.ToString("HH:mm:ss"));
                });
                await Task.Delay(5000);

            }

        }


        public void AddDataPoint(double svValue, double pvValue, double mvValue,double sv2,double pv2, string label)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChartSeries[0].Values.Add(svValue);
                ChartSeries[1].Values.Add(pvValue);
                ChartSeries[2].Values.Add(mvValue);

                ChartSeries2[0].Values.Add(sv2);
                ChartSeries2[1].Values.Add(pv2);


                Labels.Add(label);
                Labels2.Add(label);
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
    }
}
