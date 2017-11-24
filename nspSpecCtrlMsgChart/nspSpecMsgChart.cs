using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;
using nspINTERFACE;
using nspCFPExpression;

namespace nspSpecMsgChart
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,MsgChart")]
    public class nspSpecMsgChart : nspINTERFACE.IPluginExecute
    {
        public List<clsSettingForCommand> lstSettingCommand; //contain all supported special command

        private nspGeneralChart.clsGeneralChart clsMyChart;

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
            strTemp = "Date, 7/Sep/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, For General Message & Chart control"; lstobjInfo.Add(strTemp);

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
            lstSettingCommand.Add(new clsSettingForCommand("NEWMSG", 0, 0)); //1
            lstSettingCommand.Add(new clsSettingForCommand("PMSG", 0, 0)); //2

            lstSettingCommand.Add(new clsSettingForCommand("INPUT", 0, 0));

            lstSettingCommand.Add(new clsSettingForCommand("NEWCHART", 0, 0)); //1

            lstSettingCommand.Add(new clsSettingForCommand("NEWSETTING", 3, 0)); //
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "NEWMSG":
                    return cmdNEWMSG(clsInput);
                case "PMSG":
                    return cmdPMSG(clsInput);
                case "INPUT":
                    return cmdINPUT(clsInput);
                case "NEWCHART":
                    return cmdNEWCHART(clsInput);
                case "NEWSETTING":
                    return cmdNEWSETTING(clsInput);
                default:
                    return "PluginSpecialControl,MsgChart - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdNEWMSG(clsCommonCommandGuider clsInput) //Just create new class
        {
            // NEWMSG()
            clsGeneralMessage clsGeneralMSG = this.StartNewMessage(clsInput);
            //
            return clsGeneralMSG;
        }

        //*****************************************************************************************************************************
        public object cmdPMSG(clsCommonCommandGuider clsInput) 
        {
            //Push out a general message form - Create new General class & Show out window
            // PMSG(MessageContent, Message Title)

            clsGeneralMessage clsGeneralMSG = this.StartNewMessage(clsInput);
            clsGeneralMSG.Show();
            //
            return clsGeneralMSG;
        }

        private clsGeneralMessage StartNewMessage(clsCommonCommandGuider clsInput)
        {
            //Push out a general message form - Create new General class & Show out window
            // PMSG(MessageContent, Message Title)

            clsGeneralMessage clsGeneralMSG = new clsGeneralMessage();

            if (clsInput.args.Parameters.Length > 0) //At least 1 parameter (message content)
            {
                //Message content
                clsGeneralMSG.strMsgContent = clsInput.args.Parameters[0].Evaluate().ToString();

                //Message Title
                if (clsInput.args.Parameters.Length > 1) //content message title
                {
                    clsGeneralMSG.strMsgTitle = clsInput.args.Parameters[1].Evaluate().ToString();
                }
            }

            clsGeneralMSG.IniGeneralMSG();

            //Check Timer Count down option
            if (clsInput.args.Parameters.Length > 2) //
            {
                int intTimeOut = 0;
                if (int.TryParse(clsInput.args.Parameters[2].Evaluate().ToString().Trim(), out intTimeOut) == true)
                {
                    clsGeneralMSG.StartCounDownTimer(intTimeOut);
                }
            }

            //
            return clsGeneralMSG;
        }


        //*****************************************************************************************************************************
        public object cmdINPUT(clsCommonCommandGuider clsInput)
        {
            //Push out a general message Input & 
            // INPUT(Message Info, Message Title, Barcode Mode)
            // Return: What user input
            //

            string strRequestMessage = "";
            string strTitle = "";
            string strOption = "";

            if(clsInput.args.Parameters != null)
            {
                if (clsInput.args.Parameters.Length != 0) strRequestMessage = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
                if (clsInput.args.Parameters.Length > 1) strTitle = clsInput.args.Parameters[1].Evaluate().ToString().Trim();
                if (clsInput.args.Parameters.Length > 2) strOption = clsInput.args.Parameters[2].Evaluate().ToString().Trim();
            }

            //Assign user selection
            clsGeneralInput clsNewInput = new clsGeneralInput();
            clsNewInput.IniGeneralInput();

            if (strRequestMessage.Trim() != "") clsNewInput.wdInput.strRequestInfo = strRequestMessage;
            if(strTitle.Trim() != "") clsNewInput.wdInput.strTitle = strTitle;
            clsNewInput.wdInput.strOption = strOption;


            clsNewInput.ShowDialog();

            //Wait until Finish input data
            while (clsNewInput.wdInput.blFinishInput == false)
            {
                ;
            }

            //Return value
            return clsNewInput.wdInput.strUserInput;
        }

        //*****************************************************************************************************************************
        public object cmdNEWCHART(clsCommonCommandGuider clsInput) //Just create new class
        {
            //NEWCHART(Option)
            // - Option = 0: Only create new chart if not yet initialized - Default
            // - Option = 1: Always create new chart

            try
            {
                int intOption = 0;
                if (clsInput.args.Parameters.Length > 0)
                {
                    if (int.TryParse(clsInput.args.Parameters[0].Evaluate().ToString(), out intOption) == false)
                    {
                        intOption = 0; //Default Val
                    }
                }

                if (intOption == 1) //Always create new chart class
                {
                    //Before create new chart, we need dispose old one
                    if (this.clsMyChart != null)
                    {
                        //Try to close window chart
                        if (this.clsMyChart.windowChart != null)
                        {
                            this.clsMyChart.windowChart.Dispatcher.Invoke(new Action(() => this.clsMyChart.windowChart.Close()));
                        }
                        //Destroy old data
                        this.clsMyChart = null;
                    }
                }
                else //Default option: only create new chart if not initialize
                {
                    if (this.clsMyChart != null)
                    {
                        if (this.clsMyChart.windowChart != null)
                        {
                            return this.clsMyChart;
                        }
                    }
                }

                //Ini for Graph
                this.clsMyChart = new nspGeneralChart.clsGeneralChart();
                this.clsMyChart.StartChart();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            //
            return this.clsMyChart;
        }

        //*****************************************************************************************************************************
        public object cmdNEWSETTING(clsCommonCommandGuider clsInput) //Just create new class
        {
            //NEWSETTING(X1,X2,X3,X4,X5...)
            //X1: Name of Setting
            //X2: Path of setting file
            //X3: Section name of setting file will be use
            //X4,X5,X6...: User key name in setting file - Key1, Key2, Key3...

            if (clsInput.args.Parameters.Length < 3) return "cmdNEWSETTING() Error: Not enough parameter";
            
            //Analyzing User Setting input
            string strSettingName = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            string strFilePath = clsInput.args.Parameters[1].Evaluate().ToString().Trim();
            string strSectionName = clsInput.args.Parameters[2].Evaluate().ToString().Trim();

            //
            clsGeneralSetting clsNewSetting = new clsGeneralSetting();

            clsNewSetting.strSettingName = strSettingName;
            clsNewSetting.strSettingFilePath = strFilePath;
            clsNewSetting.strSettingSection = strSectionName;

            //
            int i = 0;
            if (clsInput.args.Parameters.Length > 3)
            {
                for (i = 3; i < clsInput.args.Parameters.Length; i++)
                {
                    clsUserSettingData clsTemp = new clsUserSettingData();
                    clsTemp.strUserSettingName = clsInput.args.Parameters[i].Evaluate().ToString();
                    clsNewSetting.lstclsUserSetting.Add(clsTemp);
                }
            }

            //
            clsNewSetting.IniGeneralInput();

            //
            return clsNewSetting;
        }
        //****************************************************************************************************************

    }
}
