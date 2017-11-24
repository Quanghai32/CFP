/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////                                            ///////////////////////////////////////////
/////////////////////////////////////////////                                              ///////////////////////////////////////////       
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                                ///////////////////////////////////////////
/////////////////////////////////////////////                                              ///////////////////////////////////////////
/////////////////////////////////////////////                                            ///////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  INTERFACE CLASS
//  Desc: This class, using to create interface between each module of program. Re-structure for checker frame program to support
//        unkown features in the future!
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspINTERFACE
{
    public interface IPluginInfo
    {
        string IPluginInfo { get; } // "IPluginInfo" this name of method must be the same with 1st data of meatadata of exports
    }

    public interface IPluginExecute
    {
        /// <summary>
        /// IGetPluginInfo() : Get plugin Info - What JigID, HardID, FuncID supported
        /// Output: List of string descript plugin Info. Example:
        ///     + "100,0,0,1,2,3,4" : support (JigID=100 & HardID=0) with FuncID support (0,1,2,3,4)
        ///     + "101,0,0,1,2,3,4" : support (JigID=101 & HardID=0) with FuncID support (0,1,2,3,4) ...
        /// </summary>
        /// <param name="lststrInfo"></param>
        void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo);

        /// <summary>
        /// IFunctionExecute(): Execute functions plugin supported
        ///     + Input: parameters under list of string format
        ///         "JigID, HardID, FuncID, ProcessID, Para 1, Para 2, .... ,Para n"
        ///     + Output: checking result data under list of string format 
        /// </summary>
        /// <param name="lststrInput"></param>
        /// <param name="lststrOutput"></param>
        /// <returns></returns>
        object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput);
    }

}
