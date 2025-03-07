using Modbus.Device;
using Modbus.Extensions.Enron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;  
using System.Threading.Tasks;

namespace Modbus.Master
{
    public class MbMaster
    {
        private TcpClient client;
        private ModbusIpMaster _modbus;
        private string _hostname;
        private int _port;
        private byte _slaveId;
        private Boolean _isRunning;
        public string HostName { get => _hostname; set { _hostname = value; } }
        public int Port { get => _port; set { _port = value; } }
        public byte SlaveID { get => _slaveId; set { _slaveId = value; } }
        public Boolean isRuning { get { return _isRunning; } }
        public ModbusIpMaster Modbus { get => _modbus; }
        public string ConnectError = "";

        //默认一次最大读取寄存器个数
        private int MaxQty = 100;

        public MbMaster()
        {
            _hostname = "127.0.0.1";
            _port = 502;
            _slaveId = 1;
            _isRunning = false;
        }

        public Boolean Connect()
        {
            try
            {
                client = new TcpClient();
                _modbus = ModbusIpMaster.CreateIp(client);

                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                client.Connect(this._hostname, this._port);
                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                ConnectError = "连接失败：" + ex.Message;
                _isRunning = false;
                return false;
            }
        }

        /// <summary>
        /// 断开通讯连接
        /// </summary>
        /// <returns></returns>
        public void DisConnect()
        {
            _isRunning = false;
            client.Dispose();
            _modbus.Dispose();
        }

        /// <summary>
        /// 批量读取寄存器数值
        /// </summary>
        /// <param name="address">开始地址</param>
        /// <param name="qty">读取地址个数</param>
        /// <returns>寄存器数值（数组）</returns>
        public ushort[] ReadData(int address, int qty)
        {
            try
            {
                ushort[] data;
                int iAdds = address;
                List<ushort> list = new List<ushort>();

                //当读取寄存器个数超过单次最大可读取数量时，分包读取然后合并成数组
                int mCount = qty / MaxQty;//倍数
                int lessCount = qty % MaxQty;//取余数

                for (int i = 0; i < mCount; i++)
                {
                    list.AddRange(_modbus.ReadHoldingRegisters(_slaveId, (ushort)iAdds, (ushort)MaxQty));
                    iAdds = iAdds + MaxQty;
                }
                if (lessCount > 0)
                {
                    // list.AddRange(_modbus.ReadHoldingRegisters(_slaveId, (ushort)iAdds, (ushort)lessCount));

                    ushort[] registerValue = _modbus.ReadHoldingRegisters(_slaveId, (ushort)iAdds, (ushort)lessCount);
                    ushort result = registerValue[0];
                  

                    list.AddRange(_modbus.ReadHoldingRegisters(_slaveId, (ushort)iAdds, (ushort)lessCount));
                }
                data = list.ToArray();
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public float mathFloat(int x1, int x2)  //x1 x2 为读取到浮点数的2个16位寄存器整型数据 
        {
            int fuhao, fuhaoRest, exponent, exponentRest;
            float value, weishu;
            fuhao = x1 / 32768;
            fuhaoRest = x1 % 32768;
            exponent = fuhaoRest / 128;
            exponentRest = fuhaoRest % 128;
            weishu = (float)(exponentRest * 65536 + x2) / 8388608;
            value = (float)Math.Pow(-1, fuhao) * (float)Math.Pow(2, exponent - 127) * (weishu + 1);
            return value;
        }

        /// <summary>
        /// 读取某个寄存器数值
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>寄存器数值</returns>
        public ushort[] ReadData(int address)
        {
            ushort[] data = ReadData(address, 1);
            if (data != null)
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 批量写入数值到寄存器
        /// </summary>
        /// <param name="address">开始地址</param>
        /// <param name="data">待写入数值（数组）</param>
        /// <returns></returns>
        public bool WriteData(int address, ushort[] data)
        {
            try
            {
                int allCount = data.Count();
                int mCount = allCount / MaxQty;//倍数
                int lessCount = allCount % MaxQty;//取余数
                int iAdds = address;
                int iNow = 0;

                for (int i = 0; i < mCount; i++)
                {
                    ushort[] dataWrite1 = new ushort[MaxQty];
                    for (int j = 0; j < MaxQty; j++)
                    {
                        dataWrite1[j] = (ushort)data[iNow];
                        iNow = iNow + 1;
                    }
                    _modbus.WriteMultipleRegisters(_slaveId, (ushort)iAdds, dataWrite1);
                    iAdds = Convert.ToUInt16(iAdds + MaxQty);
                }
                if (lessCount > 0)
                {
                    ushort[] dataWrite2 = new ushort[lessCount];
                    for (int j = 0; j < lessCount; j++)
                    {
                        dataWrite2[j] = (ushort)data[iNow];
                        iNow = iNow + 1;
                    }
                    _modbus.WriteMultipleRegisters(_slaveId, (ushort)iAdds, dataWrite2);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入数值到单个寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool WriteData(int address, ushort data)
        {
            ushort[] wdata = new ushort[1];
            wdata[0] = data;
            return WriteData(address, wdata);
        }

        /// <summary>
        /// 写入数值到单个寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool WriteData(int address, int data)
        {
            ushort[] wdata = new ushort[1];
            wdata[0] = (ushort)data;
            return WriteData(address, wdata);
        }
        public bool WriteOne(int address, int data) {

            _modbus.WriteSingleRegister((ushort)address, (ushort)data);
            return true;
        }
    }
}
