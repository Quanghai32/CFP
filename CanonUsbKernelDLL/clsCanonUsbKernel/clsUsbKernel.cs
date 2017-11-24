//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CANON MAIN USB KERNEL DLL - VISUAL C# 2010                                                                                   //
// AUTHOR: HOANG DO VAN - CANON VIETNAM - THANG LONG PDE DEPARTMENT                                                             //
// ENVIRONMENT: VISUAL C# .NET (VERSION: 2010)                                                                                  //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Description:                                                                                                                 //
//     1. This module, write totally by C# .Net to communication with Canon Main PCB USB                                        //
//     2. Because of SECRET MATTER, the author don't have any knowledge about Canon Main PCB firmware. All code the author      // 
//        write here base on personal knowdledges and skills, and base on old VB6 module.                                       //
//     3. You can freely use these code. But because of No.2 please UNDERSTAND YOUR OWN RISK.                                   //
//     4. If you need any further information, please contact to the author by following email address:                         //
//             + pde-adm11@cvn.canon.co.jp                                                                                      //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Version History:                                                                                                             //
//     Version1.00: New class design. Hoang PDE. JAN/2014.                                                                      //
//     Version1.01: Modify USB_WR(), USB_DR() - add 'out byte[]' parameter. Fix error in USB_DR(). Hoang PDE. 9/Sep/2014.       //
//     Version1.02: Add function DetectDevice(). Hoang PDE. 26/Aug/2016.                                                        //
//     Version1.03: Increase ReceiveData & SendData array size from 512 to 65536. Hoang PED 11/Nov/2016                         //
//                                                                                                                              //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;


namespace clsCanonUsbKernel
{
    public class clsUsbKernel
    {
        //*************************************API FUNCTONS DECLARATION***********************************************************

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Boolean ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Boolean WriteFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr SecurityAttributes, Boolean bManualReset, Boolean bInitialState, String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean GetOverlappedResult(SafeFileHandle hFile, IntPtr lpOverlapped, ref Int32 lpNumberOfBytesTransferred, Boolean bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern UInt32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        //***********************************************************************************************************************************
        //API FUNCTIONS DECLARATION - ADDITIONAL
        [DllImport("hid.dll", SetLastError = true)]
        internal static extern void HidD_GetHidGuid(ref System.Guid HidGuid);

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


        //*************************************STRUCTURES & CONSTANT DECLARATION*************************************************************
        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal Int32 nLength;
            internal Int32 lpSecurityDescriptor;
            internal Int32 bInheritHandle;
        }

        internal const Int32 GENERIC_READ = unchecked((int)0X80000000);
        //internal const Int32 GENERIC_READ = 0X7FFFFFFF;
        //internal const Int32 GENERIC_READ = 2147483648;

        internal const Int32 GENERIC_WRITE = 0X40000000;
        internal const Int32 OPEN_EXISTING = 3;
        internal const Int32 FILE_ATTRIBUTE_NORMAL = 0X80;
        internal const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;
        internal const Int32 WAIT_TIMEOUT = 0X102;
        internal const Int32 WAIT_OBJECT_0 = 0;
        internal const Int32 SEND_DEVICE_DATA = 0X222034;
        internal const Int32 GET_DEVICE_CAPABILITIES = 0X222020;

        //***************************************VARIABLES DECLARATION************************************************************
        //internal const Int32 MaxLengs = 512; //
        internal const Int32 MaxLengs = 80000; //Follow latest Yako modify...
        
        public byte[] SendData = new byte[MaxLengs]; //Contain sending data
        public byte[] ReceiveData = new byte[MaxLengs]; //Contain receive data
        public string strPIPE_NAME = "";

        public int RxdLengs = 0;
        public uint StartTime = 0;
        public uint EndTime = 0;

        //***************************************FUNCTIONS DECLARATION************************************************************

        /// <summary>
        /// Sleep function
        /// create delay
        /// <param name="intMs"></param>
        public void Sleep(int intMs)
        {
            try
            {
                Thread.Sleep(intMs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sleep()");
            }
        }
        /// <summary>
        /// PrepareForOverlappedTransfer function
        /// </summary>
        public void PrepareForOverlappedTransfer(ref NativeOverlapped Overlapped, ref IntPtr eventObject)
        {
            try
            {
                eventObject = CreateEvent(IntPtr.Zero, false, false, String.Empty);
                Overlapped.OffsetLow = 0;
                Overlapped.OffsetHigh = 0;
                Overlapped.EventHandle = eventObject;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PrepareForOverlappedTransfer()");
            }
        }

        //**************************************************************************************************************************************
        public int Send(int TimeUpW)
        {
            try
            {
                int intCmdLen = 0;
                SafeFileHandle WriteHandle;
                bool lngAPIReVal = false;
                int lngEndOfByte = 0;

                //1. Calculate SendData Length
                intCmdLen = SendData[0] * 256 + SendData[1];
                if (intCmdLen > (MaxLengs + 1))
                {
                    return 255; //Error code
                }

                //2. Create File
                WriteHandle = CreateFile("\\\\.\\\\" + strPIPE_NAME, GENERIC_WRITE, 0, IntPtr.Zero,
                                         OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
                if (WriteHandle.IsInvalid == true)
                {
                    return 1; //Error code
                }

                //3. Write File
                lngAPIReVal = WriteFile(WriteHandle, SendData, intCmdLen + 2, ref lngEndOfByte, IntPtr.Zero); //Send only length of factory command & 2 first byte
                if (!(WriteHandle.IsInvalid))
                {
                    WriteHandle.Close();
                }

                //4. If everything ok. Return 0: successful code
                return 0; //Successful code
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message, "Send()");
                return 9999; //Unexpected Error code
            }
        }
        //**************************************************************************************************************************************
        public int Receive(UInt32 TimeUpR = 1000)
        {
            try
            {
                //
                int i = 0;
                SafeFileHandle ReadHandle;
                long intAPIRetVal = 0;
                bool blnAPIRetVal = false;
                int lngEndOfByte = 0;

                IntPtr eventObject = new IntPtr();
                NativeOverlapped HidOverlapped = new NativeOverlapped();
                IntPtr nonmanagedBuffer = new IntPtr();
                IntPtr nonmanagedOverlapped = new IntPtr();

                PrepareForOverlappedTransfer(ref HidOverlapped, ref eventObject);
                nonmanagedBuffer = Marshal.AllocHGlobal(ReceiveData.Length);
                nonmanagedOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(HidOverlapped));
                Marshal.StructureToPtr(HidOverlapped, nonmanagedOverlapped, false);

                ReadHandle = CreateFile("\\\\.\\\\" + strPIPE_NAME, GENERIC_READ, 0, IntPtr.Zero,
                                        OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                if (ReadHandle.IsInvalid == true)
                {
                    return 11; //Error code
                }

                for (i = 0; i < 4; i++)
                {
                    ReceiveData[i] = 0;
                }

                blnAPIRetVal = ReadFile(ReadHandle, nonmanagedBuffer,
                                        ReceiveData.Length, ref lngEndOfByte, nonmanagedOverlapped);

                if (!(blnAPIRetVal))
                {
                    intAPIRetVal = WaitForSingleObject(eventObject, 3000);
                    switch (intAPIRetVal)
                    {
                        case WAIT_OBJECT_0:
                            blnAPIRetVal = true;
                            GetOverlappedResult(ReadHandle, nonmanagedOverlapped, ref lngEndOfByte, false);
                            break;
                        case WAIT_TIMEOUT:
                            if (ReadHandle.IsInvalid == true)
                            {
                                ReadHandle.Close();
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (blnAPIRetVal)
                {
                    Marshal.Copy(nonmanagedBuffer, ReceiveData, 0, lngEndOfByte);
                }

                //With unmanaged memory, after no use we have to free them to prevent memory leak
                Marshal.FreeHGlobal(nonmanagedBuffer);
                Marshal.FreeHGlobal(nonmanagedOverlapped);


                if (!(ReadHandle.IsInvalid))
                {
                    ReadHandle.Close();
                }

                RxdLengs = ReceiveData[0] * 256 + ReceiveData[1];

                return 0; //Successful code
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Receive()");
                return 9999; //Unexpected Error code
            }
        }
        //**************************************************************************************************************************************
        public int USBWrite(string strData, int WaitTM = 5000)
        {
            //This function, convert from factory command string to SendData numerical value
            try
            {
                int iret = 0;
                string[] strTemp;
                int i = 0;

                //We should allow user not input comma at the end of usb command - it mean intput comma is ok, and not input is OK also! => convenient
                strTemp = strData.Split(',');

                //Trim it all
                for (i = 0; i < strTemp.Length;i++)
                {
                    strTemp[i] = strTemp[i].Trim();
                }

                //for (i = 0; i < strTemp.Length - 1; i++)
                //{
                //    SendData[i] = Convert.ToByte(strTemp[i], 16);
                //}

                for (i = 0; i < strTemp.Length; i++)
                {
                    if (i == (strTemp.Length - 1)) //Last character
                    {
                        //Only send if not empty ""
                        if(strTemp[i] != "")
                        {
                            SendData[i] = Convert.ToByte(strTemp[i], 16);
                        }
                    }
                    else //Not last character
                    {
                        SendData[i] = Convert.ToByte(strTemp[i], 16);
                    }
                   
                }

                iret = Send(WaitTM);

                return iret;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message, "USBWrite()");
                return 9999; //Unexpected error code
            }
        }
        //**************************************************************************************************************************************
        public int USB_RV(long Rnum = 7000, uint RCVTM = 5000)
        {
            try
            {
                long ret = 0;
                int SndCmd = 0;
                int RcvCmd = 0;
                long i = 0;

                SndCmd = SendData[2] * 256 + SendData[3];

                for (i = 0; i < Rnum + 1; i++)
                {
                    Application.DoEvents();
                    ret = Receive(RCVTM);
                    if (ret == 0)
                    {
                        RcvCmd = ReceiveData[2] * 256 + ReceiveData[3];
                        if (RcvCmd == SndCmd)
                        {
                            return ReceiveData[4];
                        }
                        else
                        {
                            Sleep(1);
                        }
                    }
                    else
                    {
                        return 257; //Error code
                    }

                }

                return 258; //Error code
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "USB_RV()");
                return 9999; //Unexpected error code
            }
        }
        //**************************************************************************************************************************************
        public int USB_DR(string MainCmd, string SubCmd,out byte[] byteMainUsbReceiveData, long Rnum = 500, uint RCVTM = 500)
        {
            byteMainUsbReceiveData = this.ReceiveData; // Note that array is reference type => From now on, byteMainUsbReceiveData is always point to ReceiveData[]
            try
            {
                long ret = 0;
                int SndCmd = 0;
                int RcvCmd = 0;
                long i = 0;
                byte byteMainCmd = 0;
                byte byteSubCmd = 0;

                byteMainCmd = byte.Parse(MainCmd, System.Globalization.NumberStyles.HexNumber);
                byteSubCmd = byte.Parse(SubCmd, System.Globalization.NumberStyles.HexNumber);

                SndCmd = 256 * byteMainCmd + byteSubCmd;
                //SndCmd = Convert.ToByte(MainCmd, 16) * 256 + Convert.ToByte(SubCmd, 16);

                for (i = 0; i < Rnum + 1; i++)
                {
                    Application.DoEvents();
                    ret = Receive(RCVTM);
                    if (ret == 0)
                    {
                        RcvCmd = ReceiveData[2] * 256 + ReceiveData[3];
                        if (RcvCmd == SndCmd)
                        {
                            return ReceiveData[4];
                        }
                        else
                        {
                            Sleep(1);
                        }
                    }
                    else
                    {
                        return 257; //Error code
                    }

                }

                return 258; //Error code
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message, "USB_DR()");
                return 9999; //Unexpected error code
            }
        }
        //**************************************************************************************************************************************
        public int USB_WR(string strData, out byte[] byteMainUsbReceiveData, long Rnum = 500, int SNDTM = 5000, uint RCVTM = 5000)
        {
            byteMainUsbReceiveData = this.ReceiveData; // Note that array is reference type => From now on, byteMainUsbReceiveData is always point to ReceiveData[]
            try
            {
                long ret = 0;
                int SndCmd = 0;
                int RcvCmd = 0;
                long i = 0;

                ret = USBWrite(strData, SNDTM);
                if (ret != 0)
                {
                    return 256; //Error code
                }

                SndCmd = SendData[2] * 256 + SendData[3];

                for (i = 0; i < Rnum + 1; i++)
                {
                    //Application.DoEvents();
                    ret = Receive(RCVTM);
                    if (ret == 0)
                    {
                        RcvCmd = ReceiveData[2] * 256 + ReceiveData[3];
                        if (RcvCmd == SndCmd)
                        {
                            return ReceiveData[4];
                        }
                        else
                        {
                            Sleep(1);
                        }
                    }
                    else
                    {
                        return 257; //Error code
                    }

                }

                return 258; //Error code
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "USB_WR()");
                return 9999; //Unexpected error code
            }
        }

        //**************************************************************************************************************************************
        public int USB_WR2(string strData,long Rnum = 500, int SNDTM = 5000, uint RCVTM = 5000)
        {
            try
            {
                long ret = 0;
                int SndCmd = 0;
                int RcvCmd = 0;
                long i = 0;

                ret = USBWrite(strData, SNDTM);
                if (ret != 0)
                {
                    return 256; //Error code
                }

                SndCmd = SendData[2] * 256 + SendData[3];

                for (i = 0; i < Rnum + 1; i++)
                {
                    //Application.DoEvents();
                    ret = Receive(RCVTM);
                    if (ret == 0)
                    {
                        RcvCmd = ReceiveData[2] * 256 + ReceiveData[3];
                        if (RcvCmd == SndCmd)
                        {
                            return ReceiveData[4];
                        }
                        else
                        {
                            Sleep(1);
                        }
                    }
                    else
                    {
                        return 257; //Error code
                    }

                }

                return 258; //Error code
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "USB_WR()");
                return 9999; //Unexpected error code
            }
        }


        //**************************************************************************************************************************************
        public int UnLockMainPCB(string strModeinExePath, int msTimeOut = 10000, bool blAutoFindPipe = false, string strProductID = "")
        {
            try
            {
                int proRet = 0;
                
                //1. Polling to detect whether or not Main PCB connect to computer
                StartTime = GetTickCount();

                if (blAutoFindPipe == false)
                {
                    SafeFileHandle PollingHandle;
                    //
                    do
                    {
                        //Application.DoEvents();
                        Sleep(100);
                        PollingHandle = CreateFile("\\\\.\\\\" + this.strPIPE_NAME, GENERIC_WRITE, 0, IntPtr.Zero,
                                              OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                        if ((GetTickCount() - StartTime) > msTimeOut)
                        {
                            proRet = 256; //Error code
                            break;
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
                        //Application.DoEvents();

                        //
                        List<string> lststrTemp = new List<string>();
                        lststrTemp = this.AutoFindPipe(strProductID);

                        if(lststrTemp.Count==1) //Finding OK
                        {
                            //Assign pipe name & exit
                            this.strPIPE_NAME = lststrTemp[0].Remove(0,4);
                            break;
                        }
                        else if (lststrTemp.Count>1) //Error: there is more than 1 device connected to PC
                        {
                            return 257; //Error code
                        }

                        //
                        if ((GetTickCount() - StartTime) > msTimeOut)
                        {
                            return 258; //Error code
                        }

                    } while (blExit == false);
                }

                //4. If detect device match pipe name, then we unlock it
                Process proTest = new Process();
                proTest.StartInfo.FileName = strModeinExePath + @"\ModeInChecker.exe";

                proTest.StartInfo.Arguments = "\\\\.\\\\" + this.strPIPE_NAME;
                proTest.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                proTest.Start();
                proTest.WaitForExit(1000);
                proRet = proTest.ExitCode;

                //4. Finally, return code assign and exit function
                return proRet;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "UnLockMainPCB()");
                return 9999; //Error code
            }
        }

        //**************************************************************************************************************************************
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
            if (blRest==true)
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public List<USBDeviceInfoClass> GetConnectedUSBDevices()
        {
            List<USBDeviceInfoClass> _devices = new List<USBDeviceInfoClass>();
            System.Management.ManagementObjectCollection collection;
            //
            using(var searcher = new System.Management.ManagementObjectSearcher(@"Select * From Win32_USBHub"))
            {
                collection = searcher.Get();
            }
            //
            foreach(var device in collection)
            {
                //string strTest = (string)device.GetPropertyValue("ContainerID");


                _devices.Add(new USBDeviceInfoClass(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Description")));
            }
            collection.Dispose();
            return _devices;
        }

        //**************************************************************************************************************************************
        public bool DetectDevice(string strPipeCheck = "", int msTimeOut = 0, int msSliceTime = 100)
        {
            bool blRet = false;
            //////////////////////////////////////////////
            if (strPipeCheck.Trim() == "") strPipeCheck = this.strPIPE_NAME;

            //
            SafeFileHandle PollingHandle;

            //1. Polling to detect whether or not Main PCB connect to computer
            StartTime = GetTickCount();
            do
            {
                //Application.DoEvents();
                PollingHandle = CreateFile("\\\\.\\\\" + strPipeCheck, GENERIC_WRITE, 0, IntPtr.Zero,
                                      OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                //PollingHandle = CreateFile("\\\\.\\\\" + strPipeCheck, GENERIC_READ, 0, IntPtr.Zero,
                //                      OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);

                if (msTimeOut!=0) //need check timeout
                {
                    if ((GetTickCount() - StartTime) > msTimeOut)
                    {
                        blRet = false; //Time out
                        break;
                    }
                }

                Sleep(msSliceTime);

            } while (PollingHandle.IsInvalid == true);

            //2. We need to free Handle after using
            if (!(PollingHandle.IsInvalid)) //PollingHandle is valid. It mean already detect device attached!
            {
                blRet = true;
                PollingHandle.Close();
            }
           
            //////////////////////////////////////////////
            return blRet;
        }

        //**************************************************************************************************************************************

    } //End Class

    public class USBDeviceInfoClass
    {
        public string strDeviceID { get; set; }
        public string strPnpDeviceID { get; set; }
        public string strDescription { get; set; }

        //Constructor
        public USBDeviceInfoClass(string DevideID, string PnpDeviceID, string Description)
        {
            this.strDeviceID = DevideID;
            this.strPnpDeviceID = PnpDeviceID;
            this.strDescription = Description;
        }
    }


}//End namespace