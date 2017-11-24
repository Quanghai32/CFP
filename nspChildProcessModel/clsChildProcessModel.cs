using System;
using System.Collections.Generic;
using System.Windows.Forms;
using nspCFPExpression;
using nspProgramList;
using nspCFPInfrastructures;
using nspMEFLoading;
using Microsoft.Practices.Prism.Mvvm;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Threading;
using System.Diagnostics;

namespace nspChildProcessModel
{
    public enum enumChildProcessCheckingMode
    {
        eNormal, //Running normally
        eSingle, //Running each step & wait for allowing signal (button press) for continue
        eStep, //Running until reach desired step, continue if allowing condition met (button press)
        eFail, //Running until reach fail step => Stop
        eAll, //Running all step, no care about result
        eNotrecognize //Not recognize mode
    }

    public enum enumChildProcessStatus
    {
        eIni, //Have just ini
        eChecking, //Checking in Process - Request update info on UI
        eFinish, //Checking Finish - Request update info on UI
        eAfterFinish, //After finish process
        eDoNothing //No active, no request anything
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    public class classItemImformation
    {
        //ID of child process 
        public int intItemID;

        //Group Number - Assign
        public int intGroupNumber; //correspond to group number in program list
        public bool blResultAlreadyFail { get; set; } //Indicate the result of child process (when acting as sub-child process) already fail. And normally no need check further

        //Saving data control
        public bool blRequestSavingData { get; set; }

        //Skip or not? => Allow user to skip some process checking item
        public bool blSkipModeRequest { get; set; }

        //For support binding View
        public clsChildModelBindingSupport clsChildModelBindingView { get; set; }

        //For Setting File
        public clsProcessSettingReading clsChildSetting { get; set; }

        //Result class
        public clsItemResultInformation clsItemResult { get; set; }

        //For Saving Data
        public List<string> lststrProgramListUserPreInfo { get; set; } //Saving pre-infor to data saving file (CSV file)
        public List<string> lststrProgramListUserAfterInfo { get; set; } //Saving After-infor to data saving file (CSV file)

        public List<string> lststrStepListUserPreInfo { get; set; } //Saving pre-infor to data saving file (CSV file) - For step list
        public List<string> lststrStepListUserAfterInfo { get; set; } //Saving After-infor to data saving file (CSV file) - For step list

        //For Origin Step list
        public classStepList clsStepList { get; set; }

        //For Checking Process Program List
        public classProgramList clsProgramList { get; set; }

        //Define Sub List
        public List<classStepDataInfor> lstTotalStep = new List<classStepDataInfor>();
        public List<classStepDataInfor> lstChildIni = new List<classStepDataInfor>(); //0
        public List<classStepDataInfor> lstChildStartPoll = new List<classStepDataInfor>(); //1
        public List<classStepDataInfor> lstChildAfterStart = new List<classStepDataInfor>(); //2
        public List<classStepDataInfor> lstChildChecking = new List<classStepDataInfor>(); //3
        public List<classStepDataInfor> lstChildFinish = new List<classStepDataInfor>(); //4
        public List<classStepDataInfor> lstChildThreadCheck = new List<classStepDataInfor>(); //2-3-4 combine
        public List<classStepDataInfor> lstChildCheckAll = new List<classStepDataInfor>(); //2-3-4-50 combine
        public List<classStepDataInfor> lstChildAfterFinish = new List<classStepDataInfor>(); //50
        public List<classStepDataInfor> lstChildCheckingEndService = new List<classStepDataInfor>(); //51
        public List<classStepDataInfor> lstChildBackgroundPolling = new List<classStepDataInfor>(); //100
        public List<classStepDataInfor> lstChildUserEnd = new List<classStepDataInfor>(); //1000

        //////////////////////////////////////////////////////////////////
        public bool blAllowUpdateStepListViewTable { get; set; }
        public System.Data.DataTable StepListViewTable { get; set; }
        private static Mutex mut = new Mutex();

        //////////////////////////////////////////////////////////////////
        //Analyze Program List to Sub-List for control checking sequence
        public void AnalyzeProgramList(List<classStepDataInfor> lstInput)
        {
            int i = 0;

            this.lstTotalStep = lstInput;
            //Analyze all item in Program List & assign them to proper category
            for (i = 0; i < lstInput.Count; i++)
            {
                //Check if Row input belong to Main Sequence
                if (lstInput[i].intStepSequenceID != 0) //Not adding if step not belong to Main Sequence
                {
                    continue;
                }

                //
                classStepDataInfor temp = lstInput[i];

                switch (lstInput[i].intStepClass)
                {
                    case 0: //Ini
                        this.lstChildIni.Add(temp);
                        break;
                    case 1: //Start polling
                        this.lstChildStartPoll.Add(temp);
                        break;
                    case 2: //After Start
                        this.lstChildAfterStart.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;
                    case 3: //Checking Process
                        this.lstChildChecking.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;
                    case 4: //Finish
                        this.lstChildFinish.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;

                    case 50: //After finish
                        this.lstChildCheckingEndService.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;

                    case 51:
                        this.lstChildCheckingEndService.Add(temp);
                        break;

                    case 100: //Background polling
                        this.lstChildBackgroundPolling.Add(temp);
                        break;
                    case 1000: //User End
                        this.lstChildUserEnd.Add(temp);
                        break;
                    default:
                        MessageBox.Show("Error: Cannot recognize Child Process Step Class [" + lstInput[i].intStepClass.ToString() + "]");
                        break;
                }
            }
        }

        //Constructor
        public classItemImformation()
        {
            this.clsChildSetting = new clsProcessSettingReading();
            this.clsItemResult = new clsItemResultInformation();
            this.clsChildModelBindingView = new clsChildModelBindingSupport();
            this.lststrProgramListUserPreInfo = new List<string>();
            this.lststrProgramListUserAfterInfo = new List<string>();
            this.lststrStepListUserPreInfo = new List<string>();
            this.lststrStepListUserAfterInfo = new List<string>();
            this.blRequestSavingData = true;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    public class clsChildProcessModel : BindableBase
    {
        //ID of child process 
        public int intProcessID;

        //Prepare for Unify Model
        public classCommonMethod clsCommonMethod { get; set; }

        //For setting in Group Mode or not
        public bool blGroupMode { get; set; } //Indicate setting in Group Mode or not
        public List<classItemImformation> lstclsItemCheckInfo { get; set; } //In Group Mode, each child process has it own group of child process!
        public bool blResultAlreadyFail { get; set; } //Indicate the result of child process (when acting as sub-child process) already fail. And normally no need check further

        //For MEF
        public nspMEFLoading.clsMEFLoading.clsExtensionHandle clsChildExtension { get; set; }

        //Saving data control
        public bool blRequestSavingData { get; set; }

        //Skip or not? => Allow user to skip some process checking item
        public bool blSkipModeRequest { get; set; }

        //For holding instance of Master Process & Child control process
        //For extract Child Control model object
        public object objChildControlModel { get; set; }
        public bool blFoundChildControlModel { get; set; }

        //For extract Master Process model object
        public object objMasterProcessModel { get; set; }
        public bool blFoundMasterProcessModel { get; set; }

        //For support binding View
        public clsChildModelBindingSupport clsChildModelBindingView { get; set; }

        ////For Thread Checking
        //public System.Threading.Thread thrdChildProcess { get; set; }

		// For Task Checking
		public Task taskCheckingProcess { get; set; }
        public CancellationTokenSource taskCheckingProcessCancelTokenSource { get; set; }

        //For background thread
        // public System.Threading.Thread thrdBackGround { get; set; }
        public Task taskBackGround { get; set; }
        public CancellationTokenSource taskBackGroundCancelTokenSource { get; set; }
        public bool blRequestBackgroundStop { get; set; }

        //For Independent mode - Polling start timer
        public System.Timers.Timer tmrIndependent { get; set; }
        public bool blRequestCheckingEndService { get; set; }

        //For saving System checking mode
        public string strSystemCheckingMode { get; set; } //Normal Mode - Service Mode - Pm Mode...

        //For Setting File
        public clsProcessSettingReading clsChildSetting { get; set; }

        //Result class
        public clsItemResultInformation clsItemResult { get; set; }

        //For Saving Data
        public List<string> lststrProgramListUserPreInfo { get; set; } //Saving pre-infor to data saving file (CSV file)
        public List<string> lststrProgramListUserAfterInfo { get; set; } //Saving After-infor to data saving file (CSV file)

        public List<string> lststrStepListUserPreInfo { get; set; } //Saving pre-infor to data saving file (CSV file) - For step list
        public List<string> lststrStepListUserAfterInfo { get; set; } //Saving After-infor to data saving file (CSV file) - For step list

        //For Origin Step list
        public classStepList clsStepList { get; set; }

        //For Checking Process Program List
        public classProgramList clsProgramList { get; set; }

        //Define Sub List
        public List<classStepDataInfor> lstTotalStep = new List<classStepDataInfor>();
        public List<classStepDataInfor> lstChildIni = new List<classStepDataInfor>(); //0
        public List<classStepDataInfor> lstChildStartPoll = new List<classStepDataInfor>(); //1
        public List<classStepDataInfor> lstChildAfterStart = new List<classStepDataInfor>(); //2
        public List<classStepDataInfor> lstChildChecking = new List<classStepDataInfor>(); //3
        public List<classStepDataInfor> lstChildFinish = new List<classStepDataInfor>(); //4
        public List<classStepDataInfor> lstChildThreadCheck = new List<classStepDataInfor>(); //2-3-4 combine
        public List<classStepDataInfor> lstChildCheckAll = new List<classStepDataInfor>(); //2-3-4-50 combine
        public List<classStepDataInfor> lstChildAfterFinish = new List<classStepDataInfor>(); //50
        public List<classStepDataInfor> lstChildCheckingEndService = new List<classStepDataInfor>(); //51
        public List<classStepDataInfor> lstChildBackgroundPolling = new List<classStepDataInfor>(); //100
        public List<classStepDataInfor> lstChildUserEnd = new List<classStepDataInfor>(); //1000

        //For calculate tact time
        public int intStartTick { get; set; } //marking time to start checking

        //**********************SUPPORT DISPLAY INFO ON USER INTERFACE************************************************************
        public enumSystemRunningMode eChildRunningMode { get; set; } //This should be imported from Child Control Class

        public enumChildProcessCheckingMode eChildCheckMode { get; set; }  //For Checking Mode
        public bool blAllowContinueRunning { get; set; } //True if button NEXT is pressed
        public bool blRequestStopRunning { get; set; } //True if button END is pressed
        public int intStepModePosSelected { get; set; } //Indicate what step positon request to stop in 

        public enumChildProcessStatus eChildProcessStatus { get; set; } //Indicate What status of Checking Process

        public int intStepPosRunning; //Indicate what step (Position) is running
        //public int intStepLastPosRunning { get; set; } //Marking Last step running

        //For Child Process sequence testing
        public classSequenceTestData clsSeqenceTestData { get; set; }
        public bool blActiveTestingSequence { get; set; }

        //************************************Support for Binding WPF*************************************************************
        public bool blAllowUpdateViewTable { get; set; } //All child process use common View Table, cannot allow all child process update same time
        public System.Data.DataTable ViewTable { get; set; }

        public System.Data.DataTable dataTableOptionView { get; set; }

        private static Mutex mut = new Mutex(); //For Program list view table
        private static Mutex mut2 = new Mutex(); //For step list view table
        
        //*************************************************************************
        public void UpdateAllStepResultViewTable() //
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            int i, j = 0;

            //Check if running step is belong to main thread or background thread
            //Set result checking
            for (i = 0; i < this.lstTotalStep.Count; i++) //Maybe async update not ontime finish => when finish checking, need to update all again
            {
                //Reset all active color
                this.ViewTable.Rows[i][10] = SetColor(i);

                //
                if (this.lstTotalStep[i].blStepChecked == true)
                {
                    //Update result value
                    this.ViewTable.Rows[i][3] = this.GetConvertDataResult(i);

                    //Update result color
                    if (this.lstTotalStep[i].blStepResult == true) //PASS
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Red;
                    }

                    //View Origin step list result
                    for (j = 0; j < this.clsStepList.lstExcelList.Count; j++)
                    {
                        //Check if step is belong to a group checking step in program list of origin step
                        if (this.lstTotalStep[i].strOriginStepNumber == this.clsStepList.lstExcelList[j].intStepNumber.ToString())
                        {
                            //Check if step is representative step => allow to update
                            if(this.lstTotalStep[i].intStepNumber == this.clsStepList.lstExcelList[j].intStepNumber)
                            {
                                //Find group number
                                int intGroupNum = this.FindGroupNumber(i);
                                if (intGroupNum >= 0)
                                {
                                    if (this.lstclsItemCheckInfo[intGroupNum - 1].blAllowUpdateStepListViewTable == true)
                                    {
                                        //Reset active color
                                        this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[j][10] = SetColor(j);
                                        //Result
                                        var test = this.ViewTable.Rows[i][3];

                                        this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[j][3] = this.ViewTable.Rows[i][3];
                                        //Color
                                        this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[j][11] = this.ViewTable.Rows[i][11];
                                    }
                                }
                                //If already update => no need update again => exit from loop
                                break;
                            }
                        }
                    }
                }
                else
                {
                    this.ViewTable.Rows[i][3] = "";
                }
            }
            //Release mutex
            mut.ReleaseMutex();
        }

        public void UpdateStepResultViewTable(int intStepPos) //Not update color - separate to UpdateActiveColorViewTable() function 
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            //
            int i = 0;

            //Check if running step is belong to main thread or background thread
            //Set result checking
            if (this.lstTotalStep[intStepPos].blStepChecked == true)
            {
                //Test
                this.ViewTable.Rows[intStepPos][10] = System.Windows.Media.Brushes.Orange;

                //Check null
                if (this.lstTotalStep[intStepPos].objStepCheckingData == null)
                {
                    this.ViewTable.Rows[intStepPos][3] = "null";
                }
                else
                {
                    //Set result value
                    this.ViewTable.Rows[intStepPos][3] = this.GetConvertDataResult(intStepPos);

                    //Set result color
                    if (this.lstTotalStep[intStepPos].blStepResult == true) //PASS
                    {
                        this.ViewTable.Rows[intStepPos][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[intStepPos][11] = System.Windows.Media.Brushes.Red;
                    }
                }

                //View Origin step list result
                for (i = 0; i < this.clsStepList.lstExcelList.Count; i++)
                {
                    if (this.lstTotalStep[intStepPos].strOriginStepNumber == this.clsStepList.lstExcelList[i].intStepNumber.ToString())
                    {
                        //Only representative class has the right to update
                        if(this.lstTotalStep[intStepPos].intStepNumber == this.clsStepList.lstExcelList[i].intStepNumber)
                        {
                            //Find group number
                            int intGroupNum = this.FindGroupNumber(intStepPos);
                            if (intGroupNum >= 0)
                            {
                                if (this.lstclsItemCheckInfo[intGroupNum - 1].blAllowUpdateStepListViewTable == true)
                                {
                                    //set result value
                                    this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[i][3] = this.ViewTable.Rows[intStepPos][3];
                                    //set result color
                                    this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[i][11] = this.ViewTable.Rows[intStepPos][11];
                                }
                            }
                            break;
                        }
                    }
                }
            }
            else
            {
                this.ViewTable.Rows[intStepPos][3] = "";
            }

            //Release mutex
            mut.ReleaseMutex();
        }

        public void UpdateActiveColorViewTable(int intStepPos)
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //
            if (this.eChildProcessStatus == enumChildProcessStatus.eFinish) return;

            //
            if (intStepPos != this.intStepPosRunning) return;


            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            int i = 0;
            //Set color for data view
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                if (i == intStepPos) //Change color of running step
                {
                    //we need separate 2 case: main thread step & background step
                    if (intCalThreadID(intStepPos) == 1) //background thread
                    {
                        this.ViewTable.Rows[i][10] = System.Windows.Media.Brushes.Orange;
                    }
                    else //Main thread
                    {
                        this.ViewTable.Rows[i][10] = System.Windows.Media.Brushes.LightCoral;
                    }
                }
                else //Reset color of not running step
                {
                    if (intCalThreadID(intStepPos) == 1) //background thread - not change color of main thread steps
                    {
                        if (this.intCalThreadID(i) == 1) //belong to background thead
                        {
                            this.ViewTable.Rows[i][10] = SetColor(i);
                        }
                    }
                    else //Main thread - not change color of background thread steps
                    {
                        if (this.intCalThreadID(i) == 0) //belong to main thead
                        {
                            this.ViewTable.Rows[i][10] = SetColor(i);
                        }
                    }
                }
            }

            //Set Color for Steplist view step
            if(this.clsChildSetting.blUsingOriginSteplist==true)
            {
                for (i = 0; i < this.clsStepList.lstExcelList.Count; i++)
                {
                    //Find group number
                    int intGroupNum = this.FindGroupNumber(intStepPos);
                    if (intGroupNum < 0) continue;
                    if (this.lstclsItemCheckInfo[intGroupNum - 1].blAllowUpdateStepListViewTable == false) continue;

                    if (this.lstTotalStep[intStepPos].strOriginStepNumber.Trim() == this.clsStepList.lstExcelList[i].intStepNumber.ToString())
                    {
                        this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[i][10] = System.Windows.Media.Brushes.LightCoral;
                    }
                    else
                    {
                        this.lstclsItemCheckInfo[intGroupNum - 1].StepListViewTable.Rows[i][10] = SetColor(i);
                    }
                }
            }
            
            //Release mutex
            mut.ReleaseMutex();
        }

        private int intCalThreadID(int intStepPos)
        {
            //Checking
            if((this.lstTotalStep.Count-1)<intStepPos)
            {
                return -1; //Error
            }

            //Background thread
            if ((this.lstTotalStep[intStepPos].intStepClass >= 100) && (this.lstTotalStep[intStepPos].intStepClass <= 299)) //background thread
            {
                return 1; //Background thread
            }

            //Main thread
            return 0; //Main thread default
        }

        public void RefreshViewTable()
        {
            if (blAllowUpdateViewTable == false) return; //not allow update

            //protect with mutex
            mut.WaitOne();

            //Error trap
            if (this.ViewTable == null) return;

            int i = 0;
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                //Set color for data view
                this.ViewTable.Rows[i][10] = SetColor(i);

               //Set result checking
                if (this.lstTotalStep[i].blStepChecked == true)
                {
                    this.ViewTable.Rows[i][3] = this.GetConvertDataResult(i);
                    //Set result checking color
                    if (this.lstTotalStep[i].blStepResult == true) //PASS
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Red;
                    }
                }
                else
                {
                    this.ViewTable.Rows[i][3] = "";
                }
            }

            //Release mutex
            mut.ReleaseMutex();
        }

        public void RefreshStepListViewTable(int intGroupID)
        {
            if (this.lstclsItemCheckInfo[intGroupID].blAllowUpdateStepListViewTable == false) return; //not allow update
            if (this.clsChildSetting.blUsingOriginSteplist == false) return;
            //protect with mutex
            mut2.WaitOne();

            if (this.lstclsItemCheckInfo[intGroupID].StepListViewTable == null) return;
            //
            int i = 0;
            //Set color for data view
            for(i=0;i< this.clsStepList.lstExcelList.Count; i++)
            {
                int intStepPos = this.FindStepListPLStepPos(intGroupID,i);
                if (intStepPos == -1)
                {
                    continue;
                }

                //Background Color
                this.lstclsItemCheckInfo[intGroupID].StepListViewTable.Rows[i][10] = SetColor(i);
                //Result
                if (this.lstTotalStep[intStepPos].blStepChecked == true)
                {
                    this.lstclsItemCheckInfo[intGroupID].StepListViewTable.Rows[i][3] = this.GetConvertDataResult(intStepPos);

                    //Result checking color
                    if (this.lstTotalStep[intStepPos].blStepResult == true) //PASS
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[i][11] = System.Windows.Media.Brushes.Red;
                    }
                }
                else
                {
                    this.lstclsItemCheckInfo[intGroupID].StepListViewTable.Rows[i][3] = "";
                }
            }

            //Release mutex
            mut2.ReleaseMutex();
        }

        /// <summary>
        /// Find representative step in program list of 1 step in step list which belong to 1 group
        /// </summary>
        /// <param name="intSTLPos"></param>
        /// <returns></returns>
        public int FindStepListPLStepPos(int intGroupID, int intSTLPos)
        {
            int intRet = -1;
            int i = 0;
            int intStepNumber = this.clsStepList.lstExcelList[intSTLPos].intStepNumber;
            for (i = 0; i < this.lstclsItemCheckInfo[intGroupID].lstTotalStep.Count; i++)
            {

                if (this.blGroupMode == true)// Group Mode
                {
                    if (this.lstclsItemCheckInfo[intGroupID].lstTotalStep[i].strOriginStepNumber == intStepNumber.ToString())
                    {
                        intRet = this.lstclsItemCheckInfo[intGroupID].lstTotalStep[i].intStepPos;
                        break;
                    }
                }
                else //Not Group Mode
                {
                    if (this.lstclsItemCheckInfo[intGroupID].lstTotalStep[i].intStepNumber == intStepNumber)
                    {
                        intRet = this.lstclsItemCheckInfo[intGroupID].lstTotalStep[i].intStepPos;
                        break;
                    }
                }
            }

            return intRet;
        }


        public void SetCheckMode(enumChildProcessCheckingMode eMode)
        {
            this.eChildCheckMode = eMode;
        }

        private System.Windows.Media.SolidColorBrush SetColor(int i)
        {
            if((i%2)==0)
            {
                return System.Windows.Media.Brushes.White;
            }
            else
            {
                return System.Windows.Media.Brushes.LightGreen;
            }
        }

        public void ClearBindingView()
        {
            if(this.blGroupMode==false) //Not Group Mode - update form class itself
            {
                //Table checking result
                this.clsChildModelBindingView.strItemResult = (this.intProcessID + 1).ToString();
                this.clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightBlue;

                //Table detail info
                this.clsChildModelBindingView.strResultPassFail = "";
                this.clsChildModelBindingView.strPassRate = this.clsItemResult.dblItemPassRate.ToString();
                this.clsChildModelBindingView.strItemCheckPass = this.clsItemResult.intItemNumberPass.ToString();
                this.clsChildModelBindingView.strItemCheckCount = this.clsItemResult.intItemNumberCheck.ToString();
                this.clsChildModelBindingView.strStatus = "";
                this.clsChildModelBindingView.strFailInfo = "";
                this.clsChildModelBindingView.strCheckPoint = "";

                this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                              "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                              "Status: " + this.clsChildModelBindingView.strStatus + "\r\n" +
                              "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                              "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;
            }
            else //Group Mode - update from sub-child process
            {
                for(int i =0;i<this.lstclsItemCheckInfo.Count;i++)
                {
                    //Table checking result
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemResult = (this.lstclsItemCheckInfo[i].intItemID + 1).ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightBlue;

                    //Table detail info
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail = "";
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strPassRate = this.lstclsItemCheckInfo[i].clsItemResult.dblItemPassRate.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckPass = this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberPass.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckCount = this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus = "";
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo = "";
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strCheckPoint = "";

                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemInfo = "Result: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail + "\r\n" +
                                  "Pass rate: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strPassRate + " % " + "(" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckPass + "/" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                  "Status: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus + "\r\n" +
                                  "Fail: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo + "\r\n" +
                                  "CheckPoint: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strCheckPoint;
                }
            }

            //Raise property change
            OnPropertyChanged("clsChildModelBindingView");
        }

        public void ClearViewTableInfo()
        {
            mut.WaitOne();
            
            int i = 0;
            //Clear on Process View Tabpage
            for (i = 0; i < this.lstChildCheckAll.Count;i++)
            {
                int intStepPos = this.lstChildCheckAll[i].intStepPos;
                //Clear Last result checking
                this.ViewTable.Rows[intStepPos][3] = "";
                this.ViewTable.Rows[intStepPos][11] = System.Windows.Media.Brushes.Black;
                //
                this.ViewTable.Rows[intStepPos][10] = this.SetColor(i);
            }

            //Clear on Step List View
            if (this.clsChildSetting.blUsingOriginSteplist == true)
            {
                for(i=0;i<this.clsStepList.lstExcelList.Count;i++)
                {
                    //Find group number
                    for(int j=0;j<this.lstclsItemCheckInfo.Count;j++)
                    {
                        if(this.lstclsItemCheckInfo[j].blAllowUpdateStepListViewTable==true)
                        {
                            this.lstclsItemCheckInfo[j].StepListViewTable.Rows[i][3] = "";
                            this.lstclsItemCheckInfo[j].StepListViewTable.Rows[i][11] = System.Windows.Media.Brushes.Black;
                        }
                    }
                }  
            }

            //
            mut.ReleaseMutex();
        }

        public void UpdateCheckingProcessBindingView(int intStepPos)
        {
            int i = 0;
            //only allow Main thread update on Main Info Page
            if(this.intCalThreadID(intStepPos)!=0) //Not main thread?
            {
                return;
            }

            //If setting in Group Mode
            if(this.blGroupMode==true)
            {
                bool blFound = false;
                int intProID = 0;
                string strGroupNumber = this.lstTotalStep[intStepPos].strGroupNumber;
                for(i=0;i<this.lstclsItemCheckInfo.Count;i++)
                {
                    if(strGroupNumber.Trim()==(i+1).ToString())//Matching
                    {
                        intProID = i;
                        blFound = true;
                        break;
                    }
                }

                //
                if(blFound==true) //Found sub-child group
                {
                    //if normal step => orange. If Polling step in independent mode => light blue
                    if (this.lstTotalStep[intStepPos].intStepClass != 1)
                    {
                        //Table checking result
                        //this.lstGroupChildProcess[intProID].clsChildModelBindingView.strItemResult = (this.lstGroupChildProcess[intProID].intProcessId + 1).ToString() + " - Checking";
                        if (this.eChildProcessStatus != enumChildProcessStatus.eFinish)
                        {
                            this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemResult = (this.lstclsItemCheckInfo[intProID].intItemID + 1).ToString();
                            this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.Orange;
                        }
                        else //Already finish!
                        {
                            this.UpdateFinishBindingView();
                        }
                       
                    }

                    //Table detail info
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strResultPassFail = "";
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strPassRate = this.lstclsItemCheckInfo[intProID].clsItemResult.dblItemPassRate.ToString();
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemCheckPass = this.lstclsItemCheckInfo[intProID].clsItemResult.intItemNumberPass.ToString();
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemCheckCount = this.lstclsItemCheckInfo[intProID].clsItemResult.intItemNumberCheck.ToString();
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strStatus = "Step [" + this.lstTotalStep[this.intStepPosRunning].intStepNumber.ToString() + "] " +
                                                                                                   this.lstTotalStep[this.intStepPosRunning].strStepName;
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strFailInfo = "";
                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strCheckPoint = "";

                    this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemInfo = "Result: " + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strResultPassFail + "\r\n" +
                                  "Pass rate: " + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strPassRate + " % " + "(" + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemCheckPass + "/" + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                  "Status: " + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strStatus + "\r\n" +
                                  "Fail: " + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strFailInfo + "\r\n" +
                                  "CheckPoint: " + this.lstclsItemCheckInfo[intProID].clsChildModelBindingView.strCheckPoint;

                }
                else //If not found, it mean it belong to step which is common for all group
                {
                    //if normal step => orange. If Polling step in independent mode => light blue
                    if (this.lstTotalStep[intStepPos].intStepClass == 1)
                    {
                        for (i = 0; i < this.lstclsItemCheckInfo.Count; i++)
                        {
                            this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus = "Step [" + this.lstTotalStep[this.intStepPosRunning].intStepNumber.ToString() + "] " +
                                                                                                   this.lstTotalStep[this.intStepPosRunning].strStepName;

                            this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemInfo = "Result: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail + "\r\n" +
                                  "Pass rate: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strPassRate + " % " + "(" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckPass + "/" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                  "Status: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus +  " (Polling for start...)" + "\r\n" +
                                  "Fail: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo + "\r\n" +
                                  "CheckPoint: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strCheckPoint;
                        }
                    }
                }
            }
            else //Not Group Mode
            {
                //Not found sub-child group, then raise property change by current child class
                //if normal step => orange. If Polling step in independent mode => light blue
                if (this.lstTotalStep[intStepPos].intStepClass == 1) //Start polling class
                {
                    //Table detail info
                    this.clsChildModelBindingView.strResultPassFail = "";
                    this.clsChildModelBindingView.strPassRate = this.clsItemResult.dblItemPassRate.ToString();
                    this.clsChildModelBindingView.strItemCheckPass = this.clsItemResult.intItemNumberPass.ToString();
                    this.clsChildModelBindingView.strItemCheckCount = this.clsItemResult.intItemNumberCheck.ToString();
                    this.clsChildModelBindingView.strStatus = "Step [" + this.lstTotalStep[this.intStepPosRunning].intStepNumber.ToString() + "] " +
                                                                                                   this.lstTotalStep[this.intStepPosRunning].strStepName;
                    this.clsChildModelBindingView.strFailInfo = "";
                    this.clsChildModelBindingView.strCheckPoint = "";

                    this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                                "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                "Status: " + this.clsChildModelBindingView.strStatus + " (Polling for start...)" + "\r\n" +
                                "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                                "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;
                }
                else if(this.lstTotalStep[intStepPos].intStepClass == 51) //Checking end service
                {
                    //Table detail info
                    this.clsChildModelBindingView.strResultPassFail = "";
                    this.clsChildModelBindingView.strPassRate = this.clsItemResult.dblItemPassRate.ToString();
                    this.clsChildModelBindingView.strItemCheckPass = this.clsItemResult.intItemNumberPass.ToString();
                    this.clsChildModelBindingView.strItemCheckCount = this.clsItemResult.intItemNumberCheck.ToString();
                    this.clsChildModelBindingView.strStatus = "Step [" + this.lstTotalStep[this.intStepPosRunning].intStepNumber.ToString() + "] " +
                                                                                                   this.lstTotalStep[this.intStepPosRunning].strStepName;
                    this.clsChildModelBindingView.strFailInfo = "";
                    this.clsChildModelBindingView.strCheckPoint = "";

                    this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                                "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                "Status: " + this.clsChildModelBindingView.strStatus + "\r\n" +
                                "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                                "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;
                }
                else
                {
                    //Table checking result
                    //this.clsChildModelBindingView.strItemResult = (this.intProcessId + 1).ToString() + " - Checking";

                    if (this.eChildProcessStatus != enumChildProcessStatus.eFinish)
                    {
                        this.clsChildModelBindingView.strItemResult = (this.intProcessID + 1).ToString();
                        this.clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.Orange;

                        //Table detail info
                        this.clsChildModelBindingView.strResultPassFail = "";
                        this.clsChildModelBindingView.strPassRate = this.clsItemResult.dblItemPassRate.ToString();
                        this.clsChildModelBindingView.strItemCheckPass = this.clsItemResult.intItemNumberPass.ToString();
                        this.clsChildModelBindingView.strItemCheckCount = this.clsItemResult.intItemNumberCheck.ToString();
                        this.clsChildModelBindingView.strStatus = "Step [" + this.lstTotalStep[this.intStepPosRunning].intStepNumber.ToString() + "] " +
                                                                                                       this.lstTotalStep[this.intStepPosRunning].strStepName;
                        this.clsChildModelBindingView.strFailInfo = "";
                        this.clsChildModelBindingView.strCheckPoint = "";

                        if (this.lstTotalStep[intStepPos].intStepClass == 1) //Polling for start
                        {
                            this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                                      "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                      "Status: " + this.clsChildModelBindingView.strStatus + " (Polling for start...)" + "\r\n" +
                                      "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                                      "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;
                        }
                        else
                        {
                            this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                                      "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                      "Status: " + this.clsChildModelBindingView.strStatus + "\r\n" +
                                      "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                                      "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;
                        }
                    }
                    else //Already finish!
                    {
                        //this.clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.Pink;
                        this.UpdateFinishBindingView();
                    }
                }
            }

            //Raise property change
            OnPropertyChanged("clsChildModelBindingView");
        }

        public void UpdateFinishBindingView()
        {
            int i = 0;

            if(this.blGroupMode==false) //Not Group Mode => update view for this class itself
            {
                if (this.clsItemResult.blItemCheckingResult == true) //Result PASS
                {
                    //Table checking result
                    this.clsChildModelBindingView.strItemResult = (this.intProcessID + 1).ToString() + " - PASS";
                    this.clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightGreen;

                    //Table detail info
                    this.clsChildModelBindingView.strResultPassFail = "PASS";

                    this.clsChildModelBindingView.strFailInfo = "--";
                    this.clsChildModelBindingView.strFailData = "--";
                    this.clsChildModelBindingView.strCheckPoint = "--";
                }
                else //Result Fail
                {
                    //Table checking result
                    this.clsChildModelBindingView.strItemResult = (this.intProcessID + 1).ToString() + " - FAIL";
                    this.clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightCoral;

                    //Table detail info
                    this.clsChildModelBindingView.strResultPassFail = "FAIL";
                    this.clsChildModelBindingView.strFailInfo = "Step[" + this.lstTotalStep[this.clsItemResult.intStepFailPos].intStepNumber.ToString() +
                                                                        "] " + this.lstTotalStep[this.clsItemResult.intStepFailPos].strStepName;

                    this.clsChildModelBindingView.strFailData = this.GetConvertDataResult(this.clsItemResult.intStepFailPos);

                    this.clsChildModelBindingView.strCheckPoint = this.lstTotalStep[this.clsItemResult.intStepFailPos].strMeasurePad;
                }

                this.clsChildModelBindingView.strPassRate = this.clsItemResult.dblItemPassRate.ToString();
                this.clsChildModelBindingView.strItemCheckPass = this.clsItemResult.intItemNumberPass.ToString();
                this.clsChildModelBindingView.strItemCheckCount = this.clsItemResult.intItemNumberCheck.ToString();
                this.clsChildModelBindingView.strStatus = "Checking Finish!";


                this.clsChildModelBindingView.strItemInfo = "Result: " + this.clsChildModelBindingView.strResultPassFail + "\r\n" +
                              "Pass rate: " + this.clsChildModelBindingView.strPassRate + " % " + "(" + this.clsChildModelBindingView.strItemCheckPass + "/" + this.clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                              "Status: " + this.clsChildModelBindingView.strStatus + "\r\n" +
                              "Fail: " + this.clsChildModelBindingView.strFailInfo + "\r\n" +
                              "Fail Data: " + this.clsChildModelBindingView.strFailData + "\r\n" +
                              "CheckPoint: " + this.clsChildModelBindingView.strCheckPoint;

                //Update again all color of each step in View Table
                if(this.blAllowUpdateViewTable == true)
                {
                    mut.WaitOne();
                    for (i = 0; i < this.lstTotalStep.Count; i++)
                    {
                        this.ViewTable.Rows[i][10] = SetColor(i);
                    }
                    mut.ReleaseMutex();
                }
               
            }
            else //Group Mode => Update view for all sub-child process ID
            {
                i = 0;
                for(i=0;i<this.lstclsItemCheckInfo.Count;i++)
                {
                    if (this.lstclsItemCheckInfo[i].clsItemResult.blItemCheckingResult == true) //Result PASS
                    {
                        //Table checking result
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemResult = (this.lstclsItemCheckInfo[i].intItemID + 1).ToString() + " - PASS";
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightGreen;

                        //Table detail info
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail = "PASS";

                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo = "--";
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailData = "--";
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strCheckPoint = "--";
                    }
                    else //Result Fail
                    {
                        //Table checking result
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemResult = (this.lstclsItemCheckInfo[i].intItemID + 1).ToString() + " - FAIL";
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.clrItemResultBackGround = System.Windows.Media.Brushes.LightCoral;

                        //Table detail info
                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail = "FAIL";

                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo = 
                            "Step[" + this.lstTotalStep[this.lstclsItemCheckInfo[i].clsItemResult.intStepFailPos].intStepNumber.ToString() +
                            "] " + this.lstTotalStep[this.lstclsItemCheckInfo[i].clsItemResult.intStepFailPos].strStepName;

                        this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailData = this.GetConvertDataResult(this.lstclsItemCheckInfo[i].clsItemResult.intStepFailPos) +
                            " [" + this.lstTotalStep[this.lstclsItemCheckInfo[i].clsItemResult.intStepFailPos].strUnitName + "]";
                        this.clsChildModelBindingView.strCheckPoint = this.lstTotalStep[this.lstclsItemCheckInfo[i].clsItemResult.intStepFailPos].strMeasurePad;
                    }

                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strPassRate = this.lstclsItemCheckInfo[i].clsItemResult.dblItemPassRate.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckPass = this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberPass.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckCount = this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck.ToString();
                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus = "Checking Finish!";

                    this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemInfo = "Result: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strResultPassFail + "\r\n" +
                                  "Pass rate: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strPassRate + " % " + "(" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckPass + "/" + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strItemCheckCount + ")" + "\r\n" +
                                  "Status: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strStatus + "\r\n" +
                                  "Fail: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailInfo + "\r\n" +
                                  "Fail Data: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strFailData + "\r\n" +
                                  "CheckPoint: " + this.lstclsItemCheckInfo[i].clsChildModelBindingView.strCheckPoint;
                }

                //Update again all color of each step in View Table
                if (this.blAllowUpdateViewTable == true)
                {
                    mut.WaitOne();
                    for (i = 0; i < this.lstTotalStep.Count; i++)
                    {
                        this.ViewTable.Rows[i][10] = SetColor(i);
                    }
                    mut.ReleaseMutex();
                }

            }

            //
            //Raise property change - only allow Group Class raise property change
            OnPropertyChanged("clsChildModelBindingView");
        }

        public void ResetAllColorTableView()
        {
            //Update again all color of each step in View Table
            if (this.blAllowUpdateViewTable == true)
            {
                mut.WaitOne();
                for (int i = 0; i < this.lstTotalStep.Count; i++)
                {
                    this.ViewTable.Rows[i][10] = SetColor(i);
                }
                mut.ReleaseMutex();

                //
                OnPropertyChanged("GetTableView");
            }
        }

        public string GetConvertDataResult(int intStepPos)
        {
            //Check if necessary convert result to hexa format and return
            string strRet = "";
            double dblTemp = 0;
            //
            if (this.lstTotalStep[intStepPos].blStepChecked == false) return "Not Checked";
            if (this.lstTotalStep[intStepPos].objStepCheckingData == null) return "null";
            
            if (this.lstTotalStep[intStepPos].objStepCheckingData == null) return "null";
            //Convert to string
            strRet = this.lstTotalStep[intStepPos].objStepCheckingData.ToString();
            //Check if need & can convert to hexa format
            if (this.lstTotalStep[intStepPos].strUnitName.ToUpper().Trim() == "H")
            {
                if (double.TryParse(strRet, out dblTemp)==false)
                {
                   return strRet;
                }

                //
                int intTemp = Convert.ToInt32(dblTemp);
                strRet = intTemp.ToString("X");
            }

            //
            return strRet;
        }

        //*************************************************************************************************************************
        public void ResetParameterEx()
        {
            int i = 0;
            int j = 0;
            //Reset all checking process
            //Jumping times
            for (i = 0; i < this.lstTotalStep.Count; i++) //Counting each step in steplist
            {
                for (j = 0; j < this.clsCommonFunc.lstlstCommonCommandAnalyzer[i].Count; j++) //Counting each command in step
                {
                    object objTest = this.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j];
                    this.ResetCommandGuiderParameter(ref objTest);
                }
            }
        }

        public void ResetCommandGuiderParameter(ref object objCommandGuider)//(ref nspSpecialControl.clsCommonCommandGuider clsCommandGuider)
        {
            if (objCommandGuider == null) return;
            if (!(objCommandGuider is nspCFPExpression.clsCommonCommandGuider)) return;
            var clsCommandGuider = (nspCFPExpression.clsCommonCommandGuider)objCommandGuider;
            //
            int i = 0;
            //Remove parameter of command guider input
            if(clsCommandGuider.lstobjCmdPara!=null)
            {
                if(clsCommandGuider.lstobjCmdPara.Count != 0)
                {
                    clsCommandGuider.lstobjCmdParaEx = new List<object>();
                    for (i = 0; i < clsCommandGuider.lstobjCmdPara.Count; i++)
                    {
                        clsCommandGuider.lstobjCmdParaEx.Add(clsCommandGuider.lstobjCmdPara[i]); //Reset to origin parameter
                    }
                }
            }

            //Check child expression of command guider input
            if (clsCommandGuider.ParsedExpression == null) return;
            //
            if(clsCommandGuider.ParsedExpression is NCalc2.Expressions.FunctionExpression)
            {
                NCalc2.Expressions.FunctionExpression FunctionEx = (NCalc2.Expressions.FunctionExpression)clsCommandGuider.ParsedExpression;
                if (FunctionEx.Expressions!=null)
                {
                    foreach (var item in FunctionEx.Expressions)
                    {
                        object objSubCommandGuider = (object)item.objCommandGuider;
                        this.ResetCommandGuiderParameter(ref objSubCommandGuider);
                    }
                }
            }
            else if(clsCommandGuider.ParsedExpression is NCalc2.Expressions.BinaryExpression)
            {
                NCalc2.Expressions.BinaryExpression BinaryEx = (NCalc2.Expressions.BinaryExpression)clsCommandGuider.ParsedExpression;
                //Reset left
                if(BinaryEx.LeftExpression!=null)
                {
                    object objLeft = BinaryEx.LeftExpression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objLeft);
                }
                //Reset Right
                if(BinaryEx.RightExpression!=null)
                {
                    object objRight = BinaryEx.RightExpression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objRight);
                }
            }
            else if(clsCommandGuider.ParsedExpression is NCalc2.Expressions.IdentifierExpression) //Break point of recursion
            {
                //NCalc2.Expressions.IdentifierExpression IdentifierEx = (NCalc2.Expressions.IdentifierExpression)clsCommandGuider.ParsedExpression;
                //IdentifierEx.
            }
            else if(clsCommandGuider.ParsedExpression is NCalc2.Expressions.TernaryExpression)
            {
                NCalc2.Expressions.TernaryExpression TernaryEx = (NCalc2.Expressions.TernaryExpression)clsCommandGuider.ParsedExpression;
                //Reset left
                if(TernaryEx.LeftExpression!=null)
                {
                    object objLeft = TernaryEx.LeftExpression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objLeft);
                }
                //Reset middle
                if(TernaryEx.MiddleExpression !=null)
                {
                    object objMiddle = TernaryEx.MiddleExpression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objMiddle);
                }
                //Reset Right
                if(TernaryEx.RightExpression!=null)
                {
                    object objRight = TernaryEx.RightExpression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objRight);
                }
            }
            else if(clsCommandGuider.ParsedExpression is NCalc2.Expressions.UnaryExpression)
            {
                NCalc2.Expressions.UnaryExpression UnaryEx = (NCalc2.Expressions.UnaryExpression)clsCommandGuider.ParsedExpression;
                if(UnaryEx.Expression!=null)
                {
                    object objEx = UnaryEx.Expression.objCommandGuider;
                    this.ResetCommandGuiderParameter(ref objEx);
                }
            }
            else if (clsCommandGuider.ParsedExpression is NCalc2.Expressions.ValueExpression) //Break point of recursion
            {
                //NCalc2.Expressions.ValueExpression ValueEx = (NCalc2.Expressions.ValueExpression)clsCommandGuider.ParsedExpression;
                //ValueEx.
            }
            else //Break point of recursion
            {

            }
        }

        //*************************************************************************************************************************
        public void StartTaskChecking()
        {
            this.taskCheckingProcessCancelTokenSource = new CancellationTokenSource();
            this.taskCheckingProcess = Task.Factory
                            .StartNew(t => this.ThreadChecking(),
                                TaskCreationOptions.LongRunning,
                                this.taskCheckingProcessCancelTokenSource.Token
                            );
            Debug.WriteLine("Start Task " + this.intProcessID.ToString());
        }

        public void CancelTaskChecking()
        {
            this.taskCheckingProcessCancelTokenSource.Cancel();
            Debug.WriteLine("Cancel Task " + this.intProcessID.ToString());
        }

        //*************************************************************************************************************************
        /// <summary>
        /// For checking process, run every time with 3 class: 2-3-4 (after start - checking - finish)
        /// </summary>
        public void ThreadChecking()
        {
            //If Skip Mode is selected, then do nothing!
            if (this.blSkipModeRequest == true) return;

            //1. Before Process, Clear all 
            int i = 0;
            int j = 0;
            int intFirstStepFinishPos = 0;
            bool blCalResult = false;

            this.eChildProcessStatus = enumChildProcessStatus.eChecking;

            //Reset all checking process
            //Jumping times
            this.ResetParameterEx();
            

            //Mark first step position of Finish Process
            if (this.lstChildFinish.Count != 0)
            {
                intFirstStepFinishPos = this.lstChildFinish[0].intStepPos - 1;
            }
            else
            {
                if (this.lstChildCheckingEndService.Count != 0)
                {
                    intFirstStepFinishPos = this.lstChildCheckingEndService[0].intStepPos - 1;
                }
                else
                {
                    intFirstStepFinishPos = this.lstChildCheckAll[this.lstChildCheckAll.Count-1].intStepPos; //If there is no Finish process, go to an end!
                }
            }

            this.clsItemResult.blItemCheckingResult = true;
            this.blResultAlreadyFail = false;

            //Before checking started, clear all data before
            for (i = 0; i < this.lstChildCheckAll.Count; i++) //For Thread checking
            {
                int intStepPos = this.lstChildCheckAll[i].intStepPos;

                this.lstTotalStep[intStepPos].blStepResult = false;
                this.lstTotalStep[intStepPos].objStepCheckingData = "";
                this.lstTotalStep[intStepPos].dblStepTactTime = 0;
                this.lstTotalStep[intStepPos].strStepErrMsg = "";
                this.lstTotalStep[intStepPos].blStepChecked = false;
                this.lstTotalStep[intStepPos].intStartTickMark = 0;
                this.lstTotalStep[intStepPos].intExecuteTimes = 0;
            }

            //If setting is Group Mode, then we have to reset for all sub-child process also
            if(this.blGroupMode==true)
            {
                for(i=0;i<this.lstclsItemCheckInfo.Count;i++)
                {
                    this.lstclsItemCheckInfo[i].blResultAlreadyFail = false;

                    for (j = 0; j < this.lstclsItemCheckInfo[i].lstTotalStep.Count;j++)
                    {
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].blStepResult = false;
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].objStepCheckingData = "";
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].dblStepTactTime = 0;
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].strStepErrMsg = "";
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].blStepChecked = false;
                        this.lstclsItemCheckInfo[i].lstTotalStep[j].intStartTickMark = 0;
                    }
                }
            }


            this.intStepPosRunning = 0;

            //Clear binding View
            this.ClearBindingView();

            //Clear display on View Table info
            this.ClearViewTableInfo();

            //Start counting tact time
            this.intStartTick = MyLibrary.ApiDeclaration.GetTickCount();

            //Start background thread
            this.blRequestBackgroundStop = false;
            if(this.lstChildBackgroundPolling.Count>0)
            {
                //this.thrdBackGround = new System.Threading.Thread(this.ChildProcessBackgroundEx);
                //this.thrdBackGround.Name = "BGND-ID" + this.intProcessID.ToString();
                //this.thrdBackGround.Start();
                this.taskBackGround = Task.Factory
                    .StartNew(this.ChildProcessBackgroundEx, TaskCreationOptions.LongRunning);
            }


            //2. Looking and execute each step in step list
            //for (i = 0; i < this.lstChildThreadCheck.Count; i++)
            for (i = 0; i < this.lstChildCheckAll.Count; i++)
            {
                // Check if cancellation request token
                if (this.taskCheckingProcessCancelTokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine("Cancellation confirmed! ChildProcess" + this.intProcessID.ToString());
                    break;
                }


                //Find the position of step in step list
                int intStepPos = this.lstChildCheckAll[i].intStepPos;
                int intSubChildID = 0;
                bool blSearchSubChildID = false;

                //In group mode, we need to check current step is belong to what sub-child process
                //And if that sub-child process already fail, then no need check that sub-child process further
                #region _GroupModehandle
                if (this.blGroupMode==true)
                {
                    intSubChildID = 0;
                    blSearchSubChildID = false;
                    for(j=0;j<this.lstclsItemCheckInfo.Count;j++)
                    {
                        if(this.lstTotalStep[intStepPos].strGroupNumber.Trim() == (j+1).ToString())
                        {
                            blSearchSubChildID = true;
                            intSubChildID = j;
                            break;
                        }
                    }
                    //Check if Sub-child already fail or not. If fail, then stop checking for that process
                    if(blSearchSubChildID==true) //found sub-child process
                    {
                        //Check skip
                        if(this.lstclsItemCheckInfo[intSubChildID].blSkipModeRequest==true)
                        {
                            continue;
                        }

                        //
                        if(this.lstclsItemCheckInfo[intSubChildID].blResultAlreadyFail==true)
                        {
                            continue; //No need check this step, move to next
                        }
                    }
                    else //Not found
                    {
                        //We need to check if step is belong to sub-child process ID which is not config for current group. If not, then skip step, move to next
                        //Condition: group number is numeric & do not belong to group sub-child ID
                        int intTemp = 0;
                        if(int.TryParse(this.lstTotalStep[intStepPos].strGroupNumber.Trim(),out intTemp)==true)
                        {
                            continue; //The step belong to sub-child ID which not belong to current group, move to next
                        }
                    }
                }
                #endregion

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                //Execute step checking
                bool blStepResult = ChildFuncEx(intStepPos);

                #region _HandleCheckingMode

                //*************************FOR EACH TEST MODE IN ITEM INFO***********************************
                //For Normal Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eNormal) //Normal Mode
                {
                    //Checking until meet 1 step fail, then stop or until all steps pass
                    if (this.lstTotalStep[intStepPos].blStepResult == false)
                    {
                        //If step fail belong to Checking Class (Test class = 3), then we jump into first step of Finish Process (Test class = 4)
                        //But what happen if there is no Finish Process? Where will we jump to???????????????????????????????????????????????????????????????????????????????????????
                        //We will exit checking process.
                        //If step fail belong to After Start(test class=2) or Finish Process (test class=4): we do nothing.

                        if (this.lstTotalStep[intStepPos].intStepClass == 3)
                        {
                            if (this.blGroupMode == false) //Not group mode => jump to finish step class
                            {
                                //i = intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildCheckAll) - 1;
                                i = intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildCheckAll);
                            }
                            else //Group Mode - If 1 step fail, depend on what kind of step => handle it
                            {
                                //1. If step belong to 1 sub-child process, then we stop checking that process & move to checking another sub-child process
                                if (blSearchSubChildID == true)
                                {
                                    this.lstclsItemCheckInfo[intSubChildID].blResultAlreadyFail = true; //Marking to stop checking sub-child process
                                }
                                else //Not belong to any sub-child process
                                {
                                    //i = intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildCheckAll) - 1;
                                    i = intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildCheckAll);
                                }
                            }
                        }
                    }
                }

                //For single checking Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eSingle) //Single Mode
                {
                    this.blAllowContinueRunning = false;
                    while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                    {
                        Application.DoEvents();
                        //
                        if (this.blRequestStopRunning == true)
                        {
                            i = this.lstChildCheckAll.Count; //Ignore all next steps and come to ending point
                            break;
                        }
                    }
                }

                //For Step checking Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eStep) //Step Mode
                {
                    if ((this.lstTotalStep[intStepPos].blStepResult == false) || (this.intStepModePosSelected == this.lstTotalStep[intStepPos].intStepPos))
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            Application.DoEvents();
                            //
                            if (this.blRequestStopRunning == true)
                            {
                                i = this.lstChildCheckAll.Count; //Ignore all next steps and come to ending point
                                break;
                            }
                        }
                    }
                }
                //For Fail checking mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eFail) //Fail Mode
                {
                    if (this.lstTotalStep[intStepPos].blStepResult == false)
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            Application.DoEvents();
                            //
                            if (this.blRequestStopRunning == true)
                            {
                                i = this.lstChildCheckAll.Count; //Ignore all next steps and come to ending point
                                break;
                            }
                        }
                    }
                }
                //For All checking mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eAll) //All Mode
                {
                    //Automatically run all steps without caring result of each step OK or NG
                }
                #endregion

                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);

                //in the end of lstChildThreadCheck, do calculate item checking result
                if ((i == (this.lstChildThreadCheck.Count - 1)) && (blCalResult==false))
                {
                    blCalResult = true;
                    ChildProcessCalResult();
                }

            } //End for i

            //If not yet cal result, then do it here (In group mode & program list do not have step class 4!)
            if (blCalResult == false)
            {
                ChildProcessCalResult();
            }

            //Request stop background thread
            this.blRequestBackgroundStop = true;
            //waiting for background thread not alive
            //if(this.thrdBackGround != null)
            //{
            //    while (this.thrdBackGround.IsAlive == true) ;
            //}
            if (this.lstChildBackgroundPolling.Count > 0) // BackGround Task exist
            {
                while (this.taskBackGround.IsCompleted) ;
            }
            
            //Saving data if Running Mode is Single Process or Independent Mode
            if((this.eChildRunningMode == enumSystemRunningMode.IndependentMode)||(this.eChildRunningMode == enumSystemRunningMode.SingleProcessMode))
            {
                this.clsChildModelBindingView.blRequestSavingData = true;
                this.clsChildModelBindingView.blRequestUpdatePassRate = true;
            }

            //Turn to Finish status
            this.eChildProcessStatus = enumChildProcessStatus.eFinish;

            //
            UpdateFinishBindingView();
            this.UpdateAllStepResultViewTable();

            //For independent mode
            if(this.eChildRunningMode == enumSystemRunningMode.IndependentMode)
            {
                this.blRequestCheckingEndService = true;
            }

        }

        public void ChildProcessBackgroundEx()
        {
            int intStepPos = 0;

            List<classStepDataInfor> lstInput = this.lstChildBackgroundPolling;

            //this.eChildProcessStatus = enumChildProcessStatus.eIni;

            int i = 0;
            for (i = 0; i < lstInput.Count; i++)
            {
                //Find the position of step in Master step list
                intStepPos = lstInput[i].intStepPos;

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                bool blStepResult = ChildFuncEx(intStepPos);

                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);

                //To keep background thread alive, if loop reaching final element, we reset it to beginning
                if(i==(lstInput.Count-1))
                {
                    i = -1;
                }

                //Check if request stop background or not
                if(this.blRequestBackgroundStop==true)
                {
                    break;
                }
            }

            //
            //ClearBindingView();

            //return "0"; //Return OK string code if everything is OK
        }

        /// <summary>
        /// This function, summary the checking result of child process
        /// </summary>
        public void ChildProcessCalResult()
        {
            int i = 0;
            //*****************Calculate Result for all item in Child Process*****************************************************
            //Calculate Result Information for all Item in child process
            for (i = 0; i < this.lstclsItemCheckInfo.Count; i++)
            {
                //1. Calculate Item total checking result 
                this.ItemResultCalculate(i);

                //Calculate Pass rate follow Item Counting
                if (this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck >= 1000000000) //Well, we need to auto-reset to 0
                {
                    this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck = 0;
                    this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberPass = 0;
                    this.lstclsItemCheckInfo[i].clsItemResult.dblItemPassRate = 0;
                }
                this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck++; //Always increased by 1 after each checking times
                if (this.lstclsItemCheckInfo[i].clsItemResult.blItemCheckingResult == true) this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberPass++; //Increased if PASS result
                this.lstclsItemCheckInfo[i].clsItemResult.dblItemPassRate = Math.Round((double)(100 * this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberPass / this.lstclsItemCheckInfo[i].clsItemResult.intItemNumberCheck), 2);

                //Calculate tact time?
                this.lstclsItemCheckInfo[i].clsItemResult.dblItemTactTime = 0; //Counting by second
            }

            //*****************Calculate Result for Child Process*****************************************************
            //Calculate tact time
            this.clsItemResult.dblItemTactTime = Math.Round((double)(MyLibrary.ApiDeclaration.GetTickCount() - this.intStartTick) / 1000, 2); //Counting by second

            //Calculate Item total checking result (only count with step with Test class = 3)
            this.ChildProcessTotalResultCalculate();

            //Calculate Pass rate
            if (this.clsItemResult.intItemNumberCheck >= 1000000000) //Well, we need to auto-reset to 0
            {
                this.clsItemResult.intItemNumberCheck = 0;
                this.clsItemResult.intItemNumberPass = 0;
                this.clsItemResult.dblItemPassRate = 0;
            }
            this.clsItemResult.intItemNumberCheck++; //Always increased by 1 after each checking times
            if (this.clsItemResult.blItemCheckingResult == true) this.clsItemResult.intItemNumberPass++; //Increased if PASS result
            this.clsItemResult.dblItemPassRate = Math.Round((double)(100 * this.clsItemResult.intItemNumberPass / this.clsItemResult.intItemNumberCheck), 2);

            //Record date time checking
            this.clsItemResult.dateTimeChecking = DateTime.Now;
        }

        /// <summary>
        /// This function, define the rule to judge child process checking result is Pass or Fail
        /// </summary>
        public void ChildProcessTotalResultCalculate()
        {
            this.clsItemResult.blItemCheckingResult = true;
            int i = 0;
            for(i=0;i<this.lstclsItemCheckInfo.Count;i++)
            {
                if(this.lstclsItemCheckInfo[i].clsItemResult.blItemCheckingResult==false)
                {
                    this.clsItemResult.blItemCheckingResult = false;
                    break; //If 1 item fail, then all child process result is fail & no need confirm anymore
                }
            }
        }

        /// <summary>
        /// This function, define the rule to judge item's checking result is Pass or Fail
        /// With input is child item ID of child process
        /// </summary>
        /// <param name="intChildItemID"></param>
        /// <returns></returns>
        public bool ItemResultCalculate(int intChildItemID)
        {
            // If CancellationToken request => all item judgement is fail!
            if (this.taskCheckingProcessCancelTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("Fail because CancellationRequested.");
                this.lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult = false;
                return false;
            }

            //
            int i = 0;
            bool blRet = false;
            //Check out of range
            if ((intChildItemID < 0) || (intChildItemID > (this.lstclsItemCheckInfo.Count - 1))) return false;
            //2. Calculate Item total checking result 
            this.lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult = true;
            for (i = 0; i < this.lstclsItemCheckInfo[intChildItemID].lstTotalStep.Count; i++)
            {
                if (this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].blSTLPassCondition == true) //Step belong to step list must be checked & must be pass
                {
                    //1. Step with Pass condition setting need to be checked
                    //2. Step with Pass condition setting must be OK
                    if ((this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].blStepChecked == false) ||
                        (this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].blStepResult == false))
                    {
                        this.lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult = false;
                        break;
                    }
                }
                else //Step not belong to step list => Only care what step which is checked
                {
                    if (this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].blStepChecked == true)
                    {
                        //Step class = 3 will affect to result pass or fail
                        if (this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].intStepClass == 3)
                        {
                            if (this.lstclsItemCheckInfo[intChildItemID].lstTotalStep[i].blStepResult == false)
                            {
                                this.lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult = false;
                                break;
                            }
                        }
                    }
                }
            }
            //
            blRet = this.lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult;
            return blRet;
        }

        /// <summary>
        /// this function, do special control
        /// </summary>
        /// <param name="intAffectArea"></param>
        /// <param name="intStepPos"></param>
        /// <param name="intToken"></param>
        public object ChildProcessSpecialControlCommandEx(int intAffectArea, int intStepPos, ref int intToken)
        {
            int i = 0;
            object objTemp = null;
            for (i = 0; i < this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
            {
                object objTest = this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i];
                //
                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intTargetProcessID == 3) continue; //Not execute with target single thread
                //Check affect area
                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intAffectArea != intAffectArea) continue;

                objTemp = this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].evaluate();
                //Check return data
                if(objTemp is clsCommonCommandGuider)
                {
                    clsCommonCommandGuider clsCommandTemp = (clsCommonCommandGuider)objTemp;
                    //Update token if request
                    if(clsCommandTemp.blRequestUpdateToken==true)
                    {
                        intToken = clsCommandTemp.intToken;
                    }
                }
            }
            //
            return objTemp;
        }

        /// <summary>
        /// This function, analyze functional-parameter in steplist, and get return data for that parameter
        /// </summary>
        /// <param name="intStepPos"></param>
        public void ChildProcessParaSpecialCommandExecute(int intStepPos)
        {
            int i = 0;
            object objTemp = "";
            string strFinalResult = "";

            //Not Apply with the first step
            //if (intStepPos == 0) return;

            //For Analyze "Transmission" area with possible Special command
            for (i = 0; i < this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count; i++)
            {
                objTemp = this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos][i].evaluate();

                //For convention of Factory command, the end of Factory command end with  "," character => The last item of list always "empty" string => Reject this one
                if (i == (this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count - 1))
                {
                    if (objTemp!=null)
                    {
                        if (objTemp.ToString() != "") //Only allow the last one if it different from empty string ""
                        {
                            if (objTemp.ToString() != ",")
                            {
                                strFinalResult = strFinalResult + objTemp;
                            }
                        }
                    }
                }
                else
                {
                    strFinalResult = strFinalResult + objTemp + ",";
                }
            }

            this.lstTotalStep[intStepPos].strTransmisstionEx = strFinalResult;

            //If Mini-compiler analyze parameter input
            if (this.lstTotalStep[intStepPos].lstobjParameter != null)
            {
                for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameter.Count; i++)
                {
                    if (this.clsProgramList.lstlstobjStepPara[intStepPos][i].ToString().Trim() != "")
                    {
                        this.lstTotalStep[intStepPos].lstobjParameterEx[i] = this.clsCommonFunc.lstlstCommonParaCommandAnalyzer[intStepPos][i].evaluate();
                    }
                }
            }
        }

        /// <summary>
        /// This method prepare list of input parameter passing for each function ID in each plug-in
        /// </summary>
        /// <param name="intStepPos"></param>
        /// <returns></returns>
        public List<List<object>> CalListObjectInput(int intStepPos)
        {
            //List of string input has following format:
            //"Application startup path - Process ID - Test No - Test Name - Test Class - Lo Limit - Hi Limit - Unit - Jig ID - Hardware ID - Func ID -
            // Transmission - Receive - Para1 - ... - Para20 - Jump command - Signal Name - Port Measure Point - Check Pads - Control spec/Comment - Note"

            List<List<object>> lstlstRet = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            string strStartUpPath = Application.StartupPath;

            //Calculate for lstlstRet[0] : Start-up path & Process ID & Steplist information
            lstTemp.Add(strStartUpPath); //Start up path
            lstTemp.Add(this.intProcessID.ToString()); //Process ID

            lstTemp.Add(this.lstTotalStep[intStepPos].intStepNumber.ToString()); //Test Number
            lstTemp.Add(this.lstTotalStep[intStepPos].strStepName.ToString()); //Test Name
            lstTemp.Add(this.lstTotalStep[intStepPos].intStepClass.ToString()); //Test Class
            lstTemp.Add(this.lstTotalStep[intStepPos].objLoLimit.ToString()); //Low Limit
            lstTemp.Add(this.lstTotalStep[intStepPos].objHiLimit.ToString()); //Hi Limit
            lstTemp.Add(this.lstTotalStep[intStepPos].strUnitName.ToString()); //Unit name
            lstTemp.Add(this.lstTotalStep[intStepPos].intJigId.ToString()); //Jig ID
            lstTemp.Add(this.lstTotalStep[intStepPos].intHardwareId.ToString()); //Hardware ID
            lstTemp.Add(this.lstTotalStep[intStepPos].intFunctionId.ToString()); //Function ID

            //lstTemp.Add(this.lstChildTotal[intStepPos].strTransmisstion.ToString()); //Factory command Transmission
            lstTemp.Add(this.lstTotalStep[intStepPos].strTransmisstionEx.ToString()); //Change to Executed Transmission, not original one in Program List

            lstTemp.Add(this.lstTotalStep[intStepPos].strReceive.ToString()); //Factory command return data

            int i;
            for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameterEx.Count; i++)
            {
                lstTemp.Add(this.lstTotalStep[intStepPos].lstobjParameterEx[i]);
            }

            //this.lstChildTotal[intStepPos].intTestPos.ToString();
            lstTemp.Add(this.lstTotalStep[intStepPos].strSpecialControl.ToString()); //Jumping control
            lstTemp.Add(this.lstTotalStep[intStepPos].strSignalName.ToString()); //signal name
            lstTemp.Add(this.lstTotalStep[intStepPos].strMeasurePoint.ToString()); //port measure point
            lstTemp.Add(this.lstTotalStep[intStepPos].strMeasurePad.ToString()); //check pads
            lstTemp.Add(this.lstTotalStep[intStepPos].strComment.ToString()); //Comment
            lstTemp.Add(this.lstTotalStep[intStepPos].strNotes.ToString()); //Notes

            //Add lstlstRet[0] to return value
            lstlstRet.Add(lstTemp);


            //Calculate for lstlstRet[1] : CheckingMode information
            lstTemp = new List<object>();
            string strTemp = "";
            lstTemp.Add("CheckingMode");
            lstTemp.Add(this.strSystemCheckingMode);
            lstTemp.Add(strTemp);

            lstlstRet.Add(lstTemp);


            //Calculate for lstlstRet[2] : setting information
            lstTemp = new List<object>();
            lstTemp.Add("Setting");

            lstTemp.Add(this.clsChildSetting.intNumChecker.ToString()); //Add Number of checking items
            lstTemp.Add(this.clsChildSetting.intNumRow.ToString()); //Add Number of Row setting
            lstTemp.Add(this.clsChildSetting.intNumCol.ToString()); //Add Number of Col setting
            lstTemp.Add(this.clsChildSetting.intAllignMode.ToString()); //Add Number of Allign Mode setting
            lstTemp.Add(this.clsChildSetting.intRoundShapeMode.ToString()); //Add Number of RoundShape Mode setting
            lstTemp.Add(this.clsChildSetting.intOrgPosition.ToString()); //Add Number of Origin Position setting

            lstlstRet.Add(lstTemp);

            //Adding Master Process Model object
            lstTemp = new List<object>();
            lstTemp.Add("clsMasterProcessModel");
            lstTemp.Add(this.objMasterProcessModel);
            lstlstRet.Add(lstTemp);

            //Adding Child Control Process Model object
            lstTemp = new List<object>();
            lstTemp.Add("clsChildControlModel");
            lstTemp.Add(this.objChildControlModel);
            lstlstRet.Add(lstTemp);

            //Adding Child Process Model object itself
            lstTemp = new List<object>();
            lstTemp.Add("clsChildProcessModel");
            lstTemp.Add(this);
            lstlstRet.Add(lstTemp);

            return lstlstRet;
        }

        public int intSearchStepPosInClass(int intStepPos, List<classStepDataInfor> lstStepClass)
        {
            if (lstStepClass == null) return -1; //Illegal
            int i = 0;
            for (i = 0; i < lstStepClass.Count; i++)
            {
                if (lstStepClass[i].intStepPos == intStepPos)
                {
                    return i;
                }
            }

            return -1; //Not found!
        }

        /// <summary>
        /// Execute 1 function only
        /// </summary>
        /// <param name="intStepPos"></param>
        /// <returns></returns>
        public bool ChildFuncEx(int intStepPos, bool blUpdateView = true)
        {
            object objTempResult = "";
            List<List<object>> lstlstobjInput = new List<List<object>>();
            var lstlstobjOutput = new List<List<object>>();

            //Marking
            this.intStepPosRunning = intStepPos;

            //Update in Main Info tabpage
            if (blUpdateView)
            {
                UpdateStepInfo(intStepPos);
            }

            ////Marking time timing to run each step
            //if (this.clsItemResult.lstblStepChecked[intStepPos] == false) //Not yet checked
            //{
            //    //Do marking timing
            //    this.clsItemResult.lstintStartTickMark[intStepPos] = MyLibrary.clsApiFunc.GetTickCount();
            //}

            //Marking time timing to run each step
            if (this.lstTotalStep[intStepPos].blStepChecked==false)
            {
                //Do marking timing
                this.lstTotalStep[intStepPos].intStartTickMark = MyLibrary.clsApiFunc.GetTickCount();
            }


            //MINI-COMPILER : convert all command-parameter to real parameter
            this.ChildProcessParaSpecialCommandExecute(intStepPos);

            //Calculate list of string input
            lstlstobjInput = CalListObjectInput(intStepPos);

            //Execute Function
            if ((this.blActiveTestingSequence == true) && (this.clsSeqenceTestData.lstblForceChange[intStepPos] == true)) //Request change in Testing sequence mode
            {
                objTempResult = this.clsSeqenceTestData.lstobjNewValue[intStepPos];
            }
            else //Normal running
            {
                objTempResult = this.clsChildExtension.lstPluginCollection[this.clsChildExtension.lstFuncCatalogAllStep[intStepPos].intPartID].Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);
            }

            //Prevent null happen
            if (objTempResult == null)
            {
                objTempResult = "null";
            }

            //Record original return data
            this.lstTotalStep[intStepPos].objStepCheckingData = objTempResult;

            this.lstTotalStep[intStepPos].blStepChecked = true; //marking already checked
            this.lstTotalStep[intStepPos].intExecuteTimes++;
            if (this.lstTotalStep[intStepPos].intExecuteTimes > 1000000000) this.lstTotalStep[intStepPos].intExecuteTimes = 0;


            //Assign output data
            this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn = lstlstobjOutput;

            //Judgement result is OK or NG
            bool blStepResult = this.clsCommonMethod.JudgeStepResult(objTempResult, this.lstTotalStep[intStepPos].objLoLimit, this.lstTotalStep[intStepPos].objHiLimit, this.lstTotalStep[intStepPos].strUnitName);
            
            //save result
            this.lstTotalStep[intStepPos].blStepResult = blStepResult; 

            if ((blStepResult == false) && (this.lstTotalStep[intStepPos].intStepClass == 3)) //Marking step fail
            {
                //Marking for child process step fail
                this.clsItemResult.blItemCheckingResult = false;
                this.clsItemResult.intStepFailPos = intStepPos;
                
                //Marking step fail for items belong to child process
                int intGroupNumber = this.FindGroupNumber(intStepPos);
                if (intGroupNumber != -1)
                {
                    this.lstclsItemCheckInfo[intGroupNumber - 1].clsItemResult.blItemCheckingResult = false;
                    this.lstclsItemCheckInfo[intGroupNumber - 1].clsItemResult.intStepFailPos = intStepPos;
                }
            }

            //Calculate tact time for each step
            this.lstTotalStep[intStepPos].dblStepTactTime = MyLibrary.clsApiFunc.GetTickCount() - this.lstTotalStep[intStepPos].intStartTickMark;

            //Update View Table - result of each step
            UpdateStepInfo2(intStepPos);

            //Return result
            return blStepResult;
        }

        public int FindItemID(int intStepPos)
        {
            int intRet = -1;
            //
            int intGroupNumber = 0;
            if(int.TryParse(this.lstTotalStep[intStepPos].strGroupNumber.Trim(), out intGroupNumber)== false)
            {
                return intRet;
            }
            //
            for(int i =0;i<this.lstclsItemCheckInfo.Count;i++)
            {
                if(this.lstclsItemCheckInfo[i].intGroupNumber==intGroupNumber)
                {
                    intRet = this.lstclsItemCheckInfo[i].intItemID;
                    break;
                }
            }

            return intRet;
        }

        public int FindGroupNumber(int intStepPos)
        {
            int intRet = -1;

            if (this.lstTotalStep[intStepPos].strGroupNumber == null) return -3;

            if (int.TryParse(this.lstTotalStep[intStepPos].strGroupNumber.Trim(), out intRet) == false)
            {
                return -1;
            }
            //Check if Group Number is in valid range or not
            if ((intRet < 1) || (intRet > this.lstclsItemCheckInfo.Count)) return -2;

            return intRet;
        }

        public async void UpdateStepInfo(int intStepPos)
        {
            await TestInfo(intStepPos);
        }

        public Task TestInfo(int intStepPos)
        {
            return Task.Run(() =>
                {
                    //Only update on main info view if not yet finish!
                    if(this.eChildProcessStatus != enumChildProcessStatus.eFinish)
                    {
                        //Update Info on main Info tabpage
                        UpdateCheckingProcessBindingView(intStepPos);
                    }

                    //Allow update if step is in independent class
                    if ((this.lstTotalStep[intStepPos].intStepClass == 1) || (this.lstTotalStep[intStepPos].intStepClass == 51))
                    {
                        //Update View Table - result of each step
                        UpdateCheckingProcessBindingView(intStepPos);
                    }

                });
        }

        public async void UpdateStepInfo2(int intStepPos)
        {
            await TestInfo2(intStepPos);
        }

        public Task TestInfo2(int intStepPos)
        {
            return Task.Run(() =>
            {
                //Only update on main info view if not yet finish!
                if (this.eChildProcessStatus != enumChildProcessStatus.eFinish)
                {
                    //Update View Table - result of each step
                    UpdateStepResultViewTable(intStepPos);
                }
                //Allow update if step is in independent class
                if((this.lstTotalStep[intStepPos].intStepClass == 1)||(this.lstTotalStep[intStepPos].intStepClass == 51))
                {
                    //Update View Table - result of each step
                    UpdateStepResultViewTable(intStepPos);
                }

            });
        }

        public string ChildProcessIniStepEx(List<classStepDataInfor> lstInput)
        {
            int intStepPos = 0;

            this.eChildProcessStatus = enumChildProcessStatus.eIni;

            int i = 0;
            for (i = 0; i < lstInput.Count; i++)
            {
                //Find the position of step in Master step list
                intStepPos = lstInput[i].intStepPos;

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                bool blStepResult = ChildFuncEx(intStepPos, false);

                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);

                if (blStepResult == false)
                {
                    return "Child process Ini fail at step [" + lstInput[i].intStepNumber.ToString() + "] . Return data: [" + this.lstTotalStep[intStepPos].objStepCheckingData.ToString() + "]. Please check & restart program!";
                }
            }

            //
            ClearBindingView();

            return "0"; //Return OK string code if everything is OK
        }

        /// <summary>
        /// For Polling Start of Child Process in Independent mode
        /// </summary>
        /// <returns></returns>
        public bool ChildProcessPollingStepEx()
        {
            int i = 0;
            int intStepPos = 0;
            for (i = 0; i < this.lstChildStartPoll.Count; i++)
            {
                //Find the position of step in Master step list
                intStepPos = this.lstChildStartPoll[i].intStepPos;

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                bool blStepResult = ChildFuncEx(intStepPos);

                //If 1 step is OK, it mean start polling is Ok! Just return true
                if(blStepResult==true)
                {
                    return true;
                }

                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);
            }

            //
            return false; //Return OK string code if everything is OK
        }

        //private int intCheckingEndServiceToken = 0;
        public bool ChildProcessCheckingEndServiceStepEx()
        {
            int i = 0;
            int intStepPos = 0;
            bool blTotalResult = true;
            for (i = 0; i < this.lstChildCheckingEndService.Count; i++)
            {
                //Find the position of step
                intStepPos = this.lstChildCheckingEndService[i].intStepPos;

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                bool blStepResult = false;

                //blStepResult = ChildFuncEx(intStepPos);

                while (blStepResult == false) //Wait until each step in this class pass
                {
                    blStepResult = ChildFuncEx(intStepPos);
                    //
                    this.DoEvents();
                }


                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);

                //If 1 step is fail, then all fail
                if (blStepResult == false)
                {
                    blTotalResult = false;
                    break;
                    //return true;
                }
            }

            //
            this.DoEvents();

            //
            return blTotalResult; //Return OK string code if everything is OK
        }

        public void ChildProcessModelIni(bool blGroupMode = false)
        {
            //**********************************************************************************
            //  NOTES: We need separate 2 case: Group Mode & Not Group Mode
            //  If setting in Group Mode
            //      + We need to create & ini for each child Process ID of each group
            //      + With binding view: all group are using only 1 class
            //
            //**********************************************************************************

            int i, j;
            string strRet = "";
            //1.1. Ini for Origin StepList
            if(blGroupMode==true) //In group Mode => Force to use origin steplist
            {
                if(this.clsChildSetting.blUsingOriginSteplist==false)
                {
                    MessageBox.Show("Error: Group Mode require using origin step list! Please check setting again!", "ChildProcessModelIni() Error");
                    Environment.Exit(0);
                }
            }

            //If setting using origin step list, then reading step list
            if(this.clsChildSetting.blUsingOriginSteplist==true)
            {
                this.clsStepList = new classStepList(this.clsChildSetting.strOriginStepListFileName, this.clsChildSetting.strOriginStepListSheetName, ref strRet,2000);
                if (strRet != "0") //Loading Fail
                {
                    MessageBox.Show("Step List Loading fail! Error message: [" + strRet + "] Please check & Restart program", "ChildProcessModelIni() Error");
                    Environment.Exit(0);
                }

                //Passing for all items in child process => There is only 1 class of origin step list for all!
                for(i=0;i<this.lstclsItemCheckInfo.Count;i++)
                {
                    this.lstclsItemCheckInfo[i].clsStepList = this.clsStepList;
                }
            }
           
            //1. Ini for Program List
            this.clsProgramList = new classProgramList(this.clsChildSetting.strProgramListFileName, this.clsChildSetting.strProgramListSheetName, ref strRet);
            if(strRet!= "0") //Loading Fail
            {
                MessageBox.Show("Child Process Program List Loading fail! Error message: [" + strRet + "] Please check & Restart program", "ChildProcessModelIni() Error");
                Environment.Exit(0);
            }

            if (blGroupMode == false)
            {
                //If not setting in Group Mode, then each child process has only 1 Item
                //All step in program list now re-config for Group number 1
                for (i = 0; i < this.clsProgramList.lstProgramList.Count; i++)
                {
                    this.clsProgramList.lstProgramList[i].strGroupNumber = "1";
                }
            }

            //Create program list for each item of child process
            for (i = 0; i < this.lstclsItemCheckInfo.Count; i++)
            {
                //Create new program list class for items in child process
                this.lstclsItemCheckInfo[i].clsProgramList = new classProgramList();

                //Add step belong to each item into created class
                for (j = 0; j < this.clsProgramList.lstProgramList.Count; j++)
                {
                    if (this.clsProgramList.lstProgramList[j].intStepSequenceID == -1) continue;
                    if (this.clsProgramList.lstProgramList[j].strGroupNumber == null) continue;
                    if (this.clsProgramList.lstProgramList[j].strGroupNumber.Trim() == (i + 1).ToString()) //matching
                    {
                        //Item's classes will be same instance with Child Process's classes
                        this.lstclsItemCheckInfo[i].clsProgramList.lstProgramList.Add(this.clsProgramList.lstProgramList[j]); //Class step data row
                    }
                }
                //Do classify for sub-child process program list
                this.lstclsItemCheckInfo[i].AnalyzeProgramList(this.lstclsItemCheckInfo[i].clsProgramList.lstProgramList);
            }

            //2.1. Compare Checking Program List with Origin Step List if setting using
            if (this.clsChildSetting.blUsingOriginSteplist == true)
            {
                if(blGroupMode==false) //Only compare origin step list with origin Program List if not in Group Mode
                {
                    //object objRet = this.CompareOriginStepListWithProgramList(this.clsChildOriginStepList.lstExcelList, ref this.lstChildTotal);
                    List<classStepDataInfor> lstclsNew = this.clsProgramList.lstProgramList;
                    object objRet = this.CompareOriginStepListWithProgramList(this.clsStepList.lstExcelList, ref lstclsNew);
                    this.clsProgramList.lstProgramList = lstclsNew;

                    if (objRet.ToString() != "0") //Result NG
                    {
                        MessageBox.Show(objRet.ToString() + ". Please check and restart program!", "CompareOriginStepListWithProgramList() Error");
                        Environment.Exit(0);
                    }
                }

                //Assign Sub-Program list for items in child process
                //Need to compare sub-program list with origin step list if setting in Group Mode
                if (blGroupMode == true)
                {
                    for (i = 0; i < this.lstclsItemCheckInfo.Count; i++)
                    {
                        List<classStepDataInfor> lstclsNew = this.lstclsItemCheckInfo[i].clsProgramList.lstProgramList;
                        object objRet = this.CompareOriginStepListGroupMode(i, this.clsStepList.lstExcelList, ref lstclsNew);
                        this.lstclsItemCheckInfo[i].clsProgramList.lstProgramList = lstclsNew;

                        if (objRet.ToString() != "0") //Result NG
                        {
                            MessageBox.Show(objRet.ToString() + "\r\nPlease check and restart program!", "CompareOriginStepListGroupMode() Error");
                            Environment.Exit(0);
                        }
                    }
                }
            }

            //Reload Program list for apply changing point
            this.clsProgramList.ReAssignInfo();

            //2. Analyze Program List & separate to Sub List
            AnalyzeProgramList(this.clsProgramList.lstProgramList);

            //Looking for each step in Step List and set pass condition in each Item's program list step
            for (i = 0; i < this.clsProgramList.lstProgramList.Count; i++) //For each step in program list
            {
               for(j=0;j<this.clsStepList.lstExcelList.Count;j++)
                {
                    if(this.clsProgramList.lstProgramList[i].intStepNumber==this.clsStepList.lstExcelList[j].intStepNumber)
                    {
                        this.clsProgramList.lstProgramList[i].blSTLPassCondition = true;
                    }
                }
            }

            //3. Loading Plug-in for Child Process
            clsChildExtension.PluginLoader(this.lstTotalStep, "Extensions");


            //3.1. Ini for  special control
            this.clsCommonFunc.intSourcesID = 2;
            this.clsCommonFunc.objSources = this;
            this.clsCommonFunc.intProcessId = this.intProcessID;

            this.clsCommonFunc.lstlstCommonCommandAnalyzer = this.clsCommonFunc.CommonSpecialControlIni(this.clsProgramList.lststrSpecialCmd); //Special command area
            this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer = this.clsCommonFunc.CommonTransAreaSpecialControlIni(this.clsProgramList.lststrTransAreaSpecialCmd); //Special command in Transmission area
            this.clsCommonFunc.lstlstCommonParaCommandAnalyzer = this.clsCommonFunc.CommonParaSpecicalControlIni(this.clsProgramList.lstlstobjStepPara); //parameter area

            //4. Ini for some variable
            //Ini for Result Test class
            this.clsItemResult = new clsItemResultInformation();

            //Ini for Master Sequence testing
            this.clsSeqenceTestData = new classSequenceTestData();
            this.clsSeqenceTestData.lstblForceChange = new List<bool>();
            this.clsSeqenceTestData.lstobjNewValue = new List<object>();
            bool blTemp = false;
            object objTemp = new object();
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                blTemp = false;
                objTemp = new object();
                //
                this.clsSeqenceTestData.lstblForceChange.Add(blTemp);
                this.clsSeqenceTestData.lstobjNewValue.Add(objTemp);
            }

            //5. Ini for some variables
            this.eChildCheckMode = enumChildProcessCheckingMode.eNormal;

            this.intStepPosRunning = 0;

            this.eChildProcessStatus = enumChildProcessStatus.eDoNothing;
        }

        //////////////////////////////////////////////////////////////////
        public object CompareOriginStepListWithProgramList(List<classStepDataInfor> lstExcelList, ref List<classStepDataInfor> lstProgramList, bool blConfirmOnly = false)
        {
            //Rule of Comparation:
            //  1. All step in Origin step list must be executed in program list with Test Class is 3 (only Test Class = 3 has the right to decide Checking result is PASS or FAIL)
            //  2. All program list step which implemented for 1 origin step must be next to each other (continuous order in program list)
            //
            //  3. With each step in step list: there must be at least 1 step implemented in program list has similar info
            //          - Step Number 
            //          - Step Name
            //          - Lo limit
            //          - Hi Limit
            //          - Unit 
            //          - Transmission (factory command)
            //          - Receive
            //      And this step will be representative for corresponding step in step list
            //
            //  4. The program list step if marking implemented for step number which doesn't not exist in Step List => Error
            //  5. The order of list of step in origin step list must be keep same as in program list (Note that with 1 step in origin step list <=> many step in program list)

            
            //1. Check 1st condition: All step in Origin step list must be executed in program list with Test Class is 3 (only Test Class = 3 has the right to decide Checking result is PASS or FAIL)
            int i, j = 0;
            int intOriginStepNum = 0;
            bool blFound = false;
            int intTemp = 0;
            int intPos1 = 0;
            int intPos2 = 0;

            //
            if (lstExcelList == null) return "Error: lstExcelList input is null";
            if (lstProgramList == null) return "Error: lstProgramList input is null";

            //
            for(i=0;i<lstExcelList.Count;i++)
            {
                intOriginStepNum = lstExcelList[i].intStepNumber;
                //Looking for step implemented in Program List for this step - Note that only care Test Class = 3. Warning if another Test class implemented?
                for(j=0;j<lstProgramList.Count;j++)
                {
                    if (lstProgramList[j].strOriginStepNumber == null) continue;
                    //
                    if(lstProgramList[j].strOriginStepNumber.Trim()== lstExcelList[i].intStepNumber.ToString())
                    {
                        if(lstProgramList[j].intStepClass != 3)
                        {
                            return "Waring: Only step with Step Class = 3 should be implemented for Origin step in step list! The program step number ["
                               + lstProgramList[j].intStepNumber.ToString() + "] has Test Class = " + lstProgramList[j].intStepClass.ToString();
                        }
                        else //If Ok, then add to list of origin step
                        {
                            if (blConfirmOnly==false)
                            {
                                lstExcelList[i].lstintSubProgramListStep.Add(lstProgramList[j].intStepNumber);
                            }
                        }
                    }
                }
                //If Looking for all but there is no step in Program List which implement for origin step => warning & Exit!
                if(lstExcelList[i].lstintSubProgramListStep.Count==0)
                {
                    return "Error: There is no step in Program list with Test Class 3 implemented for origin step number ["
                                + lstExcelList[i].intStepNumber.ToString() + "] of Origin Step List";
                }
            }

            //2. Check 2nd Condition: All program list step which implemented for 1 origin step must be next to each other (continuous order in program list)
            for(i=0;i<lstExcelList.Count;i++)
            {
                for(j=0;j<lstExcelList[i].lstintSubProgramListStep.Count;j++)
                {
                    if (lstExcelList[i].lstintSubProgramListStep.Count < 2) break; //Only 1 step
                    //
                    if (j == (lstExcelList[i].lstintSubProgramListStep.Count - 1)) break; //last step, finish confirming
                    //
                    intPos1 = this.FindCheckingStepPos(lstExcelList[i].lstintSubProgramListStep[j]);
                    intPos2 = this.FindCheckingStepPos(lstExcelList[i].lstintSubProgramListStep[j+1]);

                    if (intPos1 == -1)
                    {
                        return "Error: cannot find step number [" + lstExcelList[i].lstintSubProgramListStep[j].ToString() + "] in program list";
                    }

                    if (intPos2 == -1)
                    {
                        return "Error: cannot find step number [" + lstExcelList[i].lstintSubProgramListStep[j + 1].ToString() + "] in program list";
                    }
                    //
                    intTemp = Math.Abs(intPos2 - intPos1);

                    if(intTemp != 1)
                    {
                        return "Error: 2  program list step number [" + lstExcelList[i].lstintSubProgramListStep[j].ToString() +
                            "] & [" + lstExcelList[i].lstintSubProgramListStep[j + 1].ToString() + "] implement for same origin step [" +
                        lstExcelList[i].intStepNumber.ToString() + "] but not continuous to each other";
                    }
                }
            }

            //3. Check 3rd condition: Looking for Representative step in program List for each origin step
            for (i = 0; i < lstExcelList.Count; i++)
            {
                blFound = false;
                for(j=0;j<lstExcelList[i].lstintSubProgramListStep.Count;j++)
                {
                    if(lstExcelList[i].intStepNumber == lstExcelList[i].lstintSubProgramListStep[j])
                    {
                        blFound = true;
                        break;
                    }
                }

                //Not Found?
                if(blFound==false)
                {
                    return "Error: cannot find Representative step for origin step [" + lstExcelList[i].intStepNumber.ToString() + "] in program list";
                }

                //If Found, then check info of representative step must be same with origin step
                int intStepPos = this.FindCheckingStepPos(lstExcelList[i].intStepNumber);
                if(intStepPos == -1)
                {
                    return "Error: cannot find step number [" + lstExcelList[i].intStepNumber.ToString() + "] in program list";
                }

                //Passing all information from Step List's step to Representative step in Program List
                lstProgramList[intStepPos].strStepName = lstExcelList[i].strStepName; //Step name
                lstProgramList[intStepPos].objLoLimit = lstExcelList[i].objLoLimit; //Step Low limit
                lstProgramList[intStepPos].objHiLimit = lstExcelList[i].objHiLimit; //Step Hi limit
                lstProgramList[intStepPos].strUnitName = lstExcelList[i].strUnitName; //Step unit name
                lstProgramList[intStepPos].strTransmisstion = lstExcelList[i].strTransmisstion; //Step transmission area
                lstProgramList[intStepPos].strTransmisstionEx = lstExcelList[i].strTransmisstionEx;
                lstProgramList[intStepPos].strReceive = lstExcelList[i].strReceive; //Step receive area
            }

            //4. Check 4th condition: The program list step if marking implemented for step number which doesn't not exist in Step List => Error
            for(i=0;i<lstProgramList.Count;i++)
            {
                if (lstProgramList[i].strOriginStepNumber == null) continue;
                //
                if (int.TryParse(lstProgramList[i].strOriginStepNumber.Trim(), out intTemp) == false) continue; //No care if not integer
                //
                blFound = false;
                for(j=0;j<lstExcelList.Count;j++)
                {
                    if(intTemp == lstExcelList[j].intStepNumber) //Found
                    {
                        blFound = true;
                        break;
                    }
                }
                //
                if(blFound==false)
                {
                    return "Error: The step number [" + lstProgramList[i].intStepNumber.ToString() + "] in program list implemented for Step [" + lstProgramList[i].strOriginStepNumber +
                    "] which does not exist in origin step list!";
                }
            }

            //5. Check 5th condition: The order of list of step in origin step list must be keep same as in program list (Note that with 1 step in origin step list <=> many step in program list)
            //   Algorithm: The position of Representative step must be same order
            for(i = 0;i<lstExcelList.Count;i++)
            {
                if (i == 0) continue; //Not check 1st step
                if (i == (lstExcelList.Count - 1)) break; //Finish confirm

                intPos1 = this.FindCheckingStepPos(lstExcelList[i].intStepNumber);
                intPos2 = this.FindCheckingStepPos(lstExcelList[i+1].intStepNumber);

                if (intPos1 == -1)
                {
                    return "Error: cannot find step number [" + lstExcelList[i].intStepNumber.ToString() + "] in program list";
                }

                if (intPos2 == -1)
                {
                    return "Error: cannot find step number [" + lstExcelList[i + 1].intStepNumber.ToString() + "] in program list";
                }

                //
                if(intPos1>=intPos2)
                {
                    return "Error: The order of implement step in program list doest not follow Origin step list: Step number [" + lstExcelList[i].intStepNumber.ToString()
                                     + "] & [" + lstExcelList[i + 1].intStepNumber.ToString() + "]";
                }
            }

            //If everything is OK, then return OK code
            return "0";
        }

        //////////////////////////////////////////////////////////////////
        public object CompareOriginStepListGroupMode(int intGroupID, List<classStepDataInfor> lstExcelList, ref List<classStepDataInfor> lstProgramList, bool blConfirmOnly = false)
        {
            //Rule of Comparation:
            //  1. All step in Origin step list must be executed in program list with Test Class is 3 (only Test Class = 3 has the right to decide Checking result is PASS or FAIL)
            //  2. With each step in step list: there must be at least 1 step implemented in program list has similar info
            //          - Step Number 
            //          - Step Name
            //          - Lo limit
            //          - Hi Limit
            //          - Unit 
            //          - Transmission (factory command)
            //          - Receive
            //      And this step will be representative for corresponding step in step list
            //
            //  3. The program list step if marking implemented for step number which doesn't not exist in Step List => Error
            //  4. The order of list of step in origin step list must be keep same as in program list (Note that with 1 step in origin step list <=> many step in program list)

            //1. Check 1st condition: All step in Origin step list must be executed in program list with Test Class is 3 (only Test Class = 3 has the right to decide Checking result is PASS or FAIL)
            int i, j = 0;
            int intOriginStepNum = 0;
            bool blFound = false;
            int intTemp = 0;
            int intPos1 = 0;
            int intPos2 = 0;

            //
            if (lstExcelList == null) return "Group " + (intGroupID + 1).ToString() + " Error: lstExcelList input is null";
            if (lstProgramList == null) return "Group " + (intGroupID + 1).ToString() + " Error: lstProgramList input is null";


            //1. All step in Origin step list must be executed in program list with Test Class is 3 (only Test Class = 3 has the right to decide Checking result is PASS or FAIL)
            for (i = 0; i < lstExcelList.Count; i++)
            {
                intOriginStepNum = lstExcelList[i].intStepNumber;
                //Looking for step implemented in Program List for this step - Note that only care Test Class = 3. Warning if another Test class implemented?
                for (j = 0; j < lstProgramList.Count; j++)
                {
                    if (lstProgramList[j].strOriginStepNumber.Trim() == lstExcelList[i].intStepNumber.ToString())
                    {
                        if (lstProgramList[j].intStepClass != 3)
                        {
                            return "Group " + (intGroupID + 1).ToString() + " Waring: Only step with Step Class = 3 should be implemented for Origin step in step list! The program step number ["
                                + lstProgramList[j].intStepNumber.ToString() + "] has Test Class = " + lstProgramList[j].intStepClass.ToString();
                        }
                        else //If Ok, then add to list of origin step
                        {
                            if (blConfirmOnly == false)
                            {
                                lstExcelList[i].lstintSubProgramListStep.Add(lstProgramList[j].intStepNumber);
                            }
                        }
                    }
                }
                //If Looking for all but there is no step in Program List which implement for origin step => warning & Exit!
                if (lstExcelList[i].lstintSubProgramListStep.Count == 0)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: There is no step in Program list with Test Class 3 implemented for origin step number ["
                                + lstExcelList[i].intStepNumber.ToString() + "] of Origin Step List";
                }
            }

            //2. Check 2nd condition: Looking for Representative step in each sub child program List for each origin step
            //   With Group Mode, representative step define in Group Data (1st collumn). Ex "1,2" => representative of step 2 of item 1
            for (i = 0; i < lstExcelList.Count; i++)
            {
                int intRepresentStepNum = lstExcelList[i].intStepNumber;
                blFound = false;
                int intFirstPos = 0;

                for (j = 0; j < lstProgramList.Count;j++)
                {
                    if(intRepresentStepNum.ToString() == lstProgramList[j].strOriginStepNumber)
                    {
                        //Check if there are multi representative step => error
                        if(blFound==true)
                        {
                            return "Group " + (intGroupID + 1).ToString() + " Error: There is more than 1 representative step for origin step number ["
                                + lstExcelList[i].intStepNumber.ToString() + "] of Origin Step List\r\n"+
                                "Please check 2 step in program list: " + this.clsProgramList.lstProgramList[intFirstPos].intStepNumber.ToString() + " And " +
                                this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].intStepNumber.ToString();
                        }


                        blFound = true;
                        intFirstPos = lstProgramList[j].intStepPos;
                        //Passing all information from Step List's step to Representative step in Program List
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].strStepName = lstExcelList[i].strStepName; //Step name
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].objLoLimit = lstExcelList[i].objLoLimit; //Step Low limit
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].objHiLimit = lstExcelList[i].objHiLimit; //Step Hi limit
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].strUnitName = lstExcelList[i].strUnitName; //Step unit name
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].strTransmisstion = lstExcelList[i].strTransmisstion; //Step transmission area
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].strTransmisstionEx = lstExcelList[i].strTransmisstionEx;
                        this.clsProgramList.lstProgramList[lstProgramList[j].intStepPos].strReceive = lstExcelList[i].strReceive; //Step receive area
                    }
                }

                //Not Found?
                if (blFound == false)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: cannot find Representative step for origin step [" + lstExcelList[i].intStepNumber.ToString() + "] in program list";
                }
            }

            //3. Check 3rd condition: The program list step if marking implemented for step number which doesn't not exist in Step List => Error
            for (i = 0; i < lstProgramList.Count; i++)
            {
                if (int.TryParse(lstProgramList[i].strOriginStepNumber.Trim(), out intTemp) == false) continue; //No care if not integer
                //
                blFound = false;
                for (j = 0; j < lstExcelList.Count; j++)
                {
                    if (intTemp == lstExcelList[j].intStepNumber) //Found
                    {
                        blFound = true;
                        break;
                    }
                }
                //
                if (blFound == false)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: The step number [" + lstProgramList[i].intStepNumber.ToString() + "] in program list implemented for Step [" + lstProgramList[i].strOriginStepNumber +
                    "] which does not exist in origin step list!";
                }
            }

            //4. Check 4th condition: The order of list of step in origin step list must be keep same as in program list (Note that with 1 step in origin step list <=> many step in program list)
            //   Algorithm: The position of Representative step must be same order
            for (i = 0; i < lstExcelList.Count; i++)
            {
                if (i == 0) continue; //Not check 1st step
                if (i == (lstExcelList.Count - 1)) break; //Finish confirm

                //intPos1 = this.FindCheckingStepPos(lstExcelList[i].intStepNumber);
                //intPos2 = this.FindCheckingStepPos(lstExcelList[i + 1].intStepNumber);
                intPos1 = -1;
                intPos2 = -1;
                bool blFound1 = false;
                bool blFound2 = false;

                for (j = 0; j < lstProgramList.Count; j++)
                {
                    if((blFound1 == false)&&(lstExcelList[i].intStepNumber.ToString() == lstProgramList[j].strOriginStepNumber))
                    {
                        intPos1 = lstProgramList[j].intStepPos;
                        blFound1 = true;
                    }

                    if ((blFound2 == false) && (lstExcelList[i + 1].intStepNumber.ToString() == lstProgramList[j].strOriginStepNumber))
                    {
                        intPos2 = lstProgramList[j].intStepPos;
                        blFound2 = true;
                    }
                }

                if (intPos1 == -1)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: cannot find step number [" + lstExcelList[i].intStepNumber.ToString() + "] in program list";
                }

                if (intPos2 == -1)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: cannot find step number [" + lstExcelList[i + 1].intStepNumber.ToString() + "] in program list";
                }

                //
                if (intPos1 >= intPos2)
                {
                    return "Group " + (intGroupID + 1).ToString() + " Error: The order of implement step in program list doest not follow Origin step list: Step number [" + lstExcelList[i].intStepNumber.ToString()
                                        + "] & [" + lstExcelList[i + 1].intStepNumber.ToString() + "]";
                }
            }


            //If everything is OK, then return OK code
            return "0";
        }

        //////////////////////////////////////////////////////////////////
        public string ChildProcessUserEndStepEx()
        {
            if (this.lstChildUserEnd.Count == 0) return "0";

            int i = 0;
            string strResult = "0";
            for (i = 0; i < this.lstChildUserEnd.Count; i++)
            {
                bool blStepResult = ChildFuncEx(this.lstChildUserEnd[i].intStepPos);
                if (blStepResult == false)
                {
                    strResult = "Warning: ChildProcess" + (this.intProcessID + 1).ToString() + " user end task fail at step [" + this.lstTotalStep[this.lstChildUserEnd[i].intStepPos].intStepNumber.ToString() +
                                    "]. Return data [" + this.lstTotalStep[this.lstChildUserEnd[i].intStepPos].objStepCheckingData.ToString() + "].";
                }
            }

            return strResult;
        }

        //////////////////////////////////////////////////////////////////
        //Analyze Program List to Sub-List for control checking sequence
        public void AnalyzeProgramList(List<classStepDataInfor> lstInput)
        {
            int i = 0;

            this.lstTotalStep = lstInput;
            //Analyze all item in Program List & assign them to proper category
            for (i = 0; i < lstInput.Count; i++)
            {
                //Check if Row input belong to Main Sequence
                if(lstInput[i].intStepSequenceID != 0) //Not adding if step not belong to Main Sequence
                {
                    continue;
                }

                //
                classStepDataInfor temp = lstInput[i];

                switch (lstInput[i].intStepClass)
                {
                    case 0: //Ini
                        this.lstChildIni.Add(temp);
                        break;
                    case 1: //Start polling
                        this.lstChildStartPoll.Add(temp);
                        break;
                    case 2: //After Start
                        this.lstChildAfterStart.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;
                    case 3: //Checking Process
                        this.lstChildChecking.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;
                    case 4: //Finish
                        this.lstChildFinish.Add(temp);
                        this.lstChildThreadCheck.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;
                    
                    case 50: //After finish
                        this.lstChildCheckingEndService.Add(temp);
                        this.lstChildCheckAll.Add(temp);
                        break;

                    case 51:
                        this.lstChildCheckingEndService.Add(temp);
                        break;

                    case 100: //Background polling
                        this.lstChildBackgroundPolling.Add(temp);
                        break;
                    case 1000: //User End
                        this.lstChildUserEnd.Add(temp);
                        break;
                    default:
                        MessageBox.Show("Error: Cannot recognize Child Process Step Class [" + lstInput[i].intStepClass.ToString() + "]");
                        break;
                }
            }
        }

        public System.Data.DataTable CaldataTableOptionView()
        {
            this.dataTableOptionView = new System.Data.DataTable();

            //Add column Info
            this.dataTableOptionView.Columns.Add("OriginStep"); //0
            this.dataTableOptionView.Columns.Add("Number"); //1
            this.dataTableOptionView.Columns.Add("Test Name"); //2
            this.dataTableOptionView.Columns.Add("Result"); //3
            this.dataTableOptionView.Columns.Add("Lo Limit"); //4
            this.dataTableOptionView.Columns.Add("Hi Limit"); //5
            this.dataTableOptionView.Columns.Add("Unit"); //6
            this.dataTableOptionView.Columns.Add("Tact Time (ms)"); //7
            this.dataTableOptionView.Columns.Add("Func ID"); //8

            //Add data Row
            int i = 0;
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                DataRow temp = this.dataTableOptionView.NewRow();

                //Check if Row is No caring sequence
                if(this.lstTotalStep[i].intStepSequenceID==-1)
                {
                    if((i+1)<this.lstTotalStep.Count)
                    {
                        if(this.lstTotalStep[i+1].intStepSequenceID==1) //User Function
                        {
                            temp[0] = "FUNC"; //No.
                            temp[1] = this.lstTotalStep[i+1].strUserFunctionName; //Test Name
                        }
                    }

                    this.dataTableOptionView.Rows.Add(temp);
                    continue;
                }

                temp[0] = this.lstTotalStep[i].strOriginStepNumber; //No.
                temp[1] = this.lstTotalStep[i].intStepNumber.ToString(); //No.
                temp[2] = this.lstTotalStep[i].strStepName; //Test Name

                if (this.lstTotalStep[i].blStepChecked == true) //Result
                {
                    temp[3] = this.GetConvertDataResult(i);
                }
                else
                {
                    temp[3] = "";
                }

                if (this.lstTotalStep[i].strUnitName.ToUpper().Trim() == "H")
                {
                    temp[4] = Convert.ToInt32(this.lstTotalStep[i].objLoLimit).ToString("X"); //Lo limit
                    temp[5] = Convert.ToInt32(this.lstTotalStep[i].objHiLimit).ToString("X"); //Hi limit
                }
                else
                {
                    temp[4] = this.lstTotalStep[i].objLoLimit.ToString(); //Lo limit
                    temp[5] = this.lstTotalStep[i].objHiLimit.ToString(); //Hi limit
                }
                
                temp[6] = this.lstTotalStep[i].strUnitName; //Unit
                temp[7] = this.lstTotalStep[i].dblStepTactTime.ToString(); //Tact Time
                temp[8] = this.lstTotalStep[i].intStepClass.ToString() + "-" + this.lstTotalStep[i].intJigId.ToString() + "-" +
                    this.lstTotalStep[i].intHardwareId.ToString() + "-" + this.lstTotalStep[i].intFunctionId.ToString(); //Function ID
                this.dataTableOptionView.Rows.Add(temp);
            }

            return this.dataTableOptionView;
        }

        void tmrIndependent_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.tmrIndependent.Stop();
            //
            if(this.eChildRunningMode != enumSystemRunningMode.IndependentMode)
            {
                this.tmrIndependent.Enabled = false;
                return;
            }
            //
            // if(this.thrdChildProcess.IsAlive==false)
			if(this.taskCheckingProcess.Status != TaskStatus.Running)
            {
                try
                {
                    bool blResult = false;
                    //Check checking end service request
                    if(this.blRequestCheckingEndService == true)
                    {
                        blResult = this.ChildProcessCheckingEndServiceStepEx();
                        if(blResult==true)
                        {
                            this.blRequestCheckingEndService = false; //reset
                            this.ResetAllColorTableView();
                        }
                    }

                    if (this.blRequestCheckingEndService == false)
                    {
                        //Polling start
                        blResult = this.ChildProcessPollingStepEx();
                        if (blResult == true)
                        {
                            //1. Creating thread to start
                            //this.thrdChildProcess = new System.Threading.Thread(this.ThreadChecking);
                            //this.thrdChildProcess.Name = "ChildProcessID" + this.intProcessID.ToString();


                            //Request saving data of all items
                            this.blRequestSavingData = true;
                            if (this.blGroupMode == true)
                            {
                                for (int i = 0; i < this.lstclsItemCheckInfo.Count; i++)
                                {
                                    this.lstclsItemCheckInfo[i].blRequestSavingData = true;
                                }
                            }

                            //
                            this.ResetAllColorTableView();

							//Start Checking
							//this.thrdChildProcess.Start();
							//while (this.thrdChildProcess.IsAlive == false) ;//Wait until thread are started
							this.taskCheckingProcess = Task.Factory
							.StartNew(this.ThreadChecking, TaskCreationOptions.LongRunning);
						}
                    }
                    
                }
                catch
                {
                    ;
                }
            }
            
            //
            this.tmrIndependent.Start();
        }

        //******************************************************************************************
        //**************************FOR CHILD PROCESS - SPECIAL CONTROL ****************************
        //******************************************************************************************

        #region _ChildProcessSpecialControl

        public clsCommonSpecialControlFunction clsCommonFunc = new clsCommonSpecialControlFunction();

        //************************************************************
        public object AddParameterToNcalc(string strParaName, object objSetValue)
        {
            return this.clsCommonFunc.AddParameter(strParaName, objSetValue);
        }

        public object SetParameterToNcalc(string strParaName, object objSetValue)
        {
            return this.clsCommonFunc.SetParameter(strParaName, objSetValue);
        }

        public object DelParameterToNcalc(string strParaName)
        {
            return this.clsCommonFunc.DelParameter(strParaName);
        }

        //************************************************************
        public object UserFunction(List<object> lstobjInput)
        {
            object objRet = new object();

            if (this.clsProgramList.lstclsUserFunction == null) return objRet;
            if (this.clsProgramList.lstclsUserFunction.Count == 0) return objRet;
            //Checking 
            if (lstobjInput.Count == 0) return "ChildProcessUserFunction() error: List of parameter input has no element";
            //
            string strUserFunctionName = lstobjInput[0].ToString().Trim();

            int i = 0;

            int intIndex = this.ChildProSearchUserFunc(strUserFunctionName);
            if (intIndex == -1) return "ChildProcessUserFunction() error: could not found user function [" + strUserFunctionName + "]";

            //Passing parameter for function
            this.clsProgramList.lstclsUserFunction[intIndex].lstobjParameter = new List<object>();
            for (i = 1; i < lstobjInput.Count;i++)
            {
                this.clsProgramList.lstclsUserFunction[intIndex].lstobjParameter.Add(lstobjInput[i]);
            }

            //
            objRet = this.ChildProcessUserFunctionStepEx(intIndex);
            //
           return objRet;
        }

        public object ChildProcessUserFunctionStepEx(int intUserFuncID)
        {
            int i = 0;
            bool blResult = false;
            object objRet = new object();

            for(i=0;i<this.clsProgramList.lstclsUserFunction[intUserFuncID].lstclsStepRowData.Count;i++)
            {
                int intStepPos = 0;
                intStepPos = this.clsProgramList.lstclsUserFunction[intUserFuncID].lstclsStepRowData[i].intStepPos;

                //For Special control
                ChildProcessSpecialControlCommandEx(1, intStepPos, ref i);

                //Execute Function
                blResult = this.ChildFuncEx(intStepPos);

                objRet = this.lstTotalStep[intStepPos].objStepCheckingData;

                #region _HandleCheckingMode

                //*************************FOR EACH TEST MODE IN ITEM INFO***********************************
                //For Normal Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eNormal) //Normal Mode
                {
                    //Checking until meet 1 step fail, then stop or until all steps pass
                    if (this.lstTotalStep[intStepPos].blStepResult == false)
                    {
                        //If step fail belong to Checking Class (Test class = 3), then we jump into first step of Finish Process (Test class = 4)
                        //But what happen if there is no Finish Process? Where will we jump to???????????????????????????????????????????????????????????????????????????????????????
                        //We will exit checking process.
                        //If step fail belong to After Start(test class=2) or Finish Process (test class=4): we do nothing.
                        //if (this.lstChildTotal[intStepPos].intStepClass == 3)
                        //{
                        //    i = intSearchStepPosInClass(intFirstStepFinishPos, this.lstChildCheckAll) - 1;
                        //}
                    }
                }

                //For single checking Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eSingle) //Single Mode
                {
                    this.blAllowContinueRunning = false;
                    while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                    {
                        if (this.blRequestStopRunning == true)
                        {
                            i = this.clsProgramList.lstclsUserFunction[intUserFuncID].lstclsStepRowData.Count; //Ignore all next steps and come to ending point
                            break;
                        }
                    }
                }

                //For Step checking Mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eStep) //Step Mode
                {
                    if ((this.lstTotalStep[intStepPos].blStepResult == false) || (this.intStepModePosSelected == this.lstTotalStep[intStepPos].intStepPos))
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            if (this.blRequestStopRunning == true)
                            {
                                i = this.clsProgramList.lstclsUserFunction[intUserFuncID].lstclsStepRowData.Count; //Ignore all next steps and come to ending point
                                break;
                            }
                        }
                    }
                }
                //For Fail checking mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eFail) //Fail Mode
                {
                    if (this.lstTotalStep[intStepPos].blStepResult == false)
                    {
                        this.blAllowContinueRunning = false;
                        while (this.blAllowContinueRunning == false) //Keep Until NEXT button pressed
                        {
                            if (this.blRequestStopRunning == true)
                            {
                                i = this.clsProgramList.lstclsUserFunction[intUserFuncID].lstclsStepRowData.Count; //Ignore all next steps and come to ending point
                                break;
                            }
                        }
                    }
                }
                //For All checking mode - OK
                if (this.eChildCheckMode == enumChildProcessCheckingMode.eAll) //All Mode
                {
                    //Automatically run all steps without caring result of each step OK or NG
                }
                #endregion

                //For Special control
                ChildProcessSpecialControlCommandEx(0, intStepPos, ref i);
            }

            //Return Last Checking step Function Result - regulation of User Function
            this.clsProgramList.lstclsUserFunction[intUserFuncID].objReturnData = objRet;
            return objRet;
        }

        public object ChildProcessGetUserFuncPara(string strUserFuncName, int intParaPos)
        {
            object objRet = new object();
            //
            int intIndex = this.ChildProSearchUserFunc(strUserFuncName);
            if (intIndex == -1) return "ChildProcessGetUserFuncPara() error: could not found user function [" + strUserFuncName + "]";

            if(this.clsProgramList.lstclsUserFunction[intIndex].lstobjParameter.Count-1<intParaPos)
            {
                return "ChildProcessGetUserFuncPara() error: User function [" + strUserFuncName + "] has not enough parameter to return.";
            }

            objRet = this.clsProgramList.lstclsUserFunction[intIndex].lstobjParameter[intParaPos];

            if (objRet==null)
            {
                objRet = "null";
            }

            //
            return objRet;
        }

        public int ChildProSearchUserFunc(string strNameInput)
        {
            int intRet = -1; //Not found default

            int i = 0;
            for (i = 0; i < this.clsProgramList.lstclsUserFunction.Count; i++)
            {
                if (strNameInput.ToUpper().Trim() == this.clsProgramList.lstclsUserFunction[i].strUserFunctionName.Trim().ToUpper())
                {
                    intRet = i;
                    break;
                }
            }

            return intRet;
        }

        //*************************Support Funtion*********************
        public int FindCheckingStepPos(int intStepNumber)
        {
            //intStepNumber: step number want to looking for
            //Return: the position of step in all step list. If not found, then return -1

            int i = 0;
            for (i = 0; i < this.clsProgramList.lstProgramList.Count; i++)
            {
                if (intStepNumber == this.clsProgramList.lstProgramList[i].intStepNumber)
                {
                    //MessageBox.Show("Found step " + intStepNumber.ToString() + " at position " + (i + 1).ToString(), "FindStepOrder()");
                    return i;
                }
            }
            //MessageBox.Show("No found step " + intStepNumber.ToString() , "FindStepOrder()");
            return -1;
        }

        //*************************************************************
        public int FindStepListStepPos(int intStepNumber)
        {
            //intStepNumber: step number want to looking for
            //Return: the position of step in step list. If not found, then return -1

            int i = 0;
            for (i = 0; i < this.clsStepList.lstExcelList.Count; i++)
            {
                if (intStepNumber == this.clsStepList.lstExcelList[i].intStepNumber)
                {
                    return i;
                }
            }
            //
            return -1;
        }

        //*************************************************************
        public int FindCheckingToken(int intStepPos)
        {
            int i = 0;
            if(this.lstTotalStep[intStepPos].intStepSequenceID == 0) //main sequence
            {
                if (this.lstTotalStep[intStepPos].intStepClass == 0) //Ini class
                {
                    for (i = 0; i < this.lstChildIni.Count; i++)
                    {
                        if (this.lstChildIni[i].intStepPos == intStepPos) return i;
                    }
                }
                else if (this.lstTotalStep[intStepPos].intStepClass == 1) //Start Polling Class
                {
                    for (i = 0; i < this.lstChildStartPoll.Count; i++)
                    {
                        if (this.lstChildStartPoll[i].intStepPos == intStepPos) return i;
                    }
                }
                else if (this.lstTotalStep[intStepPos].intStepClass == 1000) //User End class
                {
                    for (i = 0; i < this.lstChildUserEnd.Count; i++)
                    {
                        if (this.lstChildUserEnd[i].intStepPos == intStepPos) return i;
                    }
                }
                else if (this.lstTotalStep[intStepPos].intStepClass == 100) //Child Background Class
                {
                    for (i = 0; i < this.lstChildBackgroundPolling.Count; i++)
                    {
                        if (this.lstChildBackgroundPolling[i].intStepPos == intStepPos) return i;
                    }
                }
                else //Default is checking in Child Check All class
                {
                    for (i = 0; i < this.lstChildCheckAll.Count; i++)
                    {
                        if (this.lstChildCheckAll[i].intStepPos == intStepPos) return i;
                    }
                }
            }
            else if(this.lstTotalStep[intStepPos].intStepSequenceID==1) //User Function Sequence => Looking follow user function name
            {
                string strUserFuncName = "";
                strUserFuncName = this.lstTotalStep[intStepPos].strUserFunctionName;
                int intIndex = this.ChildProSearchUserFunc(strUserFuncName);
                if(intIndex!=-1)
                {
                    for(i=0;i<this.clsProgramList.lstclsUserFunction[intIndex].lstclsStepRowData.Count;i++)
                    {
                        if(this.clsProgramList.lstclsUserFunction[intIndex].lstclsStepRowData[i].intStepPos==intStepPos)
                        {
                            return i;
                        }
                    }
                }
            }
            

            //Not found - return error code
            return -1;
        }

        //*************************************************************
        public int FindFuncIDPartContent(int intJigID, int intHardID, int intFuncID)
        {
            int i = 0;
            for(i=0;i<this.clsChildExtension.IFunctionCatalog.Count;i++)
            {
                if((intJigID == this.clsChildExtension.IFunctionCatalog[i].intJigID)&&
                    (intHardID == this.clsChildExtension.IFunctionCatalog[i].intHardID) &&
                    (intFuncID == this.clsChildExtension.IFunctionCatalog[i].intFuncID))
                {
                    return this.clsChildExtension.IFunctionCatalog[i].intPartID;
                }
            }

            return -1; //Not found
        }
        //*****************************************************************************************************************************
        #endregion

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }
        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        //Constructor
        public clsChildProcessModel()
        {
            this.clsCommonMethod = new classCommonMethod();
            this.clsChildExtension = new clsMEFLoading.clsExtensionHandle();
            this.clsChildSetting = new clsProcessSettingReading();
            this.lstclsItemCheckInfo = new List<classItemImformation>();
            this.clsChildModelBindingView = new clsChildModelBindingSupport();

            // this.thrdChildProcess = new System.Threading.Thread(this.ThreadChecking);
            this.taskCheckingProcessCancelTokenSource = new CancellationTokenSource();
            this.taskBackGroundCancelTokenSource = new CancellationTokenSource();

            this.dataTableOptionView = new DataTable();
            this.lststrProgramListUserPreInfo = new List<string>();
            this.lststrProgramListUserAfterInfo = new List<string>();
            this.clsSeqenceTestData = new classSequenceTestData();
            //Ini for Independent timer
            this.tmrIndependent = new System.Timers.Timer();
            this.tmrIndependent.Interval = 10;
            this.tmrIndependent.Enabled = false;
            this.tmrIndependent.Elapsed += tmrIndependent_Elapsed;

            //
            this.clsStepList = new classStepList();
        }
    }

    //**********************************************************
    //For convenient, we have support binding View class
    public class clsChildModelBindingSupport
    {
        //Group Mode - Update from what sub-child process ID
        public int intSubChildProcessID { get; set; } //If not belong to any sub-Child process, then it will be -1

        //Table checking result
        public string strItemResult { get; set; }
        public System.Windows.Media.SolidColorBrush clrItemResultBackGround { get; set; }

        //Table detail info
        public string strResultPassFail { get; set; }
        public string strPassRate { get; set; }
        public string strItemCheckPass { get; set; }
        public string strItemCheckCount { get; set; }
        public string strStatus { get; set; }
        public string strFailInfo { get; set; }
        public string strFailData { get; set; }
        public string strCheckPoint { get; set; }
        public string strItemInfo { get; set; } //combine of all above info
        public string strItemNotes { get; set; }

        //For Saving data request - Independent mode & Single Process mode
        public bool blRequestSavingData { get; set; }
        public bool blRequestUpdatePassRate { get; set; }

        public clsChildModelBindingSupport()
        {
            this.clrItemResultBackGround = System.Windows.Media.Brushes.LightBlue;
            this.strItemInfo = "";
        }
    }
    //**********************************************************

}
