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
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;

namespace nspMainUSB
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginMainUSB,100")]
    [Export(typeof(nspMainUsbInterface.IMainUsb))]
    [ExportMetadata("IPluginMainUsbInfo", "nspMainUSB")]
    public class clsMainUSB : nspINTERFACE.IPluginExecute, nspMainUsbInterface.IMainUsb
    {
        public clsCanonUsbKernel.clsUsbKernel clsMainUsb { get; set; }
        public string strPIPE_NAME { get; set; }
        public int Month;
        public int Date;

        public struct MainUSBVar
        {
            public string strMainPipeName;
            public string strDummyMainPipeName;
            //public  int Month;
        }
        public MainUSBVar strcMainUSBVar;


        //Implement INTERFACES
        #region _Implement_nspMainUsbInterface

        public void SetPipeName(string strPipe)
        {
            this.strPIPE_NAME = strPipe;
        }

        public int UnLockMainPCB(string strModeinExePath, int msTimeOut = 10000, bool blAutoFindPipe = false, string strProductID = "") //Unlock Main PCB
        {
            return this.clsMainUsb.UnLockMainPCB(strModeinExePath, msTimeOut, blAutoFindPipe, strProductID);
        }

        public int USBWrite(string strData, int WaitTM = 5000) //Write command to Main PCB but do not receive return data
        {
            return this.clsMainUsb.USBWrite(strData, WaitTM);
        }

        public int USB_DR(string MainCmd, string SubCmd, out byte[] byteMainUsbReceiveData, long Rnum = 500, uint RCVTM = 500) //Receive data from Main PCB
        {
            return this.clsMainUsb.USB_DR(MainCmd, SubCmd, out byteMainUsbReceiveData, Rnum, RCVTM);
        }

        public int USB_WR(string strData, out byte[] byteMainUsbReceiveData, long Rnum = 500, int SNDTM = 5000, uint RCVTM = 5000)
        {
            return this.clsMainUsb.USB_WR(strData, out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM);
        }

        public int USB_WR2(string strData, long Rnum = 500, int SNDTM = 5000, uint RCVTM = 5000)
        {
            return this.clsMainUsb.USB_WR2(strData, Rnum, SNDTM, RCVTM);
        }

        #endregion

        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "100,0,0,1,2,10,12,13,14,20,21,50,51,52,53,55,56,100,101"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 26/Aug/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add Function 2 to detect device!"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlststrOutput)
        {
            lstlststrOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Process ID - ... - JigID-HardID-FuncID"
            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());
            switch (intJigID) //Select JigID
            {
                case 100:
                    return SelectHardIDFromJigID100(lstlstobjInput, out lstlststrOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID100(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return PluginMainUSBFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginMainUSBFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginMainUSBFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 10:
                    return PluginMainUSBFuncID10(lstlstobjInput, out lstlstobjOutput);
                case 12:
                    return PluginMainUSBFuncID12(lstlstobjInput, out lstlstobjOutput);
                case 13:
                    return PluginMainUSBFuncID13(lstlstobjInput, out lstlstobjOutput);
                case 14:
                    return PluginMainUSBFuncID14(lstlstobjInput, out lstlstobjOutput);
                case 20:
                    return PluginMainUSBFuncID20(lstlstobjInput, out lstlstobjOutput);
                case 21:
                    return PluginMainUSBFuncID21(lstlstobjInput, out lstlstobjOutput);
                case 50:
                    return PluginMainUSBFuncID50(lstlstobjInput, out lstlstobjOutput);
                case 51:
                    return PluginMainUSBFuncID51(lstlstobjInput, out lstlstobjOutput);
                case 52:
                    return PluginMainUSBFuncID52(lstlstobjInput, out lstlstobjOutput);
                case 53:
                    return PluginMainUSBFuncID53(lstlstobjInput, out lstlstobjOutput);
                case 55:
                    return PluginMainUSBFuncID55(lstlstobjInput, out lstlstobjOutput);
                case 56:
                    return PluginMainUSBFuncID56(lstlstobjInput, out lstlstobjOutput);
                case 100:
                    return PluginMainUSBFuncID100(lstlstobjInput, out lstlstobjOutput);
                case 101:
                    return PluginMainUSBFuncID101(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }


        /// <summary>
        /// Get usb pipe name for class clsMainUsb from ini file
        ///     +Para1 (13): File to get pipe
        ///     +Para2 (14): Section to get pipe
        ///     +Para3 (15): Keyname to get pipe (without index - automatically add index)
        ///     +Para4 (16): 0 - assign for Main PCB pipename. 1: assign for Dummy Main PCB pipe name (Default is Main PCB pipe name)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            int intPcbID = 0; //0: Main PCB. 1: Dummy Main PCB
            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intPcbID) == false)
            {
                intPcbID = 0; //Default value
            }

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                //MessageBox.Show("'" + lstPara[0] + "' file does not exist! Please check!", "CommonFunctionID002() error");
                return "Error"; //File not exist code
            }

            string strTmp = "";
            string strKeyName = "";

            //Calculate string file name
            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "error";
            strKeyName = lstlstobjInput[0][15].ToString() + (intProcessID + 1).ToString();
            //Reading value in key name
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                //MessageBox.Show("Error: cannot find '" + lstPara[2] + "' config in '" + lstPara[1] + "' of " + strFileName + " file!", "CommonFunctionID002() error");
                return "error"; //Getting key value fail
            }

            //Depend on PCB ID - assign pipename for Main PCB or Dummy Main PCB
            switch (intPcbID)
            {
                case 0: //Main PCB
                    this.strcMainUSBVar.strMainPipeName = strTmp;
                    break;
                case 1:
                    this.strcMainUSBVar.strDummyMainPipeName = strTmp;
                    break;
                default:
                    this.strcMainUSBVar.strMainPipeName = strTmp;
                    break;
            }

            //Adding UserRet Info
            lstobjTemp = new List<object>();
            lstobjTemp.Add("PipeName");
            lstobjTemp.Add(strTmp);
            lstlstobjOutput.Add(lstobjTemp);

            //Assign pipe name from ini file - always ini with MAIN PCB pipe name!
            this.clsMainUsb.strPIPE_NAME = this.strcMainUSBVar.strMainPipeName;

            //Out put class through user Ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Main");
            lstobjTemp.Add(this);
            lstlstobjOutput.Add(lstobjTemp);

            return "0"; //if everything ok, return OK code "0"
        }

        /// <summary>
        /// Set Pipename for Main USB class
        ///     +Para1 (13): Parameter want to set
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            this.clsMainUsb.strPIPE_NAME = lstlstobjInput[0][13].ToString().Trim();

            //Return value
            return "0";
        }

        /// <summary>
        /// Detect if a device is attached to a certain pipe (indentified by pipe name)
        ///     +Para1 (13): Pipename want to check
        ///     +Para2 (14): Time out setting
        ///     +Para3 (15): Time between 2 consecutive polling
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            int intTimeOutms = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intTimeOutms) == false)
            {
                intTimeOutms = 1000; //Default value
            }

            int intSliceTimeMs = 0;
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intSliceTimeMs) == false)
            {
                intSliceTimeMs = 100; //Default
            }

            bool blRet = false;
            try
            {
                //Checking
                blRet = this.clsMainUsb.DetectDevice(lstlstobjInput[0][13].ToString(), intTimeOutms, intSliceTimeMs);
            }
            catch(Exception ex)
            {
                return "110-0-2 Error: " + ex.Message;
            }

            //Return value
            if (blRet == true)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// Unlock Main PCB
        ///     +Para1 (13): Time out setting
        ///     +Para2 (14): Optional to Auto Find pipename (1: Auto Find. Other: use setting pipe name)
        ///     +Para3 (15): Product ID want to Auto Find
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            int iret = 0;
            int intTimeOut = 0; //ms

            if (lstlstobjInput[0].Count < 13) intTimeOut = 10000; //Default setting
            if (lstlstobjInput[0][13].ToString().Trim() == "") intTimeOut = 10000; //Default setting

            //Analyze retry times & time out
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = int.Parse(lstlstobjInput[0][13].ToString());
            }
            else //illegal parameter input - set to default is 10000
            {
                intTimeOut = 10000;
            }

            //
            bool AutoFindPipe = false;
            if (lstlstobjInput[0][14].ToString().Trim() == "1") AutoFindPipe = true;
            string strPID = lstlstobjInput[0][15].ToString().Trim().ToUpper();

            iret = this.UnLockMainPCB(lstlstobjInput[0][0].ToString(), intTimeOut, AutoFindPipe, strPID);

            //If NG. Add Error Message
            if (iret != 0) //Error happen
            {
                lststrTemp = new List<object>();
                lststrTemp.Add("ErrMsg");
                string strTemp = "";
                switch (iret)
                {
                    case 256:
                        strTemp = "Cannot detect Canon Main PCB USB device with setting pipe";
                        break;
                    default:
                        strTemp = "Unkown error happen";
                        break;
                }
                lststrTemp.Add(strTemp);
                lstlstobjOutput.Add(lststrTemp);
            }

            //Record Pipe
            lststrTemp = new List<object>();
            lststrTemp.Add("pipe");
            lststrTemp.Add(this.clsMainUsb.strPIPE_NAME);
            lstlstobjOutput.Add(lststrTemp);

            //Return value
            return iret.ToString();
        }
        
        /// <summary>
        /// USBWrite()
        ///     +Para1: USB command
        ///     +Para2: Time out setting
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID12(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            //return this.USBWrite(strData, WaitTM);
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            if (lstlstobjInput[0].Count < 14) return "error";

            int iret = 0;
            int WaitTM = 0;

            //Analyze retry times & time out
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out WaitTM) == true) //legal parameter
            {
                WaitTM = int.Parse(lstlstobjInput[0][14].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                WaitTM = 5000;
            }

            //Sending command
            iret = this.USBWrite(lstlstobjInput[0][13].ToString(), WaitTM);
            //Adding Error Message
            lstobjTemp = new List<object>();
            string strErrMsg = "";

            switch (iret)
            {
                case 0: //OK
                    strErrMsg = "";
                    break;
                case 255:
                    strErrMsg = "USB command length NG";
                    break;
                case 1:
                    strErrMsg = "WriteHandle invalid";
                    break;
                default:
                    strErrMsg = "Unkown error happen";
                    break;
            }

            if (iret != 0) //Error happen
            {
                lstobjTemp.Add("ErrMsg");
                lstobjTemp.Add(strErrMsg);
            }

            //Return value
            return iret.ToString();
        }

        /// <summary>
        /// USB_DR()
        ///     +Para1: Main Cmd
        ///     +Para2: Sub cmd
        ///     +Para3: Rnum
        ///     +Para4: RCVTM
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID13(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            //return this.USB_DR(MainCmd, SubCmd, out byteMainUsbReceiveData, Rnum, RCVTM);
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            if (lstlstobjInput[0].Count < 16) return "error";

            int iret = 0;
            long Rnum = 0;
            uint RCVTM = 0;
            byte[] byteMainUsbReceiveData = new byte[1];


            //Analyze retry times & time out
            if (long.TryParse(lstlstobjInput[0][15].ToString(), out Rnum) == true) //legal parameter
            {
                Rnum = long.Parse(lstlstobjInput[0][15].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                Rnum = 5000;
            }

            if (uint.TryParse(lstlstobjInput[0][16].ToString(), out RCVTM) == true) //legal parameter
            {
                RCVTM = uint.Parse(lstlstobjInput[0][16].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                RCVTM = 5000;
            }

            //Sending command
            iret = this.USB_DR(lstlstobjInput[0][13].ToString(), lstlstobjInput[0][14].ToString(), out byteMainUsbReceiveData, Rnum, RCVTM);


            //NOTE: you have to prevent memory leak!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            lstobjTemp.Add("MainUSB");
            int i = 0;
            string strTemp = "";

            int intLenSave = 0;
            if (byteMainUsbReceiveData.Length > 2)
            {
                intLenSave = byteMainUsbReceiveData[0] * 256 + byteMainUsbReceiveData[1];
            }
            else
            {
                if (byteMainUsbReceiveData.Length <= 512)
                {
                    intLenSave = byteMainUsbReceiveData.Length;
                }
                else
                {
                    intLenSave = 512;
                }
            }

            //Only allow maximum 512
            if (intLenSave > 512)
            {
                intLenSave = 512;
            }

            for (i = 0; i < intLenSave + 2; i++)
            {
                if (i < byteMainUsbReceiveData.Length - 1)
                {
                    strTemp = byteMainUsbReceiveData[i].ToString();
                    lstobjTemp.Add(strTemp);
                }
            }
            lstlstobjOutput.Add(lstobjTemp);

            //Adding Error Message
            lstobjTemp = new List<object>();
            string strErrMsg = "";

            switch (iret)
            {
                case 0: //OK
                    strErrMsg = "";
                    break;
                case 255:
                    strErrMsg = "USB command length NG";
                    break;
                case 1:
                    strErrMsg = "WriteHandle invalid";
                    break;
                default:
                    strErrMsg = "Unkown error happen";
                    break;
            }

            if (iret != 0) //Error happen
            {
                lstobjTemp.Add("ErrMsg");
                lstobjTemp.Add(strErrMsg);
            }

            //Return value
            return iret.ToString();
        }

        /// <summary>
        /// USB_WR() - Take command from "Transmission" area
        ///     + Transmission: USB command to send
        ///     +Para1 (13): Rnum setting (Retry times)
        ///     +Para2 (14): SNDTM setting (sending time out)
        ///     +Para3 (15): RCVTM setting (Receiving time out)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID14(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            if (lstlstobjInput[0].Count < 14) return "error";

            int iret = 0;
            long Rnum = 0;
            int SNDTM = 0;
            uint RCVTM = 0;
            //byte[] byteMainUsbReceiveData = new byte[1];

            //Analyze retry times & time out
            if (long.TryParse(lstlstobjInput[0][13].ToString(), out Rnum) == true) //legal parameter
            {
                Rnum = long.Parse(lstlstobjInput[0][13].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                Rnum = 1000;
            }

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out SNDTM) == true) //legal parameter
            {
                SNDTM = int.Parse(lstlstobjInput[0][14].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                SNDTM = 5000;
            }

            if (uint.TryParse(lstlstobjInput[0][15].ToString(), out RCVTM) == true) //legal parameter
            {
                RCVTM = uint.Parse(lstlstobjInput[0][15].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                RCVTM = 5000;
            }
            //Sending command
            //iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"

            iret = this.USB_WR2(lstlstobjInput[0][11].ToString(), Rnum, SNDTM, RCVTM); //11: "Transmission"
            //Calculate for user string return data
            

            //NOTE: you have to prevent memory leak!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            lstobjTemp.Add("MainUSB");
            int i = 0;
            string strTemp = "";

            int intLenSave = 0;
            if(this.clsMainUsb.ReceiveData.Length>2)
            {
                intLenSave = this.clsMainUsb.ReceiveData[0] * 256 + this.clsMainUsb.ReceiveData[1];
            }
            else
            {
                if (this.clsMainUsb.ReceiveData.Length <= 512)
                {
                    intLenSave = this.clsMainUsb.ReceiveData.Length;
                }
                else
                {
                    intLenSave = 512;
                }
            }

            //Only allow maximum 512
            if (intLenSave > 512)
            {
                intLenSave = 512;
            }

            for (i = 0; i < intLenSave + 2; i++)
            {
                if (i < this.clsMainUsb.ReceiveData.Length - 1)
                {
                    strTemp = this.clsMainUsb.ReceiveData[i].ToString();
                    lstobjTemp.Add(strTemp);
                }
            }
            lstlstobjOutput.Add(lstobjTemp);


            //Adding Error Message
            lstobjTemp = new List<object>();
            string strErrMsg = "";

            switch (iret)
            {
                case 0: //OK
                    strErrMsg = "";
                    break;
                case 256:
                    strErrMsg = "USBWrite() error";
                    break;
                case 257:
                    strErrMsg = "Receive() error";
                    break;
                case 258:
                    strErrMsg = "Time out error";
                    break;
                default:
                    strErrMsg = "Unkown error happen";
                    break;
            }

            if (iret != 0) //Error happen
            {
                lstobjTemp.Add("ErrMsg");
                lstobjTemp.Add(strErrMsg);
            }

            ////Out put class through user Ret
            //lstobjTemp = new List<object>();
            //lstobjTemp.Add("Main");
            //lstobjTemp.Add(this);
            //lstlstobjOutput.Add(lstobjTemp);

            //Return value
            return iret.ToString();
        }


        /// <summary>
        /// Reading ASIC ADC Value
        ///     + Transmission: USB command to send
        ///     + Para1 (13): High Byte position
        ///     + Para2 (14): Low Byte position
        ///     + Para3 (15): Time Out setting
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID20(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            if (lstlstobjInput[0].Count < 14) return "error";

            //Check numeric condition
            double dblLoLimit = 0; //5th
            double dblHiLimit = 0; //6th

            int intHighBytePos = 0;
            int intLowBytePos = 0;
            int intTimeOut = 0;

            if (double.TryParse(lstlstobjInput[0][5].ToString(), out dblLoLimit) == false) return "Error: Low limit is not numeric!";
            if (double.TryParse(lstlstobjInput[0][6].ToString(), out dblHiLimit) == false) return "Error: Hi limit is not numeric!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intHighBytePos) == false) return "Error: High Byte Position setting is NG";
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intLowBytePos) == false) return "Error: Low Byte Position setting is NG";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intTimeOut) == false) return "Error: Time out setting is NG";

            int iret = 0;
            long Rnum = 1000;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            byte[] byteMainUsbReceiveData = new byte[1];
            double dblResult = 0;

            //Mark time to start
            int intStartTime = MyLibrary.ApiDeclaration.GetTickCount();
            int intCurrentTime = 0;
            bool blFlagTimeOut = false;

            do
            {
                intCurrentTime = MyLibrary.ApiDeclaration.GetTickCount();
                if ((intCurrentTime - intStartTime) > intTimeOut) blFlagTimeOut = true;
                //Sending command
                iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
                //check return value
                if (iret == 0)
                {
                    //If sending is OK, then calculate AD value
                    dblResult = Convert.ToDouble(byteMainUsbReceiveData[intHighBytePos]) * 256 + Convert.ToDouble(byteMainUsbReceiveData[intLowBytePos]);
                }
                else
                {
                    //Do nothing
                }
            }
            while (((dblResult < dblLoLimit) || (dblResult > dblHiLimit)) && (blFlagTimeOut == false));

            if (blFlagTimeOut == true) return "Error: Time out! Measured value (Hexa format) = " + Convert.ToInt32(dblResult.ToString()).ToString("X");
            //Return value

            return dblResult.ToString();

        }



        /// <summary>
        /// Polling return byte from Factory Command
        ///     + Transmission: USB command to send
        ///     + Para1 (13): Timeout setting
        ///     + Para4 (14): Byte position want to wait (compare with lo & Hi limit)
        /// </summary>
        /// <param name="lstlststrInput"></param>
        /// <param name="lstlststrOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID21(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            if (lstlstobjInput[0].Count < 14) return "error";

            //Check numeric condition
            double dblLoLimit = 0; //5th
            double dblHiLimit = 0; //6th

            int intTimeOut = 0;
            int intBytePos = 0;


            if (double.TryParse(lstlstobjInput[0][5].ToString(), out dblLoLimit) == false) return "Error: Low limit is not numeric!";
            if (double.TryParse(lstlstobjInput[0][6].ToString(), out dblHiLimit) == false) return "Error: Hi limit is not numeric!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intTimeOut) == false) return "Error: Timeout setting is NG";
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intBytePos) == false) return "Error: Byte Position setting is NG";

            int iret = 0;
            long Rnum = 1000;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            byte[] byteMainUsbReceiveData = new byte[1];
            // double dblResult = 0;
            double dblResult = dblLoLimit - 1; //Prevent default 0 is PASS if Spec is 0!

            //Mark time to start
            int intStartTime = MyLibrary.ApiDeclaration.GetTickCount();
            int intCurrentTime = 0;
            bool blFlagTimeOut = false;

            do
            {
                if(intTimeOut != -1) // -1: waiting forever!
                {
                    intCurrentTime = MyLibrary.ApiDeclaration.GetTickCount();
                    if ((intCurrentTime - intStartTime) > intTimeOut) blFlagTimeOut = true;
                }

                //Sending command
                iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
                //check return value
                if (iret == 0)
                {
                    //If sending is OK, then calculate value
                    dblResult = byteMainUsbReceiveData[intBytePos];
                }
                else
                {
                    //Do nothing
                }
            }
            while (((dblResult < dblLoLimit) || (dblResult > dblHiLimit)) && (blFlagTimeOut == false));

            if (blFlagTimeOut == true) return "Error: Time out! Return value = " + dblResult.ToString();
            //Return value
            return dblResult.ToString();
        }


        /// <summary>
        /// USB serial number Info Reading
        ///     + Transmission: USB command send to check USB number
        ///     + Para1 (13): Reading Mode (0: old model - take 3 bytes in hexa format. New model: take 6 bytes in Hexa format).
        ///     + Para2 (14): Start position of Received data to get Info
        ///     + Para3 (15): Last position of Received data to get Info
        /// Return: a numer
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID50(List<List<object>> lstlstobjInput, out List<List<object>> lstobjOutput)
        {
            lstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Check numeric or not
            int intReadingMode = 0;
            int intStartPos = 0;
            int intLastPos = 0;

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intReadingMode) == false) return "Error: Reading mode setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intStartPos) == false) return "Error: Start position setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intLastPos) == false) return "Error: Last position setting is not numeric!";
            //Sending command
            int iret = 0;
            long Rnum = 500;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            byte[] byteMainUsbReceiveData = new byte[1];

            iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
            if (iret != 0) return "Error: USB command sending fail";

            //If OK, then calculate Reading Info
            string strResult = "";
            int i = 0;

            if (intReadingMode == 0) //Old model
            {
                for (i = intStartPos; i < intLastPos + 1; i++)
                {
                    if (byteMainUsbReceiveData[i] <= 0x0F)
                    {
                        strResult = strResult + "0" + byteMainUsbReceiveData[i].ToString("X");
                    }
                    else
                    {
                        strResult = strResult + byteMainUsbReceiveData[i].ToString("X");
                    }

                    //strResult = strResult + byteMainUsbReceiveData[intStartPos].ToString("X");
                }
            }
            else if (intReadingMode == 1) //New Model
            {
                for (i = intStartPos; i < intLastPos + 1; i++)
                {
                    //strResult = strResult + byteMainUsbReceiveData[i].ToString("X");
                    char chrTemp = Convert.ToChar(byteMainUsbReceiveData[i]); //ASCII?
                    strResult = strResult + chrTemp.ToString();
                }
            }


            //Convert to Number - 00CD4B
            int intResult = 0;
            if (Int32.TryParse(strResult, System.Globalization.NumberStyles.HexNumber, null, out intResult) == true)
            {
                intResult = Convert.ToInt32(strResult, 16);
                return intResult.ToString();
            }
            else
            {
                return "Error: cannot convert result [" + strResult + "] to integer!";
            }
        }

        /// <summary>
        /// Check Main PCB Info
        ///     - Factory command get from "Transmission" area
        ///     - Parameter setting:
        ///         + Para1 (13): File to get setting - must be same location with exe file
        ///         + Para2 (14): Section to get setting
        ///         + Para3 (15): Key name to get setting
        ///         + Para4 (16): Start position of USB received data contain Info
        ///         + Para5 (17): Last position of USB received data contain Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID51(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                return "Error: Setting file is not exist";
            }

            string strConfigInfo = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyName = lstlstobjInput[0][15].ToString();

            //Reading strModelCode
            strConfigInfo = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strConfigInfo.ToLower() == "error")
            {
                return "Error: cannot find '" + strKeyName + "' setting in " + strSectionName; //Getting key value fail
            }

            //Check numeric or not
            int intStartPos = 0;
            int intLastPos = 0;

            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intStartPos) == false) return "Error: Start position setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intLastPos) == false) return "Error: Last position setting is not numeric!";

            //Sending command
            int iret = 0;
            long Rnum = 500;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            byte[] byteMainUsbReceiveData = new byte[1];

            iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
            if (iret != 0) return "Error: USB command sending fail";
            //If OK, then calculate Reading Info
            string strResult = "";
            int i = 0;
            for (i = intStartPos; i < intLastPos + 1; i++)
            {
                //strResult = strResult + byteMainUsbReceiveData[i].ToString("X");
                strResult = strResult + Convert.ToChar(byteMainUsbReceiveData[i]);
            }
            //Compare with config Info
            if (strResult != strConfigInfo) return "Error: Reading result [" + strResult + "] is different from Config setting [" + strConfigInfo + "]";
            //If everything OK, then return OK code
            return "0";
        }


        /// <summary>
        /// Check Main PCB Info - Motor Driver Check
        ///     - Factory command get from "Transmission" area
        ///     - Parameter setting:
        ///         + Para1 (13): File to get setting - must be same location with exe file
        ///         + Para2 (14): Section to get setting
        ///         + Para3 (15): Key name to get setting
        ///         + Para4 (16): Start position of USB received data contain Info
        ///         + Para5 (17): Last position of USB received data contain Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID52(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: Setting file is not exist";
            }

            string strConfigInfo = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyName = lstlstobjInput[0][15].ToString();

            //Reading Motor Driver version
            strConfigInfo = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strConfigInfo.ToLower() == "error")
            {
                return "Error: cannot find '" + strKeyName + "' setting in " + strSectionName; //Getting key value fail
            }

            //Check numeric or not
            int intStartPos = 0;
            int intLastPos = 0;

            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intStartPos) == false) return "Error: Start position setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intLastPos) == false) return "Error: Last position setting is not numeric!";

            //Sending command
            int iret = 0;
            long Rnum = 500;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            byte[] byteMainUsbReceiveData = new byte[1];

            iret = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
            if (iret != 0) return "Error: USB command sending fail";
            //If OK, then calculate Reading Info
            string strResult = "";
            int i = 0;
            for (i = intStartPos; i < intLastPos + 1; i++)
            {
                if (byteMainUsbReceiveData[i] > 0x0F)
                {
                    strResult = strResult + byteMainUsbReceiveData[i].ToString("X");
                }
                else
                {
                    strResult = strResult + "0" + byteMainUsbReceiveData[i].ToString("X");
                }

            }
            //Compare with config Info
            if (strResult != strConfigInfo) return "Error: Reading result [" + strResult + "] is different from Config setting [" + strConfigInfo + "]";
            //If everything OK, then return OK code
            return "0";
        }




        /// <summary> TRUNG B add more
        /// writing process flag into factory area
        /// parameter
        ///     para1(11) : command to send
        ///     para2(13) : geting number fixture  using Ret(x) to get numberfixture
        ///     para3(14) : option date or Month
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID53(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            DateTime Rightnow;
            //int Month;
            int iRet = 0;
            int optionDate_month = 0;
            Rightnow = System.DateTime.Now;
            //get number fixture and Month 

            int intTemp = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(),out intTemp)==false)
            {
                return "Error: input data [" + lstlstobjInput[0][13].ToString()+ "] cannot convert to interger!";
            }

            Month = Convert.ToInt32((Convert.ToInt32(lstlstobjInput[0][13].ToString()) * 16 + Rightnow.Month)); 
            //string months = Convert.ToString(Month);


            string months = Month.ToString("X");

            //get date from 1 to 31
            Date = 128 + ((Rightnow.Day / 10) * 16) + (Rightnow.Day % 10);
            string date = Date.ToString("X");
            //sending command
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out optionDate_month) == false)
                return "Error: Reading option date and month is not numeric!";
            else
            {
                if (Convert.ToInt32(lstlstobjInput[0][14].ToString()) == 0)
                {
                    iRet = this.USBWrite(lstlstobjInput[0][11].ToString() + months + ",");
                }
                else
                    iRet = this.USBWrite(lstlstobjInput[0][11].ToString() + date + ",");
            }
            // return "0" if everything is OK
            return "0";
        }


        /// <summary> TRUNG B add more
        /// compare data with FCT Flag
        ///         para1 (11) : USB command send to Main PCB
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID55(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            int iRet = 0;
            byte[] byteMainUsbReceiveData = new byte[1];

            iRet = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData);

            if (Month == Convert.ToInt32(byteMainUsbReceiveData[6].ToString()) || Date == Convert.ToInt32(byteMainUsbReceiveData[6].ToString()))
            {
                return "0";
            }
            else
                return "1";
        }


        /// <summary>
        /// Median Filter for Main USB command
        /// para11: Command to send (transmission area)
        /// para13: Byte position want median filter (4,5,6,7,8...)
        /// para14: Median Filter times
        /// para15: Round digit
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID56(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            var lststraddTemp = new List<object>();
            int iRet = 0;
            byte[] byteMainUsbReceiveData = new byte[1];
            int i, k = 0;
            string[] byteposMedianFilter;//13
            string strposMedianFilter;
            long Rnum = 50000;
            int SNDTM = 5000;
            uint RCVTM = 5000;
            int intMedianFilterTimes = 0;//14
            int intRoundDigit;//15
            int[] intbyteposMedianFilter = new int[100];
            double dblLoLimit = 0; //5th
            double dblHiLimit = 0; //6th
            List<double> lstdblMedianVal = new List<double>();


            lststrTemp.Add("MainMedian");
            if (double.TryParse(lstlstobjInput[0][5].ToString(), out dblLoLimit) == false) return "Error: Low limit is not numeric!";
            if (double.TryParse(lstlstobjInput[0][6].ToString(), out dblHiLimit) == false) return "Error: Hi limit is not numeric!";
            strposMedianFilter = lstlstobjInput[0][13].ToString();
            byteposMedianFilter = strposMedianFilter.Split(',');
            for (k = 0; k < byteposMedianFilter.Length; k++)
            {
                if (int.TryParse(byteposMedianFilter[k].ToString(), out intbyteposMedianFilter[k]) == false) return "Error: Byte Position want Median Filter is not numeric";
            }

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intMedianFilterTimes) == true) //legal parameter
            {
                intMedianFilterTimes = int.Parse(lstlstobjInput[0][14].ToString());
            }
            else //illegal parameter input - set to default is 0
            {
                intMedianFilterTimes = 0;
            }

            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intRoundDigit) == true) //legal parameter
            {
                intRoundDigit = int.Parse(lstlstobjInput[0][15].ToString());
            }
            else //illegal parameter input - set to default is 0
            {
                intRoundDigit = 0;
            }

            double[,] dblTemp = new double[100, 100];
            for (i = 0; i < intMedianFilterTimes; i++)
            {
                //Sending command
                iRet = this.USB_WR(lstlstobjInput[0][11].ToString(), out byteMainUsbReceiveData, Rnum, SNDTM, RCVTM); //11: "Transmission"
                //If sending is OK. Then add result to dblTemp
                if (iRet == 0)
                {
                    for (k = 0; k < byteposMedianFilter.Length; k = k + 2)
                    {
                        dblTemp[i, k] = (Convert.ToDouble(byteMainUsbReceiveData[intbyteposMedianFilter[k]])) * 256 + (Convert.ToDouble(byteMainUsbReceiveData[intbyteposMedianFilter[k + 1]]));
                        dblTemp[i, k] = Math.Round(dblTemp[i, k], intRoundDigit);
                    }
                }
                else
                {
                    return "Error: Fail " + iRet.ToString();
                }
            }
            //Add more lstTemp for list have 512 element
            List<string> lstTemp = new List<string>();
            for (int a = 0; a < byteMainUsbReceiveData.Length; a++)
            {
                lstTemp.Add(Convert.ToString(byteMainUsbReceiveData[a]));
            }
            //not yet
            //Do Median Filter calculation
            for (k = 0; k < byteposMedianFilter.Length; k = k + 2)
            {
                for (i = 0; i < intMedianFilterTimes; i++)
                {
                    lstdblMedianVal.Add(dblTemp[i, k]);
                }
                if (lstdblMedianVal.Count == 0) return "Error: Cannot get any data for Median Filter"; //Cannot get any data

                lstTemp[intbyteposMedianFilter[k]] = (Convert.ToInt16(MedianFilterCal(lstdblMedianVal)) / 256).ToString();
                lstTemp[intbyteposMedianFilter[k + 1]] = (Convert.ToInt16(MedianFilterCal(lstdblMedianVal)) % 256).ToString();

                for (i = 0; i < intMedianFilterTimes; i++)
                {
                    lstdblMedianVal.Remove(dblTemp[i, k]);
                }
            }

            for (i = 0; i < 512; i++) lststrTemp.Add(lstTemp[i]);
            lstlstobjOutput.Add(lststrTemp);

            return "0";
        }


        public string MedianFilterCal(List<double> lstdblInput)
        {
            if (lstdblInput.Count == 0) return "Error: There is no value!";

            double dblResult = 0;
            List<double> lstTemp = new List<double>(lstdblInput);

            //sorting list from smallest to biggest value
            lstTemp.Sort();

            if ((lstTemp.Count % 2) == 0)  // count is even, average two middle elements
            {
                // count is even, average two middle elements
                dblResult = (lstTemp[(lstTemp.Count / 2) - 1] + lstTemp[lstTemp.Count / 2]) / 2;
            }
            else // count is odd, return the middle element
            {
                dblResult = lstTemp[(int)(lstTemp.Count / 2)];
            }

            return dblResult.ToString();
        }


        /// <summary>
        /// Logging Motor Data function
        ///     + Para1 (13): Motor ID (1:LF, 2:CR, 3:APP) 
        ///     + Para2 (14): Area setting (1: All, 2: Area)
        ///     + Para3 (15): Product Serial
        ///     + Para4 (16): Step Number
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID100(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //
            int i, j = 0;
            DateTime now = DateTime.Now;

            string strMotorID = lstlstobjInput[0][13].ToString().Trim();
            string strAreaID = lstlstobjInput[0][14].ToString().Trim();

            string strMotorName = "";
            string strAreaName = "";
            string strProductSerial = lstlstobjInput[0][15].ToString().Trim();
            if(strProductSerial=="")
            {
                strProductSerial = "NULL";
            }
            string strStepNum = lstlstobjInput[0][16].ToString().Trim();
            if(strStepNum=="")
            {
                strStepNum = "NONE";
            }


            string strTIME = "TIME" + now.ToString("yyMMddhhmmss");

            //
            switch (strMotorID)
            {
                case "1":
                    strMotorName = "LF";
                    break;
                case "2":
                    strMotorName = "CR";
                    break;
                case "3":
                    strMotorName = "APP";
                    break;
                default:
                    strMotorName = strMotorID;
                    break;
            }

            //
            switch (strAreaID)
            {
                case "1":
                    strAreaName = "All";
                    break;
                case "2":
                    strAreaName = "Area";
                    break;
                default:
                    strAreaName = strAreaID;
                    break;
            }

            //1. Cal Data folder name
            string strFolderName = "motor_data";
            string strFolderPath = AppDomain.CurrentDomain.BaseDirectory + strFolderName;

            if (MyLibrary.ChkExist.CheckFolderExist(strFolderPath) == false)
            {
                Directory.CreateDirectory(strFolderPath);
            }

            //2. Each day create 1 csv file to store checking data if not exist
            //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
            string strDataFileName = "";
            //strDataFileName = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString() + ".csv";
            //LF_ALL_114_NULL_TIME161111144606.txt
            strDataFileName = strMotorName + "_" + strAreaName + "_" + strStepNum + "_" + strProductSerial + "_" + strTIME + ".txt";
            string strDataFileFullName = strFolderPath + @"\" + strDataFileName;

            if (MyLibrary.ChkExist.CheckFileExist(strDataFileFullName) == false)
            {
                //Create new file
                File.Create(strDataFileFullName).Close();
            }

            //3. Start to write data to csv file
            Int64 intDataLen = this.clsMainUsb.ReceiveData[5];
            intDataLen = intDataLen * 0x10000 + this.clsMainUsb.ReceiveData[0] * 0x100 + this.clsMainUsb.ReceiveData[1];

            //Convert to Hexa format and add to list
            List<string> lststrTempBuff = new List<string>();

            for (i = 6; i <= intDataLen;i++ )
            {
                string strTemp = "";
                if(i<this.clsMainUsb.ReceiveData.Length)
                {
                    strTemp = this.clsMainUsb.ReceiveData[i].ToString("X").Trim();
                }
                else
                {
                    strTemp = "00";
                }
                //
                if(strTemp.Length<2)
                {
                    strTemp = "0" + strTemp;
                }

                lststrTempBuff.Add(strTemp);
            }

            //
            StreamWriter sWriter;
            //Open file name to write
            sWriter = File.AppendText(strDataFileFullName);

            string strDataToWrite = "";
            string strTotalData = "";
            bool blLastData = false;
            
            switch(strMotorName)
            {
                case "CR":
                    //Combine 4 bytes to 1 data, when reaching 6 data => write to log file

                    //start write
                    for (i = 0; i < lststrTempBuff.Count; i = i + 4)
                    {
                        if ((i > 0) && ((i % 24) == 0)) //Need to write 
                        {
                            sWriter.WriteLine(strDataToWrite); strTotalData += strDataToWrite + "\r\n";
                            strDataToWrite = ""; //Reset for next
                        }

                        string strNewData = "";
                        blLastData = false;
                        if (i + 3 < lststrTempBuff.Count) //Not last data
                        {
                            strNewData = lststrTempBuff[i+3] + lststrTempBuff[i+2] + lststrTempBuff[i+1] + lststrTempBuff[i];
                        }
                        else //Last data
                        {
                            blLastData = true;
                            //
                            for (j = (lststrTempBuff.Count - 1); j >= i; j--) //Ex: 25,26,27
                            {
                                strNewData = strNewData + lststrTempBuff[j];
                            }   
                        }
                        strDataToWrite = strDataToWrite + strNewData + ",";

                        //Check if last data reach
                        if (blLastData == true)
                        {
                            sWriter.WriteLine(strDataToWrite); strTotalData += strDataToWrite + "\r\n";
                            strDataToWrite = ""; //Reset for next
                        }
                    }

                    //Close when finish
                    sWriter.Close();
                    //
                    break;

                default: //For LF & APP is same
                    //Combine 2 bytes to 1 data, when reaching 13 data => write to log file
                    //start write
                    //for (i = 0; i < lststrTempBuff.Count;i=i+4)
                    for (i = 0; i < lststrTempBuff.Count; i = i + 2)
                    {
                        if((i>0)&&((i%26)==0)) //Need to write 
                        {
                            sWriter.WriteLine(strDataToWrite); strTotalData += strDataToWrite + "\r\n";
                            strDataToWrite = ""; //Reset for next
                        }

                        string strNewData = "";
                        blLastData = false;
                        if(i+1<lststrTempBuff.Count) //Not Last data
                        {
                            strNewData = lststrTempBuff[i + 1] + lststrTempBuff[i];
                        }
                        else //Last data
                        {
                            blLastData = false;
                            //
                            strNewData = lststrTempBuff[i];
                        }
                        strDataToWrite = strDataToWrite + strNewData + ",";

                        //Check if last data reach
                        if (blLastData == true)
                        {
                            sWriter.WriteLine(strDataToWrite); strTotalData += strDataToWrite + "\r\n";
                            strDataToWrite = ""; //Reset for next
                        }
                    }

                    //Close when finish
                    sWriter.Close();
                    //
                    break;
            }

            //Saving file path name to User Ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("FilePath");
            lstobjTemp.Add(strDataFileFullName);
            lstlstobjOutput.Add(lstobjTemp);

            //Save all data already write
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Data");
            lstobjTemp.Add(strTotalData);
            lstlstobjOutput.Add(lstobjTemp);


            //
            return "0";
        }


        /// <summary>
        /// Motor belt tension & Gear Dent Calculation
        /// + Para1(13): Input Mode - 0: Input text directly - Default
        ///                         - 1: Reading from File
        /// + Para1(14): Input Mode = 0: Text (all motor data) to input
        ///                         = 1: Motor Data File
        /// Return:
        /// + Function return: value of belt tension
        /// + User return: Value of gear Dent
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainUSBFuncID101(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //string strFilePath = lstlstobjInput[0][13].ToString().Trim();

            string strInputOption = lstlstobjInput[0][13].ToString().Trim();
            if (strInputOption != "1") strInputOption = "0"; //Return to default value

            string strFileData = lstlstobjInput[0][14].ToString().Trim();

            //If file exist, then load file and analyze data
            List<double> lstdblRealVal = new List<double>(); //store actual speed data
            List<double> lstdblTheoryVal = new List<double>(); //store theory speed data
            int i = 0;
            List<string> lststrLine = new List<string>();

            //
            if(strInputOption == "1") //Read data from file
            {
                //Check if file is exist or not
                if (MyLibrary.ChkExist.CheckFileExist(strFileData) == false)
                {
                    return "100-0-101 Error: file is not exist.";
                }

                //If Input Mode is reading from file => Analyze from file
                using (StreamReader reader = new StreamReader(strFileData))
                {
                    //Read all line until reach the end of file
                    string strline = "";
                    while ((strline = reader.ReadLine()) != null)
                    {
                        lststrLine.Add(strline);
                    }
                }
            }
            else //Read from text input directly
            {
                lststrLine = new List<string>(strFileData.Split(new string[]{"\r\n"}, StringSplitOptions.None));
            }

            for (i = 0; i < lststrLine.Count; i++)
            {
                List<string> lststrTemp = new List<string>(lststrLine[i].Split(','));
                //Get real value and theory value
                int intTemp = 0;
                string strTemp = "";
                if (lststrTemp.Count >= 4)
                {
                    //Get real value
                    strTemp = lststrTemp[2].Replace("\"", "");
                    if (int.TryParse(strTemp, System.Globalization.NumberStyles.HexNumber, null, out intTemp) == false)
                    {
                        break;
                    }
                    lstdblRealVal.Add(intTemp);

                    //Get theory value
                    strTemp = lststrTemp[3].Replace("\"", "");
                    if (int.TryParse(strTemp, System.Globalization.NumberStyles.HexNumber, null, out intTemp) == false)
                    {
                        break;
                    }
                    lstdblTheoryVal.Add(intTemp);
                }
            }



            //Reject first 100 value & last 100 value
            if (lstdblRealVal.Count < 200) return "100-0-101 Error: number of real value less than 200";
            if (lstdblTheoryVal.Count < 200) return "100-0-101 Error: number of theory value less than 200";

            lstdblRealVal.RemoveRange(0, 100);
            lstdblRealVal.RemoveRange(lstdblRealVal.Count - 100, 100);
            lstdblTheoryVal.RemoveRange(0, 100);
            lstdblTheoryVal.RemoveRange(lstdblTheoryVal.Count - 100, 100);

            //Cal Dent Value
            double dblGearDentValue = 100;
            double dblTotalValue = 0;
            int intCount = 0;
            for (i = 0; i < lstdblRealVal.Count;i++)
            {
                if (lstdblTheoryVal[i] != 0)
                {
                    intCount++;
                    dblTotalValue += Math.Abs((lstdblRealVal[i] - lstdblTheoryVal[i]) / lstdblTheoryVal[i]);
                }
            }
            if(intCount!=0)
            {
                dblGearDentValue = 100 * (dblTotalValue / intCount);
            }
            //Add Gear Dent to User Ret output
            lstobjTemp = new List<object>();
            lstobjTemp.Add("GearDent");
            lstobjTemp.Add(dblGearDentValue);
            lstlstobjOutput.Add(lstobjTemp);


            //Sort list of real value and theory value
            lstdblRealVal.Sort();
            lstdblTheoryVal.Sort();

            //Get average of first 5 value and last five value of actual speed
            if (lstdblRealVal.Count < 5) return "100-0-101 Error: not enough 5 real value for calculate!";
            double dbl1stAverVal = lstdblRealVal.GetRange(0, 5).Average();
            double dblLastAverVal = lstdblRealVal.GetRange(lstdblRealVal.Count - 5, 5).Average();

            //Calculate average of actual speed
            double dblActualSpeedAverVal = (lstdblTheoryVal[0] + lstdblTheoryVal.Last())/2;

            //Cal Minus and Plus side result
            double dblMsideResult = 100 * ((dbl1stAverVal - dblActualSpeedAverVal) / dblActualSpeedAverVal);
            double dblPsideResult = 100 * ((dblLastAverVal - dblActualSpeedAverVal) / dblActualSpeedAverVal);

            double dblBeltTensionResult = dblMsideResult;

            if(Math.Abs(dblPsideResult)>Math.Abs(dblMsideResult))
            {
                dblBeltTensionResult = dblPsideResult;
            }

            //Add Gear Dent to User Ret output
            lstobjTemp = new List<object>();
            lstobjTemp.Add("BeltTension");
            lstobjTemp.Add(dblBeltTensionResult);
            lstlstobjOutput.Add(lstobjTemp);

            //Return 0 if everything is Ok
            //return dblBeltTensionResult;
            return "0";
        }



        //Support function
        private static Mutex mut = new Mutex();
        public void AppendCsvFile(string strFileFullName, string strDataToAppend)
        {
            try
            {
                mut.WaitOne();
                //
                StreamWriter sWriter;
                sWriter = File.AppendText(strFileFullName);
                sWriter.WriteLine(strDataToAppend);
                sWriter.Close();
                //
                mut.ReleaseMutex();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "AppendCsvFile()");
            }
        }


        //Constructor
        clsMainUSB()
        {
            this.clsMainUsb = new clsCanonUsbKernel.clsUsbKernel();
            this.strPIPE_NAME = "";
            //Main FCT variables
            this.strcMainUSBVar = new MainUSBVar();
            this.strcMainUSBVar.strMainPipeName = "";
            this.strcMainUSBVar.strDummyMainPipeName = "";
        }

    }

}
