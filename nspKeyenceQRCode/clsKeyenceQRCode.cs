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
using nspINTERFACE;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualBasic;
using System.Windows.Forms;


namespace nspKeyenceQRCode
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginKeyenceQRCode,202")]
    public class clsKeyenceQRCode : nspINTERFACE.IPluginExecute
    {
        System.IO.Ports.SerialPort COMPort = new System.IO.Ports.SerialPort();
        public string strQRPortName;
        public int strQRBaudRate;
        public int strQRDataBits;
        public string strQRParity;
        public string strQRStopBits;
        public int strQRRecvByteThreshold;

        public string strQRCodeSettingPos = "";

        public Boolean SaveDataFlag;
        public long ReadTimeout = 5000;


        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "202,0,0,1"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.00"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 21/9/2015"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For control Keyence QR Code through RS232 communication!"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }


        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            int intHardID = 0;
            if (int.TryParse(lstlstobjInput[0][9].ToString(), out intHardID) == false) return "Error1";
            intHardID = int.Parse(lstlstobjInput[0][9].ToString());
            switch (intHardID)
            {
                case 0:
                    return SelectFuncIDFromHardID0(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize HardID: " + intHardID.ToString();
            }
            //return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object SelectFuncIDFromHardID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            int intFuncID = 0;
            if (int.TryParse(lstlstobjInput[0][10].ToString(), out intFuncID) == false) return "Error1";
            intFuncID = int.Parse(lstlstobjInput[0][10].ToString());
            switch (intFuncID)
            {
                case 0:
                    return PluginKeyenceQRCodeID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginKeyenceQRCodeID1(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID:" + intFuncID.ToString();
            }

        }

        /// <summary>
        /// Reading SR600 of value from User.ini
        /// para 1 (13) : Name of Setting File
        /// para 2 (14) : Section Name
        /// para 3 : 0- auto increase
        ///          1 - Not Auto increase
        /// para 4 : Name Port
        /// para 5 : Baud Rate
        /// para 6 : DataBits
        /// para 7 : Parity
        /// Para 8 : StopBits
        /// Para 9  (21): Recieve Byte Threshold
        /// Para 10 (22): Setting Position of QRCode reader
        /// </summary>


        public object PluginKeyenceQRCodeID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            string strAppPath = lstlstobjInput[0][0].ToString();
            string strFileName = @"\" + lstlstobjInput[0][13].ToString();
            strFileName = strAppPath + strFileName;
            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                return "Error: Setting file is not exist";
            }
            string strTmp = "";
            string strSectionName = lstlstobjInput[0][14].ToString();
            string strKeyPortName = "";
            string strKeyBaudRate = "";
            string strKeyDataBits = "";
            string strKeyParity = "";
            string strKeyStopBits = "";
            string strKeyRevByteThreshold = "";
            string strQRPos = "";

            //calculate string File name
            int strProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out strProcessID) == false) return "Error: Process ID is not numeric";
            if (lstlstobjInput[0][15].ToString().Trim() == "1")    //not auto
            {
                strKeyPortName = lstlstobjInput[0][16].ToString().Trim();
                strKeyBaudRate = lstlstobjInput[0][17].ToString().Trim();
                strKeyDataBits = lstlstobjInput[0][18].ToString().Trim();
                strKeyParity = lstlstobjInput[0][19].ToString().Trim();
                strKeyStopBits = lstlstobjInput[0][20].ToString().Trim();
                strKeyRevByteThreshold = lstlstobjInput[0][21].ToString().Trim();
                strQRPos = lstlstobjInput[0][22].ToString().Trim();
            }
            else
            {
                strKeyPortName = (lstlstobjInput[0][16].ToString().Trim() + (strProcessID + 1).ToString());
                strKeyBaudRate = (lstlstobjInput[0][17].ToString().Trim() + (strProcessID + 1).ToString());
                strKeyDataBits = (lstlstobjInput[0][18].ToString().Trim() + (strProcessID + 1).ToString());
                strKeyParity = (lstlstobjInput[0][19].ToString().Trim() + (strProcessID + 1).ToString());
                strKeyStopBits = (lstlstobjInput[0][20].ToString().Trim() + (strProcessID + 1).ToString());
                strKeyRevByteThreshold = (lstlstobjInput[0][21].ToString().Trim() + (strProcessID + 1).ToString());
                strQRPos = (lstlstobjInput[0][22].ToString().Trim() + (strProcessID + 1).ToString());
            }

            //reading COM port Name
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyPortName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find " + strKeyPortName + " setting in " + strSectionName;   //getting key value fail
            }
            COMPort.PortName = strTmp.ToUpper();
            //reading BaudRate
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyBaudRate, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'BaudRate' setting in " + strSectionName;   //getting key value fail
            }
            COMPort.BaudRate = Convert.ToInt32(strTmp);

            //reading DataBits
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyDataBits, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'DataBits' setting in " + strSectionName;   //getting key value fail
            }
            COMPort.DataBits = Convert.ToInt32(strTmp);

            //reading Receive Byte Threshold
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyRevByteThreshold, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'RevByteThreshold' setting in " + strSectionName;   //getting key value fail
            }

            COMPort.ReceivedBytesThreshold = Convert.ToInt16(strTmp);

            //reading Parity
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyParity, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'Parity' setting in " + strSectionName;   //getting key value fail
            }
            switch (strTmp.ToUpper())
            {
                case "EVEN":
                    COMPort.Parity = System.IO.Ports.Parity.Even;
                    break;
                case "ODD":
                    COMPort.Parity = System.IO.Ports.Parity.Odd;
                    break;
                case "NONE":
                    COMPort.Parity = System.IO.Ports.Parity.None;
                    break;
                default:
                    COMPort.Parity = System.IO.Ports.Parity.Even;
                    break;
            }

            //reading stopBits

            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyStopBits, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'StopBits' setting in " + strSectionName;   //getting key value fail
            }
            switch (strTmp.ToUpper())
            {
                case "ONE":
                    COMPort.StopBits = System.IO.Ports.StopBits.One;
                    break;
                case "TWO":
                    COMPort.StopBits = System.IO.Ports.StopBits.Two;
                    break;
                case "ONEPOITFIVE":
                    COMPort.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    break;
                case "NONE":
                    COMPort.StopBits = System.IO.Ports.StopBits.None;
                    break;
                default:
                    COMPort.StopBits = System.IO.Ports.StopBits.One;
                    break;
            }

            //reading QR Code Position setting
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strQRPos, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find " + strQRPos + " setting in " + strSectionName;   //getting key value fail
            }
            strQRCodeSettingPos = strTmp;

            //Return 0 if everything is OK
            return "0";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginKeyenceQRCodeID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();
            int i = 0, j = 0, k = 0;
            //int nRet;
            long StartPoint;
            long ReadTime;

            COMPort.Close();
            //IF comport is not open yet, open again
            if (COMPort.IsOpen == false)
            {
                COMPort.Open();
            }

            //Command for QR code start reading
            COMPort.Write("LON" + Environment.NewLine);
            //Mark time start
            StartPoint = MyLibrary.clsApiFunc.GetTickCount();
            string strReadingResult = "";
            bool blReadingSuccess = false;
            //Reading QR code with time out
            while (true)
            {
                //Check result
                Thread.Sleep(100);
                strReadingResult = COMPort.ReadExisting();
                if (strReadingResult != "")
                {
                    blReadingSuccess = true;
                    //MessageBox.Show(strReadingResult);
                    break;
                }
                //Check time out
                ReadTime = MyLibrary.clsApiFunc.GetTickCount() - StartPoint;
                if (ReadTime > ReadTimeout)
                {
                    //MessageBox.Show("Time out");
                    break;
                }
            }


            COMPort.Write("LOFF" + Environment.NewLine);
            COMPort.Close();

            //If time out, then return error message
            if (blReadingSuccess == false) return "1";

            //Calculate QR codes of all position
            //1. Get Info from Frame Program
            bool blTemp = false;

            int intNumChecker = 0;
            blTemp = int.TryParse(lstlstobjInput[2][1].ToString(), out intNumChecker);
            int intNumRow = 0;
            blTemp = int.TryParse(lstlstobjInput[2][2].ToString(), out intNumRow);
            int intNumCol = 0;
            blTemp = int.TryParse(lstlstobjInput[2][3].ToString(), out intNumCol);
            int intAllignMode = 0;
            blTemp = int.TryParse(lstlstobjInput[2][4].ToString(), out intAllignMode);
            int intRoundShapeMode = 0;
            blTemp = int.TryParse(lstlstobjInput[2][5].ToString(), out intRoundShapeMode);
            //2. Calculate layout position
            List<List<int>> lstlstLayout = new List<List<int>>();
            lstlstLayout = ControlLayout(intNumChecker, intNumRow, intNumCol, intAllignMode, intRoundShapeMode);

            //3. Cal Reverse Layout
            List<List<int>> lstlstReverseLayout = new List<List<int>>();

            for (i = 0; i < intNumRow; i++)
            {
                List<int> lstintTemp = new List<int>();
                for (j = 0; j < intNumCol; j++) lstintTemp.Add(0);
                lstlstReverseLayout.Add(lstintTemp);
            }

            for (i = 0; i < intNumRow; i++)
            {
                for (j = 0; j < intNumCol; j++)
                {
                    lstlstReverseLayout[i][j] = lstlstLayout[intNumRow - 1 - i][intNumCol - 1 - j];
                }
            }



            //3. Calculation
            int intX = 0, intY = 0;
            bool blFound = false;
            string strReadingCodePos = strReadingResult.Substring(strReadingResult.Length - 2, 1);
            string strCommonCode = strReadingResult.Substring(0, strReadingResult.Length - 2);

            List<string> lststrResultQRCode = new List<string>();
            for (i = 0; i < intNumChecker; i++)
            {
                if (strReadingCodePos == strQRCodeSettingPos) //Right Direction input
                {
                    lststrResultQRCode.Add(strCommonCode + (i + 1).ToString());
                }
                else //Reverse Direction input
                {
                    //Find Coordinate of item number (i+1)
                    intX = 0; intY = 0; blFound = false;
                    for (j = 0; j < intNumRow; j++)
                    {
                        for (k = 0; k < intNumCol; k++)
                        {
                            if (lstlstLayout[j][k] == (i + 1)) //Matching
                            {
                                intX = j; intY = k;
                                lststrResultQRCode.Add(strCommonCode + lstlstReverseLayout[intX][intY].ToString());
                                blFound = true;
                                break;
                            }
                        }
                        if (blFound == true) break;
                    }
                }
            }

            //UserRet string making
            lstobjOutput.Add("QRcode");
            for (i = 0; i < lststrResultQRCode.Count; i++) lstobjOutput.Add(lststrResultQRCode[i]);

            //Add to return list of list output
            lstlstobjOutput.Add(lstobjOutput);

            //If everything is OK, then return "0"
            return "0";
        }


        //******************************************************************************************
        private List<List<int>> ControlLayout(int intNumChecker, int intRow, int intCol, int intAllignMode, int intRoundShapeMode)
        {
            List<List<int>> lstlstintTest = new List<List<int>>();
            List<int> lstTemp = new List<int>();
            int intTemp = 0;

            //Ini for List of List
            int iRow = 0, iCol = 0;
            for (iRow = 0; iRow < intRow; iRow++)
            {
                lstTemp = new List<int>();
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    intTemp = 0;
                    lstTemp.Add(intTemp);
                }
                lstlstintTest.Add(lstTemp);
            }

            //Depend on each Allign method, assign value for each item 

            //Vertical * Zig-zag
            if ((intAllignMode == 0) && (intRoundShapeMode == 0))
            {
                //Count priority follow column
                //   1 - 5  - 9  - 13
                //   2 - 6  - 10 - 14
                //   3 - 7  - 11 - 15
                //   4 - 8  - 12 - 16
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    for (iRow = 0; iRow < intRow; iRow++)
                    {
                        lstlstintTest[iRow][iCol] = (iRow + 1) + iCol * intRow;

                    }
                }
            }

            //Horizontal * Zig-zag
            if ((intAllignMode == 1) && (intRoundShapeMode == 0))
            {
                //Count priority follow Row
                //   1  - 2  - 3  - 4
                //   5  - 6  - 7  - 8
                //   9  - 10 - 11 - 12
                //   13 - 14 - 15 - 16

                for (iRow = 0; iRow < intRow; iRow++)
                {
                    for (iCol = 0; iCol < intCol; iCol++)
                    {
                        lstlstintTest[iRow][iCol] = (iCol + 1) + iRow * intCol;
                    }
                }
            }

            //Vertical * Rounding
            if ((intAllignMode == 0) && (intRoundShapeMode == 1))
            {
                //Count priority follow column
                //   1 - 8  - 9  - 16
                //   2 - 7  - 10 - 15
                //   3 - 6  - 11 - 14
                //   4 - 5  - 12 - 13
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    for (iRow = 0; iRow < intRow; iRow++)
                    {

                        if ((iCol % 2) == 0) //Odd Col 1-3...
                        {
                            lstlstintTest[iRow][iCol] = (iRow + 1) + iCol * intRow;
                        }
                        else //Even Col 2-4...
                        {
                            lstlstintTest[iRow][iCol] = intRow - iRow + iCol * intRow;
                        }

                    }
                }

            }

            //Horizontal * Rounding
            if ((intAllignMode == 1) && (intRoundShapeMode == 1))
            {
                //Count priority follow Row
                //   1  - 2   - 3  - 4
                //   8  - 7   - 6  - 5
                //   9  - 10  - 11 - 12
                //   16 - 15  - 14 - 13
                for (iRow = 0; iRow < intRow; iRow++)
                {
                    for (iCol = 0; iCol < intCol; iCol++)
                    {

                        if ((iRow % 2) == 0) //Odd Row 1-3...
                        {
                            lstlstintTest[iRow][iCol] = (iCol + 1) + iRow * intCol;
                        }
                        else //Even Row 2-4...
                        {
                            lstlstintTest[iRow][iCol] = intCol - iCol + iRow * intCol;
                        }

                    }
                }
            }

            //Return result
            return lstlstintTest;

        }








    }
}






