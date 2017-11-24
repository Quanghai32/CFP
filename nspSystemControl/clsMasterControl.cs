using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using nspINTERFACE;

namespace nspSystemControl
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginMasterControl,1000")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class clsMasterControl: nspINTERFACE.IPluginExecute
    {
        //For extract Master Process Model & Child Process Model object
        bool blExtractMaster;
        nspMasterProcessModel.clsMasterProcessModel clsMasterProcess;
        bool blExtractChildControl;
        nspChildProcessModel.clsChildControlModel clsChildControl;
        bool blExtractChild;
        nspChildProcessModel.clsChildProcessModel clsChildProcess;

        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";
            //Inform to Host program which Function this plugin support
            strTemp = "1000,0,0,1,2,9,10,11,30,50,51,52,53,54"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 8/01/2015"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Create new for control master system!"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Process ID - ... - JigID-HardID-FuncID"
            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());

            //Try to extract Master Process Model object & Child Process Model object
            int i = 0;
            if (blExtractMaster == false) //Need to extract Master Process
            {
                for (i = 0; i < lstlstobjInput.Count; i++)
                {
                    if (lstlstobjInput[i].Count < 2) continue;

                    if (lstlstobjInput[i][0].ToString() == "clsMasterProcessModel")
                    {
                        if (lstlstobjInput[i][1] is nspMasterProcessModel.clsMasterProcessModel)
                        {
                            this.clsMasterProcess = (nspMasterProcessModel.clsMasterProcessModel)lstlstobjInput[i][1];
                            blExtractMaster = true;
                            break;
                        }
                    }
                }
            }

            if (blExtractChild == false) //Need to extract Child Process
            {
                for (i = 0; i < lstlstobjInput.Count; i++)
                {
                    if (lstlstobjInput[i].Count < 2) continue;

                    if (lstlstobjInput[i][0].ToString() == "clsChildProcessModel")
                    {
                        if (lstlstobjInput[i][1] is nspChildProcessModel.clsChildProcessModel)
                        {
                            this.clsChildProcess = (nspChildProcessModel.clsChildProcessModel)lstlstobjInput[i][1];
                            blExtractChild = true;
                            break;
                        }
                    }
                }
            }

            if (blExtractChildControl == false) //Need to extract Child Control Model
            {
                for (i = 0; i < lstlstobjInput.Count; i++)
                {
                    if (lstlstobjInput[i].Count < 2) continue;

                    if (lstlstobjInput[i][0].ToString() == "clsChildControlModel")
                    {
                        if (lstlstobjInput[i][1] is nspChildProcessModel.clsChildControlModel)
                        {
                            this.clsChildControl = (nspChildProcessModel.clsChildControlModel)lstlstobjInput[i][1];
                            blExtractChildControl = true;
                            break;
                        }
                    }
                }
            }

            switch (intJigID) //Select JigID
            {
                case 1000:
                    return SelectHardIDFromJigID1000(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID1000(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intHardID = 0;
            if (int.TryParse(lstlstobjInput[0][9].ToString(), out intHardID) == false) return "error 1"; //Not numeric error
            intHardID = int.Parse(lstlstobjInput[0][9].ToString());
            switch (intHardID) //Select HardID
            {
                case 0:
                    return SelectFuncIDFromHardID0(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "PluginMicroUSB: Unrecognize HardID " + intHardID.ToString();
            }
        }

        public object SelectFuncIDFromHardID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intFuncID = 0;
            if (int.TryParse(lstlstobjInput[0][10].ToString(), out intFuncID) == false) return "error 1"; //Not numeric error
            intFuncID = int.Parse(lstlstobjInput[0][10].ToString());
            switch (intFuncID) //Select FuncID
            {
                case 0:
                    return PluginMasterControlFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginMasterControlFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginMasterControlFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 9:
                    return PluginMasterControlFuncID9(lstlstobjInput, out lstlstobjOutput);
                case 10:
                    return PluginMasterControlFuncID10(lstlstobjInput, out lstlstobjOutput);
                case 11:
                    return PluginMasterControlFuncID11(lstlstobjInput, out lstlstobjOutput);
                case 30:
                    return PluginMasterControlFuncID30(lstlstobjInput, out lstlstobjOutput);
                case 50:
                    return PluginMasterControlFuncID50(lstlstobjInput, out lstlstobjOutput);
                case 51:
                    return PluginMasterControlFuncID51(lstlstobjInput, out lstlstobjOutput);
                case 52:
                    return PluginMasterControlFuncID52(lstlstobjInput, out lstlstobjOutput);
                case 53:
                    return PluginMasterControlFuncID53(lstlstobjInput, out lstlstobjOutput);
                case 54:
                    return PluginMasterControlFuncID54(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        /// <summary>
        /// Return what is receive from Para1
        /// </summary>
        /// <param name="lststrInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            return lstlstobjInput[0][13];
        }

        /// <summary>
        /// Checking Start command sending - command for all thread checking start
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            //Command for all process start checking
            //Program.ProcessAfterStart();
            if (this.clsChildControl == null) return "Error 1000-0-1: Child Control process not found!";

            this.clsChildControl.ProcessCheckingStart();

            return "0"; //Successful code
        }

        /// <summary>
        /// Retry Checking Start command sending - command for all thread checking which is NG start again!
        /// </summary>
        /// <param name="lststrInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            //Command for NG process start checking again
            this.clsChildControl.RetryCheckingStart();

            return "0"; //Successful code
        }

        /// <summary>
        /// Item Info Display from Child Process Function
        ///     + Para1 (13): The number of Info in Item Group Box want to change Display - Count from 0
        ///     + Para3 (14): Info content want to display
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID9(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            int intChildProcessID = 0;
            int intNumTextBox = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intChildProcessID) == false) return "Error: Number of child process want to change is not integer!";
            if (intChildProcessID >= this.clsChildControl.clsMainVar.intNumItem) return "Error: Number of Child Process want to change Info is over setting limit!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intNumTextBox) == false) return "Error: Number of Item Info want to change is not integer!";
            if (intNumTextBox >= this.clsChildControl.clsMainVar.intNumberUserTextBox) return "Error: Number of Item Info want to change is over setting limit!";

            string strInfo = lstlstobjInput[0][14].ToString();

            //Change Display on Main Form
            //Program.SetAnyProperty(Program.clsMainPageView.lstlstTbUser[intChildProcessID][intNumTextBox], "Text", Program.strcMainVar.lstStrTbUser[intNumTextBox] + ": " + strInfo);
            string strRet = this.clsChildControl.UpdateUserTextBoxInfo(intChildProcessID, intNumTextBox, strInfo);

            return strRet;
        }

        /// <summary>
        /// Item Info Display from Master Process Function
        ///     + Para2 (13): The number of Child Process want to change Display - Count from 0
        ///     + Para1 (14): The number of Info in Item Group Box want to change Display - Count from 0
        ///     + Para3 (15): Info content want to display
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID10(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            int intChildProcessID = 0;
            int intNumTextBox = 0;

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intChildProcessID) == false) return "Error: Number of child process want to change is not integer!";
            if (intChildProcessID >= this.clsChildControl.clsMainVar.intNumItem) return "Error: Number of Child Process want to change Info is over setting limit!";

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intNumTextBox) == false) return "Error: Number of Item Info want to change is not integer!";
            if (intNumTextBox >= this.clsChildControl.clsMainVar.intNumberUserTextBox) return "Error: Number of Item Info want to change is over setting limit!";

            string strInfo = lstlstobjInput[0][15].ToString();

            //Change Display on Main Form
            string strRet = this.clsChildControl.UpdateUserTextBoxInfo(intChildProcessID, intNumTextBox, strInfo);

            return strRet; //Successful code
        }

        /// <summary>
        /// Serial Item Info Display Function
        ///     + Para1 (13): The number of Info in Item Group Box want to change Display
        ///     + Para2 (14): The Step Number Where UserRet taken from
        ///     + Para3 (15): UserRet Name of Serial Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID11(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            int intNumTextBox = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intNumTextBox) == false) return "Error: Number of Item Info want to change is not integer!";

            ////Change Display on Main Form
            int i = 0;
            int intStepNumber = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intStepNumber) == false) return "Error: Step Number want to get info from is not integer!";
            //Find position in Master Steplist
            int intStepPos = 0;
            intStepPos = this.clsMasterProcess.FindMasterStepPos(intStepNumber);
            if (intStepPos == -1) return "The Step Number [" + intStepNumber.ToString() + "] not found in Master Steplist!";
            //Compare string UserRet
            string strUserRet = lstlstobjInput[0][15].ToString();

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            foreach (List<object> lstTemp in this.clsMasterProcess.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper() == strUserRet.ToUpper()) //Found
                {
                    blFlagFound = true;
                    //Display on screen
                    for (i = 0; i < this.clsChildControl.clsMainVar.intNumItem; i++)
                    {
                        if (lstTemp.Count > this.clsChildControl.clsMainVar.intNumItem)
                        {
                            this.clsChildControl.UpdateUserTextBoxInfo(i, intNumTextBox, lstTemp[i + 1].ToString());
                        }
                    }
                    break;
                }
            }

            if (blFlagFound == false)
            {
                //Display on screen
                for (i = 0; i < this.clsChildControl.clsMainVar.intNumItem; i++)
                {
                    this.clsChildControl.UpdateUserTextBoxInfo(i, intNumTextBox, "Not Found!");
                }
            }

            return "0"; //Successful code
        }


        /// <summary>
        /// Serial Item Pre-Info DATA SAVING
        ///     + Para1 (13): Optional Saving: 0 (Pre-Info). 1 (After-Info)
        ///     + Para2 (14): The Number of Pre- Info want to Saving
        ///     + Para3 (15): The Step Number Where UserRet taken from
        ///     + Para4 (16): UserRet Name of Serial Info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID30(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            int intOptionSaving = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intOptionSaving) == false) return "Error: Optional saving is not integer!";

            if (intOptionSaving > 1) intOptionSaving = 0; //Default

            int intInfoOrder = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intInfoOrder) == false) return "Error: Number of Pre Info want to save is not integer!";

            ////Change Display on Main Form
            int i = 0;
            int intStepNumber = 0;
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intStepNumber) == false) return "Error: Step Number want to get info from is not integer!";
            //Find position in Master Steplist
            int intStepPos = 0;
            intStepPos = this.clsMasterProcess.FindMasterStepPos(intStepNumber);
            if (intStepPos == -1) return "The Step Number [" + intStepNumber.ToString() + "] not found in Master Steplist!";
            //Compare string UserRet
            string strUserRet = lstlstobjInput[0][16].ToString();

            //If every OK. Then, we get return of step parameter and assign new value for current parameter
            bool blFlagFound = false;
            //foreach (List<string> lstTemp in Program.lstChkInfo[intProcessId].clsChildProcess.clsItemResult.lstclsStepDataRet[intTargetStepPos].lstlststrDataReturn)
            foreach (List<object> lstTemp in this.clsMasterProcess.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn)
            {
                if (lstTemp[0].ToString().ToUpper() == strUserRet.ToUpper()) //Found
                {
                    blFlagFound = true;
                    //Saving to Data file
                    for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
                    {
                        if (lstTemp.Count > this.clsChildControl.lstChildProcessModel.Count)
                        {
                            if (intOptionSaving == 1) //After-Info
                            {
                                this.clsChildControl.lstChildProcessModel[i].lststrProgramListUserAfterInfo[intInfoOrder - 1] = lstTemp[i + 1].ToString();
                            }
                            else //Pre-Info : Default
                            {
                                this.clsChildControl.lstChildProcessModel[i].lststrProgramListUserPreInfo[intInfoOrder - 1] = lstTemp[i + 1].ToString();
                            }
                        }
                    }
                    break;
                }
            }

            if (blFlagFound == false)
            {
                //Display on screen
                for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
                {
                    if (intOptionSaving == 1) //After-Info
                    {
                        this.clsChildControl.lstChildProcessModel[i].lststrProgramListUserAfterInfo[intInfoOrder - 1] = "Data Not Found!";
                    }
                    else //Pre-Info : Default
                    {
                        this.clsChildControl.lstChildProcessModel[i].lststrProgramListUserPreInfo[intInfoOrder - 1] = "Data Not Found!";
                    }
                }
            }

            return "0"; //Successful code
        }

        /// <summary>
        /// Confirm Checking finish
        /// </summary>
        /// <param name="lstlststrInput"></param>
        /// <param name="lstlststrOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID50(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            string strRet = this.clsChildControl.isCheckingFinish();

            return strRet;
        }

        /// <summary>
        /// Total Checking Result Function.
        /// Parameter: Nothing
        /// Return: 1 - if all unit is OK. 0 - if there is at least 1 unit is NG.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID51(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            //Get total result
            int i = 0;
            bool blTemp = true;

            //Scan all item result
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
            {
                if (this.clsChildControl.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == false)
                {
                    blTemp = false;
                    break;
                }
            }

            if (blTemp == true) //Checking result is OK
            {
                return "1";
            }
            else //Checking result is NG
            {
                return "0";
            }
        }

        /// <summary>
        /// Reset all jumping time to original setting
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID52(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            int i = 0;
            int j = 0;
            int k = 0;
            for (i = 0; i < this.clsMasterProcess.lstTotalStep.Count; i++)
            {
                for (j = 0; j < this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i].Count; j++)
                {
                    if (this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdPara == null) continue;
                    if (this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdPara.Count == 0) continue;
                    this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdParaEx = new List<object>();
                    for(k=0;k<this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdPara.Count;k++)
                    {
                        this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdParaEx.Add(this.clsMasterProcess.clsCommonFunc.lstlstCommonCommandAnalyzer[i][j].lstobjCmdPara[k]);
                    }
                }
            }

            return "0";
        }

        /// <summary>
        /// Single process checking Result getting Function.
        /// Parameter: Nothing
        /// Return: 1 - if that process is OK. 0 - if NG.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID53(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            //Get current process ID
            int intProcessID = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "Error: Process ID setting is not integer!";

            //Get item result

            if (this.clsChildControl.lstChildProcessModel[intProcessID].clsItemResult.blItemCheckingResult == true) //PASS
            {
                return "1";
            }
            else //FAIL
            {
                return "0";
            }
        }

        /// <summary>
        /// Confirm system is running 1 specific step in Master program list!
        /// Parameter 1 (13): The step want to confirm
        /// Return: If sytem is running that step, return 1. If not, return 0.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginMasterControlFuncID54(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();

            //Find step position need to confirm from Parameter 1
            int intStepConfirmPos = 0;
            //Check numeric or not
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intStepConfirmPos) == false)
            {
                return "Error: The step want to confirm is not integer format!";
            }

            //Try to find step position
            intStepConfirmPos = this.clsMasterProcess.FindMasterStepPos(intStepConfirmPos);
            //Check if that step is exist in Master Steplist of not
            if (intStepConfirmPos == -1) //Not found
            {
                return "Error: The step want to confirm is not included in Master Steplist!";
            }
            //Confirm with running step position
            if (intStepConfirmPos == this.clsMasterProcess.intStepPosRunning) //Matching
            {
                return "1";
            }
            else //Not match
            {
                return "0";
            }
        }

    }
}
