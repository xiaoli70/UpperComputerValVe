using DataService.Entity;
using EquipmentSignalData;
using EquipmentSignalData.Command;
using EquipmentSignalData.Converter;
using EquipmentSignalData.Views;
using Intetfaces;
using System.Collections.ObjectModel;

public static class ValveController
{
    private static readonly XMLHelper xmlHelper = new XMLHelper();
    private static readonly Intetfaces.PLCRead pLCRead = new Intetfaces.PLCRead();
    private static ObservableCollection<Valve> Valves { get; set; } = App.SharedValves;
    /// <summary>
    /// 并行打开多个阀门
    /// </summary>
    /// <param name="valves">需要打开的阀门列表</param>
    public static async Task OpenValvesAsync(List<string> valves)
    {
        if (valves == null || valves.Count == 0)
            return;

        await Parallel.ForEachAsync(valves, async (valve, token) =>
        {
            
            var valveConfig = xmlHelper.ReadNameXml(valve);

            if (valveConfig == null) { AlarmManager.Instance.ShowError("未找到阀门：" + valve); return; }

            var value = Valves.FirstOrDefault(x => x.Name == valve);
            if (value == null) { return; } 
            value.IndicatorColor = "Green";

            await Task.Run(() =>
            {
                pLCRead.ReadDate(valveConfig.IpAddress, valveConfig.Address, true);
            });
        });
    }

    /// <summary>
    /// 并行关闭多个阀门
    /// </summary>
    /// <param name="valves">需要关闭的阀门列表</param>
    public static async Task CloseValvesAsync(List<string> valves)
    {
        if (valves == null || valves.Count == 0)
            return;

        await Parallel.ForEachAsync(valves, async (valve, token) =>
        {
            var valveConfig = xmlHelper.ReadNameXml(valve);
            if (valveConfig == null) return;
            var value = Valves.FirstOrDefault(x => x.Name == valve);
            if (value == null) return;
            value.IndicatorColor = "Gray";
            await Task.Run(() =>
            {
                pLCRead.ReadDate(valveConfig.IpAddress, valveConfig.Address, false);
            });
        });
    }
}