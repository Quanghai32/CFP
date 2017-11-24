using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;
using nspMasterProcessModel;
using nspChildProcessModel;
using nspSingleThreadProcessModel;
using nspProgramList;
using System.Windows;
using nspCFPInfrastructures;

namespace nspSpecCtrlSysSequence
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,SysSequence")]
    public class clsSpecCtrlSysSequence : nspINTERFACE.IPluginExecute
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
            strTemp = "Note, building SysSequence function for special control"; lstobjInfo.Add(strTemp);

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
            clsSettingForCommand clsTemp;

            //System sequence Function
            lstSettingCommand.Add(new clsSettingForCommand("THIS", 0, 0)); //1
            lstSettingCommand.Add(new clsSettingForCommand("JUMP", 1, 0)); //1

            clsTemp = new clsSettingForCommand("FDONE", 1, 1);
            clsTemp.intTargetProcessID = 3; //Target for single thread process only
            lstSettingCommand.Add(clsTemp); //5

            clsTemp = new clsSettingForCommand("SFDONE", 1, 1);
            clsTemp.intTargetProcessID = 3; //Target for single thread process only
            lstSettingCommand.Add(clsTemp); //6

            lstSettingCommand.Add(new clsSettingForCommand("ISDONE", 0));
            lstSettingCommand.Add(new clsSettingForCommand("ISPASS", 0));

            lstSettingCommand.Add(new clsSettingForCommand("MYID", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("MYNUMITEM", 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("ITEMID", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("ITEMRESULT", 1)); //
            lstSettingCommand.Add(new clsSettingForCommand("CHILDITEMRESULT", 1)); //
            lstSettingCommand.Add(new clsSettingForCommand("TIMEOUT", 2, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("USERMENU", 1, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("FUNC", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("FPARA", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("FRET", 1, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("CLEARPASSCOND", 1, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("SETPASSCOND", 1, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("CHECKINGMODE", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("SETCHECKINGMODE", 1)); //

            lstSettingCommand.Add(new clsSettingForCommand("EXECTIMES", 1)); //
            lstSettingCommand.Add(new clsSettingForCommand("RESETEXECTIMES", 1)); //
        }

        public object ExecuteSpecialFunction(ref clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "THIS":
                    return cmdTHIS(ref clsInput);

                case "JUMP":
                    return cmdJUMP(ref clsInput);
                case "FDONE":
                    return cmdFDONE(ref clsInput);
                case "SFDONE":
                    return cmdSFDONE(ref clsInput);
                case "ISDONE":
                    return cmdISDONE(ref clsInput);
                case "ISPASS":
                    return cmdISPASS(ref clsInput);
                case "MYID":
                    return cmdMYID(ref clsInput);
                case "MYNUMITEM":
                    return cmdMYNUMITEM(ref clsInput);
                case "ITEMID":
                    return cmdITEMID(ref clsInput);
                case "ITEMRESULT":
                    return cmdITEMRESULT(ref clsInput);
                case "CHILDITEMRESULT":
                    return cmdCHILDITEMRESULT(ref clsInput);

                case "TIMEOUT":
                    return cmdTIMEOUT(ref clsInput);

                case "USERMENU":
                    return cmdUSERMENU(ref clsInput);

                case "FUNC":
                    return cmdFUNC(ref clsInput);
                case "FPARA":
                    return cmdFPARA(ref clsInput);
                case "FRET":
                    return cmdFRET(ref clsInput);

                case "CLEARPASSCOND": //ClearPassCond
                    return cmdCLEARPASSCOND(ref clsInput);
                //SETPASSCOND
                case "SETPASSCOND": //ClearPassCond
                    return cmdSETPASSCOND(ref clsInput);

                case "CHECKINGMODE":
                    return cmdCHKMODE(ref clsInput);
                case "SETCHECKINGMODE":
                    return cmdSETCHECKINGMODE(ref clsInput);

                case "EXECTIMES":
                    return cmdEXECTIMES(ref clsInput);
                case "RESETEXECTIMES":
                    return cmdRESETEXECTIMES(ref clsInput);

                default:
                    return "PluginSpecialControl,SysSequence - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }
        //*****************************************************************************************************************************
        public object cmdTHIS(ref clsCommonCommandGuider clsInput)
        {
            return clsInput.objSources;
        }

        //*****************************************************************************************************************************
        public object cmdJUMP(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdJUMPMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdJUMPChildProcess(ref clsInput);
                default:
                    return "cmdJUMP() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdJUMPMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRETMasterProcess: Not class acceptable!";

            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count == 0) return "Error: There is no parameter for JUMP()!";
            if (clsInput.args.Parameters.Length != 1) return "JUMPMasterProcess Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            //reset request update token
            clsInput.blRequestUpdateToken = false;

            //If pass, then we looking for position of step want to jump to
            int intTargetStepNum = 0;
            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intTargetStepNum) == false) return "cmdJUMPMasterProcess() Error: Target step setting is not integer!";

            //Finding target step position
            int intTargetStepPos = 0;
            int intTargetToken = 0;

            var lstTargetExcelStep = new List<classStepDataInfor>();

            int i = 0;

            intTargetStepPos = clsMasterProcess.FindMasterStepPos(intTargetStepNum);

            if (intTargetStepPos == -1) return "cmdJUMPMasterProcess() Error: cannot find step [" + intTargetStepNum.ToString() + "] in master program list!";

            //Depend on Sequence ID?
            if(clsMasterProcess.lstTotalStep[clsInput.intStepPos].intStepSequenceID==0) //Main Sequence
            {
                enumMasterStatus eStatus = clsMasterProcess.FindMasterStatusClass(clsMasterProcess.lstTotalStep[intTargetStepPos].intStepClass);

                lstTargetExcelStep = clsMasterProcess.FindMasterStepListClass(clsMasterProcess.lstTotalStep[intTargetStepPos].intStepClass);

                //Find Target token
                for (i = 0; i < lstTargetExcelStep.Count; i++)
                {
                    if (lstTargetExcelStep[i].intStepPos == intTargetStepPos)
                    {
                        intTargetToken = i;
                        break;
                    }
                }

                //If everything is OK. Then assign new value and bring system to new point
                //if ((intTargetStepPos != clsInput.intStepPos)) //Jumping OK
                //{
                switch (eStatus) //Allow jumping to itself
                {
                    case enumMasterStatus.eMasterCyclePoll:

                        clsMasterProcess.intTokenCyclePoll = intTargetToken;
                        break;

                    case enumMasterStatus.eMasterEmerPoll:
                        clsMasterProcess.intTokenEmerPoll = intTargetToken;
                        break;

                    case enumMasterStatus.eMasterSafetyPoll:
                        clsMasterProcess.intTokenSafetyPoll = intTargetToken;
                        break;

                    case enumMasterStatus.eMasterBackGroundPoll:
                        clsMasterProcess.intTokenBackGround = intTargetToken;
                        break;

                    default:
                        clsMasterProcess.intTokenMasterMain = intTargetToken;
                        break;
                }
                //}
            }
            else if (clsMasterProcess.lstTotalStep[clsInput.intStepPos].intStepSequenceID == 1) //User Function Sequence ID
            {
                //If everything is OK. Then assign new value and bring system to new point
                int intTemp = 0;

                intTemp = clsMasterProcess.FindCheckingToken(intTargetStepPos);
                //Only change if find out legal value
                if (intTemp !=-1)
                {
                    clsInput.intToken = intTemp - 1; //Cooperate with for(;;) loop.
                }
                else
                {
                    return "cmdJUMPMasterProcess() error: Token invalid!";
                }

                clsInput.blRequestUpdateToken = true;
            }


            //If everything is Ok, then return class input
            return clsInput;
        }

        public object cmdJUMPChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdIFChildProcess: Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            
            //reset request update token
            clsInput.blRequestUpdateToken = false;

            if (clsInput.args.Parameters.Length != 1) return "JUMP() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid!";

            //object objTest = clsInput.args.objSource;

            int intTargetStepNum = 0;
            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intTargetStepNum) == false) return "JUMP() Error: Target step setting is not integer!";

            //Finding target step position
            int intTargetStepPos = clsChildProcess.FindCheckingStepPos(intTargetStepNum);

            if (intTargetStepPos == -1) return "cmdJUMPChildProcess() error: cannot find step [" + intTargetStepNum.ToString() + "] in child process program list!";

            //If everything is OK. Then assign new value and bring system to new point
            int intTemp = 0;

            intTemp = clsChildProcess.FindCheckingToken(intTargetStepPos);
            //Only change if find out legal value
            if (intTemp != -1)
            {
                clsInput.intToken = intTemp - 1; //Cooperate with for(;;) loop.
                clsInput.blRequestUpdateToken = true;
            }
            else
            {
                return "cmdJUMPChildProcess() error: Token invalid!";
            }

            //If everything is Ok, then return class input
            return clsInput;
        }
        
        //*****************************************************************************************************************************
        public object cmdFDONE(ref clsCommonCommandGuider clsInput) //For single thread process only
        {
            if (!(clsInput.objSources is clsSingleThreadProcessModel)) return "cmdFDONE: Not class acceptable!";

            clsSingleThreadProcessModel clsSingleProcess = (clsSingleThreadProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length != 1) return "cmdFDONE error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid. Expected 1";

            //IF YOU'RE DEAD. I WILL RAISE YOU UP & ASK YOU DO JOB FOR ME! :D. But... no care its result! just done & no judgement anything!
            int intOption = 0;
            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intOption) == false) return "Error FDONE(): The process request ID setting value is not integer! ";

            //Check option 
            if ((clsInput.intProcessId != intOption) && (intOption != -1)) return "Error: not process want to run. No effect!";

            //Request process run this step only if that process already fail
            object objRet = null;
            if(clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].clsItemResult.blItemCheckingResult==false)
            {
                objRet = clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].ChildFuncEx(clsInput.intStepPos);
            }
            else //Result is Pass. Only allow executing if current step is also have jumping request and jumping step Target token is bigger than Target step token
            {
                if (clsSingleProcess.lstclsChildProcessProgress[clsInput.intProcessId].blRequestJump == true)
                {
                    for (int i = 0; i < clsSingleProcess.lstclsProcessTask.Count; i++)
                    {
                        if (clsSingleProcess.lstclsProcessTask[i].intProcessID != clsInput.intProcessId) continue;
                        if (clsSingleProcess.lstclsProcessTask[i].intStepPos == clsInput.intStepPos)
                        {
                            if (clsSingleProcess.lstclsChildProcessProgress[clsInput.intProcessId].intTargetToken > i)
                            {
                                objRet = clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].ChildFuncEx(clsInput.intStepPos);
                            }
                            //
                            break;
                        }
                    }
                }
            }

            return objRet;
        }
        //*****************************************************************************************************************************
        public object cmdSFDONE(ref clsCommonCommandGuider clsInput) //For single thread process only
        {

            if (!(clsInput.objSources is clsSingleThreadProcessModel)) return "cmdFDONE: Not class acceptable!";

            clsSingleThreadProcessModel clsSingleProcess = (clsSingleThreadProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length != 1) return "cmdSFDONE error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid. Expected 1";

            //IF YOU'RE DEAD. I WILL RAISE YOU UP & ASK YOU DO JOB FOR ME! :D
            int intProcessIdRequest = 0;
            string strTemp = "";

            clsInput.blRequestUpdateToken = false;

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intProcessIdRequest) == false)
            {
                //return "Error SFDONE(): The process ID request setting value is not integer! ";
                return clsInput;
            }

            //Check option 
            if (clsInput.intProcessId != intProcessIdRequest)
            {
                clsInput.intToken = -100; //Special request to reject checking
                clsInput.blRequestUpdateToken = true;
                return clsInput;
            }

            //Request process run this step only if that process already fail
            object objRet = null;
            if (clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].clsItemResult.blItemCheckingResult == false)
            {
                objRet = clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].ChildFuncEx(clsInput.intStepPos);
            }
            else //Result is Pass. Only allow executing if current step is also have jumping request and jumping step Target token is bigger than Target step token
            {
                if(clsSingleProcess.lstclsChildProcessProgress[clsInput.intProcessId].blRequestJump==true)
                {
                    for (int i = 0; i < clsSingleProcess.lstclsProcessTask.Count;i++)
                    {
                        if (clsSingleProcess.lstclsProcessTask[i].intProcessID != clsInput.intProcessId) continue;
                        if(clsSingleProcess.lstclsProcessTask[i].intStepPos == clsInput.intStepPos)
                        {
                            if (clsSingleProcess.lstclsChildProcessProgress[clsInput.intProcessId].intTargetToken > i)
                            {
                                objRet = clsSingleProcess.lstChildProcessModel[clsInput.intProcessId].ChildFuncEx(clsInput.intStepPos);
                            }
                            //
                            break;
                        }
                    }
                }
            }

            return objRet;
        }
        //*****************************************************************************************************************************
        public object cmdISDONE(ref clsCommonCommandGuider clsInput) //
        {
            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return cmdISDONEMaster(ref clsInput);
                case 2: //Child process
                    return cmdISDONEChild(ref clsInput);
                default:
                    return "cmdISDONE() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdISDONEMaster(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdISDONEMaster: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
            if (clsInput.args.Parameters.Length < 1) return "cmdISDONEMaster() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid. Expected 1";

            int intTargetStepPos = 0;
            string strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if (strInput == "") //Get result current step
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else
            {
                int intTargetStepNum = 0;
                string strTemp = "";

                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intTargetStepNum) == false) return "Error: Target step setting is not integer!";

                //Finding target step position
                intTargetStepPos = clsMasterProcess.FindMasterStepPos(intTargetStepNum);
                if (intTargetStepPos == -1) return "Error: Target step not found!";
            }

            return clsMasterProcess.lstTotalStep[intTargetStepPos].blStepChecked;
        }

        public object cmdISDONEChild(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdISDONEChild: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            if (clsInput.args.Parameters.Length < 1) return "cmdISDONEChild() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid. Expected 1";

            int intTargetStepPos = 0;
            string strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if (strInput == "") //Get result current step
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else
            {
                int intTargetStepNum = 0;
                string strTemp = "";

                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intTargetStepNum) == false) return "Error: Target step setting is not integer!";

                //Finding target step position
                intTargetStepPos = clsChildProcess.FindCheckingStepPos(intTargetStepNum);
                if (intTargetStepPos == -1) return "Error: Target step not found!";
            }

            return clsChildProcess.lstTotalStep[intTargetStepPos].blStepChecked;
        }

        //*****************************************************************************************************************************
        public object cmdISPASS(ref clsCommonCommandGuider clsInput) //
        {
            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return cmdISPASSMaster(ref clsInput);
                case 2: //Child process
                    return cmdISPASSChild(ref clsInput);
                default:
                    return "cmdISPASS() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdISPASSMaster(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdISPASSMaster: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
            if (clsInput.args.Parameters.Length < 1) return "cmdISPASSMaster() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid. Expected 1";

            int intTargetStepPos = 0;

            string strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if (strInput == "") //Get result pass/fail of current step
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else
            {
                int intTargetStepNum = 0;
                string strTemp = "";

                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intTargetStepNum) == false) return "Error: Target step setting is not integer!";

                //Finding target step position
                intTargetStepPos = clsMasterProcess.FindMasterStepPos(intTargetStepNum);
                if (intTargetStepPos == -1) return "Error: Target step not found!";
            }

            return clsMasterProcess.lstTotalStep[intTargetStepPos].blStepResult;
        }

        public object cmdISPASSChild(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdISPASSChild: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            if (clsInput.args.Parameters.Length < 1) return "cmdISPASSMaster() error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid";

            //
            int intTargetStepPos = 0;

            string strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if(strInput=="") //Get result pass/fail of current step
            {
                intTargetStepPos = clsInput.intStepPos;
            }
            else //Get result of another step
            {
                int intTargetStepNum = 0;
                string strTemp = "";

                strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out intTargetStepNum) == false) return "Error: Target step setting is not integer!";

                //Finding target step position
                intTargetStepPos = clsChildProcess.FindCheckingStepPos(intTargetStepNum);
                if (intTargetStepPos == -1) return "Error: Target step not found!";
            }

            //Return step checking result
            return clsChildProcess.lstTotalStep[intTargetStepPos].blStepResult;
        }

        //*****************************************************************************************************************************
        /// <summary>
        /// Return the ID of child process
        /// </summary>
        /// <param name="clsInput"></param>
        /// <returns></returns>
        public object cmdMYID(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return -1; //ID of master process is -1

                case 2: //Child process

                    if (!(clsInput.objSources is clsChildProcessModel)) return "cmdMYID: Not class acceptable!";
                    clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

                    return clsChildProcess.intProcessID;

                case 3: //single thread process
                    return -2; //ID of single process is -2

                default:
                    return "cmdMYID() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        //*****************************************************************************************************************************
        /// <summary>
        /// Return the number of child item in a child process
        /// </summary>
        /// <param name="clsInput"></param>
        /// <returns></returns>
        public object cmdMYNUMITEM(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return "cmdMYNUMITEM() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";

                case 2: //Child process
                    if (!(clsInput.objSources is clsChildProcessModel)) return "cmdMYID: Not class acceptable!";
                    clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

                    if (clsChildProcess.lstclsItemCheckInfo == null) return 0;
                    return clsChildProcess.lstclsItemCheckInfo.Count;

                case 3: //single thread process
                    return "cmdMYNUMITEM() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";

                default:
                    return "cmdMYNUMITEM() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdITEMID(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return -1; //ID of master process is -1

                case 2: //Child process
                    if (!(clsInput.objSources is clsChildProcessModel)) return "cmdITEMID: Not class acceptable!";
                    clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

                    return clsChildProcess.FindItemID(clsInput.intStepPos);

                case 3: //single thread process
                    return -2; //ID of single process is -2

                default:
                    return "cmdITEMID() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdITEMRESULT(ref clsCommonCommandGuider clsInput)
        {
            int intItemID = 0;
            if (clsInput.args.Parameters == null) return "cmdITEMRESULT() Error: null parameter";
            if (clsInput.args.Parameters.Length <1) return "cmdITEMRESULT() Error: There is no parameter";
            string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intItemID) == false) return "cmdITEMRESULT() Error: Parameter input is not integer [" + strTemp + "]";

            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return false; //Not apply for master process, alway return false

                case 2: //Child process - not return immediately, just break and go to next
                    break;

                case 3: //single thread process
                    return false; //Not apply for single thread, alway return false

                default:
                    return "cmdITEMRESULT() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }

            //
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdITEMRESULT: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            //clsChildControlModel clsChildControl = (clsChildControlModel)clsChildProcess.objChildControlModel;

            //return clsChildControl.GetItemResult(intItemID);
            return clsChildProcess.ItemResultCalculate(intItemID);
        }

        //*****************************************************************************************************************************
        /// <summary>
        /// Get Child Process's Item result.
        /// Input: The child item ID. Note that a child process may contains some items.
        /// </summary>
        /// <param name="clsInput"></param>
        /// <returns></returns>
        public object cmdCHILDITEMRESULT(ref clsCommonCommandGuider clsInput)
        {
            int intChildItemID = 0;
            if (clsInput.args.Parameters == null) return "cmdCHILDITEMRESULT() Error: null parameter";
            if (clsInput.args.Parameters.Length < 1) return "cmdCHILDITEMRESULT() Error: There is no parameter";
            string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intChildItemID) == false) return "cmdCHILDITEMRESULT() Error: Parameter input is not integer [" + strTemp + "]";

            switch (clsInput.intSourcesID)
            {
                case 1: //Master process
                    return "cmdCHILDITEMRESULT() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";

                case 2: //Child process - not return immediately, just break and go to next
                    break;

                case 3: //single thread process
                    return "cmdCHILDITEMRESULT() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";

                default:
                    return "cmdCHILDITEMRESULT() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }

            //
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdCHILDITEMRESULT: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            return clsChildProcess.ItemResultCalculate(intChildItemID);
        }


        //*****************************************************************************************************************************
        public object cmdTIMEOUT(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdTIMEOUT(): Not class acceptable!";

            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

          
            //if (clsInput.lstobjCmdPara.Count < 2) return "cmdTIMEOUT() Error: Not enough parameter!";
            if (clsInput.args.Parameters.Length != 2) return "cmdTIMEOUT() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is ivalid. Expected 2";


            int intTimeOut = 0;
            int intOption = 0;
            string strtemp = "";

            strtemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strtemp, out intTimeOut) == false) return "Error TIMEOUT(): Time Out setting value is not integer: " + strtemp;

            strtemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strtemp, out intOption) == false) return "Error TIMEOUT(): option setting value is not integer: " + strtemp;

            //1. If result is OK, then we do nothing, just return
            //Getting stepresult
            object objTest = clsInput.args.objCommandGuider;
            if (objTest == null) return "cmdTIMEOUT Error: Cannot find command guider object";
            if (!(objTest is clsCommonCommandGuider)) return "cmdTIMEOUT Error: Cannot find command guider object";

            clsCommonCommandGuider clsCommandGuider = (clsCommonCommandGuider)objTest;
            if (clsChildProcess.lstTotalStep[clsCommandGuider.intStepPos].blStepResult == true) return "Step Result Pass. No effect!";


            //2. If step result is fail, then depend on time out value, control to retry checking
            int intCurrentTick = MyLibrary.clsApiFunc.GetTickCount();

            int intDifferentTick = intCurrentTick - clsChildProcess.lstTotalStep[clsCommandGuider.intStepPos].intStartTickMark;

            if (intDifferentTick > intTimeOut) return "Time out reach. Retry check terminated.";
            //3. Well, it's time to do retry check

            //Check Option & Process ID
            if ((clsChildProcess.intProcessID != intOption) && (intOption != -1)) return "No Effect to non-desired process!";

            //Looking in ThreadChecking class & return token
            clsInput.intToken = clsChildProcess.FindCheckingToken(clsCommandGuider.intStepPos) - 1; //comeback step itself to check again
            clsInput.blRequestUpdateToken = true;

            //
            return clsInput;
        }

        //*****************************************************************************************************************************
        public object cmdUSERMENU(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdUSERMENU(): Not class acceptable!";
            //
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            if (clsInput.args.Parameters.Length < 1) return "cmdUSERMENU() Error: Not enough parameter!";

            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (!(objRet is System.Windows.Controls.MenuItem)) return "cmdUSERMENU() Error: object input is not menu item!";
            //
            clsChildControlModel clsChildControl = (clsChildControlModel)clsChildProcess.objChildControlModel;
            Application.Current.Dispatcher.Invoke(new Action(() => clsChildControl.AddUserMenuItem(objRet)));

            //If Ok, return 0
            return 0;
        }

        //*****************************************************************************************************************************
        public object cmdFUNC(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdFUNCMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdFUNCChildProcess(ref clsInput);
                default:
                    return "cmdFUNC() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdFUNCMasterProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdFUNCMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
            
            //
            if (clsInput.args.Parameters == null) return "cmdFUNCMasterProcess: no parameter input";
            if (clsInput.args.Parameters.Length < 1) return "cmdFUNCMasterProcess: no parameter input";

            List<object> lstobj = new List<object>();
            for (int i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                lstobj.Add(clsInput.args.Parameters[i].Evaluate());
            }

            object objRet = clsMasterProcess.UserFunction(lstobj);
            
            //
            return objRet;
        }

        public object cmdFUNCChildProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdFUNCChildProcess: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            //
            if (clsInput.args.Parameters == null) return "cmdFUNCChildProcess: no parameter input";
            if (clsInput.args.Parameters.Length < 1) return "cmdFUNCChildProcess: no parameter input";

            List<object> lstobj = new List<object>();
            for(int i = 0;i<clsInput.args.Parameters.Length;i++)
            {
                lstobj.Add(clsInput.args.Parameters[i].Evaluate());
            }

            object objRet = clsChildProcess.UserFunction(lstobj);

            //
            return objRet;
        }

        //*****************************************************************************************************************************
        public object cmdFPARA(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdFPARAMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdFPARAChildProcess(ref clsInput);
                default:
                    return "cmdFPARA() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdFPARAMasterProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdFPARAMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 2) return "cmdFPARAMasterProcess() error: not enough parameter input";
            if (clsInput.args.Parameters == null) return "cmdFPARAMasterProcess() error: not enough parameter input";
            if (clsInput.args.Parameters.Length < 2) return "cmdFPARAMasterProcess() error: not enough parameter input";

            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            string strUserFuncName = strTemp.Trim();

            int intParaPos = 0;
            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp.Trim(), out intParaPos) == false)
            {
                return "cmdFPARAMasterProcess() error: The position of parameter input [" + strTemp + "] is not integer!";
            }

            //
            object objRet = clsMasterProcess.MasterGetUserFuncPara(strUserFuncName, intParaPos);

            //
            return objRet;
        }

        public object cmdFPARAChildProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdFPARAChildProcess: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 2) return "cmdFPARAChildProcess() error: not enough parameter input";
            if (clsInput.args.Parameters == null) return "cmdFPARAMasterProcess() error: not enough parameter input";
            if (clsInput.args.Parameters.Length < 2) return "cmdFPARAMasterProcess() error: not enough parameter input";

            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            string strUserFuncName = strTemp.Trim();

            int intParaPos = 0;
            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp.Trim(), out intParaPos) == false)
            {
                return "cmdFPARAChildProcess() error: The position of parameter input [" + strTemp + "] is not integer!";
            }

            //
            object objRet = clsChildProcess.ChildProcessGetUserFuncPara(strUserFuncName, intParaPos);

            //
            return objRet;
        }

        //*****************************************************************************************************************************
        public object cmdFRET(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdFRETMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdFRETChildProcess(ref clsInput);
                default:
                    return "cmdFRET() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdFRETMasterProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdFRETMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 1) return "cmdFRETMasterProcess() error: not enough parameter input";
            if (clsInput.args.Parameters == null) return "cmdFRETMasterProcess() error: not enough parameter input";
            if (clsInput.args.Parameters.Length < 1) return "cmdFRETMasterProcess() error: not enough parameter input";

            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            string strUserFuncName = strTemp;

            int intIndex = clsMasterProcess.MasterSearchUserFunc(strUserFuncName);

            if (intIndex == -1) return "cmdFRETMasterProcess() error: cannot found user function name [" + strUserFuncName + "]";

            //
            return clsMasterProcess.clsMasterProgList.lstclsUserFunction[intIndex].objReturnData;
        }

        public object cmdFRETChildProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdFRETChildProcess: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            //if (clsInput.lstobjCmdPara.Count < 1) return "cmdFRETChildProcess() error: not enough parameter input";
            if (clsInput.args.Parameters == null) return "cmdFRETChildProcess() error: not enough parameter input";
            if (clsInput.args.Parameters.Length < 1) return "cmdFRETChildProcess() error: not enough parameter input";

            string strTemp = "";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            string strUserFuncName = strTemp;

            int intIndex = clsChildProcess.ChildProSearchUserFunc(strUserFuncName);

            if (intIndex == -1) return "cmdFRETChildProcess() error: cannot found user function name [" + strUserFuncName + "]";

            //
            return clsChildProcess.clsProgramList.lstclsUserFunction[intIndex].objReturnData;
        }

        //*****************************************************************************************************************************
        //CLEARPASSCOND
        public object cmdCLEARPASSCOND(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdCLEARPASSCOND: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            int i,j = 0;
            for (i = 0; i < clsInput.args.Parameters.Length;i++)
            {
                string strTemp = clsInput.args.Parameters[i].Evaluate().ToString().Trim();
                int intStepNum = 0;
                if(int.TryParse(strTemp,out intStepNum)==false)
                {
                    return "cmdCLEARPASSCOND() error: The step number [" + strTemp + "] is not numeric!";
                }
                //
                int intSTLStepPos = clsChildProcess.FindStepListStepPos(intStepNum);
                if (intSTLStepPos == -1) return "cmdCLEARPASSCOND() error: cannot find step number [" + intStepNum.ToString() + "] in step list!";

                //clear pass condition
                clsChildProcess.clsStepList.lstExcelList[intSTLStepPos].blSTLPassCondition = false;
                //We need to find out the representative step in programlist & set blSTLPassCondition = false too! (Result's Judgement use this)
                //We must support Group Mode also!
                for(j=0;j<clsChildProcess.lstclsItemCheckInfo.Count;j++)
                {
                    int intPLStepPos = clsChildProcess.FindStepListPLStepPos(j, intSTLStepPos);
                    if (intPLStepPos != -1)
                    {
                        clsChildProcess.lstclsItemCheckInfo[j].lstTotalStep[intPLStepPos].blSTLPassCondition = false;
                    }
                }

            }

            //return OK code - 0 if everything is ok
            return 0;
        }
        //*****************************************************************************************************************************
        //SETPASSCOND
        public object cmdSETPASSCOND(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdSETPASSCOND: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            int i, j = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                string strTemp = clsInput.args.Parameters[i].Evaluate().ToString().Trim();
                int intStepNum = 0;
                if (int.TryParse(strTemp, out intStepNum) == false)
                {
                    return "cmdSETPASSCOND() error: The step number [" + strTemp + "] is not numeric!";
                }
                //
                int intSTLStepPos = clsChildProcess.FindStepListStepPos(intStepNum);
                if (intSTLStepPos == -1) return "cmdSETPASSCOND() error: cannot find step number [" + intStepNum.ToString() + "] in step list!";
                //clear pass condition
                clsChildProcess.clsStepList.lstExcelList[intSTLStepPos].blSTLPassCondition = true;
                //We need to find out the representative step in programlist & set blSTLPassCondition = false too! (Result's Judgement use this)
                //We must support Group Mode also!
                for (j = 0; j < clsChildProcess.lstclsItemCheckInfo.Count; j++)
                {
                    int intPLStepPos = clsChildProcess.FindStepListPLStepPos(j, intSTLStepPos);
                    if (intPLStepPos != -1)
                    {
                        clsChildProcess.lstclsItemCheckInfo[j].lstTotalStep[intPLStepPos].blSTLPassCondition = true;
                    }
                }
            }

            //return OK code - 0 if everything is ok
            return 0;
        }


        //*****************************************************************************************************************************
        public object cmdCHKMODE(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdCHKMODEMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdCHKMODEChildProcess(ref clsInput);
                default:
                    return "cmdCHKMODE() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdSETCHECKINGMODE(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length == 0) return "cmdSETCHECKINGMODE() Error: There is no parameter!";
            string checkingMode = clsInput.args.Parameters[0].Evaluate().ToString();

            // Dispatch action
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ChangeCheckingModeAction(checkingMode));

            // Return OK code if everything is OK.
            return 0;
        }

        //*****************************************************************************************************************************

        public object cmdCHKMODEMasterProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdCHKMODEMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //
            return clsMasterProcess.strSystemCheckingMode;
        }

        public object cmdCHKMODEChildProcess(ref clsCommonCommandGuider clsInput) //
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdCHKMODEChildProcess: Not class acceptable!";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            //
            return clsChildProcess.strSystemCheckingMode;
        }
        //*****************************************************************************************************************************
        public object cmdEXECTIMES(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdEXECTIMESMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdEXECTIMESChildProcess(ref clsInput);
                default:
                    return "cmdEXECTIMES() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdEXECTIMESMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdCHKMODEMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //Get Info of Executed times
            int stepPos = 0;
            string strInput = "";
            if (clsInput.args.Parameters.Length > 0)
            {
                strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            }

            if (strInput == "") //Target current step
            {
                stepPos = clsInput.intStepPos;
            }
            else //Target another step
            {
                int stepNumber = 0;
                if (clsInput.args.Parameters.Length == 0) return "cmdEXECTIMES Error: There is no parameter!";
                string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out stepNumber) == false)
                {
                    return "cmdEXECTIMES Error: Step Number input [" + strTemp + "] is not integer!";
                }

                //If OK, then return 
                stepPos = clsMasterProcess.FindMasterStepPos(stepNumber);
                if (stepPos == -1) return "cmdEXECTIMES Error: cannot find step number [" + strTemp + "] in program list!";
            }

            return clsMasterProcess.lstTotalStep[stepPos].intExecuteTimes;
        }

        public object cmdEXECTIMESChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdEXECTIMES Error: Not acceptable class! Only accept Child Process";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;

            int stepPos = 0;
            string strInput = "";
            if (clsInput.args.Parameters.Length>0)
            {
                strInput = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            }

            if(strInput=="") //Target current step
            {
                stepPos = clsInput.intStepPos;
            }
            else //Target another step
            {
                int stepNumber = 0;
                if (clsInput.args.Parameters.Length == 0) return "cmdEXECTIMES Error: There is no parameter!";
                string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
                if (int.TryParse(strTemp, out stepNumber) == false)
                {
                    return "cmdEXECTIMES Error: Step Number input [" + strTemp + "] is not integer!";
                }

                //If OK, then return 
                stepPos = clsChildProcess.FindCheckingStepPos(stepNumber);
                if (stepPos == -1) return "cmdEXECTIMES Error: cannot find step number [" + strTemp + "] in program list!";
            }

            return clsChildProcess.lstTotalStep[stepPos].intExecuteTimes;
        }
        //*****************************************************************************************************************************
        public object cmdRESETEXECTIMES(ref clsCommonCommandGuider clsInput)
        {
            switch (clsInput.intSourcesID)
            {
                case 1:
                    return cmdRESETEXECTIMESMasterProcess(ref clsInput);
                case 2: //Child process
                    return cmdRESETEXECTIMESChildProcess(ref clsInput);
                default:
                    return "cmdRESETEXECTIMES() Error: not support Source ID [" + clsInput.intSourcesID.ToString() + "]";
            }
        }

        public object cmdRESETEXECTIMESMasterProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsMasterProcessModel)) return "cmdRESETEXECTIMESMasterProcess: Not class acceptable!";
            clsMasterProcessModel clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;

            //Get Info of Executed times
            int stepNumber = 0;
            if (clsInput.args.Parameters.Length == 0) return "cmdRESETEXECTIMESMasterProcess Error: There is no parameter!";
            string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out stepNumber) == false)
            {
                return "cmdRESETEXECTIMESMasterProcess Error: Step Number input [" + strTemp + "] is not integer!";
            }

            //If OK, then return 
            int stepPos = clsMasterProcess.FindMasterStepPos(stepNumber);
            if (stepPos == -1) return "cmdRESETEXECTIMESMasterProcess Error: cannot find step number [" + strTemp + "] in program list!";

            //reset
            clsMasterProcess.lstTotalStep[stepPos].intExecuteTimes = 0;
            return clsMasterProcess.lstTotalStep[stepPos].intExecuteTimes;
        }

        public object cmdRESETEXECTIMESChildProcess(ref clsCommonCommandGuider clsInput)
        {
            if (!(clsInput.objSources is clsChildProcessModel)) return "cmdRESETEXECTIMESChildProcess Error: Not acceptable class! Only accept Child Process";
            clsChildProcessModel clsChildProcess = (clsChildProcessModel)clsInput.objSources;
            //Get Info of Executed times
            int stepNumber = 0;
            if (clsInput.args.Parameters.Length == 0) return "cmdRESETEXECTIMESChildProcess Error: There is no parameter!";
            string strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out stepNumber) == false)
            {
                return "cmdRESETEXECTIMESChildProcess Error: Step Number input [" + strTemp + "] is not integer!";
            }

            //If OK, then return 
            int stepPos = clsChildProcess.FindCheckingStepPos(stepNumber);
            if (stepPos == -1) return "cmdRESETEXECTIMESChildProcess Error: cannot find step number [" + strTemp + "] in program list!";

            //reset
            clsChildProcess.lstTotalStep[stepPos].intExecuteTimes = 0;
            return clsChildProcess.lstTotalStep[stepPos].intExecuteTimes;
        }
    }
}
