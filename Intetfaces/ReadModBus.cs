using Modbus.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intetfaces
{
    public class ReadModBus
    {
        private MbMaster mbMaster = new MbMaster();
        public void Start(string IP, int Porn, int SlaveID)
        {
            mbMaster.HostName = IP;
            mbMaster.Port = Porn;
            mbMaster.SlaveID = Convert.ToByte(SlaveID);
            mbMaster.Connect();
        }
        public ushort[] Read(int Number)
        {
            ushort[] data = mbMaster.ReadData(0, Number);
            if (data != null)
            {
                return data;
            }
            else
            {
                return null;
            }
        }
        public ushort[] Read50(int Number)
        {
            ushort[] data = mbMaster.ReadData(Number);
            if (data != null)
            {
                return data;
            }
            else
            {
                return null;
            }
        }
        public List<string> ReadSeven(int Number)
        {
            ushort[] data = mbMaster.ReadData(0, Number);

            //  ushort result2 = registerValue[1];
            //0x4F3D – 0x4000 = 0x0F3D = 3901（十进制） 
            //3901 / (49152 - 16384) = 11.9 %
            //所以设备返回的瞬时流量值为11.9 % F.S.
            List<string> list = new List<string>();
            string sd = ushortToString(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 2 || i == 3 || i == 11 || i == 12)
                {
                    if (data[i] == 0)
                    {
                        list.Add(0.ToString());
                    }
                    else if (data[i] == 16384)
                    {
                        list.Add(0.25.ToString());
                    }
                    else if (data[i] == 49152)
                    {
                        list.Add(0.75.ToString());
                    }
                    else
                    {
                        int A = Convert.ToInt32(data[i]) - Convert.ToInt32(16384);
                        double B = Math.Round(Convert.ToDouble(A) / (49152 - 16384), 2);
                        list.Add(B.ToString()); ;
                    }
                }
                else
                {
                    list.Add(data[i].ToString());
                }
            }
            if (data != null)
            {
                return list;
            }
            else
            {
                return null;
            }
        }
        public string ushortToString(ushort[] inUshort)

        {

            byte[] outByte = new byte[inUshort.Length * 2];



            for (int i = 0; i < inUshort.Length; i++)

            {
                byte[] bufByte = BitConverter.GetBytes(inUshort[i]);
                outByte[i * 2] = bufByte[0];

                outByte[i * 2 + 1] = bufByte[1];

            }
            string str = ASCIIEncoding.ASCII.GetString(outByte).Trim();
            return str;

        }

        public void Write(int address, int data)
        {

            bool IsWrite = mbMaster.WriteData(address, data);
        }
    }
}
