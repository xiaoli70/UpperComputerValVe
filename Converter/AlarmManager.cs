using EquipmentSignalData.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace EquipmentSignalData.Converter
{
    public class AlarmItem : INotifyPropertyChanged
    {
        public string Message { get; set; }

        public string BackgroundColor { get; set; } // 新增属性
        public string PackIcon { get; set; } // 新增属性
        public ICommand CloseCommand { get; }
        public event Action<AlarmItem>? OnClose;

        public AlarmItem(string message, string backgroundColor, string packIcon)
        {
            Message = message;
            BackgroundColor = backgroundColor; // 赋值颜色
            CloseCommand = new RelayCommand(_ => OnClose?.Invoke(this));
            PackIcon = packIcon;
        }

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AlarmManager
    {
        private static readonly Lazy<AlarmManager> _instance =
            new Lazy<AlarmManager>(() => new AlarmManager());

        public static AlarmManager Instance => _instance.Value;

        public ObservableCollection<AlarmItem> Alarms { get; } = new ObservableCollection<AlarmItem>();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private AlarmManager() { }

        //public void ShowError(string message ,bool OutClose = true)
        //{
        //    _dispatcher.Invoke(() =>
        //    {
        //        var alarm = new AlarmItem(message);
        //        alarm.OnClose += RemoveAlarm;
        //        Alarms.Add(alarm);
        //        if (OutClose) { 
        //            //自动移除（可选）
        //             var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        //            timer.Tick += (s, e) => { timer.Stop(); RemoveAlarm(alarm); };
        //            timer.Start();
        //        }
        //    });
        //}
        public void ShowError(string message, bool autoClose = true)
        {
            ShowAlarm(message, "#FFF44336", "AlertCircleOutline", autoClose); // 红色
        }

        public void ShowInfo(string message, bool autoClose = true)
        {
            ShowAlarm(message, "#FF4CAF50", "Success", autoClose); // 绿色
        }

        private void ShowAlarm(string message, string color,string packIcon, bool autoClose)
        {
            _dispatcher.Invoke(() =>
            {
                var alarm = new AlarmItem(message, color,packIcon);
                alarm.OnClose += RemoveAlarm;
                Alarms.Add(alarm);

                if (autoClose)
                {
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        RemoveAlarm(alarm);
                    };
                    timer.Start();
                }
            });
        }
        private void RemoveAlarm(AlarmItem alarm)
        {
            _dispatcher.Invoke(() =>
            {
                if (Alarms.Contains(alarm))
                {
                    Alarms.Remove(alarm);
                }
            });
        }
    }
}
