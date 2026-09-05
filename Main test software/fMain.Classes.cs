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
        #region ============================================================== Class ==============================================================
        public class Excel
        {
            public bool sameStep { get; set; }
            public string sheetTest { get; set; }
            public string stepTest { get; set; }
            public int[] row { get; set; }
            public string[] alice { get; set; }
            public Workbook workBook { get; set; }
            public Worksheet workSheet { get; set; }
            public Info info { get; set; }
            /// <summary>nan = "NaN"</summary>
            public string nan { get; set; }
            public Color color { get; set; }
            public string lastName { get; set; }
            public PCBA pcba { get; set; }
            /// <summary>pathFile = "../../TestDescription/"</summary>
            public string pathFile { get; set; }

            public Excel()
            {
                sameStep = false;
                sheetTest = "";
                stepTest = "";
                row = new int[36];
                alice = new string[5];
                workBook = new Workbook();
                info = new Info();
                nan = "NaN";
                color = new Color();
                pcba = new PCBA();
                pathFile = "../../TestDescription/";
            }
            public class Info
            {
                /// <summary>nameSheet = "info"</summary>
                public string nameSheet { get; set; }
                /// <summary>column = 2</summary>
                public int column { get; set; }
                /// <summary>rowStart = 7</summary>
                public int rowStart { get; set; }
                /// <summary>rowSequence = 45</summary>
                public int rowSequence { get; set; }
                /// <summary>rowCustomer = 1</summary>
                public int rowCustomer { get; set; }
                /// <summary>rowDetail = 2</summary>
                public int rowDetail { get; set; }
                /// <summary>rowSpecVersion = 4</summary>
                public int rowSpecVersion { get; set; }
                /// <summary>rowFirmware = 5</summary>
                public int rowFirmware { get; set; }
                /// <summary>rowInitial = 82</summary>
                public int rowInitial { get; set; }
                /// <summary>errHead = "_ใน excel page info ไม่ได้กำหนด head"</summary>
                public string errHead { get; set; }

                public Info()
                {
                    nameSheet = "info";
                    column = 2;//เลขคอลั่ม หน้า info
                    rowStart = 7;//แถวเริ่มต้น ของหน้า info


                    //ไอศครีมเชอเบร็ตรสมะนาว
                    rowSequence = 45;//เอาเลขใน excel หน้า intro มาใส่ Sequence head 1 อ่ะ


                    rowCustomer = 1;
                    rowDetail = 2;
                    rowSpecVersion = 4;
                    rowFirmware = 5;
                    rowInitial = 82;
                    errHead = "_ใน excel page info ไม่ได้กำหนด head";
                }
            }
            public class Color
            {
                /// <summary>gold = "Gold"</summary>
                public string gold { get; set; }
                /// <summary>red = "Color2"</summary>
                public string red { get; set; }
                /// <summary>yellow = "Color5"</summary>
                public string yellow { get; set; }
                /// <summary>none = "None"</summary>
                public string none { get; set; }
                /// <summary>skyBlue = "SkyBlue"</summary>
                public string skyBlue { get; set; }

                public Color()
                {
                    gold = "Gold";
                    red = "Color2";
                    yellow = "Color5";
                    none = "None";
                    skyBlue = "SkyBlue";
                }
            }
            public class PCBA
            {
                /// <summary>columnNumber = 1</summary>
                public int columnNumber { get; set; }
                /// <summary>columnDetail = 2</summary>
                public int columnDetail { get; set; }
                /// <summary>columnMin = 3</summary>
                public int columnMin { get; set; }
                /// <summary>columnMax = 4</summary>
                public int columnMax { get; set; }
                /// <summary>errColumn = "_ใน excel คอลั่ม No ต้องมีเลขทุกบรรทัด"</summary>
                public string errColumn { get; set; }

                public PCBA()
                {
                    columnNumber = 1;
                    columnDetail = 2;
                    columnMin = 3;
                    columnMax = 4;
                    errColumn = "_ใน excel คอลั่ม No ต้องมีเลขทุกบรรทัด";
                }
            }
        }
        public class DataLog
        {
            /// <summary>nameFile = "datalog_config"</summary>
            public string nameFile { get; set; }
            public TimeLine timeLine { get; set; }
            public HeadConfig headConfig { get; set; }
            /// <summary>headLog = "Date,Time,Login ID,SW version,FW version,Spec version,Test Time(Sec),Load In/Out(Sec),
            /// Mode,Result,S/N,Failure,"</summary>
            public string headLog { get; set; }
            /// <summary>headLog_header = "Header,"</summary>
            public string headLog_header { get; set; }
            /// <summary>headLog_fgAndWo = "FG,WO,"</summary>
            public string headLog_fgAndWo { get; set; }
            /// <summary>lastNameTXT = ".txt"</summary>
            public string lastNameTXT { get; set; }
            /// <summary>lastNameCSV = ".csv"</summary>
            public string lastNameCSV { get; set; }

            public DataLog()
            {
                nameFile = "datalog_config";
                timeLine = new TimeLine();
                headConfig = new HeadConfig();
                headLog = "Date" + "," + "Time" + "," + "Login ID" + "," + "SW version" + "," + "FW version" + "," +
                    "Spec version" + "," + "Test Time(Sec)" + "," + "Load In/Out(Sec)" + "," + "Mode" + "," + "Result" +
                    "," + "S/N" + "," + "Failure" + ",";

                headLog_header = "Header" + ",";
                headLog_fgAndWo = "FG" + "," + "WO" + ",";
                lastNameTXT = ".txt";
                lastNameCSV = ".csv";
            }

            public class TimeLine
            {
                /// <summary>numFile = "TimeLine#"</summary>
                public string numFile { get; set; }
                public double rowCSV { get; set; }
                public string nameFile { get; set; }
                /// <summary>maxRow = 1000000</summary>
                public double maxRow { get; set; }
                /// <summary>nameFileRow = "row.txt"</summary>
                public string nameFileRow { get; set; }
                public string data { get; set; }


                public TimeLine()
                {
                    nameFile = "TimeLine#";
                    maxRow = 1000000;
                    nameFileRow = "row.txt";
                }
            }
            public class HeadConfig
            {
                public string fileTimeLine { get; set; }


                public HeadConfig()
                {
                    fileTimeLine = "File Time Line";
                }
            }
        }
        public class Define
        {
            /// <summary>fStartConfig = "fStart_config.txt"</summary>
            public string fStartConfig { get; set; }
            /// <summary>pass = "PASS"</summary>
            public string pass { get; set; }
            /// <summary>fail = "FAIL"</summary>
            public string fail { get; set; }
            /// <summary>formClass = "ATS.TestScriptRunner"</summary>
            public string formClass { get; set; }
            /// <summary>testing = "TESTING"</summary>
            public string testing { get; set; }
            public DataGrid dataGrid { get; set; }

            public Define()
            {
                fStartConfig = "fStart_config.txt";
                pass = "PASS";
                fail = "FAIL";
                formClass = "ATS.TestScriptRunner";
                testing = "TESTING";
                dataGrid = new DataGrid();
            }
            public class DataGrid
            {
                /// <summary>columnStep = 0</summary>
                public int columnStep { get; set; }
                /// <summary>columnDetail = 1</summary>
                public int columnDetail { get; set; }
                /// <summary>columnSpec = 2</summary>
                public int columnSpec { get; set; }
                /// <summary>columnMeasure = 3</summary>
                public int columnMeasure { get; set; }
                /// <summary>columnResult = 4</summary>
                public int columnResult { get; set; }

                public DataGrid()
                {
                    columnStep = 0;
                    columnDetail = 1;
                    columnSpec = 2;
                    columnMeasure = 3;
                    columnResult = 4;
                }
            }
        }
        public class Automation
        {
            public bool inTric1 { get; set; }
            public bool inTric2 { get; set; }
            public bool outTric1 { get; set; }
            public bool outTric2 { get; set; }
        }
        public class Server
        {
            private Socket socket { get; set; }
            private List<Socket> listSocket { get; set; }
            private byte[] buffer { get; set; }

            public Server()
            {
                listSocket = new List<Socket>();
                buffer = new byte[65536];
            }
            public void Bind(fMain form, string ip = "127.8.8.8", int port = 2424)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                while (true)
                {
                    try
                    {
                        socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                        socket.Listen(0);
                        socket.BeginAccept(AcceptCallback, null);
                        break;

                    }
                    catch
                    {
                        form.Log(LogMsgType.Error_Red, "Bind Error...");
                        DelaymS(1000);
                    }
                }
            }
            private void AcceptCallback(IAsyncResult resultIAsync)
            {
                Socket socketSup;

                try
                {
                    socketSup = socket.EndAccept(resultIAsync);
                }
                catch
                {
                    return;
                }

                listSocket.Add(socketSup);
                socketSup.BeginReceive(buffer, 0, 65536, SocketFlags.None, ReceiveCallback, socketSup);
                IPEndPoint ipEndPointNewConnect = socketSup.RemoteEndPoint as IPEndPoint;
                socket.BeginAccept(AcceptCallback, null);
            }
            private void ReceiveCallback(IAsyncResult resultIAsync)
            {
                Thread.Sleep(50);
                Socket socketSup = (Socket)resultIAsync.AsyncState;
                int received;

                try
                {
                    received = socketSup.EndReceive(resultIAsync);
                    if (received == 0)
                    {
                        ClientDisConnect(socketSup);
                        return;
                    }

                }
                catch
                {
                    ClientDisConnect(socketSup);
                    return;
                }

                byte[] buf = new byte[received];
                Array.Copy(buffer, buf, received);
                string text = Encoding.ASCII.GetString(buf);

                byte[] data = Encoding.ASCII.GetBytes(text);
                socketSup.Send(data);

                socketSup.BeginReceive(buffer, 0, 65536, SocketFlags.None, ReceiveCallback, socketSup);
            }
            private void ClientDisConnect(Socket socket)
            {
                IPEndPoint IPEndPoint = socket.RemoteEndPoint as IPEndPoint;

                socket.Close();
                listSocket.Remove(socket);
            }
        }
        public class Client
        {
            private Socket socket { get; set; }

            public bool Connect(string ip = "127.1.1.1", int port = 2424)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    return false;
                }

                IAsyncResult result = null;
                bool success;
                try
                {
                    result = socket.BeginConnect(ip, port, null, null);
                    success = result.AsyncWaitHandle.WaitOne(2000, true);
                }
                catch { }

                if (socket.Connected)
                {
                    socket.EndConnect(result);
                    return true;
                }

                socket.Close();
                return false;
            }
            public void Close()
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                try
                {
                    socket.Close();
                }
                catch { }

                try
                {
                    socket.Dispose();
                }
                catch { }
            }
            public void Send(string data)
            {
                Connect();
                bool flagSend = true;
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                try
                {
                    socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch
                {
                    flagSend = false;
                }

                Close();
            }
        }

        public static class Folder
        {
            public static List<string> list = new List<string>();
            /// <summary>driveD = "D:\\"</summary>
            public static readonly string driveD = "D:\\";
            /// <summary>dataBase = " DATA BASE\\"</summary>
            public static readonly string dataBase = " DATA BASE\\";
            /// <summary>operationComplete = "\\Operation Mode\\Complete Test Result\\"</summary>
            public static readonly string operationComplete = "\\Operation Mode\\Complete Test Result\\";
            /// <summary>operationInComplete = "\\Operation Mode\\Incomplete Test Result\\"</summary>
            public static readonly string operationInComplete = "\\Operation Mode\\Incomplete Test Result\\";
            /// <summary>debugComplete = "\\Debug Mode\\Complete Test Result\\"</summary>
            public static readonly string debugComplete = "\\Debug Mode\\Complete Test Result\\";
            /// <summary>debugInComplete = "\\Debug Mode\\Incomplete Test Result\\"</summary>
            public static readonly string debugInComplete = "\\Debug Mode\\Incomplete Test Result\\";
            /// <summary>dataMIS = "D:\\DATA_MIS"</summary>
            public static readonly string dataMIS = "D:\\DATA_MIS";
            /// <summary>timeLine = "TimeLine\\"</summary>
            public static readonly string timeLine = "TimeLine\\";
        }
        public static class DateTimePay
        {
            /// <summary>format = "dd/MM/yyyy,HH:mm:ss,"</summary>
            public static readonly string format = "dd/MM/yyyy,HH:mm:ss,";
            /// <summary>format = "en-US"</summary>
            public static readonly string us = "en-US";
        }
        public static class LastNameExcel
        {
            /// <summary>Value = ".xlsx"</summary>
            public static readonly string excel = ".xlsx";
            /// <summary>Value = ".ods"</summary>
            public static readonly string libre = ".ods";
        }

        public class JsonConvertMain
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string LoginID { get; set; }
            public string SWVersion { get; set; }
            public string FWVersion { get; set; }
            public string SpecVersion { get; set; }
            public string TestTime { get; set; }
            public string LoadInOut { get; set; }
            public string Mode { get; set; }
            public string FinalResult { get; set; }
            public string SN { get; set; }
            public object Failure { get; set; }
            public object Header { get; set; }
            public List<ResultString_> ResultString { get; set; }

            public JsonConvertMain()
            {
                Date = string.Empty;
                Time = string.Empty;
                LoginID = string.Empty;
                SWVersion = string.Empty;
                FWVersion = string.Empty;
                SpecVersion = string.Empty;
                TestTime = string.Empty;
                LoadInOut = string.Empty;
                Mode = string.Empty;
                FinalResult = string.Empty;
                SN = string.Empty;
                Failure = string.Empty;
                ResultString = new List<ResultString_>();
            }
            public class ResultString_
            {
                public string Step { get; set; }
                public string Description { get; set; }
                public string Tolerance { get; set; }
                public string Measured { get; set; }
                public string Result { get; set; }

                public ResultString_()
                {
                    Step = string.Empty;
                    Description = string.Empty;
                    Tolerance = string.Empty;
                    Measured = string.Empty;
                    Result = string.Empty;
                }
            }
        }



        #endregion
    }
}
