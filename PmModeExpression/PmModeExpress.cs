using nspCFPExpression;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspPmModeExpress
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,PmModeExpress")]
    public class PmModeExpress : nspINTERFACE.IPluginExecute
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
            strTemp = "Version, 0.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 4/Jul/2017"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For PM Check Mode"; lstobjInfo.Add(strTemp);

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

            //Function List
            lstSettingCommand.Add(new clsSettingForCommand("NEWPMMODE", 0, 0)); //1
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "NEWPMMODE":
                    return NEWPMMODE(clsInput);
                default:
                    return "PluginSpecialControl,MsgChart - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object NEWPMMODE(clsCommonCommandGuider clsInput) //Just create new class
        {
            var newClass = new PmModeControl();
            //newClass.showMe();
            newClass.IniMe();
            return newClass;
        }

        //*****************************************************************************************************************************
    }
    
}
