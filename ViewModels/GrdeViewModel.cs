using EquipmentSignalData.Command;
using EquipmentSignalData.Models;
using EquipmentSignalData.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DataService.Entity;

namespace EquipmentSignalData.ViewModels
{
    internal class GrdeViewModel : INotifyPropertyChanged
    {
        private const int PageSize = 15;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _filterText = string.Empty;

        public ObservableCollection<ItemModel> AllItems { get; set; }
        public ObservableCollection<ItemModel> PagedItems { get; set; }
        public ICollectionView FilteredItemsView { get; set; }

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public ICommand FirstPageCommand { get; }
        public ICommand LastPageCommand { get; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand FilterCommand { get; }

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
        private ExcelHelper<ItemModel> _excelHelper;
        public GrdeViewModel()
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;

            // 构建日志文件的完整路径
            string directoryPath = Path.Combine(rootPath, "database");
            
            _excelHelper = new ExcelHelper<ItemModel>();
            
            // 初始化数据
            AllItems = new ObservableCollection<ItemModel>();
            //for (int i = 1; i <= 100; i++)
            //{
                
            //    _excelHelper.AddItemToExcel(new ItemModel { Index = i, Name = $"Item {i}", Type = "A1", Unit = "1KM" });
            //}
            List<ItemModel> items = _excelHelper.ReadItemsFromExcel(Path.Combine(directoryPath, "Grid.xlsx"));
            foreach (var item in items)
            {
                AllItems.Add(item);
            }
            

            // 初始化 CollectionView 用于过滤
            FilteredItemsView = CollectionViewSource.GetDefaultView(AllItems);
            FilteredItemsView.Filter = FilterItems;

            // 初始化分页
            UpdateTotalPages();
            PagedItems = new ObservableCollection<ItemModel>();
            UpdatePagedItems();

            // 初始化命令
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
            EditCommand = new RelayCommand(EditItem);
            AddCommand = new RelayCommand(AddItem);
            DeleteCommand = new RelayCommand(DeleteItem);
            FilterCommand = new RelayCommand(_ => ApplyFilter());
            FirstPageCommand = new RelayCommand(_ => FirstPage(), _ => CurrentPage > 1);
            LastPageCommand = new RelayCommand(_ => LastPage(), _ => CurrentPage < TotalPages);
        }

        private bool FilterItems(object obj)
        {
            if (obj is ItemModel item)
            {
                if (string.IsNullOrWhiteSpace(FilterText))
                    return true;
                return item.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
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
            var items = FilteredItemsView.Cast<ItemModel>()
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
        

        private void EditItem(object parameter)
        {
            if (parameter is ItemModel item)
            {
                System.Diagnostics.Debug.WriteLine($"EditItem called for: {item.Name}");

                // 创建编辑窗口实例，传入选中的项
                EditItemWindow editWindow = new EditItemWindow(item)
                {
                    Owner = Application.Current.MainWindow
                };

                // 显示编辑窗口并等待用户操作
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    // 用户点击了保存，更新原始项的属性
                    item.Name = editWindow.Item.Name;
                    item.Unit = editWindow.Item.Unit;
                    item.Type = editWindow.Item.Type;

                    // 通知界面更新
                    // 因为 ItemModel 已实现 INotifyPropertyChanged，界面会自动更新
                    // 但为了确保分页数据也更新，可以重新调用 UpdatePagedItems
                    UpdatePagedItems();
                    _excelHelper.UpdateItemInExcel(item);
                    System.Diagnostics.Debug.WriteLine($"Item updated: {item.Name}");
                }
            }
        }
        private void AddItem(object parameter)
        {

            ItemModel item = new ItemModel();
            // 创建编辑窗口实例，传入选中的项
            EditItemWindow editWindow = new EditItemWindow(item)
            {
                Owner = Application.Current.MainWindow
            };

            // 显示编辑窗口并等待用户操作
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // 用户点击了保存，更新原始项的属性
                item.Name = editWindow.Item.Name;
                item.Unit = editWindow.Item.Unit;
                item.Type = editWindow.Item.Type;

                item.Index = AllItems.Count + 1;
                AllItems.Add(item);

                // 更新Excel
                _excelHelper.AddItemToExcel(item);
                UpdateTotalPages();
                
                UpdatePagedItems();


            }

        }

        private void DeleteItem(object parameter)
        {
            if (parameter is ItemModel item)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteItem called for: {item.Name}");
                if (MessageBox.Show($"确定要删除 {item.Name} 吗？", "删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    AllItems.Remove(item);
                    UpdateTotalPages();
                    _excelHelper.DeleteItemFromExcel(item.Index);
                    if (CurrentPage > TotalPages)
                        CurrentPage = TotalPages > 0 ? TotalPages : 1;
                    UpdatePagedItems();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
