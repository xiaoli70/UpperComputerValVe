using DataService.Entity;
using EquipmentSignalData.Command;
using EquipmentSignalData.Converter;
using EquipmentSignalData.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace EquipmentSignalData.Views
{
    /// <summary>
    /// UserControlHomePage.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlHomePage : UserControl, INotifyPropertyChanged
    {

        Intetfaces.PLCRead pLCRead = new Intetfaces.PLCRead();
        public ObservableCollection<Valve> Valves { get; set; }
        public ObservableCollection<ValveGroup> ValveGroups { get; set; } = new();

        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SkipCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RelayCommand StartCommand { get; }

        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get => totalTime;
            set
            {
                totalTime = value;
                OnPropertyChanged(nameof(TotalTime));
            }
        }

        private DispatcherTimer timer;


        private int totalCycleCount = 0;

        public int TotalCycleCount
        {
            get => totalCycleCount;
            set
            {
                if (totalCycleCount != value)
                {
                    totalCycleCount = value;
                    OnPropertyChanged(nameof(TotalCycleCount));
                }
            }
        }
        private int totalCycle = 1;

        public int TotalCycle
        {
            get => totalCycle;
            set
            {
                if (totalCycle != value)
                {
                    totalCycle = value;
                    OnPropertyChanged(nameof(TotalCycle));
                }
            }
        }

        private int cycleCount = 0;

        public int CycleCount
        {
            get => cycleCount;
            set
            {
                if (cycleCount != value)
                {
                    cycleCount = value;
                    OnPropertyChanged(nameof(CycleCount));
                }
            }
        }

        private CancellationTokenSource? _cts;
        public bool IsPaused { get; private set; }


        private ProcessState _currentState = ProcessState.Idle;

        public string StartButtonText => _currentState switch
        {
            ProcessState.Idle => "开始",
            ProcessState.Running => "暂停",
            ProcessState.Paused => "恢复",
            _ => "开始"
        };



        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RelayCommand ExportCommand { get; }

        public UserControlHomePage()
        {
            InitializeComponent();

            Valves = App.SharedValves;
            LoadCommand = new RelayCommand(_ => GenerateDataGridColumnss(ValveGroups));
            SaveCommand = new RelayCommand(_ => Save());
            SkipCommand = new RelayCommand(_ => SkipPage());
            CancelCommand = new RelayCommand(_ => CancelTask());

            
            StartCommand = new RelayCommand(async _ => await StartPauseResumeAsync());
            OnPropertyChanged(nameof(StartButtonText));
            ExportCommand = new RelayCommand(_ => ExportPDF());

            TotalTime = new TimeSpan(00, 00, 00);

            GenerateDataGridColumnss(ValveGroups);
            dataGrid.ItemsSource = ValveGroups;
            DataContext = this;
            //_ = setColor();

        }


        public void Save() {
            try
            {
                var valveGroupsContainer = new ValveGroupsContainer { ValveGroups = new List<ValveGroup>(ValveGroups) };

                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var writer = new StreamWriter("ValveGroups.xml"))
                {
                    serializer.Serialize(writer, valveGroupsContainer);
                }

                MessageBox.Show("已保存");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }
        public void SkipPage() {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                // 创建目标页面的 SubItem
                var equipmentLogSubItem = new SubItem("阀门编组", () => new ValveControl());

                // 调用 SwitchScreen 方法跳转页面
                mainWindow.SwitchScreen(equipmentLogSubItem);
            }
        }
        public void ExportPDF()
        {

            new GeneratePdf().GeneratePdfReport(Valves.ToList());

        }
        //删除英文
        public string RemoveNotNumber(string key)
        {
            string str1 = System.Text.RegularExpressions.Regex.Replace(key, @"[A-Za-z]", "");
            return str1;
        }

        public void CancelTask() {
            if (_cts == null)
                return;

            _cts.Cancel();
        }
        public async Task StartPauseResumeAsync()
        {
            switch (_currentState)
            {
                case ProcessState.Idle:
                    _cts = new CancellationTokenSource();
                    _currentState = ProcessState.Running;
                    OnPropertyChanged(nameof(StartButtonText));
                    await StartFor(_cts.Token); // 传递取消令牌
                    break;

                case ProcessState.Running:
                    _currentState = ProcessState.Paused;
                    OnPropertyChanged(nameof(StartButtonText));
                    break;

                case ProcessState.Paused:
                    _currentState = ProcessState.Running;
                    OnPropertyChanged(nameof(StartButtonText));
                    break;
            }
        }


        public async Task StartFor(CancellationToken token)
        {
            XMLHelper xML = new XMLHelper();


            TotalCycleCount = 0;
            totalTime = TimeSpan.Zero;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            for (int k = 0; k < TotalCycle; k++)
            {
                TotalCycleCount ++;
                for (int i = 0; i < ValveGroups.Count; i++)
                {
                    ValveGroups[i].IndicatorColor = "Green";
                    for (int j = 0; j < ValveGroups[i].Cycle; j++)
                    {
                        CycleCount += 1;
                        foreach (var groupValve in ValveGroups[i].Valves)
                        {
                            await WaitWhilePaused(token);  // 监听暂停状态
                            if (token.IsCancellationRequested) {
                                AlarmManager.Instance.ShowError("任务已取消");
                                timer.Stop();
                                ValveGroups[i].IndicatorColor = "Gray";
                                _currentState = ProcessState.Idle;
                                OnPropertyChanged(nameof(StartButtonText));
                                return; 

                            } // 任务被取消
                            Valve? valve = Valves.FirstOrDefault(x => x.Name == groupValve.Name);
                            if (valve != null)
                            {

                                List<string> valves = new List<string> { valve.Name };
                                if (!string.IsNullOrWhiteSpace(groupValve.SubValve))
                                { 
                                    var subValves = groupValve.SubValve.Replace("，", ",").Split(",");
                                    valves.AddRange(subValves);
                                }
                                await ValveController.OpenValvesAsync(valves);
                                
                                await ChangeValveColorAsync(valve, (groupValve.Dose * 1000));

                                await ValveController.CloseValvesAsync(valves);

                            }

                        }
                    }
                    ValveGroups[i].IndicatorColor = "Gray";

                }

            }

           

            timer.Stop();
            _currentState = ProcessState.Idle;
            OnPropertyChanged(nameof(StartButtonText));
        }
        private async Task WaitWhilePaused(CancellationToken token)
        {
            while (_currentState == ProcessState.Paused)
            {
                foreach (var valveGroup in ValveGroups)
                {
                    if (valveGroup.IndicatorColor == "Green")
                        valveGroup.IndicatorColor = "Yellow";
                }
                if (token.IsCancellationRequested) return;
                await Task.Delay(100); // 每 100ms 检查一次
            }
            foreach (var valveGroup in ValveGroups)
            {
                if (valveGroup.IndicatorColor == "Yellow")
                    valveGroup.IndicatorColor = "Green";

            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            TotalTime = TotalTime.Add(TimeSpan.FromSeconds(1));
        }
        private async Task ChangeValveColorAsync(Valve valve, int delay)
        {

            //valve.IndicatorColor = "Green";
            await Task.Delay(delay);
            //valve.IndicatorColor = "Gray";
        }

        private void GenerateDataGridColumnss(ObservableCollection<ValveGroup> valveGroups)
        {


            getValveGroup();


            // 清空之前的列（避免多次调用此方法导致重复列）
            dataGrid.Columns.Clear();

            // 添加 GroupName 列
            DataGridTextColumn groupNameColumn = new DataGridTextColumn
            {
                Header = "Group Name",
                Binding = new System.Windows.Data.Binding("GroupName"),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(groupNameColumn);
            //指示灯列
            DataGridTemplateColumn indicatorColumn = new DataGridTemplateColumn
            {
                Header = "Status",
                CellTemplate = (DataTemplate)FindResource("IndicatorTemplate")
            };
            dataGrid.Columns.Add(indicatorColumn);

            DataGridTextColumn groupCycleColumn = new DataGridTextColumn
            {
                Header = "Cycle",
                Binding = new System.Windows.Data.Binding("Cycle")
            };
            dataGrid.Columns.Add(groupCycleColumn);
            // 动态生成子级阀门列（例如，展示每个 Valve 的 Name 和 IndicatorColor）\\
            int maxValveCount = 0;
            try
            {
                maxValveCount = valveGroups.Max(g => g.Valves.Count);
            }
            catch (Exception)
            {

                return;
            }

            for (int i = 0; i < maxValveCount; i++)
            {
                int index = i; // 本地变量用于捕获循环变量

                // 添加每个阀门的 Name 列
                DataGridTextColumn valveNameColumn = new DataGridTextColumn
                {
                    Header = $"Valve {i + 1}",
                    Binding = new System.Windows.Data.Binding($"Valves[{index}].Name"),
                    IsReadOnly = true
                };
                dataGrid.Columns.Add(valveNameColumn);

                DataGridTextColumn valvezifamenColumn = new DataGridTextColumn
                {
                    Header = $"子阀门 {i + 1}",
                    Binding = new System.Windows.Data.Binding($"Valves[{index}].SubValve")
                };
                dataGrid.Columns.Add(valvezifamenColumn);

                DataGridTextColumn valveColorColumn = new DataGridTextColumn
                {
                    Header = $"Dose",
                    Binding = new System.Windows.Data.Binding($"Valves[{index}].Dose")
                };
                dataGrid.Columns.Add(valveColorColumn);
                DataGridTextColumn valvePurgeColumn = new DataGridTextColumn
                {
                    Header = $"Purge",
                    Binding = new System.Windows.Data.Binding($"Valves[{index}].Purge")
                };
                dataGrid.Columns.Add(valvePurgeColumn);
            }
        }

        private void getValveGroup()
        {

            try
            {
                var serializer = new XmlSerializer(typeof(ValveGroupsContainer));

                using (var reader = new StreamReader("ValveGroups.xml"))
                {
                    var valveGroupsContainer = (ValveGroupsContainer)serializer.Deserialize(reader);
                    ValveGroups.Clear();

                    // 复制加载的阀门组
                    foreach (var group in valveGroupsContainer.ValveGroups)
                    {
                        ValveGroups.Add(group);
                    }
                }
                //AssignValveToGroupCommand.RaiseCanExecuteChanged();
                //MessageBox.Show("编组已从 XML 文件加载。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}");
            }
        }

    }
    public class Valves : INotifyPropertyChanged
    {
        public int ValveId { get; set; }
        private string name;
        private string indicatorColor;
        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string IndicatorColor
        {
            get => indicatorColor;
            set
            {
                if (indicatorColor != value)
                {
                    indicatorColor = value;
                    OnPropertyChanged(nameof(IndicatorColor));
                }
            }
        }
    }

    public enum ProcessState
    {
        Idle,   // 初始状态（未开始）
        Running, // 运行中
        Paused  // 暂停中
    }
}
