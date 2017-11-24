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
using clsGeneralUsbHid;
using nspINTERFACE;
using nspMcrInterface;
using System.Windows.Forms;

namespace nspMicroUSB
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginMicroUSB,200")]
    [Export(typeof(nspMcrInterface.IMcrUsb))]
    [ExportMetadata("IPluginMcrInfo", "nspMicroUSB")]
    public class clsMcrUSB : nspINTERFACE.IPluginExecute, nspMcrInterface.IMcrUsb
    {
        //public LibraryVbNetHoang.ClsMcrUsb clsMicroUSB { get; set; }

        public clsGeneralUsbHid.clsGeneralUsbHid clsMicroUSB { get; set; }


        public string strMcrPipeName { get; set; }
        string strClassID { get; set; }
        bool isInitialized { get; set; }


        public void SetPipeName(string strPipe)
        {
            this.strMcrPipeName = strPipe;
        }
        public int McrUSB_WR(string strUsbCommand, ref byte[] byteMcrReceiveData, int intTimeOut)
        {
            //MessageBox.Show("Hehe");
            int iret = 0;
            iret = clsMicroUSB.McrUSB_WR(strUsbCommand, this.strMcrPipeName, ref byteMcrReceiveData, intTimeOut);
            return iret;
        }


        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";
            //Inform to Host program which Function this plugin support
            strTemp = "200,0,0,2,3,4,10,14"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 20/Apr/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add Median Filter to Function ID2"; lstobjInfo.Add(strTemp);


            lstlstobjInfo.Add(lstobjInfo);

            isInitialized = false;
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            if (this.isInitialized == false)
            {
                this.strClassID = lstlstobjInput[0][1].ToString();
                this.isInitialized = true;
            }

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
                case 200:
                    return SelectHardIDFromJigID200(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID200(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return PluginMicroUSBFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginMicroUSBFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginMicroUSBFuncID3(lstlstobjInput, out lstlstobjOutput);
                case 4:
                    return PluginMicroUSBFuncID4(lstlstobjInput, out lstlstobjOutput);
                case 10:
                    return PluginMicroUSBFuncID10(lstlstobjInput, out lstlstobjOutput);
                case 14:
                    return PluginMicroUSBFuncID14(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        /// <summary>
        /// PluginMicroUSBFuncID0(): Micro-controller HID initialization
        ///     + Get pipe name for each Micro-controller in system
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Ini for Micro USB class
            this.clsMicroUSB = new clsGeneralUsbHid.clsGeneralUsbHid();

            //Assisgn pipe name for Micro USB class
            this.strMcrPipeName = "";

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

            if (lstlstobjInput[0][16].ToString().ToUpper() == "1") //No Auto
            {
                strKeyName = lstlstobjInput[0][15].ToString();
            }
            else //Auto - Default
            {
                strKeyName = lstlstobjInput[0][15].ToString() + (intProcessID + 1).ToString();
            }


            //Reading value in key name
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                //MessageBox.Show("Error: cannot find '" + lstPara[2] + "' config in '" + lstPara[1] + "' of " + strFileName + " file!", "CommonFunctionID002() error");
                return "error"; //Getting key value fail
            }

            //Assign pipe name from ini file
            this.strMcrPipeName = strTmp;
            //MessageBox.Show(strTmp,"Process ID " + lststrInput[1]);


            //Out class Mcr through User Ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Mcr");
            lstobjTemp.Add(this.clsMicroUSB);
            lstlstobjOutput.Add(lstobjTemp);

            //
            return "0"; //if everything ok, return OK code "0"
        }
        /// <summary>
        /// Checking voltage
        ///     + Para1 (13) : USB command to send
        ///     + Para2 (14) : Time out waitting until measured value in the spec
        ///     + Para3 (15) : High byte position
        ///     + Para4 (16) : Low byte position
        ///     + Para5 (17) : Voltage reference
        ///     + Para6 (18) : Resolution
        ///     + Para7 (19) : Convert factor
        ///     + Para8 (20) : Rounding with how many digit (Default is 2)
        ///     + Para9 (21) : Optional times to get Median Filter Value (if not fill or fill 0 => Default is not get Median Filter)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            int i = 0;

            double dblLoLimit = 0; //5th
            double dblHiLimit = 0; //6th

            double dblConvertFactor = 0;
            int intTimeOut = 0;
            int intHiBytePos = 0;
            int intLoBytePos = 0;
            double dblVoltRef = 0;
            int intResolution = 0;
            int intRoundDigit = 0;
            int intMedianFilterTimes = 0;

            //Check numeric or not
            if (double.TryParse(lstlstobjInput[0][5].ToString(), out dblLoLimit) == false) return "Error: Low limit is not numeric!";
            if (double.TryParse(lstlstobjInput[0][6].ToString(), out dblHiLimit) == false) return "Error: Hi limit is not numeric!";

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intTimeOut) == false) return "Error: Timeout setting is not integer!";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intHiBytePos) == false) return "Error: High byte position is not integer!";
            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intLoBytePos) == false) return "Error: Low byte position is not integer!";
            if (double.TryParse(lstlstobjInput[0][17].ToString(), out dblVoltRef) == false) return "Error: Voltage reference is not numeric!";
            if (int.TryParse(lstlstobjInput[0][18].ToString(), out intResolution) == false) return "Error: Resolution setting is not integer!";
            if (double.TryParse(lstlstobjInput[0][19].ToString(), out dblConvertFactor) == false) return "Error: Convert factor is not numeric!";
            if (int.TryParse(lstlstobjInput[0][20].ToString(), out intRoundDigit) == false)
            {
                intRoundDigit = 2; //Default setting
            }

            if (int.TryParse(lstlstobjInput[0][21].ToString(), out intMedianFilterTimes) == false)
            {
                intMedianFilterTimes = 0; //Default Value is no get Median Filter
            }

            //Sending usb command
            int iret = 0;
            byte[] byteArrMcrUsbReceive = new byte[1];
            //intTimeOut = 1000;
            double dblResult = 0;

            //Mark time to start
            int intStartTime = MyLibrary.ApiDeclaration.GetTickCount();
            int intCurrentTime = 0;
            bool blFlagTimeOut = false;
            List<double> lstdblMedianVal = new List<double>();

            //Polling until reading value in spec
            do
            {
                intCurrentTime = MyLibrary.ApiDeclaration.GetTickCount();
                if ((intCurrentTime - intStartTime) > intTimeOut) blFlagTimeOut = true;

                if (blFlagTimeOut==false)
                {
                    //Sending command
                    iret = this.clsMicroUSB.McrUSB_WR(lstlstobjInput[0][13].ToString(), this.strMcrPipeName, ref byteArrMcrUsbReceive, intTimeOut);
                    //If sending is OK. Then calculate result
                    if (iret == 0)
                    {
                        dblResult = (256 * Convert.ToDouble(byteArrMcrUsbReceive[intHiBytePos]) + Convert.ToDouble(byteArrMcrUsbReceive[intLoBytePos])) * dblConvertFactor * dblVoltRef / Convert.ToDouble(intResolution);
                        dblResult = Math.Round(dblResult, intRoundDigit);
                    }
                    else
                    {
                        dblResult = dblHiLimit + 1000; //Force out of spec if usb command sending fail
                    }
                }
                else //Time out
                {
                    return "Error: Time out! iret = [" + iret.ToString() + "]. Last value = [" + dblResult.ToString() + "]";
                } 
            }
            while (((dblResult < dblLoLimit) || (dblResult > dblHiLimit)) && (blFlagTimeOut == false));

            if (blFlagTimeOut == true) return "Error: Time out! Measured value = " + dblResult.ToString();



            if (intMedianFilterTimes == 0) //No use Median Filter option, just polling & get return value
            {
                //If OK, then return value
                return dblResult.ToString();

            }
            else //Sending continuously to Micro & get return value - then calculate Median Filter value
            {
                //If OK, then get Median filter
                for (i = 0; i < intMedianFilterTimes; i++)
                {
                    double dblTemp = 0;
                    //Sending command
                    iret = this.clsMicroUSB.McrUSB_WR(lstlstobjInput[0][13].ToString(), this.strMcrPipeName, ref byteArrMcrUsbReceive, intTimeOut);
                    //If sending is OK. Then calculate result
                    if (iret == 0)
                    {
                        dblTemp = (256 * Convert.ToDouble(byteArrMcrUsbReceive[intHiBytePos]) + Convert.ToDouble(byteArrMcrUsbReceive[intLoBytePos])) * dblConvertFactor * dblVoltRef / Convert.ToDouble(intResolution);
                        dblTemp = Math.Round(dblTemp, intRoundDigit);
                        lstdblMedianVal.Add(dblTemp);
                    }
                }

                //Do Median Filter calculation
                if (lstdblMedianVal.Count == 0) return "Error: Cannot get any data for Median Filter"; //Cannot get any data

                return MedianFilterCal(lstdblMedianVal);
            }

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
        /// Select Pipe Name for Micro-controller
        ///     + Para1 (13): Value to assign for usb pipe name
        /// Return "0" if everything is OK.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            this.strMcrPipeName = lstlstobjInput[0][13].ToString();

            return "0";
        }

        /// <summary>
        /// Polling return byte from Micro USB Command
        ///     + Para1 (13): Micro USB command
        ///     + Para4 (14): Timeout setting.
        ///     + Para5 (15): Byte position want to wait (compare with lo & Hi limit). 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID4(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intTimeOut) == false) return "Error: Timeout setting is NG";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intBytePos) == false) return "Error: Byte Position setting is NG";


            int iret = 0;
            byte[] byteArrMcrUsbReceive = new byte[1];
            //intTimeOut = 1000;
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
                iret = this.clsMicroUSB.McrUSB_WR(lstlstobjInput[0][13].ToString(), this.strMcrPipeName, ref byteArrMcrUsbReceive, intTimeOut);

                //check return value
                if (iret == 0)
                {
                    //If sending is OK, then calculate value
                    dblResult = byteArrMcrUsbReceive[intBytePos];
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
        /// Auto find pipe name for micro-controller
        ///     + Para1 (13): Time out (ms)
        ///     + Para2 (14): Vendor ID (Hexa format)
        ///     + Para3 (15): Product ID (Hexa format)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();
            
            //
            int intTimeOutMs = 0;
            if(int.TryParse(lstlstobjInput[0][13].ToString().Trim(), out intTimeOutMs)==false)
            {
                return "Error: Time Out input [" + lstlstobjInput[0][13].ToString() + "] is not integer!";
            }

            string strVID = lstlstobjInput[0][14].ToString().Trim().ToUpper();
            string strPID = lstlstobjInput[0][15].ToString().Trim().ToUpper();




            //List<string> lststrDevicePath = this.clsMicroUSB.AutoFindPipe(strVID, strPID);

            bool blExit = false;

            int intStartTime = MyLibrary.clsApiFunc.GetTickCount();

            do
            {
                List<string> lststrDevicePath = this.clsMicroUSB.AutoFindPipe(strVID, strPID);

                if (lststrDevicePath.Count == 1) //Finding OK
                {
                    //Assign pipe name & exit
                    this.clsMicroUSB.strPipeName = lststrDevicePath[0];
                    break;
                }
                else if (lststrDevicePath.Count > 1) //Error: there is more than 1 device connected to PC
                {
                    return "Error: There are more than 1 pipe found!"; //Error code
                }

                //
                if ((MyLibrary.clsApiFunc.GetTickCount() - intStartTime) > intTimeOutMs)
                {
                    return "Error: Time out! Cannot find any pipe."; //Error code
                }

            } while (blExit == false);



            //Saving pipe name value
            lstobjTemp = new List<object>();
            lstobjTemp.Add("pipe");
            lstobjTemp.Add(this.clsMicroUSB.strPipeName);
            lstlstobjOutput.Add(lstobjTemp);

            //Return value - 0 if everything is OK
            return "0";
        }

        /// <summary>
        /// McrUSB_WR() Function with Parameter take from "Transmission area"
        /// Para1: time out
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMicroUSBFuncID14(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();
            byte[] byteArrMcrUsbReceive = new byte[1];

            int iret = 0;
            int intTimeOut = 0;

            //Analyze retry times
            if (Int32.TryParse(lstlstobjInput[0][13].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = Int32.Parse(lstlstobjInput[0][13].ToString());
            }
            else //illegal parameter input - set to default is 1000
            {
                intTimeOut = 1000;
            }

            //Then, we sending usb command and getting data return - With command Take from TRANSMISSION AREA
            iret = this.clsMicroUSB.McrUSB_WR(lstlstobjInput[0][11].ToString(), this.strMcrPipeName, ref byteArrMcrUsbReceive, intTimeOut);

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

            //
            //Out class Mcr through User Ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Mcr");
            lstobjTemp.Add(this.clsMicroUSB);
            lstlstobjOutput.Add(lstobjTemp);

            //Return value
            return iret.ToString();
        }


        //Constructor
        clsMcrUSB()
        {
            this.clsMicroUSB = new clsGeneralUsbHid.clsGeneralUsbHid();
            this.strMcrPipeName = "";
        }
    }
}
