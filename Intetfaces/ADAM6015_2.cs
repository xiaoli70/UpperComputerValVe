using Advantech.Adam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Intetfaces
{
   
        public class ADAM6015_2
        {
            private string m_szIP;
            private int m_iCount, m_iPort;
            private int m_iStart, m_iLength;
            private bool m_bRegister, m_bStart;
            private AdamSocket adamTCP;

            public void Connter6015(string IP, int porn)
            {
                int iIdx, iPos, iStart;
                m_bStart = false;           // the action stops at the beginning
                m_bRegister = true;         // set to true to read the register, otherwise, read the coil
                m_szIP = IP;    // modbus slave IP address
                m_iPort = porn;             // modbus TCP port is 502
                m_iStart = 1;               // modbus starting address
                m_iLength = 8;              // modbus reading length
                adamTCP = new AdamSocket();
                adamTCP.SetTimeout(1000, 1000, 1000); // set timeout for TCP
                iStart = 40000 + m_iStart;
            }

            public void ModBusStart()
            {
                if (m_bStart) // was started
                {
                    m_bStart = false;       // starting flag
                    adamTCP.Disconnect(); // disconnect slave
                }
                else    // was stoped
                {
                    if (adamTCP.Connect(m_szIP, ProtocolType.Tcp, m_iPort))
                    {
                        m_iCount = 0; // reset the reading counter
                        m_bStart = true; // starting flag
                    }
                }
            }


            public (string first, int[] second) Start()
            {
                int iIdx;
                int[] iData;
                bool[] bData;
                //if (m_bRegister) // Read registers (4X references)
                //{
                // read register (4X) data from slave
                if (adamTCP.Modbus().ReadHoldingRegs(m_iStart, m_iLength, out iData))
                {
                    m_iCount++; // increment the reading counter
                    return ("", iData);
                }
                else
                {
                    return ("Read registers failed!", iData);
                }
                // }
                //else
                //{
                //    // read coil (0X) data from slave
                //    if (adamTCP.Modbus().ReadCoilStatus(m_iStart, m_iLength, out bData))
                //    {
                //        m_iCount++; // increment the reading counter

                //        return ("", bData);
                //    }
                //    else
                //    {
                //        return ("Read coil failed!", iData);
                //    }
                //}
            }
        }
    }
