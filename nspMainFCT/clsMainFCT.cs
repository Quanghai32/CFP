/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.ComponentModel.Composition;
using System.Windows.Forms;
using nspMcrInterface;

namespace nspMainFCT
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginMainFCT,10")]
    public class clsMainFCT : nspINTERFACE.IPluginExecute //, nspMainUsbInterface.IMainUsb
    {
        //Both Master Process and Child Process also have their own collection. Take care to separate them!
        [ImportMany(typeof(nspINTERFACE.IPluginExecute))]
        public List<Lazy<nspINTERFACE.IPluginExecute, nspINTERFACE.IPluginInfo>> lstPluginMasterCollection;
        public Lazy<nspINTERFACE.IPluginExecute, nspINTERFACE.IPluginInfo> nspMasterCallBack;

        //For Micro-controller communication
        [ImportMany(typeof(nspMcrInterface.IMcrUsb))]
        public List<Lazy<nspMcrInterface.IMcrUsb, nspMcrInterface.IPluginMcrInfo>> lstPluginCollectionMicroUSB;
        public Lazy<nspMcrInterface.IMcrUsb, nspMcrInterface.IPluginMcrInfo> nspMicroUSB;

        //For Main PCB communication
        [ImportMany(typeof(nspMainUsbInterface.IMainUsb))]
        public List<Lazy<nspMainUsbInterface.IMainUsb, nspMainUsbInterface.IPluginMainUsbInfo>> lstPluginCollectionMainUSB;
        public Lazy<nspMainUsbInterface.IMainUsb, nspMainUsbInterface.IPluginMainUsbInfo> nspMainUSB;

        //MAIN FCT GLOBAL VARIABLES
        public struct ModelSetting
        {
            public string strModelCode;
            public string strRomVersion;
            public string strLoaderVersion;
            public string strRomCheckMode;
            public string strRamCheckMode;
            public string strMotorDriver1;
            public string strMotorDriver2;
            public string intFixNum;
        }
        public ModelSetting strcModelSetting;

        public struct FctVar
        {
            public int intFixNumber; //Fixture Number
        }
        public FctVar strcFctVar;

        //User Form
        public clsFrmUserSetting clsUserSetting; //Select Model code, program rom version...

        public clsTcpIpHandle clsTCPIPServer = new clsTcpIpHandle();


        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";
            //Inform to Host program which Function this plugin support
            strTemp = "10,0,0,1,2,3,4,100,101,102,103,104,500,1000,1001,9999"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.00"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 20/8/2015"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For control Main FCT!"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);

            //Looking for Micro-controller part
            int i = 0;
            for (i = 0; i < lstPluginCollectionMicroUSB.Count; i++)
            {
                if (lstPluginCollectionMicroUSB[i].Metadata.IPluginMcrInfo == "nspMicroUSB")
                {
                    nspMicroUSB = lstPluginCollectionMicroUSB[i];
                }
            }
            //Looking for Main USB part
            for (i = 0; i < lstPluginCollectionMainUSB.Count; i++)
            {
                if (lstPluginCollectionMainUSB[i].Metadata.IPluginMainUsbInfo == "nspMainUSB")
                {
                    nspMainUSB = lstPluginCollectionMainUSB[i];
                }
            }

        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Process ID - ... - JigID-HardID-FuncID"
            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());
            switch (intJigID) //Select JigID
            {
                case 10:
                    return SelectHardIDFromJigID10(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        //From MEF part request back to master program
        public List<List<object>> lstlstobjInputPendingCommand { get; set; }
        public object IPendingFunctionExecute(out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            return IFunctionExecute(this.lstlstobjInputPendingCommand, out lstlstobjOutput);
        }
        #endregion

        public object SelectHardIDFromJigID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return PluginMainFCTFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginMainFCTFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginMainFCTFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginMainFCTFuncID3(lstlstobjInput, out lstlstobjOutput);
                case 4:
                    return PluginMainFCTFuncID4(lstlstobjInput, out lstlstobjOutput);
                case 100:
                    return PluginMainFCTFuncID100(lstlstobjInput, out lstlstobjOutput);
                case 101:
                    return PluginMainFCTFuncID101(lstlstobjInput, out lstlstobjOutput);
                case 102:
                    return PluginMainFCTFuncID102(lstlstobjInput, out lstlstobjOutput);
                case 103:
                    return PluginMainFCTFuncID103(lstlstobjInput, out lstlstobjOutput);
                case 104:
                    return PluginMainFCTFuncID104(lstlstobjInput, out lstlstobjOutput);
                case 500:
                    return PluginMainFCTFuncID500(lstlstobjInput, out lstlstobjOutput);
                case 1000:
                    return PluginMainFCTFuncID1000(lstlstobjInput, out lstlstobjOutput);
                case 1001:
                    return PluginMainFCTFuncID1001(lstlstobjInput, out lstlstobjOutput);
                case 9999:
                    return PluginMainFCTFuncID9999(lstlstobjInput, out lstlstobjOutput);

                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }


        /// <summary>
        /// Reading Config for Main FCT
        ///     - Reading Model setting: Rom version, Loader version...
        /// Parameter setting
        ///     + Para1 (13): File to get config
        ///     + Para2 (14): Section to get config
        ///     + Para3 (15): Keyname to get config
        ///     + Para4 (16): - 0 (Default) : Read all "strModelCode,strRomVersion,strLoaderVersion,strRomCheckMode,strRamCheckMode,strMotorDriver1,strMotorDriver2"
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<string>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: Setting file is not exist";
            }

            string strTmp = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyName = lstlstobjInput[0][15].ToString();

            //Reading strModelCode
            strKeyName = "ModelCode";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'ModelCode' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strModelCode = strTmp;
            //Reading strRomVersion
            strKeyName = "RomVersion";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'RomVersion' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strRomVersion = strTmp;
            //Reading strLoaderVersion
            strKeyName = "LoaderVersion";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'LoaderVersion' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strLoaderVersion = strTmp;
            //Reading strRomCheckMode
            strKeyName = "RomCheckMode";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'RomCheckMode' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strRomCheckMode = strTmp;
            //Reading strRamCheckMode
            strKeyName = "RamCheckMode";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'RamCheckMode' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strRamCheckMode = strTmp;
            //Reading strRamCheckMode
            strKeyName = "MotorDriver1Version";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'MotorDriver1Version' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strMotorDriver1 = strTmp;
            //Reading strRamCheckMode
            strKeyName = "MotorDriver2Version";
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'MotorDriver2Version' setting in " + strSectionName; //Getting key value fail
            }
            this.strcModelSetting.strMotorDriver2 = strTmp;

            //If everything is OK. Then return successful code
            return "0";
        }

        /// <summary>
        /// McrUSB_WR()
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();
            byte[] byteArrMcrUsbReceive = new byte[1];

            int iret = 0;
            int intTimeOut = 0;

            //Analyze retry times
            if (Int32.TryParse(lstlstobjInput[0][14].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = Int32.Parse(lstlstobjInput[0][14].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                intTimeOut = 1000;
            }
            //Then, we sending usb command and getting data return
            iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][13].ToString(), ref byteArrMcrUsbReceive, intTimeOut);
            //Calculate for user string return data
            lstobjTemp.Add("McrUSB");
            int i = 0;
            string strTemp = "";
            for (i = 0; i < byteArrMcrUsbReceive.Length; i++)
            {
                strTemp = byteArrMcrUsbReceive[i].ToString();
                lstobjTemp.Add(strTemp);
            }
            //Add to list of list ouput data
            lstlstobjOutput.Add(lstobjTemp);

            //Add List result
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Result");
            lstobjTemp.Add(iret.ToString());
            //Add to list of list ouput data
            lstlstobjOutput.Add(lstobjTemp);

            //If NG, depend on Error code, add error message to output data
            string strErrMsg = "";
            switch (iret)
            {
                case 0: //OK
                    strErrMsg = "";
                    break;
                case 201:
                    strErrMsg = "USB Command Length NG";
                    break;
                case 202://NG
                    strErrMsg = "readHandle error";
                    break;
                case 203:
                    strErrMsg = "Time out error";
                    break;
                case 204://NG
                    strErrMsg = "Device disconnected";
                    break;
                default://Unkown NG code
                    break;
            }

            if (iret != 0) //NG. Return Error Message
            {
                lstobjTemp = new List<object>();
                lstobjTemp.Add("ErrMsg");
                lstobjTemp.Add(strErrMsg);
                lstlstobjOutput.Add(lstobjTemp);
            }

            //Return value
            return iret.ToString();
        }

        /// <summary>
        /// Check Fixture setting
        ///     + Para1 (13): USB command sending to check 
        ///     + Para2 (14): Position of return data (count from 0) that contain number fixture info setting
        ///     + Para3 (15): Filename to looking for setting (UserIni.ini)
        ///     + Para4 (16): Section name to looking for setting (CHECK_SETTING)
        ///     + Para5 (17): keyname to looking for setting BUT NOT INCLUDE INDEX (FixNumber)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            try
            {
                if (lstlstobjInput[0].Count < 7) return "Error";

                //1. Send usb command and get return data
                int iret = 0;
                int intFixNum = 0;
                int intBytePos = 0;
                byte[] byteArrMcrUsbReceive = new byte[1];

                int intTimeOut = 5000;
                //Then, we sending usb command and getting data return
                iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][13].ToString(), ref byteArrMcrUsbReceive, intTimeOut);

                if (iret != 0) //Sending command fail
                {
                    lststrTemp = new List<object>();
                    lststrTemp.Add("ErrMsg");
                    lststrTemp.Add("Micro USB command sending fail!");
                    lstlstobjOutput.Add(lststrTemp);
                    return "Error";
                }

                //Confirm Para2 is numeric or not
                if (int.TryParse(lstlstobjInput[0][14].ToString(), out intBytePos) == false)
                {
                    return "Error";
                }
                intBytePos = int.Parse(lstlstobjInput[0][14].ToString());
                //If OK, then assign Fixnum
                intFixNum = byteArrMcrUsbReceive[intBytePos];
                //Reading from Ini file to confirm Fix Number config in setting file

                //1. Check if file is exist or not
                string strAppPath = lstlstobjInput[0][0].ToString();
                string iniFileName = @"\" + lstlstobjInput[0][15].ToString();
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
                strKeyName = lstlstobjInput[0][17].ToString().Trim() + (intProcessID + 1).ToString(); //Automatically add

                //strKeyName = lstlststrInput[0][17].Trim() + "1";

                //Reading value in key name
                strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][16].ToString(), strKeyName, strFileName);

                //strTmp = MyLibrary.ReadFiles.IniReadValue("CHECK_SETTING", "FixNumber1", strFileName);

                if (strTmp.ToLower() == "error")
                {
                    return "error: cannot find [" + strKeyName + "] setting in [" + lstlstobjInput[0][16] + "]"; //Getting key value fail
                }
                //Confirm if return value is numeric or not
                int intTemp = 0;
                if (int.TryParse(strTmp, out intTemp) == false) return "error";
                intTemp = int.Parse(strTmp);

                //Compare with reading result from Micro-controller
                if (intTemp == intFixNum) //Setting OK
                {
                    this.strcFctVar.intFixNumber = intFixNum;
                    return intFixNum; //Succesful code  //TRUNG B 07122015
                }
                else //Setting different with reading result. Return error message
                {
                    lststrTemp = new List<object>();
                    lststrTemp.Add("ErrMsg");
                    lststrTemp.Add("Setting file [" + intTemp.ToString() + "] different from reading result [" + intFixNum.ToString() + "]");
                    lstlstobjOutput.Add(lststrTemp);
                    return "Error : Setting file [" + intTemp.ToString() + "] different from reading result [" + intFixNum.ToString() + "]";
                }
            }
            catch (Exception ex) //Unexpected error happen
            {
                lststrTemp = new List<object>();
                lststrTemp.Add("ErrMsg");
                lststrTemp.Add(ex.Message);
                lstlstobjOutput.Add(lststrTemp);
                return "Error";
            }
        }

        /// <summary>
        /// Reset Pulse width function
        ///     + Para1 (13): USB command send to supply power for Main PCB
        ///     + Para2 (14): USB command send to check 3.3V
        ///     + Para3 (15): USB command send to check Reset signal
        ///     + Para4 (16): Polling voltage level (Default is 2V)
        ///     + Para5 (17): Time out polling (Default is 3000ms)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<Object>>();
            var lststrTemp = new List<object>();

            //Polling when 3.3V reach 2 Voltage
            int intTick3V = 0;
            int intTickReset = 0;
            double dblPollingLevel = 0;
            int intTimeOut = 0;
            //Check if numeric or not
            if (double.TryParse(lstlstobjInput[0][16].ToString(), out dblPollingLevel) == false) // return "Error: Voltage Polling level is not numeric!";
            {
                dblPollingLevel = 2.0;
            }
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intTimeOut) == false) // return "Error: Time out setting is not numeric!";
            {
                intTimeOut = 3000;
            }

            bool blFlagTimeOut = false;
            int iret = 0;
            byte[] byteArrMcrUsbReceive = new byte[1];
            double dblVoltTemp = 0;


            //Start supply power for Main PCB
            iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][13].ToString(), ref byteArrMcrUsbReceive, intTimeOut);
            if (iret != 0) return "Error: Micro USB command sending fail. Cannot supply power for Main PCB!";

            //Mark Start Time
            int intStartTime = MyLibrary.ApiDeclaration.GetTickCount();
            //Polling 3.3V signal until reach over 2V
            do
            {
                //Sending command
                iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][14].ToString(), ref byteArrMcrUsbReceive, intTimeOut);
                //Mark Tick 3.3V
                intTick3V = MyLibrary.ApiDeclaration.GetTickCount();
                if ((intTick3V - intStartTime) > intTimeOut) blFlagTimeOut = true;
                //If sending is OK. Then calculate result
                if (iret == 0)
                {
                    dblVoltTemp = (256 * Convert.ToDouble(byteArrMcrUsbReceive[4]) + Convert.ToDouble(byteArrMcrUsbReceive[5])) * 5 / 1023;
                    dblVoltTemp = Math.Round(dblVoltTemp, 2);
                }
            }
            while ((dblVoltTemp < dblPollingLevel) && (blFlagTimeOut == false));

            if (blFlagTimeOut == true) return "Error: 3.3V waiting timeout!";

            //Polling Reset Pulse signal until reach over 2V
            intStartTime = MyLibrary.ApiDeclaration.GetTickCount();
            do
            {
                //Sending command
                iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][15].ToString(), ref byteArrMcrUsbReceive, intTimeOut); //Reset Pulse signal 
                //Mark Tick reset
                intTickReset = MyLibrary.ApiDeclaration.GetTickCount();
                if ((intTickReset - intStartTime) > intTimeOut) blFlagTimeOut = true;
                //If sending is OK. Then calculate result
                if (iret == 0)
                {
                    dblVoltTemp = (256 * Convert.ToDouble(byteArrMcrUsbReceive[4]) + Convert.ToDouble(byteArrMcrUsbReceive[5])) * 5 / 1023;
                    dblVoltTemp = Math.Round(dblVoltTemp, 2);
                }
            }
            while ((dblVoltTemp < dblPollingLevel) && (blFlagTimeOut == false));

            if (blFlagTimeOut == true) return "Error: Reset Pulse waiting timeout!";

            //If OK, then calculate Reset Pulse width
            int intResult = intTickReset - intTick3V;

            return intResult.ToString();
        }


        /// <summary>
        /// Current Checking Get back result
        ///     + Para1 (13): USB command send to get result back
        ///     + Para2 (14): High byte position of Result
        ///     + Para3 (15): Low byte position of Result
        ///     + Para4 (16): Vref of Micro-controller
        ///     + Para5 (17): Resolution of AD module
        ///     + Para6 (18): Kconvert (Default is 1 - Low voltage)
        ///     + Para7 (19): Voltage/Current factor
        ///     + Para8 (20): Rounding Digit (Default value is 2)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID4(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Polling when 3.3V reach 2 Voltage
            int intHiBytePos = 0;
            int intLoBytePos = 0;
            double dblVoltRef = 0;
            int intResolution = 0;
            double dblKconvert = 0;
            double dblVoltDivCurr = 0;
            int intRoundingDigit = 0;
            //Check if numeric or not
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intHiBytePos) == false) return "Error: High byte position setting is not integer!";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intLoBytePos) == false) return "Error: Low byte position setting is not integer!";
            if (double.TryParse(lstlstobjInput[0][16].ToString(), out dblVoltRef) == false) return "Error: Vref setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intResolution) == false) return "Error: Resolution setting is not integer!";
            if (double.TryParse(lstlstobjInput[0][18].ToString(), out dblKconvert) == false) return "Error: Kconvert setting is not numeric!";
            if (double.TryParse(lstlstobjInput[0][19].ToString(), out dblVoltDivCurr) == false) return "Error: Volt/Current factor setting is not numeric!";
            if (int.TryParse(lstlstobjInput[0][20].ToString(), out intRoundingDigit) == false) return "Error: Rounding digit setting is not integer!";

            int iret = 0;
            byte[] byteArrMcrUsbReceive = new byte[1];
            int intTimeOut = 1000;
            double dblResult = 0;

            //Start supply power for Main PCB
            iret = nspMicroUSB.Value.McrUSB_WR(lstlstobjInput[0][13].ToString(), ref byteArrMcrUsbReceive, intTimeOut);
            if (iret != 0) return "Error: Micro USB command sending fail. Cannot supply power for Main PCB!";
            if (byteArrMcrUsbReceive[5] != 0) return "Error: Current Checking time out!";
            //Calculate result
            dblResult = (256 * Convert.ToDouble(byteArrMcrUsbReceive[intHiBytePos]) + Convert.ToDouble(byteArrMcrUsbReceive[intLoBytePos])) * dblVoltRef / Convert.ToDouble(intResolution);
            dblResult = Math.Round((dblResult / dblVoltDivCurr), intRoundingDigit);

            return dblResult.ToString();
        }

        /// <summary>
        /// Reading USB serial number from saving file
        ///     +Para1 (13): File to get USB serial number saving
        ///     +Para2 (14): Section to get USB serial number saving
        ///     +Para3 (15): Keyname to get USB serial number saving
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID100(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<string>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: Setting file is not exist";
            }

            string strTmp = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyName = lstlstobjInput[0][15].ToString();

            //Calculate string file name
            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "error";
            strKeyName = lstlstobjInput[0][15].ToString().Trim() + (intProcessID + 1).ToString(); //Automatically add

            //Reading USB serial number
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error") return "Error: cannot find [" + strKeyName + "] setting in [" + strSectionName + "]";
            //Check numeric or not
            int intUsbSerialNumber = 0;
            if (int.TryParse(strTmp, System.Globalization.NumberStyles.HexNumber, null, out intUsbSerialNumber) == false)
            {
                return "Error: USB serial number is not numeric (Hex format)!";
            }


            //If everything is OK. Then return successful code
            return intUsbSerialNumber.ToString();
        }

        /// <summary>
        /// Reading USB serial number from backup file
        ///     +Para1 (13): File name to save Backup path for USB serial number
        ///     +Para2 (14): Section name to save backup path for USB serial number & Sector name in backup file to save USB serial number
        ///     +Para3 (15): keyname to get backup path for USB serial number
        ///     +Para4 (16): Keyname to get USB serial number BACKUP saving
        /// </summary>
        /// <param name="lstlststrInput"></param>
        /// <param name="lststrOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID101(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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

            string strTmp = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strBackUpPath = MyLibrary.ReadFiles.IniReadValue(strSectionName, lstlstobjInput[0][15].ToString(), strFileName);
            //Check back up file is exist or not
            if (MyLibrary.ChkExist.CheckFileExist(strBackUpPath) == false)
            {
                return "Error: back up file[" + strBackUpPath + "] is not exist";
            }


            string strKeyName = lstlstobjInput[0][16].ToString();

            //Calculate string file name
            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "error";
            strKeyName = lstlstobjInput[0][16].ToString().Trim() + (intProcessID + 1).ToString(); //Automatically add

            //Reading USB serial number
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyName, strBackUpPath);
            if (strTmp.ToLower() == "error") return "Error: cannot find [" + strKeyName + "] setting in [" + strSectionName + "] in backup file";
            //Check numeric or not
            int intUsbSerialNumber = 0;
            if (int.TryParse(strTmp, System.Globalization.NumberStyles.HexNumber, null, out intUsbSerialNumber) == false)
            {
                return "Error: USB serial number is not numeric (Hex format)!";
            }

            //If everything is OK. Then return successful code
            return intUsbSerialNumber.ToString();
        }


        /// <summary>
        /// USB SERIAL CALCULATION
        ///     +Para1 (13): Mode to calculate usb serial number (0: ASCII mode. 1: HEX mode)
        ///     +Para1 (14): Saving file name (Must be same folder with EXE file)
        ///     +Para2 (15): Section name to get usb serial number in saving file (And Backup file)
        ///     +Para3 (16): Key name to get usb serial number in saving file & back up file, automatically add index
        ///     +Para4 (17): Key name to indicate path of back up file (in saving file)
        ///     
        /// </summary>
        /// <param name="lstlststrInput"></param>
        /// <param name="lststrOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID102(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<string>();

            //Confirm USB mode
            int intUsbMode = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intUsbMode) == false) return "Error: USB mode setting is not integer format!";
            if ((intUsbMode != 0) && (intUsbMode != 1)) return "Error: USB mode support 0[ASCII mode] & 1[HEX mode] only!";

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][14].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false) return "Error: File is not exist!";

            string strTmp = "";
            string strKeyName = "";
            string strSectionName = "";

            //Calculate string file name
            strSectionName = lstlstobjInput[0][15].ToString();

            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "error";
            strKeyName = lstlstobjInput[0][16].ToString().Trim() + (intProcessID + 1).ToString(); //Automatically add

            //Reading value in key name
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyName, strFileName);
            if (strTmp.ToLower() == "error") return "error: cannot find [" + strKeyName + "] setting in [" + strSectionName + "]"; //Getting key value fail
            string strUsbSerialSaving = strTmp;

            //Reading from Backup file
            string strBackUpFilePath = MyLibrary.ReadFiles.IniReadValue(strSectionName, lstlstobjInput[0][17].ToString(), strFileName);
            //Confirm Backup file is exist or not
            if (MyLibrary.ChkExist.CheckFileExist(strBackUpFilePath) == false) return "Error: Back up File is not exist!";
            //Reading value in back up file
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyName, strBackUpFilePath);
            if (strTmp.ToLower() == "error") return "error: cannot find [" + strKeyName + "] setting in [" + strSectionName + "] of backup file"; //Getting key value fail
            string strUsbSerialBackup = strTmp;

            //Compare saving file & back up file
            //if (strUsbSerialSaving != strUsbSerialBackup) return "Error: USB serial number in saving file & back up file is not match!";

            long lngUsbNumber1 = 0;
            long lngUsbNumber2 = 0;

            if (long.TryParse(strUsbSerialSaving, System.Globalization.NumberStyles.HexNumber, null, out lngUsbNumber1) == false) return "USB serial saving in log file  is not numeric (Hex format)!";
            if (long.TryParse(strUsbSerialBackup, System.Globalization.NumberStyles.HexNumber, null, out lngUsbNumber2) == false) return "USB serial saving in back up file  is not numeric (Hex format)!";

            if (lngUsbNumber1 != lngUsbNumber2) return "Error: USB serial number in saving file[" + lngUsbNumber1.ToString() + "] & back up file" + lngUsbNumber2.ToString() + " is not match!";

            //Confirm numerical or not (Hexa format)
            long lngUsbNumber = 0;
            if (long.TryParse(strUsbSerialSaving, System.Globalization.NumberStyles.HexNumber, null, out lngUsbNumber) == false) return "USB serial saving before is not numeric (Hex format)!";

            //OK, now we calculate new usb serial number (Note that using Fixture number)
            //1. Increase USB number by 1
            ++lngUsbNumber;
            if (lngUsbNumber > 0xFFFFF) return "Error: USB serial number over flow!" + "[0x" + lngUsbNumber.ToString("X") + "]";
            //2. Add Fixture Number into calculation
            lngUsbNumber = lngUsbNumber + this.strcFctVar.intFixNumber * 0x100000;
            ////3. Now, base on what kind of USB serial number, we need separate to each byte to send USB command
            //string[] strUsbASCII = new string[6];
            //char[] chrUsbASCII = new char[6];
            //string[] strUsbHEX = new string[3];

            ////USB serial number = 1A2B3C
            ////
            //// + strUsbHEX[0] = "1A"
            //// + strUsbHEX[1] = "2B"
            //// + strUsbHEX[2] = "3C"
            //// 
            //// * strUsbASCII[0] = Hex(ASC("1"))
            //// * strUsbASCII[1] = Hex(ASC("A"))
            //// * strUsbASCII[2] = Hex(ASC("2"))
            //// * strUsbASCII[3] = Hex(ASC("B"))
            //// * strUsbASCII[4] = Hex(ASC("3"))
            //// * strUsbASCII[5] = Hex(ASC("C"))

            //string strUsbNumber = lngUsbNumber.ToString("X");
            ////Fill with all "0" if strUsbNumber less than 6 character
            //int i = 0;
            //for (i = strUsbNumber.Length;i< 6; i++) //43C => 00043C
            //{
            //    strUsbNumber = "0" + strUsbNumber;
            //}
            ////Ok. Now strUsbNumber have 6 characters. We assign values for ASCII & HEX array
            ////ASCII Array
            //for (i = 0; i < 6; i++)
            //{
            //    //Get each character in string
            //    strUsbASCII[i] = strUsbNumber.Substring(i, 1);
            //    chrUsbASCII[i] = Convert.ToChar(strUsbASCII[i]);
            //    //Now convert to ASCII code
            //    int intAscVal = Convert.ToInt32(chrUsbASCII[i]);
            //    //Then, convert again to Hex format
            //    strUsbASCII[i] = intAscVal.ToString("X");
            //}
            ////HEX Array
            //for (i = 0; i < 3; i++)
            //{
            //    strUsbHEX[i] = strUsbNumber.Substring(i * 2, 2);
            //}

            //If everything is OK. Then return USB serial number
            return lngUsbNumber.ToString();
        }


        /// <summary>
        /// USB SERIAL WRITING
        ///     +Para1 (13): Mode of usb serial number (0: ASCII mode. 1: HEX mode)
        ///     +Para2 (14): USB number to write (Need to take from step before: USB serial number calculation)
        ///     +Para3 (15): Main PCB USB command to write USB - without USB bytes indicate.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID103(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Confirm USB mode
            int intUsbMode = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intUsbMode) == false) return "Error: USB mode setting is not integer format!";
            if ((intUsbMode != 0) && (intUsbMode != 1)) return "Error: USB mode support 0[ASCII mode] & 1[HEX mode] only!";

            //Confirm numerical or not (Hexa format)
            int intUsbNumber = 0;
            //if (int.TryParse(lstlstobjInput[0][14].ToString(), System.Globalization.NumberStyles.HexNumber, null, out intUsbNumber) == false) return "Error: USB serial number[" + lstlstobjInput[0][14] + "] want to write is not numeric!";
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intUsbNumber) == false) return "Error: USB serial number[" + lstlstobjInput[0][14] + "] want to write is not numeric!";

            //3. Now, base on what kind of USB serial number, we need separate to each byte to send USB command
            string[] strUsbASCII = new string[6];
            char[] chrUsbASCII = new char[6];
            string[] strUsbHEX = new string[3];

            //USB serial number = 1A2B3C
            //
            // + strUsbHEX[0] = "1A"
            // + strUsbHEX[1] = "2B"
            // + strUsbHEX[2] = "3C"
            // 
            // * strUsbASCII[0] = Hex(ASC("1"))
            // * strUsbASCII[1] = Hex(ASC("A"))
            // * strUsbASCII[2] = Hex(ASC("2"))
            // * strUsbASCII[3] = Hex(ASC("B"))
            // * strUsbASCII[4] = Hex(ASC("3"))
            // * strUsbASCII[5] = Hex(ASC("C"))

            string strUsbNumber = intUsbNumber.ToString("X");
            //Fill with all "0" if strUsbNumber less than 6 character
            int i = 0;
            for (i = strUsbNumber.Length; i < 6; i++) //43C => 00043C
            {
                strUsbNumber = "0" + strUsbNumber;
            }
            //Ok. Now strUsbNumber have 6 characters. We assign values for ASCII & HEX array
            //ASCII Array
            for (i = 0; i < 6; i++)
            {
                //Get each character in string
                strUsbASCII[i] = strUsbNumber.Substring(i, 1);
                chrUsbASCII[i] = Convert.ToChar(strUsbASCII[i]);
                //Now convert to ASCII code
                int intAscVal = Convert.ToInt32(chrUsbASCII[i]);
                //Then, convert again to Hex format
                strUsbASCII[i] = intAscVal.ToString("X");
            }


            //////////calculate for using return data ASCII//// TRUNG B
            lstobjTemp.Add("ASCII");
            string strTemp1 = "";
            for (i = 0; i < 6; i++)
            {
                strTemp1 = strUsbASCII[i].ToString();
                lstobjTemp.Add(strTemp1);
            }
            //Add to list of list ouput data
            lstlstobjOutput.Add(lstobjTemp);

            //////////////////////////////////////////////
            //HEX Array
            for (i = 0; i < 3; i++)
            {
                strUsbHEX[i] = strUsbNumber.Substring(i * 2, 2);
            }


            //Depend on USB mode, calculate USB command to send to Main PCB
            string strUsbCommand = lstlstobjInput[0][15].ToString();

            switch (intUsbMode)
            {
                case 0: //ASCII mode
                    for (i = 0; i < 6; i++)
                    {
                        strUsbCommand = strUsbCommand + strUsbASCII[i] + ",";
                    }
                    break;
                case 1: //HEX mode
                    for (i = 0; i < 3; i++)
                    {
                        strUsbCommand = strUsbCommand + strUsbHEX[i] + ",";
                    }
                    break;
                default: //Unkown mode
                    strUsbCommand = "";
                    return "Error: Unrecognize USB mode[" + intUsbMode.ToString() + "]";
                //break;
            }

            //OK, now send command to Main PCB
            int iret = 0;
            byte[] byteMainUsbReceiveData = new byte[1];

            iret = nspMainUSB.Value.USB_WR(strUsbCommand, out byteMainUsbReceiveData);
            if (iret != 0) return "Error: Cannot write USB command to Main PCB";
            //Calculate for user string return data
            lstobjTemp.Add("MainUSB");
            string strTemp = "";
            for (i = 0; i < byteMainUsbReceiveData.Length; i++)
            {
                strTemp = byteMainUsbReceiveData[i].ToString();
                lstobjTemp.Add(strTemp);
            }
            lstlstobjOutput.Add(lstobjTemp);

            //If everything is OK. Then return code
            return iret.ToString();
        }



        /// <summary>
        /// USB SERIAL get from PCB USB server
        ///     +Para1 (13): file name to get information of USB server
        ///     +Para1 (14): section name to get IP server
        ///     +Para2 (15): key name to get IP address
        ///     +Para3 (16): Key name to get USB server port  
        /// </summary>
        /// <param name="lstlststrInput"></param>
        /// <param name="lststrOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID104(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<string>();
            int iRet = 0;
            string PcbServerIPAddress = "";
            int PcbServerPort;
            string strSending;
            string IPcheckerPC;
            string MsgRevUsb = "";
            string MsgErr = "";
            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;
            //checking file exist
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: Setting file is not exist";
            }

            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyName_ServerAddress = lstlstobjInput[0][15].ToString();
            string strKeyName_ServerPort = lstlstobjInput[0][16].ToString();
            //getting PCB Server IP Address from setting file
            PcbServerIPAddress = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyName_ServerAddress, strFileName);
            if (PcbServerIPAddress.ToLower() == "")
            {
                return "Error: cannot find [" + strKeyName_ServerAddress + "] setting in [" + strSectionName + "]";
            }

            // getting PCB Server Port from setting file
            PcbServerPort = Convert.ToInt16(MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyName_ServerPort, strFileName));

            // getting IP of checker PC
            IPcheckerPC = clsTCPIPServer.GetIPv4Address();

            // send command to PCB server and get USB number
            strSending = "USB-" + this.strcModelSetting.strModelCode.ToString() + "-" + IPcheckerPC.ToString();
            iRet = clsTCPIPServer.SocketServerWR(PcbServerIPAddress, PcbServerPort, strSending, ref MsgRevUsb, ref MsgErr);

            if (iRet != 0) return "Error: SocketServerWR() failed. Return data: " + iRet.ToString();

            int intUsbNumber = 0;

            if (Int32.TryParse(MsgRevUsb, System.Globalization.NumberStyles.HexNumber, null, out intUsbNumber) == false) return "Error SocketServerWR(): return data [" + MsgRevUsb + "] is not Hexa number!";

            //getting USB number from data return
            //long lngUsbNumber = 0;
            //lngUsbNumber = intTemp;


            if (intUsbNumber > 0xFFFFFF)
            {
                return "Error: USB serial number over flow!" + "[0x" + intUsbNumber.ToString("X") + "]";
            }
            //return data
            return intUsbNumber.ToString();

        }

        /// <summary>
        /// Model setting command: setting model code, romversion...
        ///     +Para1 (13): strModelCode
        ///     +Para2 (14): strRomVersion
        ///     +Para3 (15): strLoaderVersion
        ///     +Para4 (16): strRomCheckMode
        ///     +Para5 (17): strRamCheckMode
        ///     +Para6 (18): strMotorDriver1
        ///     +Para7 (19): strMotorDriver2
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID500(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //assign all info for structure
            this.strcModelSetting.strModelCode = lstlstobjInput[0][13].ToString();
            this.strcModelSetting.strRomVersion = lstlstobjInput[0][14].ToString();
            this.strcModelSetting.strLoaderVersion = lstlstobjInput[0][15].ToString();
            this.strcModelSetting.strRomCheckMode = lstlstobjInput[0][16].ToString();
            this.strcModelSetting.strRamCheckMode = lstlstobjInput[0][17].ToString();
            this.strcModelSetting.strMotorDriver1 = lstlstobjInput[0][18].ToString();
            this.strcModelSetting.strMotorDriver2 = lstlstobjInput[0][19].ToString();

            //Try to saving on ini file
            string strAppPath = "";
            string iniUserFileName = @"\UserIni.ini";
            string strFileName = "";

            strAppPath = Application.StartupPath;
            strFileName = strAppPath + iniUserFileName;


            string strFullFileName = "";
            strFullFileName = Application.StartupPath + "";
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "ModelCode", this.strcModelSetting.strModelCode);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "RomVersion", this.strcModelSetting.strRomVersion);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "LoaderVersion", this.strcModelSetting.strLoaderVersion);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "RomCheckMode", this.strcModelSetting.strRomCheckMode);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "RamCheckMode", this.strcModelSetting.strRamCheckMode);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "MotorDriver1Version", this.strcModelSetting.strMotorDriver1);
            MyLibrary.WriteFiles.IniWriteValue(strFileName, "MODEL_SETTING", "MotorDriver2Version", this.strcModelSetting.strMotorDriver2);

            //If everything is OK. Then return code
            return "0";
        }

        /// <summary>
        /// User Utilities Call.
        /// This Function, using to call User form: Model setting...
        /// With Main FCT:
        ///     + 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID1000(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Looking for Process ID - only do with Process ID = 0 (1st item only)
            string strProcessID = lstlstobjInput[0][1].ToString();
            if (strProcessID.Trim() != "0") return "Error: This function only do with item Process ID number 0";

            //if (this.clsUserSetting.frmUserSetting.IsDisposed == false)
            //{
            //    MessageBox.Show("Setting File is already open!");
            //}


            //Ok. Then call out the new setting form
            this.clsUserSetting = new clsFrmUserSetting();
            this.clsUserSetting.InitializeComponent();
            this.clsUserSetting.frmUserSetting.Text = "Setting Config for Main PCB Info" + " - From Process ID" + strProcessID;
            this.clsUserSetting.frmUserSetting_Load();
            this.clsUserSetting.Show();
            this.clsUserSetting.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            //Add current setting
            this.clsUserSetting.tbModelCode.Text = this.strcModelSetting.strModelCode;
            this.clsUserSetting.tbRomVer.Text = this.strcModelSetting.strRomVersion;
            this.clsUserSetting.tbLoaderVer.Text = this.strcModelSetting.strLoaderVersion;
            this.clsUserSetting.tbMotorDriver1.Text = this.strcModelSetting.strMotorDriver1;
            this.clsUserSetting.tbMotorDriver2.Text = this.strcModelSetting.strMotorDriver2;
            this.clsUserSetting.tbRamCheckMode.Text = this.strcModelSetting.strRamCheckMode;
            this.clsUserSetting.tbRomCheckMode.Text = this.strcModelSetting.strRomCheckMode;

            //If everything is OK. Then return code
            return "0";
        }

        /// <summary>
        /// Testing call out Main PCB Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID1001(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            MessageBox.Show("Model code setting: " + this.strcModelSetting.strModelCode);
            MessageBox.Show("Rom version setting: " + this.strcModelSetting.strRomVersion);
            MessageBox.Show("Loader version setting: " + this.strcModelSetting.strLoaderVersion);
            MessageBox.Show("Motor driver 1 setting: " + this.strcModelSetting.strMotorDriver1);
            MessageBox.Show("Motor driver 2 setting: " + this.strcModelSetting.strMotorDriver2);
            MessageBox.Show("Rom check mode setting: " + this.strcModelSetting.strRomCheckMode);
            MessageBox.Show("Ram check mode setting: " + this.strcModelSetting.strRamCheckMode);

            //If everything is OK. Then return code
            return "0";
        }

        /// <summary>
        /// For testing only
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMainFCTFuncID9999(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            clsFrmUserSetting clsTest = new clsFrmUserSetting();
            clsTest.InitializeComponent();
            //clsTest.Show();

            //If everything is OK. Then return code
            return clsTest.frmUserSetting;
        }



        //For User Form - Setting form Event Handler
        public void btnOK_Click(object sender, EventArgs e)
        {
            //When user click on OK button. Read all desired setting and send command for all child process to change 
            //setting for Main PCB.
            //MessageBox.Show("I'm inside DLL!");
            //Try to call back on Master Program
            var lstlstobjInput = new List<List<object>>();
            var lstlstobjOutput = new List<List<object>>();
            //Assign new list
            var lstobjInput = new List<object>();
            int i = 0;

            lstobjInput.Add("");
            lstobjInput.Add("-1"); //Request change for all cavity
            for (i = 2; i < 8; i++) lstobjInput.Add(""); //No care some beginning info

            lstobjInput.Add("0"); //8 - Jig ID
            lstobjInput.Add("0"); //9 - Hard ID
            lstobjInput.Add("2"); //10 - Function ID - Setting Function

            lstobjInput.Add(""); //11 - transmit
            lstobjInput.Add(""); //12 - receive

            lstobjInput.Add("-1"); //13
            lstobjInput.Add("1"); //14 -Synchronize option. 1: No waiting
            lstobjInput.Add("10"); //15
            lstobjInput.Add("0"); //16
            lstobjInput.Add("500"); //17

            lstobjInput.Add(this.clsUserSetting.tbModelCode.Text); //18 - model code - para1
            lstobjInput.Add(this.clsUserSetting.tbRomVer.Text); //19 - rom version  - para 2
            lstobjInput.Add(this.clsUserSetting.tbLoaderVer.Text); //20 - loader version 
            lstobjInput.Add(this.clsUserSetting.tbRomCheckMode.Text); //21 - Rom check mode
            lstobjInput.Add(this.clsUserSetting.tbRamCheckMode.Text); //22 - Ram check mode
            lstobjInput.Add(this.clsUserSetting.tbMotorDriver1.Text); //23 - Motor driver 1
            lstobjInput.Add(this.clsUserSetting.tbMotorDriver2.Text); //24 - Motor driver 2

            //Add to list of list output and prepare for calling function
            lstlstobjInput.Add(lstobjInput);

            //Looking for Plugin Common Master in Master parts: 0-0-2 belong to "PluginSystemControl"
            for (i = 0; i < lstPluginMasterCollection.Count; i++)
            {
                if (lstPluginMasterCollection[i].Metadata.IPluginInfo == "PluginSystemControl,0")
                {
                    nspMasterCallBack = lstPluginMasterCollection[i];
                    break;
                }
            }

           object objRet = nspMasterCallBack.Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);

            //Inform to user about setting finish
           MessageBox.Show("New setting done with return result: [" + objRet.ToString() + "]", "Change setting for Main PCB");

        }
        //Constructor
        clsMainFCT()
        {
            //Model setting
            this.strcModelSetting = new ModelSetting();
            this.strcModelSetting.strModelCode = "";
            this.strcModelSetting.strLoaderVersion = "";
            this.strcModelSetting.strMotorDriver1 = "";
            this.strcModelSetting.strMotorDriver2 = "";
            this.strcModelSetting.strRamCheckMode = "";
            this.strcModelSetting.strRomCheckMode = "";
            this.strcModelSetting.strRomVersion = "";
            //FCT Variables
            this.strcFctVar = new FctVar();
            this.strcFctVar.intFixNumber = 0; //Default value is zero
            //User setting form
            this.clsUserSetting = new clsFrmUserSetting();
            this.clsUserSetting.frmUserSetting = new Form();
        }
    } //End Class


    public class clsFrmUserSetting
    {
        public System.Windows.Forms.Form frmUserSetting;

        public System.Windows.Forms.TextBox tbModelCode;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tbRomVer;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox tbLoaderVer;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox tbMotorDriver1;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox tbMotorDriver2;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox tbRomCheckMode;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.TextBox tbRamCheckMode;
        public System.Windows.Forms.Label label7;

        //public bool blFlagButtonPress = false;


        #region FormDesigner

        public void InitializeComponent()
        {
            this.frmUserSetting = new System.Windows.Forms.Form();

            this.tbModelCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbRomVer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLoaderVer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbMotorDriver1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbMotorDriver2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbRomCheckMode = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.tbRamCheckMode = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            //this.SuspendLayout();
            // 
            // tbModelCode
            // 
            this.tbModelCode.Location = new System.Drawing.Point(132, 41);
            this.tbModelCode.Name = "tbModelCode";
            this.tbModelCode.Size = new System.Drawing.Size(135, 20);
            this.tbModelCode.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Model Code";
            // 
            // tbRomVer
            // 
            this.tbRomVer.Location = new System.Drawing.Point(132, 79);
            this.tbRomVer.Name = "tbRomVer";
            this.tbRomVer.Size = new System.Drawing.Size(135, 20);
            this.tbRomVer.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Rom Version";
            // 
            // tbLoaderVer
            // 
            this.tbLoaderVer.Location = new System.Drawing.Point(132, 115);
            this.tbLoaderVer.Name = "tbLoaderVer";
            this.tbLoaderVer.Size = new System.Drawing.Size(135, 20);
            this.tbLoaderVer.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Loader Version";
            // 
            // tbMotorDriver1
            // 
            this.tbMotorDriver1.Location = new System.Drawing.Point(132, 154);
            this.tbMotorDriver1.Name = "tbMotorDriver1";
            this.tbMotorDriver1.Size = new System.Drawing.Size(135, 20);
            this.tbMotorDriver1.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Motor Driver 1";
            // 
            // tbMotorDriver2
            // 
            this.tbMotorDriver2.Location = new System.Drawing.Point(132, 196);
            this.tbMotorDriver2.Name = "tbMotorDriver2";
            this.tbMotorDriver2.Size = new System.Drawing.Size(135, 20);
            this.tbMotorDriver2.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 196);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Motor Driver 2";
            // 
            // tbRomCheckMode
            // 
            this.tbRomCheckMode.Location = new System.Drawing.Point(132, 238);
            this.tbRomCheckMode.Name = "tbRomCheckMode";
            this.tbRomCheckMode.Size = new System.Drawing.Size(135, 20);
            this.tbRomCheckMode.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 238);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Rom Check Mode";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(295, 41);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(201, 58);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            //this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tbRamCheckMode
            // 
            this.tbRamCheckMode.Location = new System.Drawing.Point(132, 276);
            this.tbRamCheckMode.Name = "tbRamCheckMode";
            this.tbRamCheckMode.Size = new System.Drawing.Size(135, 20);
            this.tbRamCheckMode.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 276);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Ram Check Mode";
            // 
            // frmUserSetting
            // 
            this.frmUserSetting.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.frmUserSetting.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.frmUserSetting.ClientSize = new System.Drawing.Size(513, 329);
            this.frmUserSetting.Controls.Add(this.tbRamCheckMode);
            this.frmUserSetting.Controls.Add(this.label7);
            this.frmUserSetting.Controls.Add(this.btnOK);
            this.frmUserSetting.Controls.Add(this.tbRomCheckMode);
            this.frmUserSetting.Controls.Add(this.label6);
            this.frmUserSetting.Controls.Add(this.tbMotorDriver2);
            this.frmUserSetting.Controls.Add(this.label5);
            this.frmUserSetting.Controls.Add(this.tbMotorDriver1);
            this.frmUserSetting.Controls.Add(this.label4);
            this.frmUserSetting.Controls.Add(this.tbLoaderVer);
            this.frmUserSetting.Controls.Add(this.label3);
            this.frmUserSetting.Controls.Add(this.tbRomVer);
            this.frmUserSetting.Controls.Add(this.label2);
            this.frmUserSetting.Controls.Add(this.tbModelCode);
            this.frmUserSetting.Controls.Add(this.label1);
            this.frmUserSetting.Name = "frmUserSetting";
            this.frmUserSetting.Text = "frmUserSetting";
            //this.frmUserSetting.Load += new System.EventHandler(this.frmUserSetting_Load);
            this.frmUserSetting.ResumeLayout(false);
            this.frmUserSetting.PerformLayout();

        }

        #endregion

        //public frmUserSetting()
        //{
        //    InitializeComponent();
        //}

        public void frmUserSetting_Load()
        {
            //MessageBox.Show("Hello!");
            //this.blFlagButtonPress = false;
        }

        public void Show()
        {
            this.frmUserSetting.Show();
        }
    }


} //End namespace
