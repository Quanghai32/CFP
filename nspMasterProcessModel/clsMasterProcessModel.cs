using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Input;
using nspINTERFACE;
using nspCFPInfrastructures;
using nspProgramList;
using nspCFPExpression;
using System.Data;
using System.Threading;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;


namespace nspMasterProcessModel
{
    public enum enumMasterStatus
    {
        eMasterIni, //0
        eMasterCyclePoll, //1
        eMasterResetRequestPoll, //2
        eMasterResetProcess, //3
        eMasterEmerPoll, //4
        eMasterEmerHandle, //5

        eMasterSafetyPoll, //6
        eMasterSafetyHandle, //7

        eMasterPreStart, //8
        eMasterStartPoll, //9
        eMasterCheckingProcess, //10

        eMasterUserEnd, //11

        eMasterBackGroundPoll, //100

        eMasterNotRecognize
    }

    public enum enumMasterRunMode
    {
        eMasterNormal, //Running normally
        eMasterSingleAll, //Running each step & wait for allowing signal (button press) for continue
        eMasterSingleClass, //Running Single mode in steps of some selected class
        eMasterStep //Running until reach desired step, continue if allowing condition met (button press)
    }

    [Export(typeof(clsMasterProcessModel))]
    [Export(typeof(nspCFPInfrastructures.IMasterProCmdService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class clsMasterProcessModel: BindableBase, nspCFPInfrastructures.IMasterProCmdService
    {
        public string strProgramVer = "1.000";

        //
        public string GetProtectionCode()
        {
            string strRet = "";
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            string strFilePath = fileinfo.FullName;

            //Calculate Protection ID code
            strRet = this.GetSingleFileInfo(strFilePath);
            
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

        public string GetProgramListProtectionCode()
        {
            string strRet = "";
            //
            strRet = this.clsMasterProgList.GetProtectCode();
            //
            return strRet;
        }

        //************************FOR CHILD CONTROL PROCESS COMMUNICATION***********************************************************
        //Well, anyway, master process need to talking with child process control. We do it through shared service!
        //Note that we do it by "Lazy" type, to prevent crash when all part initialize at the same time!
        [Import(typeof(nspCFPInfrastructures.IChildProCmdService))]
        Lazy<nspCFPInfrastructures.IChildProCmdService> ChildControlCommand;

        //Prepare for Unify Model
        public classCommonMethod clsUnifyModel { get; set; }

        //For extract Child Control model object
        public object objChildControlModel { get; set; }
        public bool blFoundChildControlModel { get; set; }

        //For User Utilities support
        public List<clsUserUtility> lstclsUserUlt { get; set; }

       //For Master sequence testing
        public classSequenceTestData clsSeqenceTestData { get; set; }
        public bool blActiveTestingSequence { get; set; }

        //*********************************************************
        public object UserUtlSingleFuncExecute(int PartID, List<List<object>> lstlstobjInput)
        {
            object objTempResult = new object();
            var lstlststrOutput = new List<List<object>>();

            //execute function
            objTempResult = this.clsMasterExtension.lstPluginCollection[PartID].Value.IFunctionExecute(lstlstobjInput, out lstlststrOutput);
            
            return objTempResult;
        }

        //Suppport binding
        public clsBindingSupport clsBindingView { get; set; }

        //**********************************************************
        public object MasterProcessCommand(List<List<object>> lstlstobjCommand, out List<List<object>> lstlstobjReturn)
        {
            lstlstobjReturn = new List<List<object>>();
            object objRet = new object();

            int i = 0;
            for (i = 0; i < lstlstobjCommand.Count; i++)
            {
                if (lstlstobjCommand[i].Count == 0) continue;

                if (lstlstobjCommand[i][0].ToString() == "START")
                {
                    //Allow Mater Process start
                    StartMasterProcess();
                    objRet = "0"; //OK Code
                }
                else if(lstlstobjCommand[i][0].ToString() == "SHUTDOWN")
                {
                    //Start shutdown process
                    ShutdownMasterProcess();
                }
                else if (lstlstobjCommand[i][0].ToString() == "GetObject")
                {
                    objRet = this; //Return master process instance itself
                }
                else if (lstlstobjCommand[i][0].ToString() == "GetMasterProgramListDataTable")
                {
                    objRet = this.clsMasterProgList.MyDataTable;
                }
                else if (lstlstobjCommand[i][0].ToString() == "GETINFO")
                {
                    //Get Info
                    objRet = GetMasterVersionInfo();
                }
                else if (lstlstobjCommand[i][0].ToString() == "GetMasterPLVersion")
                {
                    objRet = GetMasterPLVersion();
                }
                else if(lstlstobjCommand[i][0].ToString() == "SEQUENCETESTER")
                {
                    objRet = RequestMasterSequenceTester();
                }
                else if (lstlstobjCommand[i][0].ToString() == "CHECKINGMODE")
                {
                    //objRet = RequestMasterSequenceTester();
                    //Inform to Master Process what Checking Mode is selected
                    if(lstlstobjCommand[i].Count>=2)
                    {
                        this.strSystemCheckingMode = lstlstobjCommand[i][1].ToString();
                    }
                }
                else if (lstlstobjCommand[i][0].ToString() == "SystemCommandAnswer")
                {
                    if (lstlstobjCommand[i].Count >= 2)
                    {
                        objRet = this.RequestMasterSystemCommandAnswer(lstlstobjCommand[0][1].ToString());
                    }
                }
                else if (lstlstobjCommand[i][0].ToString() == "GETPROTECTCODE")
                {
                    //
                    objRet = this.GetProtectionCode();
                }
                else if (lstlstobjCommand[i][0].ToString() == "PROGRAMLISTCODE")
                {
                    //
                    objRet = this.GetProgramListProtectionCode();
                }
                else if (lstlstobjCommand[i][0].ToString() == "ChangeProgramList") //ChangeProgramList
                {
                    this.SelectMasterProgramList();
                }
            }

            return objRet;
        }
        //**********************************************************
        public object GetMasterVersionInfo()
        {
            string strRet = "";

            strRet =  "Tool Version: " + this.strProgramVer + "\r\n" +
                      "HashCode: " + this.GetProtectionCode() + "\r\n" +
                      "Date Created: " + this.strProgramDateCreated() + "\r\n" +
                      "*****\r\n" +
                      "Program List Name: " + this.clsMasterProgList.strProgramListname + "\r\n" +
                      "Program List Version: " + this.clsMasterProgList.strProgramListVersion + "\r\n" +
                      "PL Protect Code: " + this.clsMasterProgList.GetProtectCode() + "\r\n" +
                      "Date Created: " + this.clsMasterProgList.strProgramListDateCreated + "\r\n";

            return strRet;
        }

        public string GetMasterPLVersion()
        {
            return this.clsMasterProgList.strProgramListVersion;
        }

        //**********************************************************
        public string RequestMasterSequenceTester()
        {
            string strRet = "";

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
            if (blAllowChange == false) return "Do not allow to enter!";


            //Show window for Sequence Tester
            Views.wdMasterSequenceTest wdMasterTester = new Views.wdMasterSequenceTest(this);
            wdMasterTester.Show();

            //
            this.blActiveTestingSequence = true;

            //
            return strRet;
        }
        //**********************************************************
        public object RequestMasterSystemCommandAnswer(string strUserRequest)
        {
            string strRet = "";
            //
            strUserRequest = strUserRequest.Trim();
            //Separate user input to command & prameter
            List<string> lstSystemCommand = new List<string>(strUserRequest.Split(' '));

            if (lstSystemCommand.Count == 0)
            {
                return "Error: Cannot find any command!";
            }

            //Find Process Target of command
            string strTargetCommand = lstSystemCommand[0].ToUpper().Trim();
            if (strTargetCommand.IndexOf('M')!= 0) //Master Process
            {
                return "Error: Master process target command must be start with 'M'";
            }

            if (strTargetCommand.Length>1)
            {
                return "Error: Master process target command must be start with 'M' and only this character. [" + strTargetCommand + "] is invalid!";
            }

            if (lstSystemCommand.Count < 2)
            {
                return "Error: Cannot find any command!";
            }

            string strCommand = lstSystemCommand[1];


            //1. For Asking Detail result checking. Ex: "C0 ?15" => Asking detail result of step 15 of Child Process ID 0.
            if (strCommand.IndexOf('?') == 0)
            {
                //Searching step number
                string strStepNumber = strCommand.Replace("?", "").Trim();
                int intStepNum = 0;
                if (int.TryParse(strStepNumber, out intStepNum) == false) return "? command has invalid step number input!\r\nCorrect command syntax, ex: \"C0 ?15\" (Asking detail result of step 15 of Child Process ID 0)";
                strRet = this.MasterStepResultDetail(intStepNum);
            }
            else if (strCommand.IndexOf('=') == 0)
            {
                try
                {
                    int intEqualSignPos = strUserRequest.IndexOf('=');
                    string strExpression = strUserRequest.Substring(intEqualSignPos + 1, strUserRequest.Length - intEqualSignPos - 1).Trim();
                    strRet = this.strChildExpressionEval(strExpression);
                }
                catch (Exception ex)
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
        public object GetMasterProtectCode()
        {
            string strRet = "";
            //
            strRet = this.GetProtectionCode();
            //
            return strRet;
        }


        //**********************************************************
        public void SelectMasterProgramList()
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
                nspProgramList.classProgramList clsNewProgramList = new classProgramList(dlg.FileName, this.clsMasterSetting.strProgramListSheetName, ref strRet);
                if (strRet != "0")
                {
                    MessageBox.Show("Change Master Program List Fail. Error message: [" + strRet + "]", "SelectMasterProgramList() Error");
                    return;
                }

                //Save Program List
                long lngret = 0;
                string SystemIniPath = Application.StartupPath + "\\" + "SystemIni.ini";
                string strNameOfFile = Path.GetFileName(filename);
                string strNameOfFolder = Path.GetDirectoryName(filename);


                if (strNameOfFolder == Application.StartupPath) //Same folder => only need to save file name
                {
                    lngret = MyLibrary.WriteFiles.IniWriteValue(SystemIniPath, "MASTER_STEPLIST", "MasterSteplistName", strNameOfFile);
                }
                else //Not same folder => need to save full path
                {
                    lngret = MyLibrary.WriteFiles.IniWriteValue(SystemIniPath, "MASTER_STEPLIST", "MasterSteplistName", filename);
                }

                //Restart program 
                nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ResetProgram(true));
            }
        }

        //**********************************************************
        public string MasterStepResultDetail(int intStepNum)
        {
            string strRet = "";
            string strTemp = "";
            int i, j = 0;
            //

            int intStepPos = this.FindMasterStepPos(intStepNum);
            if (intStepPos == -1)
            {
                return "? command has invalid step number input!\r\nCannot find step number [" + intStepNum.ToString() + "] in Master Program list!";
            }

            //If every thing is OK. Now return checking result detail
            strRet = strRet + "*****************************************************************************************************************************" + "\r\n";
            strRet = strRet + "Here are result detail of step number [" + intStepNum.ToString() + "] of Master Process: " + "\r\n\r\n";
            strRet = strRet + "Step Position: " + intStepPos.ToString() + "\r\n";
            strRet = strRet + "is Checked: " + this.lstTotalStep[intStepPos].blStepChecked.ToString() + "\r\n";
            strRet = strRet + "Result: " + this.lstTotalStep[intStepPos].blStepResult.ToString() + "\r\n";
            strRet = strRet + "Return Data: " + this.lstTotalStep[intStepPos].objStepCheckingData.ToString() + "\r\n";
            strRet = strRet + "Tact time: " + this.lstTotalStep[intStepPos].dblStepTactTime.ToString() + " ms" + "\r\n";
            strRet = strRet + "\r\n";

            strRet = strRet + "********************************************User Return Data*****************************************************************" + "\r\n";


            for (i = 0; i < this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn.Count; i++)
            {
                for (j = 0; j < this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i].Count; j++)
                {
                    strTemp = "";

                    if (j > 0)
                    {
                        strTemp = "[" + (j - 1).ToString() + "]     ";
                    }

                    strTemp = strTemp + this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i][j].ToString();
                    //Display convert to Hexa format if possible
                    int intTemp = 0;
                    if (int.TryParse(this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[i][j].ToString().Trim(), out intTemp) == true)
                    {
                        strTemp = strTemp + "   [Hex: " + intTemp.ToString("X") + "]";
                        //ASCII convert
                        strTemp = strTemp + "   [Asc: " + Convert.ToChar(intTemp) + "]";
                    }

                    strRet = strRet + strTemp + "\r\n";
                }
                strRet = strRet + "\r\n\r\n";
            }

            strRet = strRet + "********************************************Transmission Area - original ****************************************************" + "\r\n";
            strRet = strRet + this.lstTotalStep[intStepPos].strTransmisstion + "\r\n";

            strRet = strRet + "********************************************Transmission Area - Processed****************************************************" + "\r\n";

            strRet = strRet + this.lstTotalStep[intStepPos].strTransmisstionEx + "\r\n";

            if (this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos] != null)
            {
                for (i = 0; i < this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count; i++)
                {
                    strRet = strRet + "[" + (i + 1).ToString() + "] " + this.clsCommonFunc.CalExpressionTreeResult(this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
                }
            }


            strRet = strRet + "*********************************************Parameter - Original************************************************************" + "\r\n";
            for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameter.Count; i++)
            {
                strRet = strRet + "[" + (i + 1).ToString() + "] " + this.lstTotalStep[intStepPos].lstobjParameter[i].ToString() + "\r\n";
            }

            strRet = strRet + "*********************************************Parameter - Processed***********************************************************" + "\r\n";
            strRet = strRet + "Parameter Ex: " + "\r\n";
            for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameterEx.Count; i++)
            {
                strRet = strRet + "[" + (i + 1).ToString() + "] " + this.clsCommonFunc.CalExpressionTreeResult(this.clsCommonFunc.lstlstCommonParaCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
            }

            strRet = strRet + "*********************************************Special control***********************************************************" + "\r\n";
            if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos] != null)
            {
                for (i = 0; i < this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
                {
                    strRet = strRet + "[" + (i + 1).ToString() + "] " + this.clsCommonFunc.CalExpressionTreeResult(this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].ParsedExpression, 0);
                }
            }


            //
            return strRet;
        }

        //**********************************************************
        public string strChildExpressionEval(string strExpression)
        {
            string strRet = "";

            //Cal new command guider
            List<string> lststrInput = new List<string>();
            lststrInput.Add(strExpression);
            List<List<clsCommonCommandGuider>> lstlstCommandGuider = this.clsCommonFunc.CommonSpecialControlIni(lststrInput);
            //Cal result
            strRet = lstlstCommandGuider[0][0].evaluate().ToString() + "\r\n";

            //Add more info
            strRet = strRet + "*****************************************\r\n";
            strRet = strRet + "And here are more detail\r\n";
            strRet = strRet + this.clsCommonFunc.CalExpressionTreeResult(lstlstCommandGuider[0][0].ParsedExpression, 0);

            //
            return strRet;
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

        //*********************************************************
        public object SendReadyToChildControl()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("MASTERREADY");
            lstlstobjCommand.Add(lstTemp);
            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //Sending Command
            object objRet = ChildControlCommand.Value.ChildControlCommand(lstlstobjCommand, out lstlstobjReturn);
            return objRet;
        }

        public object GetChildControlObject()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("GetObject");
            lstTemp.Add("clsChildControlModel");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            //Marking
            this.blFoundChildControlModel = true;
            this.objChildControlModel = ChildControlCommand.Value.ChildControlCommand(lstlstobjCommand, out lstlstobjReturn);

            return this.objChildControlModel;
        }

        public bool IniChildProcess()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("START");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            object objRet = new object();
            objRet = ChildControlCommand.Value.ChildControlCommand(lstlstobjCommand, out lstlstobjReturn);
            if(objRet is bool)
            {
                return (bool)objRet;
            }
            else
            {
                return false;
            }
        }

        public bool ShutdownChildProcess()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            lstTemp.Add("SHUTDOWN");
            lstlstobjCommand.Add(lstTemp);

            List<List<object>> lstlstobjReturn = new List<List<object>>();

            object objRet = new object();
            objRet = ChildControlCommand.Value.ChildControlCommand(lstlstobjCommand, out lstlstobjReturn);
            if (objRet is bool)
            {
                return (bool)objRet;
            }
            else
            {
                return false;
            }
        }

        //************************END CHILD CONTROL PROCESS COMMUNICATION***********************************************************

        //For MEF handle
        public nspMEFLoading.clsMEFLoading.clsExtensionHandle clsMasterExtension { get; set; }

        //For Master Setting File
        public clsProcessSettingReading clsMasterSetting { get; set; }

        //For saving System checking mode
        public string strSystemCheckingMode { get; set; }

        //For Result checking
        public clsItemResultInformation clsItemInfo { get; set; }

        //For control Master Process start
        public bool blAllowMasterStart { get; set; }

        //For Master Thread Running
        System.Threading.Thread thrMasterProcess { get; set; }
        public bool blRequestStop { get; set; }

        //For Background Timer
        private System.Timers.Timer tmrMainLoopTimer { get; set; } //For host Master Main Loop
        private System.Timers.Timer tmrBackGroundTimer { get; set; } //For Back Ground Timer - Run Task which require long response

        //For master process satus control
        public enumMasterStatus eMasterStatus { get; set; }
        public enumMasterStatus eLastMasterStatus { get; set; } //saving state to restore after Safety Handle finish!

        //For Token declaration
        public int intTokenCyclePoll { get; set; }
        public int intTokenEmerPoll { get; set; }
        public int intTokenSafetyPoll { get; set; }
        public int intTokenMasterMain { get; set; }
        public int intLastTokenMasterMain { get; set; } //saving state to restore after Safety Handle finish!

        public int intTokenBackGround { get; set; }

        //SUPPORT CONTROL FROM UI
        public enumMasterRunMode eMasterRunMode { get; set; } //For Master Process Running Mode
        public bool blAllowContinueRunning { get; set; } //True if button NEXT is pressed
        public int intStepModePosSelected { get; set; } //Indicate what step positon request to stop in 

        //For Master Program List
        public classProgramList clsMasterProgList { get; set; } //Program List for Master Process

        #region _DefineSubList

        //Define Sub List
        public List<classStepDataInfor> lstTotalStep = new List<classStepDataInfor>();
        public List<classStepDataInfor> lstMasterIni = new List<classStepDataInfor>(); //0
        public List<classStepDataInfor> lstMasterCyclePoll = new List<classStepDataInfor>(); //1
        public List<classStepDataInfor> lstMasterResetRequestPoll = new List<classStepDataInfor>(); //2
        public List<classStepDataInfor> lstMasterResetProcess = new List<classStepDataInfor>(); //3
        public List<classStepDataInfor> lstMasterEmerPoll = new List<classStepDataInfor>(); //4
        public List<classStepDataInfor> lstMasterEmerHandle = new List<classStepDataInfor>(); //5
        public List<classStepDataInfor> lstMasterSafetyPoll = new List<classStepDataInfor>(); //6
        public List<classStepDataInfor> lstMasterSafetyHandle = new List<classStepDataInfor>(); //7
        public List<classStepDataInfor> lstMasterPreStart = new List<classStepDataInfor>(); //8
        public List<classStepDataInfor> lstMasterStartPoll = new List<classStepDataInfor>(); //9
        public List<classStepDataInfor> lstMasterCheckingProcess = new List<classStepDataInfor>(); //10

        public List<classStepDataInfor> lstMasterAfterFinish = new List<classStepDataInfor>(); //50

        public List<classStepDataInfor> lstMasterUserEnd = new List<classStepDataInfor>();
        public List<classStepDataInfor> lstMasterBackGroundPoll = new List<classStepDataInfor>();

        #endregion

        //**********************SUPPORT DISPLAY INFO ON USER INTERFACE************************************************************
        public int intStepPosRunning { get; set; } //Indicate what step (Position) is running
        public int intBackGroundStepPosRunning { get; set; } //Indicate what step (Position) is running on Back Ground Task

        //Analyze Program List to Sub-List for control Master sequence. 
        public void AnalyzeProgramList(List<classStepDataInfor> lstInput)
        {
            int i = 0;

            if (lstInput == null) return;
            //
            lstTotalStep = lstInput;
            //Analyze all item in Program List & assign them to proper category
            for (i = 0; i < lstInput.Count; i++)
            {
                //Check if Row input belong to Main Sequence
                if (lstInput[i].intStepSequenceID != 0) //Not adding if step not belong to Main Sequence
                {
                    continue;
                }

                classStepDataInfor temp = lstInput[i];

                switch (lstInput[i].intStepClass)
                {
                    case 0: //Ini
                        lstMasterIni.Add(temp);
                        break;
                    case 1: // CyclePoll
                        lstMasterCyclePoll.Add(temp);
                        break;
                    case 2: //ResetRequestPoll
                        lstMasterResetRequestPoll.Add(temp);
                        break;
                    case 3: //ResetProcess
                        lstMasterResetProcess.Add(temp);
                        break;
                    case 4: //EmerPoll
                        lstMasterEmerPoll.Add(temp);
                        break;
                    case 5: //EmerHandle
                        lstMasterEmerHandle.Add(temp);
                        break;
                    case 6: //SafetyPoll
                        lstMasterSafetyPoll.Add(temp);
                        break;
                    case 7: //SafetyHandle
                        lstMasterSafetyHandle.Add(temp);
                        break;
                    case 8: //PreStart
                        lstMasterPreStart.Add(temp);
                        break;
                    case 9: //StartPoll
                        lstMasterStartPoll.Add(temp);
                        break;
                    case 10: //CheckingProcess
                        lstMasterCheckingProcess.Add(temp);
                        break;
                    case 50: //AfterFinish
                        lstMasterAfterFinish.Add(temp);
                        break;

                    case 100: //Special, not follow order.
                        lstMasterBackGroundPoll.Add(temp);
                        break;
                    case 1000: //UserEnd
                        lstMasterUserEnd.Add(temp);
                        break;
                    default:
                        MessageBox.Show("Error: Cannot recognize Master Step Class [" + lstInput[i].intStepClass.ToString() + "]");
                        break;
                }
            }
        }

        /////FOR BINDING - WPF/////////////////////////////

        public Dictionary<string, enumMasterRunMode> dictMasterRunMode;
        public System.Data.DataTable ViewTable;

        //Create View Table
        public void ViewTableIni()
        {
            //ViewTable = clsMasterModel.clsMasterProgList.MyDataTable;
            ViewTable = new System.Data.DataTable();

            //Create column for data table
            ViewTable.Columns.Add("Number"); //0
            ViewTable.Columns.Add("TestName"); //1
            ViewTable.Columns.Add("Result"); //2
            ViewTable.Columns.Add("LoLimit"); //3
            ViewTable.Columns.Add("HiLimit"); //4
            ViewTable.Columns.Add("Unit"); //5
            ViewTable.Columns.Add("FuncID"); //6
            ViewTable.Columns.Add("Comment"); //7
            ViewTable.Columns.Add("Notes"); //8

            ViewTable.Columns.Add("ActiveColor", typeof(System.Windows.Media.SolidColorBrush)); //9
            ViewTable.Columns.Add("ResultColor", typeof(System.Windows.Media.SolidColorBrush));//10

            int i = 0;
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                DataRow temp = ViewTable.NewRow();

                //Check if Row is blank or User Function name area
                if(this.lstTotalStep[i].intStepSequenceID==-1) //No caring sequence class
                {
                    if((i+1)<this.lstTotalStep.Count)
                    {
                        if(this.lstTotalStep[i+1].intStepSequenceID==1) //User Function Name
                        {
                            temp[0] = "FUNC";
                            temp[1] = this.lstTotalStep[i+1].strUserFunctionName;
                        }
                    }

                    ViewTable.Rows.Add(temp);
                    continue;
                }


                temp[0] = this.lstTotalStep[i].intStepNumber.ToString();
                temp[1] = this.lstTotalStep[i].strStepName;
                temp[2] = "";

                if (this.lstTotalStep[i].strUnitName.ToUpper().Trim()=="H")
                {
                    temp[3] = Convert.ToInt32(this.lstTotalStep[i].objLoLimit.ToString()).ToString("X");
                    temp[4] = Convert.ToInt32(this.lstTotalStep[i].objHiLimit).ToString("X");
                }
                else
                {
                    temp[3] = this.lstTotalStep[i].objLoLimit.ToString();
                    temp[4] = this.lstTotalStep[i].objHiLimit.ToString();
                }
                temp[5] = this.lstTotalStep[i].strUnitName;


                temp[6] = this.lstTotalStep[i].intStepClass.ToString() + "-" +
                            this.lstTotalStep[i].intJigId.ToString() + "-" +
                            this.lstTotalStep[i].intHardwareId.ToString() + "-" +
                            this.lstTotalStep[i].intFunctionId.ToString();

                //temp[9] = "0";

                ViewTable.Rows.Add(temp);
            }
        }

        //////////////////////////////////////////////////////////////////
        private static Mutex mut = new Mutex();

        public void UpdateViewTable(int intStepPos)
        {
            //intProcess = 0: Main Loop
            //intProcess = 1: Background

            if (this.lstTotalStep[intStepPos].intStepSequenceID == -1) return;

            //protect with mutex
            mut.WaitOne();

            //
            int i = 0;

            //Error trap
            if (this.ViewTable == null) return;

            //Set color for data view
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                if (this.lstTotalStep[intStepPos].intStepClass != 100) //Main Loop
                {
                    if (i == intStepPos)
                    {
                        this.ViewTable.Rows[i][9] = System.Windows.Media.Brushes.LightCoral;
                    }
                    else
                    {
                        if (this.lstTotalStep[i].intStepClass != 100) //No reset color of background step
                        {
                            this.ViewTable.Rows[i][9] = SetColor(i);
                        }
                    }
                }
                else //Background
                {
                    if (i == intStepPos)
                    {
                        this.ViewTable.Rows[i][9] = System.Windows.Media.Brushes.Orange;
                    }
                    else
                    {
                        if (this.lstTotalStep[i].intStepClass == 100) //Only reset color of background step
                        {
                            this.ViewTable.Rows[i][9] = SetColor(i);
                        } 
                    }
                }
            }

            //Set result checking
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                this.ViewTable.Rows[i][2] = this.GetConvertDataResult(i);
            }

            //Set result checking color
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                if (this.lstTotalStep[i].blStepChecked == true)
                {
                    if (this.lstTotalStep[i].blStepResult == true) //PASS
                    {
                        this.ViewTable.Rows[i][10] = System.Windows.Media.Brushes.Blue;
                    }
                    else
                    {
                        this.ViewTable.Rows[i][10] = System.Windows.Media.Brushes.Red;
                    }
                }
            }

            //Release mutex
            mut.ReleaseMutex();

        }

        //////////////////////////////////////////////////////////////////
        public string GetConvertDataResult(int intStepPos)
        {
            //Check if necessary convert result to hexa format and return
            string strRet = "";
            double dblTemp = 0;
            //
            if (this.lstTotalStep[intStepPos].blStepChecked == false) return "";
            if (this.lstTotalStep[intStepPos].objStepCheckingData == null) return "null";
            if (this.lstTotalStep[intStepPos].objStepCheckingData == null) return "null";
            //Convert to string
            strRet = this.lstTotalStep[intStepPos].objStepCheckingData.ToString();
            //Check if need & can convert to hexa format
            if (this.lstTotalStep[intStepPos].strUnitName.ToUpper().Trim() == "H")
            {
                if (double.TryParse(strRet, out dblTemp) == false)
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


        //////////////////////////////////////////////////////////////////
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
        //////////////////////////////////////////////////////////////////
        public void SetMasterRunMode(string strMode)
        {
            if (this.dictMasterRunMode.ContainsKey(strMode))
            {
                this.eMasterRunMode = this.dictMasterRunMode[strMode];

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
                                string strRet = SetStepModeNumber(intTemp);
                                if (strRet != "0")
                                {
                                    MessageBox.Show(strRet, "Input Error");
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
                            break;
                        }
                    }
                }
            }
        }

        public string SetStepModeNumber(int intInput)
        {
            //Find step position want to stop
            int intPos = this.FindMasterStepPos(intInput);
            if (intPos == -1)
            {
                return "Error: The step [" + intInput.ToString() + "] not exist in Master Program List!";
            }

            //If everything is OK, then set all child process to stop at desired position
            this.intStepModePosSelected = intPos;

            return "0"; //OK code
        }

        //////////////////////////////////////////////////////////////////
        public List<string> GetMaserRunModeList()
        {
            List<string> lststrRet = new List<string>();
            foreach (var search in this.dictMasterRunMode)
            {
                lststrRet.Add(search.Key);
            }
            return lststrRet;
        }

        //////////////////////////////////////////////////////////////////
        public void StartMasterProcess()
        {
            string strTemp = "";
            bool blResult = false;
            //Do ini for master process
            strTemp = MasterProcessIniStepEx(this.lstMasterIni);

            if (strTemp != "0") //Ini process fail
            {
                MessageBox.Show(strTemp, "Master Process Ini Fail! Program cannot running!");
                //Environment.Exit(0);
                return;
            }
            else //Ini OK
            {
                this.blAllowMasterStart = true;
                //Do ini for child process
                blResult = IniChildProcess();
                if(blResult==false)
                {
                    MessageBox.Show("Child Process ini fail! Program cannot running!", "Child Process ini fail!");
                    return;
                }
            }

            //Start timer main loop
            MasterProcessMainLoopStart();
            //Start background timer
            BackGroundProcessStart();

        }
        //////////////////////////////////////////////////////////////////
        public void ShutdownMasterProcess()
        {
            //Shutdown child process
            bool blTemp = this.ShutdownChildProcess();
            if (blTemp == false) MessageBox.Show("Warning: Child process shutdown fail!","Shutdown process fail");

            //Stop master process main loop timer
            this.tmrBackGroundTimer.Enabled = false;
            this.tmrMainLoopTimer.Enabled = false;

            //Execute user end steps
            string strTemp = "";
            strTemp = this.MasterProcessUserEndStepEx();
            if (strTemp != "0")
            {
                MessageBox.Show(strTemp,"Master shutdown process fail!");
            }
        }

        //////////////////////////////////////////////////////////////////
        public void MasterMainLoop()
        {
            //0. Check if start command is OK or not
            if (blAllowMasterStart == false) return;

            //1. Always allow Cycle Polling
            MasterSequenceControl(enumMasterStatus.eMasterCyclePoll, this.intTokenCyclePoll);

            //2. Always allow Emer Polling
            MasterSequenceControl(enumMasterStatus.eMasterEmerPoll, this.intTokenEmerPoll);

            //3. Always allow Safety Condition Polling
            MasterSequenceControl(enumMasterStatus.eMasterSafetyPoll, this.intTokenSafetyPoll);

            //4. Depend on each case, select 1 process to running
            MasterSequenceControl(this.eMasterStatus, this.intTokenMasterMain);
        }

        //////////////////////////////////////////////////////////////////
        public void BackGroundMainLoop()
        {
            MasterSequenceControl(enumMasterStatus.eMasterBackGroundPoll, this.intTokenBackGround);
        }

        //////////////////////////////////////////////////////////////////
        public void MasterSequenceControl(enumMasterStatus eStatus, int intToken)
        {
            List<classStepDataInfor> lstExcelStep = FindMasterStepListClass(eStatus);

            #region _handleNoStep

            //If there is no step in class, then return to another status
            if (lstExcelStep.Count == 0)
            {
                switch (eStatus)
                {
                    case enumMasterStatus.eMasterResetRequestPoll: // 2
                        //Turn to PreStart Process
                        this.eMasterStatus = enumMasterStatus.eMasterPreStart;
                        break;

                    case enumMasterStatus.eMasterResetProcess: // 3
                        //Turn to PreStart Process
                        this.eMasterStatus = enumMasterStatus.eMasterPreStart;
                        break;

                    case enumMasterStatus.eMasterEmerHandle: // 5
                        //Turn to Reset Request Polling
                        this.eMasterStatus = enumMasterStatus.eMasterResetRequestPoll;
                        break;

                    case enumMasterStatus.eMasterSafetyHandle: // 7
                        //Turn to Checking Process
                        this.eMasterStatus = enumMasterStatus.eMasterCheckingProcess;
                        break;

                    case enumMasterStatus.eMasterCheckingProcess: // 8
                        //Turn to start polling
                        this.eMasterStatus = enumMasterStatus.eMasterStartPoll;
                        break;

                    default:
                        break;
                }

                //Always return
                return;
            }

            #endregion

            #region _HandleChecking

            //Find the position of step in Master step list
            int intStepPos = lstExcelStep[intToken].intStepPos;

            if (eStatus != enumMasterStatus.eMasterBackGroundPoll) //ForeGround of master process
            {
                this.intStepPosRunning = intStepPos;
            }
            else //Background of master process
            {
                this.intBackGroundStepPosRunning = intStepPos;
            }

            //Do function execute
            bool blStepResult = MasterFuncEx(intStepPos);

            #endregion

            #region _HandleClassRelation

            //Depend on each class - Do something necessary
            switch (eStatus)
            {
                case enumMasterStatus.eMasterCyclePoll: // 1
                    //No care about result - Just increase for next step
                    intToken++;
                    //Process finish when reach final step
                    if (intToken == lstExcelStep.Count)
                    {
                        intToken = 0; //Reset for next time
                    }
                    //Assign directly Token value for next time running
                    this.intTokenCyclePoll = intToken;
                    break;

                case enumMasterStatus.eMasterBackGroundPoll: // 100
                    
                    //No care about result - Just increase for next step - RUNNING ON BACKGROUND

                    intToken++;
                    //Process finish when reach final step
                    if (intToken == lstExcelStep.Count)
                    {
                        intToken = 0; //Reset for next time
                    }
                    //Assign directly Token value for next time running
                    this.intTokenBackGround = intToken;

                    break;

                case enumMasterStatus.eMasterResetRequestPoll: // 2

                    //If one step OK - immidiately return value
                    if (blStepResult == true)
                    {
                        this.eMasterStatus = enumMasterStatus.eMasterResetProcess;
                        intToken = 0; //Reset for Reset Process
                    }
                    else
                    {
                        //Increase for next step
                        intToken++;
                        //Process finish when reach final step
                        if (intToken == lstExcelStep.Count)
                        {
                            intToken = 0; //Reset for next time
                        }
                    }
                    this.intTokenMasterMain = intToken;

                    break;

                case enumMasterStatus.eMasterResetProcess: // 3

                    if (blStepResult == true) //If 1 step is OK, then increase token and goto next step
                    {
                        //Increase for next step
                        intToken++;
                    }

                    //Confirm Finish?
                    if (intToken == lstExcelStep.Count)
                    {
                        //Turn to Pre-Start process
                        this.eMasterStatus = enumMasterStatus.eMasterPreStart;
                        intToken = 0; //Reset for next time
                    }

                    this.intTokenMasterMain = intToken;
                    break;

                case enumMasterStatus.eMasterEmerPoll: // 4
                    //If one step OK - immidiately return value & turn to Emer handle
                    if (blStepResult == true)
                    {
                        this.eMasterStatus = enumMasterStatus.eMasterEmerHandle; //Turn to Emer handle
                        this.intTokenMasterMain = 0; //reset main token
                    }
                    else //No emer detect
                    {
                        //Increase for next step
                        intToken++;
                        //Process finish when reach final step
                        if (intToken == lstExcelStep.Count)
                        {
                            intToken = 0; //Reset for next time
                        }
                    }
                    this.intTokenEmerPoll = intToken;
                    break;

                case enumMasterStatus.eMasterEmerHandle: // 5

                    if (blStepResult == true) //If ok, then increase token and goto next step in Emer handle class
                    {
                        //Increase for next step
                        intToken++;
                    }

                    if (intToken == lstExcelStep.Count) //Finish Emer handle process => Turn to reset request polling
                    {
                        this.eMasterStatus = enumMasterStatus.eMasterResetRequestPoll;
                        intToken = 0; //Reset for ResetRequestPoll
                    }

                    //strcMainVar.intSystemControlMasterToken = intToken;
                    this.intTokenMasterMain = intToken;
                    break;

                case enumMasterStatus.eMasterSafetyPoll: // 6
                    //If one step OK - immidiately return value & turn to Safety condition handle
                    if (blStepResult == true)
                    {
                        //Reset some variables here!
                        this.eMasterStatus = enumMasterStatus.eMasterSafetyHandle;
                        this.intTokenMasterMain = 0; //Reset for safety handle process
                    }
                    else
                    {
                        //Increase for next step
                        intToken++;
                        //Process finish when reach final step
                        if (intToken == lstExcelStep.Count)
                        {
                            intToken = 0; //Reset for next time
                        }
                    }

                    //strcMainVar.intSystemControlSafetyPollingToken = intToken;
                    this.intTokenSafetyPoll = intToken;
                    break;

                case enumMasterStatus.eMasterSafetyHandle: // 7
                    if (blStepResult == true) //If result is OK, then goto next step in Safety handle class
                    {
                        //MainMessageBot("Safety Handle Process is running...");
                        //Increase for next step
                        intToken++;
                    }

                    //If complete all class, Reset to before safety handle state!!!
                    if (intToken == lstExcelStep.Count) 
                    {
                        //intToken = 0; //Reset safety handle token for next time
                        //this.eMasterStatus = enumMasterStatus.eMasterCheckingProcess;

                        this.eMasterStatus = this.eLastMasterStatus;
                        this.intTokenMasterMain = this.intLastTokenMasterMain;
                    }
                    else
                    {
                        //Increase token for next step
                        this.intTokenMasterMain = intToken;
                    }
                    break;

                case enumMasterStatus.eMasterPreStart: // 8

                    if (blStepResult == true)
                    {
                        //Increase for next step
                        intToken++;
                    }

                    if (intToken == lstExcelStep.Count) //If finish all step in class, turn to Start Polling
                    {
                        intToken = 0; //Reset for next time
                        //strcMainVar.intSystemControl = intSystemControlStartPolling;
                        this.eMasterStatus = enumMasterStatus.eMasterStartPoll;
                    }
                    //strcMainVar.intSystemControlMasterToken = intToken;
                    this.intTokenMasterMain = intToken;
                    break;

                case enumMasterStatus.eMasterStartPoll: // 9
                    //If one step OK - immidiately return value & turn to checking process
                    if (blStepResult == true)
                    {
                        //strcMainVar.intSystemControl = intSystemControlCheckingProcess;
                        this.eMasterStatus = enumMasterStatus.eMasterCheckingProcess;
                        intToken = 0; //Reset for Checking Process test class
                    }
                    else
                    {
                        //Increase for next step
                        intToken++;
                        //Process finish when reach final step
                        if (intToken == lstExcelStep.Count)
                        {
                            intToken = 0; //Reset for next time
                        }
                    }
                    this.intTokenMasterMain = intToken;
                    break;

                case enumMasterStatus.eMasterCheckingProcess: // 10
                    if (blStepResult == true)
                    {
                        //Increase for next step
                        intToken++;
                    }

                    if (intToken == lstExcelStep.Count) //Finish all. Turn to Pre-Start Process
                    {
                        intToken = 0; //Reset for next time
                        //Marking checking time
                        this.clsItemInfo.dateTimeChecking = DateTime.Now;

                        this.eMasterStatus = enumMasterStatus.eMasterPreStart;
                    }
                    this.intTokenMasterMain = intToken;
                    break;

                default:
                    //MessageBox.Show("NG");
                    break;
            }

            #endregion

            //***********************************************************************************************************
            //Do Special Control Command here, after finish step!!!
            this.MasterSpecialControlCommandEx(intStepPos);

            #region _HandleRunMode

            if (eStatus != enumMasterStatus.eMasterBackGroundPoll) //ForeGround of master process
            {
                //Safety Handle - Save the last state & token to restore system status after safety Handle process finish
                if (eStatus != enumMasterStatus.eMasterCyclePoll) //Do not save Cycle polling state
                {
                    if (eStatus != enumMasterStatus.eMasterEmerPoll) //Do not save Emer polling state
                    {
                        if (eStatus != enumMasterStatus.eMasterEmerHandle) //Do not save Emer Handle state
                        {
                            if (eStatus != enumMasterStatus.eMasterSafetyPoll) //Do not save safety polling state
                            {
                                if (eStatus != enumMasterStatus.eMasterSafetyHandle) //Do not save safety handle state
                                {
                                    this.eLastMasterStatus = eStatus;
                                    this.intLastTokenMasterMain = this.intTokenMasterMain;
                                }
                            }
                        }
                    }
                }
                

                //Handle Master running mode
                switch (eMasterRunMode)
                {
                    case enumMasterRunMode.eMasterNormal: //Running normally
                        break;

                    case enumMasterRunMode.eMasterSingleAll: //Running each step & wait for allowing signal (button press) for continue

                        while (this.blAllowContinueRunning == false)  //Waitting button press
                        {
                            //Application.DoEvents();
                        }
                        this.blAllowContinueRunning = false; //Reset for next time
                        break;

                    case enumMasterRunMode.eMasterSingleClass: //Running Single mode in steps of some selected class
                        break;

                    case enumMasterRunMode.eMasterStep: //Running until reach desired step, continue if allowing condition met (button press)

                        if (intStepPos == this.intStepModePosSelected)
                        {
                            while (this.blAllowContinueRunning == false)  //Waitting button press
                            {
                                //Application.DoEvents();
                            }
                            this.blAllowContinueRunning = false; //Reset for next time

                        }
                        break;

                    default:
                        break;
                }
            }
            else //Background of master process
            {
                ;
            }

            #endregion
        }

        //*********************************************************
        public List<classStepDataInfor> FindMasterStepListClass(enumMasterStatus eStatus)
        {

            List<classStepDataInfor> lstExcelStep = new List<classStepDataInfor>();

            //Select Master Excel class
            switch (eStatus)
            {
                case enumMasterStatus.eMasterIni: // 0
                    lstExcelStep = this.lstMasterIni;
                    break;

                case enumMasterStatus.eMasterCyclePoll: // 1
                    lstExcelStep = this.lstMasterCyclePoll;
                    break;


                case enumMasterStatus.eMasterResetRequestPoll: // 2
                    lstExcelStep = this.lstMasterResetRequestPoll;
                    break;

                case enumMasterStatus.eMasterResetProcess: // 3
                    lstExcelStep = this.lstMasterResetProcess;
                    break;

                case enumMasterStatus.eMasterEmerPoll: // 4
                    lstExcelStep = this.lstMasterEmerPoll;
                    break;

                case enumMasterStatus.eMasterEmerHandle: // 5
                    lstExcelStep = this.lstMasterEmerHandle;
                    break;

                case enumMasterStatus.eMasterSafetyPoll: // 6
                    lstExcelStep = this.lstMasterSafetyPoll;
                    break;

                case enumMasterStatus.eMasterSafetyHandle: // 7
                    lstExcelStep = this.lstMasterSafetyHandle;
                    break;
                case enumMasterStatus.eMasterPreStart: // 8
                    lstExcelStep = this.lstMasterPreStart;
                    break;

                case enumMasterStatus.eMasterStartPoll: // 9
                    lstExcelStep = this.lstMasterStartPoll;
                    break;

                case enumMasterStatus.eMasterCheckingProcess: // 10
                    lstExcelStep = this.lstMasterCheckingProcess;
                    break;

                case enumMasterStatus.eMasterUserEnd: // 11
                    lstExcelStep = this.lstMasterUserEnd;
                    break;

                case enumMasterStatus.eMasterBackGroundPoll: // 100
                    lstExcelStep = this.lstMasterBackGroundPoll;
                    break;

                default:
                    break;
            }

            return lstExcelStep;
        }

        //////////////////////////////////////////////////////////////////
        public void MasterReadingSetting()
        {
            this.clsMasterSetting = new clsProcessSettingReading();
            //////////////////////////////////////////////////////

            string strTmp = "";

            //1. Check if file is exist or not
            string strAppPath = "";
            string iniFileName = @"\SystemIni.ini";
            string strFileName = "";

            strAppPath = Application.StartupPath;
            strFileName = strAppPath + iniFileName;

            if (MyLibrary.ChkExist.CheckFileExist(strFileName) == false)
            {
                MessageBox.Show("'SystemIni.ini' file does not exist! Please check!", "File not exist");
                System.Environment.Exit(0);
            }
            //2. Reading steplist info contents
            //Master Step list name config
            strTmp = MyLibrary.ReadFiles.IniReadValue("MASTER_STEPLIST", "MasterSteplistName", strFileName);
            if (strTmp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'MasterSteplistName' config in 'MASTER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            this.clsMasterSetting.strProgramListFileName = Application.StartupPath + "\\" + strTmp; //For full file name

            //Master Sheet name config
            strTmp = MyLibrary.ReadFiles.IniReadValue("MASTER_STEPLIST", "MasterSheetName", strFileName);
            if (strTmp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'MasterSheetName' config in 'MASTER_STEPLIST' of System.ini file!", "ReadSystemIniFile()");
                Environment.Exit(0);
            }
            this.clsMasterSetting.strProgramListSheetName = strTmp;

            //
            //3. Reading checker Info config

            //clsTemp.clsChildSetting.intNumChecker = clsMainVar.intNumChildPro;
            //clsTemp.clsChildSetting.intNumRow = clsMainVar.intNumRow;
            //clsTemp.clsChildSetting.intNumCol = clsMainVar.intNumCol;
            //clsTemp.clsChildSetting.intAllignMode = clsMainVar.intAllignMode;
            //clsTemp.clsChildSetting.intRoundShapeMode = clsMainVar.intRoundShapeMode;
            //clsTemp.clsChildSetting.intOrgPosition = clsMainVar.intOrgPosition;


            string strTemp = "";
            int intTemp = 0;

            strTemp = MyLibrary.ReadFiles.IniReadValue("CHECKER_INFO", "Number_Checker", strFileName);
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

            this.clsMasterSetting.intNumChecker = intTemp;
            //

            //Number of Row
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "NumRow", strFileName);
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
            this.clsMasterSetting.intNumRow = int.Parse(strTemp);

            //Number of Collumn
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "NumCol", strFileName);
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
            this.clsMasterSetting.intNumCol = int.Parse(strTemp);

            //
            //Allign Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "AllignMode", strFileName);
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
            this.clsMasterSetting.intAllignMode = int.Parse(strTemp);
            if ((this.clsMasterSetting.intAllignMode != 0) && (this.clsMasterSetting.intAllignMode != 1))
            {
                this.clsMasterSetting.intAllignMode = 0; //Default value
            }

            //Rounding Shape Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "RoundShapeMode", strFileName);
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
            this.clsMasterSetting.intRoundShapeMode = int.Parse(strTemp);
            if ((this.clsMasterSetting.intRoundShapeMode != 0) && (this.clsMasterSetting.intRoundShapeMode != 1))
            {
                this.clsMasterSetting.intRoundShapeMode = 0; //Default value => zig-zag Shape
            }

            //Rounding OrgPosition  Mode
            strTemp = MyLibrary.ReadFiles.IniReadValue("DISPLAY_SETTING", "OrgPosition", strFileName);
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
            this.clsMasterSetting.intOrgPosition = int.Parse(strTemp);
            if ((this.clsMasterSetting.intOrgPosition < 0) && (this.clsMasterSetting.intOrgPosition > 3))
            {
                this.clsMasterSetting.intOrgPosition = 0; //Default value => zig-zag Shape
            }



        }
        //////////////////////////////////////////////////////////////////
        public void MasterProcessModelIni()
        {
            int i = 0;
            string strTemp = "";

            //0. Reading Master setting file
            MasterReadingSetting();

            //Ini for Program List
            string strRet = "";
            clsMasterProgList = new classProgramList(this.clsMasterSetting.strProgramListFileName, this.clsMasterSetting.strProgramListSheetName,ref strRet);

            if(strRet!="0") //Loading Fail
            {
                MessageBox.Show(strRet, "MasterProcessModelIni() Error");
            }

            //Analyze Program List & separate to Sub List
            AnalyzeProgramList(this.clsMasterProgList.lstProgramList);

            //1. Loading Plug-in for Master Process
            clsMasterExtension.PluginLoader(this.lstTotalStep, "Extensions");


            //2. Ini for Master special control
            this.clsCommonFunc.intSourcesID = 1;
            this.clsCommonFunc.objSources = this;
            this.clsCommonFunc.intProcessId = -1;

            this.clsCommonFunc.lstlstCommonCommandAnalyzer = this.clsCommonFunc.CommonSpecialControlIni(this.clsMasterProgList.lststrSpecialCmd);
            this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer = this.clsCommonFunc.CommonTransAreaSpecialControlIni(this.clsMasterProgList.lststrTransAreaSpecialCmd);
            this.clsCommonFunc.lstlstCommonParaCommandAnalyzer = this.clsCommonFunc.CommonParaSpecicalControlIni(this.clsMasterProgList.lstlstobjStepPara);

            //Ini for Result class
            this.clsItemInfo = new clsItemResultInformation();

            //3. Running step in Master Program List which belong to Ini Class
            //strTemp = MasterProcessIniStepEx(this.lstMasterIni);

            //if (strTemp != "0") //Ini process fail
            //{
            //    MessageBox.Show(strTemp, "Master Process Ini Fail!");
            //    Environment.Exit(0);
            //}

            //Ini for some variable
            eMasterRunMode = enumMasterRunMode.eMasterNormal;
            this.intStepModePosSelected = -1;
            this.intStepPosRunning = -1;
            this.intBackGroundStepPosRunning = -1;

            //Ini for view table
            ViewTableIni();

            //Ini for Master Sequence testing
            this.clsSeqenceTestData = new classSequenceTestData();
            this.clsSeqenceTestData.lstblForceChange = new List<bool>();
            this.clsSeqenceTestData.lstobjNewValue = new List<object>();
            bool blTemp = false;
            object objTemp = new object();
            for (i = 0; i < this.lstTotalStep.Count;i++)
            {
                blTemp = false;
                objTemp = new object();
                //
                this.clsSeqenceTestData.lstblForceChange.Add(blTemp);
                this.clsSeqenceTestData.lstobjNewValue.Add(objTemp);
            }

            //OK. Now prepare for Master Process start up with reset request polling
            this.eMasterStatus = enumMasterStatus.eMasterResetRequestPoll;

            //Inform to Child Process Control that Master Process has finish ini
            this.SendReadyToChildControl();


            //For debugging
            Debug.WriteLine("Master Process Ini Finish!");
        }

        public string MasterProcessIniStepEx(List<classStepDataInfor> lstInput)
        {
            int intStepPos = 0;

            int i = 0;
            for (i = 0; i < lstInput.Count; i++)
            {
                //Find the position of step in Master step list
                intStepPos = lstInput[i].intStepPos;
                this.lstTotalStep[intStepPos].blStepResult = false; //Clear data

                //Set color for data view
                UpdateViewTable(intStepPos);

                //Execute step
                bool blStepResult = MasterFuncEx(intStepPos);
                //Do Special Control Command here, after finish step!!!
                this.MasterSpecialControlCommandEx(intStepPos);

                if (blStepResult == false) //Step result is NG - return immediately
                {
                    return "Master Ini fail at step [" + lstInput[i].intStepNumber.ToString() + "] . Return data: [" + this.lstTotalStep[intStepPos].objStepCheckingData.ToString() + "]. Please check & restart program!";
                }
            }

            return "0"; //Return OK string code if everything is OK
        }

        public string MasterProcessUserEndStepEx()
        {
            if (this.lstMasterUserEnd.Count == 0) return "0";

            int intStepPos = 0;

            int i = 0;
            string strResult = "0";
            for (i = 0; i < this.lstMasterUserEnd.Count; i++)
            {
                //Find the position of step in Master step list
                intStepPos = this.lstMasterUserEnd[i].intStepPos;

                //Execute step
                bool blStepResult = MasterFuncEx(intStepPos);

                //Do Special Control Command here, after finish step!!!
                this.MasterSpecialControlCommandEx(intStepPos);

                if (blStepResult == false) //Step result is NG - return immediately
                {
                    strResult = "Master user end fail at step [" + this.lstMasterUserEnd[i].intStepNumber.ToString() + "] . Return data: [" + this.lstTotalStep[intStepPos].objStepCheckingData.ToString() + "].";
                }
            }

            return strResult; //Return OK string code if everything is OK
        }

        //////////////////////////////////////////////////////////////////
        public bool MasterFuncEx(int intStepPos)
        {
            //Check if row is blank row or user function name area
            if (this.lstTotalStep[intStepPos].intStepSequenceID==-1)
            {
                return false;
            }

            UpdateViewTable(intStepPos);

            //
            object objTempResult = "";
            List<List<object>> lstlstobjInput = new List<List<object>>();
            var lstlstobjOutput = new List<List<object>>();

            //MINI-COMPILER : convert all command-parameter to real parameter
            this.MasterParaSpecialCommandExecute(intStepPos);

            //Calculate list of string input
            lstlstobjInput = MasterCalListObjectInput(intStepPos);

            //Execute Function
            if(this.blActiveTestingSequence==false) //Normal running
            {
                objTempResult = this.clsMasterExtension.lstPluginCollection[this.clsMasterExtension.lstFuncCatalogAllStep[intStepPos].intPartID].Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);
            }
            else //Testing sequence mode
            {
                if(this.clsSeqenceTestData.lstblForceChange[intStepPos]==true) //Request change
                {
                    objTempResult = this.clsSeqenceTestData.lstobjNewValue[intStepPos];
                }
                else //Not request change
                {
                    objTempResult = this.clsMasterExtension.lstPluginCollection[this.clsMasterExtension.lstFuncCatalogAllStep[intStepPos].intPartID].Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);
                }
            }

            //Prevent null happen
            if (objTempResult == null)
            {
                objTempResult = "null";
            }

            //Saving origin return value
            this.lstTotalStep[intStepPos].objStepCheckingData = objTempResult;

            //mark already check
            this.lstTotalStep[intStepPos].blStepChecked = true;
            this.lstTotalStep[intStepPos].intExecuteTimes++;
            if (this.lstTotalStep[intStepPos].intExecuteTimes > 1000000000) this.lstTotalStep[intStepPos].intExecuteTimes = 0;

            //Assign output data
            this.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn = lstlstobjOutput;

            //Judgement result is OK or NG
            bool blStepResult = this.clsUnifyModel.JudgeStepResult(objTempResult, this.lstTotalStep[intStepPos].objLoLimit, this.lstTotalStep[intStepPos].objHiLimit, this.lstTotalStep[intStepPos].strUnitName);

            //Saving result checking
            this.lstTotalStep[intStepPos].blStepResult = blStepResult;

            //
            UpdateViewTable(intStepPos);

            //Return result
            return blStepResult;
        }

        /// <summary>
        /// This function, analyze functional-parameter in steplist, and get return data for that parameter
        /// </summary>
        /// <param name="intStepPos"></param>
        public void MasterParaSpecialCommandExecute(int intStepPos)
        {
            int i = 0;

            //Not Apply with the first step
            //if (intStepPos == 0) return;

            object objTemp = "";
            string strFinalResult = "";
            //int intTemp = 0;

            //For Analyze "Transmission" area with possible Special command
            for (i = 0; i < this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count; i++)
            {
                objTemp = this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos][i].evaluate();

                //For convention of Factory command, the end of Factory command end with  "," character => The last item of list always "empty" string => Reject this one
                if (i == (this.clsCommonFunc.lstlstCommonTransmissionCommandAnalyzer[intStepPos].Count - 1))
                {
                    if (objTemp.ToString() != "") //Only allow the last one if it different from empty string ""
                    {
                        if (objTemp.ToString() != ",")
                        {
                            strFinalResult = strFinalResult + objTemp;
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
            for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameter.Count; i++)
            {
                if (this.clsMasterProgList.lstlstobjStepPara[intStepPos][i].ToString().Trim() != "")
                {
                    this.lstTotalStep[intStepPos].lstobjParameterEx[i] = this.clsCommonFunc.lstlstCommonParaCommandAnalyzer[intStepPos][i].evaluate();
                }

            }
        }

        //*********************************************************
        public void MasterSpecialControlCommandEx(int intStepPos)
        {
            int i = 0;

            for (i = 0; i < this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
            {
                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intTargetProcessID == 3) continue; //Not execute with target single thread

                object objTemp = this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].evaluate();
            }
        }

        /// <summary>
        /// This method prepare list of input parameter passing for each function ID in each plug-in
        /// </summary>
        /// <param name="intStepPos"></param>
        /// <returns></returns>
        public List<List<object>> MasterCalListObjectInput(int intStepPos)
        {
            //List of string input has following format:
            //"Application startup path - Process ID - Test No - Test Name - Test Class - Lo Limit - Hi Limit - Unit - Jig ID - Hardware ID - Func ID -
            // Transmission - Receive - Para1 - ... - Para20 - Jump command - Signal Name - Port Measure Point - Check Pads - Control spec/Comment - Note"

            List<List<object>> lstlstRet = new List<List<object>>();
            List<object> lstTemp = new List<object>();
            string strStartUpPath = Application.StartupPath;

            //Check if row is blank row or user function name area
            if(this.lstTotalStep[intStepPos].intStepSequenceID == -1) //No caring
            {
                return lstlstRet;
            }

            //Calculate for lstlstRet[0] : Start-up path & Process ID & Steplist information
            lstTemp.Add(strStartUpPath); //Start up path
            lstTemp.Add("-1"); //Process ID - with master, is -1

            lstTemp.Add(this.lstTotalStep[intStepPos].intStepNumber.ToString()); //Test Number
            lstTemp.Add(this.lstTotalStep[intStepPos].strStepName.ToString()); //Test Name
            lstTemp.Add(this.lstTotalStep[intStepPos].intStepClass.ToString()); //Test Class
            lstTemp.Add(this.lstTotalStep[intStepPos].objLoLimit.ToString()); //Low Limit
            lstTemp.Add(this.lstTotalStep[intStepPos].objHiLimit.ToString()); //Hi Limit
            lstTemp.Add(this.lstTotalStep[intStepPos].strUnitName.ToString()); //Unit name
            lstTemp.Add(this.lstTotalStep[intStepPos].intJigId.ToString()); //Jig ID
            lstTemp.Add(this.lstTotalStep[intStepPos].intHardwareId.ToString()); //Hardware ID
            lstTemp.Add(this.lstTotalStep[intStepPos].intFunctionId.ToString()); //Function ID


            //lstTemp.Add(this.lstMasterTotal[intStepPos].strTransmisstion.ToString()); //Factory command Transmission
            lstTemp.Add(this.lstTotalStep[intStepPos].strTransmisstionEx.ToString()); //Change to Executed Transmission, not original one in Program List


            lstTemp.Add(this.lstTotalStep[intStepPos].strReceive.ToString()); //Factory command return data

            int i;
            for (i = 0; i < this.lstTotalStep[intStepPos].lstobjParameterEx.Count; i++)
            {
                lstTemp.Add(this.lstTotalStep[intStepPos].lstobjParameterEx[i]);
            }
            //lstTemp.Add(this.lstMasterTotal[intStepPos].intTestPos.ToString());
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

            lstTemp.Add(this.clsMasterSetting.intNumChecker.ToString()); //Add Number of checking items
            lstTemp.Add(this.clsMasterSetting.intNumRow.ToString()); //Add Number of Row setting
            lstTemp.Add(this.clsMasterSetting.intNumCol.ToString()); //Add Number of Col setting
            lstTemp.Add(this.clsMasterSetting.intAllignMode.ToString()); //Add Number of Allign Mode setting
            lstTemp.Add(this.clsMasterSetting.intRoundShapeMode.ToString()); //Add Number of RoundShape Mode setting
            lstTemp.Add(this.clsMasterSetting.intOrgPosition.ToString()); //Add Number of Origin Position setting

            lstlstRet.Add(lstTemp);


            //Adding Master Process Model object itself
            lstTemp = new List<object>();
            lstTemp.Add("clsMasterProcessModel");
            lstTemp.Add(this);
            lstlstRet.Add(lstTemp);

            //Adding Child Control Process Model object
            if(this.blFoundChildControlModel==false)
            {
                this.GetChildControlObject();
            }

            lstTemp = new List<object>();
            lstTemp.Add("clsChildControlModel");
            lstTemp.Add(this.objChildControlModel);
            lstlstRet.Add(lstTemp);


            return lstlstRet;
        }

        //////////////////////////////////////////////////////////////////
        public void MasterProcessMainLoopStart()
        {
            this.tmrMainLoopTimer.Enabled = true;
            //this.thrMasterProcess.Start();
        }
        //////////////////////////////////////////////////////////////////
        public void MasterProcessStop()
        {
            this.tmrMainLoopTimer.Enabled = false;
        }

        //////////////////////////////////////////////////////////////////
        public void BackGroundProcessStart()
        {
            this.tmrBackGroundTimer.Enabled = true;
        }
        
        //////////////////////////////////////////////////////////////////
        public void BackGroundProcessStop()
        {
            this.tmrBackGroundTimer.Enabled = false;
        }

        //////////////////////////////////////////////////////////////////
        public void MasterProcessThread()
        {
            while (this.blRequestStop == false) //Forever loop!
            {
                MasterMainLoop();
            }
        }

        private bool blFlashMainLoop { get; set; }
        private int intMainLoopLastTick { get; set; }
        //////////////////////////////////////////////////////////////////
        public void OnTimedMainLoopTimer(Object source, ElapsedEventArgs e)
        {
            this.tmrMainLoopTimer.Enabled = false;
            ////////////////////////////////
            blFlashMainLoop = !(blFlashMainLoop);
            if (blFlashMainLoop)
            {
                this.clsBindingView.clrMainLoop = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                this.clsBindingView.clrMainLoop = System.Windows.Media.Brushes.Blue;
            }

            //Cal Cycle Time
            int intCurrentTick = MyLibrary.clsApiFunc.GetTickCount();
            this.clsBindingView.intMainLoopCycle = intCurrentTick - intMainLoopLastTick;
            intMainLoopLastTick = intCurrentTick;

            OnPropertyChanged("clsBindingView");

            //

            try
            {
                MasterMainLoop();
            }
            catch (Exception ex)
            {
                string strTest = ex.Message;//Do nothing?
            }

            ////////////////////////////////
            this.tmrMainLoopTimer.Enabled = true;
        }

        private bool blFlashBackGround { get; set; }
        private int intBackGoundLastTick { get; set; }
        //////////////////////////////////////////////////////////////////
        public void OnTimedBackGroundTimer(Object source, ElapsedEventArgs e)
        {
            this.tmrBackGroundTimer.Enabled = false;
            ////////////////////////////////
            blFlashBackGround = !(blFlashBackGround);
            if (blFlashBackGround)
            {
                this.clsBindingView.clrBackGround = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                this.clsBindingView.clrBackGround = System.Windows.Media.Brushes.LightCoral;
            }
            //Cal Cycle Time
            int intCurrentTick = MyLibrary.clsApiFunc.GetTickCount();
            this.clsBindingView.intBackGroundCycle = intCurrentTick - intBackGoundLastTick;
            intBackGoundLastTick = intCurrentTick;

            OnPropertyChanged("clsBindingView");

            //
            BackGroundMainLoop();
            ////////////////////////////////
            this.tmrBackGroundTimer.Enabled = true;
        }

        //Constructor
        public clsMasterProcessModel()
        {
            // clsMasterProcessModel
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SetSplashScreenMessage("Loading clsMasterProcessModel..."));

            this.clsUnifyModel = new classCommonMethod();
            //Setting for MainLoop timer
            tmrMainLoopTimer = new System.Timers.Timer();
            //Hook up the Elapsed event for the timer.
            tmrMainLoopTimer.Elapsed += OnTimedMainLoopTimer;
            //Set timer interval
            tmrMainLoopTimer.Interval = 1; //set default with 1 ms interval value
            tmrMainLoopTimer.Enabled = false; //Default not running

            //Setting for BackGround timer
            tmrBackGroundTimer = new System.Timers.Timer();
            //Hook up the Elapsed event for the timer.
            tmrBackGroundTimer.Elapsed += OnTimedBackGroundTimer;
            //Set timer interval
            tmrBackGroundTimer.Interval = 1; //set default with 1 ms interval value
            tmrBackGroundTimer.Enabled = false; //Default not running


            this.thrMasterProcess = new System.Threading.Thread(this.MasterProcessThread);
            this.blRequestStop = false;

            this.clsMasterExtension = new nspMEFLoading.clsMEFLoading.clsExtensionHandle();

            //Ini for Master Run Mode
            this.dictMasterRunMode = new Dictionary<string, enumMasterRunMode>();
            this.dictMasterRunMode.Add("Normal", enumMasterRunMode.eMasterNormal);
            this.dictMasterRunMode.Add("Single", enumMasterRunMode.eMasterSingleAll);
            this.dictMasterRunMode.Add("Single Cls", enumMasterRunMode.eMasterSingleClass);
            this.dictMasterRunMode.Add("Step", enumMasterRunMode.eMasterStep);

            //For binding
            this.clsBindingView = new clsBindingSupport();

            //Ini for user utilities
            this.lstclsUserUlt = new List<clsUserUtility>();

            //
            this.clsSeqenceTestData = new classSequenceTestData();
        }

        //Destructor
        ~clsMasterProcessModel()
        {
            this.blRequestStop = true;
        }

        //**************************FOR MASTER PROCESS - SPECIAL CONTROL ****************************
        #region _MasterProcessSpecialControl

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

        //****************************************************************************************************************
        public int FindMasterStepPos(int intStepNumber)
        {
            //intStepNumber: step number want to looking for
            //Return: the position of step in all step list. If not found, then return -1

            int i = 0;
            for (i = 0; i < this.lstTotalStep.Count; i++)
            {
                if (intStepNumber == this.lstTotalStep[i].intStepNumber)
                {
                    //MessageBox.Show("Found step " + intStepNumber.ToString() + " at position " + (i + 1).ToString(), "FindStepOrder()");
                    return i;
                }
            }

            //MessageBox.Show("No found step " + intStepNumber.ToString() , "FindStepOrder()");
            return -1; //No found matching step
        }

        //*************************************************************
        public int FindCheckingToken(int intStepPos)
        {
            //Only apply with User Function sequence
            int intRet = -1;
            int i = 0;
            //
            if(this.lstTotalStep[intStepPos].intStepSequenceID==1) //User Function Sequence
            {
                string strUserFuncName = "";
                strUserFuncName = this.lstTotalStep[intStepPos].strUserFunctionName;
                int intIndex = this.MasterSearchUserFunc(strUserFuncName);
                if (intIndex != -1)
                {
                    for (i = 0; i < this.clsMasterProgList.lstclsUserFunction[intIndex].lstclsStepRowData.Count; i++)
                    {
                        if (this.clsMasterProgList.lstclsUserFunction[intIndex].lstclsStepRowData[i].intStepPos == intStepPos)
                        {
                            intRet = i;
                            break;
                        }
                    }
                }
            }

            //
            return intRet;
        }

        //************************************************************
        /// <summary>
        /// User Function Define in Program List!
        /// </summary>
        /// <param name="lstobjInput"></param>
        /// <returns></returns>
        public object UserFunction(List<object> lstobjInput)
        {
            object objRet = new object();

            if (this.clsMasterProgList.lstclsUserFunction == null) return objRet;
            if (this.clsMasterProgList.lstclsUserFunction.Count == 0) return objRet;
            //Checking 
            if (lstobjInput.Count == 0) return "MasterProcessUserFunction() error: List of parameter input has no element";
            //
            string strUserFunctionName = lstobjInput[0].ToString().Trim();

            int i = 0;

            int intIndex = this.MasterSearchUserFunc(strUserFunctionName);
            if (intIndex == -1) return "MasterProcessUserFunction() error: could not found user function [" + strUserFunctionName + "]";

            //Passing parameter for function
            this.clsMasterProgList.lstclsUserFunction[intIndex].lstobjParameter = new List<object>();
            for (i = 1; i < lstobjInput.Count; i++)
            {
                this.clsMasterProgList.lstclsUserFunction[intIndex].lstobjParameter.Add(lstobjInput[i]);
            }

            //
            objRet = this.MasterUserFunctionStepEx(intIndex);
            //
            return objRet;
        }

        public object MasterUserFunctionStepEx(int intUserFuncID)
        {
            int i = 0;
            bool blResult = false;
            object objRet = new object();

            for (i = 0; i < this.clsMasterProgList.lstclsUserFunction[intUserFuncID].lstclsStepRowData.Count; i++)
            {
                int intStepPos = 0;
                intStepPos = this.clsMasterProgList.lstclsUserFunction[intUserFuncID].lstclsStepRowData[i].intStepPos;

                //Execute Function
                blResult = this.MasterFuncEx(intStepPos);

                objRet = this.lstTotalStep[intStepPos].objStepCheckingData;

                //For Special control
                this.MasterUserFuncSpecialControlCommandEx(intStepPos, ref i);
            }

            //Return Last Checking step Function Result - regulation of User Function
            this.clsMasterProgList.lstclsUserFunction[intUserFuncID].objReturnData = objRet;
            return objRet;
        }

        public object MasterGetUserFuncPara(string strUserFuncName, int intParaPos)
        {
            object objRet = new object();
            //
            int intIndex = this.MasterSearchUserFunc(strUserFuncName);
            if (intIndex == -1) return "MasterGetUserFuncPara() error: could not found user function [" + strUserFuncName + "]";

            if (this.clsMasterProgList.lstclsUserFunction[intIndex].lstobjParameter.Count - 1 < intParaPos)
            {
                return "MasterGetUserFuncPara() error: User function [" + strUserFuncName + "] has not enough parameter to return.";
            }

            objRet = this.clsMasterProgList.lstclsUserFunction[intIndex].lstobjParameter[intParaPos];

            if (objRet == null)
            {
                objRet = "null";
            }

            //
            return objRet;
        }

        public int MasterSearchUserFunc(string strNameInput)
        {
            int intRet = -1; //Not found default

            int i = 0;
            for (i = 0; i < this.clsMasterProgList.lstclsUserFunction.Count; i++)
            {
                if (strNameInput.ToUpper().Trim() == this.clsMasterProgList.lstclsUserFunction[i].strUserFunctionName.Trim().ToUpper())
                {
                    intRet = i;
                    break;
                }
            }

            return intRet;
        }

        //*********************************************************
        public void MasterUserFuncSpecialControlCommandEx(int intStepPos, ref int intToken)
        {
            int i = 0;

            for (i = 0; i < this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos].Count; i++)
            {
                //if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].blIsCmdPara == false) continue;

                if (this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i].clsSettingCommand.intTargetProcessID == 3) continue; //Not execute with target single thread

                //Execute if is special command
                //var clsTemp = new clsCommonCommandGuider();
                //clsTemp = this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i];
                //this.MasterSpecialCommandExecute(ref clsTemp, intStepPos, ref intToken);
                //this.clsCommonFunc.lstlstCommonCommandAnalyzer[intStepPos][i] = clsTemp;





            }
        }

        //*******************************************************************************************************************
        public List<classStepDataInfor> FindMasterStepListClass(int intClass)
        {
            //Select Master Excel class
            switch (intClass)
            {
                case 0: //Ini
                    return this.lstMasterIni;

                case 1: //Cycle Polling
                    return this.lstMasterCyclePoll;

                case 2: // Request Polling
                    return this.lstMasterResetRequestPoll;

                case 3: // ResetProcess
                    return this.lstMasterResetProcess;

                case 4: // EmerPolling
                    return this.lstMasterEmerPoll;

                case 5: // EmerHandle
                    return this.lstMasterEmerHandle;

                case 6: // SafetyConditionPolling
                    return this.lstMasterSafetyPoll;

                case 7: // SafetyConditionHandle
                    return this.lstMasterSafetyHandle;

                case 8: // PreStartProcess
                    return this.lstMasterPreStart;

                case 9: // StartPolling
                    return this.lstMasterStartPoll;

                case 10: // CheckingProcess
                    return this.lstMasterCheckingProcess;

                case 11: // User End
                    return this.lstMasterUserEnd;

                case 100: // Background class
                    return this.lstMasterBackGroundPoll;

                default:
                    return null;
            }

        }

        public enumMasterStatus FindMasterStatusClass(int intClass)
        {
            //Select Master Excel class
            switch (intClass)
            {
                case 0: //Ini
                    return enumMasterStatus.eMasterIni;

                case 1: //Cycle Polling
                    return enumMasterStatus.eMasterCyclePoll;

                case 2: // Request Polling
                    return enumMasterStatus.eMasterResetRequestPoll;

                case 3: // ResetProcess
                    return enumMasterStatus.eMasterResetProcess;

                case 4: // EmerPolling
                    return enumMasterStatus.eMasterEmerPoll;

                case 5: // EmerHandle
                    return enumMasterStatus.eMasterEmerHandle;

                case 6: // SafetyConditionPolling
                    return enumMasterStatus.eMasterSafetyPoll;

                case 7: // SafetyConditionHandle
                    return enumMasterStatus.eMasterSafetyHandle;

                case 8: // PreStartProcess
                    return enumMasterStatus.eMasterPreStart;

                case 9: // StartPolling
                    return enumMasterStatus.eMasterStartPoll;

                case 10: // CheckingProcess
                    return enumMasterStatus.eMasterCheckingProcess;

                case 11: // User End
                    return enumMasterStatus.eMasterUserEnd;

                case 100: // Background Polling
                    return enumMasterStatus.eMasterBackGroundPoll;

                default:
                    return enumMasterStatus.eMasterNotRecognize;
            }

        }

        //*****************************************************************************************************************************
        #endregion

    }

    //**********************************************************
    public class clsBindingSupport
    {
        public System.Windows.Media.SolidColorBrush clrMainLoop { get; set; }
        public int intMainLoopCycle { get; set; }
        public System.Windows.Media.SolidColorBrush clrBackGround { get; set; }
        public int intBackGroundCycle { get; set; }

        public clsBindingSupport()
        {
            this.clrMainLoop = System.Windows.Media.Brushes.Blue;
            this.clrBackGround = System.Windows.Media.Brushes.LightCoral;
        }
    }

    //**********************************************************
    public class classMasterSequenceTester: BindableBase
    {
        public clsMasterProcessModel clsMasterModel { get; set; }
        public int intStepPos { get; set; }
        public object objNewValue { get; set; }
        //
        public List<string> GetMasterList
        {
            get
            {
                List<string> lststrRet = new List<string>();
                //
                int i = 0;
                for (i = 0; i < this.clsMasterModel.lstTotalStep.Count; i++)
                {
                    lststrRet.Add(this.clsMasterModel.lstTotalStep[i].intStepNumber.ToString());
                }
                //
                return lststrRet;
            }
            set
            {
                this.GetMasterList = value;
            }
        }

        //
        private string _strSelectedItem;
        public string TheSelectedItem
        {
            get { return _strSelectedItem; }
            set
            {
                if(value != null)
                {
                    _strSelectedItem = value;
                    //Analyze & display selected step name
                    int intStepNum = 0;
                    if (int.TryParse(this._strSelectedItem, out intStepNum) == false) return;
                    //
                    int intStepPos = this.clsMasterModel.FindMasterStepPos(intStepNum);
                    if (intStepPos == -1) return;

                    //Marking
                    this.intStepPos = intStepPos;

                    //Display selected step name
                    this.strSelectedStepName = this.clsMasterModel.lstTotalStep[intStepPos].strStepName;
                    //
                    OnPropertyChanged("strSelectedStepName");
                }
                
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
                //Setting for Master Process forcing selected step with desired value
                this.clsMasterModel.clsSeqenceTestData.lstblForceChange[this.intStepPos] = true;
                this.clsMasterModel.clsSeqenceTestData.lstobjNewValue[this.intStepPos] = this._strNewValue;
            }
        }

        //
        public ICommand ICommandApply { get; set; }
        public void ICommandApplyHandle()
        {
            //
            this.objNewValue = this._strNewValue;
            //
            this.clsMasterModel.clsSeqenceTestData.lstblForceChange[this.intStepPos] = true;
            this.clsMasterModel.clsSeqenceTestData.lstobjNewValue[this.intStepPos] = this.objNewValue;
        }
        //
        public ICommand ICommandReset { get; set; }
        public void ICommandResetHandle()
        {
            //
            this.objNewValue = new object();
            //
            this.clsMasterModel.clsSeqenceTestData.lstblForceChange[this.intStepPos] = false;
            this.clsMasterModel.clsSeqenceTestData.lstobjNewValue[this.intStepPos] = this.objNewValue;
        }
        //
        public ICommand ICommandResetAll { get; set; }
        public void ICommandResetAllHandle()
        {
            int i = 0;
            //
            this.objNewValue = new object();
            //
            for (i = 0; i < this.clsMasterModel.clsSeqenceTestData.lstblForceChange.Count;i++)
            {
                //
                this.clsMasterModel.clsSeqenceTestData.lstblForceChange[i] = false;
                this.clsMasterModel.clsSeqenceTestData.lstobjNewValue[i] = this.objNewValue;
            } 
        }


        //Constructor
        public classMasterSequenceTester(clsMasterProcessModel clsMasterModel)
        {
            this.clsMasterModel = clsMasterModel;
            //
            this._strSelectedStepName = "";
            this.objNewValue = new object();
            //
            this.ICommandApply = new DelegateCommand(this.ICommandApplyHandle);
            this.ICommandReset = new DelegateCommand(this.ICommandResetHandle);
            this.ICommandResetAll = new DelegateCommand(this.ICommandResetAllHandle);
        }
    }

    //**********************************************************
}
