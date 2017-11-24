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
//                  - lstlststrInput[1]: Other information from Host program:
//                      "InfoCode - Info1 - Info2 - Info3 - ..."
//              + lststrOutput: output information from plug-in (under List of list of string format)
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Timers;
using nspINTERFACE;

namespace nspPluginCommonTest
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginCommonTest,1")]
    public class clsCommonTest : nspINTERFACE.IPluginExecute
    {
        private System.Timers.Timer tmrWaitingTimer = new System.Timers.Timer();
        private bool blFlagTimerIni = false;
        private bool blFlagTimerOverFlow = false;

        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";
            //Inform to Host program which Function this plugin support
            strTemp = "1,0,0,1,2,3,4,5,6,7,20,9999"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Thuan CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 21/Apr/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add Func 20"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {

            lstlstobjOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Application startup path - Process ID - ... - JigID-HardID-FuncID"

            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());
            switch (intJigID) //Select JigID
            {
                case 1:
                    return SelectHardIDFromJigID1(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return "Unrecognize HardID: " + intHardID.ToString();
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
                    return PluginCommonTestFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginCommonTestFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginCommonTestFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginCommonTestFuncID3(lstlstobjInput, out lstlstobjOutput);
                case 4:
                    return PluginCommonTestFuncID4(lstlstobjInput, out lstlstobjOutput);
                case 5:
                    return PluginCommonTestFuncID5(lstlstobjInput, out lstlstobjOutput);
                case 6:
                    return PluginCommonTestFuncID6(lstlstobjInput, out lstlstobjOutput);
                case 7:
                    return PluginCommonTestFuncID7(lstlstobjInput, out lstlstobjOutput);
                case 20:
                    return PluginCommonTestFuncID20(lstlstobjInput, out lstlstobjOutput);
                case 9999:
                    return PluginCommonTestFuncID9999(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        /// <summary>
        /// PluginCommonTestFuncID0(): This function will return what is receive from Para1
        ///     +Para1 (13): Data to return
        ///     +Para2 (14): Optional to out message box - 1: output. 0(default): No output.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<string>();

            int intOption = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intOption) == false) intOption = 0;

            if (intOption == 1) MessageBox.Show(lstlstobjInput[0][13].ToString());


            return lstlstobjInput[0][13]; //13 is the order of Parameter number 1
        }

        /// <summary>
        /// Delay function (ms)
        ///     + Para1: Number of ms want to delay
        ///     + Para2: Optional to Waiting by Timer (Return: 1  if not finish. 0 if Finish waiting).
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<string>();

            int intTimeDelay = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intTimeDelay) == false)
            {
                return "Error: Number of millisecond want to delay is not integer format!";
            }

            int intOptionTimer = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intOptionTimer) == false) intOptionTimer = 0;

            if (intOptionTimer == 1) //Waiting by Timer
            {
                if (blFlagTimerIni == false)
                {
                    blFlagTimerOverFlow = false;
                    blFlagTimerIni = true;
                    //Assign Elapsing value for Timer
                    tmrWaitingTimer = new System.Timers.Timer(intTimeDelay);
                    //Hook up the Elapsed event for the timer.
                    tmrWaitingTimer.Elapsed += OnTimedEvent;
                    //Set timer interval
                    tmrWaitingTimer.Interval = intTimeDelay;
                    tmrWaitingTimer.Enabled = true;
                }

                if (blFlagTimerOverFlow == true) //Waiting completed!
                {
                    //reset some flag
                    blFlagTimerOverFlow = false;
                    blFlagTimerIni = false;
                    //return code

                    return "0"; //Successful code
                }
                else //Waiting not yet completed...
                {
                    return "1"; //Successful code
                }
            }
            else
            {
                System.Threading.Thread.Sleep(intTimeDelay);
                return "0"; //Successful code
            }

        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            tmrWaitingTimer.Enabled = false;
            blFlagTimerOverFlow = true;
            //tmrWaitingTimer.Enabled = true;
        }



        /// <summary>
        /// For Reading a value in setting file and return
        ///     +Para1 (13): File name
        ///     +Para2 (14): Section name
        ///     +Para3 (15): Key name
        ///     +Para4 (16): optional to add auto index: 0 - Add. 1 - No Add.
        ///     +Para5 (17): optional to return: 0 - Return "0" if everything is OK (with user return string)
        ///                                      1 - Convert to Numeric (Double format) and return under string format (with user return string)
        /// User Return command: "ReadIni"
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            nspFileHandle.ChkExist clsTest = new nspFileHandle.ChkExist();
            if (clsTest.CheckFileExist(strFileName) == false)
            {
                //MessageBox.Show("'" + lstPara[0] + "' file does not exist! Please check!", "CommonFunctionID002() error");
                return "Error: File is not exist"; //File not exist code
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
            strTmp = nspFileHandle.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                //MessageBox.Show("Error: cannot find '" + lstPara[2] + "' config in '" + lstPara[1] + "' of " + strFileName + " file!", "CommonFunctionID002() error");
                return "Error: fail to read file"; //Getting key value fail
            }

            //If everything is OK, the create a User Return structure
            lstobjTemp = new List<object>();
            lstobjTemp.Add("ReadIni");
            lstobjTemp.Add(strTmp);
            lstlstobjOutput.Add(lstobjTemp);

            //Decide what to return
            string strOptReturn = "";
            strOptReturn = lstlstobjInput[0][17].ToString().Trim().ToUpper();
            if (strOptReturn == "1") //Convert to numeric and return
            {
                double dblRet;
                if (double.TryParse(strTmp, out dblRet) == false) return "Error: cannot convert " + strTmp + "to numeric.";
                return strTmp; //If OK, then return result
            }

            return "0"; //if everything ok, return OK code "0"
        }

        /// <summary>
        /// Message box function
        ///     +Para1 (13): Content of message box
        ///     +Para2 (14): Caption of message box
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            MessageBox.Show(lstlstobjInput[0][13].ToString(), lstlstobjInput[0][14].ToString());

            return "0";
        }

        /// <summary>
        /// Writing a value to setting file which is same location with EXE
        ///     +Para1 (13): File name want to write 
        ///     +Para2 (14): Section name want to write
        ///     +Para3 (15): keyname want to write
        ///     +Para4 (16): Option to get auto index (0: add, 1: No add)
        ///     +Para5 (17): Value to write
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID4(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            nspFileHandle.ChkExist clsTest = new nspFileHandle.ChkExist();
            if (clsTest.CheckFileExist(strFileName) == false)
            {
                //MessageBox.Show("'" + lstPara[0] + "' file does not exist! Please check!", "CommonFunctionID002() error");
                return "Error: File is not exist"; //File not exist code
            }


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


            //Write to ini file the value desired
            long lngTemp = 0;
            lngTemp = nspFileHandle.WriteFiles.IniWriteValue(strFileName, lstlstobjInput[0][14].ToString(), strKeyName, lstlstobjInput[0][17].ToString());

            if (lngTemp == 9999) //Error
            {
                return "Error: Cannot write to ini file!";
            }

            //If everything is OK, return 0
            return "0";
        }

        /// <summary>
        /// Writing a value to setting file which is different location with EXE
        ///     +Para1 (13): File name in same location with exe file, which show where file want to write info to 
        ///     +Para2 (14): Section name (file which contents info)
        ///     +Para3 (15): keyname (file which contents info)
        ///     +Para4 (16): Section Name of target file
        ///     +Para5 (17): Key Name of target file
        ///     +Para6 (18): Option to get auto index (0: add, 1: No add)
        ///     +Para7 (19): Value to write
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID5(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            //Check file exist
            nspFileHandle.ChkExist clsTest = new nspFileHandle.ChkExist();
            if (clsTest.CheckFileExist(strFileName) == false)
            {
                //MessageBox.Show("'" + lstPara[0] + "' file does not exist! Please check!", "CommonFunctionID002() error");
                return "Error: Info File is not exist"; //File not exist code
            }

            string strTmp = "";
            //Reading value in key name
            strTmp = nspFileHandle.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), lstlstobjInput[0][15].ToString(), strFileName);
            if (strTmp.ToLower() == "error")
            {
                //MessageBox.Show("Error: cannot find '" + lstPara[2] + "' config in '" + lstPara[1] + "' of " + strFileName + " file!", "CommonFunctionID002() error");
                return "Error: fail to read file"; //Getting key value fail
            }

            //Assign new value for Target file
            strFileName = strTmp;
            //Check file exist or not
            if (clsTest.CheckFileExist(strFileName) == false)
            {
                return "Error: Target File is not exist"; //File not exist code
            }

            //Calculate key name
            string strKeyName = "";
            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "error";

            if (lstlstobjInput[0][18].ToString().ToUpper() == "1") //No Auto
            {
                strKeyName = lstlstobjInput[0][17].ToString();
            }
            else //Auto - Default
            {
                strKeyName = lstlstobjInput[0][17].ToString() + (intProcessID + 1).ToString();
            }

            //Write to ini file the value desired
            long lngTemp = 0;
            lngTemp = nspFileHandle.WriteFiles.IniWriteValue(strFileName, lstlstobjInput[0][16].ToString(), strKeyName, lstlstobjInput[0][19].ToString());

            if (lngTemp == 9999) //Error
            {
                return "Error: Cannot write to target file!";
            }

            //If everything is OK, return 0
            return "0";
        }


        /// <summary>
        /// Get system time to send MainPCB
        ///     + Convert Year-2000 to 2 byte
        ///     + Convert Month, Day, Hour, Minute, Second to 2 byte
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID6(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {

            /////////THUAN add more///////////
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            lstobjTemp.Add("S");
            string NewYear;
            NewYear = System.DateTime.Now.Year.ToString();
            Int32 year;
            string year1;
            string year2;
            year = Convert.ToInt32(NewYear) - 2000;
            //int Int1 = Convert.ToInt16(Year);
            if (year >= 10)
            {
                year1 = Convert.ToString(year / 10);
                year2 = Convert.ToString(year % 10);

            }
            else
            {
                year1 = "00";
                year2 = Convert.ToString(year);
            }


            string Newmonth;
            Int32 month;
            string month1;
            string month2;

            Newmonth = System.DateTime.Now.Month.ToString();
            month = Convert.ToInt32(Newmonth);
            if (month >= 10)
            {
                month1 = Convert.ToString(month / 10);
                month2 = Convert.ToString(month % 10);
            }
            else
            {
                month1 = "00";
                month2 = Convert.ToString(month);
            }

            string NewDay;
            Int32 day;
            string day1;
            string day2;

            NewDay = System.DateTime.Now.Day.ToString();
            day = Convert.ToInt32(NewDay);

            if (day >= 10)
            {
                day1 = Convert.ToString(day / 10);
                day2 = Convert.ToString(day % 10);
            }
            else
            {
                day1 = "00";
                day2 = Convert.ToString(day);
            }

            string NewHour;
            Int32 hour;
            string hour1;
            string hour2;

            NewHour = System.DateTime.Now.Hour.ToString();
            hour = Convert.ToInt32(NewHour);
            if (hour >= 10)
            {
                hour1 = Convert.ToString(hour / 10);
                hour2 = Convert.ToString(hour % 10);
            }
            else
            {
                hour1 = "00";
                hour2 = Convert.ToString(hour);
            }

            string NewMin;
            Int32 min;
            string min1;
            string min2;

            NewMin = System.DateTime.Now.Minute.ToString();
            min = Convert.ToInt32(NewMin);

            if (min >= 10)
            {
                min1 = Convert.ToString(min / 10);
                min2 = Convert.ToString(min % 10);
            }
            else
            {
                min1 = "00";
                min2 = Convert.ToString(min);
            }

            string NewSec;
            Int32 sec;
            string sec1;
            string sec2;

            NewSec = System.DateTime.Now.Second.ToString();
            sec = Convert.ToInt32(NewSec);
            if (sec >= 10)
            {
                sec1 = Convert.ToString(sec / 10);
                sec2 = Convert.ToString(sec % 10);
            }
            else
            {
                sec1 = "00";
                sec2 = Convert.ToString(sec);
            }

            Int64 total;
            string sectotal;
            total = Convert.ToInt64(DateTime.Now.Ticks.ToString()) / 10000000;
            sectotal = Convert.ToString(total);

            //return value: year1,year2,month1,month2,day1,day2,hour1,hour2,min1,min2,sec1,sec2
            lstobjTemp.Add(sectotal);
            lstobjTemp.Add(year1);
            lstobjTemp.Add(year2);
            lstobjTemp.Add(month1);
            lstobjTemp.Add(month2);
            lstobjTemp.Add(day1);
            lstobjTemp.Add(day2);
            lstobjTemp.Add(hour1);
            lstobjTemp.Add(hour2);
            lstobjTemp.Add(min1);
            lstobjTemp.Add(min2);
            lstobjTemp.Add(sec1);
            lstobjTemp.Add(sec2);
            lstlstobjOutput.Add(lstobjTemp);

            //If everything is OK, return 0
            return "0";
        }

        /// <summary>
        /// Calculate Timing (Second) from input data
        ///     + Para1 (13): Input Year 
        ///     + Para2 (14): Input Month
        ///     + Para3 (15): Input Day
        ///     + Para4 (16): Input Hour
        ///     + Para5 (17): Input Minute
        ///     + Para6 (18): Input Second
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID7(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            lstobjTemp.Add("CalTiming");

            int intyear, intmonth, intday, inthour, intmin, intsec;
            Int64 total;

            intyear = int.Parse(lstlstobjInput[0][13].ToString());
            intmonth = int.Parse(lstlstobjInput[0][14].ToString());
            intday = int.Parse(lstlstobjInput[0][15].ToString());
            inthour = int.Parse(lstlstobjInput[0][16].ToString());
            intmin = int.Parse(lstlstobjInput[0][17].ToString());
            intsec = int.Parse(lstlstobjInput[0][18].ToString());
            //Check Input Year
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intyear) == false) return "Error: Input Year is not numeric!";
            if (intyear < 0) return "Error: Input Year was fail";

            //Check Input Month
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intmonth) == true)
            {
                if ((intmonth > 12) || (intmonth < 1)) return "Error: Input Month was fail";
            }
            else
            {
                return "Error: Input Month is not numeric!";
            }
            //Check Input Day
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intday) == true)
            {
                if ((intday > 31) || (intday < 1)) return "Error: Input Day was fail";
            }
            else
            {
                return "Error: Input Day is not numeric!";
            }
            //Check Input Hour
            if (int.TryParse(lstlstobjInput[0][16].ToString(), out inthour) == true)
            {
                if ((inthour > 60) || (inthour < 0)) return "Error: Input Hour was fail";
            }
            else
            {
                return "Error: Input Hour is not numeric!";
            }
            //Check Input Minute
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intmin) == true)
            {
                if ((intmin > 60) || (intmin < 0)) return "Error: Input Minute was fail";
            }
            else
            {
                return "Error: Input Minute is not numeric!";
            }
            //Check Input Second
            if (int.TryParse(lstlstobjInput[0][18].ToString(), out intsec) == true)
            {
                if ((intsec > 60) || (intsec < 0)) return "Error: Input Second was fail";
            }
            else
            {
                return "Error: Input Second is not numeric!";
            }


            DateTime s;
            try
            {
                s = new DateTime(intyear, intmonth, intday, inthour, intmin, intsec);
            }
            catch (Exception ex)
            {
                return "Error in Function ID 1-0-7. Error Message: " + ex.Message;
            }

            total = s.Ticks / 10000000;

            lstobjTemp.Add(total.ToString());
            lstobjTemp.Add(intyear.ToString());
            lstobjTemp.Add(intmonth.ToString());
            lstobjTemp.Add(intday.ToString());
            lstobjTemp.Add(inthour.ToString());
            lstobjTemp.Add(intmin.ToString());
            lstobjTemp.Add(intsec.ToString());

            lstlstobjOutput.Add(lstobjTemp);

            //If everything is OK, return 0
            return "0";
        }


        /// <summary>
        /// Add all string from all parameter to form new string if string in parameter is empty, then no add
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID20(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstObjTemp = new List<object>();

            string strResult = "";
            string separator = lstlstobjInput[0][13].ToString();

            int i = 0;
            for (i = 1; i < 20; i++)
            {
                //if (lstlstobjInput[0][13 + i].ToString() == "") continue;

                if (i == 19) separator = ""; //the last one=> no add parameter

                strResult = strResult + lstlstobjInput[0][13 + i].ToString() + separator;
            }

            lstObjTemp.Add("str");
            lstObjTemp.Add(strResult);

            lstlstobjOutput.Add(lstObjTemp);

            return "0";
        }

        /// <summary>
        /// This function is for testing only
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginCommonTestFuncID9999(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstObjTemp = new List<object>();

            //Create sample windows form
            System.Windows.Forms.Form frmTest = new Form();
            frmTest.Text = "This is sample Windows Form!";
            //frmTest.Show();
            return frmTest;
        }

    }

}
