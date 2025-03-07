using Advantech.Adam;
using Advantech.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Intetfaces
{
    public class ADAM6022
    {
        private PID_Loop m_loopBase;
        private float[] m_pv1LblHigh, m_pv1LblLow;
        private float[] m_pv2LblHigh, m_pv2LblLow;
        private float[] m_dVals;
        private int trackSVValue;

        private int m_iCount;
        private bool m_bStart;
        private AdamSocket adamModbus;
        private Adam6000Type m_Adam6000Type;
        private string m_szIP;
        private int m_iPort;
        public void Connter(string IP, int porn,int Loop)
        {
            m_bStart = false;			// the action stops at the beginning
            //m_szIP = "192.168.1.41";	// modbus slave IP address
            //m_iPort = 502;				// modbus TCP port is 502
            m_szIP = IP;	// modbus slave IP address
            m_iPort = porn;				// modbus TCP port is 502
            adamModbus = new AdamSocket();
            adamModbus.SetTimeout(1000, 1000, 1000); // set timeout for TCP

            m_Adam6000Type = Adam6000Type.Adam6022; // the sample is for ADAM-6050

            m_dVals = new float[3];
            m_pv1LblHigh = new float[2];
            m_pv1LblLow = new float[2];
            m_pv2LblHigh = new float[2];
            m_pv2LblLow = new float[2];
            m_loopBase = (PID_Loop)Loop;
        }

        public string Start()
        {
            bool bRet;
            string NUm = "";
            if (m_bStart) // was started
            {
                m_bStart = false;       // starting flag

                adamModbus.Disconnect(); // disconnect slave          
                return null;
            }
            else	// was stoped
            {
                if (adamModbus.Connect(m_szIP, ProtocolType.Tcp, m_iPort))
                {
                    m_iCount = 0; // reset the reading counter


                    // PID
                 
                    bRet = (adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop0) && adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop1));

                    if (bRet)
                    {
                        m_pv1LblHigh[0] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop0, PID_Addr.Sv1HighLimit);
                        m_pv1LblHigh[1] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop1, PID_Addr.Sv1HighLimit);
                        m_pv1LblLow[0] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop0, PID_Addr.Sv1LowLimit);
                        m_pv1LblLow[1] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop1, PID_Addr.Sv1LowLimit);
                        m_pv2LblHigh[0] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop0, PID_Addr.Sv2HighLimit);
                        m_pv2LblHigh[1] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop1, PID_Addr.Sv2HighLimit);
                        m_pv2LblLow[0] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop0, PID_Addr.Sv2LowLimit);
                        m_pv2LblLow[1] = adamModbus.Pid().GetBufferFloat(PID_Loop.Loop1, PID_Addr.Sv2LowLimit);
                        //RefreshPIDStatic();
                    }
                    else
                    {
                        //MessageBox.Show("Failed to refresh data!", "Error");
                        return null;
                    }



                    m_bStart = true; // starting flag


                    NUm = RefreshPIDValue();
                    return NUm;

                }
                return null;
                //MessageBox.Show("Connect to " + m_szIP + " failed", "Error");
            }
        }

        private string RefreshPIDValue()
        {
            bool bRet;
            int iSVGraphPer, iPVGraphPer, iMVGraphPer;
            float fVal;
            int iVal;

            bRet = adamModbus.Pid().ModbusRefreshBuffer(m_loopBase);

            if (bRet)
            {
                // check divider
                if ((m_pv1LblHigh[Convert.ToInt32(m_loopBase)] == m_pv1LblLow[Convert.ToInt32(m_loopBase)]) ||
                    (m_pv2LblHigh[Convert.ToInt32(m_loopBase)] == m_pv2LblLow[Convert.ToInt32(m_loopBase)]) ||
                    adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.MvRangeHigh) == adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.MvRangeLow))
                    return null;
                // graph bound check
                if (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) > m_pv1LblHigh[Convert.ToInt32(m_loopBase)])
                {
                    if (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) + 1f < adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1RangeHigh))
                        m_pv1LblHigh[Convert.ToInt32(m_loopBase)] = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) + 1f;
                    else
                        m_pv1LblHigh[Convert.ToInt32(m_loopBase)] = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1RangeHigh);
                    //RefreshPIDStatic();
                }
                else if (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) < m_pv1LblLow[Convert.ToInt32(m_loopBase)])
                {
                    if (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) > adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1RangeLow) - 1f)
                        m_pv1LblLow[Convert.ToInt32(m_loopBase)] = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) - 1000;
                    else
                        m_pv1LblLow[Convert.ToInt32(m_loopBase)] = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1RangeLow);
                    //RefreshPIDStatic();
                }
                var fValwe = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Sv1);
                // SV
                iSVGraphPer = Convert.ToInt32((adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Sv1) - m_pv1LblLow[Convert.ToInt32(m_loopBase)]) * 100f /
                    (m_pv1LblHigh[Convert.ToInt32(m_loopBase)] - m_pv1LblLow[Convert.ToInt32(m_loopBase)]));
                // PV
                if (adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.Pv1OpenWireFlag) == 0)
                {
                    fVal = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData);
                    //txtPV.Text = fVal.ToString("#0.000");
                }
                else
                {
                    string txtPVText = "*****";
                }
                iPVGraphPer = Convert.ToInt32((adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData) - m_pv1LblLow[Convert.ToInt32(m_loopBase)]) * 100f /
                    (m_pv1LblHigh[Convert.ToInt32(m_loopBase)] - m_pv1LblLow[Convert.ToInt32(m_loopBase)]));
                int progressBarPVValue = iPVGraphPer;
                // MV
                fVal = (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvEngData) - adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeLow)) * 100f /
                    (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeHigh) - adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeLow));
                iMVGraphPer = Convert.ToInt32(fVal);
                //if (!txtMV.Focused)
                //    txtMV.Text = fVal.ToString("#0.000");
                //progressBarMV.Value = iMVGraphPer;
                // PV alarm
                iVal = adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.Pv1AlarmStatus);
                //adamLightPV.Value = (iVal != 0);
                if (iVal == 0)
                {
                    string erro0 = "Normal";
                }
                else if (iVal == 1)
                {
                    string erro1 = "H-High alarm";
                }
                else if (iVal == 2)
                {
                    string erro2 = "High alarm";
                }
                else if (iVal == 3)
                {
                    string erro3 = "Low alarm";
                }
                else
                {
                    string erro = "L-Low alarm";
                }
                // MV alarm
                iVal = adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.MvAlarmStatus);
                //adamLightMV.Value = (iVal != 0);
                //if (iVal == 0)
                //    txtMVAlarm.Text = "Normal";
                //else if (iVal == 1)
                //    txtMVAlarm.Text = "High alarm";
                //else
                //    txtMVAlarm.Text = "Low alarm";
                m_dVals[0] = Convert.ToSingle(iSVGraphPer);
                m_dVals[1] = Convert.ToSingle(iPVGraphPer);
                m_dVals[2] = Convert.ToSingle(iMVGraphPer);

                return m_dVals[0].ToString() + "*" + m_dVals[1].ToString() + "*" + m_dVals[2].ToString();
            }
            return null;
        }

        //读取SV比例
        public (string cbxLoop, string cbxControl, string txtSV, string pVal, string mVal) RefreshPIDStatic()
        {

            if (adamModbus.Connect(m_szIP, ProtocolType.Tcp, m_iPort))
            {
                bool bRet = adamModbus.Pid().ModbusRefreshBuffer(m_loopBase);
                bRet = (adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop0) && adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop1));
                float fVal;

                //if ((m_pv1LblHigh[Convert.ToInt32(m_loopBase)] == m_pv1LblLow[Convert.ToInt32(m_loopBase)]) ||
                //    (m_pv2LblHigh[Convert.ToInt32(m_loopBase)] == m_pv2LblLow[Convert.ToInt32(m_loopBase)]))
                //    return ("","","");
                int cbxLoop = Convert.ToInt32(m_loopBase);
                int cbxControl = adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.ControlMode);
                // MV text
                //if (adamModbus.Pid().GetBufferInt(m_loopBase, PID_Addr.ControlMode) == 1) // PID auto mode
                //    txtMV.ReadOnly = true;
                //else
                //    txtMV.ReadOnly = false;
                // PV
                // graph's high/low limit         
                //txtGraphHigh.Text = m_pv1LblHigh[Convert.ToInt32(m_loopBase)].ToString("#0.000");
                //txtGraphLow.Text = m_pv1LblLow[Convert.ToInt32(m_loopBase)].ToString("#0.000");
                // SV trackbar

                fVal = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Sv1);
                string txtSV = fVal.ToString();
                float pVal = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Pv1EngData);

                //MV
                float mVal = (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvEngData) - adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeLow)) * 100f /
                  (adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeHigh) - adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.MvRangeLow));
                return (cbxLoop.ToString(), cbxControl.ToString(), txtSV, pVal.ToString(), mVal.ToString());
            }
            else
            {
                return ("", "", "","","");
            }
        }


        public string trackBarSV_ValueChanged(int trackBarSV)
        {
            if (adamModbus.Connect(m_szIP, ProtocolType.Tcp, m_iPort))
            {
                bool bRet = adamModbus.Pid().ModbusRefreshBuffer(m_loopBase);
                bRet = (adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop0) && adamModbus.Pid().ModbusRefreshBuffer(PID_Loop.Loop1));
                float svLarge, fHigh, fLow;
                string szMsg;
                svLarge = (float)trackBarSV;
                //svLarge = m_pv1LblLow[Convert.ToInt32(m_loopBase)] + (m_pv1LblHigh[Convert.ToInt32(m_loopBase)] - m_pv1LblLow[Convert.ToInt32(m_loopBase)]) * trackBarSV / 20f;
                fHigh = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Sv1HighLimit);
                fLow = adamModbus.Pid().GetBufferFloat(m_loopBase, PID_Addr.Sv1LowLimit);

                bRet = adamModbus.Pid().ModbusSetValue(m_loopBase, PID_Addr.Sv1, svLarge);

                if (bRet)
                {
                    adamModbus.Pid().SetBufferFloat(m_loopBase, PID_Addr.Sv1, svLarge);
                    return "OK";
                }
                else { return "Failed to set data!"; }


            }
            else
            {
                return null;
            }

            //if (!RefreshPIDStatic()) { return "Hi-Lo range Setting is invalid!"; }


        }


    }
}
