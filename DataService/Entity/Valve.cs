using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataService.Entity
{
    [XmlRoot("Valve")]
    public class Valve : INotifyPropertyChanged
    {
        private int id;
        [XmlElement("Id")]
        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }
        public string IpAddress { get; set; }//储存IP

        private string name;
        [XmlElement("Name")]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        private string processnum;
        [XmlElement("ProcessNum")]
        public string ProcessNum
        {
            get => processnum;
            set => SetProperty(ref processnum, value);
        }

        private string address;
        [XmlElement("Address")]
        public string Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        private string region;
        [XmlElement("Region")]
        public string Region
        {
            get => region;
            set => SetProperty(ref region, value);
        }

        private string dataType;
        [XmlElement("DataType")]
        public string DataType
        {
            get => dataType;
            set => SetProperty(ref dataType, value);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        private int dose = 1;
        public int Dose
        {
            get => dose;
            set => SetProperty(ref dose, value);
        }


        private string subvalve;
        [XmlElement("SubValve")]
        public string SubValve
        {
            get => subvalve;
            set => SetProperty(ref subvalve, value);
        }


        private int openCount;
        public int OpenCount
        {
            get => openCount;
            set
            {
                openCount = value;
                OnPropertyChanged(nameof(OpenCount));
            }
        }

        private string indicatorColor;
        public string IndicatorColor
        {
            get => indicatorColor;
            set
            {
                if (indicatorColor != value)
                {
                    indicatorColor = value;
                    if (indicatorColor == "Green")
                    {
                        OpenCount++;
                    }
                    OnPropertyChanged(nameof(IndicatorColor));
                }
            }
        }


        private int purge = 2;
        public int Purge
        {
            get => purge;
            set => SetProperty(ref purge, value);
        }

        private int groupId;
        [XmlElement("GroupId")]
        public int GroupId
        {
            get => groupId;
            set => SetProperty(ref groupId, value);
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
