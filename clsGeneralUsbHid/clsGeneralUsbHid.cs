//'***************************************************************************************************************************
//'***************************************************************************************************************************
//'*                                     USB HID MODULE - MICROSOFT C# 2010                                                  *
//'*                                        <WRITTEN BY HOANG CVN>                                                           *                                        
//'***************************************************************************************************************************
//'*   VERSION HISTORY:                                                                                                      *
//'*       Version 1.0: New DLL File                                                                                         *
//'***************************************************************************************************************************
//'***************************************************************************************************************************
//'*   ABOUT:                                                                                                                *
//'*       1. This module, written by Hoang cvn. For communication with USB HID device                                       *
//'*       2. Functions and Sub using in these code are written by or collected by Hoang - cvn.                              *
//'***************************************************************************************************************************
//'*                                     USB COMMUNICATION FRAME                                                             *
//'***************************************************************************************************************************
//'*   This communication frame, using for communication with micro-controller. Description:                                 *
//'*   - Frame format:                                                                                                       *
//'*       + USB command send from Host to Micro-controller:                                                                 *
//'*                                                                                                                         *
//'*      Length(1Byte) - Main Cmd(1Byte) - Sub Cmd(1Byte) - Parameter 0(1Byte) - Parameter 1(1Byte) - Parameter 2(1Byte) -  *
//'*      Data sending are outputReportBuffer().                                                                             *
//'*      Note that using from outputReportBuffer(1). outputReportBuffer(0) always is 0: Send report code                    *
//'*      Length = outputReportBuffer(1). Main Cmd = outputReportBuffer(2). Sub Cmd = outputReportBuffer(3)...               *
//'*                                                                                                                         *
//'*       + USB data send from Micro-controller to Host:                                                                    *
//'*                                                                                                                         *
//'*      Length(1Byte) - Main Cmd(1Byte) - Sub Cmd(1Byte) - Parameter 0(1Byte) - Parameter 1(1Byte) - Parameter 2(1Byte) -  *
//'*      Data received are inputReportBuffer().                                                                             *
//'*      Note that using from inputReportBuffer(1). inputReportBuffer(0) always is 0: Read report code                      *
//'*      Length = inputReportBuffer(1). Main Cmd = inputReportBuffer(2). Sub Cmd = inputReportBuffer(3)...                  *
//'*                                                                                                                         *
//'***************************************************************************************************************************
//'***************************************************************************************************************************
//'*   WARNING:                                                                                                              *
//'*       1. I am not responsible for any kind of risk or damage caused by any code in these modules                        *
//'*       2. Use these modules and understand that your own risks                                                           *
//'*       3. You can freely use or distribute these module but should not modify anything if you do not understand          *
//'***************************************************************************************************************************
//'*   CONTACT:                                                                                                              *
//'*       1. Email: pde-adm11@canon-cvn.canon.co.jp                                                                         *
//'*       2. Phone: +84904.809.771                                                                                          *
//'*       3. PHS: 6609-1405                                                                                                 *
//'***************************************************************************************************************************
//'***************************************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading; 

namespace clsGeneralUsbHid
{
    public class clsGeneralUsbHid
    {
        //API FUNCTIONS DECLARATION
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetValueCaps(Int32 ReportType, Byte[] ValueCaps, ref Int32 ValueCapsLength, IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FlushQueue(SafeFileHandle HidDeviceObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean WriteFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateEvent(IntPtr SecurityAttributes, Boolean bManualReset, Boolean bInitialState, String lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean GetOverlappedResult(SafeFileHandle hFile, IntPtr lpOverlapped, ref Int32 lpNumberOfBytesTransferred, Boolean bWait);       


        //GLOBAL VARIABLES & STRUCTURES DECLARATION
        internal const Int32 DIGCF_PRESENT = 2;
        internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

        internal const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;
        internal const Int32 FILE_SHARE_READ = 1;
        internal const Int32 FILE_SHARE_WRITE = 2;
        internal const UInt32 GENERIC_READ = 0X80000000;
        internal const UInt32 GENERIC_WRITE = 0X40000000;
        internal const Int32 INVALID_HANDLE_VALUE = -1;
        internal const Int32 OPEN_EXISTING = 3;
        internal const Int32 WAIT_TIMEOUT = 0X102;
        internal const Int32 WAIT_OBJECT_0 = 0;

        internal const Int16 HidP_Input = 0;
        internal const Int16 HidP_Output = 1;
        internal const Int16 HidP_Feature = 2; 

        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal Int32 cbSize;
            internal System.Guid InterfaceClassGuid;
            internal Int32 Flags;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDD_ATTRIBUTES
        {
            internal Int32 Size;
            internal UInt16 VendorID;
            internal UInt16 ProductID;
            internal UInt16 VersionNumber;
        }

        internal struct HIDP_CAPS
        {
            internal Int16 Usage;
            internal Int16 UsagePage;
            internal Int16 InputReportByteLength;
            internal Int16 OutputReportByteLength;
            internal Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            internal Int16[] Reserved;
            internal Int16 NumberLinkCollectionNodes;
            internal Int16 NumberInputButtonCaps;
            internal Int16 NumberInputValueCaps;
            internal Int16 NumberInputDataIndices;
            internal Int16 NumberOutputButtonCaps;
            internal Int16 NumberOutputValueCaps;
            internal Int16 NumberOutputDataIndices;
            internal Int16 NumberFeatureButtonCaps;
            internal Int16 NumberFeatureValueCaps;
            internal Int16 NumberFeatureDataIndices;
        } 
        
        //GLOBAL VARIABLES DECLARATION
        public int intVendorID {get;set;}
        public int intProductID { get; set; }

        private SafeFileHandle hidHandle;
        private SafeFileHandle readHandle;
        private SafeFileHandle writeHandle;


        internal HIDD_ATTRIBUTES DeviceAttributes;

        internal HIDP_CAPS Capabilities;
        public bool blManualSetCapabilities { get; set; }
        public short shrtOutputReportByteLength { get; set; }
        public short shrtInputReportByteLength { get; set; }

        public bool blUseAsyncRead { get; set; }


        private Boolean myDeviceDetected;

        public string strPipeName { get; set; }

        private delegate void ReadInputReportDelegate(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] readBuffer, ref Boolean success);

        private int intUSBCommandSendingCode;
        private int intUSBCommandReceiveCode;
        private bool blUSBSendCmdFinish;
        private bool blUSBSendCmdResult;


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Boolean FlushQueue(SafeFileHandle hidHandle)
        {
            Boolean success = false;

            try
            {
                //  ***
                //  API function: HidD_FlushQueue

                //  Purpose: Removes any Input reports waiting in the buffer.

                //  Accepts: a handle to the device.

                //  Returns: True on success, False on failure.
                //  ***

                success = HidD_FlushQueue(hidHandle);

                return success;
            }
            catch (Exception ex)
            {
                throw;
            }
        }  
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal String GetHidUsage(HIDP_CAPS MyCapabilities)
        {
            Int32 usage = 0;
            String usageDescription = "";

            try
            {
                //  Create32-bit Usage from Usage Page and Usage ID.

                usage = MyCapabilities.UsagePage * 256 + MyCapabilities.Usage;

                if (usage == Convert.ToInt32(0X102))
                {
                    usageDescription = "mouse";
                }

                if (usage == Convert.ToInt32(0X106))
                {
                    usageDescription = "keyboard";
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return usageDescription;
        }  
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal HIDP_CAPS GetDeviceCapabilities(SafeFileHandle hidHandle)
        {
            IntPtr preparsedData = new System.IntPtr();
            Int32 result = 0;
            Boolean success = false;

            try
            {
                //  retrieves a pointer to a buffer containing information about the device's capabilities.
                //  HidP_GetCaps and other API functions require a pointer to the buffer.
                success = HidD_GetPreparsedData(hidHandle, ref preparsedData);

                //  Purpose: find out a device's capabilities.
                //  For standard devices such as joysticks, you can find out the specific
                //  capabilities of the device.
                //  For a custom device where the software knows what the device is capable of,
                //  this call may be unneeded.

                result = HidP_GetCaps(preparsedData, ref Capabilities);
                if ((result != 0))
                {
                    //  Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
                    //  Each structure defines the capabilities of one value.

                    Int32 vcSize = Capabilities.NumberInputValueCaps;
                    Byte[] valueCaps = new Byte[vcSize];

                    result = HidP_GetValueCaps(HidP_Input, valueCaps, ref vcSize, preparsedData);                 
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                //  frees the buffer reserved by HidD_GetPreparsedData.
                if (preparsedData != IntPtr.Zero)
                {
                    success = HidD_FreePreparsedData(preparsedData);
                }
            }

            return Capabilities;
        }         
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Boolean FindDeviceFromGuid(System.Guid myGuid, ref String[] devicePathName)
        {
            Int32 memberIndex = 0;
            Boolean deviceFound =false;
            Boolean lastDevice = false;
            Boolean success= false;
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
                throw;
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
        /// <summary>
        /// This function searching & return true if device with matching Vendor ID & Product ID is connected to PC
        /// </summary>
        /// <param name="intVendorID"></param>
        /// <param name="intProductID"></param>
        /// <returns></returns>
        public bool isDeviceConnected(int intVendorID, int intProductID)
        {
            Boolean deviceFound = false; 
            Guid hidGuid = Guid.Empty;
            String[] devicePathName = new String[128];
            Int32 memberIndex = 0;
            Boolean success = false; 

            //Retrieves the interface class GUID for the HID class.
            HidD_GetHidGuid(ref hidGuid);

            //  Fill an array with the device path names of all attached HIDs.
            deviceFound = FindDeviceFromGuid(hidGuid, ref devicePathName);

            //  If there is at least one HID, attempt to read the Vendor ID and Product ID
            //  of each device until there is a match or all devices have been examined.
            if (deviceFound == true)
            {
                memberIndex = 0;
                myDeviceDetected = false;
                do
                {
                    hidHandle = CreateFile(devicePathName[memberIndex], 0, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, 0);

                    if (!hidHandle.IsInvalid) //If create file is OK.
                    {
                        //  Set the Size property of DeviceAttributes to the number of bytes in the structure.
                        DeviceAttributes.Size = Marshal.SizeOf(DeviceAttributes);

                        //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                        //  Product ID, and Product Version Number for a device.                       
                        success = HidD_GetAttributes(hidHandle, ref DeviceAttributes);

                        if (success == true)
                        {
                            if ((DeviceAttributes.VendorID == intVendorID) && (DeviceAttributes.ProductID == intProductID)) //Matching Vendor ID & Product ID
                            {
                                //If found, then assign pipe name for class, and exit
                                myDeviceDetected = true;
                                strPipeName = devicePathName[memberIndex];
                                break;
                            }
                        }
                        else  // It's not a match, so close the handle.
                        {
                            hidHandle.Close();
                        }

                    }
                    else //There was a problem in retrieving the information.
                    {
                        hidHandle.Close();
                    }

                    memberIndex++;
                }
                while (memberIndex < devicePathName.Length);

            }

            //If device is detected, then create Read Handle for use later

            return myDeviceDetected;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public List<string> AutoFindPipe(string strVendorID, string strProductID) //strProductID = "12FE"... Vendor ID always = 04A9 (Canon)
        {
            List<string> lststrRet = new List<string>();

            //OK: USB#VID_04A9&PID_12FE#6&3a34e632&0&1#{28d78fad-5a12-11d1-ae5b-0000f803a8c2}
            string strVID = "VID_" + strVendorID.ToUpper();
            string strPID = "PID_" + strProductID.ToUpper();

            //
            //List<USBDeviceInfoClass> lstDevices = new List<USBDeviceInfoClass>();
            //lstDevices = this.GetConnectedUSBDevices();

            Guid hidGuid = Guid.Empty;

            //Retrieves the interface class GUID for the HID class.
            HidD_GetHidGuid(ref hidGuid);

            String[] devicePathName = new String[128];
            bool blRest = this.FindDeviceFromGuid(hidGuid, ref devicePathName);

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
                    if ((strDevicePath.Contains(strVID) == true) && (strDevicePath.Contains(strPID) == true)) //Matching
                    {
                        lststrRet.Add(strDevicePath);
                    }
                }
            }

            //
            return lststrRet;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal Boolean Write(Byte[] outputReportBuffer, SafeFileHandle writeHandle)
        {
            Int32 numberOfBytesWritten = 0;
            Boolean success = false;

            try
            {
                //  The host will use an interrupt transfer if the the HID has an interrupt OUT
                //  endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version). 
                //  Otherwise the the host will use a control transfer.
                //  The application doesn't have to know or care which type of transfer is used.

                numberOfBytesWritten = 0;

                //  ***
                //  API function: WriteFile

                //  Purpose: writes an Output report to the device.

                //  Accepts:
                //  A handle returned by CreateFile
                //  An integer to hold the number of bytes written.

                //  Returns: True on success, False on failure.
                //  ***

                success = WriteFile(writeHandle, outputReportBuffer, outputReportBuffer.Length, ref numberOfBytesWritten, IntPtr.Zero);


                if (!((success)))
                {

                    if ((!(writeHandle.IsInvalid)))
                    {
                        writeHandle.Close();
                    }
                }
                return success;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///  <summary>
        ///  Retrieves Input report data and status information.
        ///  This routine is called automatically when myInputReport.Read
        ///  returns. Calls several marshaling routines to access the main form.
        ///  </summary>
        ///  
        ///  <param name="ar"> an object containing status information about 
        ///  the asynchronous operation. </param>

        private void GetInputReportData(IAsyncResult ar)
        {
            String byteValue = null;
            Int32 count = 0;
            Byte[] inputReportBuffer = null;
            Boolean success = false;

            try
            {
                // Define a delegate using the IAsyncResult object.

                ReadInputReportDelegate deleg = ((ReadInputReportDelegate)(ar.AsyncState));

                //  Get the IAsyncResult object and the values of other paramaters that the
                //  BeginInvoke method passed ByRef.

                deleg.EndInvoke(ref myDeviceDetected, ref inputReportBuffer, ref success, ar);

                //  Display the received report data in the form's list box.

                if ((ar.IsCompleted && success))
                {
                    blUSBSendCmdResult = true;
                    intUSBCommandReceiveCode = inputReportBuffer[2] * 256 + inputReportBuffer[3];
                }

                blUSBSendCmdFinish = true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int McrUSB_WR(string strData, string strUSBpipeName, ref byte[] McrUsbReceiveDataReturn, int TimeUpRcv = 1000)
        {
            int i;
            string[] strUSBCmd;
            byte[] byteUSBCmd;
            bool success = false;

            try
            {
                //Analyze command
                strUSBCmd = strData.Split(',');

                byteUSBCmd = new byte[strUSBCmd.Length];
                //Convert to hexa format
                for (i = 0; i < strUSBCmd.Length; i++)
                {
                    if (byte.TryParse(strUSBCmd[i], System.Globalization.NumberStyles.HexNumber, null, out byteUSBCmd[i]) == false)
                    {
                        return 200; //Error: Cannot convert to numeric
                    }
                }
                //Check if command length is OK or not
                if ((byteUSBCmd.Length - 1) != byteUSBCmd[0])
                {
                    return 201; //Command length is NG
                }
                //Calculate Command code for check return later
                intUSBCommandSendingCode = byteUSBCmd[1] * 256 + byteUSBCmd[2];

                //Check if device is connected or not
                bool blDeviceConnect = isDeviceConnected(this.intVendorID, this.intProductID);
                if (blDeviceConnect == false) return 202; //Error: Device not connected

                //Create HID handle
                hidHandle = CreateFile(strUSBpipeName, 0, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, 0);

                if (hidHandle.IsInvalid == true) //If hidHandle create fail
                {
                    hidHandle.Close();
                    return 202; //Error: Cannot create HID Handle
                }

                //Learn the Capabilities of device
                if (blManualSetCapabilities == true)
                {
                    Capabilities.OutputReportByteLength = shrtOutputReportByteLength;
                    Capabilities.InputReportByteLength = shrtInputReportByteLength;
                }
                else
                {
                    Capabilities = GetDeviceCapabilities(hidHandle);
                }
                
                

                // Get handles to use in requesting Input and Output reports.
                readHandle = CreateFile(strUSBpipeName, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, 0);

                if (readHandle.IsInvalid == true) //If readHandle create fail
                {
                    readHandle.Close();
                    return 203; //Error: Cannot create Read Handle
                }

                //Get write handle
                writeHandle = CreateFile(strUSBpipeName, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, 0);
                //Flush any waiting reports in the input buffer. (optional)
                FlushQueue(readHandle);

                if (writeHandle.IsInvalid == true) //If readHandle create fail
                {
                    writeHandle.Close();
                    return 204; //Error: Cannot create Write Handle
                }

                //Don't attempt to send an Output report if the HID has no Output report.
                if (Capabilities.OutputReportByteLength <= 0)
                {
                    readHandle.Close(); writeHandle.Close();
                    return 205; //Error: HID has no Output report
                }

                //Attemp to write
                //byte[] outputReportBuffer = new byte[256];
                byte[] outputReportBuffer = null;
                Array.Resize(ref outputReportBuffer, Capabilities.OutputReportByteLength);

                //Store the report ID in the first byte of the buffer:
                outputReportBuffer[0] = 0;
                //for (i = 1; i < Capabilities.OutputReportByteLength; i++)
                //{
                //    outputReportBuffer[i] = byteUSBCmd[i-1];
                //}

                for (i = 1; i < byteUSBCmd.Length + 1; i++)
                {
                    outputReportBuffer[i] = byteUSBCmd[i - 1];
                }

                success = Write(outputReportBuffer, writeHandle);


                //  Don't attempt to send an Input report if the HID has no Input report.
                //  (The HID spec requires all HIDs to have an interrupt IN endpoint,
                //  which suggests that all HIDs must support Input reports.)
                if (Capabilities.InputReportByteLength <= 0)
                {
                    readHandle.Close(); writeHandle.Close();
                    return 206; //Error: HID has no Input report
                }

                Byte[] inputReportBuffer = null;
                Array.Resize(ref inputReportBuffer, Capabilities.InputReportByteLength);



                //  Read a report using interrupt transfers.   
                InputReportViaInterruptTransfer myInputReport = new InputReportViaInterruptTransfer();

                if (blUseAsyncRead == true)
                {
                                    
                        //  To enable reading a report without blocking the main thread, this
                        //  application uses an asynchronous delegate.
                        IAsyncResult ar = null;

                        //  Define a delegate for the Read method of myInputReport.
                        ReadInputReportDelegate MyReadInputReportDelegate = new ReadInputReportDelegate(myInputReport.Read);

                        //  The BeginInvoke method calls myInputReport.Read to attempt to read a report.
                        //  The method has the same parameters as the Read function,
                        //  plus two additional parameters:
                        //  GetInputReportData is the callback procedure that executes when the Read function returns.
                        //  MyReadInputReportDelegate is the asynchronous delegate object.
                        //  The last parameter can optionally be an object passed to the callback.

                        ar = MyReadInputReportDelegate.BeginInvoke(hidHandle, readHandle, writeHandle, ref myDeviceDetected, ref inputReportBuffer, ref success, new AsyncCallback(GetInputReportData), MyReadInputReportDelegate);

                        blUSBSendCmdFinish = false;
                        intUSBCommandReceiveCode = -1;
                        //Wait return data with default 1000 ms time out
                        int intStartTick = Environment.TickCount;

                        do
                        {
                            if ((Environment.TickCount - intStartTick) > TimeUpRcv) break;
                        }
                        while (intUSBCommandReceiveCode != intUSBCommandSendingCode);
                }
                else
                {
                    myInputReport.Read(hidHandle, readHandle, writeHandle, ref myDeviceDetected, ref inputReportBuffer, ref success);
                }

                //Calculate received command code
                intUSBCommandReceiveCode = inputReportBuffer[2] * 256 + inputReportBuffer[3];

                //Transfer data to McrUsbReceiveDataReturn array
                int intLen = McrUsbReceiveDataReturn.Length;
                if (inputReportBuffer.Length > intLen) intLen = inputReportBuffer.Length;
                McrUsbReceiveDataReturn = new byte[intLen]; //Clear old data

                if (intUSBCommandReceiveCode == intUSBCommandSendingCode) //Sending & Receive usb command is OK
                {
                    for (i = 1; i < inputReportBuffer.Length; i++)
                    {
                        McrUsbReceiveDataReturn[i - 1] = inputReportBuffer[i];
                    }
                }

                //Finally close all handle
                hidHandle.Close();
                readHandle.Close();
                writeHandle.Close();

                //
                if (intUSBCommandReceiveCode == intUSBCommandSendingCode)
                {
                    return 0; //OK code
                }
                else
                {
                    return 299; //NG code
                }
            }
            catch (Exception ex)
            {
                return -1; //Unexpected error code
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal abstract class ReportIn
        {
            ///  <summary>
            ///  Each class that handles reading reports defines a Read method for reading 
            ///  a type of report. Read is declared as a Sub rather
            ///  than as a Function because asynchronous reads use a callback method 
            ///  that can access parameters passed by ByRef but not Function return values.
            ///  </summary>

            internal abstract void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] readBuffer, ref Boolean success);
        }

        ///  <summary>
        ///  For reading Input reports.
        ///  </summary>

        internal class InputReportViaInterruptTransfer : ReportIn
        {
            ///  <summary>
            ///  closes open handles to a device.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>

            internal void CancelTransfer(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, IntPtr eventObject)
            {
                try
                {
                    //  ***
                    //  API function: CancelIo

                    //  Purpose: Cancels a call to ReadFile

                    //  Accepts: the device handle.

                    //  Returns: True on success, False on failure.
                    //  ***

                    CancelIo(readHandle);

                    //  The failure may have been because the device was removed,
                    //  so close any open handles and
                    //  set myDeviceDetected=False to cause the application to
                    //  look for the device on the next attempt.

                    if ((!(hidHandle.IsInvalid)))
                    {
                        hidHandle.Close();
                    }

                    if ((!(readHandle.IsInvalid)))
                    {
                        readHandle.Close();
                    }

                    if ((!(writeHandle.IsInvalid)))
                    {
                        writeHandle.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            ///  <summary>
            ///  Creates an event object for the overlapped structure used with ReadFile. 
            ///  </summary>
            ///  
            ///  <param name="hidOverlapped"> the overlapped structure </param>
            ///  <param name="eventObject"> the event object </param>

            internal void PrepareForOverlappedTransfer(ref NativeOverlapped hidOverlapped, ref IntPtr eventObject)
            {
                try
                {
                    //  ***
                    //  API function: CreateEvent

                    //  Purpose: Creates an event object for the overlapped structure used with ReadFile.

                    //  Accepts:
                    //  A security attributes structure or IntPtr.Zero.
                    //  Manual Reset = False (The system automatically resets the state to nonsignaled 
                    //  after a waiting thread has been released.)
                    //  Initial state = False (not signaled)
                    //  An event object name (optional)

                    //  Returns: a handle to the event object
                    //  ***

                    eventObject = CreateEvent(IntPtr.Zero, false, false, "");

                    //  Set the members of the overlapped structure.

                    hidOverlapped.OffsetLow = 0;
                    hidOverlapped.OffsetHigh = 0;
                    hidOverlapped.EventHandle = eventObject;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            ///  <summary>
            ///  reads an Input report from the device using interrupt transfers.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
            ///  <param name="inputReportBuffer"> contains the requested report. </param>
            ///  <param name="success"> read success </param>

            internal override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inputReportBuffer, ref Boolean success)
            {
                IntPtr eventObject = IntPtr.Zero;
                NativeOverlapped HidOverlapped = new NativeOverlapped();
                IntPtr nonManagedBuffer = IntPtr.Zero;
                IntPtr nonManagedOverlapped = IntPtr.Zero;
                Int32 numberOfBytesRead = 0;
                Int32 result = 0;

                try
                {
                    //  Set up the overlapped structure for ReadFile.

                    PrepareForOverlappedTransfer(ref HidOverlapped, ref eventObject);

                    // Allocate memory for the input buffer and overlapped structure. 

                    nonManagedBuffer = Marshal.AllocHGlobal(inputReportBuffer.Length);
                    nonManagedOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(HidOverlapped));
                    Marshal.StructureToPtr(HidOverlapped, nonManagedOverlapped, false);

                    //  ***
                    //  API function: ReadFile
                    //  Purpose: Attempts to read an Input report from the device.

                    //  Accepts:
                    //  A device handle returned by CreateFile
                    //  (for overlapped I/O, CreateFile must have been called with FILE_FLAG_OVERLAPPED),
                    //  A pointer to a buffer for storing the report.
                    //  The Input report length in bytes returned by HidP_GetCaps,
                    //  A pointer to a variable that will hold the number of bytes read. 
                    //  An overlapped structure whose hEvent member is set to an event object.

                    //  Returns: the report in ReadBuffer.

                    //  The overlapped call returns immediately, even if the data hasn't been received yet.

                    //  To read multiple reports with one ReadFile, increase the size of ReadBuffer
                    //  and use NumberOfBytesRead to determine how many reports were returned.
                    //  Use a larger buffer if the application can't keep up with reading each report
                    //  individually. 
                    //  ***                    

                    success = ReadFile(readHandle, nonManagedBuffer, inputReportBuffer.Length, ref numberOfBytesRead, nonManagedOverlapped);

                    if (!success)
                    {

                        //  API function: WaitForSingleObject

                        //  Purpose: waits for at least one report or a timeout.
                        //  Used with overlapped ReadFile.

                        //  Accepts:
                        //  An event object created with CreateEvent
                        //  A timeout value in milliseconds.

                        //  Returns: A result code.

                        result = WaitForSingleObject(eventObject, 3000);

                        //  Find out if ReadFile completed or timeout.

                        switch (result)
                        {
                            case (System.Int32)WAIT_OBJECT_0:

                                //  ReadFile has completed

                                success = true;

                                // Get the number of bytes read.

                                //  API function: GetOverlappedResult

                                //  Purpose: gets the result of an overlapped operation.

                                //  Accepts:
                                //  A device handle returned by CreateFile.
                                //  A pointer to an overlapped structure.
                                //  A pointer to a variable to hold the number of bytes read.
                                //  False to return immediately.

                                //  Returns: non-zero on success and the number of bytes read.	

                                GetOverlappedResult(readHandle, nonManagedOverlapped, ref numberOfBytesRead, false);

                                break;

                            case WAIT_TIMEOUT:

                                //  Cancel the operation on timeout

                                CancelTransfer(hidHandle, readHandle, writeHandle, eventObject);
                                success = false;
                                myDeviceDetected = false;
                                break;
                            default:

                                //  Cancel the operation on other error.
                                CancelTransfer(hidHandle, readHandle, writeHandle, eventObject);
                                success = false;
                                myDeviceDetected = false;
                                break;
                        }

                    }
                    if (success)
                    {
                        // A report was received.
                        // Copy the received data to inputReportBuffer for the application to use.

                        Marshal.Copy(nonManagedBuffer, inputReportBuffer, 0, numberOfBytesRead);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        } 

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public clsGeneralUsbHid()
        {
            this.intVendorID = 0x04D8;
            this.intProductID = 0x0020;

            this.strPipeName = "";

            blManualSetCapabilities = true;
            shrtOutputReportByteLength = 18;
            shrtInputReportByteLength = 18;

            blUseAsyncRead = false;
        }



    }
}
