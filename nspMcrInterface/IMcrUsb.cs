using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspMcrInterface
{
    public interface IPluginMcrInfo
    {
        string IPluginMcrInfo { get; } // "IPluginMcrInfo" this name of method must be the same with 1st data of meatadata of exports
    }

    public interface IMcrUsb
    {
        void SetPipeName(string strPipe);
        int McrUSB_WR(string strUsbCommand,ref byte[] byteMcrReceiveData, int intTimeOut);
    }
}
