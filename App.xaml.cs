using DataService.Entity;
using EquipmentSignalData.Command;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace EquipmentSignalData
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ObservableCollection<Valve> SharedValves { get; set; } = new ObservableCollection<Valve>();

        protected override async void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly; // 适配DPI
            base.OnStartup(e);
            // 初始化共享阀门数据
            for (int i = 0; i < 64; i++)
            {
                string name = i < 9 ? $"V10{i + 1}" : $"V1{i + 1}";
                SharedValves.Add(new Valve { Name = name, IndicatorColor = "Gray" });

                
            }
            //await new ElectromagneticValveController().CloseValveAsync(SharedValves.Select(v => v.Name).ToArray());
           
        }
    }

}
