using DataService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Entity
{
    public class FlowMeter : BaseNotify
    {
        private int id;
        private string name;
        private string ip;
        private int port;
        private string selectedValveIslands;
        private double? alarmValue;

        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Ip
        {
            get => ip;
            set => SetProperty(ref ip, value);
        }
        public int Port
        {
            get => port;
            set => SetProperty(ref port, value);
        }

        public string SelectedValveIslands
        {
            get => selectedValveIslands;
            set => SetProperty(ref selectedValveIslands, value);
        }

        public double? AlarmValue
        {
            get => alarmValue;
            set => SetProperty(ref alarmValue, value);
        }

    }
}
