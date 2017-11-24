using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;
using nspMasterProcessModel;
using nspChildProcessModel;


namespace nspSpecCtrlReturn
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,Return")]
    public class clsSpecCtrlReturn : nspINTERFACE.IPluginExecute
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
            strTemp = "Note, building Return function for special control"; lstobjInfo.Add(strTemp);

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
            object objResult = ExecuteSpecialFunction(ref clsInput);

            return objResult;
        }

        #endregion


        public void IniSpecialControl()
        {
            lstSettingCommand = new List<clsSettingForCommand>();

            //Return Function
            lstSettingCommand.Add(new clsSettingForCommand("RET", 0)); //1
            lstSettingCommand.Add(new clsSettingForCommand("TRANS", 2, 0)); //2
            lstSettingCommand.Add(new clsSettingForCommand("USERRET", 3, 0)); //3
            lstSettingCommand.Add(new clsSettingForCommand("GETSERI", 3, 0)); //4
            lstSettingCommand.Add(new clsSettingForCommand("GETLIST", 2, 0)); //5
        }

        public object ExecuteSpecialFunction(ref clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "RET":
                    return cmdRET(ref clsInput);
                case "TRANS":
                    return cmdTRANS(ref clsInput);
                case "USERRET":
                    return cmdUSERRET(ref clsInput);
                case "GETSERI":
                    return cmdGETSERI(ref clsInput);
                case "GETLIST":
                    return cmdGETLIST(ref clsInput);
                default:
                    return "PluginSpecialControl,Return - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdRET(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdRETMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdRETChildProcess(ref clsInput);
                default:
                    return "cmdRET() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdRETMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRETMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters == null) return "cmdRETMasterProcess() error: null parameter";
            if (clsInput.args.Parameters.Length > 1) return "cmdRETMasterProcess() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";
            
            int intTargetStepPos = 0;
            string parameter = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if (parameter == "") //Get current step result
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else //Get another step result
            {
                string strTemp = "";
                int intStepRet = 0;
                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intStepRet) == false)
                {
                    return "cmdRETMasterProcess() Error: Parameter for RET function is not integer: " + strTemp;
                }

                //Check if step number want to return data is exist in step list or not
                intTargetStepPos = clsMasterProcess.FindMasterStepPos(intStepRet);
                if (intTargetStepPos == -1)
                {
                    return "cmdRETMasterProcess() Error: No found step " + intStepRet.ToString() + " in step list";
                }
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            return clsMasterProcess.lstTotalStep[intTargetStepPos].objStepCheckingData;
        }

        public object cmdRETChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdRETChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            int intStepRet = 0;
            int intTargetStepPos = 0;
            

            if (clsInput.args.Parameters == null) return "cmdRETChildProcess() error: null parameter";
            if (clsInput.args.Parameters.Length > 1) return "cmdRETChildProcess() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            string parameter = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if(parameter=="") //Get current step result
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else //Get another step result
            {
                string strTemp = "";
                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intStepRet) == false)
                {
                    return "Parameter for RET function is not numeric: " + strTemp;
                }

                //Check if step number want to return data is exist in step list or not
                intTargetStepPos = clsChildProcess.FindCheckingStepPos(intStepRet);

                if (intTargetStepPos == -1)
                {
                    return "cmdRETChildProcess() Error: No found step " + strTemp.ToString() + " in step list";
                }
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            return clsChildProcess.lstTotalStep[intTargetStepPos].objStepCheckingData;
        }
        
        //*****************************************************************************************************************************
        public string cmdTRANS(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdTRANSMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdTRANSChildProcess(ref clsInput);
                default:
                    return "cmdTRANS() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public string cmdTRANSMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRETMasterProcess: Not class acceptable!";

            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //Function Desc
            //  - strStepRet: The number os step want to take transmission data from
            //  - intStepPos: The current step position which is running
            //  *Return: return value of target step
            int intStepRet = 0;
            int intTargetStepPos = 0;
            int intOption = 0;
            string strTemp = "";

            if (clsInput.args.Parameters == null) return "cmdRETMasterProcess() error: null parameter";
            if (clsInput.args.Parameters.Length != 2) return "cmdRETMasterProcess() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for TRANS function is not numeric: " + strTemp;
            }

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intOption) == false)
            {
                return "Option parameter is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsMasterProcess.FindMasterStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "TRANS() function NG: No found step " + intStepRet.ToString() + " in step list";
            }

            //Return value
            if (intOption == 1)
            {
                return clsMasterProcess.lstTotalStep[intTargetStepPos].strTransmisstionEx; //Return transmission value
            }
            else //Default is 0 & return origin
            {
                return clsMasterProcess.lstTotalStep[intTargetStepPos].strTransmisstion; //Return transmission already executed value
            }
        }

        public string cmdTRANSChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdTRANSChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;


            if (clsInput.args.Parameters == null) return "cmdTRANSChildProcess() error: null parameter";
            if (clsInput.args.Parameters.Length != 2) return "cmdTRANSChildProcess() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            //Function Desc
            int intStepRet = 0;
            int intTargetStepPos = 0;
            int intOption = 0;
            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for TRANS function is not numeric: " + strTemp;
            }

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intOption) == false)
            {
                return "Option parameter is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsChildProcess.FindCheckingStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "TRANS() function NG: No found step " + intStepRet.ToString() + " in step list";
            }

            //Return value
            if (intOption == 1)
            {
                return clsChildProcess.lstTotalStep[intTargetStepPos].strTransmisstionEx; //Return transmission value which is already done special command if exist
            }
            else
            {
                return clsChildProcess.lstTotalStep[intTargetStepPos].strTransmisstion; //Return origin transmission value
            }
        }
        
        //*****************************************************************************************************************************
        public object cmdUSERRET(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdUSERRETMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdUSERRETChildProcess(ref clsInput);
                default:
                    return "cmdUSERRET() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdUSERRETMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            //UserRet(McrUsb,4,2)
            //Function Desc
            //  +strUserDesc: string to describe what need to looking for to return. Ex: "McrUsb" , "MainUsb", "DumMainUsb"... this come from user plug-in
            //  +strStepRet: the step number want to get return value
            //  +strPosition: the position of data in list of user data want to get return. Count from Zero.
            //  +intStepPos: the position of step in all step which are running, count from Zero
            //  *Return: return value of target step

            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRETMasterProcess: Not class acceptable!";

            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 3) return "cmdUSERRETMasterProcess() Error: not enough parameter";
            if (clsInput.args.Parameters.Length != 3) return "cmdUSERRETMasterProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";



            int intStepRet = 0;
            int intTargetStepPos = 0;
            object objRet = "";
            string strTemp = "";

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for USERRET function is not numeric: " + strTemp;
            }


            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsMasterProcess.FindMasterStepPos(intStepRet);
            if (intTargetStepPos == -1)
            {
                return "Master steplist USERRET() function NG: No found step " + intStepRet.ToString() + " in step list";
            }

            //Check position to get bit
            int intPos = 0;
            strTemp = clsInput.args.Parameters[2].Evaluate().ToString();
            if (int.TryParse(strTemp, out intPos) == false)
            {
                return "Master steplist USERRET() error: Second parameter input [" + strTemp + "] is not integer format!";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsMasterProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Found
                {
                    if ((lstTemp.Count - 1) < (intPos + 1)) //error
                    {
                        return "error";
                    }
                    objRet = lstTemp[intPos + 1]; //Not counting lstTemp[0] : description for list of return data
                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "cmdUSERRETMasterProcess() error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return objRet;
        }

        public object cmdUSERRETChildProcess(ref clsCommonCommandGuider clsInput)
        {
            //UserRet(McrUsb,4,2)
            //Function Desc
            //  +strUserDesc: string to describe what need to looking for to return. Ex: "McrUsb" , "MainUsb", "DumMainUsb"... this come from user plug-in
            //  +strStepRet: the step number want to get return value
            //  +strPosition: the position of data in list of user data want to get return. Count from Zero.
            //  +intStepPos: the position of step in all step which are running, count from Zero
            //  +intParamOrder: the position of executing parameter in all parameter of step which are running
            //  *Return: return value of target step

            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdTRANSChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 3) return "cmdUSERRETChildProcess() Error: not enough parameter";
            if (clsInput.args.Parameters.Length != 3) return "cmdUSERRETChildProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            int intStepRet = 0;
            int intTargetStepPos = 0;
            object objRet = "";
            string strTemp = "";

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for USERRET function is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsChildProcess.FindCheckingStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "USERRET() function NG: No found step " + intStepRet.ToString() + " in step list";
            }

            //Check position to get data
            int intPos = 0;
            strTemp = clsInput.args.Parameters[2].Evaluate().ToString();
            if (int.TryParse(strTemp, out intPos) == false)
            {
                return "USERRET() error: Second parameter input [" + strTemp + "] is not integer format!";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsChildProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Found
                {
                    if ((lstTemp.Count - 1) < (intPos + 1)) //error
                    {
                        return "error";
                    }
                    objRet = lstTemp[intPos + 1]; //Not counting lstTemp[0] : description for list of return data
                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "cmdUSERRETChildProcess Error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return objRet;
        }

        //*****************************************************************************************************************************
        public object cmdGETSERI(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdGETSERIMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdGETSERIChildProcess(ref clsInput);
                default:
                    return "cmdGETSERI() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdGETSERIMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRETMasterProcess: Not class acceptable!";

            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length != 3) return "cmdGETSERIMasterProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            int intStepRet = 0;
            int intTargetStepPos = 0;
            string strRet = "";
            string strTemp = "";
            string strSeparator = ",";

            strSeparator = clsInput.args.Parameters[2].Evaluate().ToString();
            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for GETSERI function is not numeric: " + strTemp;
            }


            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsMasterProcess.FindMasterStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "cmdGETSERIMasterProcess() Error: No found step " + intStepRet.ToString() + " in step list";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsMasterProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Matching user description
                {
                    if (lstTemp.Count < 2) return "Error: there is no seri element";

                    int i = 0;
                    strRet = "";
                    for (i = 1; i < lstTemp.Count; i++) //Create seri string
                    {
                        if (i != (lstTemp.Count - 1))
                        {
                            strRet = strRet + lstTemp[i].ToString() + strSeparator;
                        }
                        else //The last one - No adding separator
                        {
                            strRet = strRet + lstTemp[i].ToString();
                        }
                    }

                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "Error: " + "GETSERI() error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return strRet;
        }

        public object cmdGETSERIChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdGETSERIChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length != 3) return "cmdGETSERIChildProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";


            int intStepRet = 0;
            int intTargetStepPos = 0;
            string strRet = "";
            string strTemp = "";
            string strSeparator = ",";

            strSeparator = clsInput.args.Parameters[2].Evaluate().ToString();
            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for GETSERI function is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsChildProcess.FindCheckingStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "cmdGETSERIChildProcess() Error: No found step " + intStepRet.ToString() + " in step list";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsChildProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Matching user description
                {
                    if (lstTemp.Count < 2) return "Error: there is no seri element";

                    int i = 0;
                    strRet = "";
                    for (i = 1; i < lstTemp.Count; i++) //Create seri string
                    {
                        if (i != (lstTemp.Count - 1))
                        {
                            strRet = strRet + lstTemp[i].ToString() + strSeparator;
                        }
                        else //The last one - No adding separator
                        {
                            strRet = strRet + lstTemp[i].ToString();
                        }
                    }

                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "cmdGETSERIChildProcess() error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return strRet;
        }
        
        //*****************************************************************************************************************************
        public object cmdGETLIST(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdGETLISTMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdGETLISTChildProcess(ref clsInput);
                default:
                    return "cmdGETLIST() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdGETLISTMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdGETLISTMasterProcess: Not class acceptable!";

            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length != 2) return "cmdGETLISTMasterProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            int intStepRet = 0;
            int intTargetStepPos = 0;
            List<object> lstobjRet = new List<object>();
            string strTemp = "";

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for cmdGETLISTMasterProcess function is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsMasterProcess.FindMasterStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "cmdGETLISTMasterProcess() Error: No found step " + intStepRet.ToString() + " in step list";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsMasterProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Matching user description
                {
                    if (lstTemp.Count < 2) return "cmdGETLISTMasterProcess Error: there is no element";

                    int i = 0;
                    for (i = 1; i < lstTemp.Count; i++) //
                    {
                        lstobjRet.Add(lstTemp[i]);
                    }

                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "cmdGETLISTMasterProcess() error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return lstobjRet;
        }

        public object cmdGETLISTChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdGETLISTChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 2) return "cmdGETLISTChildProcess() Error: not enough parameter";
            if (clsInput.args.Parameters.Length != 2) return "cmdGETLISTChildProcess() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            int intStepRet = 0;
            int intTargetStepPos = 0;
            List<object> lstobjRet = new List<object>();
            string strTemp = "";

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intStepRet) == false)
            {
                return "Parameter for cmdGETLISTChildProcess function is not numeric: " + strTemp;
            }

            //Check if step number want to return data is exist in step list or not
            intTargetStepPos = clsChildProcess.FindCheckingStepPos(intStepRet);

            if (intTargetStepPos == -1)
            {
                return "cmdGETLISTChildProcess() Error: No found step " + intStepRet.ToString() + " in step list";
            }

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            foreach (List<object> lstTemp in clsChildProcess.lstTotalStep[intTargetStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper().Trim() == strTemp.ToUpper().Trim()) //Matching user description
                {
                    if (lstTemp.Count < 2) return "cmdGETLISTChildProcess() error: there is no seri element";

                    int i = 0;
                    for (i = 1; i < lstTemp.Count; i++) //Create seri string
                    {
                        lstobjRet.Add(lstTemp[i]);
                    }

                    blFlagFound = true;
                    break;
                }
            }

            if (blFlagFound == false)
            {
                return "cmdGETLISTChildProcess() error: cannot find any list of data return have description: " + strTemp + "!";
            }

            return lstobjRet;
        }
        

        //*****************************************************************************************************************************
    }
}
