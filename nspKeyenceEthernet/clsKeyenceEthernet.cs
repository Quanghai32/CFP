/////////////////////////////////////////////                                            ///////////////////////////////////////////
/////////////////////////////////////////////                                              ///////////////////////////////////////////
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                              ///////////////////////////////////////////
/////////////////////////////////////////////                                            ///////////////////////////////////////////
///////////////////////////////////////////////////////// KEEP IT SIMPLE, STUPID! ///////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  PLUG-IN REQUIREMENTS
//  1. Plug-in must use MEF technology from Microsoft to export parts to Host program
//  2. Plug-in must use common INTERFACE from "nspINTERFACE.dll" to communicate to Host Program
//  3. Plug-in must Export type of "nspINTERFACE.IPluginExecute" for Host program recognize plug-in
//  4. Plug-in must Export metadata follow format: [ExportMetadata("IPluginInfo", "PluginDescription")]
//      + "IPluginInfo" : Do not change this. This string for Host program recognize plug-in
//      + "PluginDescription" : This string describe plug-in. You can freely choose string for description.
//  5. Plug-in inherited from "nspINTERFACE.IPluginExecute" interface, therefore must implement methods inside this interface:
//      + public void IGetPluginInfo(out List<string> lststrInfo)
//          => you must output a list of string to describe what function your plug-in support. Follow this format:
//              "JigID,HardID,FuncID1,FuncID2,...,FuncIDn"
//              + JigID: must be numeric format
//              + HardID: must be numeric format
//              + FuncID1,FuncID2,...,FuncIDn: must be numeric format. List of function ID support correspond to JigID, HardID.
//      + public string IFunctionExecute(List<List<string>> lstlststrInput, out List<List<string>> lststrOutput)
//          => this method reponse input info from Host program
//              + lstlststrInput: infor receive from Host program, this list have following format:
//                  - lstlststrInput[0]: Include most of often information needed from Host & steplist:
//                  "Application startup path - Process ID - Test No - Test Name - Test Class - Lo Limit - Hi Limit - Unit - Jig ID - Hardware ID - Func ID -
//                   Transmission - Receive - Para1 - ... - Para20 - Jump command - Signal Name - Port Measure Point - Check Pads - Control spec/Comment - Note"
//                  - lstlststrInput[0]: Other information from Host program:
//                      "InfoCode - Info1 - Info2 - Info3 - ..."
//              + lststrOutput: output information from plug-in (under List of list of string format)
/////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketLibHoang;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ComponentModel.Composition;
using nspINTERFACE;
using System.Windows.Forms;

using System.Timers;
using System.Threading;

namespace nspKeyenceEthernet
{

    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginKeyenceEthernet,201")]
    public class clsKeyenceEthernet : nspINTERFACE.IPluginExecute
    {
        //User Form
        windowPLCSetting windowPLCSetting = new windowPLCSetting();
        public string strKeyMemoryArea = "DM700"; //Default is DM700

        //Address of Keyence PLC
        public string strPlcServerIpAddress;
        public Int32 intPlcServerPort;
        public bool blFlagPlcBusy = false;

        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "201,0,0,1,1000"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 30/Aug/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add Form Setting"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Process ID - ... - JigID-HardID-FuncID"
            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());
            switch (intJigID) //Select JigID
            {
                case 201:
                    return SelectHardIDFromJigID201(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID201(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intHardID = 0;
            if (int.TryParse(lstlstobjInput[0][9].ToString(), out intHardID) == false) return "error 1"; //Not numeric error
            intHardID = int.Parse(lstlstobjInput[0][9].ToString());
            switch (intHardID) //Select HardID
            {
                case 0:
                    return SelectFuncIDFromHardID0(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "PluginMicroUSB: Unrecognize HardID " + intHardID.ToString();
            }
        }

        public object SelectFuncIDFromHardID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intFuncID = 0;
            if (int.TryParse(lstlstobjInput[0][10].ToString(), out intFuncID) == false) return "error 1"; //Not numeric error
            intFuncID = int.Parse(lstlstobjInput[0][10].ToString());
            switch (intFuncID) //Select FuncID
            {
                case 0:
                    return PluginKeyenceEthernetFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginKeyenceEthernetFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 1000:
                    return PluginKeyenceEthernetFuncID1000(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }


        /// <summary>
        /// Reading Ini File & assign address & port for Keyence PLC server
        ///     + Para1 (13): File Name to get setting Info
        ///     + Para2 (14): Section name to get setting Info
        ///     + Para3 (15): Keyname to get IP address setting Info
        ///     + Para4 (16): Keyname to get Host Port setting Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginKeyenceEthernetFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();


            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: File is not exist"; //File not exist code
            }

            string strTmp = "";

            //Reading IP address setting
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), lstlstobjInput[0][15].ToString(), strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: fail to read file"; //Getting key value fail
            }
            //Assign for IP address
            this.strPlcServerIpAddress = strTmp;

            //Reading Host Port setting
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), lstlstobjInput[0][16].ToString(), strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: fail to read file"; //Getting key value fail
            }
            //check if host port setting is numeric or not & assign value for Host Port
            if (Int32.TryParse(strTmp, out this.intPlcServerPort) == false) return "Error: Host Port setting is not integer format!";

            //MessageBox.Show(this.strPlcServerIpAddress);
            //MessageBox.Show(this.intPlcServerPort.ToString());

            //If everything is OK, then return OK code
            return "0";
        }


        /// <summary>
        /// Call SocketServerWR() function
        ///     + Para1 (13): Socket command to send
        ///     + Para2 (14): option for display message - 1: display. 0(default): No display
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginKeyenceEthernetFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            int intRet = 0;
            string MsgRcv = "";
            string MsgErr = "";
            int i = 0;

            if (blFlagPlcBusy == true) return "Error: PLC busy!";

            //At 1 time, only allow 1 thread to access this class! prevent racing problem, we lock it!
            blFlagPlcBusy = true;
            lock (this)
            {
                try
                {
                    intRet = SocketServerWR(this.strPlcServerIpAddress, this.intPlcServerPort, lstlstobjInput[0][13].ToString().Trim(), ref MsgRcv, ref MsgErr);
                }
                catch (Exception ex)
                {
                    return "SocketServerWR() Error: " + ex.Message;
                }
            }
            blFlagPlcBusy = false;

            //Depend on intRet, calculate return value
            if (intRet == 0)
            {
                //MessageBox.Show(MsgRcv);

                //Calculate for user string return data
                lstobjTemp.Add("KeyenceEthernet");
                //Try to split receive data into array
                string[] strData;
                MsgRcv = MsgRcv.Trim();
                strData = MsgRcv.Split(' ');
                //MessageBox.Show(strData.Length.ToString());


                //lstobjTemp.Add(MsgRcv);
                for (i = 0; i < strData.Length; i++)
                {
                    lstobjTemp.Add(strData[i]);
                }


                //Add to list of list ouput data
                lstlstobjOutput.Add(lstobjTemp);


                //Decide message out or not
                int intOptionMsgOut = 0;
                if (int.TryParse(lstlstobjInput[0][14].ToString(), out intOptionMsgOut) == false) intOptionMsgOut = 0;
                if (intOptionMsgOut == 1) MessageBox.Show(MsgRcv);

                return intRet.ToString(); //If everything is OK, then return OK code
            }
            else
            {
                return MsgErr; //If NG, then return Message error
            }

        }

        //************************************************************************************************************************
        /// <summary>
        /// Call out setting or control form for manual control Keyence PLC - Using calling from Main User Interface only!!!
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginKeyenceEthernetFuncID1000(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "201-0-1000 Error: Invalid Process ID";

            try
            {
                //Request Password
                string strUserInput = "";
                System.Windows.Forms.DialogResult dlgTemp;

                bool blAllowChange = false;

                while (true)
                {
                    dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Please input password first:", ref strUserInput);
                    if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                    {
                        //Confirm password
                        if (strUserInput.ToUpper().Trim() == "PED")
                        {
                            blAllowChange = true;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Password Wrong! Please input again or cancel!", "PASSWORD WRONG");
                        }
                    }
                    else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                    {
                        MessageBox.Show("Cancel selected. Nothing will be changed!", "PASSWORD WRONG");
                        break;
                    }
                }
                if (blAllowChange == false)
                {
                    return "201-0-100 Func: Select cancel!";
                }

                this.windowPLCSetting = new windowPLCSetting();
                this.windowPLCSetting.intProcessID = intProcessID;
                this.windowPLCSetting.btnApply.Click += btnApply_Click;
                this.windowPLCSetting.Show();
            }
            catch(Exception ex)
            {
                return "201-0-100 Error: " + ex.Message;
            }
            
            //
            return "0";
        }

        void btnApply_Click(object sender, System.Windows.RoutedEventArgs e)
        {


            //Check if this class is initialized or not
            if (this.strPlcServerIpAddress == null)
            {
                this.windowPLCSetting.tbInputID.Text = "Error: Not yet initialized. PLC Server IP Address is null!";
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (this.strPlcServerIpAddress == "")
            {
                this.windowPLCSetting.tbInputID.Text = "Error: Not yet initialized. PLC Server IP Address is empty!";
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if(this.intPlcServerPort == 0)
            {
                this.windowPLCSetting.tbInputID.Text = "Error: Not yet initialized. PLC Server port is 0!";
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            //
            string strInput = this.windowPLCSetting.tbInputID.Text.Trim();
            //Check numeric or not
            int intPLCId = 0;
            if(int.TryParse(strInput, out intPLCId)==false)
            {
                this.windowPLCSetting.tbInputID.Text = "Error: Input ID [" + strInput + "] is not numeric!" ;
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            //If input OK, then we try to write down to PLC
            int intRet = 0;
            string MsgRcv = "";
            string MsgErr = "";

            string strCommandSend = "WR" + " " + this.strKeyMemoryArea + " " + intPLCId.ToString();

            //At 1 time, only allow 1 thread to access this class! prevent racing problem, we lock it!
            lock (this)
            {
                try
                {
                    intRet = SocketServerWR(this.strPlcServerIpAddress, this.intPlcServerPort, strCommandSend, ref MsgRcv, ref MsgErr);
                }
                catch (Exception ex)
                {
                    this.windowPLCSetting.tbInputID.Text = "SocketServerWR() Error: " + ex.Message;
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }
            }

            //Check if sending is OK or not
            if (intRet == 0)
            {
                this.windowPLCSetting.tbInputID.Text = "Command sent OK!";
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Blue;
            }
            else
            {
                this.windowPLCSetting.tbInputID.Text = "SocketServerWR() Error: " + MsgErr;
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            //Reading again from PLC & compare with sent value
            strCommandSend = "RD" + " " + this.strKeyMemoryArea;
            //At 1 time, only allow 1 thread to access this class! prevent racing problem, we lock it!
            lock (this)
            {
                try
                {
                    intRet = SocketServerWR(this.strPlcServerIpAddress, this.intPlcServerPort, strCommandSend, ref MsgRcv, ref MsgErr);
                }
                catch (Exception ex)
                {
                    this.windowPLCSetting.tbInputID.Text = "SocketServerWR() Error: " + ex.Message;
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }
            }

            if(intRet==0) //Sending OK
            {
                //Try to split receive data into array
                string[] strData;
                MsgRcv = MsgRcv.Trim();
                strData = MsgRcv.Split(' ');

                if (strData.Length == 0)
                {
                    this.windowPLCSetting.tbInputID.Text = "SocketServerWR() Error: Data return NG!";
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                int intReceivedID = 0;
                if (int.TryParse(strData[0].Trim(), out intReceivedID)==false)
                {
                    this.windowPLCSetting.tbInputID.Text = "PLC ID has changed fail! Received ID [" +strData[0].Trim() + "] is not numeric!";
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                //Confirm return matching or not
                if (intReceivedID == intPLCId) //matching sent key & received key
                {
                    this.windowPLCSetting.tbInputID.Text = "PLC ID has changed successfully! New ID: " + intPLCId.ToString();
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }
                else
                {
                    //NG case
                    this.windowPLCSetting.tbInputID.Text = "Confirmation fail. Set value [" + intPLCId.ToString() + "] is different from received ID [" + strData[0].Trim() + "]";
                    this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                this.windowPLCSetting.tbInputID.Text = "Command sending to confirm new ID is fail! Return data: [" + intRet.ToString() + "]";
                this.windowPLCSetting.tbInputID.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }
        }

        //************************************************************************************************************************

        /// <summary>
        /// This function, send a socket to host and waiting data return
        /// </summary>
        /// <returns></returns>
        int SocketServerWR(string ServerIpAdrr, int ServerPort, string MsgToSend, ref string MsgRcv, ref string MsgErr)
        {

            try
            {
                //As request format of KV mode, we need attach vbCr at the end of MsgToSend
                MsgToSend = MsgToSend + "\r";

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

                //Delay alittle time?
                //System.Threading.Thread.Sleep(500);

                int intRetryTimes = 0;
                bool blFlagSucess = false;

                while ((intRetryTimes < 5) && (blFlagSucess == false))
                {
                    try
                    {
                        //Send the message to the connected TcpServer
                        stream.Write(data, 0, data.Length);
                        blFlagSucess = true; //Exit retry
                    }
                    catch (Exception ex1)
                    {
                        MsgErr = ex1.Message;
                        //return 9995; //Error code
                    }
                    intRetryTimes++;
                }

                if (blFlagSucess == false)
                {
                    return 9995; //Error code
                }


                //Receive the TcpServer.response.
                //Buffer to store the response bytes.
                data = new byte[1024];
                //String to store the response ASCII representation.
                string responseData = string.Empty;
                //Read the first batch of the TcpServer response bytes.

                int bytes = 0;
                intRetryTimes = 0;
                blFlagSucess = false;
                while ((intRetryTimes<5)&&(blFlagSucess==false))
                {
                    try
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        blFlagSucess = true; //Exit retry
                    }
                    catch (Exception ex2)
                    {
                        MsgErr = ex2.Message;
                        //return 9996; //Error code
                    }
                    intRetryTimes++;
                }

                if (blFlagSucess==false)
                {
                    return 9996; //Error code
                }

                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                MsgRcv = responseData;
                //Close everything
                stream.Close();
                client.Close();
                MsgErr = "";

                //If everything is OK. Then return OK code
                return 0;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "SocketServerWR()");
                MsgErr = ex.Message;
                return 9000; //Error code
            }

        }


        /// <summary>
        /// Supporting Function
        /// </summary>
        /// <param name="strPC_IPAdrress"></param>
        /// <returns></returns>
        public bool CheckNetWorkPCconnectStatus(string strPC_IPAdrress)
        {
            //Test Pinging for PC
            int intRetry;
            int i;

            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "CheckNetWorkPCconnectStatus()");
                MessageBox.Show("Please check your network connection!", "CheckNetWorkPCconnectStatus()");
                return false;
            }

        }
        //************************************************************************************************************************

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

