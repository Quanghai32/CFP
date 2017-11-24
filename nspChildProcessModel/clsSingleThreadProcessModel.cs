using System.Collections.Generic;
using System.Threading;
using nspCFPExpression;
using nspChildProcessModel;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;


namespace nspSingleThreadProcessModel
{

    public class classProcessTask
    {
        public int intProcessID { get; set; } //Task belong to what process
        public int intStepPos { get; set; } //Task belong to what step position in program list

        public bool blRequestReject { get; set; } //In some case need reject running marking
        //
        public classProcessTask(int intProcessID, int intStepPos)
        {
            this.intProcessID = intProcessID;
            this.intStepPos = intStepPos;
        }
    }

    public class classChildProcessProgressInfo
    {
        //For normal sequence
        public int intCurrentToken { get; set; }

        //For jumping request
        public bool blRequestJump { get; set; } //Indicate request jumping or not
        public int intTargetStep { get; set; } //Indicate what step child process want to jump to
        public int intTargetToken { get; set; } //Indicate target token (calculated from target step number)
    }


    public class clsSingleThreadProcessModel
    {
        //For list of task
        public List<classProcessTask> lstclsProcessTask { get; set; }
        List<int> lst1stFinishPos { get; set; } //Search for first task position in case if fail of each process
        public List<classChildProcessProgressInfo> lstclsChildProcessProgress { get; set; }

        //For Thread Checking
        // public System.Threading.Thread thrdSingleThread { get; set; }

		// For Task Checking
		public Task taskSingleThreadChecking { get; set; }
        public CancellationTokenSource taskSingleThreadCancellationTokenSource { get; set; }

		public List<bool> lstblStepResult { get; set; } //Step is OK or NG for all sheet - Need initialize for this collection

        public int intMarkItemNumFail { get; set; }

        public int intLastStepPos { get; set; } //Marking last step position

        public List<bool> lstblStepChecked { get; set; } //Step is checked or not checked when complete checking process?

        //For Child process of Single Thread class
        public List<nspChildProcessModel.clsChildProcessModel> lstChildProcessModel { get; set; }
        public int intNumChildPro { get; set; }

        public clsCommonSpecialControlFunction clsCommonFunc = new clsCommonSpecialControlFunction();

        //**********************SUPPORT DISPLAY INFO ON USER INTERFACE************************************************************
        public enumChildProcessCheckingMode eRunMode { get; set; }  //For Checking Mode

        public enumChildProcessStatus eSingleThreadProcessStatus { get; set; } //Indicate What status of Checking Process

        public bool blAllowContinueRunning { get; set; } //True if button NEXT is pressed
        public bool blRequestStopRunning { get; set; } //True if button END is pressed
        public int intStepModePosSelected { get; set; } //Indicate what step positon request to stop in 

        //************************************Support for Binding WPF*************************************************************
        public bool blAllowUpdateViewTable { get; set; } //All child process use common View Table, cannot allow all child process update same time
        
        public System.Data.DataTable ViewTable { get; set; }
        //public System.Data.DataTable StepListViewTable { get; set; }

        private static Mutex mut = new Mutex();

        public void UpdateViewTable(int intStepPos)
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            int i, j = 0;
            //Set color for data view
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                if (i == intStepPos)
                {
                    this.ViewTable.Rows[i][10] = System.Windows.Media.Brushes.LightCoral;
                }
                else
                {
                    this.ViewTable.Rows[i][10] = SetColor(i);
                }
            }

            //Set result checking
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                if (this.lstblStepChecked[i] == true)
                {
                    //this.ViewTable.Rows[i][3] = this.lstChildProcessModel[0].clsItemInfo.lstobjStepCheckingData[i].ToString();
                    if(this.lstblStepResult[i]==true)
                    {
                        this.ViewTable.Rows[i][3] = "PASS";
                    }
                    else
                    {
                        this.ViewTable.Rows[i][3] = "FAIL";
                    }
                }
                else
                {
                    this.ViewTable.Rows[i][3] = "";
                }
            }


            //Set result checking color
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                if (this.lstblStepChecked[i] == true)
                {
                    if (this.lstblStepResult[intStepPos] == true) //PASS
                    {
                        this.ViewTable.Rows[intStepPos][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[intStepPos][11] = System.Windows.Media.Brushes.Red;
                    }
                } 
            }


            //Release mutex
            mut.ReleaseMutex();
        }

        public void RefreshViewTable()
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            int i, j = 0;
            //Set color for data view
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
               this.ViewTable.Rows[i][10] = SetColor(i);
            }

            //Set result checking
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                if (this.lstblStepChecked[i] == true)
                {
                    if (this.lstblStepResult[i] == true)
                    {
                        this.ViewTable.Rows[i][3] = "PASS";
                    }
                    else
                    {
                        this.ViewTable.Rows[i][3] = "FAIL";
                    }
                }
                else
                {
                    this.ViewTable.Rows[i][3] = "";
                }
            }


            //Set result checking color
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                if (this.lstblStepChecked[i] == true)
                {
                    if (this.lstblStepResult[i] == true) //PASS
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Red;
                    }
                }
            }

            //Release mutex
            mut.ReleaseMutex();
        }

        private System.Windows.Media.SolidColorBrush SetColor(int i)
        {
            if ((i % 2) == 0)
            {
                return System.Windows.Media.Brushes.White;
            }
            else
            {
                return System.Windows.Media.Brushes.LightGreen;
            }
        }

        public void SetCheckMode(enumChildProcessCheckingMode eMode)
        {
            this.eRunMode = eMode;
        }

        //*************************************************************************************************

        public void SingleThreadIni(List<nspChildProcessModel.clsChildProcessModel> lstInput)
        {
            int i, j = 0;
            bool blTemp = false;
            //Import from System Ini?
            //Note that lstChildProcessModel should be initialized first before pass to single thread class
            this.lstChildProcessModel = lstInput;
            this.intNumChildPro = lstInput.Count;
            this.eRunMode = enumChildProcessCheckingMode.eNormal;

            //Ini for classChildProcessProgressInfo
            this.lstclsChildProcessProgress = new List<classChildProcessProgressInfo>();
            for (i = 0; i < this.lstChildProcessModel.Count;i++)
            {
                classChildProcessProgressInfo clsNewProgress = new classChildProcessProgressInfo();
                this.lstclsChildProcessProgress.Add(clsNewProgress);
            }

            //this.lstblStepResult
            this.lstblStepResult = new List<bool>();
            this.lstblStepChecked = new List<bool>();
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                this.lstblStepResult.Add(blTemp);
                this.lstblStepChecked.Add(blTemp);
            }

            this.eSingleThreadProcessStatus = enumChildProcessStatus.eIni;

            //
            this.clsCommonFunc.intSourcesID = 2;
            this.clsCommonFunc.objSources = this;
            this.clsCommonFunc.intProcessId = -2; //ID of Single Thread process is -2

            this.clsCommonFunc.lstlstCommonCommandAnalyzer = this.clsCommonFunc.CommonSpecialControlIni(this.lstChildProcessModel[0].clsProgramList.lststrSpecialCmd); //Special command area
            this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer = this.clsCommonFunc.CommonTransAreaSpecialControlIni(this.lstChildProcessModel[0].clsProgramList.lststrTransAreaSpecialCmd); //Special command in Transmission area
            this.clsCommonFunc.lstlstCommonParaCommandAnalyzer = this.clsCommonFunc.CommonParaSpecicalControlIni(this.lstChildProcessModel[0].clsProgramList.lstlstobjStepPara); //parameter area
        
            //Create new list of task
            for(i=0;i<this.lstChildProcessModel[0].lstChildCheckAll.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel.Count;j++)
                {
                    classProcessTask clsNewTask = new classProcessTask(j,this.lstChildProcessModel[j].lstChildCheckAll[i].intStepPos);
                    this.lstclsProcessTask.Add(clsNewTask);
                }
            }

            //Search for first task position in case if fail of each process
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                int inttemp = this.lstclsProcessTask.Count; //Default is the last position => It mean surely ending checking is all case
                //
                for (j = 0; j < this.lstclsProcessTask.Count; j++)
                {
                    if (this.lstclsProcessTask[j].intProcessID != i) continue;
                    //Confirm class 4
                    if(this.lstChildProcessModel[i].lstTotalStep[this.lstclsProcessTask[j].intStepPos].intStepClass == 4)
                    {
                        inttemp = j;
                        break;
                    }
                    //Confirm class 50
                    if (this.lstChildProcessModel[i].lstTotalStep[this.lstclsProcessTask[j].intStepPos].intStepClass == 50)
                    {
                        inttemp = j;
                        break;
                    }
                }
                this.lst1stFinishPos.Add(inttemp);
            }
        }


        //********************************************************************************************************************************
        public void StartTaskThreadChecking()
        {
            this.taskSingleThreadCancellationTokenSource = new CancellationTokenSource();
            foreach (var process in this.lstChildProcessModel)
            {
                process.taskCheckingProcessCancelTokenSource = new CancellationTokenSource(); // Need to do this because Result OK/NG calculation.
                process.taskBackGroundCancelTokenSource = new CancellationTokenSource();
            }
            //this.taskCheckingProcess = Task.Factory
            //                .StartNew(t => this.ThreadChecking(),
            //                    TaskCreationOptions.LongRunning,
            //                    this.taskCheckingProcessCancelTokenSource.Token
            //                );
           
            this.taskSingleThreadChecking = Task.Factory
                    .StartNew(t => this.SingleThreadSequence(), 
                    TaskCreationOptions.LongRunning,
                    this.taskSingleThreadCancellationTokenSource.Token
                    );
            Debug.WriteLine("Start Task SingleThread Checking.");
        }

        public void CancelTaskThreadChecking()
        {
            this.taskSingleThreadCancellationTokenSource.Cancel();
            foreach(var process in this.lstChildProcessModel)
            {
                process.CancelTaskChecking(); // Need to do this because Result OK/NG calculation.
            }
            Debug.WriteLine("Cancel Task SingleThread Checking.");
        }

        /// <summary>
        /// New sequence checking for single thread model
        /// Rule of sequence:
        ///     + Each step number of each child process will be execute until reach the last step
        ///     + If 1 step of 1 process fail with Test Class = 3 then:
        ///              => Do not check remaining step with class 3 of that child process
        ///              => Check other step of other process normally
        ///              => After all class 3 of all step finish checked, from class 4, fail child process step will be checked as normal
        /// </summary>
        public void SingleThreadSequence()
        {
            int i,j,k;
            bool blCalResult = false;

            #region _reset
            //Change status
            this.eSingleThreadProcessStatus = enumChildProcessStatus.eChecking;

            //1. Prepare something before checking
            //For all Child Process
            for (k = 0; k < this.intNumChildPro; k++)
            {
                //Check if skip mode
                if (this.lstChildProcessModel[k].blSkipModeRequest == true)
                {
                    continue;
                }

                //
                this.lstChildProcessModel[k].ResetParameterEx();

                this.lstChildProcessModel[k].clsItemResult.blItemCheckingResult = true;
                this.lstChildProcessModel[k].eChildProcessStatus = nspChildProcessModel.enumChildProcessStatus.eChecking;

                //Before checking started, clear all data before
                for (i = 0; i < this.lstChildProcessModel[k].lstChildCheckAll.Count; i++)
                {
                    int intStepPos = this.lstChildProcessModel[k].lstChildCheckAll[i].intStepPos;

                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].blStepResult = false;
                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].dblStepTactTime = 0;
                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].strStepErrMsg = "";
                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].blStepChecked = false;
                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].intStartTickMark = 0;
                    this.lstChildProcessModel[k].lstTotalStep[intStepPos].intExecuteTimes = 0;
                }

                this.lstChildProcessModel[k].intStepPosRunning = 0;
                this.lstChildProcessModel[k].blResultAlreadyFail = false;

                //Reset for all items of child processes
                for(j=0;j<this.lstChildProcessModel[k].lstclsItemCheckInfo.Count;j++)
                {
                    this.lstChildProcessModel[k].lstclsItemCheckInfo[j].blResultAlreadyFail = false;
                    this.lstChildProcessModel[k].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult = true;
                }

                //clear binding view
                this.lstChildProcessModel[k].ClearBindingView();
                //Clear info on Process View
                this.lstChildProcessModel[k].ClearViewTableInfo();
            }

            //For Single Thread Class
            for (i = 0; i < this.lstblStepResult.Count; i++)
            {
                this.lstblStepResult[i] = false;
            }
            this.intMarkItemNumFail = 0;

            //Reset flag marking checked step
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                this.lstblStepChecked[i] = false;
            }

            //For all child process progress
            for (i = 0; i < this.lstclsChildProcessProgress.Count;i++)
            {
                this.lstclsChildProcessProgress[i].blRequestJump = false;
                this.lstclsChildProcessProgress[i].intTargetStep = -1;
                this.lstclsChildProcessProgress[i].intTargetToken = -1;
            }

            //
            for (i = 0; i < this.lstclsProcessTask.Count; i++)
            {
                this.lstclsProcessTask[i].blRequestReject = false;
            }

            #endregion

            //Execute each task in task list
            for (i = 0; i < this.lstclsProcessTask.Count; i++)
            {
                // Check If CancellationToken Request
                if (this.taskSingleThreadCancellationTokenSource.IsCancellationRequested) break;

                //
                int intProcessID = this.lstclsProcessTask[i].intProcessID;
                int intStepPos = this.lstclsProcessTask[i].intStepPos;
                int intItemID = this.lstChildProcessModel[intProcessID].FindItemID(intStepPos);
                int intGroupNumber = this.lstChildProcessModel[intProcessID].FindGroupNumber(intStepPos);

                //Do not run step belong to Item which does not belong to Child Process
                if(intGroupNumber == -2) //Out of range
                {
                    continue;
                }

                //Do not allow child process skipped running
                if (this.lstChildProcessModel[intProcessID].blSkipModeRequest == true)
                {
                    this.NotActiveStepTask(intProcessID, intStepPos);
                    continue;
                }

                //Check if request reject
                //When reject request exists?
                //  1. When Jumping command
                //  2. 
                if (this.lstclsProcessTask[i].blRequestReject == true)
                {
                    this.NotActiveStepTask(intProcessID, intStepPos);
                    continue;
                } 

                //Check if jumping request coming and handle it
                #region HandleJumpingRequest
                if (this.lstclsChildProcessProgress[intProcessID].blRequestJump == true)
                {
                    if (i < this.lstclsChildProcessProgress[intProcessID].intTargetToken) //Jump forward
                    {
                        this.NotActiveStepTask(intProcessID, intStepPos);
                        continue;
                    }
                    else if (i > this.lstclsChildProcessProgress[intProcessID].intTargetToken) //Jump backward
                    {
                        i = this.lstclsChildProcessProgress[intProcessID].intTargetToken; //Force jump backward
                        intStepPos = this.lstclsProcessTask[i].intStepPos;

                        //In case of jumping backward, other child process need to reject steps which already checked!
                        for(j=0;j<this.lstChildProcessModel.Count;j++)
                        {
                            if (j == intProcessID) continue;
                            //
                            if(this.lstclsChildProcessProgress[j].blRequestJump == true) //Other process also have jumping request
                            {
                                //Masking all steps from current token of single thread sequence to child process its own token or Target jumping token
                                if(this.lstclsChildProcessProgress[j].intCurrentToken < this.lstclsChildProcessProgress[j].intTargetToken)
                                {
                                    for (k = i; k <= this.lstclsChildProcessProgress[j].intCurrentToken; k++)
                                    {
                                        if (this.lstclsProcessTask[k].intProcessID != j) continue;
                                        //
                                        this.lstclsProcessTask[k].blRequestReject = true;
                                    }
                                }
                                else
                                {
                                    for (k = i; k < this.lstclsChildProcessProgress[j].intTargetToken; k++)
                                    {
                                        if (this.lstclsProcessTask[k].intProcessID != j) continue;
                                        //
                                        this.lstclsProcessTask[k].blRequestReject = true;
                                    }
                                }
                            }
                            else //No jumping request, normal running
                            {
                                //Masking all steps from current token of single thread sequence to child process its own token
                                for(k=i;k<=this.lstclsChildProcessProgress[j].intCurrentToken;k++)
                                {
                                    if (this.lstclsProcessTask[k].intProcessID != j) continue;
                                    //
                                    this.lstclsProcessTask[k].blRequestReject = true;
                                }
                            }
                        }
                    }
                    //Clear jumping
                    this.lstclsChildProcessProgress[intProcessID].blRequestJump = false;
                    this.lstclsChildProcessProgress[intProcessID].intTargetToken = -1;
                    this.lstclsChildProcessProgress[intProcessID].intTargetStep = -1;
                }
                #endregion

                //Check if child process is already fail. If fail, do not allow step class 3 of that child process execute
                if (intGroupNumber > -1) //Found group number
                {
                    if (this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo[intGroupNumber - 1].clsItemResult.blItemCheckingResult == false)
                    {
                        if (this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].intStepClass == 3)
                        {
                            //If step already fail, then do special control and goto another task
                            this.NotActiveStepTask(intProcessID, intStepPos);
                            continue;
                        }
                    }
                }

                //Do special control for child process - Before
                int intTempTokenBefore = -2;
                this.lstChildProcessModel[intProcessID].ChildProcessSpecialControlCommandEx(1, intStepPos, ref intTempTokenBefore);

                //Do special control - Before. Follow single thread process special control
                this.SingleThreadProcessSpecialControlCommandEx(intProcessID, 1, intStepPos, ref intTempTokenBefore);

                //Executed step
                if (intTempTokenBefore != -100) //Token = -100 mean request to reject checking -SFDONE() function
                {
                    bool blTempResult = this.lstChildProcessModel[intProcessID].ChildFuncEx(intStepPos);
                }

                //Do special control for child process - after
                int intTempTokenAfter = -2;
                object objRet = this.lstChildProcessModel[intProcessID].ChildProcessSpecialControlCommandEx(0, intStepPos, ref intTempTokenAfter);

                //Update for child process progress
                #region updatechildprogress
                this.lstclsChildProcessProgress[intProcessID].intCurrentToken = i;

                if (intTempTokenAfter != -2)//Request update token from child process comming
                {
                    intTempTokenAfter++; //In fact, when do jumping, target token already reduce 1 to cooperate to for(;;i++) loop 
                    //Find step want to jump to
                    for (j = 0; j < this.lstChildProcessModel[intProcessID].lstChildCheckAll.Count; j++)
                    {
                        if (intTempTokenAfter == j) //Found
                        {
                            this.lstclsChildProcessProgress[intProcessID].blRequestJump = true;
                            this.lstclsChildProcessProgress[intProcessID].intTargetStep = this.lstChildProcessModel[intProcessID].lstChildCheckAll[j].intStepNumber;
                            //Find token
                            for (k = 0; k < this.lstclsProcessTask.Count; k++)
                            {
                                if (this.lstclsProcessTask[k].intProcessID != intProcessID) continue;
                                //
                                if (this.lstChildProcessModel[intProcessID].lstTotalStep[this.lstclsProcessTask[k].intStepPos].intStepNumber ==
                                    this.lstclsChildProcessProgress[intProcessID].intTargetStep)
                                {
                                    this.lstclsChildProcessProgress[intProcessID].intTargetToken = k;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                #endregion

                //Do special control - after. Follow single thread process special control
                this.SingleThreadProcessSpecialControlCommandEx(intProcessID, 0, intStepPos, ref intTempTokenBefore);

                //Cal all result necessary when not yet cal and reach first step of class 50
                //If there is no step belong to class 50, then after exit loop, result calculated
                if ((blCalResult == false) && (this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].intStepClass == 50))
                {
                    blCalResult = true;
                    this.SingleThreadProcessCalResult();
                }

                //*************************Following task only accept to run 1 time with each step of al child process**************************
                //Calculate Result of each step in SINGLE THREAD MODE
                #region _UpdateView

                //Find biggest process which is OK
                int intBiggestID = 0;
                for (j = 0; j < this.lstChildProcessModel.Count; j++)
                {
                    if (this.lstChildProcessModel[j].blSkipModeRequest == false)
                    {
                        if (this.lstChildProcessModel[j].clsItemResult.blItemCheckingResult == true)
                        {
                            intBiggestID = j;
                        }
                        else //Fail at biggest one?
                        {
                            if (this.lstChildProcessModel[j].clsItemResult.intStepFailPos == intStepPos)
                            {
                                intBiggestID = j;
                            }
                        }
                    }
                }
                if (intProcessID == intBiggestID)
                {
                    //Marking single thread step aready checked
                    this.lstblStepChecked[intStepPos] = true;
                    //Cal Result
                    this.lstblStepResult[intStepPos] = true;
                    for (k = 0; k < this.intNumChildPro; k++) //Counting each child process
                    {
                        //Check if skip mode
                        if (this.lstChildProcessModel[k].blSkipModeRequest == true)
                        {
                            continue;
                        }

                        if ((this.lstChildProcessModel[k].lstTotalStep[intStepPos].blStepResult == false) && (this.lstChildProcessModel[k].lstTotalStep[intStepPos].intStepClass == 3))
                        {
                            this.lstblStepResult[intStepPos] = false;
                            break;
                        }
                    }

                    //Update view
                    if (this.eRunMode == enumChildProcessCheckingMode.eNormal) //Normal mode
                    {
                        UpdateStepInfo2(intStepPos);
                    }
                    else //Debug mode
                    {
                        UpdateViewTable(intStepPos);
                    }
                }
                #endregion

                //Debug mode (Normal
                //Find smallest process which is OK
                int intSmallestOKID = 0;
                for (j = 0; j < this.lstChildProcessModel.Count; j++)
                {
                    if ((this.lstChildProcessModel[j].clsItemResult.blItemCheckingResult == true) && (this.lstChildProcessModel[j].blSkipModeRequest == false))
                    {
                        intSmallestOKID = j;
                        break;
                    }
                }

                //if (intProcessID == intSmallestOKID)
                if (intProcessID == intBiggestID)
                {
                    #region DebugModeHandle

                    //Checking Handle Mode
                    ////////////////////SINGLE THREAD - NORMAL MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumChildProcessCheckingMode.eNormal)
                    {
                        //Do nothing
                    }

                    ////////////////////SINGLE THREAD - SINGLE MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumChildProcessCheckingMode.eSingle)
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            Application.DoEvents();
                            //
                            if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                            {
                                this.blRequestStopRunning = false; //Reset for next time
                                i = this.lstclsProcessTask.Count; //Ignore all next steps and come to ending
                                break;
                            }
                        }
                    } //End If SINGLE MODE

                    ////////////////////SINGLE THREAD - STEP MODE///////////////////////////////////////////////////////
                    if (this.eRunMode == enumChildProcessCheckingMode.eStep)
                    {
                        //Stop if reach selected step
                        if ((this.intStepModePosSelected == this.lstChildProcessModel[0].lstTotalStep[intStepPos].intStepPos) ||
                            ((this.lstblStepResult[intStepPos] == false) && (this.lstblStepChecked[intStepPos] == true)))
                        {
                            this.blAllowContinueRunning = false;
                            while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                            {
                                Application.DoEvents();
                                //
                                if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                                {
                                    this.blRequestStopRunning = false; //Reset for next time
                                    i = this.lstclsProcessTask.Count; //Ignore all next steps and come to ending
                                    break;
                                }
                            }
                        }

                    } //End If STEP MODE

                    ////////////////////SINGLE THREAD - FAIL MODE///////////////////////////////////////////////////////
                    if (this.eRunMode == enumChildProcessCheckingMode.eFail)
                    {
                        //Stop if fail or reach selected step
                        if ((this.lstblStepResult[intStepPos] == false) && (this.lstblStepChecked[intStepPos] == true))
                        {
                            this.blAllowContinueRunning = false;
                            while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                            {
                                Application.DoEvents();
                                //
                                if (this.blRequestStopRunning == true) //If request ending from user (End button pressed)
                                {
                                    this.blRequestStopRunning = false; //Reset for next time
                                    i = this.lstclsProcessTask.Count; //Ignore all next steps and come to ending
                                    break;
                                }
                            }
                        }
                    }

                    ////////////////////SINGLE THREAD - ALL MODE////////////////////////////////////////////////////////
                    if (this.eRunMode == enumChildProcessCheckingMode.eAll)
                    {
                        //Automatically run all steps without caring result of each step OK or NG
                    }

                    #endregion
                }
            } //End i loop

            //Calculate result
            if (blCalResult == false) this.SingleThreadProcessCalResult();

            //Change status of child process
            for (k = 0; k < this.intNumChildPro; k++)
            {
                //Check if skip mode
                if (this.lstChildProcessModel[k].blSkipModeRequest == true)
                {
                    continue;
                }
                this.lstChildProcessModel[k].eChildProcessStatus = nspChildProcessModel.enumChildProcessStatus.eFinish;
                this.lstChildProcessModel[k].UpdateFinishBindingView();
            }

            this.eSingleThreadProcessStatus = enumChildProcessStatus.eFinish;
        }

        //******************************
        private int FindSubChildID(int intChildProcessID, int intStepPos)
        {
            //
            int intSubChildID = -1;
            //
            if (this.lstChildProcessModel[intChildProcessID].blGroupMode == true)
            {
                int j = 0;
                for (j = 0; j < this.lstChildProcessModel[intChildProcessID].lstclsItemCheckInfo.Count; j++)
                {
                    if (this.lstChildProcessModel[intChildProcessID].lstTotalStep[intStepPos].strGroupNumber.Trim() == (j + 1).ToString())
                    {
                        intSubChildID = j;
                        break;
                    }
                }
            }
            //
            return intSubChildID;
        }

        //******************************
        private void NotActiveStepTask(int intProcessID, int intStepPos)
        {
            //Do special control only. Note that do not allow these process to change token here!
            int intTempToken = 0;

            this.lstChildProcessModel[intProcessID].ChildProcessSpecialControlCommandEx(1, intStepPos, ref intTempToken);
            //Do special control - Before. Follow single thread process special control
            this.SingleThreadProcessSpecialControlCommandEx(intProcessID, 1, intStepPos, ref intTempToken);

            this.lstChildProcessModel[intProcessID].ChildProcessSpecialControlCommandEx(0, intStepPos, ref intTempToken);
            //Do special control - Before. Follow single thread process special control
            this.SingleThreadProcessSpecialControlCommandEx(intProcessID, 0, intStepPos, ref intTempToken);
        }

        //******************************
        public async void UpdateStepInfo2(int intStepPos)
        {
            await TestInfo2(intStepPos);
        }

        public Task TestInfo2(int intStepPos)
        {
            return Task.Run(() =>
            {
                //Only update on main info view if not yet finish!
                if (this.eSingleThreadProcessStatus != enumChildProcessStatus.eFinish)
                {
                    //Update View Table - result of each step
                    UpdateViewTable(intStepPos);
                }
            });
        }

        //******************************
        public void SingleThreadProcessCalResult()
        {
            int i = 0;
            //Calculate result of each child process
            for (i = 0; i < this.intNumChildPro; i++)
            {
                //Check if skip mode
                if (this.lstChildProcessModel[i].blSkipModeRequest == true)
                {
                    continue;
                }
                this.lstChildProcessModel[i].ChildProcessCalResult();
            }
        }

        //******************************For Single Thread Process Special Control**************************************
        public object SingleThreadProcessSpecialControlCommandEx(int intProcessId, int intAffectArea, int intStepPos, ref int intToken)
        {
            int i = 0;
            object objTemp = null;
            for (i = 0; i < this.lstChildProcessModel[intProcessId].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
            {
                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intTargetProcessID != 3) continue; //Not target single thread. No execute.

                //Check affect area
                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intAffectArea != intAffectArea) continue;

                //Execute if is special command
                this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].intProcessId = intProcessId;
                objTemp = this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].evaluate();


                //Check return data
                if (objTemp is clsCommonCommandGuider)
                {
                    clsCommonCommandGuider clsCommandTemp = (clsCommonCommandGuider)objTemp;
                    //Update token if request
                    if (clsCommandTemp.blRequestUpdateToken == true)
                    {
                        intToken = clsCommandTemp.intToken;
                    }
                }
            }
            //
            return objTemp;
        }
        
        //constructor
        public clsSingleThreadProcessModel()
        {
            this.lstclsProcessTask = new List<classProcessTask>();
            this.lst1stFinishPos = new List<int>();
            this.taskSingleThreadCancellationTokenSource = new CancellationTokenSource();
        }

    } //class clsSingleThreadProcessModel

} //End namespace
