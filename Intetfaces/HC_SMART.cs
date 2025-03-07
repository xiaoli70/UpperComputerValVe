using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intetfaces
{
    public class HC_SMART
    {
        public ModbusClient client;
        public bool Connected;
        public bool Connect(string ip, int port)
        {
            client = new ModbusClient(ip, port);
            client.ConnectionTimeout = 500;
            try
            {
                client.Connect();
                Connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            client.Disconnect();
            return Connected;
        }

        /// <summary>
        /// 读取PLC-BOOL
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <returns></returns>
        public bool Read_PLC_bit(string 设备类型, string 通讯方式, string 地址区域, int 地址)
        {
            bool return_bool = false;
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int read_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":
                        switch (地址区域)
                        {
                            case "M":
                                read_address = 0;
                                break;
                            case "S":
                                read_address = 57344;
                                break;
                            case "T":
                                read_address = 61440;
                                break;
                            case "C":
                                read_address = 62464;
                                break;
                            case "X":
                                read_address = 63488;
                                break;
                            case "Y":
                                read_address = 64512;
                                break;
                        }
                        break;
                    case "汇川H5U系列":
                        switch (地址区域)
                        {
                            case "M":
                                read_address = 0;
                                break;
                            case "B":
                                read_address = 12288;
                                break;
                            case "S":
                                read_address = 57344;
                                break;
                            case "X":
                                read_address = 63488;
                                break;
                            case "Y":
                                read_address = 64512;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                read_address += 地址;
                try
                {
                    client.Connect();
                    bool[] bools = client.ReadCoils(read_address, 1);
                    return_bool = bools[0];
                    client.Disconnect();//关闭TCP
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return return_bool;
        }

        /// <summary>
        /// 读取PLC-INT 从可以同
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <returns></returns>
        public int Read_PLC_int(string 设备类型, string 通讯方式, string 地址区域, int 地址)
        {


            int return_int = 0;
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int read_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                read_address += 地址;
                try
                {
                    client.Connect();
                    int[] ints = client.ReadHoldingRegisters(read_address, 1);
                    return_int = ints[0];
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return return_int;
        }

        /// <summary>
        /// 读取PLC-DINT
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <returns></returns>
        public Int64 Read_PLC_dint(string 设备类型, string 通讯方式, string 地址区域, int 地址)
        {
            Int64 return_int64 = 0;
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int read_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                read_address += 地址;
                try
                {
                    client.Connect();
                    int[] ints = client.ReadHoldingRegisters(read_address, 2);
                    byte[] bytes1 = BitConverter.GetBytes(ints[0]);
                    byte[] bytes2 = BitConverter.GetBytes(ints[1]);
                    byte[] bytes = new byte[8];
                    bytes[0] = bytes1[0];
                    bytes[1] = bytes1[1];

                    bytes[2] = bytes2[0];
                    bytes[3] = bytes2[1];

                    return_int64 = BitConverter.ToInt64(bytes, 0);
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return return_int64;
        }

        /// <summary>
        /// 读取PLC-REAL
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <returns></returns>
        public float Read_PLC_real(string 设备类型, string 通讯方式, string 地址区域, int 地址)
        {
            float return_float = 0;
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int read_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                read_address = 0;
                                break;
                            case "R":
                                read_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                read_address += 地址;
                try
                {
                    client.Connect();
                    int[] ints = client.ReadHoldingRegisters(read_address, 2);
                    byte[] bytes1 = BitConverter.GetBytes(ints[0]);
                    byte[] bytes2 = BitConverter.GetBytes(ints[1]);
                    byte[] bytes = new byte[8];
                    bytes[0] = bytes1[0];
                    bytes[1] = bytes1[1];

                    bytes[2] = bytes2[0];
                    bytes[3] = bytes2[1];

                    return_float = BitConverter.ToSingle(bytes, 0);
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return return_float;
        }

        /// <summary>
        /// 写PLC-BOOL
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <param name="数据"></param>
        public void Write_PLC_bool(string 设备类型, string 通讯方式, string 地址区域, int 地址, bool 数据)
        {
            // 判断是否为ModbusTCP通讯方式
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int write_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "M":
                                write_address = 0;
                                break;
                            case "S":
                                write_address = 57344;
                                break;
                            case "T":
                                write_address = 61440;
                                break;
                            case "C":
                                write_address = 62464;
                                break;
                            case "X":
                                write_address = 63488;
                                break;
                            case "Y":
                                write_address = 64512;
                                break;
                        }

                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "M":
                                write_address = 0;
                                break;
                            case "B":
                                write_address = 12288;
                                break;
                            case "S":
                                write_address = 57344;
                                break;
                            case "X":
                                write_address = 63488;
                                break;
                            case "Y":
                                write_address = 64512;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                write_address += 地址;
                try
                {
                    client.Connect();
                    client.WriteSingleCoil(write_address, 数据);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// 写PLC-INT
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <param name="数据"></param>   
        public void Write_PLC_int(string 设备类型, string 通讯方式, string 地址区域, int 地址, int 数据)
        {
            // 判断是否为ModbusTCP通讯方式
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int write_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                write_address += 地址;
                try
                {
                    client.Connect();
                    client.WriteSingleRegister(write_address, 数据);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        /// <summary>
        /// 写PLC-DINT
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <param name="数据"></param>
        public void Write_PLC_dint(string 设备类型, string 通讯方式, string 地址区域, int 地址, Int32 数据)
        {
            // 判断是否为ModbusTCP通讯方式
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int write_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                write_address += 地址;
                try
                {
                    client.Connect();
                    byte[] bytes = BitConverter.GetBytes(数据);
                    byte[] bytes1 = new byte[2];
                    byte[] bytes2 = new byte[2];
                    bytes1[0] = bytes[0];
                    bytes1[1] = bytes[1];
                    bytes2[0] = bytes[2];
                    bytes2[1] = bytes[3];

                    int[] ints = new int[2];
                    ints[0] = BitConverter.ToInt16(bytes1, 0);
                    ints[1] = BitConverter.ToInt16(bytes2, 0);
                    client.WriteMultipleRegisters(write_address, ints);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        /// <summary>
        /// 写PLC-REAL
        /// </summary>
        /// <param name="设备类型"></param>
        /// <param name="通讯方式"></param>
        /// <param name="地址区域"></param>
        /// <param name="地址"></param>
        /// <param name="数据"></param>
        public void Write_PLC_real(string 设备类型, string 通讯方式, string 地址区域, int 地址, float 数据)
        {
            // 判断是否为ModbusTCP通讯方式
            if (通讯方式.IndexOf("TCP") > -1)
            {
                int port = 502;
                int write_address = 0;
                string ip_address = "";
                switch (设备类型)
                {
                    case "汇川H3U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 3000;
                                break;
                        }
                        break;
                    case "汇川H5U系列":

                        switch (地址区域)
                        {
                            case "D":
                                write_address = 0;
                                break;
                            case "R":
                                write_address = 12288;
                                break;
                        }
                        break;
                }
                ip_address = 通讯方式.Substring(通讯方式.IndexOf("IP:") + 3, 通讯方式.Length - 通讯方式.IndexOf("IP:", 0) - 3);
                client = new ModbusClient(ip_address, port);

                write_address += 地址;
                try
                {
                    client.Connect();
                    byte[] bytes = BitConverter.GetBytes(数据);
                    byte[] bytes1 = new byte[2];
                    byte[] bytes2 = new byte[2];
                    bytes1[0] = bytes[0];
                    bytes1[1] = bytes[1];
                    bytes2[0] = bytes[2];
                    bytes2[1] = bytes[3];

                    int[] ints = new int[2];
                    ints[0] = BitConverter.ToInt16(bytes1, 0);
                    ints[1] = BitConverter.ToInt16(bytes2, 0);
                    client.WriteMultipleRegisters(write_address, ints);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }


    }
}
