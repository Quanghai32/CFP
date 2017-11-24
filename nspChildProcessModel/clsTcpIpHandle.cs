//'********************************************************************************************************************************************
//'*  SOCKET SEVER - CLIENTS COMMUNICATION MODULE
//'*  AUTHOR: HOANG DO VAN
//'*  Too1.00: June/10/2013. Hoang PDE.
//'*  Tool1.01: Nov/07/2013. Hoang PDE.
//'****************************************************************************************************************************
//'*  SOCKET SERVER - CLIENTS COMMUNICATION FRAME
//'**********************************************
//'*  Desc: This communication frame, using for develope data exchange between applications between different PCs.
//'*  Warning:
//'*      1. Do not modify any code if you do not understand
//'*      2. The author, will be not reponsible for any kind of risk or damage caused by this module
//'*      3. If you have any question, please contact to Hoang-TL PDE-CVN
//'**********************************************
//'*  Clients will send data to Server with data format:
//'*      "start",Length,request command,parameter 1,parameter 2, .... ,parameter n,"end"
//'*      Example: "start,04,0000,end"
//'*  Server will respond to client with data format:
//'*      "start",Length,requested command,parameter 1,parameter 2,....,parameter m ,"end"
//'*  Note:
//'*      1. Any data sending between Clients & Server must be begin with string "start" and terminate with "end" string
//'*      2. Length: indicate how many sub string have in string
//'*      3. 'request command' and 'requested command' must be same, to ensure that respond data is true with requested data
//'********************************************************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLibHoang;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Timers;
using System.Threading;
using System.Windows.Forms;

namespace nspChildProcessModel
{
    public class clsTcpIpHandle
    {
        public clsChildControlModel objChildControlModel { get; set; }

        //For detect whether or not TCP/IP server is exist or not!
        public SocketLibHoang.TimeOutSocket TimeOutSocket = new SocketLibHoang.TimeOutSocket();
        //Declare system timer for Server (in Server Role) to polling request from Clients
        public System.Timers.Timer SocketTimer = new System.Timers.Timer();
        //Flickering flag to visulaize the rythm of Socket
        public bool flgSocketServerLoop = false;
        //Socket receive data
        public const int MaxSocketLength = 1023;
        public string[] SocketReceiveData = new string[MaxSocketLength];

        public TcpListener ServerSocket;
        public TcpClient clientSocket;
        public int intHostPort;
        public IPAddress IPHostlocalAddr;
        public string strHostIpAddr = "";

        //******************************************************************************************************************************************************************

        public void SocketBackGroundTask()
        {
            //Flickering flag
            flgSocketServerLoop = !(flgSocketServerLoop);
            //Server loop
            ServerLoop();
        }

        public void ServerLoop()
        {
            if (ServerSocket.Pending() == true)
            {
                clientSocket = ServerSocket.AcceptTcpClient();
                handleClient client = new handleClient(this.objChildControlModel);
                client.startClient(clientSocket);
            }
        }

        //*****************************************************************************CLIENT ROLE**************************************************************************************
        public int SocketServerWR(string ServerIpAdrr, int ServerPort, string MsgToSend,ref string MsgRcv,ref string MsgErr)
        {
            try
            {
                //First analyze data format correct or not
                string strCommand = "";
                string[] strData;
                int intLen = 0;

                strData = MsgToSend.Split(',');
                intLen = strData.Length;

                int i;

                for (i = 0; i < intLen; i++)
                {
                    strData[i] = strData[i].Trim();
                    strData[i] = strData[i].ToLower(); //Do not separate UpperCase & LowerCase
                }
                //check if start string and ending string is OK
                if (strData[0].ToLower() != "start")
                {
                    MsgErr = "Error, start string is different from 'start'";
                    return 9000; //Error code
                }

                if (strData[intLen - 1].ToLower() != "end")
                {
                    MsgErr = "Error, ending string is different from 'end'";
                    return 9001; //Error code
                }

                //Check length of string command is correct?
                if (intLen < 2)
                {
                    MsgErr = "Error, only start and end string only";
                    return 9002; //Error code
                }

                int intTemp = 0;
                if (int.TryParse(strData[1], out intTemp) == false)
                {
                    MsgErr = "Error, the second string: length string is not numerical";
                    return 9003; //Error code
                }

                if (intTemp != intLen)
                {
                    MsgErr = "Error, the length of string command is not correct";
                    return 9004; //Error code
                }

                if (intLen < 4)
                {
                    MsgErr = "Error, the length of string command is less than 4. Not enough for any command ";
                    return 9005; //Error code
                }

                //If everything is OK. We analyze data and decide how to respond to client here
                strCommand = strData[2];

                //Check network connection
                if (CheckNetWorkPCconnectStatus(ServerIpAdrr) == false)
                {
                    MsgErr = "Error, cannot ping to: " + ServerIpAdrr.ToString();
                    return 9006; //Error code
                }

                //First we need check if Server exist or not. With timeout is 2 second!!!
                bool testBool = false;
                testBool = TimeOutSocket.TestTcpServerExist(ServerIpAdrr, ServerPort, 2000); //Timeout is 2 second

                //If server not exist. output message error and return error code
                if (testBool == false)
                {
                    MsgErr = "Cannot find server: " + ServerIpAdrr + "[" + ServerPort + "]";
                    return 9001; //Error code
                }

                //Create a TcpClient.
                //Note, for this client to work you need to have a TcpServer 
                //connected to the same address as specified by the server, port combination.
                TcpClient client = new TcpClient(ServerIpAdrr, ServerPort);
                //Translate the passed message into ASCII and store it as a Byte array
                byte[] data = System.Text.Encoding.ASCII.GetBytes(MsgToSend);
                //Get a client stream for reading and writing
                NetworkStream stream = client.GetStream();
                //Send the message to the connected TcpServer
                stream.Write(data, 0, data.Length);

                //Receive the TcpServer.response.
                //Buffer to store the response bytes.
                data = new byte[1024]; 
                //String to store the response ASCII representation.
                string responseData = string.Empty;
                //Read the first batch of the TcpServer response bytes.
                int bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                MsgRcv = responseData;
                //Close everything
                stream.Close();
                client.Close();
                MsgErr = "";

                //Analyze data feedback
                string[] strDataReturn;
                strDataReturn = responseData.Split(',');
                int intLenRet;
                intLenRet = strDataReturn.Count();
                //transfer to global array
                for (i = 0; i < intLenRet; i++)
                {
                    SocketReceiveData[i] = strDataReturn[i];
                }

                if (intLenRet < 2)
                {
                    MsgErr = "Error, the length of return data is less than 2. Maybe communication failed.";
                    return 9010; //Error code
                }

                if (strDataReturn[2] != strCommand)
                {
                    MsgErr = "Error, the length of return data have command section is different from sent command";
                    return 9011; //Error code
                }

                return 0; //OK code
            }
            catch (Exception ex)
            {
                MsgErr = ex.Message;
                return 9999; //Unexpected Error Code
            }   
        }

        //****************************************************************************COMMON METHOD*************************************************************************************
        public string SocketServerStart()
        {
            //Check if IP address in setting file (Systemini.ini) match IP address of PC or not
            int i = 0;
            string strTemp = "";
            //Initialization for socket receive data array
            for (i = 0; i < MaxSocketLength; i++)
            {
                SocketReceiveData[i] = "";
            }

            strTemp = GetIPv4Address();

            if (strTemp != strHostIpAddr)
            {
                //MessageBox.Show("IP address not match: Setting file[" + strHostIpAddr + "] <> PC setting[" + strTemp + "]. Please check!", "SocketServerStart() Error");
                return "Error: " + "IP address not match: Setting file[" + strHostIpAddr + "] <> PC setting[" + strTemp + "]";
            }

            try
            {
                this.IPHostlocalAddr = IPAddress.Parse(strHostIpAddr);
                this.ServerSocket = null;
                this.clientSocket = null;
                this.ServerSocket = new TcpListener(IPHostlocalAddr, intHostPort);
                this.ServerSocket.Start();

                RunTimer();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "SocketServerStart() Error");
                return "Error: " + ex.Message;
            }

            return "0";
        }

        public string GetIPv4Address()
        {
            try
            {
                string strRet = string.Empty;
                string strHostName = System.Net.Dns.GetHostName();
                System.Net.IPHostEntry iphe = System.Net.Dns.GetHostEntry(strHostName);

                foreach (System.Net.IPAddress ipheal in iphe.AddressList)
                {
                    if (ipheal.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        strRet = ipheal.ToString();
                    }
                }
                return strRet;
            }
            catch
            {
                return "error";
            }
        }

        public string SocketIniFileReading()
        {
            //Check if Ini file exist or not
            string strSystemSettingFileName = "SystemIni.ini";
            string strSysSettingFullPath = "";

            strSysSettingFullPath = Application.StartupPath + @"\" + strSystemSettingFileName;

            if (MyLibrary.ChkExist.CheckFileExist(strSysSettingFullPath) == false)
            {
                //MessageBox.Show("System File not exist: " + strSysSettingFullPath, "SocketIniFileReading() Error");
                return "Error: System File not exist. " + strSysSettingFullPath;
            }

            string strTemp = "";

            try
            {
                //Get IP Address Config
                strTemp = MyLibrary.ReadFiles.IniReadValue("SOCKET_HOST", "HostIpAddress", strSysSettingFullPath);
                if ((strTemp != "error") && (strTemp.Trim()!="")) //Config & config not empty
                {
                    this.strHostIpAddr = strTemp.Trim();
                }
                else //No config => turn to default is current setting of PC
                {
                    this.strHostIpAddr = this.GetIPv4Address();
                }

                //Get Host Port Config
                strTemp = MyLibrary.ReadFiles.IniReadValue("SOCKET_HOST", "HostPort", strSysSettingFullPath);
                if (strTemp=="Error")
                {
                    MessageBox.Show("HostPort setting error. Auto config to CFP default host port: 14000", "SocketIniFileReading() warning");
                    strTemp = "14000";
                }

                if(int.TryParse(strTemp.Trim(), out intHostPort)==false)
                {
                    MessageBox.Show("HostPort setting error. Auto config to CFP default host port: 14000", "SocketIniFileReading() warning");
                    intHostPort = 14000;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "SocketIniFileReading() Error");
                return "Error: unexpected error happened. " + ex.Message;
            }

            //If everything is OK. Return 0
            return "0";
        }

        public bool CheckNetWorkPCconnectStatus(string strPC_IPAdrress)
        {
            //Test Pinging for PC
            int intRetry;
            int i;

            intRetry = 5;
            for (i = 0; i < intRetry; i++)
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(strPC_IPAdrress, 1000);
                if (reply != null)
                {
                    return true;
                }
            }
            return false;
        }

        //////////////////////////THREAD TIMER//////////////////////////////////////////////////////
        public void RunTimer()
        {
            //Create a timer with a ten second interval.
            SocketTimer = new System.Timers.Timer(10000);
            //Hook up the Elapsed event for the timer.
            SocketTimer.Elapsed += OnSocketTimedEvent;
            //Set timer interval to 100ms
            SocketTimer.Interval = 100;
            SocketTimer.Enabled = true;
        }

        private void OnSocketTimedEvent(Object source, ElapsedEventArgs e)
        {
            SocketTimer.Enabled = false;
            ////////////////////////////
            try
            {
                SocketBackGroundTask();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "OnSocketTimedEvent() Error");
            }
            ////////////////////////////
            SocketTimer.Enabled = true;
        }
        /////////////////////////SOCKET SERVER CLOSE//////////////////////////////////////////////
        public void SocketServerClose()
        {
            SocketTimer.Enabled = false;
            ServerSocket.Stop();
        }
        /////////////////////////////////////////////////////////////////////////////////////////

        //Constructor
        public clsTcpIpHandle(clsChildControlModel objChildControlModel)
        {
            this.objChildControlModel = objChildControlModel;
        }

    }

    /////////////////////////////////////////////////
    public class handleClient
    {
        public TcpClient clientSocket;
        public string clNo;
        public Thread ctThread;

        public clsChildControlModel objChildControlModel { get; set; }

        public void startClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            this.ctThread = new Thread(HandleSocketMsg);
            ctThread.Start();
        }
        private void HandleSocketMsg()
        {
            //Buffer for reading data
            byte[] bytes = new byte[1024];
            string data = "";
            string strDataReceive = "";
            string strDataServerRespond = "";

            //Enter the listening loop
            while (true)
            {
                data = "";
                strDataReceive = "";

                //Get a stream object for reading and writing
                NetworkStream stream = clientSocket.GetStream();
                byte[] msg;
                int i = 0;
                if (stream.DataAvailable == true)
                {
                    i = stream.Read(bytes, 0, bytes.Length);
                }

                if (i != 0)
                {
                    while (i != 0)
                    {
                        //Translate data bytes to a ASCII string
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        strDataReceive = strDataReceive + data;
                        if (i < 10)
                        {
                            break;
                        }
                        //Process the data sent by the client
                        data = data.ToUpper();
                        if (stream.DataAvailable == true)
                        {
                            i = stream.Read(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            i = 0;
                        }
                    }

                    //********************************************************
                    UserSocketServerHandle(strDataReceive, ref strDataServerRespond);


                    msg = System.Text.Encoding.ASCII.GetBytes(strDataServerRespond);
                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    strDataReceive = "";
                    stream.Flush();
                } //End if

                this.clientSocket.Close();
                this.ctThread.Abort();

            } //End while

        } //End Method

        public void UserSocketServerHandle(string strInServer, ref string strServerOut)
        {

            #region IniCheck
            //First analyze data format correct or not
            string strCommand = "";
            string[] strData;
            int intLen = 0;
            string strTemp = "";

            strData = strInServer.Split(',');
            intLen = strData.Length;

            int i;

            for (i = 0; i < intLen; i++)
            {
                strData[i] = strData[i].Trim();
                strData[i] = strData[i].ToLower(); //Do not separate UpperCase & LowerCase
            }

            //check if start string and ending string is OK
            if (strData[0].ToLower() != "start")
            {
                strTemp = "Error, start string is different from 'start'";
                return; //Error code
            }

            if (strData[intLen - 1].ToLower() != "end")
            {
                strTemp = "Error, ending string is different from 'end'";
                return;
            }

            //Check length of string command is correct?
            if (intLen < 2)
            {
                strTemp = "Error, only start and end string only";
                return; //Error code
            }

            int intTemp = 0;
            if (int.TryParse(strData[1], out intTemp) == false)
            {
                strTemp = "Error, the second string: length string is not numerical";
                return; //Error code
            }

            if (intTemp != intLen)
            {
                strTemp = "Error, the length of string command is not correct";
                return; //Error code
            }

            if (intLen < 4)
            {
                strTemp = "Error, the length of string command is less than 4. Not enough for any command ";
                return; //Error code
            }

            //If everything is OK. We analyze data and decide how to respond to client here
            strCommand = strData[2];

            #endregion

            switch (strCommand)
            {
                case "0000": //"start,04,0000,end" - Return what received
                    strServerOut = strInServer;
                    break;

                case "0001": //"start,04,0001,end" - Request basic Information
                    strServerOut = strInServer;
                    break;

                case "0002": //"start,04,0002,end" - Start checking request  
                    //objChildControlModel.MasterStartCheck();

                    strServerOut = "start,05,0002,Checking started OK,end";
                    break;

                case "0003": //"start,04,0003,end" - Get latest checking result

                    //string strResult = "FAIL";
                    //if (Program.strcMainVar.blTotalCheckingResult == true) strResult = "PASS";

                    ////Cal return string
                    //strServerOut = "start,00,0003," +
                    //    "Result," + strResult + "," +
                    //    "Pass rate," + Math.Round(Program.strcMainVar.dblTotal_PassRate,0).ToString() + "," +
                    //    "Checking Times," + Program.strcMainVar.intTotal_CheckCount.ToString() + "," +
                    //    "Result Pass Times," + Program.strcMainVar.intTotal_CheckPass.ToString() + "," + 
                    //    "end";

                    break;

                case "0004": //"start,04,0004,end" - Get Master Step List Info
                    strServerOut = strInServer;
                    break;

                default:
                    break;
            }

        }

        //Constructor
        public handleClient(clsChildControlModel objChildControlModel)
        {
            this.objChildControlModel = objChildControlModel;
        }
    }
}




namespace SocketLibHoang
{
    public class TimeOutSocket
    {
        public static bool IsConnectionSuccessful = false;
        public static Exception socketexception;
        public static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        public static bool TestTcpServerExist(string serverip, int serverport, int timeoutMSec)
        {
            //Reset all varibles
            IsConnectionSuccessful = false;
            TimeoutObject = new ManualResetEvent(false);

            TcpClient tcpclient = new TcpClient();

            tcpclient.BeginConnect(serverip, serverport, new AsyncCallback(CallBackMethod), tcpclient);

            if (TimeoutObject.WaitOne(timeoutMSec, false))
            {
                if (IsConnectionSuccessful)
                {
                    tcpclient.Close();
                    return true;
                }
                else
                {
                    tcpclient.Close();
                    return false;
                    throw socketexception;
                }

                //tcpclient.Close();
            }
            else
            {
                tcpclient.Close();
                return false;
                throw new TimeoutException("TimeOut Exception");
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncresult"></param>
        private static void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(asyncresult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
        //////////////////////////////////////////////////////////////

    }
}

