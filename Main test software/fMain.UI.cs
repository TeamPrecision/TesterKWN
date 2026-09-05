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
        #region====================================================== region Display_Message ======================================================
        private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        public enum LogMsgType { Incoming_Blue, Outgoing_Green, Normal_Black, Warning_Orange, Error_Red };
        public void Log(LogMsgType msgtype, string msg)
        {
            try
            {
                rtfTerminal.Invoke(new EventHandler(delegate
                {
                    rtfTerminal.SelectedText = string.Empty;
                    rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
                    rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
                    rtfTerminal.AppendText(msg);
                    rtfTerminal.ScrollToCaret();

                    if (rtfTerminal.TextLength > 50000)
                    {
                        rtfTerminal.Text = string.Empty;
                    }
                }));
            }
            catch (Exception) { /* suppress: calling Log() here would recurse infinitely */ }
        }
        #endregion
    }
}
