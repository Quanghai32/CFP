using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspMainUsbInterface
{
    public interface IPluginMainUsbInfo
    {
        string IPluginMainUsbInfo { get; } // "IPluginMainUsbInfo" this name of method must be the same with 1st data of meatadata of exports
    }

    public interface IMainUsb
    {
        void SetPipeName(string strPipe); //Set pipe name for class holding Main USB communication
        int UnLockMainPCB(string strModeinExePath, int msTimeOut = 10000, bool blAutoFindPipe = false, string strProductID = ""); //Unlock Main PCB
        int USBWrite(string strData, int WaitTM = 5000); //Write command to Main PCB but do not receive return data
        int USB_DR(string MainCmd, string SubCmd, out byte[] byteMainUsbReceiveData, long Rnum = 500, uint RCVTM = 500); //Receive data from Main PCB
        int USB_WR(string strData, out byte[] byteMainUsbReceiveData, long Rnum = 500, int SNDTM = 5000, uint RCVTM = 5000); //Write command to Main PCB & reading return data
    }
}
