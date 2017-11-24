using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Timers;
using nspINTERFACE;
using nspCFPInfrastructures;
using System.Windows;


namespace nspSystemControl
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSystemControl,0")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class clsSystemControl: nspINTERFACE.IPluginExecute
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
            strTemp = "0,0,0,1,2,3,4,5,6,20,21,22,23,51,52,500"; lstobjInfo.Add(strTemp);
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hoang CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 14/10/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add funtion ID51"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Application startup path - Process ID - ... - JigID-HardID-FuncID"
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
                case 0:
                    return SelectHardIDFromJigID0(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
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
                    return "Unrecognize HardID: " + intHardID.ToString();
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
                    return PluginSystemControlFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1:
                    return PluginSystemControlFuncID1(lstlstobjInput, out lstlstobjOutput);
                case 2:
                    return PluginSystemControlFuncID2(lstlstobjInput, out lstlstobjOutput);
                case 3:
                    return PluginSystemControlFuncID3(lstlstobjInput, out lstlstobjOutput);
                case 4:
                    return PluginSystemControlFuncID4(lstlstobjInput, out lstlstobjOutput);
                case 5:
                    return PluginSystemControlFuncID5(lstlstobjInput, out lstlstobjOutput);
                case 6:
                    return PluginSystemControlFuncID6(lstlstobjInput, out lstlstobjOutput);
                case 20:
                    return PluginSystemControlFuncID20(lstlstobjInput, out lstlstobjOutput);
                case 21:
                    return PluginSystemControlFuncID21(lstlstobjInput, out lstlstobjOutput);
                case 22:
                    return PluginSystemControlFuncID22(lstlstobjInput, out lstlstobjOutput);
                case 23:
                    return PluginSystemControlFuncID23(lstlstobjInput, out lstlstobjOutput);
                case 51:
                    return PluginSystemControlFuncID51(lstlstobjInput, out lstlstobjOutput);
                case 52:
                    return PluginSystemControlFuncID52(lstlstobjInput, out lstlstobjOutput);
                //case 500:
                //    return PluginSystemControlFuncID500(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        /// <summary>
        /// This function for testing only?
        /// </summary>
        /// <param name="lststrInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            //Return a testing user Description
            int i = 0;
            lstobjTemp.Add("test1");
            for (i = 0; i < 10; i++)
            {
                lstobjTemp.Add((i + 1).ToString());
            }

            lstobjTemp.Add("0");
            lstobjTemp.Add("1");
            lstobjTemp.Add("2");
            lstobjTemp.Add("3");
            lstobjTemp.Add("4");
            lstobjTemp.Add("5");
            lstobjTemp.Add("6");
            lstobjTemp.Add("7");
            lstobjTemp.Add("8");
            lstobjTemp.Add("9");
            lstobjTemp.Add("10");

            lstlstobjOutput.Add(lstobjTemp);

            lstobjTemp = new List<object>();

            //Return a testing user Description
            lstobjTemp.Add("test2");

            lstobjTemp.Add("A");
            lstobjTemp.Add("B");
            lstobjTemp.Add("C");
            lstobjTemp.Add("D");
            lstobjTemp.Add("E");
            lstobjTemp.Add("F");

            for (i = 0; i < 10; i++)
            {
                lstobjTemp.Add((i + 1).ToString());
            }

            lstlstobjOutput.Add(lstobjTemp);


            lstobjTemp = new List<object>();

            //Return a testing user Description
            lstobjTemp.Add("test3");

            lstobjTemp.Add("1000");
            lstobjTemp.Add("1001");
            lstobjTemp.Add("1002");
            lstobjTemp.Add("1003");
            lstobjTemp.Add("1004");
            lstobjTemp.Add("1005");

            lstobjTemp.Add("A005");
            lstobjTemp.Add("A006");
            lstobjTemp.Add("A007");
            lstobjTemp.Add("AFFF");
            lstobjTemp.Add("FFFF");

            lstlstobjOutput.Add(lstobjTemp);



            //Add class Test
            clsTestClass clsTest = new clsTestClass();
            lstobjTemp = new List<object>();
            lstobjTemp.Add("clsTest");
            lstobjTemp.Add(clsTest);
            lstlstobjOutput.Add(lstobjTemp);

            //
            return lstlstobjInput[0][1].ToString(); //Return what Item Process ID is running
        }


        class clsTestClass
        {
            public string strTest { get; set; }

            public string Msg(string strMsg)
            {
                MessageBox.Show(strMsg);
                return strMsg;
            }

        }


        /// <summary>
        /// User Utility Registering Function
        ///     - Para1 (13): Indicate what MEF Part Description Program use to load User Utility Form
        ///     - Para2 (14): The optinal indicate what process which user utility belong to: 0 - Dafault: Child Process. -1: Master Process
        ///     - para3 (15): The title of user utility form (what name display on main user interface)
        ///     - para4 (16): The Jig ID to call User Form
        ///     - para5 (17): The Hard ID to call User Form
        ///     - para6 (18): The Func ID to call User Form
        ///     - Para7 (19): The ID of User Utility Form in MEF Part (1 MEF part can has more than 1 user utility). Default is 0.
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<string>();

            string strPartDesc = "";
            int intOptionalProcess = 0;
            string strUserUltTitle = "";
            int intJigID = 0;
            int intHardID = 0;
            int intFuncID = 0;
            int intUserUtlID = 0;

            if (lstlstobjInput[0].Count < 2) return "Error";

            //Get Info from steplist
            strPartDesc = lstlstobjInput[0][13].ToString().Trim();

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intOptionalProcess) == false)
            {
                //return "Error: The Jig ID of function to call User Utility setting in steplist is not numerical!";
                intOptionalProcess = 0;//Default is child process
            }
            
            strUserUltTitle = lstlstobjInput[0][15].ToString();

            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intJigID) == false)
            {
                return "Error: The Jig ID of function to call User Utility setting in steplist is not numerical!";
            }

            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intHardID) == false)
            {
                return "Error: The Hard ID of function to call User Utility setting in steplist is not numerical!";
            }

            if (int.TryParse(lstlstobjInput[0][18].ToString(), out intFuncID) == false)
            {
                return "Error: The Function ID of function to call User Utility setting in steplist is not numerical!";
            }

            if (int.TryParse(lstlstobjInput[0][19].ToString(), out intUserUtlID) == false)
            {
                //return "Error: The ID of User Utility setting in steplist is not numerical!";
                intUserUtlID = 0; //Default value
            }

            //Looking for if there is any Master MEF part match to strPartDesc setting in steplist
            int i = 0;
            bool blFlagFound = false;
            int intPartFound = 0;
            for (i = 0; i < this.clsMasterProcess.clsMasterExtension.IFunctionCatalog.Count; i++)
            {
                if (strPartDesc == this.clsMasterProcess.clsMasterExtension.IFunctionCatalog[i].strPartDesc.Trim())
                {
                    blFlagFound = true;
                    intPartFound = i;
                    break;
                }
            }
            if (blFlagFound == false) return "Error: we cannot find Master MEF Part has description: " + strPartDesc;

            //OK, Now we have found what MEF part need to access and call out user utility form
            //Create new menu Item

            var UserMenuItem = new System.Windows.Controls.MenuItem();
            //UserMenuItem.Name = strUserUltTitle;
            UserMenuItem.Header = strUserUltTitle;

            var subFuncCatalog = new FunctionCatalog();

            subFuncCatalog.intPartID = this.clsMasterProcess.clsMasterExtension.IFunctionCatalog[intPartFound].intPartID;
            subFuncCatalog.strPartDesc = strPartDesc;
            subFuncCatalog.intFuncID = intFuncID;
            subFuncCatalog.intHardID = intHardID;
            subFuncCatalog.intJigID = intJigID;

            clsUserUtility clsNew = new clsUserUtility();
            clsNew.lstlstobjInput = new List<List<object>>();
            clsNew.UserUltFuncCatalog = new FunctionCatalog();
            clsNew.UserUltFuncCatalog.intPartID = this.clsMasterProcess.clsMasterExtension.IFunctionCatalog[intPartFound].intPartID;
            clsNew.UserUltFuncCatalog.strPartDesc = strPartDesc;
            clsNew.UserUltFuncCatalog.intJigID = intJigID;
            clsNew.UserUltFuncCatalog.intHardID = intHardID;
            clsNew.UserUltFuncCatalog.intFuncID = intFuncID;
            clsNew.intUserUltID = intUserUtlID;
            clsNew.lstlstobjInput = lstlstobjInput;
            

            //Add to list
            if (intOptionalProcess == -1) //Master Process
            {
                this.clsMasterProcess.lstclsUserUlt.Add(clsNew);
                this.clsMasterProcess.lstclsUserUlt[this.clsMasterProcess.lstclsUserUlt.Count - 1].UserMenuItem = UserMenuItem;

                //add child item - Note that we have to add to Child Control class. Because the menu strip display on Main User Interface is belong to Child control class!!!
                //this.clsMasterProcess.obsMenuUserUtilities.Add(this.clsMasterProcess.lstclsUserUlt[this.clsMasterProcess.lstclsUserUlt.Count - 1].UserMenuItem);
                this.clsChildControl.obsMenuUserUtilities.Add(this.clsMasterProcess.lstclsUserUlt[this.clsMasterProcess.lstclsUserUlt.Count - 1].UserMenuItem);

                //Add Handler for new item click
                this.clsMasterProcess.lstclsUserUlt[this.clsMasterProcess.lstclsUserUlt.Count - 1].UserMenuItem.Click += new System.Windows.RoutedEventHandler(this.MasterUserUtilityClick);
            }
            else //Default is Child Process
            {
                this.clsChildControl.lstclsUserUlt.Add(clsNew);
                this.clsChildControl.lstclsUserUlt[this.clsChildControl.lstclsUserUlt.Count - 1].UserMenuItem = UserMenuItem;
                //add child item
                this.clsChildControl.obsMenuUserUtilities.Add(this.clsChildControl.lstclsUserUlt[this.clsChildControl.lstclsUserUlt.Count - 1].UserMenuItem);

                //Add Handler for new item click
                this.clsChildControl.lstclsUserUlt[this.clsChildControl.lstclsUserUlt.Count - 1].UserMenuItem.Click += new System.Windows.RoutedEventHandler(this.UserUtilityClick);
            }

            //Return 0 if everything is OK
            return "0";
        }

        private void UserUtilityClick(object sender, RoutedEventArgs e)
        {
            //Looking for sender and assign proper handling function
            int i;
            int intIndexFound = 0;

            for (i = 0; i < this.clsChildControl.lstclsUserUlt.Count; i++)
            {
                if (sender == this.clsChildControl.lstclsUserUlt[i].UserMenuItem)
                {
                    //MessageBox.Show("Hello, My master! I'm test item number " + i.ToString());
                    intIndexFound = i;
                    break;
                }
            }


            //When User Click, we just call out function which call out User Utility Form
            //Note: We have to modify lstlstlstobjInput!!!
            //For Jig ID-Hard ID-Func ID

            this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput[0][8] = this.clsChildControl.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intJigID;
            this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput[0][9] = this.clsChildControl.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intHardID;
            this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput[0][10] = this.clsChildControl.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intFuncID;

            //Add more info of User Utilities ID
            List<object> lstobjTemp = new List<object>();
            lstobjTemp.Add("intUserUltID");
            lstobjTemp.Add(this.clsChildControl.lstclsUserUlt[intIndexFound].intUserUltID);
            this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput.Add(lstobjTemp);

            //Affect all process ID
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
            {
                object objResult = new object();
                this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput[0][1] = i.ToString(); //If not have this command, only process 0 affected
                objResult = this.clsChildControl.UserUtlSingleFuncExecute(this.clsChildControl.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intPartID, this.clsChildControl.lstclsUserUlt[intIndexFound].lstlstobjInput);
            }
        }

        private void MasterUserUtilityClick(object sender, RoutedEventArgs e)
        {
            //Looking for sender and assign proper handling function
            int i;
            int intIndexFound = 0;

            for (i = 0; i < this.clsMasterProcess.lstclsUserUlt.Count; i++)
            {
                if (sender == this.clsMasterProcess.lstclsUserUlt[i].UserMenuItem)
                {
                    //MessageBox.Show("Hello, My master! I'm test item number " + i.ToString());
                    intIndexFound = i;
                    break;
                }
            }

            //When User Click, we just call out function which call out User Utility Form
            //Note: We have to modify lstlstlstobjInput!!!
            //For Jig ID-Hard ID-Func ID

            this.clsMasterProcess.lstclsUserUlt[intIndexFound].lstlstobjInput[0][8] = this.clsMasterProcess.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intJigID;
            this.clsMasterProcess.lstclsUserUlt[intIndexFound].lstlstobjInput[0][9] = this.clsMasterProcess.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intHardID;
            this.clsMasterProcess.lstclsUserUlt[intIndexFound].lstlstobjInput[0][10] = this.clsMasterProcess.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intFuncID;

            //Add more info of User Utilities ID
            List<object> lstobjTemp = new List<object>();
            lstobjTemp.Add("intUserUltID");
            lstobjTemp.Add(this.clsMasterProcess.lstclsUserUlt[intIndexFound].intUserUltID);
            this.clsMasterProcess.lstclsUserUlt[intIndexFound].lstlstobjInput.Add(lstobjTemp);

            //Executed command
            object objResult = new object();
            objResult = this.clsMasterProcess.UserUtlSingleFuncExecute(this.clsMasterProcess.lstclsUserUlt[intIndexFound].UserUltFuncCatalog.intPartID, this.clsMasterProcess.lstclsUserUlt[intIndexFound].lstlstobjInput);
        }

        //*********************************************************
        /// <summary>
        /// Request Multi Child process run specific function
        ///  - Para1 (13): Child process ID want to request (If -1: then all process will be requested)
        ///  - Para2 (14): Synchronize option. 0: Waiting for all step reach current step. 1: No wait
        ///  - Para3 (15): Jig ID of function requested
        ///  - Para4 (16): Hardware ID of function requested
        ///  - Para5 (17): Function ID requested
        ///  - Para6 (18) => Para20 (32): parameter for function requested (para 1=> para15)
        /// Return: Result of function requested
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID2(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            //Check numeric condition
            int intProcessID = 0;
            int intChildProcessID = 0;
            int intWaitingOption = 0;
            int intPartID, intJigID, intHardID, intFuncID;
            intPartID = 0; intJigID = 0; intHardID = 0; intFuncID = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "Error: ProcessID setting is not integer type!";
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intChildProcessID) == false) return "Error: ChildProcessID setting is not integer type!";
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intWaitingOption) == false) return "Error: Waiting Option setting is not integer type!";
            if (int.TryParse(lstlstobjInput[0][15].ToString(), out intJigID) == false) return "Error: JigID setting is not integer type!";
            if (int.TryParse(lstlstobjInput[0][16].ToString(), out intHardID) == false) return "Error: HardID setting is not integer type!";
            if (int.TryParse(lstlstobjInput[0][17].ToString(), out intFuncID) == false) return "Error: FunctionID setting is not integer type!";

            //Check if Function ID is exist or not
            int i = 0;
            bool blFlagFound = false;
            for (i = 0; i < this.clsChildControl.lstChildProcessModel[0].clsChildExtension.IFunctionCatalog.Count; i++)
            {
                if ((intJigID == this.clsChildControl.lstChildProcessModel[0].clsChildExtension.IFunctionCatalog[i].intJigID) &&
                    (intHardID == this.clsChildControl.lstChildProcessModel[0].clsChildExtension.IFunctionCatalog[i].intHardID) &&
                    (intFuncID == this.clsChildControl.lstChildProcessModel[0].clsChildExtension.IFunctionCatalog[i].intFuncID))
                {
                    blFlagFound = true;
                    intPartID = this.clsChildControl.lstChildProcessModel[0].clsChildExtension.IFunctionCatalog[i].intPartID;
                    break;
                }
            }
            if (blFlagFound == false) return "Error: cannot find function: " + intJigID.ToString() + "-"
                                              + intHardID.ToString() + "-" + intFuncID.ToString()
                                              + " in Function Catalog of Child Process!";
            //Re-assign value for function requested
            var lstlstobjInputNew = new List<List<object>>();
            var lstobjInputNew = new List<object>();
            var lstlststrOutputNew = new List<List<object>>();
            //Assign new value
            for (i = 0; i < 8; i++) lstobjInputNew.Add(lstlstobjInput[0][i]); //0-7 Keep same 
            lstobjInputNew.Add(intJigID.ToString()); //8 - new Jig ID
            lstobjInputNew.Add(intHardID.ToString()); //9 - new Hard ID
            lstobjInputNew.Add(intFuncID.ToString()); //10 - new Func ID
            lstobjInputNew.Add(lstlstobjInput[0][11]); //11 - transmit: keep same
            lstobjInputNew.Add(lstlstobjInput[0][12]); //12 - receive: keep same

            //lstobjInputNew.Add(lstlstobjInput[0][17]); //13 -para1
            //lstobjInputNew.Add(lstlstobjInput[0][18]); //14 -para2

            for (i = 18; i < 33; i++) //Only support 15 parameter!!!
            {
                //Becareful that user may be input not full of parameter!!!
                if (i < lstlstobjInput[0].Count) lstobjInputNew.Add(lstlstobjInput[0][i]); //13->28: para1=>para15 (only support maximum 15 parameter)
            }


            //Add to list of list input and call function requested
            lstlstobjInputNew.Add(lstobjInputNew);
            object objTempResult = new object();
            if (intChildProcessID == -1) //request for all?
            {
                for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++) //Apply for all
                {
                    objTempResult = this.clsChildControl.lstChildProcessModel[i].clsChildExtension.lstPluginCollection[intPartID].Value.IFunctionExecute(lstlstobjInputNew, out lstlststrOutputNew);
                    //No care about result?
                }
            }
            else //Request only with one Child process
            {
                objTempResult = this.clsChildControl.lstChildProcessModel[intChildProcessID].clsChildExtension.lstPluginCollection[intPartID].Value.IFunctionExecute(lstlstobjInputNew, out lstlststrOutputNew);
                //No care about result?
            }

            return objTempResult; //return 0 if everything is OK
        }

        //*********************************************************
        /// <summary>
        /// Get Checking Result of all cavity and return to user ret "result" + (ProcessID+1)?
        /// Return "0" if everything is OK
        /// + Para1: process ID want to get info (if -1, then take all process. If empty "" : then take current process)
        /// + Para2: step want to get data (if have more than 1 step, then separate by ',' character)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID3(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();


            int intProcessID = 0;
            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "0";

            string strProcessRequestID = lstlstobjInput[0][13].ToString().Trim();


            int intProcessRequestID = 0;
            if (int.TryParse(strProcessRequestID, out intProcessRequestID) == false)
            {
                intProcessRequestID = -1;// Default is get all process info
            }


            string[] ArrstrInput = lstlstobjInput[0][14].ToString().Trim().Split(',');
            List<string> lststrRequestStep = new List<string>(ArrstrInput);

            int i = 0;
            int j = 0;
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
            {

                if (strProcessRequestID == "") // for current process only
                {
                    if (intProcessID != i) continue;
                }
                else
                {
                    if (intProcessRequestID != -1)
                    {
                        if (intProcessRequestID != intProcessID) continue;
                        if (intProcessRequestID != i) continue;
                    }
                }

                //Create user ret string for each item
                lstobjTemp = new List<object>();

                //Add user description
                if (intProcessRequestID != -1) //for all
                {
                    lstobjTemp.Add("result" + (i + 1).ToString()); //No Index
                }
                else //for only 1 selected process
                {
                    lstobjTemp.Add("result");
                }

                //Add current Checking Process ID
                lstobjTemp.Add(i.ToString()); //Index = 0

                //Add PC name
                lstobjTemp.Add(MyLibrary.clsMyFunc.GetPcName()); //Index = 1

                //Add PC IP address
                lstobjTemp.Add(MyLibrary.clsMyFunc.GetIPv4Address()); //Index = 2

                //Add PC MAC address
                lstobjTemp.Add(MyLibrary.clsMyFunc.GetMACAddress()); //Index = 3

                //Add Tool version & Program list info
                lstobjTemp.Add(this.clsChildControl.strProgramVer); //Index = 4

                //Add Checking Program list info
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsProgramList.strProgramListname); //Index = 5
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsProgramList.strProgramListVersion); //Index = 6

                //Add Origin Step List Info. If not setting using step list => empty string return.
                if(this.clsChildControl.clsMainVar.blUsingOriginSteplist==true)
                {
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsStepList.strStepListname); //Index = 7
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsStepList.strStepListVersion); //Index = 8
                }
                else //No use origin step list
                {
                    lstobjTemp.Add(""); //Index = 7
                    lstobjTemp.Add(""); //Index = 8
                }
                

                //Add Passrate
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsItemResult.dblItemPassRate.ToString()); //Index = 9

                //Add checking time
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsItemResult.dateTimeChecking.ToString("dd/MM/yy")); //Checking date //Index = 10
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsItemResult.dateTimeChecking.ToString("HH:mm:ss")); //Checking time //Index = 11

                //Add Tact time
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].clsItemResult.dblItemTactTime.ToString()); //Index = 12

                //Add checking result
                if (this.clsChildControl.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == true) //PASS case
                {
                    lstobjTemp.Add("PASS"); //Index = 13
                    //If PASS then no have step fail => Save empty string
                    lstobjTemp.Add(""); //Index = 14
                    lstobjTemp.Add(""); //Index = 15
                }
                else //FAIL case
                {
                    lstobjTemp.Add("FAIL"); //Index = 13
                    //Add step fail number
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].lstTotalStep[this.clsChildControl.lstChildProcessModel[i].clsItemResult.intStepFailPos].intStepNumber.ToString()); //Index = 14
                    //Add step fail data
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].lstTotalStep[this.clsChildControl.lstChildProcessModel[i].clsItemResult.intStepFailPos].objStepCheckingData); //Index = 15
                }


                //Attach requested step data
                if (lststrRequestStep.Count != 0)
                {
                    for (j = 0; j < lststrRequestStep.Count; j++) //Index = 15+j+1
                    {
                        int intTemp = 0;
                        if (int.TryParse(lststrRequestStep[j], out intTemp) == false) // not numeric setting
                        {
                            lstobjTemp.Add("");
                            continue;
                        }
                        else
                        {
                            int intStepPos = this.clsChildControl.lstChildProcessModel[i].FindCheckingStepPos(intTemp);
                            if (intStepPos == -1) //not found
                            {
                                lstobjTemp.Add("");
                                continue;
                            }
                            else
                            {
                                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].lstTotalStep[intStepPos].objStepCheckingData);
                            }
                        }
                    }
                }

                lstlstobjOutput.Add(lstobjTemp);
            }

            return "0"; //return 0 if everything is OK 
        }

        /// <summary>
        /// Get Info of Master Process and return to user ret "result"
        /// + Para1 [13]: step want to get data (if have more than 1 step, then separate by ',' character)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID4(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            string[] ArrstrInput = lstlstobjInput[0][13].ToString().Trim().Split(',');
            List<string> lststrRequestStep = new List<string>(ArrstrInput);

            lstobjTemp.Add("result"); //Add user description

            lstobjTemp.Add("-1"); //master process ID is -1 :Index = 0

            lstobjTemp.Add(this.clsChildControl.clsMainVar.intNumItem.ToString()); //Number of child process : Index = 1

            //Add PC name
            lstobjTemp.Add(MyLibrary.clsMyFunc.GetPcName()); //Index = 2

            //Add PC IP address
            lstobjTemp.Add(MyLibrary.clsMyFunc.GetIPv4Address()); //Index = 3

            //Add PC MAC address
            lstobjTemp.Add(MyLibrary.clsMyFunc.GetMACAddress()); //Index = 4

            //Add Tool version & Program list info
            lstobjTemp.Add(this.clsChildControl.strProgramVer); //Index = 5

            //Add master program list info
            lstobjTemp.Add(this.clsMasterProcess.clsMasterProgList.strProgramListname); //Index = 6
            lstobjTemp.Add(this.clsMasterProcess.clsMasterProgList.strProgramListVersion);  //Index = 7


            //Add Checking Program list info
            lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[0].clsProgramList.strProgramListname); //Index = 8
            lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[0].clsProgramList.strProgramListVersion); //Index = 9

            //Add Origin Step List Info. If not setting using step list => empty string return.
            if (this.clsChildControl.clsMainVar.blUsingOriginSteplist == true)
            {
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[0].clsStepList.strStepListname); //Index = 10
                lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[0].clsStepList.strStepListVersion); //Index = 11
            }
            else //No use origin step list
            {
                lstobjTemp.Add(""); //Index = 10
                lstobjTemp.Add(""); //Index = 11
            }


            //Add Passrate
            lstobjTemp.Add(this.clsChildControl.clsMainVar.dblTotal_PassRate.ToString()); //Index = 12

            //Add last time checking
            lstobjTemp.Add(this.clsChildControl.clsMainVar.dateTimeChecking.ToString("dd/MM/yy")); //Checking date //Index = 13
            lstobjTemp.Add(this.clsChildControl.clsMainVar.dateTimeChecking.ToString("HH:mm:ss")); //Checking time //Index = 14

            //Add tact time
            lstobjTemp.Add(this.clsChildControl.clsMainVar.dblTotalTactTime.ToString()); //Index = 15

            //Add total result
            if (this.clsChildControl.clsMainVar.blTotalCheckingResult == true)
            {
                lstobjTemp.Add("PASS"); //Index = 16
            }
            else
            {
                lstobjTemp.Add("FAIL"); //Index = 16
            }

            //Add all cavity result
            int i = 0;
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
            {
                if (this.clsChildControl.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == true)
                {
                    lstobjTemp.Add("PASS"); //Index = 17 + i
                }
                else
                {
                    lstobjTemp.Add("FAIL"); //Index = 17 + i
                }
            }

            //Attach requested step data
            int j = 0;
            if (lststrRequestStep.Count != 0)
            {
                for (j = 0; j < lststrRequestStep.Count; j++) //Index = 18+i+j
                {
                    int intTemp = 0;
                    if (int.TryParse(lststrRequestStep[j], out intTemp) == false) // not numeric setting
                    {
                        lstobjTemp.Add("");
                        continue;
                    }
                    else
                    {
                        int intStepPos = this.clsMasterProcess.FindMasterStepPos(intTemp);
                        if (intStepPos == -1) //not found
                        {
                            lstobjTemp.Add("");
                            continue;
                        }
                        else
                        {
                            lstobjTemp.Add(this.clsMasterProcess.lstTotalStep[intStepPos].objStepCheckingData);
                        }
                    }
                }
            }

            lstlstobjOutput.Add(lstobjTemp);

            return "0"; //Return 0 if everything is OK
        }

        /// <summary>
        /// Get all result (checking result, UserRet…) of Master Process. ALWAYS RETURN "0" !
        /// +Para1 (13): The step Number in master program List want to take data
        /// +UserRet: "result"
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID5(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            int intStepPos = 0;
            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intStepPos) == false) return "0";  //"Error: The step number setting is not integer!";

            intStepPos = this.clsMasterProcess.FindMasterStepPos(intStepPos);
            if (intStepPos == -1) return "0"; //"Error: The step number setting not found in master program list";

            //Now, getting all necessary info
            //1. Copy all return data (UserRet...)
            lstlstobjOutput = new List<List<object>>(this.clsMasterProcess.lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn);

            //2. Adding Result necessary
            lstobjTemp.Add("Mresult");
            lstobjTemp.Add(this.clsMasterProcess.lstTotalStep[intStepPos].objStepCheckingData);

            //3. Combine to 1 return list
            lstlstobjOutput.Add(lstobjTemp);

            return "0"; //Return OK code
        }


        /// <summary>
        /// Get all result (checking result, UserRet…) of checking Process . ALWAYS RETURN "0" !
        /// +Para1 (13): The Process ID want to take data (if input -1: then take all data from all child process)
        /// +Para2 (14): The step Number in checking program List want to take data
        /// +UserRet: "Cresult" or "Cresult1", "Cresult2"...
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID6(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjTemp = new List<object>();

            string strProcessID = lstlstobjInput[0][13].ToString();
            int intProcessID = 0;
            if (int.TryParse(strProcessID, out intProcessID) == false) return "0";//return "Error: the process ID setting is not integer";
            if ((intProcessID < -1) || (intProcessID > this.clsChildControl.lstChildProcessModel.Count)) return "0"; //return "Error: the process ID setting is not in allow range [-1 ~ " + Program.strcMainVar.intNumChildPro.ToString() + "]";


            int intStepPos = 0;
            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intStepPos) == false) return "0"; // return "Error: The step number setting is not integer!";

            intStepPos = this.clsChildControl.lstChildProcessModel[0].FindCheckingStepPos(intStepPos);
            if (intStepPos == -1) return "0";//"Error: The step number setting not found in master program list";

            int i = 0; int j = 0;
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++)
            {
                if (intProcessID == -1) //get all data from all child process
                {
                    for (j = 0; j < this.clsChildControl.lstChildProcessModel[i].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn.Count; j++)
                    {
                        lstobjTemp = new List<object>();
                        lstobjTemp = this.clsChildControl.lstChildProcessModel[i].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn[j];
                        //Add index of child process
                        if (lstobjTemp.Count != 0)
                        {
                            lstobjTemp[0] = lstobjTemp[0].ToString() + (i + 1).ToString();
                        }
                        //add to list return
                        lstlstobjOutput.Add(lstobjTemp);
                    }
                    //Add result of child process
                    lstobjTemp = new List<object>();
                    lstobjTemp.Add("Cresult" + (i + 1).ToString());
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[i].lstTotalStep[intStepPos].objStepCheckingData);
                    //add to list return
                    lstlstobjOutput.Add(lstobjTemp);

                }
                else //Get only 1 child process
                {
                    if (i != intProcessID) continue;
                    //1. copy all return data
                    lstlstobjOutput = new List<List<object>>(this.clsChildControl.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].clsStepDataRet.lstlstobjDataReturn);
                    //2. add result
                    lstobjTemp.Add("Cresult");
                    lstobjTemp.Add(this.clsChildControl.lstChildProcessModel[intProcessID].lstTotalStep[intStepPos].objStepCheckingData);
                    //Add to return data
                    lstlstobjOutput.Add(lstobjTemp);
                }
            }

            return "0"; //Return OK code
        }

        /// <summary>
        /// Saving Child Process addition Info - From Child Process
        ///     +Para1 (13): Optional Saving: 0 (Pre-Info) default. 1 (After-Info)
        ///     +Para2 (14):The number of Info in Pre-Info or After-Info want to saving. Count from 0.
        ///     +Para3 (15): Data want to save to csv file 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID20(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            int intProcessID = 0;
            int intOptionalSaving = 0;
            int intInfoOrder = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "Error: Process ID setting is not integer!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intOptionalSaving) == false)
            {
                intOptionalSaving = 0; //Default
            }

            if ((intOptionalSaving != 0) && (intOptionalSaving != 1)) intOptionalSaving = 0; //Default

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intInfoOrder) == false) return "Error: Info Order setting is not integer!";


            if (intOptionalSaving == 1) //After-Info
            {
                if (intInfoOrder >= this.clsChildControl.lstChildProcessModel[intProcessID].lststrProgramListUserAfterInfo.Count) return "Error: Number of Info Order want to save is over setting limit";
                this.clsChildControl.lstChildProcessModel[intProcessID].lststrProgramListUserAfterInfo[intInfoOrder] = lstlstobjInput[0][15].ToString();
            }
            else //Pre-Info : Default
            {
                if (intInfoOrder >= this.clsChildControl.lstChildProcessModel[intProcessID].lststrProgramListUserPreInfo.Count) return "Error: Number of Info Order want to save is over setting limit";
                this.clsChildControl.lstChildProcessModel[intProcessID].lststrProgramListUserPreInfo[intInfoOrder] = lstlstobjInput[0][15].ToString();
            }

            return "0"; //return 0 if everything is OK
        }

        /// <summary>
        /// Add user info on Header Area
        /// + Para1 (13): Row position of Info to display (count from 0 but not include default info)
        /// + Para2 (14): Title of Info
        /// + Para3 (15): content of user info
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID21(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            //Check valid input data
            int intNumRow = 0;
            if(int.TryParse(lstlstobjInput[0][13].ToString(),out intNumRow)==false)
            {
                return "Error: the Number Row info input [" + lstlstobjInput[0][13].ToString() + "] is not integer!";
            }

            //Now add info to header & display!
            this.clsChildControl.UpdateUserHeaderInfo(intNumRow, lstlstobjInput[0][14].ToString(), lstlstobjInput[0][15].ToString());

            return "0"; //return 0 if everything is OK
        }


        /// <summary>
        /// Saving Child Process addition Info for ORIGIN STEP LIST - From Child Process
        ///     +Para1 (13): Optional Saving: 0 (Pre-Info) default. 1 (After-Info)
        ///     +Para2 (14):The number of Info in Pre-Info or After-Info want to saving. Count from 0.
        ///     +Para3 (15): Data want to save to csv file 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID22(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            int intProcessID = 0;
            int intOptionalSaving = 0;
            int intInfoOrder = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "0-0-22 Error: Process ID setting is not integer!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intOptionalSaving) == false)
            {
                intOptionalSaving = 0; //Default
            }

            if ((intOptionalSaving != 0) && (intOptionalSaving != 1)) intOptionalSaving = 0; //Default

            if (int.TryParse(lstlstobjInput[0][14].ToString(), out intInfoOrder) == false) return "0-0-22 Error: Info Order setting is not integer!";


            if (intOptionalSaving == 1) //After-Info
            {
                if (intInfoOrder >= this.clsChildControl.lstChildProcessModel[intProcessID].lststrStepListUserAfterInfo.Count) return "0-0-22 Error: Number of Info Order want to save is over setting limit";
                this.clsChildControl.lstChildProcessModel[intProcessID].lststrStepListUserAfterInfo[intInfoOrder] = lstlstobjInput[0][15].ToString();
            }
            else //Pre-Info : Default
            {
                if (intInfoOrder >= this.clsChildControl.lstChildProcessModel[intProcessID].lststrStepListUserPreInfo.Count) return "0-0-22 Error: Number of Info Order want to save is over setting limit";
                this.clsChildControl.lstChildProcessModel[intProcessID].lststrStepListUserPreInfo[intInfoOrder] = lstlstobjInput[0][15].ToString();
            }

            return "0"; //return 0 if everything is OK
        }


         /// <summary>
        /// Saving Child Process addition Info for ORIGIN STEP LIST - From Child Process
        ///     +Para1 (13): Optional Saving: 0 (Fail label) default. 1 (Pass label)
        ///     +Para2 (14): New value for Pass/Fail label
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID23(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lstobjOutput = new List<object>();

            int intProcessID = 0;
            int intOptionalSaving = 0;

            if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false) return "0-0-23 Error: Process ID setting is not integer!";

            if (int.TryParse(lstlstobjInput[0][13].ToString(), out intOptionalSaving) == false)
            {
                intOptionalSaving = 0; //Default
            }

            if ((intOptionalSaving != 0) && (intOptionalSaving != 1)) intOptionalSaving = 0; //Default


            if (intOptionalSaving == 1) //Pass label
            {
                this.clsChildControl.lstChildProcessModel[intProcessID].clsChildSetting.strPassLabel = lstlstobjInput[0][14].ToString().Trim();
            }
            else //Fail label
            {
                this.clsChildControl.lstChildProcessModel[intProcessID].clsChildSetting.strFailLabel = lstlstobjInput[0][14].ToString().Trim();
            }

            return "0"; //return 0 if everything is OK
        }



        /// <summary>
        /// Confirm Child Process Checking Result
        /// + Para1 (13): ID of Child Process want to confirm
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID51(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            int i = 0;
            //1. check condition: all step number with class ID = 3 if checked must be OK
            bool blResultCheck = true;
            for (i = 0; i < clsChildProcess.lstTotalStep.Count;i++)
            {
                if (clsChildProcess.lstTotalStep[i].intStepClass != 3) continue; //No care
                //
                if(clsChildProcess.lstTotalStep[i].blStepChecked==true)
                {
                    if(clsChildProcess.lstTotalStep[i].blStepResult==false)
                    {
                        blResultCheck = false;
                        break;
                    }
                }
            }

            //2. If result is OK, check 2nd condition: all step list if using must be check & result OK
            //Confirm condition with Step List Step if setting
            //Pass condition: All step in step list must be checked & must be OK
            if(blResultCheck ==true)
            {
                if (this.clsChildProcess.clsChildSetting.blUsingOriginSteplist == true)
                {
                    for (i = 0; i < this.clsChildProcess.clsStepList.lstExcelList.Count; i++)
                    {
                        int intStepPos = 0;
                        intStepPos = this.clsChildProcess.FindCheckingStepPos(this.clsChildProcess.clsStepList.lstExcelList[i].intStepNumber);
                        if (intStepPos == -1) //Not found - something wrong
                        {
                            blResultCheck = false;
                            break; //No need confirm anymore - break loop i
                        }
                        //Check all step
                        if ((this.clsChildProcess.lstTotalStep[intStepPos].blStepChecked == false) || (this.clsChildProcess.lstTotalStep[intStepPos].blStepResult == false)) //Not yet checked or checked but fail
                        {
                            blResultCheck = false;
                            break; //No need confirm anymore - break loop i
                        }
                    }
                }
            }
            
            if(blResultCheck==true)
            {
                return "1"; //Pass
            }
            else
            {
                return "0"; //Fail
            }
        }

        /// <summary>
        /// Reset all Jumping Times in checking process program list (Parallel or Single Thread Mode)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID52(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            //Reset all checking process
            int i = 0;
            for (i = 0; i < this.clsChildControl.lstChildProcessModel.Count; i++) //Counting each Number checker
            {
                this.clsChildControl.lstChildProcessModel[i].ResetParameterEx();
            }

            return 0; //return 0 if everything is OK
        }

        /// <summary>
        /// TCP/IP server start
        /// This Function. Only done with Process ID = -1 (Server start only 1 time & only with Master Process has ID = -1)
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginSystemControlFuncID500(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            //string strErrMsg = "";
            //string strTemp = "";

            //int iret = 0;

            ////Check if Process ID = 0 or not
            //int intProcessID = 0;
            //if (lstlstobjInput[0].Count < 2) return "Error";
            //if (int.TryParse(lstlstobjInput[0][1].ToString(), out intProcessID) == false)
            //{
            //    return "Error";
            //}
            //else
            //{
            //    intProcessID = int.Parse(lstlstobjInput[0][1].ToString());
            //}

            //if (intProcessID == -1) //Only start with the Master process ID = -1
            //{
            //    try
            //    {
            //        Program.clsTcpIp = new clsTcpIpHandle();

            //        strTemp = Program.clsTcpIp.SocketIniFileReading();
            //        if (strTemp != "0") return strTemp;

            //        strTemp = Program.clsTcpIp.SocketServerStart();
            //        if (strTemp != "0") return strTemp;

            //    }
            //    catch (Exception ex)
            //    {
            //        strErrMsg = ex.Message;
            //        iret = 9999; //Unexpected Error code
            //    }
            //}
            ////Add error message if NG
            //if (iret != 0)
            //{
            //    lststrTemp.Add("ErrMsg");
            //    lststrTemp.Add(strErrMsg);
            //}

            //return iret.ToString(); //Get number of item setting

            return "Error: Not yet support TCP/IP command!";

        }

    }
}
