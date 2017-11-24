using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nspSpecialControl;


namespace nspSingleThreadProcessModel
{
    public enum enumSingleThreadProcessRunMode
    {
        eNormal, //Running normally
        eSingle, //Running each step & wait for allowing signal (button press) for continue
        eStep, //Running until reach desired step, continue if allowing condition met (button press)
        eFail, //Running until reach fail step => Stop
        eAll, //Running all step, no care about result
        eNotrecognize //Not recognize mode
    }

    public class clsSingleThreadProcessModel
    {
        //For Thread Checking
        public System.Threading.Thread thrdSingleThread { get; set; }

        public List<bool> lstblStepResult { get; set; } //Step is OK or NG for all sheet - Need initialize for this collection
        
        public int intMarkItemNumFail { get; set; }

        public int intSelectedStep { get; set; } //for Step checking Mode

        public int intLastStepPos { get; set; } //Marking last step position

        public int intSlowestToken { get; set; } //Marking Slowest token

        public List<bool> lstblStepChecked { get; set; } //Step is checked or not checked when complete checking process?

        //For Child process of Single Thread class
        public List<nspChildProcessModel.clsChildProcessModel> lstChildProcessModel { get; set; }
        public int intNumChildPro { get; set; }


        //**********************SUPPORT DISPLAY INFO ON USER INTERFACE************************************************************
        public enumSingleThreadProcessRunMode eRunMode { get; set; }  //For Checking Mode
        public bool blAllowContinueRunning { get; set; } //True if button NEXT is pressed
        public bool blRequestStopRunning { get; set; } //True if button END is pressed
        public int intStepModeSelected { get; set; } //Indicate what step positon request to stop in 

        public void SingleThreadIni(List<nspChildProcessModel.clsChildProcessModel> lstInput)
        {
            int i = 0;
            bool blTemp = false;
            //Import from System Ini?
            //Note that lstChildProcessModel should be initialized first before pass to single thread class
            this.lstChildProcessModel = lstInput;
            this.intNumChildPro = lstInput.Count;
            this.eRunMode = enumSingleThreadProcessRunMode.eNormal;

            //this.lstblStepResult
            this.lstblStepResult = new List<bool>();
            this.lstblStepChecked = new List<bool>();
            for(i=0;i<this.lstChildProcessModel[0].lstChildTotal.Count;i++)
            {
                this.lstblStepResult.Add(blTemp);
                this.lstblStepChecked.Add(blTemp);
            }
        }

        public void SingleThreadChecking()
        {
            int i = 0; //temporary count
            int k = 0; //Child process count
            int intFirstStepFinishPos = 0; //Marking first step position which belong to Finish class

            //Mark first step position of Finish Process
            if(this.lstChildProcessModel[0].lstChildFinish.Count!= 0)
            {
                intFirstStepFinishPos = this.lstChildProcessModel[0].lstChildFinish[0].intTestPos;
            }
            else
            {
                intFirstStepFinishPos = this.lstChildProcessModel[0].lstChildThreadCheck.Count; //If there is no Finish process, go to an end!
            }

            //1. Prepare something before checking
            //For all Child Process
            for(k=0;k<this.intNumChildPro;k++)
            {
                this.lstChildProcessModel[k].clsItemInfo.blItemCheckingResult = true;

                this.lstChildProcessModel[k].eChildProcessStatus = nspChildProcessModel.enumChildProcessStatus.eChecking;
                this.lstChildProcessModel[k].blClearLastInfo = false;


                //Before checking started, clear all data before
                for (i = 0; i < this.lstChildProcessModel[k].lstChildThreadCheck.Count; i++)
                {
                    int intStepPos = this.lstChildProcessModel[k].lstChildThreadCheck[i].intTestPos;

                    this.lstChildProcessModel[k].clsItemInfo.lstblStepResult[intStepPos] = false;
                    this.lstChildProcessModel[k].clsItemInfo.lstdblStepCheckingData[intStepPos] = 0;
                    this.lstChildProcessModel[k].clsItemInfo.lstdblStepTactTime[intStepPos] = 0;
                    this.lstChildProcessModel[k].clsItemInfo.lststrStepErrMsg[intStepPos] = "";
                    this.lstChildProcessModel[k].clsItemInfo.lstblStepChecked[intStepPos] = false;
                    this.lstChildProcessModel[k].clsItemInfo.lstintStartTickMark[intStepPos] = 0;
                }

                this.lstChildProcessModel[k].intStepPosRunning = 0;
                this.lstChildProcessModel[k].intStepLastPosRunning = 0;

                this.lstChildProcessModel[k].intStepPosToken = 0;
                this.lstChildProcessModel[k].intStepPosLastToken = 0;
            }

            //For Single Thread Class
            for (i = 0; i < this.lstblStepResult.Count; i++)
            {
                this.lstblStepResult[i] = false;
            }
            this.intMarkItemNumFail = 0;

            //Reset flag marking checked step
            for (i = 0; i < this.lstChildProcessModel[0].lstChildTotal.Count;i++ )
            {
                this.lstblStepChecked[i] = false;
            }

            //2. Looking and execute each step in step list and DO IT FOR ALL CHILD PROCESS!!!
            //*********************************USING TOKEN FUNCTION************************************************************************
            //
            //                              SINGLE THREAD PRINCIPLES
            //
            //      1. All process "should" run together, with same progress and same step
            //      2. In case all process has different progress, then:
            //              - Only allow process has slowest progress running. Other faster process have to wait the slowest one.
            //              - After all process have same progress again, allow them to run together.
            //
            //******************************************************************************************************************************

            //Now control Single Thread Checking Sequences - Follow Process which has slowest progress (checking Token)
            this.intSlowestToken = 0;
            bool blFlagSyncProcess = false; //Indicate that all child process is same progress, running with same step together

            for (k = 0; k < this.intNumChildPro;k++ )
            {
                this.lstChildProcessModel[k].clsItemInfo.blItemCheckingResult = true;
            }

            while (this.intSlowestToken < this.lstChildProcessModel[0].lstChildThreadCheck.Count) //Counting each step in step list which belong to thread checking
            {
                //Find the position of current step - slowest process in step list
                int intStepPos = this.lstChildProcessModel[0].lstChildThreadCheck[this.intSlowestToken].intTestPos;

                #region ChilProcessStep

                //If which thread has token smaller than slowest token, then do checking for that process with current step
                for (k = 0; k < this.intNumChildPro; k++) //Counting each child process
                {
                    //Only process which has same progress  with the slowest one are allowed to execute step checking
                    if (this.lstChildProcessModel[k].intStepPosToken == this.intSlowestToken)
                    {
                        bool blTempResult = false;

                        //Do special control - Before. Follow Child Process special control
                        this.lstChildProcessModel[k].intStepPosLastToken = this.lstChildProcessModel[k].intStepPosToken; //Backup Token
                        int intTempTokenBefore = this.intSlowestToken;
                        this.lstChildProcessModel[k].ChildProcessSpecialControlCommandEx(1, intStepPos, ref intTempTokenBefore);

                        //Do special control - Before. Follow single thread process special control
                        this.SingleThreadProcessSpecialControlCommandEx(k, 1, intStepPos, ref intTempTokenBefore);
                        
                        this.lstChildProcessModel[k].intStepPosToken = intTempTokenBefore;

                        //Check again condition of Token - We need to do this again because maybe special control change token or do something
                        //Only allow the slowest process to run
                        if (this.lstChildProcessModel[k].intStepPosToken == this.intSlowestToken)
                        {
                            blTempResult = this.lstChildProcessModel[k].ChildFuncEx(intStepPos);
                            if (blTempResult == false)
                            {
                                this.intMarkItemNumFail = k; //Marking step fail
                            }
                        }
                        else
                        {
                            //Reset Token to last Token if need
                            if (this.lstChildProcessModel[k].intStepPosToken == -1) // -1 is special value to point out that Token need to be reset
                            {
                                blTempResult = true; //IF not allow to run, then set result of this step to PASS
                                this.lstChildProcessModel[k].intStepPosToken = this.lstChildProcessModel[k].intStepPosLastToken; //reset to Backup Token
                            }
                        }


                        //Do special control - after. Follow Child Process special control
                        int intTempTokenAfter = this.intSlowestToken;
                        this.lstChildProcessModel[k].ChildProcessSpecialControlCommandEx(0, intStepPos, ref intTempTokenAfter);

                        //Do special control - Before. Follow single thread process special control
                        this.SingleThreadProcessSpecialControlCommandEx(k, 0, intStepPos, ref intTempTokenBefore);

                        //only process which own current slowest progress has the right to change Token if has request
                        if (this.lstChildProcessModel[k].intStepPosToken == this.intSlowestToken)
                        {
                            this.lstChildProcessModel[k].intStepPosToken = intTempTokenAfter;
                        }

                        //If 1 process fail at step class 3 (checking class) then move the token of that step to the first step of finish class. Need one more condition: it must be normal mode
                        if ((blTempResult == false) && (this.lstChildProcessModel[k].lstChildTotal[intStepPos].intTestClass == 3) && (this.eRunMode == enumSingleThreadProcessRunMode.eNormal))
                        {
                            this.lstChildProcessModel[k].intStepPosToken = this.lstChildProcessModel[k].intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildProcessModel[k].lstChildThreadCheck);
                        }
                        else// if not, then Increase by 1 prepare for next time
                        {
                            this.lstChildProcessModel[k].intStepPosToken++;
                        }

                    }
                    else //For process has faster progress than the slowest one (because it already fail & jump to finish process. Or it simply pass & run faster)
                    {
                        //Do special control only. Note that do not allow these process to change token here!
                        int intTempToken = this.intSlowestToken;

                        this.lstChildProcessModel[k].ChildProcessSpecialControlCommandEx(1, intStepPos, ref intTempToken);
                        //Do special control - Before. Follow single thread process special control
                        this.SingleThreadProcessSpecialControlCommandEx(k, 1, intStepPos, ref intTempToken);

                        this.lstChildProcessModel[k].ChildProcessSpecialControlCommandEx(0, intStepPos, ref intTempToken);
                        //Do special control - Before. Follow single thread process special control
                        this.SingleThreadProcessSpecialControlCommandEx(k, 0, intStepPos, ref intTempToken);
                    }

                } //End For k

                //Marking step aready checked
                this.lstblStepChecked[intStepPos] = true;

                #endregion

                //After each step, find out a gain what Process has smallest Token, then assign that token for slowest one
                int intTempSmall = this.lstChildProcessModel[0].intStepPosToken;
                int intTempVal = intTempSmall;
                blFlagSyncProcess = true;
                for (k = 0; k < this.intNumChildPro; k++) //Counting each child process
                {
                    if (intTempVal != this.lstChildProcessModel[k].intStepPosToken) blFlagSyncProcess = false; //Not sync

                    if (this.lstChildProcessModel[k].intStepPosToken < intTempSmall)
                    {
                        intTempSmall = this.lstChildProcessModel[k].intStepPosToken; //Find new smaller value
                    }

                    intTempVal = this.lstChildProcessModel[k].intStepPosToken;
                }

                //Now assign new value for Slowest Token
                this.intSlowestToken = intTempSmall;

                //Calculate Result of each step in SINGLE THREAD MODE
                this.lstblStepResult[intStepPos] = true;
                for (k = 0; k < this.intNumChildPro; k++) //Counting each child process
                {
                    if ((this.lstChildProcessModel[k].clsItemInfo.lstblStepResult[intStepPos] == false) && (this.lstChildProcessModel[k].lstChildTotal[intStepPos].intTestClass == 3))
                    {
                        this.lstblStepResult[intStepPos] = false;
                        break;
                    }
                }

                //Checking Handle Mode
                #region HandleMode
                if (blFlagSyncProcess == true) //Allow only if all process is sync - running with same progress, same step
                {
                    ////////////////////SINGLE THREAD - NORMAL MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumSingleThreadProcessRunMode.eNormal)
                    {
                        //Do nothing
                    }

                    ////////////////////SINGLE THREAD - SINGLE MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumSingleThreadProcessRunMode.eSingle)
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                            {
                                this.blRequestStopRunning = false; //Reset for next time
                                this.intSlowestToken = this.lstChildProcessModel[0].lstChildThreadCheck.Count; //Ignore all next steps and come to ending
                                break;
                            }
                        }
                    } //End If SINGLE MODE

                    ////////////////////SINGLE THREAD - STEP MODE///////////////////////////////////////////////////////
                    if (this.eRunMode == enumSingleThreadProcessRunMode.eStep)
                    {
                        //Stop if reach selected step
                        if (this.intSelectedStep == this.lstChildProcessModel[0].lstChildTotal[intStepPos].intTestNumber) //(this.lstblStepResult[intStepPos] == false)
                        {
                            this.blAllowContinueRunning = false;
                            while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                            {
                                if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                                {
                                    this.blRequestStopRunning = false; //Reset for next time
                                    this.intSlowestToken = this.lstChildProcessModel[0].lstChildThreadCheck.Count; //Ignore all next steps and come to ending
                                    break;
                                }
                            }
                        }

                    } //End If STEP MODE

                    ////////////////////SINGLE THREAD - FAIL MODE///////////////////////////////////////////////////////
                    if (this.eRunMode == enumSingleThreadProcessRunMode.eFail)
                    {
                        //Stop if fail or reach selected step
                        if (this.lstblStepResult[intStepPos] == false)
                        {
                            this.blAllowContinueRunning = false;
                            while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                            {
                                if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                                {
                                    this.blRequestStopRunning = false; //Reset for next time
                                    this.intSlowestToken = this.lstChildProcessModel[0].lstChildThreadCheck.Count; //Ignore all next steps and come to ending
                                    break;
                                }
                            }
                        }
                    }

                    ////////////////////SINGLE THREAD - ALL MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumSingleThreadProcessRunMode.eAll)
                    {
                        //Automatically run all steps without caring result of each step OK or NG
                    }

                    //Marking last step running
                    this.intLastStepPos = intStepPos;

                } //End if blFlagSyncProcess

                #endregion


            } //End While(this.intSlowestToken ....)

            #region _FinishChecking

            //Calculate result of each child process
            for (k = 0; k < this.intNumChildPro; k++)
            {
                this.lstChildProcessModel[k].clsItemInfo.blItemCheckingResult = true;
                for(i=0;i<this.lstChildProcessModel[0].lstChildChecking.Count;i++)
                {
                    if (this.lstChildProcessModel[k].clsItemInfo.lstblStepResult[this.lstChildProcessModel[0].lstChildChecking[i].intTestPos] == false)
                    {
                        this.lstChildProcessModel[k].clsItemInfo.blItemCheckingResult = false;
                        break;
                    }
                }
            }

            //Change status
            for (k = 0; k < this.intNumChildPro; k++)
            {
                this.lstChildProcessModel[k].eChildProcessStatus = nspChildProcessModel.enumChildProcessStatus.eFinish;
            }
            #endregion

        } //End SingleThreadChecking() Method


        //******************************For Single Thread Process Special Control**************************************
        public void SingleThreadProcessSpecialControlCommandEx(int intProcessId, int intAffectArea, int intStepPos, ref int intToken)
        {
            int i = 0;
            for (i = 0; i < this.lstChildProcessModel[i].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
            {
                if (this.lstChildProcessModel[i].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].blIsCmdPara == false) return;

                //Check affect area
                if (this.lstChildProcessModel[i].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].intAffectArea != intAffectArea) return;

                //Execute if is special command
                var clsTemp = new clsCommonCommandGuider();
                clsTemp = this.lstChildProcessModel[i].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i];
                this.SingleThreadProcessSpecialCommandExecute(intProcessId, ref clsTemp, intStepPos, ref intToken);
                this.lstChildProcessModel[i].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i] = clsTemp;
            }
        }


        public string SingleThreadProcessSpecialCommandExecute(int intProcessId, ref clsCommonCommandGuider clsInput, int intStepPos, ref int intToken)
        {

            int i = 0;
            //Only calculate if clsInput is special command. If not, then return itself parameter
            if (clsInput.blIsCmdPara == false) return clsInput.strPara;

            //Calculate all command-parameter if exist! using RECURSIVE Algorithm to execute all child-command inside mother command
            for (i = 0; i < clsInput.intNumPara; i++) //intNumPara: how many parameter of each type of special control command have
            {
                if (clsInput.blIsCmdPara == true) //If command-parameter, we need calculate result and re-assign value for parameter
                {
                    //RECURSIVE Algorithm!!!
                    clsCommonCommandGuider clsTemp = new clsCommonCommandGuider();
                    clsTemp = clsInput.lstclsChildCmdGuider[i];
                    clsInput.lststrCmdPara[i] = SingleThreadProcessSpecialCommandExecute(intProcessId, ref clsTemp, intStepPos, ref intToken);
                    clsInput.lstclsChildCmdGuider[i] = clsTemp;
                }
            }

            //OK. Now, all child-command is executed. All parameter now is just number or string. Execute command!!!
            switch (clsInput.cmdId)
            {
                case cmdCommonSpecialControl.cmdFDone:
                    return cmdFDONE(intProcessId, ref clsInput, intStepPos, ref intToken);
                case cmdCommonSpecialControl.cmdSFDone:
                    return cmdSFDONE(intProcessId, ref clsInput, intStepPos, ref intToken);
                default:
                    return "ERROR"; //Not recognize command
            }

        } //End SingleThreadProcessSpecialCommandExecute()


        //****************************************************
        public string cmdFDONE(int intProcessId, ref clsCommonCommandGuider clsInput, int intStepPos, ref int intToken)
        {
            //MessageBox.Show("FDONE");

            //IF YOU'RE DEAD. I WILL RAISE YOU UP & ASK YOU DO JOB FOR ME! :D. But... noc are its result! just done & no judgement anything!
            int intOption = 0;

            if (int.TryParse(clsInput.lststrCmdPara[0].ToString(), out intOption) == false) return "Error FDONE(): The process request ID setting value is not integer! ";

            //if (CheckerFrameProgram.Program.eSystemRunningMode != CheckerFrameProgram.enumSystemRunningMode.SingleThreadMode) return "Not Single Thread Mode. No support!";

            //If all process have result NG, then not allow Force Done function executed
            bool blTemp = true;
            int i = 0;
            for (i = 0; i < this.intNumChildPro; i++)
            {
                if (this.lstChildProcessModel[i].clsItemInfo.blItemCheckingResult == true) //there is a process still OK?
                {
                    blTemp = false;
                    break; //No need confirm anymore
                }
            }

            if (blTemp == true) return "Error FDONE(): All process is already NG. Then FDONE() no effect!";

            //Check option 
            if ((intProcessId != intOption) && (intOption != -1)) return "Error: not process want to run. No effect!";

            //Request this process run this step if The Token is not match with the slowest one
            if (this.lstChildProcessModel[intProcessId].intStepPosToken != this.intSlowestToken)
            {
                this.lstChildProcessModel[intProcessId].ChildFuncEx(intStepPos);
            }

            return "0";
        }

        //****************************************************
        public string cmdSFDONE(int intProcessId, ref clsCommonCommandGuider clsInput, int intStepPos, ref int intToken)
        {
            //MessageBox.Show("SFDONE");

            //IF YOU'RE DEAD. I WILL RAISE YOU UP & ASK YOU DO JOB FOR ME! :D
            int intProcessIdRequest = 0;

            if (int.TryParse(clsInput.lststrCmdPara[0].ToString(), out intProcessIdRequest) == false) return "Error SFDONE(): The process ID request setting value is not integer! ";

            //if (CheckerFrameProgram.Program.eSystemRunningMode != CheckerFrameProgram.enumSystemRunningMode.SingleThreadMode) return "Not Single Thread Mode. No support!";

            //If all process have result NG, then not allow Force Done function executed
            bool blTemp = true;
            int i = 0;
            for (i = 0; i < this.intNumChildPro; i++)
            {
                if (this.lstChildProcessModel[i].clsItemInfo.blItemCheckingResult == true) //there is a process still OK?
                {
                    blTemp = false;
                    break; //No need confirm anymore
                }
            }

            if (blTemp == true)
            {
                intToken = -1; //Marking that these process, if not match Process ID => cannot run!
            } 

            //Check option 
            if (intProcessId != intProcessIdRequest) return "Error: not process want to run. No effect!";

            //Request this process run this step if The Token is not match with the slowest one
            if (this.lstChildProcessModel[intProcessId].intStepPosToken != this.intSlowestToken)
            {
                this.lstChildProcessModel[intProcessId].ChildFuncEx(intStepPos);
            }
           
            return "0";
        }

        //****************************************************

    } //class clsSingleThreadProcessModel

} //End namespace
