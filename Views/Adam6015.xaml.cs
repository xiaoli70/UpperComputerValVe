using EquipmentSignalData.Command;
using Intetfaces;
using LiveCharts;
using LiveCharts.Wpf;
using NPOI.SS.UserModel.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// Adam6015.xaml 的交互逻辑
    /// </summary>
    public partial class Adam6015 : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ChartDataModel> ChartViewModels { get; set; } // 八个折线图
        public ObservableCollection<RealTimeDataModel> RealTimeData { get; set; } // 实时数据

        public Adam6015()
        {
            InitializeComponent();


            // 初始化折线图数据
            ChartViewModels = new ObservableCollection<ChartDataModel>();
            for (int i = 0; i < 8; i++)
            {
                ChartViewModels.Add(new ChartDataModel
                {
                    Title = $"折线图 {i + 1}",
                    ChartSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = new ChartValues<double> {},
                            Title = "6015"
                        }
              
                    },
                    Labels = new ObservableCollection<string> { },
                    RowIndex = i / 4, // 行索引
                    ColumnIndex = i % 4 // 列索引
                });
            }

            // 初始化实时数据
            RealTimeData = new ObservableCollection<RealTimeDataModel>();
            for (int i = 1; i <= 8; i++)
            {
                RealTimeData.Add(new RealTimeDataModel
                {
                    Title = $"数据 {i}",
                    Value = 0
                });
            }

            // 启动数据更新任务
            StartRealTimeDataUpdate();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void StartRealTimeDataUpdate()
        {
            var random = new Random();

            XMLHelper xMLHelper = new XMLHelper();

            //ADAM6015 aDAM60152 = new ADAM6015();//ADAD6015请求数据
            //aDAM60152.Connter6015("192.168.1.42", 502);
            //aDAM60152.ModBusStart();
            //var (first, second) = aDAM60152.Start();//首先判断second是否位null,null输出日志first


            // 创建每个折线图单独的更新逻辑
            while (true)
            {
                ADAM6015_2 aDAM6015 = new ADAM6015_2();
                var (ipAddress, port) = xMLHelper.GetDeviceConfiguration("ADAM6022");
                aDAM6015.Connter6015(ipAddress, Convert.ToInt32(port));
                aDAM6015.ModBusStart();
                var (first2, second2) = aDAM6015.Start();//首先判断second是否位null,null输出日志first
                if (second2==null)
                    break;

                for (int i = 0; i < ChartViewModels.Count; i++)
                {
                    var chartModel = ChartViewModels[i];
                    var realTimeData = RealTimeData[i];

                    //ADAD6015请求数据
                    
                    // 添加新数据到折线图
                    //var newValue = random.Next(0, 100);
                    var newValue = second2[i];

                    ((LineSeries)chartModel.ChartSeries[0]).Values.Add(Math.Round(Convert.ToDouble(newValue), 2));

                    // 更新实时数据
                    realTimeData.Value = newValue;

                    // 保持显示数据点数量不超过 20（避免内存占用过高）
                    if (((LineSeries)chartModel.ChartSeries[0]).Values.Count > 20)
                    {
                        ((LineSeries)chartModel.ChartSeries[0]).Values.RemoveAt(0);
                    }

                    // 更新 X 轴标签
                    chartModel.Labels.Add($"点{chartModel.Labels.Count + 1}");
                    if (chartModel.Labels.Count > 20)
                    {
                        chartModel.Labels.RemoveAt(0);
                    }
                }

                OnPropertyChanged(nameof(ChartViewModels));
                OnPropertyChanged(nameof(RealTimeData));

                // 每 5 秒更新一次
                await Task.Delay(5000);
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ChartDataModel
    {
        public string Title { get; set; } // 折线图标题
        public SeriesCollection ChartSeries { get; set; } // 数据点集合
        public ObservableCollection<string> Labels { get; set; } // X轴标签
        public int RowIndex { get; set; } // 图表所在行
        public int ColumnIndex { get; set; } // 图表所在列
    }

    public class RealTimeDataModel : INotifyPropertyChanged
    {
        private double value;
        public string Title { get; set; } // 数据标题
        public double Value
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
