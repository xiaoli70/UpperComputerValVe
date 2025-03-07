using DataService.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataService.Entity
{
    [XmlRoot("ValveGroup")]
    public class ValveGroup : INotifyPropertyChanged
    {
        private int groupId;
        public int GroupId
        {
            get => groupId;
            set => SetProperty(ref groupId, value);
        }

        private string groupName;
        public string GroupName
        {
            get => groupName;
            set => SetProperty(ref groupName, value);
        } 

        private string indicatorColor = "Gray";
        public string IndicatorColor
        {
            get => indicatorColor;
            set => SetProperty(ref indicatorColor, value);
        }


        private int cycle=1;
        public int Cycle
        {
            get => cycle;
            set => SetProperty(ref cycle, value);
        }

        private ObservableCollection<Valve> valves = new();
        [XmlArray("Valves")]
        [XmlArrayItem("Valve")]
        public ObservableCollection<Valve> Valves
        {
            get => valves;
            set => SetProperty(ref valves, value);
        }


        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
