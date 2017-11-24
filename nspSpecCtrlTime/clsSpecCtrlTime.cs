using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;

namespace nspSpecCtrlTime
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,Time")]
    public class clsSpecCtrlTime : nspINTERFACE.IPluginExecute
    {
        public List<clsSettingForCommand> lstSettingCommand; //contain all supported special command

        #region _Interface_implement

        /// <summary>
        /// The module will inform to Host (Frame) about what special control supported
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjInfo"></param>
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Ini for list of supported special control
            IniSpecialControl();

            //Inform to Host program which Special control Function this plugin support
            strTemp = "special,"; //key for searching

            foreach (clsSettingForCommand clsElement in lstSettingCommand)
            {
                strTemp = strTemp + clsElement.strDetectCode + ",";
            }
            lstobjInfo.Add(strTemp);

            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.00"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 30/May/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, building Time function for special control"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);

            //Adding list setting command to output
            lstobjInfo = new List<object>();
            lstobjInfo.Add("lstSettingCommand");
            lstobjInfo.Add(this.lstSettingCommand);

            lstlstobjInfo.Add(lstobjInfo);
        }

        /// <summary>
        /// The Host (Frame) will ask for execute special control and get result return
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            List<object> lstobjTemp = new List<object>();

            //searching input parameter for command guider
            bool blFound = false;
            int i = 0;
            int j = 0;
            for (i = 0; i < lstlstobjInput.Count; i++)
            {
                lstobjTemp = lstlstobjInput[i];

                for (j = 0; j < lstobjTemp.Count; j++)
                {
                    if (lstobjTemp.Count < 2) continue; //"SPECIAL" - (clsCommonCommandGuider)

                    if (lstobjTemp[0].ToString().Trim().ToUpper() == "SPECIAL") //Found
                    {
                        //If matching, then confirm the second object is clsCommonCommandGuider or not
                        if (lstobjTemp[1] is clsCommonCommandGuider)
                        {
                            blFound = true;
                            break;
                        }
                    }
                }

                if (blFound == true) break;
            }

            if (blFound == false) return "Error: cannot find matching special control request!";

            //If command found, then executed coressponding special control
            clsCommonCommandGuider clsInput = (clsCommonCommandGuider)lstobjTemp[1];
            object objResult = ExecuteSpecialFunction(clsInput);

            return objResult;
        }

        #endregion


        public void IniSpecialControl()
        {
            lstSettingCommand = new List<clsSettingForCommand>();

            //Time Function
            lstSettingCommand.Add(new clsSettingForCommand("TICK", 0, 0)); //1
            lstSettingCommand.Add(new clsSettingForCommand("TIME", 0, 0)); //2
            lstSettingCommand.Add(new clsSettingForCommand("WAIT", 2, 0)); //3
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "TICK":
                    return cmdTICK(clsInput);
                case "TIME":
                    return cmdTIME(clsInput);
                case "WAIT":
                    return cmdWAIT(clsInput);
                default:
                    return "PluginSpecialControl,Time - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdTICK(clsCommonCommandGuider clsInput)
        {
            return MyLibrary.clsApiFunc.GetTickCount();
        }

        //****************************************************************************************************************
        public object cmdTIME(clsCommonCommandGuider clsInput)
        {
            string strOption = "";
            if (clsInput.args.Parameters != null)
            {
                if (clsInput.args.Parameters.Length != 0)
                {
                    //return "TIME() Error: not enough parameter";
                    strOption = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
                }
            }
            

            DateTime now = DateTime.Now;

            return now.ToString(strOption);
        }

        //****************************************************************************************************************
        public object cmdWAIT(clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length != 2) return "WAIT() Error: only valid if input 2 parameter";

            int intStartTick = 0;
            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStartTick) == false)
            {
                return "WAIT() Error: Start Tick input [" + strTemp + "] is not integer!";
            }

            int intWaitTick = 0;
            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intWaitTick) == false)
            {
                return "WAIT() Error: Wait Tick input [" + strTemp + "] is not integer!";
            }

            while ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) < intWaitTick)
            {
                ;//Do nothing, just keep polling until reaching waiting time request
            }

            return 0; //Return ok code
        }
        //****************************************************************************************************************


    }
}
