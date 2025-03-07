using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Intetfaces;
using System.ComponentModel;
using DataService.Helpers;
using EquipmentSignalData.Command;
using MathNet.Numerics.Random;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// TaiDaWend.xaml 的交互逻辑
    /// </summary>
    public partial class TaiDaWend : UserControl,INotifyPropertyChanged
    {
        ReadModBus readModBus = new ReadModBus();
        public string[] UxMList = { "Hx268", "Hx269", "Hx26A", "Hx26B", "Hx26C", "Hx26D", "Hx26E", "Hx26F" };
        public string[] SVList = { "Hx000", "Hx001" ,"Hx002", "Hx003", "Hx004", "Hx005", "Hx006", "Hx007" };

        public ObservableCollection<ChartDataWenDuModel> ChartViewModels { get; set; } // 八个折线图
        public ObservableCollection<RealTimeDataModel> RealTimeData { get; set; } // 实时数据

        public ICommand WriteCommand { get; }
        public TaiDaWend()
        {
            InitializeComponent();
            // 初始化折线图数据
            ChartViewModels = new ObservableCollection<ChartDataWenDuModel>();
            for (int i = 0; i < 8; i++)
            {
                ChartViewModels.Add(new ChartDataWenDuModel
                {
                    Id = i,
                    Title = $"折线图 {i + 1}",
                    ChartSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = new ChartValues<double> {10},
                            Title = "PV"
                        }

                    },
                    Labels = new ObservableCollection<string> {"10:00" },
                    RowIndex = i / 4, // 行索引
                    ColumnIndex = i % 4, // 列索引
                    SetSV=0,
                    SetMV=0,
                });
            }
            WriteCommand = new RelayCommand(WriteValues);
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
        private void WriteValues(object parameter)
        {
            

            // 处理写入逻辑
            var chart = parameter as ChartDataWenDuModel;
            if (chart != null)
            {
                int sv = chart.SetSV;
                int mv = chart.SetMV;

                string key = RemoveNotNumber(SVList[chart.Id]);
                int decValue = Convert.ToInt32(key, 16);

                
                readModBus.Start("192.168.1.50", 502, 1);
                readModBus.Write(decValue, sv);

                readModBus.Start("192.168.1.50", 502, 1);
                ushort[] a1 = readModBus.Read50(decValue);
                Console.WriteLine(  );
                // 执行写入操作
                //MessageBox.Show($"写入成功: SV={sv}, MV={mv}");readModBus.Read50(decValue);
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private async void StartRealTimeDataUpdate()
        {
            var random = new Random();
            await Task.Run(async () =>
            {
                // 创建每个折线图单独的更新逻辑
                while (true)
                {
                    //string key = RemoveNotNumber("Hx268");
                    //int decValue = Convert.ToInt32(key, 16);
                    //ReadModBus readModBus = new ReadModBus();
                    //readModBus.Start("192.168.1.50", 502, 1);
                    //ushort[] a1 = readModBus.Read50(decValue);
                    try
                    {

                    
                    for (int i = 0; i < ChartViewModels.Count; i++)
                    {
                        var chartModel = ChartViewModels[i];
                        var realTimeData = RealTimeData[i];

                        string key = RemoveNotNumber(UxMList[i]);
                        int decValue = Convert.ToInt32(key, 16);

                        readModBus.Start("192.168.1.50", 502, 1);
                        ushort[] a1 = readModBus.Read50(decValue);

                        //ADAD6015请求数据

                        // 添加新数据到折线图
                        //var newValue = random.Next(0, 100);
                        var newValue = a1[0];

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
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("设备连接失败");
                        return;
                        throw;
                    }
                    // 每 5 秒更新一次
                    await Task.Delay(5000);
                }

            });
        }
        public string RemoveNotNumber(string key)
        {
            string str1 = System.Text.RegularExpressions.Regex.Replace(key, @"[A-Za-z]", "");
            return str1;
        }
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
    public class ChartDataWenDuModel
    {
        public int Id { get; set; }
        public string Title { get; set; } // 折线图标题
        public SeriesCollection ChartSeries { get; set; } // 数据点集合
        public ObservableCollection<string> Labels { get; set; } // X轴标签
        public int RowIndex { get; set; } // 图表所在行
        public int ColumnIndex { get; set; } // 图表所在列

        public int SetSV { get; set; }
        public int SetMV { get; set; }

    }
}
