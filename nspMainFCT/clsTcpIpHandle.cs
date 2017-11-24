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

namespace nspMainFCT
{
    public class clsTcpIpHandle
    {
        //For detect whether or not TCP/IP server is exist or not!
        public SocketLibHoang.TimeOutSocket TimeOutSocket = new SocketLibHoang.TimeOutSocket();
        //Declare system timer for Server (in Server Role) to polling request from Clients
        public System.Timers.Timer SocketTimer = new System.Timers.Timer();
        //Flickering flag to visulaize the rythm of Socket
        public bool flgSocketServerLoop = false;
        //Socket receive data
        public const int MaxSocketLength = 1023;
        public string[] SocketReceiveData = new string[MaxSocketLength];

        /////////////////////SERVER ROLE//////////////////////////////////////////////////////
        //For muti-threading server, we need create each class when receive request from clients
      
        public TcpListener serverSocket;
        public TcpClient clientSocket;
        public int HostPort;
        public IPAddress HostlocalAddr;
        public string strHostIpAddr = "";

        /////////////////////CLIENT ROLE/////////////////////////////////////////////////////
        public int SocketServerWR(string ServerIpAdrr, int ServerPort, string MsgToSend,ref string MsgRcv,ref string MsgErr)
        {
            try
            {
                //First analyze data format correct or not
                //string strCommand = "";
                //string[] strData;
                //int intLen = 0;

                //strData = MsgToSend.Split(',');
                //intLen = strData.Length;

                //int i;

                //for (i = 0; i < intLen; i++)
                //{
                //    strData[i] = strData[i].Trim();
                //    strData[i] = strData[i].ToLower(); //Do not separate UpperCase & LowerCase
                //}
                ////check if start string and ending string is OK
                //if (strData[0].ToLower() != "start")
                //{
                //    MsgErr = "Error, start string is different from 'start'";
                //    return 9000; //Error code
                //}

                //if (strData[intLen - 1].ToLower() != "end")
                //{
                //    MsgErr = "Error, ending string is different from 'end'";
                //    return 9001; //Error code
                //}

                ////Check length of string command is correct?
                //if (intLen < 2)
                //{
                //    MsgErr = "Error, only start and end string only";
                //    return 9002; //Error code
                //}

                //int intTemp = 0;
                //if (int.TryParse(strData[1], out intTemp) == false)
                //{
                //    MsgErr = "Error, the second string: length string is not numerical";
                //    return 9003; //Error code
                //}

                //if (intTemp != intLen)
                //{
                //    MsgErr = "Error, the length of string command is not correct";
                //    return 9004; //Error code
                //}

                //if (intLen < 4)
                //{
                //    MsgErr = "Error, the length of string command is less than 4. Not enough for any command ";
                //    return 9005; //Error code
                //}

                ////If everything is OK. We analyze data and decide how to respond to client here
                //strCommand = strData[2];

                ////Check network connection
                //if (CheckNetWorkPCconnectStatus(ServerIpAdrr) == false)
                //{
                //    MsgErr = "Error, cannot ping to: " + ServerIpAdrr.ToString();
                //    return 9006; //Error code
                //}

                ////First we need check if Server exist or not. With timeout is 2 second!!!
                //bool testBool = false;
                //testBool = TimeOutSocket.TestTcpServerExist(ServerIpAdrr, ServerPort, 2000); //Timeout is 2 second

                ////If server not exist. output message error and return error code
                //if (testBool == false)
                //{
                //    MsgErr = "Cannot find server: " + ServerIpAdrr + "[" + ServerPort + "]";
                //    return 9001; //Error code
                //}

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
                //string[] strDataReturn;
                //strDataReturn = responseData.Split(',');
                //int intLenRet;
                //intLenRet = strDataReturn.Count();
                ////transfer to global array
                //for (i = 0; i < intLenRet; i++)
                //{
                //    SocketReceiveData[i] = strDataReturn[i];
                //}

                //if (intLenRet < 2)
                //{
                //    MsgErr = "Error, the length of return data is less than 2. Maybe communication failed.";
                //    return 9010; //Error code
                //}

                //if (strDataReturn[2] != strCommand)
                //{
                //    MsgErr = "Error, the length of return data have command section is different from sent command";
                //    return 9011; //Error code
                //}

                return 0; //OK code
            }
            catch (Exception ex)
            {
                MsgErr = ex.Message;
                return 9999; //Unexpected Error Code
            }   
        }

        //////////////////////////////COMMON METHOD/////////////////////////////////////
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

        public void SocketIniFileReading()
        {
            //Check if Ini file exist or not
            string strSystemSettingFileName = "SystemIni.ini";
            string strSysSettingFullPath = "";

            strSysSettingFullPath = Application.StartupPath + @"\" + strSystemSettingFileName;

            if (MyLibrary.ChkExist.CheckFileExist(strSysSettingFullPath) == false)
            {
                MessageBox.Show("System File not exist: " + strSysSettingFullPath, "SocketIniFileReading() Error");
                return;
            }

            string strTemp = "";

            try
            {
                this.strHostIpAddr = MyLibrary.ReadFiles.IniReadValue("SOCKET_HOST", "HostIpAddress", strSysSettingFullPath);
                strTemp = MyLibrary.ReadFiles.IniReadValue("SOCKET_HOST", "HostPort", strSysSettingFullPath);
                HostPort = Convert.ToInt32(strTemp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SocketIniFileReading() Error");
            }
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
        /////////////////////////////////////////////////////////////////////////////////////////
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
