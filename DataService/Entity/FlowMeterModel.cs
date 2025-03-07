using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Entity
{

    public class FlowMeterModel : INotifyPropertyChanged
    {
        private int id;
        private string _name;
        private double _sv;
        private double _pv;


        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                }
            }
        }


        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public double SV
        {
            get { return _sv; }
            set
            {
                if (_sv != value)
                {
                    _sv = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PV
        {
            get { return _pv; }
            set
            {
                if (_pv != value)
                {
                    _pv = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
