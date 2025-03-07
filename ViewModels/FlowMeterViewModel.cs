using DataService.Entity;
using DataService.Helpers;
using EquipmentSignalData.Command;
using EquipmentSignalData.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace EquipmentSignalData.ViewModels
{
    public class FlowMeterViewModel : BaseNotify
    {
        private ObservableCollection<FlowMeter> flowMeters;
        private ObservableCollection<Valve> availableValveIslands;
        public ObservableCollection<ValveGroup> ValveGroups { get; set; } = new();


        public ObservableCollection<FlowMeter> FlowMeters
        {
            get => flowMeters;
            set => SetProperty(ref flowMeters, value);
        }

        public ObservableCollection<Valve> AvailableValveIslands
        {
            get => availableValveIslands;
            set => SetProperty(ref availableValveIslands, value);
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadGroupsCommand { get; }

        public FlowMeterViewModel()
        {
            FlowMeters = new ObservableCollection<FlowMeter>();
            AvailableValveIslands = new ObservableCollection<Valve>(); // 50个阀岛

            SaveCommand = new RelayCommand(_ => SaveGroups());
            LoadGroupsCommand = new RelayCommand(_ => LoadGroups());
            
            
            LoadGroups();
        }

        public void SaveGroups()
        {
            var serializer = new XmlSerializer(typeof(ObservableCollection<FlowMeter>));
            using var writer = new StreamWriter("FlowGroup.xml");
            serializer.Serialize(writer, FlowMeters);
        }

        public void LoadGroups()
        {
            XMLHelper xML = new XMLHelper();
            string filePath = "FlowGroup.xml";
            if (!File.Exists(filePath)) return;

            var serializer = new XmlSerializer(typeof(ObservableCollection<FlowMeter>));
            using var reader = new StreamReader(filePath);
            FlowMeters = (ObservableCollection<FlowMeter>)serializer.Deserialize(reader);

            for (int i = 0; i < FlowMeters.Count; i++) // 13个流量计
            {

                FlowMeters[i].SelectedValveIslands = xML.ReadXmlToFilowGroup(FlowMeters[i].Name);
                ///FlowMeters[i].Name = i < 9 ? $"FC10{i + 1}" : $"FC1{i + 1}";
                
            }

        }


    }


}

