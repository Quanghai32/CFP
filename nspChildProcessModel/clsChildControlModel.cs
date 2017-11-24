using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using nspCFPExpression;
using nspProgramList;
using nspCFPInfrastructures;
using System.Data;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
//using System.Windows;

namespace nspChildProcessModel
{
    public enum enumSystemRunningMode
    {
        ParallelMode,  //All child process run paralell, no care about other child process speed
        SingleThreadMode, //All child process run, but with only 1 thread
        SingleProcessMode, //want to run only one process for testing?
        IndependentMode, //Each process run & no care each others
        NotRecognize //Unkown mode
    }

    //Define class for main variables
    public class clsMainVariables 
    {
        //For total result checking of all items
        public bool blTotalCheckingResult { get; set; } //Indicate last checking total result - for all items
        public double dblTotalTactTime { get; set; } //Indicate of total tact time of last checking - for all items

        //For marking checking time
        public int intStartTick { get; set; }
        public int intFinishTick { get; set; }

        //For Total result checking
        public int intTotal_CheckCount { get; set; } //How many object had been checked
        public int intTotal_CheckPass { get; set; } //How many Object had "PASS" checking resulted
        public double dblTotal_PassRate { get; set; } //Pass rate of checking process
        public DateTime dateTimeChecking { get; set; } //For record Date & time checking


        /// <summary>
        /// The number of item checking in system
        /// </summary>
        public int intNumItem { get; set; }

        //For Master Steplist info
        public string strMasterStepListFileName { get; set; } //File Name of step list
        public string strMasterSheetName { get; set; } //Sheet name of steplist using "kikaku"

        //For steplist information
        public string strProgramListFileName { get; set; } //File Name of step list
        public string strProgramListSheetName { get; set; } //Sheet name of steplist using "kikaku"

        //For running mode
        public string strChildRunningMode { get; set; } //Parallel- singleThread - ...

        //For Group Check Mode
        public bool blGroupMode { get; set; } //Indicate whether or not user want to use Group Check Mode
        public int intGroupNum { get; set; } //Indicate how many group want to use
        public List<string> lststrGroupChildID { get; set; } //Contain info of each child process ID assign for each group

        //For user select mode
        public List<string> lststrSelectCheckingMode { get; set; }
        public string strSaveUserSelectMode { get; set; }

        //Display setting
        public int intNumRow { get; set; } //How many Row
        public int intNumCol { get; set; } //How many Column

        public int intAllignMode { get; set; } //Numbering PCB Vertically(0) or Horizionally (1)
        public int intRoundShapeMode { get; set; } //Numbering PCB zig-zag (0) or Rounding Shape (1)
        public int intOrgPosition { get; set; } //Setting for origin position of item 1: 0(Upper-left. Default). 1(Lower-Left). 2(Upper-Right). 3(lower-Right) 
        public int intNumberUserTextBox { get; set; } //How many user text box inside groupbox

        //For display label on main form
        public string strlblGroup { get; set; } // "Main PCB 1" "Main PCB 2" ...
        public List<string> lstStrTbUser { get; set; } // "QR code" "USB serial" ...
        public List<List<string>> lstlstStrTbUserContent { get; set; }

        public bool blpnlUser1Change { get; set; }
        public bool blpnlUser2Change { get; set; }
        public bool blpnlUser3Change { get; set; }


        //1: View master process program list
        //2: View Extension Info
        //3: View checking data
        //4: View Special control extension Info
        public int intProcessViewSelect { get; set; }

        //For config using TCP/IP server or not
        public bool blUsingTCPIP { get; set; }

        //***********************DATA SAVING FOR PROGRAM LIST****************************************************
        //For Data saving
        public string strDataSavePath { get; set; }

        public int intProgramListNumberUserPreInfo { get; set; }
        public List<string> lststrProgramListUserPreInfoHeader { get; set; }

        public int intProgramListNumberUserAfterInfo { get; set; }
        public List<string> lststrProgramListUserAfterInfoHeader { get; set; }

        //***********************DATA SAVING FOR ORIGIN STEP LIST*************************************************
        //For Origin Steplist using
        //For decide using PE Origin Step List or not
        public bool blUsingOriginSteplist { get; set; } //For reference to origin step list making by PE1 or PE2 - Default is YES
        public string strOriginStepListFileName { get; set; } //File Name of origin step list from PE
        public string strOriginStepListSheetName { get; set; } //Sheet name of origin steplist using "kikaku"

        //For Data saving
        public string strStepListDataSavePath { get; set; }

        public int intStepListNumberUserPreInfo { get; set; }
        public List<string> lststrStepListUserPreInfoHeader { get; set; }

        public int intStepListNumberUserAfterInfo { get; set; }
        public List<string> lststrStepListUserAfterInfoHeader { get; set; }

        //************************************************************************************************************

        //StartUp Path
        public string strStartUpPath { get; set; }
        public string strSystemIniPath { get; set; }
        public string strUserIniPath { get; set; }

        //For Child Sequence Tester
        public int intChildTesterProIDSelect { get; set; }
        public int intChildTesterStepPosSelect { get; set; }

        //For Protection Code
        public bool blMasterProcessReady { get; set; }

        //
        public clsMainVariables()
        {
            this.blMasterProcessReady = false;
        }

    }

    //For Protection Code Confirmmation
    public class clsVerifyProtectInformation
    {
        public bool blDone { get; set; } //Indicate Protect Information is already calculated or not
        //
        public string strTotalProtectCode { get; set; }
        public string strSourceProtectCode { get; set; }
        //
        public List<string> lststrModuleCode { get; set; }
        public List<string> lststrModuleName { get; set; }
        public List<string> lststrModuleLastModify { get; set; }
        //
        public string strMasterPLCode { get; set; } //Master Program List Protection Code
        public string strChildPLCode { get; set; }  //Child Process Program List Protection Code
        public string strChildSLCode { get; set; } //Child Process Step List Protection Code

        //Constructor
        public clsVerifyProtectInformation()
        {
            this.blDone = false;
            //
            this.lststrModuleCode = new List<string>();
            this.lststrModuleName = new List<string>();
            this.lststrModuleLastModify = new List<string>();
            //
            this.strTotalProtectCode = "Calculating...";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(clsChildControlModel))]
    [Export(typeof(nspCFPInfrastructures.IChildProCmdService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class clsChildControlModel: BindableBase, nspCFPInfrastructures.IChildProCmdService
    {
        //*******************************************************************************************************************
        public string strProgramVer = "1.000";
        //************************FOR MASTER PROCESS COMMUNICATION***********************************************************
        //Well, anyway, Child control process need to talking with master process control. We do it through shared service!
        //Note that we do it by "Lazy" type, to prevent crash when all part initialize at the same time!
        [Import(typeof(nspCFPInfrastructures.IMasterProCmdService))]
        Lazy<nspCFPInfrastructures.IMasterProCmdService> MasterProcessCommand;
        
        //For User Utilities support
        public List<clsUserUtility> lstclsUserUlt { get; set; }

        //for support user utilities
        public ObservableCollection<System.Windows.Controls.MenuItem> obsMenuUserUtilities;

        
        //*********************************************************
        public object UserUtlSingleFuncExecute(int PartID, List<List<object>> lstlstobjInput)
        {
            object objTempResult = new object();
            var lstlststrOutput = new List<List<object>>();

            //Get the Process ID
            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == true)
            {
                objTempResult = this.lstChildProcessModel[intProcessID].clsChildExtension.lstPluginCollection[PartID].Value.IFunctionExecute(lstlstobjInput, out lstlststrOutput);
            }
            return objTempResult;
        }

        //For extract Master Process model object
        public object objMasterProcessModel { get; set; }
        public bool blFoundMasterProcessModel { get; set; }

        public clsBindingSupport clsBindingView { get; set; }
        public int intViewID { get; set; }

        //***********************************************************************************

        public clsMainVariables clsMainVar { get; set; }
        //CHILD PROCESS MODEL
        public List<clsChildProcessModel> lstChildProcessModel { get; set; }

        //SINGLE THREAD PROCESS MODEL
        public nspSingleThreadProcessModel.clsSingleThreadProcessModel clsSingleThreadModel { get; set; }

        //Data saving
        public nspSavingData.clsSavingData clsDataSaving { get; set; }

        ////FOR Connect to center server
        //public clsSystemWatcher clsSystemWatch { get; set; }

        //For Protection Code confirmation
        public clsVerifyProtectInformation clsVerifyProtectInfo { get; set; }


        //FOR TCP/IP SERVER
        public clsTcpIpHandle clsTCPSERVER { get; set; }
        //
        public enumSystemRunningMode eChildRunningMode { get; set; }
        public int intProcessIDSelect { get; set; } //save the ID of child process selected by user
        
        //******************************* FOR BINDING - WPF**************************************
        public Dictionary<string, enumSystemRunningMode> dictChildRunMode;
        public Dictionary<string, enumChildProcessCheckingMode> dictChildCheckMode;

        //Create View Table - display in main region area
        private DataTable ViewTable;

        //**********************************************************
        public string GetProtectionCode()
        {
            string strRet = "";
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            strRet = this.GetSingleFileInfo(assembly.Location);
            //
            return strRet;
        }
        public string GetSingleFileInfo(string strFilePath)
        {
            string strRet = "";
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(strFilePath);
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            string strHash = new FileHasher.HashCalculator(strFilePath).CalculateFileHash();
            strRet = strHash;
            //
            return strRet;
        }
        public string GetAllModulesProtectInfo()
        {
            string strRet = "";
            //
            int i = 0;
            int intNumUserModule = this.lstChildProcessModel[0].clsChildExtension.lststrLoadFiles.Count;


            for (i = 0; i < intNumUserModule; i++)
            {
                string strFilePath = this.lstChildProcessModel[0].clsChildExtension.lststrLoadFiles[i];
                string strFileName = Path.GetFileName(strFilePath);
                string strFileProtectCode = this.GetSingleFileInfo(strFilePath);
                //
                string strTempInfo = "*****\r\n";
                strTempInfo += strFileName + "\r\n";
                strTempInfo += strFileProtectCode + "\r\n";

                //
                strRet += strTempInfo;
            }

            //
            return strRet;
        }
        public string GetTotalProtectCode(bool blIniRun = false)
        {
            //
            if(this.clsVerifyProtectInfo==null)
            {
                //this.clsVerifyProtectInfo = new clsVerifyProtectInformation();
                return "Error: Ini Calculation not yet finish!";
            }

            if ((blIniRun ==false) && (this.clsVerifyProtectInfo.blDone == false))
            {
                MessageBox.Show("Error: Ini Calculation not yet finish! Please wait a moment...", "GetTotalProtectCode()");
                return "Error: Ini Calculation not yet finish!";
            }

            if(this.clsVerifyProtectInfo.blDone==true) //Only allow 1 time running
            {
                return "0";
            }

            //
            try
            {
                string strResult = "";
                string strTemp = "";
                List<string> lststrTemp = new List<string>();

                //1. Get Frame Protect code
                // strTemp = this.GetFrameProtectCode().ToString();
                strTemp = nspAppStore.clsAppStore.GetCurrentState().shellFrameInfo.ProtectionCode;


                lststrTemp.Add(strTemp);

                //2. Get all modules Protect code
                int i, j = 0;
                int intNumUserModule = this.lstChildProcessModel[0].clsChildExtension.lststrLoadFiles.Count;

                for (i = 0; i < intNumUserModule; i++)
                {
                    string strFilePath = this.lstChildProcessModel[0].clsChildExtension.lststrLoadFiles[i];
                    string strFileName = Path.GetFileName(strFilePath);

                    //if (strFileName.ToUpper() == "nspSpecCtrlCSharpSimul.dll".ToUpper())
                    //{
                    //    ;
                    //}

                    string strFileProtectCode = this.GetSingleFileInfo(strFilePath);
                    string strFileLastModify = File.GetLastWriteTime(strFilePath).ToString();
                    //
                    lststrTemp.Add(strFileProtectCode);
                    this.clsVerifyProtectInfo.lststrModuleCode.Add(strFileProtectCode);
                    this.clsVerifyProtectInfo.lststrModuleName.Add(strFileName);
                    this.clsVerifyProtectInfo.lststrModuleLastModify.Add(strFileLastModify);
                }

                //Now, cal all result
                strResult = "";
                decimal dcTemp = 0;
                decimal dcResult1 = 0;
                //
                for (i = 0; i < lststrTemp.Count; i++)
                {
                    dcTemp = 0;
                    //
                    for (j = 0; j < lststrTemp[i].Length; j++)
                    {
                        dcTemp += (decimal)((double)lststrTemp[i][j] * Math.Sqrt(j + 1));
                    }
                    //
                    dcResult1 += dcTemp * (decimal)Math.Sqrt(i + 1);
                }


                //
                lststrTemp.Reverse();
                decimal dcResult2 = 0;
                for (i = 0; i < lststrTemp.Count; i++)
                {
                    dcTemp = 0;
                    //
                    for (j = 0; j < lststrTemp[i].Length; j++)
                    {
                        dcTemp += (decimal)((double)lststrTemp[i][j] * Math.Sqrt(j + 1));
                    }
                    //
                    dcResult2 += dcTemp * (decimal)Math.Sqrt(i + 1);
                }

                //Combine 2 result
                decimal dcToTal = dcResult1 + dcResult2;

                //Cal Source Code ID
                string strTotal = dcToTal.ToString();
                if (strTotal.Length > 8)
                {
                    this.clsVerifyProtectInfo.strSourceProtectCode = strTotal.Substring(strTotal.Length - 8, 8);
                }
                else
                {
                    this.clsVerifyProtectInfo.strSourceProtectCode = strTotal;
                }


                //Reset for calculate total protect code
                lststrTemp = new List<string>();
                lststrTemp.Add(strTotal);
                //Add Master Program List protect code
                string strMasterPLCode = this.GetMasterProgramListProtectCode().ToString();
                lststrTemp.Add(strMasterPLCode);

                //Add Child Program List Protect code
                string strChildPLCode = this.lstChildProcessModel[0].clsProgramList.GetProtectCode();
                lststrTemp.Add(strChildPLCode);

                //Add Child Step List if using
                if (this.clsMainVar.blUsingOriginSteplist == true)
                {
                    string strChildSLCode = this.lstChildProcessModel[0].clsStepList.GetProtectCode();
                    lststrTemp.Add(strChildSLCode);
                }


                //CALCULATE TOTAL CODE
                dcTemp = 0;
                dcResult1 = 0;
                //
                for (i = 0; i < lststrTemp.Count; i++)
                {
                    dcTemp = 0;
                    //
                    for (j = 0; j < lststrTemp[i].Length; j++)
                    {
                        dcTemp += (decimal)((double)lststrTemp[i][j] * Math.Sqrt(j + 1));
                    }
                    //
                    dcResult1 += dcTemp * (decimal)Math.Sqrt(i + 1);
                }


                //
                lststrTemp.Reverse();
                dcResult2 = 0;
                for (i = 0; i < lststrTemp.Count; i++)
                {
                    dcTemp = 0;
                    //
                    for (j = 0; j < lststrTemp[i].Length; j++)
                    {
                        dcTemp += (decimal)((double)lststrTemp[i][j] * Math.Sqrt(j + 1));
                    }
                    //
                    dcResult2 += dcTemp * (decimal)Math.Sqrt(i + 1);
                }

                //Combine 2 result
                dcToTal = dcResult1 + dcResult2;

                //Cal Source Code ID
                strTotal = dcToTal.ToString();
                if (strTotal.Length > 8)
                {
                    strResult = strTotal.Substring(strTotal.Length - 8, 8);
                }
                else
                {
                    strResult = strTotal;
                }

                //
                this.clsVerifyProtectInfo.strTotalProtectCode = strResult;
                this.clsVerifyProtectInfo.blDone = true;

                //Record Protect Code to System Ini file for confirming
                MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "PROTECT_CODE", "ProtectCode", this.clsVerifyProtectInfo.strTotalProtectCode);
                MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "PROTECT_CODE", "SourceCode", this.clsVerifyProtectInfo.strSourceProtectCode);

            }
            catch(Exception ex)
            {
                return "Error: Unexpected error happen. Error message: " + ex.Message;
            }

            //Return 0 if everything is OK
            return "0";
        }
        public async void UpdateSystemProtectCodeInfo()
        {
            await GetProtectCodeInfo();
        }
        public Task GetProtectCodeInfo()
        {
            return Task.Run(() =>
            {
                //Wait until child control class finish initialization
                while (this.clsVerifyProtectInfo==null)
                {
                    ;
                }

                //Clear old data in ini file
                MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "PROTECT_CODE", "ProtectCode", "Calculating...");
                MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "PROTECT_CODE", "SourceCode", "Calculating...");

                //
                string strRet = "";
                int intRetryTimes = 0;
                do
                {
                    strRet = this.GetTotalProtectCode(true);

                    //
                    if(strRet!="0") intRetryTimes++;
                    if (intRetryTimes > 5) break;

                } while (strRet != "0");
                
                //Update on UI
                if (strRet=="0")
                {
                    this.CalChildHeaderSysInfo();
                }
                else
                {
                    MessageBox.Show(strRet, "GetProtectCodeInfo() Ini fail!");
                }
            });
        }

        //****************************************************************************************************************************
        //Child control command shared service
        public object ChildControlCommand(List<List<object>> lstlstobjCommand, out List<List<object>> lstlstobjReturn)
        {
            lstlstobjReturn = new List<List<object>>();
            object objRet = new object();

            int i = 0;
            for (i = 0; i < lstlstobjCommand.Count; i++)
            {
                if (lstlstobjCommand[i].Count == 0) continue;

                if (lstlstobjCommand[i][0].ToString() == "GetObject")
                {
                    if (lstlstobjCommand[i].Count < 2) continue;
                    if (lstlstobjCommand[i][1].ToString() == "clsChildControlModel")
                    {
                        objRet = this;
                    }
                }
                else if (lstlstobjCommand[i][0].ToString() == "MASTERREADY")
                {
                    this.clsMainVar.blMasterProcessReady = true;
                }
                else if (lstlstobjCommand[i][0].ToString() == "START")
                {
                    objRet = IniChildProcess();
                }
                else if (lstlstobjCommand[i][0].ToString() == "SHUTDOWN")
                {
                    objRet = ShutdownChildProcess();
                }
                else if (lstlstobjCommand[i][0].ToString() == "USERMENU") //Add user menu item to Main menu
                {
                    if (lstlstobjCommand[i].Count < 2) continue;
                    this.AddUserMenuItem(lstlstobjCommand[i][1]);
                }
            }

            return objRet;
        }

        public object GetMasterProcessObject()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GetObject");
            lstTemp.Add("clsMasterProcessModel");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //Marking
            this.blFoundMasterProcessModel = true;
            this.objMasterProcessModel = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);

            return this.objMasterProcessModel;
        }

        public DataTable GetMasterProgramListDataTable()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GetMasterProgramListDataTable");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);

            DataTable dbRet = new DataTable();
            if (objRet is DataTable)
            {
                dbRet = (DataTable)objRet;
            }

            return dbRet;
        }
        public string GetMasterPLVersion()
        {
            //
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GetMasterPLVersion");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();
            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet.ToString();
        }
        public object GetMasterVersionInfo()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GETINFO");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object RequestMasterSequenceTester()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("SEQUENCETESTER");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object RequestMasterSequenceCheckingMode(string strInput)
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("CHECKINGMODE");
            lstTemp.Add(strInput);
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object RequestMasterSystemCommandAnswer(string strUserRequest)
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("SystemCommandAnswer");
            lstTemp.Add(strUserRequest);
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object RequestMasterChangeProgramList()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("ChangeProgramList");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object GetMasterProtectCode()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GETPROTECTCODE");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();
            //
            while(this.clsMainVar.blMasterProcessReady == false)
            {
                ;
            }
            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public object GetMasterProgramListProtectCode()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("PROGRAMLISTCODE");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();
            //
            while (this.clsMainVar.blMasterProcessReady == false)
            {
                ;
            }
            //
            object objRet = MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }
        public string GetFrameVersionInfo()
        {
            string strRet = "";
            var state = nspAppStore.clsAppStore.GetCurrentState();

            strRet = "Tool Version: " + state.shellFrameInfo.Version + "\r\n" +
                      "ProtectCode: " + state.shellFrameInfo.ProtectionCode + "\r\n" +
                      "Date Created: " + state.shellFrameInfo.DateCreated + "\r\n";

            return strRet;
        }

        //****************************************************************************************************************************
        public DataTable GetTableView()
        {
            return ViewTable;
        }
        public void ViewTableIni()
        {
            ViewTable = new System.Data.DataTable();

            //Create column for data table
            ViewTable.Columns.Add("GroupStep"); //0

            ViewTable.Columns.Add("Number"); //1
            ViewTable.Columns.Add("TestName"); //2
            ViewTable.Columns.Add("Result"); //3
            ViewTable.Columns.Add("LoLimit"); //4
            ViewTable.Columns.Add("HiLimit"); //5
            ViewTable.Columns.Add("Unit"); //6
            ViewTable.Columns.Add("FuncID"); //7
            ViewTable.Columns.Add("Comment"); //8
            ViewTable.Columns.Add("Notes"); //9

            ViewTable.Columns.Add("ActiveColor", typeof(System.Windows.Media.SolidColorBrush)); //10
            ViewTable.Columns.Add("ResultColor", typeof(System.Windows.Media.SolidColorBrush));//11

            int i = 0;
            for (i = 0; i < this.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                DataRow temp = ViewTable.NewRow();

                //Checking step is blank or user function name row
                if (this.lstChildProcessModel[0].lstTotalStep[i].intStepSequenceID==-1) //No Caring
                {
                    if((i+1)<this.lstChildProcessModel[0].lstTotalStep.Count)
                    {
                        if(this.lstChildProcessModel[0].lstTotalStep[i+1].intStepSequenceID==1) //User Function
                        {
                            temp[0] = "FUNC";
                            temp[1] = this.lstChildProcessModel[0].lstTotalStep[i+1].strUserFunctionName;
                        }
                    }

                    ViewTable.Rows.Add(temp);
                    continue;
                }

                //temp[0] = this.lstChildProcessModel[0].lstChildTotal[i].strOriginStepNumber;
                temp[0] = this.lstChildProcessModel[0].lstTotalStep[i].strGroupData;

                temp[1] = this.lstChildProcessModel[0].lstTotalStep[i].intStepNumber.ToString();
                temp[2] = this.lstChildProcessModel[0].lstTotalStep[i].strStepName;
                temp[3] = "";

                if(this.lstChildProcessModel[0].lstTotalStep[i].strUnitName.ToUpper().Trim()=="H")
                {
                    temp[4] = Convert.ToInt32(this.lstChildProcessModel[0].lstTotalStep[i].objLoLimit).ToString("X");
                    temp[5] = Convert.ToInt32(this.lstChildProcessModel[0].lstTotalStep[i].objHiLimit).ToString("X");
                }
                else
                {
                    temp[4] = this.lstChildProcessModel[0].lstTotalStep[i].objLoLimit.ToString();
                    temp[5] = this.lstChildProcessModel[0].lstTotalStep[i].objHiLimit.ToString();
                }

                temp[6] = this.lstChildProcessModel[0].lstTotalStep[i].strUnitName;
                temp[7] = this.lstChildProcessModel[0].lstTotalStep[i].intStepClass.ToString() + "-" +
                            this.lstChildProcessModel[0].lstTotalStep[i].intJigId.ToString() + "-" +
                            this.lstChildProcessModel[0].lstTotalStep[i].intHardwareId.ToString() + "-" +
                            this.lstChildProcessModel[0].lstTotalStep[i].intFunctionId.ToString();

                //temp[9] = "0";

                ViewTable.Rows.Add(temp);
            }

            //Now, set all child process using same view table
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                this.lstChildProcessModel[i].ViewTable = this.ViewTable;
            }
            //Also set for single thread process
            this.clsSingleThreadModel.ViewTable = this.ViewTable;

            //Select default view is child process 0
            this.lstChildProcessModel[0].blAllowUpdateViewTable = true;
        }
        //Create Step List View Table - display in main region area
        private DataTable StepListViewTable;
        public DataTable GetStepListTableView()
        {
            return StepListViewTable;
        }
        public void ViewStepListTableIni()
        {
            if (this.lstChildProcessModel[0].clsChildSetting.blUsingOriginSteplist == false) return;

            //
            StepListViewTable = new System.Data.DataTable();

            //Create column for data table
            StepListViewTable.Columns.Add("GroupStep"); //0 - No use

            StepListViewTable.Columns.Add("Number"); //1
            StepListViewTable.Columns.Add("TestName"); //2
            StepListViewTable.Columns.Add("Result"); //3
            StepListViewTable.Columns.Add("LoLimit"); //4
            StepListViewTable.Columns.Add("HiLimit"); //5
            StepListViewTable.Columns.Add("Unit"); //6
            StepListViewTable.Columns.Add("FuncID"); //7 - No use
            StepListViewTable.Columns.Add("Comment"); //8
            StepListViewTable.Columns.Add("Notes"); //9

            StepListViewTable.Columns.Add("ActiveColor", typeof(System.Windows.Media.SolidColorBrush)); //10
            StepListViewTable.Columns.Add("ResultColor", typeof(System.Windows.Media.SolidColorBrush));//11

            int i = 0;
            for (i = 0; i < this.lstChildProcessModel[0].clsStepList.lstExcelList.Count; i++)
            {
                DataRow temp = StepListViewTable.NewRow();

                //Checking step is blank or user function name row
                temp[0] = "";

                temp[1] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].intStepNumber.ToString();
                temp[2] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].strStepName;
                temp[3] = "";

                if (this.lstChildProcessModel[0].clsStepList.lstExcelList[i].strUnitName.ToUpper().Trim() == "H")
                {
                    temp[4] = Convert.ToInt32(this.lstChildProcessModel[0].clsStepList.lstExcelList[i].objLoLimit).ToString("X");
                    temp[5] = Convert.ToInt32(this.lstChildProcessModel[0].clsStepList.lstExcelList[i].objHiLimit).ToString("X");
                }
                else
                {
                    temp[4] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].objLoLimit.ToString();
                    temp[5] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].objHiLimit.ToString();
                }

                temp[6] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].strUnitName;
                temp[8] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].strComment;
                temp[9] = this.lstChildProcessModel[0].clsStepList.lstExcelList[i].strNotes;
                StepListViewTable.Rows.Add(temp);
            }

            //Set all Item ID using same view table
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                for(int j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].StepListViewTable = this.StepListViewTable;
                }
            }

            //Also set for single thread process
            //this.clsSingleThreadModel.StepListViewTable = this.StepListViewTable;

            //Select default view is child process 0 - item 0
            this.lstChildProcessModel[0].lstclsItemCheckInfo[0].blAllowUpdateStepListViewTable = true;
        }
        //For display in running mode select combobox 
        public List<string> GetRunModeList()
        {
            List<string> lststrRet = new List<string>();
            foreach (var search in this.dictChildRunMode)
            {
                lststrRet.Add(search.Key);
            }
            return lststrRet;
        }
        //For display in checking mode select combobox 
        public List<string> GetCheckModeList()
        {
            List<string> lststrRet = new List<string>();
            foreach (var search in this.dictChildCheckMode)
            {
                lststrRet.Add(search.Key);
            }
            return lststrRet;
        }
        //For display in select item combobox 
        public List<string> GetChildItemList()
        {
            List<string> lststrRet = new List<string>();

            for (int i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                lststrRet.Add((i + 1).ToString());//count from 1
            }

            //Adding SingleThread view item
            lststrRet.Add("SingleThread");

            return lststrRet;
        }
        //******************************************For Step List View**************************************
        public List<string> GetItemCheckList()
        {
            List<string> lststrRet = new List<string>();
            int i, j = 0;
            for(i=0; i<this.lstChildProcessModel.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    lststrRet.Add((this.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID+1).ToString());
                }
            }
            return lststrRet;
        }
        public void SelectStepListViewItem(int intItemID)
        {
            int i, j = 0;
            for(i=0;i<this.lstChildProcessModel.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    if(this.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID==intItemID)
                    {
                        this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blAllowUpdateStepListViewTable = true;
                        //Refresh for step list view
                        this.lstChildProcessModel[i].RefreshStepListViewTable(j);
                    }
                    else
                    {
                        this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blAllowUpdateStepListViewTable = false;
                    }
                }
            }
        }
        //******************************************FOR CHILD SEQUENCE TESTER*******************************
        //Child Sequence Tester - For display in select item combobox 
        public List<string> GetChildProcessIDList
        {
            get
            {
                List<string> lststrRet = new List<string>();
                //
                for (int i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    lststrRet.Add((i + 1).ToString());//count from 1
                }
                //
                return lststrRet;
            }
            set
            {
                this.GetChildProcessIDList = value;
            }
        }
        //
        private string _TheChildIDSelectedItem;
        public string TheChildIDSelectedItem
        {
            get { return _TheChildIDSelectedItem; }
            set
            {
                _TheChildIDSelectedItem = value;
                //Analyze & display selected step name
                int intChildProID = 0;
                if (int.TryParse(this._TheChildIDSelectedItem, out intChildProID) == false) return;
                if ((this.lstChildProcessModel.Count < intChildProID) || (intChildProID<1)) return;
                this.clsMainVar.intChildTesterProIDSelect = intChildProID - 1;
                //
                OnPropertyChanged("TheChildIDSelectedItem");
            }
        }

        //GetChildProgStep
        public List<string> GetChildProgStep
        {
            get
            {
                List<string> lststrRet = new List<string>();
                //
                for (int i = 0; i < this.lstChildProcessModel[0].clsProgramList.lstProgramList.Count; i++)
                {
                    lststrRet.Add(this.lstChildProcessModel[0].clsProgramList.lstProgramList[i].intStepNumber.ToString());
                }
                //
                return lststrRet;
            }
            set
            {
                this.GetChildProgStep = value;
            }
        }

        //TheChildStepSelectedItem
        private string _TheChildStepSelectedItem;
        public string TheChildStepSelectedItem
        {
            get { return _TheChildStepSelectedItem; }
            set
            {
                _TheChildStepSelectedItem = value;
                //Analyze & display selected step name
                int intStepNum = 0;
                if (int.TryParse(this._TheChildStepSelectedItem, out intStepNum) == false) return;

                //
                int intStepPos = this.lstChildProcessModel[0].FindCheckingStepPos(intStepNum);
                if (intStepPos == -1) return;

                //Marking
                this.clsMainVar.intChildTesterStepPosSelect = intStepPos;

                //Display selected step name
                this.strSelectedStepName = this.lstChildProcessModel[0].lstTotalStep[intStepPos].strStepName;

                //
                OnPropertyChanged("strSelectedStepName");
            }
        }

        //
        private string _strSelectedStepName;
        public string strSelectedStepName
        {
            get { return _strSelectedStepName; }
            set
            {
                _strSelectedStepName = value;
            }
        }

        //
        private string _strNewValue;
        public string strNewValue
        {
            get { return _strNewValue; }
            set
            {
                _strNewValue = value;
                //Setting for Child Process model forcing selected step with desired value
                this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstblForceChange[this.clsMainVar.intChildTesterStepPosSelect] = true;
                this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstobjNewValue[this.clsMainVar.intChildTesterStepPosSelect] = this._strNewValue;
            }
        }
        //
        public ICommand ICommandChildTesterApply { get; set; }
        public void ICommandChildTesterApplyHandle()
        {
            //
            object objTemp = this._strNewValue;
            //
            this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].blActiveTestingSequence = true;
            this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstblForceChange[this.clsMainVar.intChildTesterStepPosSelect] = true;
            this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstobjNewValue[this.clsMainVar.intChildTesterStepPosSelect] = objTemp;
        }
        //
        public ICommand ICommandChildTesterReset { get; set; }
        public void ICommandChildTesterResetHandle()
        {
            //
            object objTemp = new object();
            //

            this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstblForceChange[this.clsMainVar.intChildTesterStepPosSelect] = false;
            this.lstChildProcessModel[this.clsMainVar.intChildTesterProIDSelect].clsSeqenceTestData.lstobjNewValue[this.clsMainVar.intChildTesterStepPosSelect] = objTemp;
        }
        //
        public ICommand ICommandChildTesterResetAll { get; set; }
        public void ICommandChildTesterResetAllHandle()
        {
            int i, j = 0;
            //
            for(i=0;i<this.lstChildProcessModel.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel[i].lstTotalStep.Count;j++)
                {
                    this.lstChildProcessModel[i].clsSeqenceTestData.lstblForceChange[j] = false;
                    this.lstChildProcessModel[i].clsSeqenceTestData.lstobjNewValue[j] = new object();
                }
            }
        }

        //**************************************************************************************************************
        //Handle user select child process
        public void SetChildProcessView(int intProcessID)
        {
            //Set View Table again
            for (int i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                if(i==intProcessID) //Matching
                {
                    this.lstChildProcessModel[i].blAllowUpdateViewTable = true;
                    //Update current status of process
                    this.lstChildProcessModel[i].RefreshViewTable();
                }
                else //Not match
                {
                    this.lstChildProcessModel[i].blAllowUpdateViewTable = false;
                }
            }
        }
        public void SetSingleThreadView()
        {
            //Set View Table again
            for (int i = 0; i < this.lstChildProcessModel.Count; i++)
            {
               this.lstChildProcessModel[i].blAllowUpdateViewTable = false;
            }
            //Set view right to singlethread process
            this.clsSingleThreadModel.blAllowUpdateViewTable = true;
            //Update current status of singlethread process
            this.clsSingleThreadModel.RefreshViewTable();
        }
        //Handle user select running mode: Parallel - single thread - single process - independent 
        public void SetChildRunMode(string strMode)
        {
            if (this.dictChildRunMode.ContainsKey(strMode))
            {
                this.eChildRunningMode = this.dictChildRunMode[strMode];

                //Passing for all Child Process
                for(int i=0;i<this.lstChildProcessModel.Count;i++)
                {
                    this.lstChildProcessModel[i].eChildRunningMode = this.eChildRunningMode;
                    
                    //If select Independent mode, then start independent polling start timer. If not, disable it
                    if(this.eChildRunningMode== enumSystemRunningMode.IndependentMode)
                    {
                        this.lstChildProcessModel[i].tmrIndependent.Enabled = true;
                        this.lstChildProcessModel[i].tmrIndependent.Start();
                    }
                    else
                    {
                        this.lstChildProcessModel[i].tmrIndependent.Enabled = false;
                        this.lstChildProcessModel[i].tmrIndependent.Stop();
                    }

                }

                //saving selection to ini file
                long lngret = 0;
                lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "CONTROL_MODE", "ControlProcessMode", strMode);

                this.clsMainVar.strChildRunningMode = strMode;

                CalChildHeaderInfo();
            }
        }
        //Handle user select checking mode for child process: Normal - single - step - fail - all
        public void SetChildCheckMode(string strMode)
        {
            int i = 0;
            int intStepModeSelectPos = 0;
            if (this.dictChildCheckMode.ContainsKey(strMode))
            {
                //Handle for Step Mode: User must input the desired step number want to stop
                if (strMode.ToUpper() == "STEP")
                {
                    bool blExit = false;
                    string strStepNum = "";
                    DialogResult dlgTemp;

                    while (blExit == false)
                    {
                        dlgTemp = MyLibrary.clsMyFunc.InputBox("Input Step Number want to stop at", "", ref strStepNum);
                        if (dlgTemp == DialogResult.OK)
                        {
                            //Check if user input is integer or not
                            int intTemp = 0;
                            if (int.TryParse(strStepNum, out intTemp) == true)
                            {
                                //string strRet = SetStepModeNumber(intTemp);
                                intStepModeSelectPos = this.lstChildProcessModel[0].FindCheckingStepPos(intTemp);
                                if (intStepModeSelectPos == -1)
                                {
                                    MessageBox.Show("Error: cannot find step " + strStepNum, "Input Error");
                                }
                                else
                                {
                                    blExit = true;
                                    break;
                                }
                            }
                            else
                            {
                                MessageBox.Show("The number you input is not integer! Please input again!", "Input Error");
                            }

                        }
                        else if (dlgTemp == DialogResult.Cancel)
                        {
                            blExit = true;
                            return; //Exit function
                        }
                    }

                } //End handle step mode



                //if(this.eChildRunningMode == enumSystemRunningMode.SingleThreadMode)
                //{
                //    this.clsSingleThreadModel.SetCheckMode(this.dictChildCheckMode[strMode]);
                //}
                //else
                //{
                //    this.lstChildProcessModel[this.intProcessIDSelect].SetCheckMode(this.dictChildCheckMode[strMode]);
                //}

                switch(this.eChildRunningMode)
                {
                    case enumSystemRunningMode.SingleProcessMode:
                        this.lstChildProcessModel[this.intProcessIDSelect].SetCheckMode(this.dictChildCheckMode[strMode]);
                        if (strMode.ToUpper() == "STEP")
                        {
                            this.lstChildProcessModel[this.intProcessIDSelect].intStepModePosSelected = intStepModeSelectPos;
                        }
                        break;

                    case enumSystemRunningMode.SingleThreadMode:
                        this.clsSingleThreadModel.SetCheckMode(this.dictChildCheckMode[strMode]);
                        if (strMode.ToUpper() == "STEP")
                        {
                            this.clsSingleThreadModel.intStepModePosSelected = intStepModeSelectPos;
                        }
                        break;

                    default: //Parallel mode, Independent Mode & Other mode => set for all!
                        for (i = 0; i < this.lstChildProcessModel.Count; i++)
                        {
                            this.lstChildProcessModel[i].SetCheckMode(this.dictChildCheckMode[strMode]);
                            if (strMode.ToUpper() == "STEP")
                            {
                                this.lstChildProcessModel[i].intStepModePosSelected = intStepModeSelectPos;
                            }
                        }
                        break;
                }

                //
            }
        }
        //Handle user select checking mode: Normal Mode - Service Mode - PM Mode
        public void SetUserSelectCheckingMode(string strInput)
        {
            //Request Password First
            string strUserInput = "";
            System.Windows.Forms.DialogResult dlgTemp;
            bool blAllowChange = false;

            while (true)
            {
                dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Please input password first:", ref strUserInput);
                if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                {
                    //Confirm password
                    if (strUserInput.ToUpper().Trim() == "PED")
                    {
                        blAllowChange = true;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Password Wrong! Please input again or cancel!", "PASSWORD WRONG");
                    }
                }
                else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                {
                    //MessageBox.Show("Cancel selected!", "PASSWORD WRONG");
                    break;
                }
            }
            if (blAllowChange == false) return;

            // Change checking mode if password is OK.
            this.SetCheckingMode(strInput);
        }

        public void SetCheckingMode(string checkingMode)
        {
            //
            int i = 0;
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                this.lstChildProcessModel[i].strSystemCheckingMode = checkingMode;
            }

            //Inform to Master Process also
            object objTemp = this.RequestMasterSequenceCheckingMode(checkingMode);

            //Saving choice to ini file
            long lngret = 0;
            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "DISPLAY_SETTING", "SaveUserSelectMode", checkingMode);

            //Calculate again Child Header Info
            CalChildHeaderInfo();
        }



        //Handle Next button pressed
        public void btnNextCommandHandle()
        {
            int i = 0;
            switch (this.eChildRunningMode)
            {
                case enumSystemRunningMode.ParallelMode:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blAllowContinueRunning = true;
                    }
                    break;

                case enumSystemRunningMode.SingleThreadMode:
                    this.clsSingleThreadModel.blAllowContinueRunning = true;
                    for (i = 0; i < this.lstChildProcessModel.Count;i++)
                    {
                        this.lstChildProcessModel[i].blAllowContinueRunning = true;
                    }
                    break;

                case enumSystemRunningMode.SingleProcessMode:
                    this.lstChildProcessModel[this.intProcessIDSelect].blAllowContinueRunning = true;
                    break;

                case enumSystemRunningMode.IndependentMode:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blAllowContinueRunning = true;
                    }
                    break;

                default:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blAllowContinueRunning = true;
                    }
                    break;
            }
        }

        //Handle End button pressed
        public void btnEndCommandHandle()
        {
            int i = 0;

            switch (this.eChildRunningMode)
            {
                case enumSystemRunningMode.ParallelMode:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blRequestStopRunning = true;
                        this.lstChildProcessModel[i].CancelTaskChecking();
                    }
                    break;

                case enumSystemRunningMode.SingleThreadMode:
                    this.clsSingleThreadModel.blRequestStopRunning = true;
                    this.clsSingleThreadModel.CancelTaskThreadChecking();
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blRequestStopRunning = true;
                    }
                    break;

                case enumSystemRunningMode.SingleProcessMode:
                    this.lstChildProcessModel[this.intProcessIDSelect].blRequestStopRunning = true;
                    this.lstChildProcessModel[this.intProcessIDSelect].CancelTaskChecking();
                    break;

                case enumSystemRunningMode.IndependentMode:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blRequestStopRunning = true;
                        this.lstChildProcessModel[i].CancelTaskChecking();
                    }
                    break;

                default:
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.lstChildProcessModel[i].blRequestStopRunning = true;
                        this.lstChildProcessModel[i].CancelTaskChecking();
                    }
                    break;
            }
        }     
        //For display info & result in header region
        public void CalHeaderInfo()
        {
            string strRet = "";

            //Add Info of program list
            //strRet = strRet + "Program List: " + this.lstChildProcessModel[0].clsChildProgList.strStepListname + "\r\n";
            //strRet = strRet + "Version: " + this.lstChildProcessModel[0].clsChildProgList.strStepListVersion + "\r\n";

            //Add Info of checking mode
            strRet = strRet + "Run Mode: " + this.clsMainVar.strChildRunningMode + "\r\n";

            //Add Info of checking mode
            strRet = strRet + "Checking Mode: " + this.lstChildProcessModel[0].strSystemCheckingMode + "\r\n";

            //Add Info of Group Mode
            strRet = strRet + "Group Mode: " + this.clsMainVar.blGroupMode.ToString() + "\r\n";

            //Add Info of last result checking
            strRet = strRet + "Pass rate: " + this.clsMainVar.dblTotal_PassRate.ToString() + " %"
                + " (" + this.clsMainVar.intTotal_CheckPass.ToString() + "/"
                + this.clsMainVar.intTotal_CheckCount.ToString() + ")" + "\r\n";

            strRet = strRet + "Tact time: " + this.clsMainVar.dblTotalTactTime.ToString() + " s" + "\r\n";

            //Total result
            if(this.clsMainVar.blTotalCheckingResult==true)
            {
                this.clsBindingView.strTotalResult = "PASS";
            }
            else
            {
                this.clsBindingView.strTotalResult = "FAIL";
            }

            //Update and raise property change
            this.clsBindingView.strChildHeaderInfo = strRet;

            OnPropertyChanged("clsBindingView");
        }

        public void CalChildHeaderInfo()
        {
            string strRet = "";

            //Add Info of program list
            //strRet = strRet + "Program List: " + this.lstChildProcessModel[0].clsChildProgList.strStepListname + "\r\n";
            //strRet = strRet + "Version: " + this.lstChildProcessModel[0].clsChildProgList.strStepListVersion + "\r\n";

            //Add Info of checking mode
            strRet = strRet + "Run Mode: " + this.clsMainVar.strChildRunningMode + "\r\n";

            //Add Info of checking mode
            strRet = strRet + "Checking Mode: " + this.lstChildProcessModel[0].strSystemCheckingMode + "\r\n";

            //Add Info of Group Mode
            strRet = strRet + "Group Mode: " + this.clsMainVar.blGroupMode.ToString() + "\r\n";

            //Add Info of last result checking
            strRet = strRet + "Pass rate: " + this.clsMainVar.dblTotal_PassRate.ToString() + " %"
                + " (" + this.clsMainVar.intTotal_CheckPass.ToString() + "/"
                + this.clsMainVar.intTotal_CheckCount.ToString() + ")" + "\r\n";

            strRet = strRet + "Tact time: " + this.clsMainVar.dblTotalTactTime.ToString() + " s" + "\r\n";

            //Update and raise property change
            this.clsBindingView.strChildHeaderInfo = strRet;

            OnPropertyChanged("clsBindingView");
        }

        //User Header Info
        private List<string> lststrUserHeaderInfo { get; set; }
        public void CalChildHeaderSysInfo()
        {

            if (this.clsBindingView == null) return;
            //
            string strRet = "";

            //System Protection Code
            if (this.clsVerifyProtectInfo!=null)
            {
                strRet += "Protect Code: " + this.clsVerifyProtectInfo.strTotalProtectCode + "-" + this.clsVerifyProtectInfo.strSourceProtectCode + "\r\n";
                //strRet += "Source Code: " + this.clsVerifyProtectInfo.strSourceProtectCode + "\r\n";
            }
            else
            {
                strRet += "Protect Code: Calculating...\r\n";
                //strRet += "Source Code: Calculating...\r\n";
            }
            
            //Add Info of program list & Step list
            if(this.clsMainVar.blUsingOriginSteplist == true)
            {
                strRet = strRet + this.lstChildProcessModel[0].clsProgramList.strProgramListname + "\r\n";
                strRet = strRet + "Program Version: " + this.lstChildProcessModel[0].clsProgramList.strProgramListVersion + "\r\n";
                strRet = strRet + "Step List: " + this.lstChildProcessModel[0].clsStepList.strStepListVersion + "\r\n";
            }
            else
            {
                strRet = strRet + this.lstChildProcessModel[0].clsProgramList.strProgramListname + "\r\n";
                strRet = strRet + "Program Version: " + this.lstChildProcessModel[0].clsProgramList.strProgramListVersion + "\r\n";
                //strRet = strRet + "Step List: " + this.lstChildProcessModel[0].clsChildProgList.strProgramListVersion + "\r\n";
            }


            //Add user header info
            if (this.lststrUserHeaderInfo != null)
            {
                for (int i = 0; i < this.lststrUserHeaderInfo.Count; i++)
                {
                    strRet = strRet + this.lststrUserHeaderInfo[i];
                    if (i != (this.lststrUserHeaderInfo.Count - 1))
                    {
                        strRet = strRet + "\r\n";
                    }
                }
            }

            //Update and raise property change
            this.clsBindingView.strChildHeaderSysInfo = strRet;

            OnPropertyChanged("clsBindingView");
        }
        public void UpdateUserHeaderInfo(int intNumRow, string strInfoTitle, string strInfoContent)
        {
            int intExistLine = this.lststrUserHeaderInfo.Count;

            if(intNumRow>(intExistLine-1)) //need add new line
            {
                this.lststrUserHeaderInfo.Add(strInfoTitle + ": " + strInfoContent);
            }
            else //Modify exist line
            {
                this.lststrUserHeaderInfo[intNumRow] = strInfoTitle + ": " + strInfoContent;
            }
            //cal again & display
            CalChildHeaderSysInfo();
        }

        //****************************END FOR BINDING - WPF**************************************

        //**********************************************************
        public void ReadSystemIniFile()
        {
            string strTemp = "";
            int intTemp = 0;

            //1. Check if file is exist or not
            string strAppPath = "";
            string iniSystemFileName = @"\SystemIni.ini";
            string iniUserFileName = @"\UserIni.ini";
            string strSystemIniFilePath = "";

            strAppPath = Application.StartupPath;
            strSystemIniFilePath = strAppPath + iniSystemFileName;


            clsMainVar.strStartUpPath = strAppPath;
            clsMainVar.strSystemIniPath = strSystemIniFilePath;
            clsMainVar.strUserIniPath = strAppPath + iniUserFileName;

            if (MyLibrary.ChkExist.CheckFileExist(strSystemIniFilePath) == false)
            {
                MessageBox.Show("'SystemIni.ini' file does not exist! Please check!", "File not exist");
                System.Environment.Exit(0);
            }
            //2. Reading steplist info contents
            //Master Step list name config
            strTemp = MyLibrary.ReadFiles.IniReadValue("MASTER_STEPLIST", "MasterSteplistName", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'MasterSteplistName' config in 'MASTER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.strMasterStepListFileName = strTemp;
            //Master Sheet name config
            strTemp = MyLibrary.ReadFiles.IniReadValue("MASTER_STEPLIST", "MasterSheetName", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'MasterSheetName' config in 'MASTER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.strMasterSheetName = strTemp;

            //Step list name config
            strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_STEPLIST", "SteplistName", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'SteplistName' config in 'CHECKER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.strProgramListFileName = strTemp;
            //Sheet name config
            strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_STEPLIST", "SheetName", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'SheetName' config in 'CHECKER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.strProgramListSheetName = strTemp;

            //3. Reading checker Info config
            strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_INFO", "Number_Checker", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'Number_Checker' config in 'CHECKER_INFO' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'Number_Checker' config in 'CHECKER_INFO' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intNumItem = int.Parse(strTemp);


            //******************************************************Reading config of Group Mode**************************************************************************************
            //Group Mode setting
            strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_INFO", "GroupMode", clsMainVar.strSystemIniPath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'GroupMode' config in 'CHECKER_INFO' of System Ini file! Return to default setting: No use group Mode", "ReadSystemIniFile()");
                strTemp = "0";
            }

            if (strTemp == "1") //Config use group Mode
            {
                this.clsMainVar.blGroupMode = true;
            }
            else //Default is no using
            {
                this.clsMainVar.blGroupMode = false;
            }

            if (this.clsMainVar.blGroupMode == true) //If config using group mode, then we have to analyze further
            {
                //Load number of group want to use
                strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_INFO", "GroupNum", clsMainVar.strSystemIniPath);
                if (strTemp.ToLower() == "error")
                {
                    MessageBox.Show("Warning: Config using Group Mode but cannot find 'GroupNum' config in 'CHECKER_INFO' of System Ini file! Please check & restart program!", "ReadSystemIniFile()");
                    Environment.Exit(0);
                }

                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("Error: 'GroupNum' config in 'CHECKER_INFO' of System.ini file is not integer! Please check & restart program", "ReadSystemIniFile()");
                    Environment.Exit(0);
                }

                this.clsMainVar.intGroupNum = intTemp;

                //Loading all child process ID assign for each group
                this.clsMainVar.lststrGroupChildID = new List<string>();
                for (int i = 0; i < this.clsMainVar.intGroupNum; i++)
                {
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_INFO", "GroupCheck" + (i + 1).ToString(), clsMainVar.strSystemIniPath);
                    if (strTemp.ToLower() == "error")
                    {
                        MessageBox.Show("Warning: Config using Group Mode but cannot find 'GroupCheck" + (i + 1).ToString() + "' config in 'CHECKER_INFO' of System Ini file! Please check & restart program!", "ReadSystemIniFile()");
                        Environment.Exit(0);
                    }
                    this.clsMainVar.lststrGroupChildID.Add(strTemp);
                }

            }


            //**************************************************************************************************************************************************************************

            //Display setting
            //Number of Row
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "NumRow", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'NumRow ' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'NumRow ' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intNumRow = int.Parse(strTemp);

            //Number of Collumn
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "NumCol", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'NumCol' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'NumCol' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intNumCol = int.Parse(strTemp);


            //Allign Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "AllignMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'AllignMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'AllignMode' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intAllignMode = int.Parse(strTemp);
            if ((clsMainVar.intAllignMode != 0) && (clsMainVar.intAllignMode != 1))
            {
                clsMainVar.intAllignMode = 0; //Default value
            }

            //Rounding Shape Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "RoundShapeMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'RoundShapeMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'RoundShapeMode' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intRoundShapeMode = int.Parse(strTemp);
            if ((clsMainVar.intRoundShapeMode != 0) && (clsMainVar.intRoundShapeMode != 1))
            {
                clsMainVar.intRoundShapeMode = 0; //Default value => zig-zag Shape
            }

            //Rounding OrgPosition  Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "OrgPosition", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'OrgPosition' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'OrgPosition' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intOrgPosition = int.Parse(strTemp);
            if ((clsMainVar.intOrgPosition < 0) && (clsMainVar.intOrgPosition > 3))
            {
                clsMainVar.intOrgPosition = 0; //Default value => zig-zag Shape
            }


            //Number User Textbox setting
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "NumberUserTextBox", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'NumberUserTextBox' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("Error: 'NumberUserTextBox' config in 'DISPLAY_SETTING' of System.ini file is not numerical!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            clsMainVar.intNumberUserTextBox = int.Parse(strTemp);


            //User Label
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "UserTextSetting", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'NumberUserTextBox' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }

            string[] tmpArr;
            tmpArr = strTemp.Split(',');
            clsMainVar.lstStrTbUser = new List<string>();

            for (intTemp = 0; intTemp < clsMainVar.intNumberUserTextBox; intTemp++)
            {
                if ((tmpArr.GetUpperBound(0) + 1) > intTemp)
                {
                    strTemp = tmpArr[intTemp];
                }
                else
                {
                    strTemp = "";
                }
                clsMainVar.lstStrTbUser.Add(strTemp);
            }

            clsMainVar.lstlstStrTbUserContent = new List<List<string>>();
            for (int i = 0; i < this.clsMainVar.intNumItem; i++)
            {
                List<string> lstTemp = new List<string>();
                for (intTemp = 0; intTemp < clsMainVar.intNumberUserTextBox; intTemp++)
                {
                    lstTemp.Add("");
                }

                clsMainVar.lstlstStrTbUserContent.Add(lstTemp);
            }

            //Control Process Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("CONTROL_MODE", "ControlProcessMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'ControlProcessMode' config in 'CONTROL_MODE' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }

            if (strTemp == "SingleThread") //Single Thread Mode
            {
                this.eChildRunningMode = enumSystemRunningMode.SingleThreadMode;
                this.clsMainVar.strChildRunningMode = strTemp;
            }
            else if (strTemp == "SingleProcess") //Single Process mode
            {
                this.eChildRunningMode = enumSystemRunningMode.SingleProcessMode;
                this.clsMainVar.strChildRunningMode = strTemp;
            }
            else if (strTemp == "Independent") //Independent mode
            {
                this.eChildRunningMode = enumSystemRunningMode.IndependentMode;
                this.clsMainVar.strChildRunningMode = strTemp;
            }
            else //Default is Paralell mode - 0
            {
                this.eChildRunningMode = enumSystemRunningMode.ParallelMode;
                this.clsMainVar.strChildRunningMode = "Parallel";
            }

            //User Select checking Mode
            this.clsMainVar.lststrSelectCheckingMode = new List<string>();
            this.clsMainVar.lststrSelectCheckingMode.Add("NormalMode"); //Default
            this.clsMainVar.lststrSelectCheckingMode.Add("ServiceMode"); //Default
            this.clsMainVar.lststrSelectCheckingMode.Add("PmMode"); //Default

            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "UserSelectMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'UserSelectMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                //Environment.Exit(0);
            }
            else
            {
                tmpArr = strTemp.Split(',');

                for (intTemp = 0; intTemp < (tmpArr.GetUpperBound(0) + 1); intTemp++)
                {
                    strTemp = tmpArr[intTemp].Trim();
                    this.clsMainVar.lststrSelectCheckingMode.Add(strTemp);
                }
            }

            //Reading user save select mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "SaveUserSelectMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'SaveUserSelectMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                //Environment.Exit(0);
                this.clsMainVar.strSaveUserSelectMode = this.clsMainVar.lststrSelectCheckingMode[0]; //Default is normal mode
            }
            else
            {
                //Looking for all supported checking mode
                bool blFound = false;
                for (int i = 0; i < this.clsMainVar.lststrSelectCheckingMode.Count; i++)
                {
                    if (this.clsMainVar.lststrSelectCheckingMode[i] == strTemp)
                    {
                        blFound = true;
                        this.clsMainVar.strSaveUserSelectMode = this.clsMainVar.lststrSelectCheckingMode[i];
                        break;
                    }
                }
                if (blFound == false)
                {
                    this.clsMainVar.strSaveUserSelectMode = this.clsMainVar.lststrSelectCheckingMode[0];
                }
            }

            //*************************************************DATA SAVING FOR PROGRAM LIST**********************************************************************
            //For data saving
            strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "TestDataSavePath", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'TestDataSavePath' config in 'DATA_SAVING' of System.ini file! Please check!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }

            if (strTemp.Trim() == "") //Change to default setting location
            {
                this.clsMainVar.strDataSavePath = Application.StartupPath + @"\Data";
            }
            else
            {
                this.clsMainVar.strDataSavePath = strTemp;
            }

            //NumberUserPreInfo 
            strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "NumberUserPreInfo", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'NumberUserPreInfo' config in 'DATA_SAVING' of System.ini file!", "ReadSystemIniFile()");
                clsMainVar.intProgramListNumberUserPreInfo = 10;
            }
            else
            {
                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("Error: 'NumberUserPreInfo' config in 'DATA_SAVING' of System.ini file is not numerical! Return to default: 10", "ReadSystemIniFile()");
                    clsMainVar.intProgramListNumberUserPreInfo = 10;
                }
                else
                {
                    clsMainVar.intProgramListNumberUserPreInfo = int.Parse(strTemp);
                }
            }

            string[] ArrTemp;
            strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "UserPreInfoHeader", strSystemIniFilePath);
            ArrTemp = strTemp.Split(',');
            clsMainVar.lststrProgramListUserPreInfoHeader = new List<string>(ArrTemp);

            //NumberUserAfterInfo 
            strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "NumberUserAfterInfo", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'NumberUserAfterInfo' config in 'DATA_SAVING' of System.ini file!", "ReadSystemIniFile()");
                clsMainVar.intProgramListNumberUserAfterInfo = 10;
            }
            else
            {
                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("Error: 'NumberUserAfterInfo' config in 'DATA_SAVING' of System.ini file is not numerical! Return to default: 10", "ReadSystemIniFile()");
                    clsMainVar.intProgramListNumberUserAfterInfo = 10;
                }
                else
                {
                    clsMainVar.intProgramListNumberUserAfterInfo = int.Parse(strTemp);
                }
            }

            strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "UserAfterInfoHeader", strSystemIniFilePath);
            ArrTemp = strTemp.Split(',');
            clsMainVar.lststrProgramListUserAfterInfoHeader = new List<string>(ArrTemp);

            //****************************************************************FOR ORIGIN STEP LIST**********************************************************************
            //Reading Origin Step List setting - In System Ini
            strTemp = MyLibrary.ReadFiles.IniReadValue("ORIGIN_STEPLIST", "UsingOriginStepList", strSystemIniFilePath);
            if (strTemp.ToLower() == "error") //Cannot find setting
            {
                MessageBox.Show("Error: cannot find 'UsingOriginStepList' config in 'ORIGIN_STEPLIST' of System.ini file!", "ChildReadingSetting()");
                Environment.Exit(0);
            }
            if (strTemp.ToLower() == "0") //Setting no use origin steplist
            {
                this.clsMainVar.blUsingOriginSteplist = false;
            }
            else //Default setting is using origin steplist
            {
                this.clsMainVar.blUsingOriginSteplist = true;

                //NOTE: All config behind, setting in User Ini
                strTemp = MyLibrary.ReadFiles.IniReadValue("ORIGIN_STEPLIST", "SteplistName", clsMainVar.strUserIniPath);
                if (strTemp.ToLower() == "error") //Cannot find setting
                {
                    MessageBox.Show("Error: cannot find 'SteplistName' config in 'ORIGIN_STEPLIST' of User ini file!", "ChildReadingSetting()");
                    Environment.Exit(0);
                }
                this.clsMainVar.strOriginStepListFileName = Application.StartupPath + "\\" + strTemp; //For full file name
                //
                strTemp = MyLibrary.ReadFiles.IniReadValue("ORIGIN_STEPLIST", "SheetName", clsMainVar.strUserIniPath);
                if (strTemp.ToLower() == "error") //Cannot find setting
                {
                    MessageBox.Show("Error: cannot find 'SheetName' config in 'ORIGIN_STEPLIST' of User ini file!", "ChildReadingSetting()");
                    Environment.Exit(0);
                }
                this.clsMainVar.strOriginStepListSheetName = strTemp; //


                //
                //For data saving
                strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "TestDataSavePath", clsMainVar.strUserIniPath);
                if (strTemp.ToLower() == "error")
                {
                    MessageBox.Show("Warning: cannot find 'TestDataSavePath' config in 'DATA_SAVING' of User Ini file! Please check!", "ReadSystemIniFile()");
                    Environment.Exit(0);
                }

                if (strTemp.Trim() == "") //Change to default setting location
                {
                    this.clsMainVar.strStepListDataSavePath = Application.StartupPath + @"\Data";
                }
                else
                {
                    this.clsMainVar.strStepListDataSavePath = strTemp;
                }

                //NumberUserPreInfo 
                strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "NumberUserPreInfo", clsMainVar.strUserIniPath);
                if (strTemp.ToLower() == "error")
                {
                    MessageBox.Show("Warning: cannot find 'NumberUserPreInfo' config in 'DATA_SAVING' of User Ini file!", "ReadSystemIniFile()");
                    clsMainVar.intStepListNumberUserPreInfo = 10; //Default value
                }
                else
                {
                    if (int.TryParse(strTemp, out intTemp) == false)
                    {
                        MessageBox.Show("Error: 'NumberUserPreInfo' config in 'DATA_SAVING' of User Ini file is not numerical! Return to default: 10", "ReadSystemIniFile()");
                        clsMainVar.intStepListNumberUserPreInfo = 10; //Default value
                    }
                    else
                    {
                        clsMainVar.intStepListNumberUserPreInfo = int.Parse(strTemp);
                    }
                }

                strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "UserPreInfoHeader", clsMainVar.strUserIniPath);
                ArrTemp = strTemp.Split(',');
                clsMainVar.lststrStepListUserPreInfoHeader = new List<string>(ArrTemp);

                //NumberUserAfterInfo 
                strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "NumberUserAfterInfo", clsMainVar.strUserIniPath);
                if (strTemp.ToLower() == "error")
                {
                    MessageBox.Show("Warning: cannot find 'NumberUserAfterInfo' config in 'DATA_SAVING' of User Ini file!", "ReadSystemIniFile()");
                    clsMainVar.intStepListNumberUserAfterInfo = 10;
                }
                else
                {
                    if (int.TryParse(strTemp, out intTemp) == false)
                    {
                        MessageBox.Show("Error: 'NumberUserAfterInfo' config in 'DATA_SAVING' of User Ini file is not numerical! Return to default: 10", "ReadSystemIniFile()");
                        clsMainVar.intStepListNumberUserAfterInfo = 10;
                    }
                    else
                    {
                        clsMainVar.intStepListNumberUserAfterInfo = int.Parse(strTemp);
                    }
                }

                strTemp = MyLibrary.ReadFiles.IniReadValue("DATA_SAVING", "UserAfterInfoHeader", clsMainVar.strUserIniPath);
                ArrTemp = strTemp.Split(',');
                clsMainVar.lststrStepListUserAfterInfoHeader = new List<string>(ArrTemp);
            }


            //*******************************************************************************************************************************************************

            //Reading Total result checking
            strTemp = MyLibrary.ReadFiles.IniReadValue("TOTAL_RESULT", "Total_cnt", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'Total_cnt' config in 'TOTAL_RESULT' of System.ini file! Auto reset to 0!", "ReadSystemIniFile()");
                clsMainVar.intTotal_CheckCount = 0;
            }
            else
            {
                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("Error: 'Total_cnt' config in 'TOTAL_RESULT' of System.ini file is not numerical! Auto reset to 0!", "ReadSystemIniFile()");
                    clsMainVar.intTotal_CheckCount = 0;
                }
                else
                {
                    clsMainVar.intTotal_CheckCount = int.Parse(strTemp);
                }
            }
            //Reading Total Pass result checking
            strTemp = MyLibrary.ReadFiles.IniReadValue("TOTAL_RESULT", "Total_pass", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'Total_pass' config in 'TOTAL_RESULT' of System.ini file! Auto reset to 0!", "ReadSystemIniFile()");
                clsMainVar.intTotal_CheckPass = 0;
            }
            else
            {
                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("Error: 'Total_pass' config in 'TOTAL_RESULT' of System.ini file is not numerical! Auto reset to 0!", "ReadSystemIniFile()");
                    clsMainVar.intTotal_CheckPass = 0;
                }
                else
                {
                    clsMainVar.intTotal_CheckPass = int.Parse(strTemp);
                }
            }
            //Total pass rate, we not reading from ini file. But calculating.
            if (clsMainVar.intTotal_CheckCount == 0)
            {
                clsMainVar.dblTotal_PassRate = 0;
            }
            else
            {
                clsMainVar.dblTotal_PassRate = 100 * (Convert.ToDouble(clsMainVar.intTotal_CheckPass) / Convert.ToDouble(clsMainVar.intTotal_CheckCount));
                clsMainVar.dblTotal_PassRate = Math.Round(clsMainVar.dblTotal_PassRate, 2);
            }


            //Reading config using TCP/IP or not
            strTemp = MyLibrary.ReadFiles.IniReadValue("SOCKET_HOST", "UsingServer", clsMainVar.strSystemIniPath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Warning: cannot find 'UsingServer' config in 'SOCKET_HOST' of system Ini file! Return to default setting: using TCP/IP server", "ReadSystemIniFile()");
                strTemp = "1";
            }

            if (strTemp == "0") //Config no use TCP/IP server
            {
                this.clsMainVar.blUsingTCPIP = false;
            }
            else //Default is using
            {
                this.clsMainVar.blUsingTCPIP = true;
            }

            

        }
        
        //**********************************************************
        public void SystemIni()
        {
            //Ini for user utilities
            this.lstclsUserUlt = new List<clsUserUtility>();

            //Ini for Child Processs model
            lstChildProcessModel = new List<clsChildProcessModel>();

            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            string strTemp = "";

            //***************************NOTES: GROUP MODE OR NOT*********************************
            //Depend on group mode setting or not, decide how many child process neccessary
            int intNumberChildProcess = 0;
            if(this.clsMainVar.blGroupMode==false) //Normal Mode
            {
                intNumberChildProcess = this.clsMainVar.intNumItem;
            }
            else //Group Mode
            {
                intNumberChildProcess = this.clsMainVar.intGroupNum;
            }

            //Create child process class
            for (i = 0; i < intNumberChildProcess; i++)
            {
                var clsTemp = new clsChildProcessModel();
                strTemp = "";

                clsTemp.lststrProgramListUserPreInfo = new List<string>();
                for (j = 0; j < clsMainVar.intProgramListNumberUserPreInfo; j++)
                {
                    clsTemp.lststrProgramListUserPreInfo.Add(strTemp);
                }

                clsTemp.lststrProgramListUserAfterInfo = new List<string>();
                for (j = 0; j < clsMainVar.intProgramListNumberUserAfterInfo; j++)
                {
                    clsTemp.lststrProgramListUserAfterInfo.Add(strTemp);
                }

                clsTemp.lststrStepListUserPreInfo = new List<string>();
                for (j = 0; j < clsMainVar.intStepListNumberUserPreInfo; j++)
                {
                    clsTemp.lststrStepListUserPreInfo.Add(strTemp);
                }

                clsTemp.lststrStepListUserAfterInfo = new List<string>();
                for (j = 0; j < clsMainVar.intStepListNumberUserAfterInfo; j++)
                {
                    clsTemp.lststrStepListUserAfterInfo.Add(strTemp);
                }


                clsTemp.intProcessID = i;

                clsTemp.blGroupMode = this.clsMainVar.blGroupMode;
                
                //Passing setting for Program List
                clsTemp.clsChildSetting.strProgramListFileName = this.clsMainVar.strProgramListFileName;
                clsTemp.clsChildSetting.strProgramListSheetName = this.clsMainVar.strProgramListSheetName;

                //Passing origin step list setting
                clsTemp.clsChildSetting.blUsingOriginSteplist = this.clsMainVar.blUsingOriginSteplist;
                clsTemp.clsChildSetting.strOriginStepListFileName = this.clsMainVar.strOriginStepListFileName;
                clsTemp.clsChildSetting.strOriginStepListSheetName = this.clsMainVar.strOriginStepListSheetName;

                //Passing checking mode
                clsTemp.strSystemCheckingMode = this.clsMainVar.strSaveUserSelectMode; //Reload from last choice
                
                //For Child Setting
                clsTemp.clsChildSetting.intNumChecker = clsMainVar.intNumItem;
                clsTemp.clsChildSetting.intNumRow = clsMainVar.intNumRow;
                clsTemp.clsChildSetting.intNumCol = clsMainVar.intNumCol;
                clsTemp.clsChildSetting.intAllignMode = clsMainVar.intAllignMode;
                clsTemp.clsChildSetting.intRoundShapeMode = clsMainVar.intRoundShapeMode;
                clsTemp.clsChildSetting.intOrgPosition = clsMainVar.intOrgPosition;

                //If setting is Group Mode, then find out which child process belong to group
                if(this.clsMainVar.blGroupMode==true)
                {
                    clsTemp.lstclsItemCheckInfo = new List<classItemImformation>();

                    int intLowIndex = 0;
                    int intHiIndex = 0;
                    //Handle comma delimiter
                    string[] strTest = this.clsMainVar.lststrGroupChildID[i].Split(',');

                    for (j = 0; j < strTest.Length; j++)
                    {
                        //Handle -> Delimiter
                        string[] strTest2 = strTest[j].Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);

                        if (strTest2.Length == 1) //setting only 1 child process ID
                        {
                            if (int.TryParse(strTest2[0], out intLowIndex) == false)
                            {
                                MessageBox.Show("Error: Setting Process ID [" + strTest2[0] + "] config in 'GroupCheck" + (i + 1).ToString() + "' of System Ini file is not integer! Please check & restart program!", "ReadSystemIniFile()");
                                Environment.Exit(0);
                            }
                            //
                            intHiIndex = intLowIndex;
                        }
                        else if (strTest2.Length >= 2) //setting with short indication 1->3, 5->8...
                        {
                            //
                            if (int.TryParse(strTest2[0], out intLowIndex) == false)
                            {
                                MessageBox.Show("Error: Setting Process ID [" + strTest2[0] + "] config in 'GroupCheck" + (i + 1).ToString() + "' of System Ini file is not integer! Please check & restart program!", "ReadSystemIniFile()");
                                Environment.Exit(0);
                            }

                            if (int.TryParse(strTest2[1], out intHiIndex) == false)
                            {
                                MessageBox.Show("Error: Setting Process ID [" + strTest2[1] + "] config in 'GroupCheck" + (i + 1).ToString() + "' of System Ini file is not integer! Please check & restart program!", "ReadSystemIniFile()");
                                Environment.Exit(0);
                            }

                            if (intLowIndex > intHiIndex)
                            {
                                MessageBox.Show("Error: Setting Process ID Low index [" + intLowIndex.ToString() + "] is bigger than Hi Index [" + intHiIndex.ToString() + "] config in 'GroupCheck" + (i + 1).ToString() + "'! Please check & restart program!", "ReadSystemIniFile()");
                                Environment.Exit(0);
                            }
                        }

                        for (k = intLowIndex; k <= intHiIndex; k++)
                        {
                            var clsNewItem = new classItemImformation();
                            clsNewItem.intItemID = k;
                            clsNewItem.intGroupNumber = k - intLowIndex + 1;

                            //Add to Group
                            clsTemp.lstclsItemCheckInfo.Add(clsNewItem);
                        }
                    } //end for j

                } //end if group mode
                else //Not group mode setting
                {
                    //Not group mode setting, then each child process has only 1 item ID
                    //And the Item ID is same as child process ID
                    //Group number setting is 1 (There is only 1 group with 1 item)
                    classItemImformation clsNewItem = new classItemImformation();
                    clsNewItem.intItemID = clsTemp.intProcessID;
                    clsNewItem.intGroupNumber = 1;

                    clsTemp.lstclsItemCheckInfo.Add(clsNewItem);
                }

                //Ini for Item's User Pre Info & After Info
                for(j=0;j<clsTemp.lstclsItemCheckInfo.Count;j++)
                {
                    strTemp = "";

                    clsTemp.lstclsItemCheckInfo[j].lststrProgramListUserPreInfo = new List<string>();
                    for (k = 0; k < clsMainVar.intProgramListNumberUserPreInfo; k++)
                    {
                        clsTemp.lstclsItemCheckInfo[j].lststrProgramListUserPreInfo.Add(strTemp);
                    }

                    clsTemp.lstclsItemCheckInfo[j].lststrProgramListUserAfterInfo = new List<string>();
                    for (k = 0; k < clsMainVar.intProgramListNumberUserAfterInfo; k++)
                    {
                        clsTemp.lstclsItemCheckInfo[j].lststrProgramListUserAfterInfo.Add(strTemp);
                    }

                    clsTemp.lstclsItemCheckInfo[j].lststrStepListUserPreInfo = new List<string>();
                    for (k = 0; k < clsMainVar.intStepListNumberUserPreInfo; k++)
                    {
                        clsTemp.lstclsItemCheckInfo[j].lststrStepListUserPreInfo.Add(strTemp);
                    }

                    clsTemp.lstclsItemCheckInfo[j].lststrStepListUserAfterInfo = new List<string>();
                    for (k = 0; k < clsMainVar.intStepListNumberUserAfterInfo; k++)
                    {
                        clsTemp.lstclsItemCheckInfo[j].lststrStepListUserAfterInfo.Add(strTemp);
                    }
                }

                //Ini for Child Process Model
                //clsTemp.ChildProcessModelIni(this.clsMainVar.blGroupMode);

                //Add to list
                this.lstChildProcessModel.Add(clsTemp);

            } //End for i

            //Ini for all child process
            for(i=0;i<this.lstChildProcessModel.Count;i++)
            {
                //Ini for Child Process Model
                this.lstChildProcessModel[i].ChildProcessModelIni(this.clsMainVar.blGroupMode);
            }



            if(this.clsMainVar.blGroupMode==true)
            {
                //Check Child process ID setting in group
                List<classItemImformation> lstclsChildTemp = new List<classItemImformation>();

                for(i=0;i<this.lstChildProcessModel.Count;i++)
                {
                    for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                    {
                        lstclsChildTemp.Add(this.lstChildProcessModel[i].lstclsItemCheckInfo[j]);
                    }
                }
                //
                if(this.clsMainVar.intNumItem != lstclsChildTemp.Count)
                {
                    MessageBox.Show("Error: The number of Checker & Child Process ID setting in Group Mode not match! Please check & restart program!", "ReadSystemIniFile()");
                    Environment.Exit(0);
                }
                //
                for(i=0;i<this.clsMainVar.intNumItem;i++)
                {
                    bool blFound = false;
                    for(j=0;j<lstclsChildTemp.Count;j++)
                    {
                        if(i==lstclsChildTemp[j].intItemID)
                        {
                            blFound = true;
                            break;
                        }
                    }
                    //
                    if(blFound==false)
                    {
                        MessageBox.Show("Error: Could not found Child Process ID [" + i.ToString() + "] in group setting! Please check & restart program!", "ReadSystemIniFile()");
                        Environment.Exit(0);
                    }
                }

            }
            
            //Ini for Single Thread class
            this.clsSingleThreadModel = new nspSingleThreadProcessModel.clsSingleThreadProcessModel();
            this.clsSingleThreadModel.SingleThreadIni(this.lstChildProcessModel); //Assign list of chils process same as 'this'

            //Ini for saving data
            this.clsDataSaving = new nspSavingData.clsSavingData(this);
            DataSavingIni();

            //Start CFP TCP/IP SOCKET SERVER
            if(this.clsMainVar.blUsingTCPIP==true)
            {
                string strRet = "";
                this.clsTCPSERVER = new clsTcpIpHandle(this);
                strRet = this.clsTCPSERVER.SocketIniFileReading();
                strRet = this.clsTCPSERVER.SocketServerStart();
                if (strRet != "0")
                {
                    MessageBox.Show("TCP/IP Server start fail. Error message: " + strRet, "SystemIni() Warning");
                }
            }

            //Imported Child Running Mode
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                this.lstChildProcessModel[i].eChildRunningMode = this.eChildRunningMode;
            }

            //if Run Mode is independent mode => start timer of each child process
            if(this.eChildRunningMode == enumSystemRunningMode.IndependentMode)
            {
                for (i = 0; i < this.lstChildProcessModel.Count;i++)
                {
                    this.lstChildProcessModel[i].tmrIndependent.Enabled = true;
                    this.lstChildProcessModel[i].tmrIndependent.Start();
                }
            }

            ////Ini for system watcher class
            //this.clsSystemWatch = new clsSystemWatcher(this);
            //this.clsSystemWatch.PropertyChanged += clsSystemWatch_PropertyChanged;

            ////Calculate system protection code
            //this.UpdateSystemProtectCodeInfo();
        }
        //**********************************************************
        public string GetSystemWatcherInfo { get; set; }
        void clsSystemWatch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.PropertyName == "strWatccherInfo")
            {
                //this.GetSystemWatcherInfo = this.clsSystemWatch.strWatcherInfo;
                //OnPropertyChanged("GetSystemWatcherInfo");
            }
        }

        //**********************************************************
        public void DataSavingIni()
        {
            //Do ini for class saving data
            this.clsDataSaving.SavingDataIni();
        }

        //**********************************************************
        public string strProgramDateCreated()
        {
            return GetDateTimeLastModify().ToString();
        }
        
        //**********************************************************
        public DateTime GetDateTimeLastModify()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            return fileinfo.LastWriteTime;
        }

        //**********************************************************
        public bool IniChildProcess()
        {
            bool blRet = true;
            int i = 0;

            //Pass Child control Model & Master process model object for all child process
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                this.lstChildProcessModel[i].objChildControlModel = this;
                this.lstChildProcessModel[i].objMasterProcessModel = this.GetMasterProcessObject();
            }

            //Now, do execute ini step
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                string strTemp = "";
                strTemp = this.lstChildProcessModel[i].ChildProcessIniStepEx(this.lstChildProcessModel[i].lstChildIni);

                if (strTemp != "0") //Ini process fail
                {
                    blRet = false;
                    MessageBox.Show(strTemp, "Child Process" + this.lstChildProcessModel[i].intProcessID.ToString() + " Ini Fail!");
                    //Environment.Exit(0);
                }
            }

            //
            CalChildHeaderInfo();

            return blRet;
        }

        //**********************************************************
        public bool ShutdownChildProcess()
        {
            bool blRet = true;
            int i = 0;
            //1. Abort all checking thread which is still alive
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {

                //Stop timer Independent
                if (this.lstChildProcessModel[i].tmrIndependent != null)
                {
                    this.lstChildProcessModel[i].tmrIndependent.Enabled = false;
                    this.lstChildProcessModel[i].tmrIndependent.Stop();
                }

                //Stop Thread Background
                //if (this.lstChildProcessModel[i].thrdBackGround != null)
                //{
                //    if (this.lstChildProcessModel[i].thrdBackGround.IsAlive == true)
                //    {
                //        this.lstChildProcessModel[i].thrdBackGround.Abort();
                //    }
                //}

                ////Stop Thread Checking
                //if (this.lstChildProcessModel[i].thrdChildProcess == null) continue;
                //if (this.lstChildProcessModel[i].thrdChildProcess.IsAlive == true)
                //{
                //    this.lstChildProcessModel[i].thrdChildProcess.Abort();
                //}
            }

            //if (this.clsSingleThreadModel.thrdSingleThread != null)
            //{
            //    if (this.clsSingleThreadModel.thrdSingleThread.IsAlive == true)
            //    {
            //        this.clsSingleThreadModel.thrdSingleThread.Abort();
            //    }
            //}

            //2. Execute user end step in child process
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                string strTemp = "";
                strTemp = this.lstChildProcessModel[i].ChildProcessUserEndStepEx();
                if (strTemp != "0")
                {
                    blRet = false;
                    MessageBox.Show(strTemp);
                }
            }

            return blRet;
        }

        //**********************************************************
        public void AddUserMenuItem(object objMenuItem) //System.Windows.Controls.MenuItem newMenuItem
        {
            if (objMenuItem is System.Windows.Controls.MenuItem)
            {
                System.Windows.Controls.MenuItem newMenuItem = (System.Windows.Controls.MenuItem)objMenuItem;
                this.obsMenuUserUtilities.Add(newMenuItem);
                OnPropertyChanged("obsMenuUserUtilities"); //No need?
            }
        }

        //**********************************************************
        public void ProcessCheckingStart() //When command to start checking. We do it here!
        {
            //Check if checking process can start or not
            //Get Current State
            var state = nspAppStore.clsAppStore.GetCurrentState();
            if(state.PmModeRequestStop == true)
            {
                if (this.lstChildProcessModel[0].strSystemCheckingMode != "PmMode") //Only allow checking process running in PM Mode
                {
                    return;
                }
            }
            // MessageBox.Show(this.lstChildProcessModel[0].strSystemCheckingMode);
            
            //If allow start checking, then prepare for start...
            int i, j = 0;

            //Clear PASS/FAIL label
            this.clsBindingView.strTotalResult = "";

            //Notify change
            OnPropertyChanged("clsBindingView");

            //Marking start time
            this.clsMainVar.intStartTick = MyLibrary.clsApiFunc.GetTickCount();

            //1. Creating thread to start
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                //this.lstChildProcessModel[i].thrdChildProcess = new System.Threading.Thread(lstChildProcessModel[i].ThreadChecking);
                //this.lstChildProcessModel[i].thrdChildProcess.Name = "ChildProcessID" + i.ToString();

                //Request saving data of all items
                this.lstChildProcessModel[i].blRequestSavingData = true;
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blRequestSavingData = true;
                }
            }

            //2. Base on running mode, do proper starting task
            int intStartTick = MyLibrary.clsApiFunc.GetTickCount();

            switch(this.eChildRunningMode)
            {
                case enumSystemRunningMode.ParallelMode:

                    // this.lstChildProcessModel[0].thrdChildProcess.Priority = System.Threading.ThreadPriority.Highest;
                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        //Not start thread of child process which all of its item skipped
                        if (this.isAllItemSkipped(i) == true)
                        {
                            continue;
                        }

						//Start all other thread of not skipped items
						//this.lstChildProcessModel[i].thrdChildProcess.Start();
						//while (this.lstChildProcessModel[i].thrdChildProcess.IsAlive == false)//Wait until all threads are started - Time out is 3 second
						//{
						//	if ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) > 3000)
						//	{
						//		break;
						//	}
						//}

						// Testing TPL Task
						//var testTask = Task.Factory
						//	.StartNew(this.lstChildProcessModel[i].ThreadChecking, TaskCreationOptions.LongRunning)
						//	.ContinueWith(t => MessageBox.Show(t.ToString()));

                        //this.lstChildProcessModel[i].taskCheckingProcess = Task.Factory
                        //    .StartNew(this.lstChildProcessModel[i].ThreadChecking, TaskCreationOptions.LongRunning);

                        //this.lstChildProcessModel[i].taskCheckingProcess = Task.Factory
                        //    .StartNew(t => this.lstChildProcessModel[i].ThreadChecking(),
                        //        TaskCreationOptions.LongRunning,
                        //        this.lstChildProcessModel[i].taskCheckingProcessCancelTokenSource.Token,
                        //        TaskScheduler.FromCurrentSynchronizationContext
                        //    );
                        this.lstChildProcessModel[i].StartTaskChecking();
					}
                    break;

                case enumSystemRunningMode.SingleThreadMode:
                    //In Single Thread Mode
                    //this.clsSingleThreadModel.thrdSingleThread = new System.Threading.Thread(this.clsSingleThreadModel.SingleThreadSequence);
                    //this.clsSingleThreadModel.thrdSingleThread.Start();
                    //this.clsSingleThreadModel.taskSingleThreadChecking = Task.Factory
                    //        .StartNew(this.clsSingleThreadModel.SingleThreadSequence, TaskCreationOptions.LongRunning);
                    this.clsSingleThreadModel.StartTaskThreadChecking();
					break;

                case enumSystemRunningMode.SingleProcessMode:
					//this.lstChildProcessModel[this.intProcessIDSelect].thrdChildProcess.Start();
					//while (this.lstChildProcessModel[this.intProcessIDSelect].thrdChildProcess.IsAlive == false)//Wait until thread are started - Time out is 3 second
					//{
					//    if ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) > 3000)
					//    {
					//        break;
					//    }
					//}
                    //this.lstChildProcessModel[this.intProcessIDSelect].taskCheckingProcess = Task.Factory
                    //        .StartNew(this.lstChildProcessModel[this.intProcessIDSelect].ThreadChecking, TaskCreationOptions.LongRunning);
                    this.lstChildProcessModel[this.intProcessIDSelect].StartTaskChecking();
					break;

                case enumSystemRunningMode.IndependentMode:
                    break;

                default:
                    break;
            }

            //Start polling for asking checking finish or not
            IDisposable test = null;
            test = Observable.Interval(TimeSpan.FromMilliseconds(100)) // Polling with 100ms interval
                .Subscribe(_ =>
                {
                    // MessageBox.Show("Check Finish!");
                    var isFinish = this.isCheckingFinish();
                    if (isFinish == "0") // Finish
                    {
                        // MessageBox.Show("Finish!");
                        test.Dispose(); // Ending subscribe, no need polling anymore.
                    }
                });
        }

        //**********************************************************
        public bool isAllItemSkipped(int intProcessID)
        {
            for(int i=0;i<this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo.Count; i++)
            {
                if (this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo[i].blSkipModeRequest == true) return true;
            }
            //
            return false;
        }

        //**********************************************************
        public string isCheckingFinish()
        {
            //we have 2 case: Multi-thread mode & single Thread mode
            int i = 0;
            bool blFlagFinish = true;

            if (this.eChildRunningMode == enumSystemRunningMode.ParallelMode)
            {
                blFlagFinish = true;
                for (i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    //if (this.lstChildProcessModel[i].thrdChildProcess.IsAlive == true)
                    //{
                    //    blFlagFinish = false;
                    //    break;
                    //}

                    if (this.lstChildProcessModel[i].taskCheckingProcess != null) // In some case like skip mode, it can be null => no care
                    {
                        if (!this.lstChildProcessModel[i].taskCheckingProcess.IsCompleted)
                        {
                            blFlagFinish = false;
                            break;
                        }
                    }
				}
            }

            if (this.eChildRunningMode == enumSystemRunningMode.SingleThreadMode)
            {
				//blFlagFinish = true;
				//if (this.clsSingleThreadModel.thrdSingleThread.IsAlive == true) //Not yet finish
				//{
				//    blFlagFinish = false;
				//}
				if (!this.clsSingleThreadModel.taskSingleThreadChecking.IsCompleted) //Not yet finish
				{
					blFlagFinish = false;
				}
			}


            if (this.eChildRunningMode == enumSystemRunningMode.SingleProcessMode)
            {
                //if (this.lstChildProcessModel[this.intProcessIDSelect].thrdChildProcess.IsAlive == true) //Not yet finish
                //{
                //    blFlagFinish = false;
                //}

				if (!this.lstChildProcessModel[this.intProcessIDSelect].taskCheckingProcess.IsCompleted) //Not yet finish
				{
					blFlagFinish = false;
				}
			}

            if (blFlagFinish == true)
            {
                //Record time finish checking
                this.clsMainVar.dateTimeChecking = DateTime.Now;
                //Do finish task: update and display result... But not apply for Single Process
                if(this.eChildRunningMode != enumSystemRunningMode.SingleProcessMode)
                {
                    FinishCheckingTask();
                }
                return "0"; //Finish Code
            }
            else
            {
                return "9000"; //Not Finish Code
            }
        }

        //**********************************************************
        private static Mutex mut = new Mutex();

        public void FinishCheckingTask(int intGroupProID = -1)
        {
            int i = 0;
            int j = 0;
            //If single process mode
            if (this.eChildRunningMode == enumSystemRunningMode.SingleProcessMode)
            {
                intGroupProID = this.intProcessIDSelect;
            }

            if(this.clsMainVar.blGroupMode==false) //Not Group Mode
            {
                //Cal all result - except skipped items
               
                //Cal total result
                this.clsMainVar.blTotalCheckingResult = true;
                for (i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    //
                    if (intGroupProID != -1)
                    {
                        if (i != intGroupProID) continue; //Not process ID want to update
                    }

                    //Not counting skipped items
                    // if (this.lstChildProcessModel[i].blSkipModeRequest == true) continue;
                    for (j = 0; j < this.lstChildProcessModel[i].lstclsItemCheckInfo.Count; j++) //
                    {
                        //Not counting skipped items
                        if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest == true) continue;
                        //Counting all other items
                        if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult == false)
                        {
                            this.clsMainVar.blTotalCheckingResult = false;
                            break; //Loop j
                        }
                    }
                }

                //Counting checking time
                if (this.clsMainVar.intTotal_CheckCount >= 1000000000) //need to reset to prevent overflow
                {
                    this.clsMainVar.intTotal_CheckCount = 0;
                    this.clsMainVar.intTotal_CheckPass = 0;
                    this.clsMainVar.dblTotal_PassRate = 0;
                }
                else
                {
                    this.clsMainVar.intTotal_CheckCount++;
                    if (this.clsMainVar.blTotalCheckingResult == true) this.clsMainVar.intTotal_CheckPass++; //If PASS then increased by 1

                    if (this.clsMainVar.intTotal_CheckCount != 0)
                    {
                        this.clsMainVar.dblTotal_PassRate = Math.Round(100 * ((double)this.clsMainVar.intTotal_CheckPass / (double)this.clsMainVar.intTotal_CheckCount), 2);
                    }
                }

                //Calculate tact time
                if (intGroupProID == -1) //Total tact time
                {
                    this.clsMainVar.dblTotalTactTime = Math.Round((double)(MyLibrary.ApiDeclaration.GetTickCount() - this.clsMainVar.intStartTick) / 1000, 2);
                }
                else //Only Process selected
                {
                    this.clsMainVar.dblTotalTactTime = Math.Round((double)this.lstChildProcessModel[intGroupProID].clsItemResult.dblItemTactTime, 2);
                }


                //Cal Header Info for display result
                this.CalHeaderInfo();

                //Saving checking data - Note that we only save data of what item already started checking. Which item not yet started => No saving data
                if(this.eChildRunningMode!= enumSystemRunningMode.IndependentMode)
                {
                    clsDataSaving.RecordTestData(intGroupProID);
                }
               
            }
            else //Group Mode - for all group. Note that in group mode, we count pass rate by pieces not by sheet!
            {
                //Cal total result - Note that Total resul here is count for Group itself, not for all other group!
                this.clsMainVar.blTotalCheckingResult = true;
                for (i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    //
                    if (intGroupProID != -1)
                    {
                        if (i != intGroupProID) continue; //Not process ID want to update
                    }

                    for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                    {
                        //Not counting skipped items
                        if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest == true) continue;
                        //Counting all other items
                        if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult == false)
                        {
                            this.clsMainVar.blTotalCheckingResult = false;
                            break; //Loop j
                        }
                    }

                    //
                    if(this.clsMainVar.blTotalCheckingResult==false)
                    {
                        break; //Loop i
                    }
                }


                //Cal Pass rate - Note that pass rate in Group Mode is count by pieces, not by sheet!
                for (i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    //
                    if (intGroupProID != -1)
                    {
                        if (i != intGroupProID) continue; //Not process ID want to update
                    }

                    for (j = 0; j < this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                    {
                        //Counting checking time
                        if (this.clsMainVar.intTotal_CheckCount >= 1000000000) //need to reset to prevent overflow
                        {
                            this.clsMainVar.intTotal_CheckCount = 0;
                            this.clsMainVar.intTotal_CheckPass = 0;
                            this.clsMainVar.dblTotal_PassRate = 0;
                        }
                        else
                        {
                            //Check skip
                            if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest == true) continue;
                            //Always increase total count
                            this.clsMainVar.intTotal_CheckCount++;
                            //Base on result increase total pass
                            if (this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult == true) this.clsMainVar.intTotal_CheckPass++; //If PASS then increased by 1

                            if (this.clsMainVar.intTotal_CheckCount != 0)
                            {
                                this.clsMainVar.dblTotal_PassRate = Math.Round(100 * ((double)this.clsMainVar.intTotal_CheckPass / (double)this.clsMainVar.intTotal_CheckCount), 2);
                            }
                        }
                    }
                }

                //Calculate tact time
                if (intGroupProID == -1) //Total tact time
                {
                    this.clsMainVar.dblTotalTactTime = Math.Round((double)(MyLibrary.ApiDeclaration.GetTickCount() - this.clsMainVar.intStartTick) / 1000, 2);
                }
                else //Only Process selected
                {
                    this.clsMainVar.dblTotalTactTime = Math.Round((double)this.lstChildProcessModel[intGroupProID].clsItemResult.dblItemTactTime, 2);
                }

                //Cal Header Info for display result
                this.CalHeaderInfo();

                //Saving checking data - Note that we only save data of what item already started checking. Which item not yet started => No saving data
                if (this.eChildRunningMode != enumSystemRunningMode.IndependentMode)
                {
                    clsDataSaving.RecordTestData(intGroupProID);
                } 
            }
            //Saving to ini file some information

            mut.WaitOne();
            //
            long lngret = 0;
            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "TOTAL_RESULT", "Total_cnt", this.clsMainVar.intTotal_CheckCount.ToString());
            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "TOTAL_RESULT", "Total_pass", this.clsMainVar.intTotal_CheckPass.ToString());
            //
            mut.ReleaseMutex();
        }

        //*********************************************************
        public void RetryCheckingStart() //Only working with Parallel Checking Mode & Single Thread mode?
        {
            //1. Looking for What thread is NG and create new thread for retry check
            int i,j;
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                if (this.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == false) //Only do with NG items
                {
                    //this.lstChildProcessModel[i].thrdChildProcess = new System.Threading.Thread(this.lstChildProcessModel[i].ThreadChecking);
                    //this.lstChildProcessModel[i].thrdChildProcess.Name = "ChildProcessID" + i.ToString();
                    //Request saving data
                    this.lstChildProcessModel[i].blRequestSavingData = true;

                    if (this.lstChildProcessModel[i].lstclsItemCheckInfo != null)
                    {
                        for (j = 0; j < this.lstChildProcessModel[i].lstclsItemCheckInfo.Count; j++)
                        {
                            this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blRequestSavingData = true;
                        }
                    }
                    
                }
                else //OK item - do not start & do not saving data
                {
                    this.lstChildProcessModel[i].blRequestSavingData = false;
                    if (this.lstChildProcessModel[i].lstclsItemCheckInfo != null)
                    {
                        for (j = 0; j < this.lstChildProcessModel[i].lstclsItemCheckInfo.Count; j++)
                        {
                            this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blRequestSavingData = false;
                        }
                    }
                    
                }
            }

            //
            int intStartTick = MyLibrary.clsApiFunc.GetTickCount();

            //In Parallel mode
            if (this.eChildRunningMode == enumSystemRunningMode.ParallelMode)
            {
                for (i = 0; i < this.lstChildProcessModel.Count; i++)
                {
                    if (this.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == false) //Only do with NG items
                    {
						//this.lstChildProcessModel[i].thrdChildProcess.Start();
						//while (this.lstChildProcessModel[i].thrdChildProcess.IsAlive == false) //Wait until all threads are started - timeout is 3 second
						//{
						//    if ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) > 3000)
						//    {
						//        break;
						//    }
						//}; 
                        //this.lstChildProcessModel[i].taskCheckingProcess = Task.Factory
                        //    .StartNew(this.lstChildProcessModel[i].ThreadChecking, TaskCreationOptions.LongRunning);
                        this.lstChildProcessModel[i].StartTaskChecking();
					}
                }
            }

            //In Single Thread Mode
            if (this.eChildRunningMode == enumSystemRunningMode.SingleThreadMode)
            {
				//this.clsSingleThreadModel.thrdSingleThread = new System.Threading.Thread(this.clsSingleThreadModel.SingleThreadSequence);
				//this.clsSingleThreadModel.thrdSingleThread.Start();
				//while (this.clsSingleThreadModel.thrdSingleThread.IsAlive == false)
				//{
				//    if ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) > 3000) //Wait until all threads are started - timeout is 3 second
				//    {
				//        break;
				//    }
				//};
                //this.clsSingleThreadModel.taskSingleThreadChecking = Task.Factory
                //    .StartNew(this.clsSingleThreadModel.SingleThreadSequence, TaskCreationOptions.LongRunning);
                this.clsSingleThreadModel.StartTaskThreadChecking();
            }
        }
        
        //*********************************************************
        public void ResetCounter()
        {
            long lngret = 0;
            this.clsMainVar.intTotal_CheckCount = 0;
            this.clsMainVar.intTotal_CheckPass = 0;
            this.clsMainVar.dblTotal_PassRate = 0;

            //Reset all Child Process ID
            int i,j = 0;
            for (i = 0; i < this.lstChildProcessModel.Count;i++)
            {
                this.lstChildProcessModel[i].clsItemResult.intItemNumberCheck = 0;
                this.lstChildProcessModel[i].clsItemResult.intItemNumberPass = 0;
                this.lstChildProcessModel[i].clsItemResult.dblItemPassRate = 0;
                //
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.intItemNumberCheck = 0;
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.intItemNumberPass = 0;
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.dblItemPassRate = 0;
                }
            }

            //Write to ini file
            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "TOTAL_RESULT", "Total_cnt", this.clsMainVar.intTotal_CheckCount.ToString());
            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "TOTAL_RESULT", "Total_pass", this.clsMainVar.intTotal_CheckPass.ToString());
            
            //Display on main form
            this.CalChildHeaderInfo();

            OnPropertyChanged("clsBindingView"); 
        }

        //*********************************************************
        public void ResetProgram()
        {
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ResetProgram(true));
        }

        //*********************************************************
        public void SkipCheckItem()
        {
            //Request user input password first
            //Request Password
            string strUserInput = "";
            System.Windows.Forms.DialogResult dlgTemp;

            bool blAllowChange = false;

            while (true)
            {
                dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Becareful when you select skip mode! Please input password first:", ref strUserInput);
                if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                {
                    //Confirm password
                    if (strUserInput.ToUpper().Trim() == "PED")
                    {
                        blAllowChange = true;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Password Wrong! Please input again or cancel skip mode!", "PASSWORD WRONG");
                    }
                }
                else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Cancel selected. Skip mode will not be activated!", "PASSWORD WRONG");
                    break;
                }
            }
            if (blAllowChange == false) return;

            //If Password OK, then ask user to select what item will be skipped
            blAllowChange = false;
            List<string> lstInput = new List<string>();

            while (true)
            {
                dlgTemp = MyLibrary.clsMyFunc.InputBox("SELECT SKIP ITEM", "Please input what item you wish to skip (separate by comma ','):", ref strUserInput);
                if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                {
                    //Confirm user input
                    string[] strSkipItemInput = strUserInput.Trim().Split(',');
                    lstInput = new List<string>(strSkipItemInput);

                    blAllowChange = true;
                    break;
                }
                else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Cancel selected. Skip mode will not be activated!", "CANCEL SKIP");
                    break;
                }
            }
            if (blAllowChange == false) return;

            //Check input validation
            int i, j, k = 0;
            List<int> lstintSkipID = new List<int>();
            for(i=0;i<lstInput.Count;i++)
            {
                int intTemp = 0;
                //Check numeric
                if(Int32.TryParse(lstInput[i],out intTemp)==false)
                {
                    MessageBox.Show("Input value [" + lstInput[i].ToString() + "] is not numeric!","Skip invalid");
                    continue;
                }

                //Check in valid range or not
                if ((intTemp > this.clsMainVar.intNumItem) || (intTemp < 1))
                {
                    MessageBox.Show("Input value [" + lstInput[i].ToString() + "] is out of range!", "Skip invalid");
                    continue;
                }
                //Find input process ID belong to what group
                bool blFound = false;
                int intGroupID = 0;
                int intSubChildID = 0;
                for(j=0;j<this.lstChildProcessModel.Count;j++)
                {
                    for(k=0;k<this.lstChildProcessModel[j].lstclsItemCheckInfo.Count;k++)
                    {
                        if(this.lstChildProcessModel[j].lstclsItemCheckInfo[k].intItemID == (intTemp-1))
                        {
                            blFound = true;
                            intGroupID = j;
                            intSubChildID = k;
                            break;
                        }
                    }
                    //
                    if (blFound == true) break;
                }
                //
                if(blFound == true)
                {
                    lstintSkipID.Add(this.lstChildProcessModel[intGroupID].lstclsItemCheckInfo[intSubChildID].intItemID);
                    this.lstChildProcessModel[intGroupID].lstclsItemCheckInfo[intSubChildID].blSkipModeRequest = true;
                }

            } //End for i

            //Summary what item will be skipped!
            if(lstintSkipID.Count>0)
            {
                string strSkipItems = "";
                for(i=0;i<lstintSkipID.Count;i++)
                {
                    strSkipItems = strSkipItems + (lstintSkipID[i] + 1).ToString() + ",";
                    //Change Color of skip item to Notify user
                    this.clsBindingView.lststrItemResult[lstintSkipID[i]] = (lstintSkipID[i] + 1).ToString() + " - Skipped";
                    this.clsBindingView.lstclrItemResultBackGround[lstintSkipID[i]] = System.Windows.Media.Brushes.Gray;
                    //Update
                    OnPropertyChanged("clsBindingView");
                    //Clear flag
                    this.ClearFlagCheckSkipItem(lstintSkipID[i]);

                }
                //Message out
                MessageBox.Show("OK. These items will be skipped: " + strSkipItems, "Skip Mode");
            }
            else
            {
                MessageBox.Show("There is no item will be skipped!", "Skip Mode");
            }
        }

        public void ClearFlagCheckSkipItem(int intItemID)
        {
            //With skipped item, the flag checked should be clear except Ini Class
            //(normal process: the flag will be clear in the begin of Thread Checking)
            int i,j = 0;
            bool blFound = false;
            int intProcessID = 0;
            int intGroupID = 0;
            //Looking for item ID belong to what child process
            for(i=0;i<this.lstChildProcessModel.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    if(this.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID==intItemID)
                    {
                        blFound = true;
                        intProcessID = i;
                        intGroupID = j;
                        break;
                    }
                }
                if (blFound == true) break;
            }
            if (blFound == false) return;
            //Clear flag check
            for(i=0;i< this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo[intGroupID].lstTotalStep.Count;i++)
            {
                int intStepPos = this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo[intGroupID].lstTotalStep[i].intStepPos;
                if (this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].intStepClass!=0)
                {
                    this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].blStepChecked = false;
                }
            }
        }

        //*********************************************************
        public void ClearAllSkip()
        {
            int i,j = 0;
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                int intSubChildID = 0;
                this.lstChildProcessModel[i].blSkipModeRequest = false;
                for (j = 0; j < this.lstChildProcessModel[i].lstclsItemCheckInfo.Count; j++)
                {
                    this.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest = false;
                    intSubChildID = this.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID;
                    //Change display
                    this.clsBindingView.lststrItemResult[intSubChildID] = (intSubChildID + 1).ToString();
                    this.clsBindingView.lstclrItemResultBackGround[intSubChildID] = System.Windows.Media.Brushes.LightGreen;
                }
            }

            //Update
            OnPropertyChanged("clsBindingView");

        }

        //*********************************************************
        public void SelectProgramList()
        {
            //************************************NOTES***********************************************************
            // Basically, change program list is so complicated => We need to restart program after update.
            // But before restart, we need to confirm is there any trouble with selected program list
            //****************************************************************************************************


            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            //Process open file dialog box results
            if (result == true) //User already select excel file
            {
                //Open document
                string filename = dlg.FileName;

                //Reload Program list
                string strRet = "";
                nspProgramList.classStepList clsNewStepList = new classStepList("", "", ref strRet);
                nspProgramList.classProgramList clsNewProgramList = new classProgramList(dlg.FileName, clsMainVar.strProgramListSheetName, ref strRet);
                if(strRet!= "0")
                {
                    MessageBox.Show("Change Program List Fail. Cannot load selected program list. Error message: [" + strRet + "]", "SelectProgramList() Error");
                    return;
                }
                //Assign temporary for Child Process 0
                //this.lstChildProcessModel[0].clsChildProgList = clsNewProgramList;
                //this.lstChildProcessModel[0].AnalyzeProgramList(this.lstChildProcessModel[0].clsChildProgList.lstProgramList);
                //
                long lngret = 0;
                //If Loading OK, then try to change Program List for all child process model
                bool blSaveSteplist = false;
                if(this.clsMainVar.blUsingOriginSteplist == false) //No use step list setting
                {
                    //Saving to ini file
                    //saving selection to ini file
                    string strNameOfFile = Path.GetFileName(filename);
                    string strNameOfFolder = Path.GetDirectoryName(filename);

                    if(strNameOfFolder == Application.StartupPath)
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "CHECKER_STEPLIST", "SteplistName", strNameOfFile);
                    }
                    else
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "CHECKER_STEPLIST", "SteplistName", filename);
                    }

                    //Restart program 
                    this.ResetProgram();
                    //
                    return;
                }
                else //If setting is using step list, then we need to confirm user want to change step list also or not
                {
                    DialogResult dlgResult = new DialogResult();
                    dlgResult = MessageBox.Show("Warning: The setting is using Step List. Do you want to change Step List too?", "Confirm Change Step List", MessageBoxButtons.YesNo);
                    if (dlgResult == DialogResult.Yes)
                    {
                        //MessageBox.Show("Please make change step list handle here!");

                        dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.FileName = ""; // Default file name
                        dlg.DefaultExt = ".xlsx"; // Default file extension
                        dlg.Filter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls"; // Filter files by extension

                        // Show open file dialog box
                        result = dlg.ShowDialog();


                        if (result == true) //New step list select
                        {
                            clsNewStepList = new classStepList(dlg.FileName, this.clsMainVar.strOriginStepListSheetName, ref strRet, 2000);
                            //  
                            if (strRet != "0") // Step list NG
                            {
                                MessageBox.Show("New steplist selected is NG. Error Message: [" + strRet + "]. New selection will not be applied!", "Select Step List Fail");
                                return;
                            }

                            //Assign temporarily for child process ID 0
                            //this.lstChildProcessModel[0].clsChildOriginStepList = clsNewStepList;

                            blSaveSteplist = true;
                        }
                    }
                    //else //Not select new step list
                    //{
                    //    clsNewStepList = this.lstChildProcessModel[0].clsChildOriginStepList;
                    //}

                    //
                    if(blSaveSteplist == false)
                    {
                        clsNewStepList = this.lstChildProcessModel[0].clsStepList;
                    }

                    //If New Step List is OK, then we have to compare & check compatibility between New Program list & new Step List

                    //Do checking compability
                    List<classStepDataInfor> lstTemp = new List<classStepDataInfor>();
                    lstTemp = clsNewProgramList.lstProgramList;
                    object objRet = this.lstChildProcessModel[0].CompareOriginStepListWithProgramList(clsNewStepList.lstExcelList, ref lstTemp, true);
                    clsNewProgramList.lstProgramList = lstTemp;
                    if (objRet.ToString() != "0") //NG
                    {
                        MessageBox.Show("Selected Steplist & Program List is not compatible! Error Message: [" + objRet.ToString() + "]. New selection will not be applied!", "CompareOriginStepListWithProgramList Fail");
                        return;
                    }

                    
                    //Saving Program list
                    string strNameOfFile = Path.GetFileName(filename);
                    string strNameOfFolder = Path.GetDirectoryName(filename);

                    if (strNameOfFolder == Application.StartupPath)
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "CHECKER_STEPLIST", "SteplistName", strNameOfFile);
                    }
                    else
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strSystemIniPath, "CHECKER_STEPLIST", "SteplistName", filename);
                    }

                    //Save Steplist
                    if (blSaveSteplist==true)
                    {
                        strNameOfFile = Path.GetFileName(dlg.FileName);
                        strNameOfFolder = Path.GetDirectoryName(dlg.FileName);

                        if (strNameOfFolder == Application.StartupPath)
                        {
                            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "ORIGIN_STEPLIST", "SteplistName", strNameOfFile);
                        }
                        else
                        {
                            lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "ORIGIN_STEPLIST", "SteplistName", dlg.FileName);
                        }
                    }

                    //Restart program 
                    this.ResetProgram();
                }
            }
        }

        //*********************************************************
        public void SelectStepList()
        {
            //************************************NOTES***********************************************************
            // Basically, change step list is so complicated => We need to restart program after update.
            // But before restart, we need to confirm is there any trouble with selected step list
            //****************************************************************************************************

            if(this.clsMainVar.blUsingOriginSteplist == false)
            {
                MessageBox.Show("Setting is no use step list. Change step list has no meaning!","Cancel change");
                return;
            }

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            //Process open file dialog box results
            if (result == true) //User already select excel file
            {
                //Open document
                string filename = dlg.FileName;

                //Reload Program list
                string strRet = "";

                classStepList clsNewStepList = new classStepList(dlg.FileName, this.clsMainVar.strOriginStepListSheetName, ref strRet, 2000);
                //  
                if (strRet != "0") // Step list NG
                {
                    MessageBox.Show("New steplist selected is NG. Error Message: [" + strRet + "]. New selection will not be applied!", "Select Step List Fail");
                    return;
                }

                //
                long lngret = 0;
                //
                bool blSaveProgramlist = false;

                nspProgramList.classProgramList clsNewProgramList = new classProgramList("", "", ref strRet);

                DialogResult dlgResult = new DialogResult();
                dlgResult = MessageBox.Show("Warning: Step List must be compatible with Program List. Do you want to change Program List too?", "Confirm Change Program List", MessageBoxButtons.YesNo);
                if (dlgResult == DialogResult.Yes)
                {
                    dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.FileName = ""; // Default file name
                    dlg.DefaultExt = ".xlsx"; // Default file extension
                    dlg.Filter = "Excel Files (*.xlsx, *.xls)|*.xlsx;*.xls"; // Filter files by extension

                    // Show open file dialog box
                    result = dlg.ShowDialog();

                    if (result == true) //New Program list select
                    {
                        clsNewProgramList = new classProgramList(dlg.FileName, clsMainVar.strProgramListSheetName, ref strRet);
                        
                        if (strRet != "0")
                        {
                            MessageBox.Show("Change Program List Fail. Cannot load selected program list. Error message: [" + strRet + "]", "SelectProgramList() Error");
                            return;
                        }
                        //
                        blSaveProgramlist = true;
                    }
                }

                //
                if (blSaveProgramlist == false)
                {
                    clsNewProgramList = this.lstChildProcessModel[0].clsProgramList;
                }

                //If New Step List is OK, then we have to compare & check compatibility between New Program list & new Step List
                //Do checking compability
                List<classStepDataInfor> lstTemp = new List<classStepDataInfor>();
                lstTemp = clsNewProgramList.lstProgramList;
                object objRet = this.lstChildProcessModel[0].CompareOriginStepListWithProgramList(clsNewStepList.lstExcelList, ref lstTemp, false);
                clsNewProgramList.lstProgramList = lstTemp;
                if (objRet.ToString() != "0") //NG
                {
                    MessageBox.Show("Selected Steplist & Program List is not compatible! Error Message: [" + objRet.ToString() + "]. New selection will not be applied!", "CompareOriginStepListWithProgramList Fail");
                    return;
                }

                //Saving Step list
                string strNameOfFile = Path.GetFileName(filename);
                string strNameOfFolder = Path.GetDirectoryName(filename);

                if (strNameOfFolder == Application.StartupPath)
                {
                    lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "ORIGIN_STEPLIST", "SteplistName", strNameOfFile);
                }
                else
                {
                    lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "ORIGIN_STEPLIST", "SteplistName", filename);
                }

                //Save Program List
                if (blSaveProgramlist == true)
                {
                    strNameOfFile = Path.GetFileName(dlg.FileName);
                    strNameOfFolder = Path.GetDirectoryName(dlg.FileName);

                    if (strNameOfFolder == Application.StartupPath)
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "CHECKER_STEPLIST", "SteplistName", strNameOfFile);
                    }
                    else
                    {
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.clsMainVar.strUserIniPath, "CHECKER_STEPLIST", "SteplistName", dlg.FileName);
                    }
                }

                //Restart program 
                this.ResetProgram();
            }
        }

        //*********************************************************
        public void SelectMasterProgramList()
        {
            this.RequestMasterChangeProgramList();
        }

        //*********************************************************
        public void ExportOptionViewTable()
        {
            //Prompt request user to select file name
            string strSavingFilepath = "C:\\OptionViewTable.csv";

            // Configure open file dialog box
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "CSV Files (*.csv)|*.csv"; // Filter files by extension
            dlg.FilterIndex = 1;


            //Process open file dialog box results
            if (dlg.ShowDialog() == DialogResult.OK) //User already select excel file
            {
                //
                strSavingFilepath = dlg.FileName;
            }
            else
            {
                return;
            }


            //Cal what content to save
            string strContentToWrite = this.DataTableToCSV(this.clsBindingView.dataTableOptionView, ',');

            //Start saving to csv file
            try
            {
                StreamWriter sWriter;
                sWriter = File.AppendText(strSavingFilepath);
                sWriter.WriteLine(strContentToWrite);
                sWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot saving to csv file. Error message: " + ex.Message, "ExportOptionViewTable() Error");
            }
        }

        public string DataTableToCSV(DataTable datatable, char seperator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < datatable.Columns.Count; i++)
            {
                sb.Append(datatable.Columns[i]);
                if (i < datatable.Columns.Count - 1)
                    sb.Append(seperator);
            }
            sb.AppendLine();
            foreach (DataRow dr in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    //sb.Append(dr[i].ToString());
                    sb.Append(dr[i].ToString().Replace("\r\n",". ").Replace(",",";"));

                    if (i < datatable.Columns.Count - 1)
                        sb.Append(seperator);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        //*********************************************************
        public void MasterSequenceTester()
        {
            object objTemp = this.RequestMasterSequenceTester();
        }

        //*********************************************************
        public void ChildProcessSequenceTester()
        {
            //Confirm password first
            //Request Password
            string strUserInput = "";
            System.Windows.Forms.DialogResult dlgTemp;

            bool blAllowChange = false;

            while (true)
            {
                dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Please input password first:", ref strUserInput);
                if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                {
                    //Confirm password
                    if (strUserInput.ToUpper().Trim() == "PED")
                    {
                        blAllowChange = true;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Password Wrong! Please input again or cancel!", "PASSWORD WRONG");
                    }
                }
                else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Cancel selected!", "PASSWORD WRONG");
                    break;
                }
            }
            if (blAllowChange == false) return;


            //Show window for Sequence Tester
            Views.wdChildSequenceTest wdChildControlTester = new Views.wdChildSequenceTest(this);
            wdChildControlTester.Show();

            ////
            //this.blActiveTestingSequence = true;
        }

        //*********************************************************
        public void SystemCommandMode()
        {
            //MessageBox.Show("SystemCommandMode");

            //Confirm password first
            string strUserInput = "";
            System.Windows.Forms.DialogResult dlgTemp;

            bool blAllowChange = false;

            while (true)
            {
                dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Please input password first:", ref strUserInput);
                if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                {
                    //Confirm password
                    if (strUserInput.ToUpper().Trim() == "PED")
                    {
                        blAllowChange = true;
                        break;
                    }
                    else
                    {
                        MessageBox.Show("Password Wrong! Please input again or cancel!", "PASSWORD WRONG");
                    }
                }
                else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show("Cancel selected!", "PASSWORD WRONG");
                    break;
                }
            }
            if (blAllowChange == false) return;

            //If password is OK. Then display System Command Mode Window
            Views.wdSystemCommand wdMySystemCommand = new Views.wdSystemCommand(this);
            wdMySystemCommand.Show();
        }

        public string MasterSystemCommandAnswer(string strUserRequest)
        {
            return this.RequestMasterSystemCommandAnswer(strUserRequest).ToString();
        }

        public string SystemCommandAnswer(string strUserRequest)
        {
            string strRet = "";
            string strChildProcessID = "";
            int intProcessID = 0;
            //
            strUserRequest = strUserRequest.Trim();
            //Separate user input to command & prameter
            List<string> lstSystemCommand = new List<string>(strUserRequest.Split(' '));

            if(lstSystemCommand.Count==0)
            {
                return "Error: Cannot find any command!";
            }

            //Analyze command
            //Find Process Target of command
            string strTargetCommand = lstSystemCommand[0].ToUpper();
            if(strTargetCommand.IndexOf('M')==0) //Master Process
            {
                return this.MasterSystemCommandAnswer(strUserRequest);
            }
            else if (strTargetCommand.IndexOf('C') == 0) //Child Process
            {
                //Checking Process ID
                strChildProcessID = strTargetCommand.Replace("C", "");
                if (int.TryParse(strChildProcessID, out intProcessID) == false)
                {
                    return "? command has invalid syntax!\r\nCorrect command syntax, ex: \"C0 ?15\" (Asking detail result of step 15 of Child Process ID 0)";
                }
                //
                if (intProcessID > this.lstChildProcessModel.Count - 1)
                {
                    return "? command has invalid ProcessID!\r\nMaximum ID input allow is " + (this.lstChildProcessModel.Count - 1).ToString();
                }
            }
            else //Not recognize Target Process of command
            {
                return "Error: Invalid Target Process of command. Valid command start with M - Master process or C - Child Process!";
            }

            if (lstSystemCommand.Count <2)
            {
                return "Error: Cannot find any command!";
            }

            string strCommand = lstSystemCommand[1];


            //1. For Asking Detail result checking. Ex: "C0 ?15" => Asking detail result of step 15 of Child Process ID 0.
            if(strCommand.IndexOf('?')==0)
            {
                //Searching step number
                string strStepNumber = strCommand.Replace("?", "").Trim();
                int intStepNum = 0;
                if (int.TryParse(strStepNumber, out intStepNum) == false) return "? command has invalid step number input!\r\nCorrect command syntax, ex: \"C0 ?15\" (Asking detail result of step 15 of Child Process ID 0)";
                strRet = this.strChildStepResultDetail(intProcessID, intStepNum);
            }
            else if (strCommand.IndexOf('=') == 0)
            {
                try
                {
                    int intEqualSignPos = strUserRequest.IndexOf('=');
                    string strExpression = strUserRequest.Substring(intEqualSignPos + 1, strUserRequest.Length - intEqualSignPos - 1).Trim();
                    strRet = this.strChildExpressionEval(intProcessID, strExpression);
                }
                catch(Exception ex)
                {
                    strRet = ex.Message;
                }
            }
            else
            {
                return "Error: command [" + strCommand + "] is invalid!";
            }

            //
            return strRet;
        }

        //**********************************************************
        public string strChildStepResultDetail(int intProcessID, int intStepNum)
        {
            string strRet = "";
            string strTemp = "";
            int i, j = 0;

            //
            //Check if step number is in program list or not
            if (intProcessID > this.lstChildProcessModel.Count - 1)
            {
                return "? command has invalid ProcessID!\r\nMaximum ID input allow is " + (this.lstChildProcessModel.Count - 1).ToString();
            }
            int intStepPos = this.lstChildProcessModel[intProcessID].FindCheckingStepPos(intStepNum);
            if (intStepPos == -1)
            {
                return "? command has invalid step number input!\r\nCannot find step number [" + intStepNum.ToString() + "] in Child Program list!";
            }

            //
            //If every thing is OK. Now return checking result detail
            strRet = strRet + "*****************************************************************************************************************************" + "\r\n";
            strRet = strRet + "Here are checking result detail of step number [" + intStepNum.ToString() + "] of checking process ID" + intProcessID.ToString() + "\r\n\r\n";
            strRet = strRet + "Step Position: " + intStepPos.ToString() + "\r\n";
            strRet = strRet + "is Checked: " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].blStepChecked.ToString() + "\r\n";
            strRet = strRet + "Result: " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].blStepResult.ToString() + "\r\n";

            if(this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].objStepCheckingData!=null)
            {
                strRet = strRet + "Return Data: " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].objStepCheckingData.ToString() + "\r\n";
            }
            else
            {
                strRet = strRet + "Return Data: " + "null" + "\r\n";
            }
            
            strRet = strRet + "Tact time: " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].dblStepTactTime.ToString() + " ms" + "\r\n";
            strRet = strRet + "Executed times: " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].intExecuteTimes.ToString() + "\r\n";
            strRet = strRet + "\r\n";

            strRet = strRet + "********************************************User Return Data*****************************************************************" + "\r\n";

            if(this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn!=null)
            {
                for (i = 0; i < this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn.Count; i++)
                {
                    for (j = 0; j < this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i].Count; j++)
                    {
                        strTemp = "";

                        if (j > 0)
                        {
                            strTemp = "[" + (j - 1).ToString() + "]     ";
                        }

                        strTemp = strTemp + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i][j].ToString();
                        //Display convert to Hexa format if possible
                        int intTemp = 0;
                        if (int.TryParse(this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i][j].ToString().Trim(), out intTemp) == true)
                        {
                            strTemp = strTemp + "   [Hex: " + intTemp.ToString("X") + "]";
                            //ASCII convert
                            char chrTest;
                            if (Char.TryParse(intTemp.ToString(), out chrTest) == true)
                            {
                                strTemp = strTemp + "   [Asc: " + Convert.ToChar(intTemp) + "]";
                            }
                            else
                            {
                                strTemp = strTemp + "   [Asc: ]";
                            }
                        }

                        strRet = strRet + strTemp + "\r\n";
                    }
                    strRet = strRet + "\r\n\r\n";
                }
            }
            
            strRet = strRet + "********************************************Transmission Area - original ****************************************************" + "\r\n";
            strRet = strRet + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].strTransmisstion + "\r\n";

            strRet = strRet + "********************************************Transmission Area - Processed****************************************************" + "\r\n";

            strRet = strRet + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].strTransmisstionEx + "\r\n";
            if (this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos] != null)
            {
                for (i = 0; i < this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count; i++)
                {
                    strRet = strRet + "[" + (i + 1).ToString() + "] " + this.lstChildProcessModel[intProcessID].clsCommonFunc.CalExpressionTreeResult(this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
                }
            }


            strRet = strRet + "*********************************************Parameter - Original************************************************************" + "\r\n";
            for (i = 0; i < this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].lstobjParameter.Count; i++)
            {
                strRet = strRet + "[" + (i + 1).ToString() + "] " + this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].lstobjParameter[i].ToString() + "\r\n";
            }

            strRet = strRet + "*********************************************Parameter - Processed***********************************************************" + "\r\n";
            strRet = strRet + "Parameter Ex: " + "\r\n";
            for (i = 0; i < this.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].lstobjParameterEx.Count; i++)
            {
                strRet = strRet + "[" + (i + 1).ToString() + "] " + this.lstChildProcessModel[intProcessID].clsCommonFunc.CalExpressionTreeResult(this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonParaCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
            }

            strRet = strRet + "*********************************************Special control***********************************************************" + "\r\n";
            if (this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos] != null)
            {
                for (i = 0; i < this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
                {
                    strRet = strRet + "[" + (i + 1).ToString() + "] " + this.lstChildProcessModel[intProcessID].clsCommonFunc.CalExpressionTreeResult(this.lstChildProcessModel[intProcessID].clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
                }
            }
            //
            return strRet;
        }
        
        public string strChildExpressionEval(int intProcessID, string strExpression)
        {
            string strRet = "";

            //Cal new command guider
            List<string> lststrInput = new List<string>();
            lststrInput.Add(strExpression);
            List<List<clsCommonCommandGuider>> lstlstCommandGuider = this.lstChildProcessModel[intProcessID].clsCommonFunc.CommonSpecialControlIni(lststrInput);
            //Cal result
            strRet = lstlstCommandGuider[0][0].evaluate().ToString() + "\r\n";

            //Add more info
            strRet = strRet + "*****************************************\r\n";
            strRet = strRet + "And here are more detail\r\n";
            strRet = strRet + this.lstChildProcessModel[intProcessID].clsCommonFunc.CalExpressionTreeResult(lstlstCommandGuider[0][0].ParsedExpression, 0);

            //
            return strRet;
        }

        //**********************************************************
        //Event handle from ChildProcessModel
        public void UpdateChangeFromChildProcessModel(object objSender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //
            if (e.PropertyName == "GetTableView")
            {
                OnPropertyChanged("GetTableView");
                return;
            }


            //
            int i = 0;
            int intGroupProID = 0;
            bool blFound = false;

            //Looking for what group of child process is sender
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                if (objSender == this.lstChildProcessModel[i])
                {
                    intGroupProID = i; //Found
                    blFound = true;
                    break;
                }
            }

            if (blFound == false) return; //Not found & not allow sub-child process update!


            //Update Info for sender
            this.CalChildItemInfo(intGroupProID);

            //Confirm if saving data request
            if (this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.blRequestSavingData == true)
            {
                this.clsDataSaving.RecordProgramListTestData(intGroupProID);
                if (this.clsMainVar.blUsingOriginSteplist == true)
                {
                    this.clsDataSaving.RecordStepListTestData(intGroupProID);
                }
                this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.blRequestSavingData = false;
            }

            //Confirm Update Pass Rate
            if (this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.blRequestUpdatePassRate == true)
            {
                //
                this.FinishCheckingTask(intGroupProID);
                //
                this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.blRequestUpdatePassRate = false;
            }

            //Notify change to view
            OnPropertyChanged("clsBindingView");
        }

        public void DefaultUserVview()
        {
            this.clsBindingView.strOptionViewHeader = "You are viewing Checking Program List!";
            this.clsBindingView.lststrOptionViewCombo = new List<string>();
            this.clsBindingView.dataTableOptionView = this.lstChildProcessModel[0].clsProgramList.MyDataTable;
        }

        public void SelectOptionView(int intViewID)
        {
            //Marking user selection
            this.intViewID = intViewID;

            //Request Shell switch to UserView
            
            int i = 0;
            switch(intViewID)
            {
                case 0: //View Data
                    this.clsBindingView.strOptionViewHeader = "You are viewing Checking Data!";
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();

                    for (i = 0; i < this.lstChildProcessModel.Count; i++)
                    {
                        this.clsBindingView.lststrOptionViewCombo.Add((i + 1).ToString());
                    }
                    this.lstChildProcessModel[0].CaldataTableOptionView();
                    this.clsBindingView.dataTableOptionView = this.lstChildProcessModel[0].dataTableOptionView;
                    break;

                case 1: //View Checking Program List
                    this.clsBindingView.strOptionViewHeader = "You are viewing Checking Program List!";
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    this.clsBindingView.dataTableOptionView = this.lstChildProcessModel[0].clsProgramList.MyDataTable;

                    //int intTest1 = this.lstChildProcessModel[0].clsChildProgList.MyDataTable.Columns.Count;
                    //int intTest2 = this.lstChildProcessModel[0].clsChildProgList.MyDataTable.Rows.Count;

                    break;
                case 2: //View Master Program List
                    this.clsBindingView.strOptionViewHeader = "You are viewing Master Program List!";
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    this.clsBindingView.dataTableOptionView = this.GetMasterProgramListDataTable();
                    break;
                case 3: //View Extension Info
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    this.ViewExtensionInfo(0);
                    break;
                case 4: //View Special Extension Info
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    this.ViewExtensionInfo(1);
                    break;
                case 5: //view version info
                    this.clsBindingView.strOptionViewHeader = "You are viewing Version Information!";
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    ViewVersionInfo();
                    break;
                case 6: //View Origin Step List
                    this.clsBindingView.lststrOptionViewCombo = new List<string>();
                    if(this.lstChildProcessModel[0].clsChildSetting.blUsingOriginSteplist==true)
                    {
                        this.clsBindingView.strOptionViewHeader = "You are viewing Original Step List!";
                        //this.clsBindingView.dataTableOptionView = this.lstChildProcessModel[0].clsChildOriginStepList.MyDataTable;

                        //NOTES: For some reason related to old format of steplist, the number rows is extremely high (65120!!!)
                        //We do not display all, but necessary data on data grid view
                        DataTable test = this.lstChildProcessModel[0].clsStepList.MyDataTable.Clone();

                        //int intTest = test.Rows.Count;
                        for (i = 0; i < test.Columns.Count; i++)
                        {
                            if (test.Columns[i].DataType != typeof(string))
                            {
                                test.Columns[i].DataType = typeof(string);
                            }
                        }

                        //Cal Test Row
                        int intNumberRow = 13 + this.lstChildProcessModel[0].clsStepList.lstExcelList.Count;

                        for (i = 0; i < intNumberRow;i++)
                        {
                            DataRow dr = this.lstChildProcessModel[0].clsStepList.MyDataTable.Rows[i];
                            test.ImportRow(dr);
                        }

                        this.clsBindingView.dataTableOptionView = test;
                    }
                    else
                    {
                        this.clsBindingView.strOptionViewHeader = "Setting is no use Original Step List!";
                        this.clsBindingView.dataTableOptionView = new DataTable();
                    }
                    
                    break;
                default:
                    break;
            }

            //Notify change to view
            OnPropertyChanged("clsBindingView"); //clsBindingView.dataTableOptionView
        }

        public void ComboSelectOptionView(string strInput)
        {
            switch(this.intViewID)
            {
                case 0: //View Data
                    SelectViewDataChildProcess(strInput);
                    break;
                default:
                    break;
            }
        }

        public void SelectViewDataChildProcess(string strInput)
        {
            //check if string input is numeric or not
            int intProcessID = 0;
            if (int.TryParse(strInput, out intProcessID) == false) return;
            intProcessID = intProcessID - 1; //ID count from 0


            this.clsBindingView.strOptionViewHeader = "You are viewing Checking Data!";
            this.clsBindingView.lststrOptionViewCombo.Clear();
            int i = 0;
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                this.clsBindingView.lststrOptionViewCombo.Add((i + 1).ToString());
            }
            this.lstChildProcessModel[intProcessID].CaldataTableOptionView();
            this.clsBindingView.dataTableOptionView = this.lstChildProcessModel[intProcessID].dataTableOptionView;
            //Notify change
            OnPropertyChanged("clsBindingView");
        }

        public void ViewExtensionInfo(int intExtensionClass) //0: normal. 1: Special control extension
        {

            if(intExtensionClass==0)
            {
                this.clsBindingView.strOptionViewHeader = "You are viewing Extension Info!";
            }
            else if (intExtensionClass==1)
            {
                this.clsBindingView.strOptionViewHeader = "You are viewing Special Control Extension Info!";
            }
            else
            {
                return;
            }
            
            this.clsBindingView.lststrOptionViewCombo.Clear();

            //this.clsBindingView.dataTableOptionView = this.GetMasterProgramListDataTable();
            this.clsBindingView.dataTableOptionView = new DataTable();

            //Add column Info
            this.clsBindingView.dataTableOptionView.Columns.Add("Part ID"); //0
            this.clsBindingView.dataTableOptionView.Columns.Add("Name"); //1
            this.clsBindingView.dataTableOptionView.Columns.Add("Version"); //2
            this.clsBindingView.dataTableOptionView.Columns.Add("Date"); //3
            this.clsBindingView.dataTableOptionView.Columns.Add("Func ID"); //4
            this.clsBindingView.dataTableOptionView.Columns.Add("Note"); //5
            this.clsBindingView.dataTableOptionView.Columns.Add("Author"); //6

            //Add data Row
            int i = 0;
            for (i = 0; i < this.lstChildProcessModel[0].clsChildExtension.lstPluginCollection.Count; i++)
            {
                DataRow temp = this.clsBindingView.dataTableOptionView.NewRow();

                temp[0] = i.ToString(); //Part ID
                temp[1] = this.lstChildProcessModel[0].clsChildExtension.lstPluginCollection[i].Metadata.IPluginInfo.ToString(); //Name

                //Reading Info
                List<List<object>> lstlstInput = new List<List<object>>();
                List<List<object>> lstlstTemp = new List<List<object>>();
                this.lstChildProcessModel[0].clsChildExtension.lstPluginCollection[i].Value.IGetPluginInfo(lstlstInput, out lstlstTemp);

                if (lstlstTemp.Count == 0) continue;
                if (lstlstTemp[0].Count == 0) continue;

                //Check class of extension is normal or special

                //Support Function ID
                string strTemp = "";
                strTemp = lstlstTemp[0][0].ToString().Replace("(", " ");
                int intTemp = 0;
                intTemp = strTemp.IndexOf("special");
                if (intTemp == 0) //Special
                {
                    if (intExtensionClass != 1) //Not special
                    {
                        continue;
                    }
                    strTemp = strTemp.Replace("special,", "");
                }
                else //Normal
                {
                    if (intExtensionClass != 0) //Not normal
                    {
                        continue;
                    }
                    //
                    if(lstlstTemp[0].Count>1)
                    {
                        //Looking further for more Function with different Hardware ID
                        int j = 0;
                        for (j = 1; j < lstlstTemp[0].Count; j++)
                        {
                            List<string>  lstTemp = new List<string>(lstlstTemp[0][j].ToString().Split(','));
                            //Check valid condition
                            if (lstTemp.Count < 3) continue; //Not satify minimum data (JigID-HardID-FuncID)
                            int intJigID = 0;
                            int intHardID = 0;
                            if (int.TryParse(lstTemp[0], out intJigID) == false) continue; //JigID is not numeric
                            if (int.TryParse(lstTemp[1], out intHardID) == false) continue; //JigID is not numeric

                            //Adding OK
                            strTemp = strTemp + "\r\n" + lstlstTemp[0][j].ToString();
                        }
                    }  
                }

                //Support Function ID
                temp[4] = strTemp;

                //Looking for info
                foreach (object objInfo in lstlstTemp[0])
                {
                    //Convert to all lower Case
                    string strTemp2 = objInfo.ToString().ToLower().Trim();
                    string strOrigin = objInfo.ToString().Trim();
                    int intStartIndex = 0;

                    intStartIndex = strTemp2.IndexOf("version");
                    if ((strTemp2.Contains("version") == true) && (intStartIndex == 0))
                    {
                        strOrigin = strOrigin.Substring((intStartIndex + "version".Length), strOrigin.Length - (intStartIndex + "version".Length));
                        strOrigin = strOrigin.Replace(",", " ").Trim();

                        temp[2] = strOrigin;
                    }

                    intStartIndex = strTemp2.IndexOf("date");
                    if ((strTemp2.Contains("date") == true) && (intStartIndex == 0))
                    {
                        intStartIndex = strTemp2.IndexOf("date");
                        strOrigin = strOrigin.Substring((intStartIndex + "date".Length), strOrigin.Length - (intStartIndex + "date".Length));
                        strOrigin = strOrigin.Replace(",", " ").Trim();

                        temp[3] = strOrigin;
                    }

                    intStartIndex = strTemp2.IndexOf("note");
                    if ((strTemp2.Contains("note") == true) && (intStartIndex == 0))
                    {
                        intStartIndex = strTemp2.IndexOf("note");
                        strOrigin = strOrigin.Substring((intStartIndex + "note".Length), strOrigin.Length - (intStartIndex + "note".Length));
                        strOrigin = strOrigin.Replace(",", " ").Trim();

                        temp[5] = strOrigin;
                    }

                    intStartIndex = strTemp2.IndexOf("author");
                    if ((strTemp2.Contains("author") == true) && (intStartIndex == 0))
                    {
                        intStartIndex = strTemp2.IndexOf("author");
                        strOrigin = strOrigin.Substring((intStartIndex + "author".Length), strOrigin.Length - (intStartIndex + "author".Length));
                        strOrigin = strOrigin.Replace(",", " ").Trim();

                        temp[6] = strOrigin;
                    }
                }

                this.clsBindingView.dataTableOptionView.Rows.Add(temp);
            }
        }

        public void ViewVersionInfo()
        { 
            string strTemp = "";
            //
            this.clsBindingView.dataTableOptionView = new DataTable();
            
            //
            strTemp = this.GetTotalProtectCode();
            if (strTemp != "0") return; //Error

            //Add column Info
            this.clsBindingView.dataTableOptionView.Columns.Add("Item No"); //0
            this.clsBindingView.dataTableOptionView.Columns.Add("Item Name"); //1
            this.clsBindingView.dataTableOptionView.Columns.Add("Information"); //2
            
            DataRow temp = this.clsBindingView.dataTableOptionView.NewRow();

            //Add General Info
            temp = this.clsBindingView.dataTableOptionView.NewRow();

            temp[0] = "0";
            temp[1] = "General Info";
            temp[2] = "Protect Code: " + this.clsVerifyProtectInfo.strTotalProtectCode + "\r\n"
                + "Source Code: " + this.clsVerifyProtectInfo.strSourceProtectCode;

            this.clsBindingView.dataTableOptionView.Rows.Add(temp);


            //Add Framework version
            temp = this.clsBindingView.dataTableOptionView.NewRow();
            temp[0] = "1";
            temp[1] = "CFP FrameWork";
            temp[2] = this.GetFrameVersionInfo();
            this.clsBindingView.dataTableOptionView.Rows.Add(temp);

            //Add Master Process version
            temp = this.clsBindingView.dataTableOptionView.NewRow();
            temp[0] = "2";
            temp[1] = "Master Process";
            temp[2] = this.GetMasterVersionInfo().ToString();
            this.clsBindingView.dataTableOptionView.Rows.Add(temp);

            //Add Child Process version
            temp = this.clsBindingView.dataTableOptionView.NewRow();

            if(this.clsMainVar.blUsingOriginSteplist==true)
            {
                strTemp = "*****\r\n" + "Step List Name: " + this.lstChildProcessModel[0].clsStepList.strStepListname + "\r\n" +
                      "Step List Version: " + this.lstChildProcessModel[0].clsStepList.strStepListVersion + "\r\n" +
                      "SL Protect Code: " + this.lstChildProcessModel[0].clsStepList.GetProtectCode() + "\r\n" +
                      "Date Created: " + this.lstChildProcessModel[0].clsStepList.strStepListDateCreated + "\r\n";
            }

            temp[0] = "3";
            temp[1] = "Child Process";
            temp[2] = "Tool Version: " + this.strProgramVer + "\r\n" +
                      //"HashCode: " + this.GetHashString() + "\r\n" +
                      "HashCode: " + this.GetProtectionCode() + "\r\n" +
                      "Date Created: " + this.strProgramDateCreated() + "\r\n" +
                      "*****\r\n" +
                      "Program List Name: " + this.lstChildProcessModel[0].clsProgramList.strProgramListname + "\r\n" +
                      "Program List Version: " + this.lstChildProcessModel[0].clsProgramList.strProgramListVersion + "\r\n" +
                      "PL Protect Code: " + this.lstChildProcessModel[0].clsProgramList.GetProtectCode() + "\r\n" +
                      "Date Created: " + this.lstChildProcessModel[0].clsProgramList.strProgramListDateCreated + "\r\n"
                      + strTemp;

            this.clsBindingView.dataTableOptionView.Rows.Add(temp);

            //Add Module Information
            int i = 0;
            for (i = 0; i < this.clsVerifyProtectInfo.lststrModuleName.Count; i++)
            {
                temp = this.clsBindingView.dataTableOptionView.NewRow();
               
                string strTempInfo = "";
                strTempInfo += this.clsVerifyProtectInfo.lststrModuleName[i] + "\r\n";
                strTempInfo += this.clsVerifyProtectInfo.lststrModuleCode[i] + "\r\n";
                strTempInfo += this.clsVerifyProtectInfo.lststrModuleLastModify[i] + "\r\n";

                //
                temp[0] = (4 + i).ToString();
                temp[1] = "Module " + (i + 1).ToString();
                temp[2] = strTempInfo;

                this.clsBindingView.dataTableOptionView.Rows.Add(temp);
            }

        }

        //Support Function
        public void CalChildItemInfo(int intGroupProID)
        {
            int i,j = 0;
            if(this.clsMainVar.blGroupMode==false) //Not Group Mode
            {
                //Update table checking result
                this.clsBindingView.lststrItemResult[intGroupProID] = this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.strItemResult;
                this.clsBindingView.lstclrItemResultBackGround[intGroupProID] = this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.clrItemResultBackGround;

                //Update info in table info
                string strDefaultView = "";
                string strUserView = "";

                strDefaultView = this.lstChildProcessModel[intGroupProID].clsChildModelBindingView.strItemInfo;
                //Calculate User Info View
                strUserView = "";
                for (i = 0; i < this.clsMainVar.intNumberUserTextBox; i++)
                {
                    strUserView = strUserView + this.clsMainVar.lstStrTbUser[i] + ": " + clsMainVar.lstlstStrTbUserContent[intGroupProID][i] + "\r\n";
                }

                this.clsBindingView.lststrItemInfo[intGroupProID] = strDefaultView + "\r\n" + strUserView;
            }
            else //Group Mode
            {
                for (i = 0; i < this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo.Count;i++)
                {
                    //Skip check
                    if (this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo[i].blSkipModeRequest == true) continue;
                    //
                    int intSubProcessID = this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo[i].intItemID;
                    //Update table checking result
                    this.clsBindingView.lststrItemResult[intSubProcessID] = this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo[i].clsChildModelBindingView.strItemResult;
                    this.clsBindingView.lstclrItemResultBackGround[intSubProcessID] = this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo[i].clsChildModelBindingView.clrItemResultBackGround;

                    //Update info in table info
                    string strDefaultView = "";
                    string strUserView = "";

                    strDefaultView = this.lstChildProcessModel[intGroupProID].lstclsItemCheckInfo[i].clsChildModelBindingView.strItemInfo;

                    //Calculate User Info View
                    strUserView = "";
                    for (j = 0; j < this.clsMainVar.intNumberUserTextBox; j++)
                    {
                        strUserView = strUserView + this.clsMainVar.lstStrTbUser[j] + ": " + clsMainVar.lstlstStrTbUserContent[intSubProcessID][j] + "\r\n";
                    }

                    this.clsBindingView.lststrItemInfo[intSubProcessID] = strDefaultView + "\r\n" + strUserView;
                }
            }
        }

        /// <summary>
        /// Get item checking result.
        /// Note that 1 child process can have some checking item.
        /// Item ID is unique!
        /// </summary>
        /// <param name="intItemID"></param>
        /// <returns></returns>
        public object GetItemResult(int intItemID)
        {
            bool blRet = false;
            //Check item ID input is in valid range or not
            if((intItemID<0)||(intItemID>(this.clsMainVar.intNumItem-1)))
            {
                return "GetItemResult() Error: Item ID input is out of range!";
            }
            //Find Item ID belong to what child process
            int intChildItemID = 0;
            int intProcessID = this.FindProcessIDofItem(intItemID, out intChildItemID);
            if (intProcessID < 0) return "GetItemResult() Error: Cannot found Process ID of Item";
            if (intChildItemID < 0) return "GetItemResult() Error: Cannot found Group ID of Item";
            //Get result
            this.lstChildProcessModel[intProcessID].ItemResultCalculate(intChildItemID);
            blRet = this.lstChildProcessModel[intProcessID].lstclsItemCheckInfo[intChildItemID].clsItemResult.blItemCheckingResult;

            //
            return blRet;
        }

        public int FindProcessIDofItem(int intItemID, out int intChildItemID)
        {
            int intProcessID = -1;
            intChildItemID = -1;
            int i, j = 0;
            for(i=0;i<this.lstChildProcessModel.Count;i++)
            {
                for(j=0;j<this.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    if(intItemID==this.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID)
                    {
                        intChildItemID = j;
                        return i;
                    }
                }
            }
            return intProcessID;
        }

        public string UpdateUserTextBoxInfo(int intProcessID, int intNumberTextBox, string strNewContent)
        {
            if (intProcessID > this.clsMainVar.intNumItem) return "UpdateUserTextBoxInfo() Error: Process ID over setting limit!";
            if (intNumberTextBox > this.clsMainVar.intNumberUserTextBox) return "UpdateUserTextBoxInfo() Error: NumberUserTextBox want to change over setting limit!";

            if ((intProcessID < 0) || (intProcessID >= this.clsMainVar.intNumItem)) return "UpdateUserTextBoxInfo() Error: Process ID " + intProcessID.ToString() + " is invalid!";

            //Change content
            this.clsMainVar.lstlstStrTbUserContent[intProcessID][intNumberTextBox] = strNewContent;

            CalChildItemInfo(intProcessID);

            //Raise proper change
            OnPropertyChanged("clsBindingView");

            return "0"; //Return OK code
        }

        //**********************************************************
        //Constructor
        public clsChildControlModel()
        {
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SetSplashScreenMessage("Loading clsChildControlModel..."));

            //Testing
            //string strTest = this.GetProtectionCode();


            //initialize variable
            this.clsMainVar = new clsMainVariables();

            //Reading ini file
            ReadSystemIniFile();

            //ini for list of child process model
            SystemIni();

            //Ini for View Table
            ViewTableIni();

            //Ini for Step List View Table
            ViewStepListTableIni();

            //Ini for system Run Mode
            this.dictChildRunMode = new Dictionary<string, enumSystemRunningMode>();
            this.dictChildRunMode.Add("Parallel", enumSystemRunningMode.ParallelMode);
            this.dictChildRunMode.Add("SingleThread", enumSystemRunningMode.SingleThreadMode);
            this.dictChildRunMode.Add("SingleProcess", enumSystemRunningMode.SingleProcessMode);
            this.dictChildRunMode.Add("Independent", enumSystemRunningMode.IndependentMode);

            //this.clsMainVar.strChildRunningMode = this.dictChildRunMode[]

            //Ini for system Checking Mode
            this.dictChildCheckMode = new Dictionary<string, enumChildProcessCheckingMode>();
            this.dictChildCheckMode.Add("Normal", enumChildProcessCheckingMode.eNormal);
            this.dictChildCheckMode.Add("Single", enumChildProcessCheckingMode.eSingle);
            this.dictChildCheckMode.Add("Step", enumChildProcessCheckingMode.eStep);
            this.dictChildCheckMode.Add("Fail", enumChildProcessCheckingMode.eFail);
            this.dictChildCheckMode.Add("All", enumChildProcessCheckingMode.eAll);


            //Ini for Binding View Support
            this.clsBindingView = new clsBindingSupport(this.clsMainVar.intNumItem);
            //Calculate Layout map code
            this.clsBindingView.lstlstintLayoutMapCode = this.clsBindingView.ControlLayout(this.clsMainVar.intNumItem, this.clsMainVar.intOrgPosition, this.clsMainVar.intNumRow,
                                                        this.clsMainVar.intNumCol, this.clsMainVar.intAllignMode, this.clsMainVar.intRoundShapeMode);

            //Add property change handle
            int i = 0;
            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                //Add notify change event handle
                this.lstChildProcessModel[i].PropertyChanged += (s, e) => { UpdateChangeFromChildProcessModel(s, e); };
            }

            //Ini for user utilities
            this.lstclsUserUlt = new List<clsUserUtility>();

            //Ini for User Utilities
            this.obsMenuUserUtilities = new ObservableCollection<System.Windows.Controls.MenuItem>();
            
            //
            this.lststrUserHeaderInfo = new List<string>();
            //Refresh view
            CalChildHeaderInfo();

            //Child Header system Info
            CalChildHeaderSysInfo();

            //
            this.DefaultUserVview();

            //
            this.ICommandChildTesterApply = new DelegateCommand(this.ICommandChildTesterApplyHandle);
            this.ICommandChildTesterReset = new DelegateCommand(this.ICommandChildTesterResetHandle);
            this.ICommandChildTesterResetAll = new DelegateCommand(this.ICommandChildTesterResetAllHandle);

            this.clsVerifyProtectInfo = new clsVerifyProtectInformation();
            //Calculate system protection code
            // this.UpdateSystemProtectCodeInfo();

            // Subscribe to AppStore
            // Handle User select view mode
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.UserInterfaceControl.SelectOptionViewMode })
                .Subscribe(
                    state => this.SelectViewMode(state.UserInterfaceControl.SelectOptionViewMode)
                );
            // Handle Calculate Protection Code (After Shell Frame Finish Cal Protection Code)
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.shellFrameInfo.FinishCalInfo })
                .Subscribe(
                    state =>
                    {
                        if (state.shellFrameInfo.FinishCalInfo == true) this.UpdateSystemProtectCodeInfo();
                    }
                );

            // Handle Manual Start
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.UserInterfaceControl.ManualStartCheck })
                .Skip(1) // Reject 1st run time when system startup
                .Subscribe(
                    state =>
                    {
                        // MessageBox.Show(state.UserInterfaceControl.ManualStartCheck.ToString());
                        // if (state.UserInterfaceControl.ManualStartCheck == true) this.ProcessAfterStart();
                        this.ProcessCheckingStart();
                    }
                );

            // Handle Change Checking Mode
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.CheckingMode })
                .Skip(1) // Reject 1st run time when system startup
                .Subscribe(
                    state =>
                    {
                        this.SetCheckingMode(state.CheckingMode);
                    }
                );

            // Handle Cancel Checking Process
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.UserInterfaceControl.CancelCheckingProcess })
                .Skip(1) // Reject 1st run time when system startup
                .Subscribe(
                    state =>
                    {
                        this.btnEndCommandHandle();
                    }
                );

        }
        //**********************************************************
        public void SelectViewMode(string viewMode)
        {
            switch(viewMode)
            {
                case "ViewData":
                    this.SelectOptionView(0);
                    break;
                case "ChildProgramList":
                    this.SelectOptionView(1);
                    break;
                case "MasterProgramList":
                    this.SelectOptionView(2);
                    break;
                case "ViewUserFunctionInfo":
                    this.SelectOptionView(3);
                    break;
                case "ViewUserExpressionInfo":
                    this.SelectOptionView(4);
                    break;
                case "ViewVersionInfo":
                    this.SelectOptionView(5);
                    break;
                case "OriginStepList":
                    this.SelectOptionView(6);
                    break;

                default:
                    break;
            }
        }
    }

    //**********************************************************
    public class clsBindingSupport
    {
        //Total result PASS of FAIL
        public string strTotalResult { get; set; }

        //Total checking time
        public double dblTotalTactTime { get; set; }

        //Total pass rate 
        public double dblPassRate { get; set; }

        //For combo all info to display result on: Program List name - version - pass rate - tact time
        public string strChildHeaderInfo { get; set; } //Run Mode - Checking Mode - Pass Rate - Tact Time...
        public string strChildHeaderSysInfo { get; set; } //Program Ver - Step list Info...


        //For display info in main info flow document
        //TableInfo
        public List<string> lststrItemResult { get; set; }
        public List<System.Windows.Media.SolidColorBrush> lstclrItemResultBackGround { get; set; }

        public List<string> lststrItemResultView 
        {
            get
            {
                int i, j = 0;
                List<string> lstTemp = new List<string>();

                for (i = 0; i < this.lstlstintLayoutMapCode.Count; i++)
                {
                    for(j=0;j<this.lstlstintLayoutMapCode[i].Count;j++)
                    {
                        lstTemp.Add("");
                    }
                }

                int intPosition = 0;
                for (i = 0; i < this.lststrItemResult.Count;i++)
                {
                    bool blFound = this.CalPosition(i, ref intPosition);
                    if(blFound == true)
                    {
                        lstTemp[intPosition] = this.lststrItemResult[i];
                    }
                }
                return lstTemp;
            }
            set
            {
                this.lststrItemResultView = value;
            }
        }

        public List<System.Windows.Media.SolidColorBrush> lstclrItemResultBackGroundView
        {
            get
            {
                int i, j = 0;
                List<System.Windows.Media.SolidColorBrush> lstTemp = new List<System.Windows.Media.SolidColorBrush>();

                for (i = 0; i < this.lstlstintLayoutMapCode.Count; i++)
                {
                    for (j = 0; j < this.lstlstintLayoutMapCode[i].Count; j++)
                    {
                        lstTemp.Add(System.Windows.Media.Brushes.White);
                    }
                }

                int intPosition = 0;
                for (i = 0; i < this.lststrItemResult.Count; i++)
                {
                    bool blFound = this.CalPosition(i, ref intPosition);
                    if (blFound == true)
                    {
                        lstTemp[intPosition] = this.lstclrItemResultBackGround[i];
                    }
                }

                return lstTemp;
            }
            set
            {
                this.lstclrItemResultBackGroundView = value;
            }
        }

        public bool CalPosition(int intOrder, ref int intPosition) //Count from 0
        {
            int i, j = 0;
            bool blFound = false;
            intPosition = 0;
            for(i=0;i<this.lstlstintLayoutMapCode.Count;i++) //Row
            {
                for(j=0;j<this.lstlstintLayoutMapCode[i].Count;j++) //Col
                {
                    if (intOrder == (this.lstlstintLayoutMapCode[i][j] - 1)) //Count from 0
                    {
                        blFound = true;
                        break;
                    }
                    intPosition++;
                }
                if (blFound == true) break;
            }

            return blFound;
        }

        //TableDetail
        public List<List<int>> lstlstintLayoutMapCode { get; set; }//Count priority follow column
        //   1 - 5  - 9  - 13  lstintLayoutMapCode[0] ={1,5,9,13}
        //   2 - 6  - 10 - 14  lstintLayoutMapCode[1] ={2,6,10,14}
        //   3 - 7  - 11 - 15  lstintLayoutMapCode[2] ={3,7,11,15}
        //   4 - 8  - 12 - 16  lstintLayoutMapCode[3] ={4,8,12,16}

        public List<string> lststrResultPassFail { get; set; }
        public List<string> lststrPassRate { get; set; }
        public List<string> lststrItemCheckPass { get; set; }
        public List<string> lststrItemCheckCount { get; set; }
        public List<string> lststrStatus { get; set; }
        public List<string> lststrFailInfo { get; set; }
        public List<string> lststrCheckPoint { get; set; }
        public List<string> lststrItemInfo { get; set; } //combine of all above info
        public List<string> lststrItemNotes { get; set; }

        ////Support Function
        public List<List<int>> ControlLayout(int intNumChecker,int intOrgPosition, int intRow, int intCol, int intAllignMode, int intRoundShapeMode)
        {
            List<List<int>> lstlstintTest = new List<List<int>>();
            List<int> lstTemp = new List<int>();
            int intTemp = 0;

            //Ini for List of List
            int iRow = 0, iCol = 0;
            for (iRow = 0; iRow < intRow; iRow++)
            {
                lstTemp = new List<int>();
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    intTemp = 0;
                    lstTemp.Add(intTemp);
                }
                lstlstintTest.Add(lstTemp);
            }

            //Depend on each Allign method, assign value for each item 

            //Vertical * Zig-zag
            if ((intAllignMode == 0) && (intRoundShapeMode == 0))
            {
                //Count priority follow column
                //   1 - 5  - 9  - 13
                //   2 - 6  - 10 - 14
                //   3 - 7  - 11 - 15
                //   4 - 8  - 12 - 16
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    for (iRow = 0; iRow < intRow; iRow++)
                    {
                        lstlstintTest[iRow][iCol] = (iRow + 1) + iCol * intRow;

                    }
                }
            }

            //Horizontal * Zig-zag
            if ((intAllignMode == 1) && (intRoundShapeMode == 0))
            {
                //Count priority follow Row
                //   1  - 2  - 3  - 4
                //   5  - 6  - 7  - 8
                //   9  - 10 - 11 - 12
                //   13 - 14 - 15 - 16

                for (iRow = 0; iRow < intRow; iRow++)
                {
                    for (iCol = 0; iCol < intCol; iCol++)
                    {
                        lstlstintTest[iRow][iCol] = (iCol + 1) + iRow * intCol;
                    }
                }
            }

            //Vertical * Rounding
            if ((intAllignMode == 0) && (intRoundShapeMode == 1))
            {
                //Count priority follow column
                //   1 - 8  - 9  - 16
                //   2 - 7  - 10 - 15
                //   3 - 6  - 11 - 14
                //   4 - 5  - 12 - 13
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    for (iRow = 0; iRow < intRow; iRow++)
                    {

                        if ((iCol % 2) == 0) //Odd Col 1-3...
                        {
                            lstlstintTest[iRow][iCol] = (iRow + 1) + iCol * intRow;
                        }
                        else //Even Col 2-4...
                        {
                            lstlstintTest[iRow][iCol] = intRow - iRow + iCol * intRow;
                        }

                    }
                }

            }

            //Horizontal * Rounding
            if ((intAllignMode == 1) && (intRoundShapeMode == 1))
            {
                //Count priority follow Row
                //   1  - 2   - 3  - 4
                //   8  - 7   - 6  - 5
                //   9  - 10  - 11 - 12
                //   16 - 15  - 14 - 13
                for (iRow = 0; iRow < intRow; iRow++)
                {
                    for (iCol = 0; iCol < intCol; iCol++)
                    {

                        if ((iRow % 2) == 0) //Odd Row 1-3...
                        {
                            lstlstintTest[iRow][iCol] = (iCol + 1) + iRow * intCol;
                        }
                        else //Even Row 2-4...
                        {
                            lstlstintTest[iRow][iCol] = intCol - iCol + iRow * intCol;
                        }

                    }
                }
            }

            List<List<int>> lstlstintLayout = new List<List<int>>();
            //Ini for List of List
            for (iRow = 0; iRow < intRow; iRow++)
            {
                lstTemp = new List<int>();
                for (iCol = 0; iCol < intCol; iCol++)
                {
                    intTemp =lstlstintTest[iRow][iCol];
                    lstTemp.Add(intTemp);
                }
                lstlstintLayout.Add(lstTemp);
            }


            //Re-Layout again follow Origin Position setting
            //Swaping position
            switch (intOrgPosition)
            {
                case 0: //Upper-left
                    break;
                case 1: //Lower-left
                    //intTempY = Program.strcMainVar.intNumRow - 1 - intTempY;

                    //Swap position of Y to (Number of Row - Y)
                    for (iRow = 0; iRow < intRow;iRow++)
                    {
                        for (iCol = 0; iCol < intCol; iCol++)
                        {
                            lstlstintLayout[iRow][iCol] = lstlstintTest[intRow-1-iRow][iCol];
                        }
                    }
                    break;

                case 2: //Upper-Right
                    //intTempX = Program.strcMainVar.intNumCol - 1 - intTempX;
                    //Swap position of X to (Number of Col - X)
                    for (iRow = 0; iRow < intRow; iRow++)
                    {
                        for (iCol = 0; iCol < intCol; iCol++)
                        {
                            lstlstintLayout[iRow][iCol] = lstlstintTest[iRow][intCol-1-iCol];
                        }
                    }
                    break;
                case 3: //Lower-Right
                    //intTempY = Program.strcMainVar.intNumRow - 1 - intTempY;
                    //intTempX = Program.strcMainVar.intNumCol - 1 - intTempX;
                    //Swap position of X to (Number of Col - X) & Swap position of Y to (Number of Row - Y)
                    for (iRow = 0; iRow < intRow; iRow++)
                    {
                        for (iCol = 0; iCol < intCol; iCol++)
                        {
                            lstlstintLayout[iRow][iCol] = lstlstintTest[intRow - 1 - iRow][intCol - 1 - iCol];
                        }
                    }

                    break;
                default:
                    break;
            }

            //Return result
            return lstlstintLayout;
        }

        //**********************************************************
        
        //For binding in Option View
        public string strOptionViewHeader { get; set; }
        public List<string> lststrOptionViewCombo { get; set; }
        public DataTable dataTableOptionView { get; set; }
        
        //Constructor
        public clsBindingSupport(int intNumChildPro)
        {
            this.lststrItemResult = new List<string>();
            this.lstclrItemResultBackGround = new List<System.Windows.Media.SolidColorBrush>();

            //For table detail info
            this.lstlstintLayoutMapCode = new List<List<int>>();
            this.lststrItemInfo = new List<string>();
            this.lststrResultPassFail = new List<string>();
            this.lststrPassRate = new List<string>();
            this.lststrItemCheckPass = new List<string>();
            this.lststrItemCheckCount = new List<string>();
            this.lststrStatus = new List<string>();
            this.lststrFailInfo = new List<string>();
            this.lststrCheckPoint = new List<string>();
            this.lststrItemNotes = new List<string>();

            int i = 0;
            for (i = 0; i < intNumChildPro; i++)
            {
                this.lststrItemResult.Add((i+1).ToString());
                System.Windows.Media.SolidColorBrush clrTemp = new System.Windows.Media.SolidColorBrush();
                this.lstclrItemResultBackGround.Add(clrTemp);

                this.lststrItemInfo.Add("");
                this.lststrResultPassFail.Add("");
                this.lststrPassRate.Add("");
                this.lststrItemCheckPass.Add("");
                this.lststrItemCheckCount.Add("");
                this.lststrStatus.Add("");
                this.lststrFailInfo.Add("");
                this.lststrCheckPoint.Add("");
                this.lststrItemNotes.Add("");

            }

            //this.CalListstrItemInfo();

            //For binding in Option View
            this.lststrOptionViewCombo = new List<string>();
            this.dataTableOptionView = new DataTable();


            //this.strChildHeaderSysInfo = "Hehe: Hihi";

        }
    }

    //**********************************************************
}
