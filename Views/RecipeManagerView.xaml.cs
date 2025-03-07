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
    /// RecipeManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class RecipeManagerView : UserControl, INotifyPropertyChanged
    {

        public ObservableCollection<RecipeModel> Recipes { get; set; }
        private RecipeModel _selectedRecipe;

        public List<string> ActionTypes { get; } = new()
    {
        "流量计开",
        "流量计关",
        "蝶阀开",
        "蝶阀关",
        "阀门开",
        "阀门关",
        "阀岛循环"
    };
        public RecipeModel SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged(nameof(SelectedRecipe)); // 触发 UI 更新
            }
        }

        public ObservableCollection<Valve> Valves { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand AddRecipeCommand { get; }

        public ICommand MoveRecipeUpCommand { get; }
        public ICommand MoveRecipeDownCommand { get; }

        public ICommand DeleteCommand { get; }

        public RecipeManagerView()
        {
            InitializeComponent();
            Valves = App.SharedValves;
            Recipes = new ObservableCollection<RecipeModel>();
            SaveCommand = new RelayCommand(_=>SaveRecipe());
            LoadCommand = new RelayCommand(_=>LoadRecipe());
            RunCommand = new RelayCommand(_=>RunRecipe());
            AddRecipeCommand = new RelayCommand(_ => AddRecipe());  // 新增添加配方命令

            MoveRecipeUpCommand = new RelayCommand(MoveRecipeUp, CanMoveRecipeUp);
            MoveRecipeDownCommand = new RelayCommand(MoveRecipeDown, CanMoveRecipeDown);
            DeleteCommand = new RelayCommand(_ => DeleteSelectedGroup(), _ => CanDelete());
            LoadRecipe();
            SelectedRecipe= Recipes.First();
            this.DataContext = this;
        }

        #region 上移 下移

        private bool CanMoveRecipeUp(object parameter)
        {
            if (parameter is RecipeModel recipe)
            {
                return Recipes.IndexOf(recipe) > 0;
            }
            return false;
        }

        private bool CanMoveRecipeDown(object parameter)
        {
            if (parameter is RecipeModel recipe)
            {
                return Recipes.IndexOf(recipe) < Recipes.Count - 1;
            }
            return false;
        }

        private void MoveRecipeUp(object parameter)
        {
            if (parameter is RecipeModel recipe)
            {
                int index = Recipes.IndexOf(recipe);
                if (index > 0)
                {
                    Recipes.Move(index, index - 1);
                }
            }
        }

        private void MoveRecipeDown(object parameter)
        {
            if (parameter is RecipeModel recipe)
            {
                int index = Recipes.IndexOf(recipe);
                if (index < Recipes.Count - 1)
                {
                    Recipes.Move(index, index + 1);
                }
            }
        }

        #endregion

        private void DeleteSelectedGroup()
        {
            if (SelectedRecipe != null)
            {
                Recipes.Remove(SelectedRecipe);
                SelectedRecipe = null; // 可选：清空选中项
            }
        }
        private bool CanDelete()
        {
            return SelectedRecipe != null; // 只有在有选中项时才允许删除
        }
        private void SaveRecipe()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Recipes, Formatting.Indented);
                File.WriteAllText("recipes.json", json);
                
                AlarmManager.Instance.ShowInfo("配方已保存！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddRecipe()
        {
            var newRecipe = new RecipeModel
            {
                Name = "New Recipe",
                Temperature = 0,
                MFCFlowRate = 0,
                ButterflyValveOpening = 0,
                Steps = new List<StepModel>()
            };

            Recipes.Add(newRecipe);
            SelectedRecipe = newRecipe; // 选中新建的配方，触发 UI 更新
        }
        private void LoadRecipe()
        {
            try
            {
                if (File.Exists("recipes.json"))
                {
                    string json = File.ReadAllText("recipes.json");
                    var loadedRecipes = JsonConvert.DeserializeObject<ObservableCollection<RecipeModel>>(json);

                    if (loadedRecipes != null)
                    {
                        Recipes.Clear();  // 避免直接赋值，确保 UI 绑定
                        foreach (var recipe in loadedRecipes)
                        {
                            Recipes.Add(recipe);
                        }
                        AlarmManager.Instance.ShowInfo("配方已加载！");
                        
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

        private void RunRecipe()
        {
            if (SelectedRecipe == null) return;

            foreach (var step in SelectedRecipe.Steps)
            {
                ExecuteStep(step);
            }
        }

        private void ExecuteStep(StepModel step)
        {
            // 这里调用设备控制逻辑，比如打开阀门、设置流量等
            //Debug.WriteLine($"执行步骤 {step.StepNumber}: {step.Action}, 目标值 {step.TargetValue}, 时长 {step.Duration}ms");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


    public class RecipeModel:INotifyPropertyChanged
    {
        public string Name { get; set; }  // Recipe 名称
        public double Temperature { get; set; }  // 目标温度
        public double MFCFlowRate { get; set; }  // 目标流量
        public double ButterflyValveOpening { get; set; }  // 蝶阀开度
        public List<StepModel> Steps { get; set; } = new List<StepModel>(); // 避免空引用

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class StepModel : INotifyPropertyChanged
    {
        private string _actionType;
        private int? _butterflyValveOpening;
        private string? _valveName;
        private int _duration;

        public int StepNumber { get; set; }

        public string ActionType
        {
            get => _actionType;
            set
            {
                _actionType = value;
                OnPropertyChanged(nameof(ActionType));
               
            }
        }

        public int? ButterflyValveOpening
        {
            get => _butterflyValveOpening;
            set
            {
                _butterflyValveOpening = value;
                OnPropertyChanged(nameof(ButterflyValveOpening));
            }
        }

        public string? ValveName
        {
            get => _valveName;
            set
            {
                _valveName = value;
                OnPropertyChanged(nameof(ValveName));
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
