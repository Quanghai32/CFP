using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Collections;

namespace nspVEChecker
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginVEChecker,11")]
    public class clsVEChecker : nspINTERFACE.IPluginExecute
    {
        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "11,0,0,10,100,101,102"; lstobjInfo.Add(strTemp); //Jig ID,HardwareID,FunctionID
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, HOANG CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 0.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 7/Sep/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, New Extension for VE Checker"; lstobjInfo.Add(strTemp);

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
                case 11:
                    return SelectHardIDFromJigID11(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID11(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return PluginVECheckerFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 10:
                    return PluginVECheckerFuncID10(lstlstobjInput, out lstlstobjOutput);
                case 100:
                    return PluginVECheckerFuncID100(lstlstobjInput, out lstlstobjOutput);
                case 101:
                    return PluginVECheckerFuncID101(lstlstobjInput, out lstlstobjOutput);
                case 102:
                    return PluginVECheckerFuncID102(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        //Global Variables
        public classVEConfig clsVEModelConfig { get; set; }
        public classKMK clsReadExcel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginVECheckerFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();


            return "0";
        }

        /// <summary>
        /// Convert List of AD value to list of actual Voltage with sampling timing adding
        /// + Para1 (13): List of AD value input
        /// + Para2 (14): sampling timing (ms)
        /// + Para3 (15): Calibration Offset value
        /// + Para4 (16): Calibration Gain Value
        /// Output: List of Point(time, Voltage) through UserRet "VE"
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginVECheckerFuncID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();
            //
            List<double> lstdblADVal = new List<double>();
            int i = 0;
            double dblTemp = 0;
            //Check if parameter input is List of value or only 1 value
            if (this.isGenericList(lstlstobjInput[0][13]) == true) //Series of value
            {
                IList lstTemp1 = (IList) lstlstobjInput[0][13];

                for(i=0;i<lstTemp1.Count;i++)
                {
                    if (double.TryParse(lstTemp1[i].ToString(), out dblTemp) == false) return "11-0-10 error: data input number " + (i + 1).ToString() + " [" + lstTemp1[i].ToString() + "] is not numeric";
                    lstdblADVal.Add(dblTemp);
                }
            }
            else //Only one value
            {
                if (double.TryParse(lstlstobjInput[0][13].ToString(), out dblTemp) == false) return "11-0-10 error: data input [" + lstlstobjInput[0][13].ToString() + "] is not numeric";
                lstdblADVal.Add(dblTemp);
            }

            //Check sampling time
            double dblSamplingTime = 0;
            if (double.TryParse(lstlstobjInput[0][14].ToString().Trim(), out dblSamplingTime) == false) return "11-0-10 error: sampling time input [" + lstlstobjInput[0][14].ToString() + "] is not numeric";
            dblSamplingTime = dblSamplingTime / 1000; //Convert to second

            //Check calibration offset value
            double dblOffsetVal = 0;
            if (double.TryParse(lstlstobjInput[0][15].ToString().Trim(), out dblOffsetVal) == false) return "11-0-10 error: Offset value input [" + lstlstobjInput[0][15].ToString() + "] is not numeric";

            //Check Calibration gain value
            double dblGainVal = 0;
            if (double.TryParse(lstlstobjInput[0][16].ToString().Trim(), out dblGainVal) == false) return "11-0-10 error: Gain value input [" + lstlstobjInput[0][16].ToString() + "] is not numeric";

            //if OK, then convert to actual voltage
            List<double> lstdblPressure = new List<double>();
            for (i = 0; i < lstdblADVal.Count;i++)
            {
                double dblPressureTemp = 0;
                //dblVoltTemp = (-100 * (5*lstdblADVal[i]/ (double)1024 - dblOffsetVal) / 5) * dblGainVal; //Note negative pressure sensor resolution -100kPa/5V
                //dblPressureTemp = -20 * (5 * lstdblADVal[i] / (double)1024 - dblOffsetVal) * dblGainVal; //Note negative pressure sensor resolution -100kPa/5V

                //New Cal method
                //Gain = (5/1024)*dblGainVal*Ksensor
                //offset = -dblOffsetVal * dblGainVal * Ksensor

                dblPressureTemp = lstdblADVal[i] * dblGainVal + dblOffsetVal; // y = a*x + b

                lstdblPressure.Add(dblPressureTemp);
            }

            //Create new list of point
            List<object> lstobjVolt = new List<object>();

            lstobjTemp = new List<object>();
            lstobjTemp.Add("TimePressure");

            for (i = 0; i < lstdblPressure.Count; i++)
            {
                double dblTime = i * dblSamplingTime;
                Point pointTemp = new Point(dblTime, lstdblPressure[i]);
                lstobjVolt.Add(pointTemp);

                //
                string strTemp = "";
                strTemp = i.ToString() + "   [" + pointTemp.X.ToString() + " - " + pointTemp.Y.ToString() + "]";
                lstobjTemp.Add(strTemp);
            }
            lstlstobjOutput.Add(lstobjTemp);

            //Return through User Ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("VE");
            lstobjTemp.Add(lstobjVolt);
            lstlstobjOutput.Add(lstobjTemp);

            //negative presure value
            lstobjTemp = new List<object>();
            lstobjTemp.Add("NP"); //Negative Pressure
            for (i = 0; i < lstdblPressure.Count; i++)
            {
                lstobjTemp.Add(lstdblPressure[i]);
            }
            lstlstobjOutput.Add(lstobjTemp);

            //Timing
            lstobjTemp = new List<object>();
            lstobjTemp.Add("Time"); //Negative Pressure
            for (i = 0; i < lstdblPressure.Count; i++)
            {
                double dblTime = i * dblSamplingTime;
                lstobjTemp.Add(dblTime); 
            }
            lstlstobjOutput.Add(lstobjTemp);

            //
            return "0";
        }

        public bool isGenericList(object objInput)
        {
            bool blRet = false;
            if (objInput.GetType().IsGenericType == true)
            {
                if (objInput.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    blRet = true;
                }
            }

            return blRet;
        }

        /// <summary>
        /// WLAN Mac address file reading - Reading file & output to UserRet "WLAN"
        /// + Para1 (13): Name of file - path file should be same folder with exe file
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginVECheckerFuncID100(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Checking if this file exist in same folder 
            string strAppPath = lstlstobjInput[0][0].ToString();
            string strFileName = lstlstobjInput[0][13].ToString();
            string strFilePath = strAppPath + @"\" + strFileName; 

            bool blFound = false;

            if (MyLibrary.ChkExist.CheckFileExist(strFilePath) == true) //exist
            {
                blFound = true;
            }
            else //Not exist
            {
                //Try to find with strFileName is full path input
                if (MyLibrary.ChkExist.CheckFileExist(strFileName) == true) //exist
                 {
                     blFound = true;
                     strFilePath = strFileName;
                 }
            }

            if (blFound == false) return "File not found: [" + strFileName + "]";

            //If file is exist, then we read file content & analyze
            var lines = File.ReadAllLines(strFilePath); //Read all lines in text file

            lstobjTemp.Add("WLAN"); //User Ret key

            //Analyze each line
            int intNumLine = 0;
            foreach(string line in lines)
            {
                intNumLine++;
                //If line is empty, continue to reading
                if (line.Trim() == "") continue; 
                
                //If line is beginning with ' character => command out => discard. Continue to reading
                if (line[0] == '\'') continue;

                //Then, try to analyze further
                string[] strTemp = line.Split(',');

                //Only accept if result content 2 element (Low Range & Hi Range of Address)
                if (strTemp.Length != 2) continue;

                //OK, now remain only 2 element array
                string strLowRange = strTemp[0].Trim();
                string strHiRange = strTemp[1].Trim();

                Int64 int64LowRange = 0;
                Int64 int64HiRange = 0;

                //Check if data is Hexa numeric or not
                if (Int64.TryParse(strLowRange, System.Globalization.NumberStyles.HexNumber, null, out int64LowRange) == false) continue;
                if (Int64.TryParse(strHiRange, System.Globalization.NumberStyles.HexNumber, null, out int64HiRange) == false) continue;

                //Check if data is content exactly 12 characters (MAC Address format)
                string strErrorMessage = "";
                if (strLowRange.Length != 12)
                {
                    //Output Error message
                    strErrorMessage = "Error: Low range of Mac address [" + strLowRange + "] is not 12 character format!";
                    MessageBox.Show(strErrorMessage, "MAC Address format NG");
                    return strErrorMessage;
                }

                if (strHiRange.Length != 12)
                {
                    //Output Error message
                    strErrorMessage = "Error: High range of Mac address [" + strHiRange + "] is not 12 character format!";
                    MessageBox.Show(strErrorMessage, "MAC Address format NG");
                    return strErrorMessage;
                }

                //Check if Hi range is bigger than Lo range
                if(int64HiRange < int64LowRange)
                {
                    strErrorMessage = "Error: High range of Mac address [" + strHiRange + "] is smaller than Low range ["+ strLowRange + "]!";
                    MessageBox.Show(strErrorMessage, "MAC Address format NG");
                    return strErrorMessage;
                }

                //Check if Range is NG Case "000000000000" => "FFFFFFFFFFFF" or not
                if((strLowRange.ToUpper() == "000000000000")&&(strHiRange.ToUpper()=="FFFFFFFFFFFF"))
                {
                    strErrorMessage = "Error: High range of Mac address [" + strHiRange + "] & Low range [" + strLowRange + "] cannot be accepted!";
                    MessageBox.Show(strErrorMessage, "MAC Address format NG");
                    return strErrorMessage;
                }

                //Now everything is OK. We add to result
                lstobjTemp.Add(strLowRange);
                lstobjTemp.Add(strHiRange);
            }

            //Return OK code
            return "0";
        }


        /// <summary>
        /// Reading Destination Information
        /// + Para1 (13): Name of file - path file should be same folder with exe file
        /// + Para2 (14): Name of Sheet contain Destination Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginVECheckerFuncID101(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Checking if this file exist in same folder 
            string strAppPath = lstlstobjInput[0][0].ToString();
            string strFileName = lstlstobjInput[0][13].ToString();
            string strFilePath = strAppPath + @"\" + strFileName;

            bool blFound = false;

            if (MyLibrary.ChkExist.CheckFileExist(strFilePath) == true) //exist
            {
                blFound = true;
            }
            else //Not exist
            {
                //Try to find with assuming that strFileName is full path input
                if (MyLibrary.ChkExist.CheckFileExist(strFileName) == true) //exist
                {
                    blFound = true;
                    strFilePath = strFileName;
                }
            }

            if (blFound == false) return "File not found: [" + strFileName + "]";

            //OK, now excel file is exist, we try to reading destination Info
            int i = 0;
            string strSheetName = lstlstobjInput[0][14].ToString().Trim();
            this.clsVEModelConfig.clsDestination = new nspVEChecker.clsProgramList(strFilePath, strSheetName);

            if (this.clsVEModelConfig.clsDestination.lstclsDestination.Count == 0)
            {
                return "11-0-101 Error: There is no Destination reading data";
            }


            //Export Info to UserRet
            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestVer");
            lstobjTemp.Add(this.clsVEModelConfig.clsDestination.strShimukeVer);
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestIndex");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strIndex.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestOrder");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strOrder.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestDomesticOrAbroad");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strDomesticOrAbroad.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestModel");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strModel.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestA4LTR");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strA4LTR.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestData");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strShimukeData.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestLang");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strLanguage.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();
            lstobjTemp.Add("DestLangData");
            for (i = 0; i < this.clsVEModelConfig.clsDestination.lstclsDestination.Count; i++)
            {
                lstobjTemp.Add(this.clsVEModelConfig.clsDestination.lstclsDestination[i].strLanguageData.Trim());
            }
            lstlstobjOutput.Add(lstobjTemp);

            //Return OK code
            return "0";
        }

        /// <summary>
        /// Read SheetKey Excel File
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginVECheckerFuncID102(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Checking if this file exist in same folder 
            string strAppPath = lstlstobjInput[0][0].ToString();
            string strFileName = lstlstobjInput[0][13].ToString(); //para 1
            string strFilePath = strAppPath + @"\" + strFileName;
            int j;
            int maxCol;
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out maxCol) == false) return "Error: Maximum Column is not numeric!";//para 3

            bool blFound = false;
            if (MyLibrary.ChkExist.CheckFileExist(strFilePath) == true) //exist
            {
                blFound = true;
            }
            else //Not exist
            {
                //Try to find with assuming that strFileName is full path input
                if (MyLibrary.ChkExist.CheckFileExist(strFileName) == true) //exist
                {
                    blFound = true;
                    strFilePath = strFileName;
                }
            }

            if (blFound == false) return "File not found: [" + strFileName + "]";

            //OK, now excel file is exist, we try to reading destination Info
            int i = 0;
            string strSheetName = lstlstobjInput[0][14].ToString().Trim();//para 2
            string strInput = lstlstobjInput[0][16].ToString().Trim(); //para 4
            this.clsReadExcel.ReadExcelFile = new nspKMK.clsProgramList(strFilePath, strSheetName, maxCol);

            if (this.clsReadExcel.ReadExcelFile.lstExcelFile.Count == 0)
            {
                return "11-0-102 Error: There is no Destination reading data";
            }
            //Add Row Data: Row0, Row1, Row2,......../////////////////////////////////////////////////
            for (i = 0; i < this.clsReadExcel.ReadExcelFile.lstExcelFile.Count; i++)
            {
                lstobjTemp = new List<object>();
                lstobjTemp.Add("Row" + Convert.ToString(i));
                for (j = 0; j < maxCol; j++)
                {
                    lstobjTemp.Add(this.clsReadExcel.ReadExcelFile.lstExcelFile[i].Col[j].Trim());
                }
                lstlstobjOutput.Add(lstobjTemp);
            }
            //Add Row Data: Col0, Col1, Col2,......../////////////////////////////////////////////////
            for (j = 0; j < maxCol; j++)
            {
                lstobjTemp = new List<object>();
                lstobjTemp.Add("Col" + Convert.ToString(j));
                for (i = 0; i < this.clsReadExcel.ReadExcelFile.lstExcelFile.Count; i++)
                {
                    lstobjTemp.Add(this.clsReadExcel.ReadExcelFile.lstExcelFile[i].Col[j].Trim());
                }
                lstlstobjOutput.Add(lstobjTemp);
            }
            //Search Data................................/////////////////////////////////////////////
            for (j = 0; j < maxCol; j++)
            {
                lstobjTemp = new List<object>();
                //lstobjTemp.Add("Col" + Convert.ToString(j));
                for (i = 0; i < this.clsReadExcel.ReadExcelFile.lstExcelFile.Count; i++)
                {
                    //lstobjTemp.Add(this.clsReadExcel.ReadExcelFile.lstExcelFile[i].Col[j].Trim());
                    if (strInput == this.clsReadExcel.ReadExcelFile.lstExcelFile[i].Col[j].Trim())
                    {
                        lstobjTemp.Add("Search");
                        lstobjTemp.Add(i);
                        lstobjTemp.Add(j);
                        lstlstobjOutput.Add(lstobjTemp);
                    }
                }

            }
            return "0";
        }

        //Constructor
        public clsVEChecker()
        {
            this.clsVEModelConfig = new classVEConfig();
            this.clsReadExcel = new classKMK();
        }

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class classVEConfig
    {
        //Basic Infomation of Model setting
        public string strModel { get; set; }

        //Destination
        public nspVEChecker.clsProgramList clsDestination { get; set; }
    }

    public class classKMK
    {
        public nspKMK.clsProgramList ReadExcelFile { get; set; }

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
