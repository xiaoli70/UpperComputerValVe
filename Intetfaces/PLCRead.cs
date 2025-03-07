using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intetfaces
{
    public class PLCRead
    {
        public bool Connected;
        HC_SMART HC_SMART = new HC_SMART();
        public ModbusClient client;
        public void ReadDate(string IPs,string Data,bool IsOC)
        {
            try
            {
                bool Comstatus = HC_SMART.Connect(IPs, 502);
                if (Comstatus)
                {
                    string SBName = "汇川H5U系列";
                    string IP = "ModbusTCP IP:"+ IPs;
                    string AddressArea = "M";
                    string Address = "20000";
                    
                    HC_SMART.Write_PLC_bool(SBName, IP, AddressArea, Convert.ToInt32(Data), IsOC);
                }
            }
            catch (Exception ex)
            {
            }
        }
      
    }
}
