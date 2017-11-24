using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace nspNECWireless
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginNECWireless,204")]
    public class clsNECWireless: nspINTERFACE.IPluginExecute
    {
        //Form setting
        public clsWirelessCom clsNECWirelessControl{get;set;}

        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";
            //Inform to Host program which Function this plugin support
            strTemp = "204,0,0,1,2,3,4,14,1000"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Trung B"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 0.00"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 05-05-2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For control communicate with DMH"; lstobjInfo.Add(strTemp);

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
        #endregion
        
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
                    return PluginWirelessModule0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginWirelessModule1(lstlstobjInput, out lstlstobjOutput);  
                case 2:
                    return PluginWirelessModule2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginWirelessModule3(lstlstobjInput, out lstlstobjOutput);
                case 4:
                    return PluginWirelessModule4(lstlstobjInput, out lstlstobjOutput);
                case 14:
                    return PluginWirelessModule14(lstlstobjInput, out lstlstobjOutput);  
                case 1000:
                    return PluginWirelessModule1000(lstlstobjInput, out lstlstobjOutput); 
                
                default:
                    return "Unrecognize FuncID:" + intFuncID.ToString();
            }
        }

        /// <summary>
        /// Ini for class Wireless control & Reading Comport setting for Receiver unit from User.ini
        ///     + para 1 (13) : Name of Setting File
        ///     + para 2 (14) : Section Name
        ///     + para 3 (15): Name Port
        ///     + para 4 (16): Baud Rate
        ///     + para 5 (17): DataBits
        ///     + para 6 (18): Parity
        ///     + Para 7 (19): StopBits
        ///     + Para 8 (20): Module Serial - Default value
        /// </summary>

        public object PluginWirelessModule0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
            string strKeyModuleID = "";

            //Ini for class
            if(this.clsNECWirelessControl == null)
            {
                this.clsNECWirelessControl = new clsWirelessCom();
            }
            

            //calculate string File name
            int strProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out strProcessID) == false) return "Error: Process ID is not numeric";

            strKeyPortName = lstlstobjInput[0][15].ToString().Trim();
            strKeyBaudRate = lstlstobjInput[0][16].ToString().Trim();
            strKeyDataBits = lstlstobjInput[0][17].ToString().Trim();
            strKeyParity = lstlstobjInput[0][18].ToString().Trim();
            strKeyStopBits = lstlstobjInput[0][19].ToString().Trim();
            strKeyModuleID = lstlstobjInput[0][20].ToString().Trim();
          
            //reading COM port Name
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyPortName, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find " + strKeyPortName + " setting in " + strSectionName;   //getting key value fail
            }
            this.clsNECWirelessControl.COMPort.PortName = strTmp.ToUpper();
            //reading BaudRate
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyBaudRate, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'BaudRate' setting in " + strSectionName;   //getting key value fail
            }
            this.clsNECWirelessControl.COMPort.BaudRate = Convert.ToInt32(strTmp);

            //reading DataBits
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyDataBits, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'DataBits' setting in " + strSectionName;   //getting key value fail
            }
            this.clsNECWirelessControl.COMPort.DataBits = Convert.ToInt32(strTmp);

            //reading Parity
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyParity, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'Parity' setting in " + strSectionName;   //getting key value fail
            }
            switch (strTmp.ToUpper())
            {
                case "EVEN":
                    this.clsNECWirelessControl.COMPort.Parity = System.IO.Ports.Parity.Even;
                    break;
                case "ODD":
                    this.clsNECWirelessControl.COMPort.Parity = System.IO.Ports.Parity.Odd;
                    break;
                case "NONE":
                    this.clsNECWirelessControl.COMPort.Parity = System.IO.Ports.Parity.None;
                    break;
                default:
                    this.clsNECWirelessControl.COMPort.Parity = System.IO.Ports.Parity.Even;
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
                    this.clsNECWirelessControl.COMPort.StopBits = System.IO.Ports.StopBits.One;
                    break;
                case "TWO":
                    this.clsNECWirelessControl.COMPort.StopBits = System.IO.Ports.StopBits.Two;
                    break;
                case "ONEPOITFIVE":
                    this.clsNECWirelessControl.COMPort.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    break;
                case "NONE":
                    this.clsNECWirelessControl.COMPort.StopBits = System.IO.Ports.StopBits.None;
                    break;
                default:
                    this.clsNECWirelessControl.COMPort.StopBits = System.IO.Ports.StopBits.One;
                    break;
            }

            //Getting Module ID - Default
            strTmp = MyLibrary.ReadFiles.IniReadValue(strSectionName, strKeyModuleID, strFileName);
            if (strTmp.ToLower() == "error")
            {
                return "Error: cannot find 'ModuleID' setting in " + strSectionName;   //getting key value fail
            }
            this.clsNECWirelessControl.strModuleSerial = strTmp;

            //Try to Open COMPort
            try
            {
                if (this.clsNECWirelessControl.COMPort.IsOpen == false) //Not yet open
                {
                    this.clsNECWirelessControl.COMPort.Open();
                }
                else //Already open => Try to close & open again
                {
                    this.clsNECWirelessControl.COMPort.Close();
                    this.clsNECWirelessControl.COMPort.Open();
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            //Return Class of NEC Wireless control through User Return data
            List<object> lstobjTemp = new List<object>();
            lstobjTemp.Add("NEC");
            lstobjTemp.Add(this.clsNECWirelessControl);
            lstlstobjOutput.Add(lstobjTemp);

            //Return 0 if everything is OK
            return "0";
        }

        /// <summary>
        /// Get Info of NEC Wireless Module setting
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            List<object> lstobjTemp = new List<object>();
            Boolean boolTemp = false;

            if (clsNECWirelessControl == null) return "204-0-1 Error: clsNECWirelessControl not yet initialized!";
            boolTemp = clsNECWirelessControl.cmdGetModuleProperty();   //call Getting Module Property Func

            //To confirm Get Module Property OK or not
            if (boolTemp == true)
            {
                //Add to User Return
                lstobjTemp.Add("NecSetting");
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytChannel.ToString()); //0
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytPower.ToString()); //1
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRspBackoffCount.ToString()); //2
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRspBackoffMin.ToString()); //3
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRspBackoffMax.ToString()); //4
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRspEnable.ToString()); //5
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRetryCount.ToString()); //6
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytRetryWait.ToString()); //7
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytBackoffCount.ToString()); //8
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytBackoffMax.ToString()); //9
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytBackoffMin.ToString()); //10
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.lngRcvTime.ToString()); //11
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytSleepTime.ToString()); //12
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytReserved1.ToString()); //13
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytReserved2.ToString()); //14
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytCmdEnable.ToString()); //15
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.bytEDThreshold.ToString()); //16
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.lngSystemID.ToString()); //17
                lstobjTemp.Add(this.clsNECWirelessControl.clsModuleProperty.lngProductID.ToString()); //18

                lstlstobjOutput.Add(lstobjTemp);
            }
            else
            {
                return "204-0-1 Error: cannot get Module Property!";
            }

            return "0"; //OK code
        }

        /// <summary>
        /// Reset Wireless Module
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            List<object> lstobjTemp = new List<object>();

            //
            if (this.clsNECWirelessControl == null) return "204-0-2 Error: clsNECWirelessControl not yet initialized!";

            string strRet = this.clsNECWirelessControl.NECExecuteReset();


            //
            return strRet;
        }

        /// <summary>
        /// Getting AD data form B command
        /// + Para1 (13): Data size want to get
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            List<object> lstobjTemp = new List<object>();

            string strRet = "0";
            List<byte> lstbyteReceive = new List<byte>();
            //
            int intDataSize = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString().Trim(), out intDataSize) == false) return "204-0-3: Datasize setting [" + lstlstobjInput[0][13].ToString() + "] is not integer!";

            //
            strRet = this.clsNECWirelessControl.NECBCommandGetADhandle(intDataSize, out lstbyteReceive);

         
            //Add user ret
            lstobjTemp = new List<object>();
            lstobjTemp.Add("NEC");
            for(int i = 0;i<lstbyteReceive.Count;i++)
            {
                lstobjTemp.Add(lstbyteReceive[i]);
            }
            lstlstobjOutput.Add(lstobjTemp);
            
            //Cal all received byte to AD value (High byte * 256 + Low byte)
            lstobjTemp = new List<object>();
            lstobjTemp.Add("AD");
            //if ((lstbyteReceive.Count%2)==0)
            //{
            for(int j=0;j<lstbyteReceive.Count;j++)
            {
                if (j + 1 > lstbyteReceive.Count - 1) break;

                int intADTemp = 256 * lstbyteReceive[j] + lstbyteReceive[j + 1];
                lstobjTemp.Add(intADTemp);
                //
                j++; //Need to do this to abort next j
            }
            //}
            lstlstobjOutput.Add(lstobjTemp);

            //
            return strRet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule4(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            ////public string NECWR(string strCommand, string strModuleSerial,long lngTimeOut = 2000,long lngRcvTimeOut = 1000)
            //string strModuleSerial = lstlstobjInput[0][15].ToString().Trim();
            //if (strModuleSerial == "") //Not input - use current Module Serial
            //{
            //    strModuleSerial = this.clsNECWirelessControl.strModuleSerial;
            //}

            //long lngTimeOut = 0;
            //if (long.TryParse(lstlstobjInput[0][13].ToString(), out lngTimeOut) == false)
            //{
            //    lngTimeOut = 2000; //Default Value
            //}

            //long lngRcvTimeOut = 0;
            //if (long.TryParse(lstlstobjInput[0][14].ToString(), out lngRcvTimeOut) == false)
            //{
            //    lngRcvTimeOut = 2000; //Default Value
            //}

            string strRet = "";
            //
            //strRet = this.clsNECWirelessControl.NECWRBCommand(lstlstobjInput[0][11].ToString().Trim(), strModuleSerial, lngTimeOut, lngRcvTimeOut);

            //UserRet
            //if (strRet == "0") //Sending OK
            //{
            //lstobjOutput.Add("NEC");
            //for (int i = 0; i < this.clsNECWirelessControl.lstByteReceiveData.Count; i++)
            //{
            //    lstobjOutput.Add(this.clsNECWirelessControl.lstByteReceiveData[i]);
            //}
            //lstlstobjOutput.Add(lstobjOutput);
            //}
            //return value
            return strRet;
        }


        
        /// <summary>
        /// NEC Wireless Module Sending Command Function
        ///     - Transmission area: Command for Sending
        ///     - Para1 (13): Time Out for each sending
        ///     - Para2 (14): Time out for receiving polling process
        ///     - Para3 (15): Optional Confirm Data return (byte No 3 must be 0x11) - 0: No confirm, 1: confirm (Default)
        ///     - Para4 (16): Optional Auto insert sequence number - 0: no input, 1: Auto input (Default)
        ///     - Para3 (17): Module serial input (optional): If no input, use current setting of wireless control class
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule14(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            //public string NECWR(string strCommand, string strModuleSerial,long lngTimeOut = 2000,long lngRcvTimeOut = 1000)
            string strModuleSerial = lstlstobjInput[0][17].ToString().Trim();
            if(strModuleSerial=="") //Not input - use current Module Serial
            {
                strModuleSerial = this.clsNECWirelessControl.strModuleSerial;
            }

            long lngTimeOut = 0;
            if(long.TryParse(lstlstobjInput[0][13].ToString(),out lngTimeOut)==false)
            {
                lngTimeOut = 2000; //Default Value
            }

            long lngRcvTimeOut = 0;
            if (long.TryParse(lstlstobjInput[0][14].ToString(), out lngRcvTimeOut) == false)
            {
                lngRcvTimeOut = 2000; //Default Value
            }


            //Confirm Optional confirm data return
            bool blConfirmDataRet = true;
            if(lstlstobjInput[0][15].ToString().Trim() == "0")
            {
                blConfirmDataRet = false;
            }

            //Confirm Optional auto insert sequence number
            bool blAutoSequenceNo = true;
            if (lstlstobjInput[0][16].ToString().Trim() == "0")
            {
                blAutoSequenceNo = false;
            }


            string strRet = "";
            //
            strRet = this.clsNECWirelessControl.NECWR(lstlstobjInput[0][11].ToString().Trim(), strModuleSerial, lngTimeOut, lngRcvTimeOut, blConfirmDataRet, blAutoSequenceNo);

            //UserRet
            //if (strRet == "0") //Sending OK
            //{
            lstobjOutput.Add("NEC");
            for (int i = 0; i < this.clsNECWirelessControl.lstByteReceiveData.Count; i++)
            {
                lstobjOutput.Add(this.clsNECWirelessControl.lstByteReceiveData[i]);
            }
            lstlstobjOutput.Add(lstobjOutput);
            //}
            //return value
            return strRet;
        }

        /// <summary>
        /// Call out Window Setting of Wireless module
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginWirelessModule1000(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            List<object> lstObj = new List<object>();
            ///////////////////////////////////////////////////////
            if (this.clsNECWirelessControl == null)
            {
                this.clsNECWirelessControl = new clsWirelessCom();
            }

            this.clsNECWirelessControl.WindowSettingNEC = new windowWirelessSetting();
            this.clsNECWirelessControl.IniFormSetting();

            int intProcessID = 0;
            if(int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID)==false) return "Error: Process ID is not integer!";

            this.clsNECWirelessControl.WindowSettingNEC.intProcessID = intProcessID;
            this.clsNECWirelessControl.WindowSettingNEC.Show();

            ///////////////////////////////////////////////////////
            return "0";
        }
    }
}
