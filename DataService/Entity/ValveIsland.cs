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
    public class ValveIsland: INotifyPropertyChanged
    {
        private string Ip;
        [XmlElement("Ip")]
        public string IP
        {
            get => Ip;
            set => SetProperty(ref Ip, value);
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
