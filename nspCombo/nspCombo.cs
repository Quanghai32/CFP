using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Windows;
using nspINTERFACE;
using System.Net.Sockets;
using System.Management;
using System.Net.NetworkInformation;   

namespace nspCombo
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginComboChecker,12")]

    public class nspCombo : nspINTERFACE.IPluginExecute
    {

        public clsCanonUsbKernel.clsUsbKernel clsComboUsb { get; set; }


        public TcpClient client;
        public NetworkStream stream;
        public string HostName = System.Environment.MachineName;


        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);


        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal Int32 cbSize;
            internal System.Guid InterfaceClassGuid;
            internal Int32 Flags;
            internal IntPtr Reserved;
        }

        //GLOBAL VARIABLES & STRUCTURES DECLARATION
        internal const Int32 DIGCF_PRESENT = 2;
        internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

        private static readonly Guid GUID_DEVINTERFACE_USBPRINT = new Guid(0x28d78fad, 0x5a12, 0x11D1, 0xae, 0x5b, 0x00, 0x00, 0xf8, 0x03, 0xa8, 0xc2);


        internal const Int32 GENERIC_WRITE = 0X40000000;
        internal const Int32 OPEN_EXISTING = 3;
        internal const Int32 FILE_ATTRIBUTE_NORMAL = 0X80;
        public string strPIPE_NAME = "";

        //Implement INTERFACES
        #region _Implement_nspINTERFACE

        public void Sleep(int intMs) //Delay
        {
            this.clsComboUsb.Sleep(intMs);
        }

        #endregion


        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "12,0,0,1,2,3,10,21,22,23,1000"; lstobjInfo.Add(strTemp); //Jig ID,HardwareID,FunctionID
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, NINH CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 0.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 7/Sep/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, New Extension for Combo Checker"; lstobjInfo.Add(strTemp);

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
                case 12:
                    return SelectHardIDFromJigID12(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID12(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return PluginComboCheckerFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginComboCheckerFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginComboCheckerFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginComboCheckerFuncID3(lstlstobjInput, out lstlstobjOutput);
                case 10:
                    return PluginComboCheckerFuncID10(lstlstobjInput, out lstlstobjOutput);
                case 21:
                    return PluginComboCheckerFuncID21(lstlstobjInput, out lstlstobjOutput);
                case 22:
                    return PluginComboCheckerFuncID22(lstlstobjInput, out lstlstobjOutput);
                case 23:
                    return PluginComboCheckerFuncID23(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        public object PluginComboCheckerFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            return "0";
        }
/////////*********************************************************************************************************************************************************************///////////////////////////
 public object PluginComboCheckerFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            return 0; //OK code
        }
////////*********************************************************************************************************************************************************************///////////////////////////
        // Para 1 (13): Jigport
        // TCP Connection
  public object PluginComboCheckerFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            int JigPort = Convert.ToInt16(lstlstobjInput[0][13].ToString());
            try
            {
                //Create a TcpClient.
                client = new TcpClient(HostName, JigPort);
                //Get a client stream for reading and writing
                stream = client.GetStream();
                //Send the message to the connected TcpServer
                return 0; //OK code
            }
            catch (Exception ex)
            {
                return "Error in Function ID 12-0-2. Error Message: " + ex.Message;
            }
        }

 ////////*********************************************************************************************************************************************************************///////////////////////////
        // TCP Close
        public object PluginComboCheckerFuncID3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            client.Close();
            stream.Close();
            return 0;
        }
 /////////*********************************************************************************************************************************************************************//////////////////////////
        /// Detect USB for UI Mode
        ///     +Para1 (13): Time out setting
        ///     +Para2 (14): Optional to Auto Find pipename (1: Auto Find. Other: use setting pipe name)
        ///     +Para3 (15): Product ID want to Auto Find
  public object PluginComboCheckerFuncID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {

            uint StartTime = 0;
            int intTimeOut = 0; //ms
            bool AutoFindPipe = false;

            lstlstobjOutput = new List<List<object>>();
            var lststrTemp1 = new List<object>();

            if (lstlstobjInput[0][14].ToString().Trim() == "1") AutoFindPipe = true;
            //time out
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = int.Parse(lstlstobjInput[0][13].ToString());
            }
            else //illegal parameter input - set to default is 10000
            {
                intTimeOut = 10000;
            }
            string strProductID = lstlstobjInput[0][15].ToString();

            try
            {
                int proRet = 0;
                //1. Polling to detect whether or not Main PCB connect to computer
                StartTime = GetTickCount();

                if (AutoFindPipe == false)
                {
                    SafeFileHandle PollingHandle;
                    //
                    do
                    {
                        //Application.DoEvents();
                        Sleep(100);
                        PollingHandle = CreateFile("\\\\.\\\\" + this.strPIPE_NAME, GENERIC_WRITE, 0, IntPtr.Zero,
                                              OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                        if ((GetTickCount() - StartTime) > intTimeOut)
                        {
                            return 256; //Error code
                        }

                    } while (PollingHandle.IsInvalid == true);

                    //2. We need to free Handle after using
                    if (!(PollingHandle.IsInvalid))
                    {
                        PollingHandle.Close();
                    }
                    //3. If polling time out, then exit return error code & exit function
                    if (proRet == 256)
                    {
                        return proRet;
                    }
                }



                else //Auto find pipe
                {
                    bool blExit = false;
                    do
                    {
                        List<string> lststrTemp = new List<string>();
                        lststrTemp = this.AutoFindPipe(strProductID);

                        if (lststrTemp.Count == 1) //Finding OK
                        {
                            //Assign pipe name & exit
                            this.strPIPE_NAME = lststrTemp[0].Remove(0, 4);
                            break;
                        }
                        else if (lststrTemp.Count > 1) //Error: there is more than 1 device connected to PC
                        {
                            return 257; //Error code
                        }

                        if ((GetTickCount() - StartTime) > intTimeOut)
                        {
                            return 258; //Error code
                        }

                    } while (blExit == false);
                }
            }
            catch (Exception ex)
            {
                return 9999; //Error code
            }

            //Record Pipe
            lststrTemp1 = new List<object>();
            lststrTemp1.Add("pipe");
            lststrTemp1.Add(this.strPIPE_NAME);
            lstlstobjOutput.Add(lststrTemp1);

            //Return value

            return 0;
        }
  /////////***********************************************Open file .EXE, .Bat ****************************************************************************************************************////////////////////
        //        Para1(13) : File folder
        //        para2(14) : File name in folder
        //        para3(15) : Name  
  public object PluginComboCheckerFuncID21(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //1. Check if file is exist or not
            string strAppPath = lstlstobjInput[0][0].ToString();
            string iniFileName = @"\" + lstlstobjInput[0][13].ToString();
            string strFileName = strAppPath + iniFileName;

            string strTmp = "";
            string strKeyName = "";
            //2.Calculate string file name
            strKeyName = lstlstobjInput[0][15].ToString();//+ (intProcessID + 1).ToString();
            //Reading value in key name
            strTmp = MyLibrary.ReadFiles.IniReadValue(lstlstobjInput[0][14].ToString(), strKeyName, strFileName);

            //3.Check file exist if not will open exe
            if (MyLibrary.ChkExist.CheckFileExist(strTmp) == false)
            {
                return "Error: File is not exist"; //File not exist code
            }
            else
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = strTmp;
                proc.Start();
                return "0";
            }
        }
/////////*********************************************************************************************************************************************************************///////////////////////////
        // Para1 (13) : TCP Sending
        // Para2 (14) : Time Out
        // Para3 (15) : Compare
  public object PluginComboCheckerFuncID22(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<string>();

            uint StartTime = 0;
            int intTimeOut = 0; //ms

            Boolean blExit = false;

            int iRet = 0;
            string strSending = "";
            string strCompare = "";
            string DataRev = "";
            string MsgErr = "";
            string DataSend = "";


            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = int.Parse(lstlstobjInput[0][14].ToString());
            }
            else //illegal parameter input - set to default is 10000
            {
                intTimeOut = 10000;
            }
            // send command to PCB server and get USB number
            strSending = lstlstobjInput[0][13].ToString();
            strCompare = lstlstobjInput[0][15].ToString().Trim();


            StartTime = GetTickCount();
            iRet = ComboSocketServerWR(strSending, ref DataRev, ref MsgErr);
            if (iRet != 0)
            {
                DataSend = "Error: ComboSocketServerWR() failed. Return data: " + iRet.ToString();
            }
            else
            {
                do
                {

                    if (DataRev.Trim() == strCompare) //Finding OK
                    {
                        return 0;
                    }
                    else
                    {
                        iRet = ComboSocketServerWR(strSending, ref DataRev, ref MsgErr);
                        System.Threading.Thread.Sleep(200);
                    }
                    if ((GetTickCount() - StartTime) > intTimeOut)
                    {
                        DataSend = DataRev; //Error code
                        blExit = true;
                    }

                } while (blExit == false);

            }
            return DataSend;
        }
/////////*********************************************************************************************************************************************************************////////////////////////
        /// Check Status Scanner
        ///  +Para1 (13): What Kind Of Scanner (9000F Mark II, 9000F...)
        ///  +Para2 (14): Properties
        ///  +Para3 (15): Timeout
  public object PluginComboCheckerFuncID23(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<string>();

            uint StartTime = 0;
            int intTimeOut = 0; //ms

            string strDeviceName = "";
            string strProperties = "";
            string Result = "";
            string Compare = "";
            double dblCounter = 0;

            // Get Request Properties
            strProperties = lstlstobjInput[0][14].ToString();
            strDeviceName = lstlstobjInput[0][13].ToString();
            Compare = lstlstobjInput[0][16].ToString();
            string strDeviceNameSend = @"SELECT * From Win32_PnPEntity where Name Like " + string.Format("\"{0}\"", strDeviceName);

            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intTimeOut) == true) //legal parameter
            {
                intTimeOut = int.Parse(lstlstobjInput[0][15].ToString());
            }
            else //illegal parameter input - set to default is 10000
            {
                intTimeOut = 10000;
            }

            StartTime = GetTickCount();

            ManagementObjectCollection collection;
            
            do
            {
                using (var searcher = new ManagementObjectSearcher(strDeviceNameSend))
                    collection = searcher.Get();
                dblCounter = collection.Count;
                if ((GetTickCount() - StartTime) > intTimeOut)
                {
                    Result = "Time Out ";
                    dblCounter = 99;
                }
                if (dblCounter == 1)
                {
                    foreach (var device in collection)
                    {
                        Result = (string)device.GetPropertyValue(strProperties);
                        if (Result == Compare)
                        {
                            Result = "0";
                            dblCounter = 99;
                        }
                        else
                        {
                            dblCounter = 0;
                        }
                    }
                }


            } while (dblCounter < 1);
            return Result;
        }
/////////*********************************************************************************************************************************************************************////////////////////
  //private static readonly Guid GUID_DEVINTERFACE_USBPRINT = new Guid(0x28d78fad, 0x5a12, 0x11D1, 0xae, 0x5b, 0x00, 0x00, 0xf8, 0x03, 0xa8, 0xc2);
  public List<string> AutoFindPipe(string strProductID) //strProductID = "12FE"... Vendor ID always = 04A9 (Canon)
        {
            List<string> lststrRet = new List<string>();

            //OK: USB#VID_04A9&PID_12FE#6&3a34e632&0&1#{28d78fad-5a12-11d1-ae5b-0000f803a8c2}
            string strVID = "VID_04A9";
            string strPID = "PID_" + strProductID.ToUpper();

            //
            //List<USBDeviceInfoClass> lstDevices = new List<USBDeviceInfoClass>();
            //lstDevices = this.GetConnectedUSBDevices();

            String[] devicePathName = new String[128];
            bool blRest = this.FindDeviceFromGuid(GUID_DEVINTERFACE_USBPRINT, ref devicePathName);

            //Analyze data 
            int i = 0;
            if (blRest == true)
            {
                for (i = 0; i < devicePathName.Length; i++)
                {
                    if (devicePathName[i] == null) continue;
                    //
                    string strDevicePath = devicePathName[i].ToUpper();
                    //Verify VendorID & Product ID matching or not
                    if ((strDevicePath.Contains(strVID) == true) && (strDevicePath.Contains(strVID) == true)) //Matching
                    {
                        lststrRet.Add(strDevicePath);
                    }
                }
            }

            //
            return lststrRet;
        }
/////////*********************************************************************************************************************************************************************////////////////////////
  public int ComboSocketServerWR(string MsgToSend, ref string MsgRcv, ref string MsgErr)
        {
            try
            {
                //Translate the passed message into ASCII and store it as a Byte array
                //stream = client.GetStream();
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(MsgToSend);
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
                MsgErr = "";

                return 0; //OK code
            }
            catch (Exception ex)
            {
                MsgErr = ex.Message;
                return 9999; //Unexpected Error Code
            }
        }
 /////////*********************************************************************************************************************************************************************////////////////////////
  internal Boolean FindDeviceFromGuid(System.Guid myGuid, ref String[] devicePathName)
        {
            Int32 memberIndex = 0;
            Boolean deviceFound = false;
            Boolean lastDevice = false;
            Boolean success = false;
            IntPtr deviceInfoSet = new System.IntPtr();
            SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            Int32 bufferSize = 0;
            IntPtr detailDataBuffer = IntPtr.Zero;

            try
            {
                //Retrieves a device information set for a specified group of devices.
                deviceInfoSet = SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

                //Start searching
                deviceFound = false;
                memberIndex = 0;

                do
                {
                    //The SetupDiEnumDeviceInterfaces function enumerates the device interfaces that are contained in a device information set. 
                    success = SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref myGuid, memberIndex, ref MyDeviceInterfaceData);

                    if (success == true) //There is still device for searching
                    {
                        //The SetupDiGetDeviceInterfaceDetail function returns details about a device interface.
                        success = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref MyDeviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, IntPtr.Zero);

                        // Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        // Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.
                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        // Call SetupDiGetDeviceInterfaceDetail again. This time, pass a pointer to DetailDataBuffer and the returned required buffer size.
                        success = SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref MyDeviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, IntPtr.Zero);

                        // Skip over cbsize (4 bytes) to get the address of the devicePathName.
                        IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

                        // Get the String containing the devicePathName.
                        devicePathName[memberIndex] = Marshal.PtrToStringAuto(pDevicePathName);

                        deviceFound = true;
                    }
                    else
                    {
                        lastDevice = true; //No more device for searching
                    }

                    //Continue seraching for next member
                    memberIndex = memberIndex + 1;
                }
                while (lastDevice == false);

                return deviceFound;
            }
            catch (Exception ex)
            {
                //throw;
                return false;
            }
            finally
            {
                if (detailDataBuffer != IntPtr.Zero)
                {
                    // Free the memory allocated previously by AllocHGlobal.
                    Marshal.FreeHGlobal(detailDataBuffer);
                }

                //  Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
