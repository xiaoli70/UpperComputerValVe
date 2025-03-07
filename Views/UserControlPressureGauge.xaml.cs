using EquipmentSignalData.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// UserControlPressureGauge.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlPressureGauge : UserControl, INotifyPropertyChanged
    {
        private int normalSamplingPeriod;
        private int testSamplingPeriod;
        private bool isTestMode;

        public ICommand StartReadingCommand { get; set; }
        public ICommand CalibrateZeroCommand { get; set; }
      

        private bool isReading;
        public bool IsReading
        {
            get => isReading;
            set { isReading = value; OnPropertyChanged(nameof(IsReading)); }
        }


        public int NormalSamplingPeriod
        {
            get => normalSamplingPeriod;
            set { normalSamplingPeriod = value; OnPropertyChanged(nameof(NormalSamplingPeriod)); }
        }

        public int TestSamplingPeriod
        {
            get => testSamplingPeriod;
            set
            {
                testSamplingPeriod = Math.Max(value, 100); // 设置最小频率为100ms
                OnPropertyChanged(nameof(TestSamplingPeriod));
            }
        }

        public bool IsTestMode
        {
            get => isTestMode;
            set { isTestMode = value; OnPropertyChanged(nameof(IsTestMode)); }
        }

        public ICommand ToggleTestModeCommand { get; set; }
        public UserControlPressureGauge()
        {

            InitializeComponent();
            ToggleTestModeCommand = new RelayCommand(_=>ToggleTestMode());
            StartReadingCommand = new RelayCommand(_ => StartReading());
            CalibrateZeroCommand = new RelayCommand(_ => CalibrateZero());
            this.DataContext = this;
        }
        private void ToggleTestMode()
        {
            IsTestMode = !IsTestMode;
        }
        private void StartReading()
        {
            // 启动数据读取逻辑
            IsReading = true;
        }

        private void CalibrateZero()
        {
            // 校准逻辑
        }

     
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
