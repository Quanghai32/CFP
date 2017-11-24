using nspCFPExpression;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspWebApiHandle
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,WebApiHandle")]
    public class WebApiHandleExpression : nspINTERFACE.IPluginExecute
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
            strTemp = "Date, 5/Jul/2017"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For Access Secured Web API"; lstobjInfo.Add(strTemp);

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
            lstSettingCommand.Add(new clsSettingForCommand("NEWWEBAPI", 0, 0));
            lstSettingCommand.Add(new clsSettingForCommand("NEWWEBAPIBACKUP", 2)); //NewWebApiBackup
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "NEWWEBAPI":
                    return NEWWEBAPI(clsInput);
                case "NEWWEBAPIBACKUP":
                    return NEWWEBAPIBACKUP(clsInput);
                default:
                    return "PluginSpecialControl,WebApiHandle - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object NEWWEBAPI(clsCommonCommandGuider clsInput) //Just create new class
        {
            if (clsInput.args.Parameters.Length < 3) return "NEWWEBAPI() Error: not enough 3 parameter.";
            string apiBaseUri = clsInput.args.Parameters[0].Evaluate().ToString();
            string UserName = clsInput.args.Parameters[1].Evaluate().ToString();
            string Password = clsInput.args.Parameters[2].Evaluate().ToString();

            var myWebApi = new AccessSecuredWebApi.AccessWebApi(apiBaseUri, UserName, Password);
            return myWebApi;
        }

        //*****************************************************************************************************************************
        ///// <summary>
        ///// Param1: UriBase (http://localhost:58314)
        ///// Param2: RequestPath ("/api/ProductModels")
        ///// Param3: Query Params (List of String)
        ///// </summary>
        ///// <param name="clsInput"></param>
        ///// <returns></returns>
        //public object WEBAPIGET(clsCommonCommandGuider clsInput) //Just create new class
        //{
        //    if (clsInput.args.Parameters.Length < 2) return "Error: Not enough parameter input";
        //    string strBasedUri = clsInput.args.Parameters[0].Evaluate().ToString();
        //    string strPath = clsInput.args.Parameters[1].Evaluate().ToString();

        //    //List<string> lstParams = new List<string>();
        //    //for(int i=2;i< clsInput.args.Parameters.Length;i++)
        //    //{
        //    //    lstParams.Add(clsInput.args.Parameters[i].Evaluate().ToString());
        //    //}
        //    List<string> lstQueryParams = new List<string>();

        //    if (clsInput.args.Parameters.Length>2)
        //    {
        //        var objParams = clsInput.args.Parameters[2].Evaluate();
        //        if (!(objParams is List<object>)) return "Error: Query Parameter invalid!";
        //        List<object> lstParams = (List<object>)objParams;
        //        foreach (var param in lstParams)
        //        {
        //            lstQueryParams.Add(param.ToString());
        //        }
        //    }

        //    var ret = this.myWebApi.GetApiCall(strPath, lstQueryParams);
        //    return ret;
        //}

        //*****************************************************************************************************************************
        public object NEWWEBAPIBACKUP(clsCommonCommandGuider clsInput) //Just create new class
        {
            if (clsInput.args.Parameters.Length < 5) return "Error: Not enough parameter input";
            string strInfoName = clsInput.args.Parameters[0].Evaluate().ToString();
            string strBasedUri = clsInput.args.Parameters[1].Evaluate().ToString();
            string strPath = clsInput.args.Parameters[2].Evaluate().ToString();
            string strInterval = clsInput.args.Parameters[4].Evaluate().ToString();

            //Check interval
            int intInterval = 0;
            if (int.TryParse(strInterval, out intInterval) == false) return "Error: Interval setting is not integer!";

            //Check if global setting is using hostwebsite or not
            var state = nspAppStore.clsAppStore.GetCurrentState();
            //if(state.hostWebsite.UsingHostWebsite==false)
            //{
            //    return "Error: Current Setting no use Host Website";
            //}

            var objParams = clsInput.args.Parameters[3].Evaluate();
            if (!(objParams is List<object>)) return "Error: Query Parameter invalid!";
            List<object> lstParams = (List<object>)objParams;

            //Request start Host web service with interval time GET()
            var hostWebService = (HostWebsiteService.HostWebService)state.hostWebsite.objHostWebService;
            hostWebService.newWebApiBackup(strInfoName, strBasedUri, strPath, lstParams, intInterval);

            return 0;
        }

    }
}
