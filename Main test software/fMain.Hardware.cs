using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Spire.Xls;
using System.IO.Ports;
using System.Management;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Web.Script.Serialization;
using System.Net;
using ATS.MiniForm;
using ATS.Config;
using ATS.Hardware;
using ATS.Models;
using System.Text.RegularExpressions;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace ATS
{
    public partial class fMain
    {
        #region================================================== RS485 relay cards Cotrol =====================================================
        public RelayCtrl rs485 = new RelayCtrl();
        public string comportRS485;
        public void connect_relay(bool initial = true)
        {
            if (!configTester.useRelayCard) return;
            bt_relayCard.BackColor = Color.Red;
            rs485.DisConnectRelay();

            //for display wait scan find port rs485
            Label formFindPort_label = new Label();
            formFindPort_label.Text = "Find prot RS485...";
            formFindPort_label.Size = new Size(350, 75);
            FontFamily fontFamily = new FontFamily("Arial");
            formFindPort_label.Font = new Font(fontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
            Form formFindPort = new Form();
            formFindPort.Size = new Size(400, 100);
            formFindPort.ControlBox = false;
            formFindPort.StartPosition = FormStartPosition.CenterScreen;
            formFindPort.Controls.Add(formFindPort_label);
            formFindPort.Show();

            //use old comport before scan all comport
            string oldComport = setupPay.read_text(ConfigTester.Header.arduinoComport, configTester.nameFile);

            //try open comport
            //if error be goto connect_relay_lable_1
            if (!rs485.ConnectRelay(oldComport, 9600, 8, Parity.None, StopBits.One).Contains("Connected"))
            {
                goto Label_Scan_Comport;
            }

            //read address 1 , address start at 1
            //if read error retrun null
            int? rs485Read = rs485.Read.ReadAllChannel(1);
            if (rs485Read != null)
            {
                Log(LogMsgType.Incoming_Blue, "\n\nConnect Relay Port to " + oldComport);
                File.WriteAllText("ComportMainRelayCard.txt", oldComport); // write file for reserve comport rs485
                comportRS485 = oldComport;
                goto Label_Scan_Address;
            }
            else
            {
                rs485.DisConnectRelay();
            }

        //for scan find comport rs485 control relay
        Label_Scan_Comport:

            string comport = "";
            ManagementObjectSearcher objOSDetails2 = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            ManagementObjectCollection osDetailsCollection2 = objOSDetails2.Get();
            foreach (ManagementObject usblist in osDetailsCollection2)
            {
                if (usblist["Description"].ToString() != "USB-SERIAL CH340" &&
                    usblist["Description"].ToString() != "USB Serial Port" &&
                    usblist["Description"].ToString() != "USB Serial Device") continue;
                string namePortArduino = usblist.GetPropertyValue("NAME").ToString();
                string[] arrport = namePortArduino.Split('(', ')');
                try
                {
                    comport = arrport[1];
                }
                catch
                {
                    Log(LogMsgType.Error_Red, "\nname port not format " + usblist.GetPropertyValue("NAME").ToString());
                    return;
                }

                //try open comport
                //if error be continue
                if (!rs485.ConnectRelay(comport, 9600, 8, Parity.None, StopBits.One).Contains("Connected"))
                {
                    continue;
                }

                //read address 1 , address start at 1
                //if read error retrun null
                rs485Read = rs485.Read.ReadAllChannel(1);
                if (rs485Read != null)
                {
                    Log(LogMsgType.Incoming_Blue, "\n\nConnect Relay Port to " + comport);
                    setupPay.write_text(ConfigTester.Header.arduinoComport, comport, configTester.nameFile);
                    File.WriteAllText("ComportMainRelayCard.txt", comport);
                    comportRS485 = comport;
                    goto Label_Scan_Address;
                }
                else
                {
                    rs485.DisConnectRelay();
                }
            }

            Log(LogMsgType.Error_Red, "\nCannot connect to Realay Card!!");
            formFindPort.Close();
            return;

        //check count of io board
        Label_Scan_Address:
            int numAddressIO = 1;
            for (int loop = 2; loop <= 8; loop++)
            {
                rs485Read = rs485.Read.ReadAllChannel(loop);
                if (rs485Read != null)
                {
                    numAddressIO = loop;
                    continue;
                }
                else
                {
                    break;
                }
            }
            if (numAddressIO == Convert.ToInt32(configTester.numCardRelay))
            {
                bt_relayCard.BackColor = Color.LimeGreen;
            }
            else
            {
                MessageBox.Show("_ใส่การ์ดรีเลย์ไม่ครบ");
            }
            formFindPort.Close();
            OffAllRelay();
        }
        public void OffAllRelay()
        {
            if (!configTester.useRelayCard) return;

            lock (lockRS485)
            {
                rs485.Write.Off_All(Convert.ToInt32(configTester.numCardRelay));
            }
            Log(LogMsgType.Incoming_Blue, "\nOff all relay");
        }
        public void Relay_On(int card, int bit)
        {
            //RelayControl(card.ToString(), bit.ToString(), "1"); DelaymS(50);

            lock (lockRS485)
            {
                rs485.Write.OnChannel(card, bit);
            }
            DelaymS(50);
        }
        public void Relay_Off(int card, int bit)
        {
            //RelayControl(card.ToString(), bit.ToString(), "0"); DelaymS(50);

            lock (lockRS485)
            {
                rs485.Write.OffChannel(card, bit);
            }
            DelaymS(50);
        }


        /// <summary>
        /// For Disable and Enable port
        /// </summary>
        /// <param name="status">True is Enable</param>
        /// <param name="fullNamePort">Name full of serial port</param>
        public void Discom(bool status, string fullNamePort)
        {//enable disable//
            string statusString = string.Empty;
            if (status)
            {
                statusString = "enable";
            }
            else
            {
                statusString = "disable";
            }
            Process devManViewProc = new Process();
            devManViewProc.StartInfo.FileName = "DevManView.exe";
            devManViewProc.StartInfo.Arguments = "/" + statusString + " \"" + fullNamePort + "\"";
            devManViewProc.Start();
            devManViewProc.WaitForExit();

            //string statusString = status ? "enable" : "disable";
            //Process.Start("DevManView.exe", $"/{statusString} \"{fullNamePort}\"").WaitForExit();
        }

        /// <summary>
        /// เอาไว้ get ชื่อเต็มของ device โดยระบุ com... เข้ามาว่าเป็นคอมอะไร
        /// </summary>
        /// <returns></returns>
        public string GetFullNamePortByCom(string comPort)
        {
            ManagementObjectSearcher objOSDetails2 = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            ManagementObjectCollection osDetailsCollection2 = objOSDetails2.Get();
            foreach (ManagementObject usblist in osDetailsCollection2)
            {
                string sup = usblist.GetPropertyValue("NAME").ToString();
                if (sup.Contains(comPort))
                {
                    return sup;
                }
            }
            return string.Empty;
        }
        #endregion
    }
}
