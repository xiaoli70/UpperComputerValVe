using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using PdfSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using DataService.Entity;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlSampling.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlSampling : UserControl,INotifyPropertyChanged
    {

        private int samplingPeriod;
        private bool isSampling;
        private string samplingButtonContent = "开始采样";
        private string samplingStatus = "采样未启动";
        private const int PageSize = 20; 
        private int currentPage;
        public int SamplingPeriod
        {
            get => samplingPeriod;
            set { samplingPeriod = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> SamplingPeriods { get; set; }

        public string SamplingButtonContent
        {
            get => samplingButtonContent;
            set { samplingButtonContent = value; OnPropertyChanged(); }
        }

        public string SamplingStatus
        {
            get => samplingStatus;
            set { samplingStatus = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SamplingData> AllSamplingData { get; set; } = new ObservableCollection<SamplingData>();
        public ObservableCollection<SamplingData> PagedItems { get; set; } = new ObservableCollection<SamplingData>();

        public int CurrentPage
        {
            get => currentPage + 1;
            set
            {
                currentPage = value - 1;
                UpdatePagedItems();
                OnPropertyChanged();
            }
        }

        public int TotalPages => (AllSamplingData.Count + PageSize - 1) / PageSize;

        public ICommand ToggleSamplingCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand LastPageCommand { get; }

        public UserControlSampling()
        {
            InitializeComponent();
            SamplingPeriods = new ObservableCollection<int> { 100, 200, 500, 1000, 2000 }; 
            SamplingPeriod = SamplingPeriods.FirstOrDefault(); 
            ToggleSamplingCommand = new RelayCommand(_ => ToggleSampling());
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => currentPage < TotalPages - 1);
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => currentPage > 0);
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => currentPage > 0);
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => currentPage < TotalPages - 1);
            UpdatePagedItems();
            this.DataContext = this;
        }
        private void ToggleSampling()
        {
            if (isSampling)
            {
                isSampling = false;
                SamplingButtonContent = "开始采样";
                SamplingStatus = "采样已停止";
            }
            else
            {
                isSampling = true;
                SamplingButtonContent = "停止采样";
                SamplingStatus = "采样中...";
                _ = StartSampling();
            }
        }

        private async Task StartSampling()
        {
            while (isSampling)
            {
                var data = new SamplingData
                {
                    Timestamp = DateTime.Now,
                    PressureValue = GetPressureData()
                };

                AllSamplingData.Insert(0, data); 
                UpdatePagedItems();
                await Task.Delay(SamplingPeriod);
            }
        }

        private void UpdatePagedItems()
        {
            PagedItems.Clear();
            var items = AllSamplingData.Skip(currentPage * PageSize).Take(PageSize);
            foreach (var item in items)
            {
                PagedItems.Add(item);
            }

            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(TotalPages));
        }

        private void GoToNextPage() { currentPage++; UpdatePagedItems(); }
        private void GoToPreviousPage() { currentPage--; UpdatePagedItems(); }
        private void GoToFirstPage() { currentPage = 0; UpdatePagedItems(); }
        private void GoToLastPage() { currentPage = TotalPages - 1; UpdatePagedItems(); }

        private double GetPressureData()
        {
            
            return new Random().NextDouble() * 100;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
