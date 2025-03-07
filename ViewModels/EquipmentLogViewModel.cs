using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using EquipmentSignalData.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Data;
using DataService.Entity;

namespace EquipmentSignalData.ViewModels
{
    /// <summary>
    /// 设备日志
    /// </summary>
    internal class EquipmentLogViewModel : INotifyPropertyChanged
    {
        private const int PageSize = 20;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _filterText = string.Empty;

        private string directoryPath = string.Empty;

        private DateTime selectedStartDate;
        private DateTime selectedEndDate;

        public ObservableCollection<OperationLog> AllItems { get; set; }
        public ObservableCollection<OperationLog> PagedItems { get; set; }
        public ICollectionView FilteredItemsView { get; set; }

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public ICommand FirstPageCommand { get; }
        public ICommand LastPageCommand { get; }

        public ICommand FilterCommand { get; }


        public DateTime SelectedStartDate
        {
            get => selectedStartDate;
            set
            {
                selectedStartDate = value;
                OnPropertyChanged(nameof(SelectedStartDate));
            }
        }

        public DateTime SelectedEndDate
        {
            get => selectedEndDate;
            set
            {
                selectedEndDate = value;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                    ApplyFilter();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    UpdatePagedItems();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }
        private ExcelHelper<OperationLog> _excelHelper;
        public EquipmentLogViewModel()
        {
            SelectedStartDate = DateTime.Now;
            SelectedEndDate = DateTime.Now;
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            directoryPath = Path.Combine(rootPath, "database", "Log");
            // 构建日志文件的完整路径


            _excelHelper = new ExcelHelper<OperationLog>();

            // 初始化数据
            AllItems = new ObservableCollection<OperationLog>();
            List<OperationLog> items = _excelHelper.ReadItemsFromExcel(Path.Combine(directoryPath, $"operationLogs{SelectedStartDate:yyyyMMdd}.xlsx"));
            foreach (var item in items)
            {
                AllItems.Add(item);
            }

            // 初始化 CollectionView 用于过滤
            FilteredItemsView = CollectionViewSource.GetDefaultView(AllItems);
            FilteredItemsView.Filter = FilterItems;

            // 初始化分页
            UpdateTotalPages();
            PagedItems = new ObservableCollection<OperationLog>();
            UpdatePagedItems();

            // 初始化命令
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
            FilterCommand = new RelayCommand(_ => ApplyFilterAsync());
            FirstPageCommand = new RelayCommand(_ => FirstPage(), _ => CurrentPage > 1);
            LastPageCommand = new RelayCommand(_ => LastPage(), _ => CurrentPage < TotalPages);
            
        }

        private bool FilterItems(object obj)
        {
            if (obj is OperationLog item)
            {
                if (string.IsNullOrWhiteSpace(FilterText))
                    return true;
                return item.ValveName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

        private void ApplyFilter()
        {
            FilteredItemsView.Refresh();
            UpdateTotalPages();
            CurrentPage = 1;
            UpdatePagedItems();
        }

        private async Task ApplyFilterAsync()
        {
            if (AllItems.Count == 0 ||
        SelectedStartDate != selectedEndDate ||
        SelectedEndDate != selectedEndDate)
            {
                await Task.Run(() => MergeExcelData(SelectedStartDate, SelectedEndDate));
            }
            FilteredItemsView.Refresh();
            UpdateTotalPages();
            CurrentPage = 1;
            UpdatePagedItems();
        }

        public void MergeExcelData(DateTime startDate, DateTime endDate)
        {
            // 用于存放所有Excel数据的DataTable
            List<List<OperationLog>> list= new List<List<OperationLog>>();

            // 获取两个日期之间的所有日期
            List<DateTime> dateList = GetDatesInRange(startDate, endDate);

            foreach (var date in dateList)
            {
                // 生成文件路径，根据日期构造文件名
                string excelFilePath = Path.Combine(directoryPath, $"operationLogs{date.ToString("yyyyMMdd")}.xlsx");

                // 检查文件是否存在
                if (File.Exists(excelFilePath))
                {
                    List<OperationLog> items = _excelHelper.ReadItemsFromExcel(excelFilePath);
                    if (items != null && items.Count>0)
                        list.Add(items);
                }
                else
                {
                    Console.WriteLine($"文件未找到: {excelFilePath}");
                }
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                AllItems.Clear();
                foreach (var itemList in list)
                {
                    foreach (var item in itemList)
                    {
                        AllItems.Add(item);
                    }
                }

                // 初始化 CollectionView 用于过滤
                FilteredItemsView = CollectionViewSource.GetDefaultView(AllItems);
                FilteredItemsView.Filter = FilterItems;
            });
        }

        // 获取两个日期之间的所有日期
        private List<DateTime> GetDatesInRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dates.Add(date);
            }
            return dates;
        }

        private void UpdateTotalPages()
        {
            int totalItems = FilteredItemsView.Cast<object>().Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (TotalPages == 0)
                TotalPages = 1;
        }

        private void UpdatePagedItems()
        {
            PagedItems.Clear();
            var items = FilteredItemsView.Cast<OperationLog>()
                                         .Skip((CurrentPage - 1) * PageSize)
                                         .Take(PageSize);
            foreach (var item in items)
            {
                PagedItems.Add(item);
            }
            CommandManager.InvalidateRequerySuggested();
        }


        private void NextPage() => CurrentPage++;
        private void PreviousPage() => CurrentPage--;
        private void FirstPage() => CurrentPage = 1;
        private void LastPage() => CurrentPage = TotalPages;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
