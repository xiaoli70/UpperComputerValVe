using DataService.Entity;
using Intetfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentSignalData.Command
{
    public class ElectromagneticValveController
    {
        PLCRead pLCRead = new Intetfaces.PLCRead();
        XMLHelper xML = new XMLHelper();

        /// <summary>
        /// 异步打开电磁阀的方法
        /// </summary>
        /// <returns>任务</returns>
        public async Task OpenValveAsync(string[] listNames)
        {
            foreach (var name in listNames)
            {
                Valve valve = xML.ReadNameXml(name);

                await Task.Run(() =>
                    pLCRead.ReadDate(valve.IpAddress, valve.Address, true)
                );

                Console.WriteLine($"电磁阀 '{name}' 已成功打开。");
            }
        }

        public async Task CloseValveAsync(string[] listNames)
        {
            foreach (var name in listNames)
            {

                Valve valve = xML.ReadNameXml(name);

                await Task.Run(() =>
                    pLCRead.ReadDate(valve.IpAddress, valve.Address, false)

                    );

            }
           
        }


    }
}
