using DataService.Entity;
using EquipmentSignalData.Command;
using EquipmentSignalData.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    /// ValveRecipe.xaml 的交互逻辑
    /// </summary>
    public partial class ValveRecipe : UserControl, INotifyPropertyChanged
    {

        public ObservableCollection<ValveRecipeModel> ValveRecipes { get; set; }
        private ValveRecipeModel _selectedRecipe;
        public ValveRecipeModel SelectedValveRecipe 
        {
            get => _selectedRecipe;
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged(nameof(SelectedValveRecipe)); // 触发 UI 更新
            }
        }
        public ICommand AddValveRecipeCommand { get; }
        public ICommand SaveValveRecipeCommand { get; }
        public ICommand LoadValveRecipeCommand { get; }
        public ICommand RunValveRecipeCommand { get; }
        public ValveRecipe()
        {
            InitializeComponent();
            ValveRecipes = new ObservableCollection<ValveRecipeModel>();
            AddValveRecipeCommand = new RelayCommand(_ => AddValveRecipe());
            SaveValveRecipeCommand = new RelayCommand(_ => SaveValveRecipe());
            LoadValveRecipeCommand = new RelayCommand(_ => LoadValveRecipe());
            RunValveRecipeCommand = new RelayCommand(_ => RunValveRecipe());
            LoadValveRecipe();
            SelectedValveRecipe = ValveRecipes.First();
            this.DataContext = this;
        }
        private void AddValveRecipe()
        {
            var newRecipe = new ValveRecipeModel
            { 
                Name = "New Valve Recipe",
                ValveStates = new ObservableCollection<Valve>()
            };
         
            newRecipe.ValveStates= App.SharedValves;
            

            ValveRecipes.Add(newRecipe);
            SelectedValveRecipe = newRecipe;
        }

        private void SaveValveRecipe()
        {
            try
            {
                string json = JsonConvert.SerializeObject(ValveRecipes, Formatting.Indented);
                File.WriteAllText("valve_recipes.json", json);
                AlarmManager.Instance.ShowInfo("配方已保存");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadValveRecipe()
        {
            try
            {
                if (File.Exists("valve_recipes.json"))
                {
                    string json = File.ReadAllText("valve_recipes.json");
                    var loadedRecipes = JsonConvert.DeserializeObject<ObservableCollection<ValveRecipeModel>>(json);

                    if (loadedRecipes != null)
                    {
                        ValveRecipes.Clear();
                        foreach (var recipe in loadedRecipes)
                        {
                            ValveRecipes.Add(recipe);
                        }
                        
                        AlarmManager.Instance.ShowInfo("阀门配方已加载！");
                    }
                }
                else
                {
                    MessageBox.Show("没有找到配置文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunValveRecipe()
        {
            if (SelectedValveRecipe == null) return;

            //foreach (var valve in SelectedValveRecipe.ValveStates)
            //{
            //    Debug.WriteLine($"执行阀门 {valve.ValveId}: {(valve.IsOpen ? "打开" : "关闭")}");
            //}
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class ValveRecipeModel
    {
        public string Name { get; set; }
        public ObservableCollection<Valve> ValveStates { get; set; } = new ObservableCollection<Valve>();
    }
}
