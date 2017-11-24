using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
//using System.Windows.Forms;
using nspMEFLoading;
using nspCFPInfrastructures;
using nspINTERFACE;
using NCalc2.Expressions;

namespace nspCFPExpression
{
    public class clsSettingForCommand
    {
        public string strDetectCode { get; set; } //The special function name 
        public int intNumPara { get; set; } //The minimum parameter request 
        public int intAffectArea { get; set; } //Special function will be executed before or after each step in program list running
        public int intTargetProcessID { get; set; }  //Indicate which process want to apply. 
        //intTargetProcessID = 0: for all process (default)
        //intTargetProcessID = 1: for master only. 
        //intTargetProcessID = 2: for child process only
        //intTargetProcessID = 3: for single thread process only

        //constructor
        public clsSettingForCommand(string DetectCode, int NumPara, int AffectArea = 0, int TargetProcessID = 0)
        {
            strDetectCode = DetectCode;
            intNumPara = NumPara;
            intAffectArea = AffectArea;
            intTargetProcessID = TargetProcessID;
        }
    }

    //For store all result of analyze process of each step, for using later
    public class clsCommonCommandGuider
    {
        public clsSettingForCommand clsSettingCommand { get; set; }
        public List<object> lstobjCmdPara { get; set; } //Detail of each parameter - should not be changed in running time
        public List<object> lstobjCmdParaEx { get; set; } //The copy of lstCmdPara with each parameter can be changed in running time

        public int intSourcesID { get; set; } //Indicate what is source process
        //intSourcesID = 0: for all process. 
        //intSourcesID = 1: for master only. 
        //intSourcesID = 2: for child process only
        //intSourcesID = 3: for single thread process only
        public object objSources { get; set; } //master process model or child process model or single thread process model
        public int intProcessId { get; set; } //Indicate what process ID is running (normally use to indicate what child process is running only)

        //For Marking step position & token for Master Process, Child process & single thread process
        public int intStepPos { get; set; }
        public int intToken { get; set; }

        //For Jumping update token
        public bool blRequestUpdateToken { get; set; }

        //For new function expression using Ncalc engine
        public NCalc2.Expression NcalcExpression { get; set; } //This should be assigned common for all class command guider
        public NCalc2.FunctionArgs args { get; set; } //arguments passing for special control command when detect function name matching
        public NCalc2.Expressions.LogicalExpression ParsedExpression { get; set; } //saving analyzed expression when initialization

        public object evaluate()
        {
            object objRet = new object();
            //
            objRet = this.NcalcExpression.Evaluate(this.ParsedExpression);
            //
            return objRet;
        }
    }

    //***********************************************************************************************************
    //************************** FOR COMMON FUNCTION - MASTER & CHILD PROCESS ***********************************
    //***********************************************************************************************************
    public class clsCommonSpecialControlFunction
    {
        //For MEF handle
        public nspMEFLoading.clsMEFLoading.clsExtensionHandle clsExtension { get; set; }

        //Storing source object
        public int intSourcesID { get; set; } //Indicate what is source process
        public object objSources { get; set; } //master process model or child process model or single thread process model
        public int intProcessId { get; set; } //Indicate what process ID is running (normally use to indicate what child process is running only)

        //For new method of special control expression calculation
        public NCalc2.Expression NCalcExpression { get; set; } 

        //
        public List<clsSettingForCommand> lstSettingCommand; //contain all supported special command

        //Steplist have many step, each step need 1 command analyzer, for all steplist, we need a list for store all command analyzer
        // 1 special command => Need 1 command guider
        // 1 step in step list may have more than 1 special command => Need a list of command guider
        // A steplist - have many step => Need a list of list command guider
        public List<List<clsCommonCommandGuider>> lstlstCommonCommandAnalyzer { get; set; }

        //For Parameter Special Command (Maximum 20 Parameter?)
        //Step list has many step
        //Each step has total 20 parameter column
        //=>We need list of list command analyzer!
        public List<List<clsCommonCommandGuider>> lstlstCommonParaCommandAnalyzer { get; set; }

        //For Transmission area
        //Step list has a number of step => List of step
        //Each step has 1 transmission area which content a factory command
        //Each factory command maybe include a number of Special command => List of special command
        // => We need a List of List Command analyzer
        public List<List<clsCommonCommandGuider>> lstlstCommonTransmissionCommandAnalyzer { get; set; }

        public void LoadingSpecialModule()
        {
            //Ini for extension class
            this.clsExtension = new clsMEFLoading.clsExtensionHandle();

            //Loading Plug-in
            this.clsExtension.PluginLoaderSpecialControl("Extensions");

            //Analyze to get all command list
            this.lstSettingCommand = new List<clsSettingForCommand>();

            int i = 0;
            int j = 0;
            for (i = 0; i < this.clsExtension.lstPluginCollection.Count; i++)
            {
                //Looking for information from plugin
                List<List<object>> lstlstInput = new List<List<object>>();
                List<List<object>> lstlstOutput = new List<List<object>>();
                List<string> lstTemp = new List<string>();

                //Get info from plug-in 
                this.clsExtension.lstPluginCollection[i].Value.IGetPluginInfo(lstlstInput, out lstlstOutput);

                if (lstlstOutput.Count == 0) continue;

                //Analyze received info
                string strKey = "lstSettingCommand";
                for (j = 0; j < lstlstOutput.Count; j++)
                {
                    if (lstlstOutput[j].Count < 2) continue;

                    if (lstlstOutput[j][0].ToString().Trim().ToUpper() == strKey.ToUpper().Trim()) //Found list attach
                    {
                        if (lstlstOutput[j][1] is List<clsSettingForCommand>)
                        {
                            var clsTemp = new List<clsSettingForCommand>();
                            clsTemp = (List<clsSettingForCommand>)lstlstOutput[j][1];

                            this.lstSettingCommand.AddRange(clsTemp);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// In one step may have more than 1 special command: "RetJUMP(233,0,0,235,-1); Test(0,1)"
        /// And we have rule: each special command need to separate by ';' character.
        /// This function, using to find out and separate special command from 1 string: "RetJUMP(233,0,0,235,-1)" and "Test(0,1)"
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public List<string> SeparateSpecialCommand(string strInput, char chrSeparate)
        {
            List<string> lststrRet = new List<string>();

            //Check string input
            if (strInput == null) return lststrRet;
            if (strInput.Trim() == "") return lststrRet;

            //pre-process
            string[] strCommandPack;
            //strCommandPack = strInput.Split(';');
            strCommandPack = strInput.Split(chrSeparate);

            int i = 0;
            for (i = 0; i < strCommandPack.Length; i++)
            {
                strCommandPack[i] = strCommandPack[i].Trim();
                lststrRet.Add(strCommandPack[i]);
            }

            return lststrRet;
        }

        /// <summary>
        /// This function. Do analyze all steplist and take result of special command analyze into a list of list.
        /// </summary> 
        /// <param name="lststrInput"></param> 
        /// <returns></returns>
        public List<List<clsCommonCommandGuider>> CommonSpecialControlIni(List<string> lststrInput)
        {
            //Steplist have a list of step, each step has a list of command guider
            List<List<clsCommonCommandGuider>> lstlstCmdReturn = new List<List<clsCommonCommandGuider>>();

            List<clsCommonCommandGuider> lstTemp = new List<clsCommonCommandGuider>();
            clsCommonCommandGuider clsTemp = new clsCommonCommandGuider();

            int i, j;
            //int intNumPara = 0;
            int intNumStep = 0;

            //intNumPara = lstlststrInput[0].Count; 20 is maximum number of parameter in steplist
            intNumStep = lststrInput.Count;

            for (i = 0; i < intNumStep; i++) //Looking in each step 
            {
                lstTemp = new List<clsCommonCommandGuider>(); //Prepare for each step
                List<string> lststrTemp = new List<string>();
                lststrTemp = SeparateSpecialCommand(lststrInput[i], ';'); //Separate special command package into each special command

                NCalc2.Expression ncalTemp = new NCalc2.Expression();

                //Analyze each possible special command & add to list
                for (j = 0; j < lststrTemp.Count; j++)
                {
                    //clsTemp = cmdCommonAnalyzer(lststrTemp[j]);
                    clsTemp = new clsCommonCommandGuider();

                    //For new ncalc expression analyze
                    string strTemp = lststrTemp[j].ToString().Trim();
                    clsTemp.NcalcExpression = this.NewExpressionIni();

                    if (strTemp != "")
                    {
                        clsTemp.ParsedExpression = ncalTemp.AnalyzeExpression(strTemp);
                    }

                    //Find command setting if parsed expression is function expression
                    clsSettingForCommand clsCommandTemp = new clsSettingForCommand("", 0);
                    if (clsTemp.ParsedExpression is NCalc2.Expressions.FunctionExpression)
                    {
                        var FuncTemp = (NCalc2.Expressions.FunctionExpression)clsTemp.ParsedExpression;
                        
                        foreach (var item in this.lstSettingCommand)
                        {
                            if(item.strDetectCode.ToUpper().Trim()== FuncTemp.Identifier.Name.ToUpper().Trim())
                            {
                                clsCommandTemp = item;
                            }
                        }
                    }
                    clsTemp.clsSettingCommand = clsCommandTemp;
                    clsTemp.objSources = this.objSources;
                    clsTemp.intSourcesID = this.intSourcesID;
                    clsTemp.intProcessId = this.intProcessId;
                    clsTemp.intStepPos = i;

                    //Passing command guider
                    if (clsTemp.ParsedExpression != null)
                    {
                        clsTemp.ParsedExpression.objCommandGuider = clsTemp;
                        clsTemp.ParsedExpression = this.PassingSourceObject(clsTemp.ParsedExpression);
                    }
                    
                    //Add to list
                    lstTemp.Add(clsTemp);
                }

                //Add to list of list
                lstlstCmdReturn.Add(lstTemp);
            }
            //Return value
            return lstlstCmdReturn;

        } //End of CommonSpecialControlIni() method

        /// <returns></returns>
        public List<List<clsCommonCommandGuider>> CommonTransAreaSpecialControlIni(List<string> lststrInput)
        {
            //Steplist have a list of step, each step has a list of command guider
            List<List<clsCommonCommandGuider>> lstlstCmdReturn = new List<List<clsCommonCommandGuider>>();

            List<clsCommonCommandGuider> lstTemp = new List<clsCommonCommandGuider>();
            //clsCommonCommandGuider clsTemp = new clsCommonCommandGuider();

            int i;//, j;

            for (i = 0; i < lststrInput.Count; i++) //Looking in each step 
            {
                lstTemp = AnalyzeSeriSpecialCommand(lststrInput[i],i);

                //
                lstlstCmdReturn.Add(lstTemp);
            }

            //Return value
            return lstlstCmdReturn;

        } //End of CommonTransAreaSpecialControlIni() method

        public List<clsCommonCommandGuider> AnalyzeSeriSpecialCommand(string strInput, int intStepPos)
        {
            List<clsCommonCommandGuider> lstTemp = new List<clsCommonCommandGuider>();
            clsCommonCommandGuider clsTemp = new clsCommonCommandGuider();

            List<string> lststrTemp = new List<string>();

            if (strInput == null) return lstTemp;

            if (strInput.Trim() == "")
            {
                return lstTemp;
            }


            string strNewCmd = "";
            bool blFinishSearchElement = false;
            bool blSpecialCmd = false;
            int intOpenBraceCount = 0;
            int intCloseBraceCount = 0;
            int i = 0;

            for (i = 0; i < strInput.Length; i++)
            {
                switch (strInput[i])
                {
                    case '(':
                        blSpecialCmd = true; //Marking that the element searching is special command
                        intOpenBraceCount++;
                        strNewCmd = strNewCmd + strInput[i];
                        break;
                    case ')':
                        intCloseBraceCount++;
                        strNewCmd = strNewCmd + strInput[i];
                        break;
                    case ',':
                        if (blSpecialCmd == false) //Finish searching & not add to string if not special command
                        {
                            blFinishSearchElement = true;
                        }
                        else //special command => 2 case
                        {
                            if (intOpenBraceCount == intCloseBraceCount) //Finish seacrhing if find out coressponding ')' character to beginning ')' character
                            {
                                blFinishSearchElement = true;
                            }
                            else //Not yet finish, just add character to string
                            {
                                strNewCmd = strNewCmd + strInput[i];
                            }
                        }
                        break;
                    default:
                        strNewCmd = strNewCmd + strInput[i]; //Not special character => just adding to string
                        break;
                }


                if (strInput.Length >= 1)
                {
                    if (i == (strInput.Length - 1)) //Last character => Finish
                    {
                        blFinishSearchElement = true;
                    }
                }

                if (blFinishSearchElement == true)
                {
                    lststrTemp.Add(strNewCmd);
                    strNewCmd = ""; //Reset for new searching
                    blFinishSearchElement = false; //Reset for new searching 
                    blSpecialCmd = false; //Reset for new searching 
                    intOpenBraceCount = 0; //Reset for new searching 
                    intCloseBraceCount = 0; //Reset for new searching 
                }

            }

            if (strInput.Length >= 1)
            {
                if (strInput[strInput.Length - 1] == ',') //Add if ',' is the last character of string
                {
                    strNewCmd = strNewCmd + strInput[strInput.Length - 1];
                    lststrTemp.Add(strNewCmd);
                }
            }

            NCalc2.Expression ncalTemp = new NCalc2.Expression();

            //Analyze each possible special command & add to list
            for (i = 0; i < lststrTemp.Count; i++)
            {
                //clsTemp = cmdCommonAnalyzer(lststrTemp[i]);
                clsTemp = new clsCommonCommandGuider();

                //For new ncalc expression analyze
                string strTemp = lststrTemp[i].ToString().Trim();
                clsTemp.NcalcExpression = this.NewExpressionIni();
                
                if (strTemp != "")
                {
                    clsTemp.ParsedExpression = ncalTemp.AnalyzeExpression(strTemp);
                }

                //Find command setting if parsed expression is function expression
                clsSettingForCommand clsCommandTemp = new clsSettingForCommand("", 0);
                if (clsTemp.ParsedExpression is NCalc2.Expressions.FunctionExpression)
                {
                    var FuncTemp = (NCalc2.Expressions.FunctionExpression)clsTemp.ParsedExpression;

                    foreach (var item in this.lstSettingCommand)
                    {
                        if (item.strDetectCode.ToUpper().Trim() == FuncTemp.Identifier.Name.ToUpper().Trim())
                        {
                            clsCommandTemp = item;
                        }
                    }
                }
                clsTemp.clsSettingCommand = clsCommandTemp;
                clsTemp.objSources = this.objSources;
                clsTemp.intSourcesID = this.intSourcesID;
                clsTemp.intProcessId = this.intProcessId;
                clsTemp.intStepPos = intStepPos;


                //Passing command guider
                if (clsTemp.ParsedExpression != null)
                {
                    clsTemp.ParsedExpression.objCommandGuider = clsTemp;
                    clsTemp.ParsedExpression = this.PassingSourceObject(clsTemp.ParsedExpression);
                }

                //
                lstTemp.Add(clsTemp);
            }

            return lstTemp;
        }

        public List<List<clsCommonCommandGuider>> CommonParaSpecicalControlIni(List<List<object>> lstlststrInput)
        {
            //Steplist have a list of step, each step has a list of parameter
            List<List<clsCommonCommandGuider>> lstlstCmdReturn = new List<List<clsCommonCommandGuider>>();

            List<clsCommonCommandGuider> lstTemp = new List<clsCommonCommandGuider>();
            clsCommonCommandGuider clsTemp = new clsCommonCommandGuider();

            NCalc2.Expression ncalTemp = new NCalc2.Expression();

            int i, j;
            int intNumPara = 0;
            int intNumStep = 0;

            intNumPara = lstlststrInput[0].Count;
            intNumStep = lstlststrInput.Count;

            for (i = 0; i < intNumStep; i++)
            {
                lstTemp = new List<clsCommonCommandGuider>();

                if (lstlststrInput[i] == null)
                {
                    lstTemp.Add(new clsCommonCommandGuider());
                    lstlstCmdReturn.Add(lstTemp);
                    continue;
                }
                //for (j = 0; j < intNumPara; j++)
                for (j = 0; j < lstlststrInput[i].Count; j++)
                {
                    //
                    clsTemp = new clsCommonCommandGuider();

                    //For new ncalc expression analyze
                    string strTemp = lstlststrInput[i][j].ToString().Trim();
                    clsTemp.NcalcExpression = this.NewExpressionIni();

                    if (strTemp!="")
                    {
                        clsTemp.ParsedExpression = ncalTemp.AnalyzeExpression(strTemp);
                    }

                    //Find command setting if parsed expression is function expression
                    clsSettingForCommand clsCommandTemp = new clsSettingForCommand("", 0);
                    if (clsTemp.ParsedExpression is NCalc2.Expressions.FunctionExpression)
                    {
                        var FuncTemp = (NCalc2.Expressions.FunctionExpression)clsTemp.ParsedExpression;

                        foreach (var item in this.lstSettingCommand)
                        {
                            if (item.strDetectCode.ToUpper().Trim() == FuncTemp.Identifier.Name.ToUpper().Trim())
                            {
                                clsCommandTemp = item;
                            }
                        }
                    }
                    clsTemp.clsSettingCommand = clsCommandTemp;
                    clsTemp.objSources = this.objSources;
                    clsTemp.intSourcesID = this.intSourcesID;
                    clsTemp.intProcessId = this.intProcessId;
                    clsTemp.intStepPos = i;

                    //Passing command guider
                    if (clsTemp.ParsedExpression!=null)
                    {
                        clsTemp.ParsedExpression.objCommandGuider = clsTemp;
                        clsTemp.ParsedExpression = this.PassingSourceObject(clsTemp.ParsedExpression);
                    }
                    
                    //Add to list
                    lstTemp.Add(clsTemp);
                }

                //Add to list of list
                lstlstCmdReturn.Add(lstTemp);
            }
            //Return value
            return lstlstCmdReturn;

        } //End of MasterParaSpecicalControlIni() method

        public object ExecuteSpecialCommand(ref clsCommonCommandGuider clsInput, out bool blFoundCommand)
        {
            //Searching function inside user module for special control
            object objResult = new object();
            int i = 0;
            for (i = 0; i < this.clsExtension.IFunctionCatalog.Count; i++)
            {
                if (clsInput.clsSettingCommand == null) break;

                if (clsInput.clsSettingCommand.strDetectCode == this.clsExtension.IFunctionCatalog[i].strSpecialCmdCode) //Matching
                {
                    List<List<object>> lstlstobjInput = new List<List<object>>();
                    List<List<object>> lstlstobjOutput = new List<List<object>>();

                    blFoundCommand = true;

                    //Cal input list
                    List<object> lstobjInput = new List<object>();
                    lstobjInput.Add("SPECIAL");
                    lstobjInput.Add(clsInput);
                    lstlstobjInput.Add(lstobjInput);

                    objResult = this.clsExtension.lstPluginCollection[this.clsExtension.IFunctionCatalog[i].intPartID].Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);

                    return objResult;
                }
            }

            blFoundCommand = false;
            return "ExecuteSpecialCommand() Error: command not support!";
        }

        //****************************FOR NEW MODULE TO CAL EXPRESSION*****************************************************************
        /// <summary>
        /// With each logical expression, we need to create new & pass Common Command Guider class to each child expression
        /// All child command guider will be common at:
        ///     + Step Position
        ///     + Source ID
        ///     + Process ID
        /// </summary>
        /// <param name="lgExpression"></param>
        /// <returns></returns>
        public LogicalExpression PassingSourceObject(LogicalExpression lgRet)
        {
            //separate case
            if (lgRet is TernaryExpression)
            {
                TernaryExpression TernaryEx = (TernaryExpression)lgRet;

                if (TernaryEx.LeftExpression != null)
                {
                    TernaryEx.LeftExpression = this.AssignCommandGuider(lgRet, TernaryEx.LeftExpression);
                    TernaryEx.LeftExpression = this.PassingSourceObject(TernaryEx.LeftExpression);
                }

                if (TernaryEx.MiddleExpression != null)
                {
                    //TernaryEx.MiddleExpression.objSource = clsNewCommandGuider;
                    TernaryEx.MiddleExpression = this.AssignCommandGuider(lgRet, TernaryEx.MiddleExpression);
                    TernaryEx.MiddleExpression = this.PassingSourceObject(TernaryEx.MiddleExpression);
                }

                if (TernaryEx.RightExpression != null)
                {
                    //TernaryEx.RightExpression.objSource = clsNewCommandGuider;
                    TernaryEx.RightExpression = this.AssignCommandGuider(lgRet, TernaryEx.RightExpression);
                    TernaryEx.RightExpression = this.PassingSourceObject(TernaryEx.RightExpression);
                }

            }
            else if (lgRet is BinaryExpression)
            {
                BinaryExpression BinaryEx = (BinaryExpression)lgRet;

                if (BinaryEx.LeftExpression != null)
                {
                    //BinaryEx.LeftExpression.objSource = clsNewCommandGuider;
                    BinaryEx.LeftExpression = this.AssignCommandGuider(lgRet, BinaryEx.LeftExpression);
                    BinaryEx.LeftExpression = this.PassingSourceObject(BinaryEx.LeftExpression);
                }

                if (BinaryEx.RightExpression != null)
                {
                    //BinaryEx.RightExpression.objSource = clsNewCommandGuider;
                    BinaryEx.RightExpression = this.AssignCommandGuider(lgRet, BinaryEx.RightExpression);
                    BinaryEx.RightExpression = this.PassingSourceObject(BinaryEx.RightExpression);
                }
            }
            else if (lgRet is UnaryExpression)
            {
                UnaryExpression UnaryEx = (UnaryExpression)lgRet;

                if (UnaryEx.Expression != null)
                {
                    //UnaryEx.Expression.objSource = clsNewCommandGuider;
                    UnaryEx.Expression = this.AssignCommandGuider(lgRet, UnaryEx.Expression);
                    UnaryEx.Expression = this.PassingSourceObject(UnaryEx.Expression);
                }
            }
            else if (lgRet is ValueExpression) //Break point of recursive 
            {
                ValueExpression ValueEx = (ValueExpression)lgRet;
            }
            else if (lgRet is FunctionExpression)
            {
                FunctionExpression FunctionEx = (FunctionExpression)lgRet;

                if (FunctionEx.Expressions != null)
                {
                    for (int i = 0; i < FunctionEx.Expressions.Length; i++)
                    {
                        LogicalExpression item = FunctionEx.Expressions[i];

                        if (item != null)
                        {
                            item = this.AssignCommandGuider(lgRet, item);
                            //
                            item = this.PassingSourceObject(item);
                        }
                    }
                }
            }
            else if (lgRet is IdentifierExpression)  //Break point of recursive 
            {
                IdentifierExpression IdentifierEx = (IdentifierExpression)lgRet;
            }
            else  //Break point of recursive 
            {

            }

            //
            return lgRet;
        }

        public LogicalExpression AssignCommandGuider(LogicalExpression lgInputExpression, LogicalExpression lgChildExpression)
        {
            if (lgInputExpression == null) return null;
            if (lgChildExpression == null) return null;


            LogicalExpression lgRet = null;

            //Create new command guider and prepare to pass
            clsCommonCommandGuider clsInputCommandGuider = new clsCommonCommandGuider();
            if (lgInputExpression.objCommandGuider is clsCommonCommandGuider)
            {
                clsInputCommandGuider = (clsCommonCommandGuider)lgInputExpression.objCommandGuider;
            }

            //With each child expression, we will supply a new command guider for it
            clsCommonCommandGuider clsNewCommandGuider = new clsCommonCommandGuider();
            clsNewCommandGuider.objSources = this.objSources;
            clsNewCommandGuider.intStepPos = clsInputCommandGuider.intStepPos;
            clsNewCommandGuider.intSourcesID = this.intSourcesID;
            clsNewCommandGuider.intProcessId = this.intProcessId;
            clsNewCommandGuider.intToken = clsInputCommandGuider.intToken;

            if (lgChildExpression is FunctionExpression)
            {
                FunctionExpression FunctionExTemp = (FunctionExpression)lgChildExpression;
                foreach (var setting in this.lstSettingCommand)
                {
                    if (setting.strDetectCode.ToUpper().Trim() == FunctionExTemp.Identifier.Name.ToUpper().Trim())
                    {
                        clsNewCommandGuider.clsSettingCommand = setting;
                    }
                }
            }

            lgChildExpression.objCommandGuider = clsNewCommandGuider;
            lgRet = lgChildExpression;
            //
            return lgRet;
        }

        /// <summary>
        /// This function create handler for both NCalc function & NCalc parameter in runtime
        /// </summary>
        /// <returns></returns>
        public NCalc2.Expression NewExpressionIni()
        {
            NCalc2.Expression ncalc = new NCalc2.Expression();

            //Add for NCalc Parameters Expression
            ncalc.EvaluateParameter += delegate (string name, NCalc2.ParameterArgs args)
            {
                //if (name == "Pi")
                //    args.Result = 3.14;

                bool blFound = false;
                object objRet = null;
                objRet = this.ParameterExpressionExecute(name, args, out blFound);
            };

            //Add for NCalc Functions Expression
            ncalc.EvaluateFunction += delegate(string name, NCalc2.FunctionArgs args)
            {
                bool blFound = false;
                object objRet = null;
                objRet = this.FunctionExpressionExecute(name, args, out blFound);
                if (blFound == true)
                {
                    args.Result = objRet;
                }
            };
            //
            return ncalc;
        }

        public string CalExpressionTreeResult(NCalc2.Expressions.LogicalExpression lgInput, int intSpaceInput)
        {
            if (lgInput == null) return "\r\n";
            //
            string strRet = "";
            string strTemp = "";
            int i, j = 0;

            //Each time we call this function in another level, add more indication space
            int intSpace = intSpaceInput + 1;
            //
            if ((lgInput == null) || (lgInput.objResult == null)) //This one is Recursion stop condition
            {
                strRet = strRet + "null" + "\r\n";
            }
            else if (lgInput is NCalc2.Expressions.ValueExpression) //This one is Recursion stop condition
            {
                NCalc2.Expressions.ValueExpression ValueEx = (NCalc2.Expressions.ValueExpression)lgInput;
                strRet = strRet + lgInput.ToString() + ":" + ValueEx.Value.ToString() + "\r\n";
            }
            else if (lgInput is NCalc2.Expressions.IdentifierExpression) //This one is Recursion stop condition
            {
                strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
            }
            else if (lgInput is NCalc2.Expressions.FunctionExpression) //This case may do recursion
            {
                strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
                //
                NCalc2.Expressions.FunctionExpression FuncExpress = (NCalc2.Expressions.FunctionExpression)lgInput;
                for (j = 0; j < intSpace; j++)
                {
                    strTemp = strTemp + "___";
                }

                for (i = 0; i < FuncExpress.Expressions.Length; i++)
                {
                    strRet = strRet + strTemp + this.CalExpressionTreeResult(FuncExpress.Expressions[i], intSpace);// + "\r\n";
                }
            }
            else if (lgInput is NCalc2.Expressions.BinaryExpression) //This case may do recursion
            {
                NCalc2.Expressions.BinaryExpression BinaryEx = (NCalc2.Expressions.BinaryExpression)lgInput;
                strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
                for (j = 0; j < intSpace; j++)
                {
                    strTemp = strTemp + "___";
                }

                strRet = strRet + strTemp + this.CalExpressionTreeResult(BinaryEx.LeftExpression, intSpace);// + "\r\n";
                strRet = strRet + strTemp + this.CalExpressionTreeResult(BinaryEx.RightExpression, intSpace);// + "\r\n";
            }
            else if (lgInput is NCalc2.Expressions.TernaryExpression) //This case may do recursion
            {
                NCalc2.Expressions.TernaryExpression TernaryEx = (NCalc2.Expressions.TernaryExpression)lgInput;
                strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
                for (j = 0; j < intSpace; j++)
                {
                    strTemp = strTemp + "___";
                }

                strRet = strRet + strTemp + this.CalExpressionTreeResult(TernaryEx.LeftExpression, intSpace);// + "\r\n";
                strRet = strRet + strTemp + this.CalExpressionTreeResult(TernaryEx.MiddleExpression, intSpace);// + "\r\n";
                strRet = strRet + strTemp + this.CalExpressionTreeResult(TernaryEx.RightExpression, intSpace);// + "\r\n";
            }
            else if (lgInput is NCalc2.Expressions.UnaryExpression) //This case may do recursion
            {
                NCalc2.Expressions.UnaryExpression UnaryEx = (NCalc2.Expressions.UnaryExpression)lgInput;
                strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
                for (j = 0; j < intSpace; j++)
                {
                    strTemp = strTemp + "___";
                }

                strRet = strRet + strTemp + this.CalExpressionTreeResult(UnaryEx.Expression, intSpace);// + "\r\n";
            }
            else //This one is Recursion stop condition
            {
                //strRet = strRet + lgInput.ToString() + ": " + lgInput.objResult.ToString() + "\r\n";
            }

            //
            return strRet;
        }

        public object FunctionExpressionExecute(string strFuncName, NCalc2.FunctionArgs args, out bool blFound)
        {
            object objRet = null;
            blFound = false;
            //Looking for function name
            int i = 0;
            for (i = 0; i < this.clsExtension.IFunctionCatalog.Count; i++)
            {
                if (strFuncName.Trim().ToUpper() == this.clsExtension.IFunctionCatalog[i].strSpecialCmdCode.Trim().ToUpper()) //Matching
                {
                    List<List<object>> lstlstobjInput = new List<List<object>>();
                    List<List<object>> lstlstobjOutput = new List<List<object>>();

                    //Cal input list
                    List<object> lstobjInput = new List<object>();

                    lstobjInput.Add("SPECIAL");

                    //Passing parameter
                    if (args.objCommandGuider is clsCommonCommandGuider)
                    {
                        clsCommonCommandGuider clsCommandGuider = (clsCommonCommandGuider)args.objCommandGuider;
                        clsCommandGuider.args = args;
                    }
                    lstobjInput.Add(args.objCommandGuider);

                    lstlstobjInput.Add(lstobjInput);

                    //execute special command
                    objRet = this.clsExtension.lstPluginCollection[this.clsExtension.IFunctionCatalog[i].intPartID].Value.IFunctionExecute(lstlstobjInput, out lstlstobjOutput);
                    blFound = true;//Marking already found
                    //if already found, no need analyze anymore
                    break;
                }
            }
            //
            return objRet;
        }

        /// <summary>
        /// Adding Parameter to NCalc class
        ///     + Parameter not exist: Create new one
        ///     + Parameter already exist: return error
        /// </summary>
        /// <param name="strParaName"></param>
        /// <param name="objSetValue"></param>
        /// <returns></returns>
        public object AddParameter(string strParaName, object objSetValue)
        {
            //1. First looking if parameter already exist or not
            foreach(var item in this.NCalcExpression.Parameters)
            {
                if(item.Key.ToLower().Trim() == strParaName.ToLower().Trim()) //Matching, already exist => return error message
                {
                    return "Ncalc AddParameter() error: The parameter name [" + strParaName.Trim() + "] already exist!";
                }
            }

            //If not yet exist, create new one!
            this.NCalcExpression.Parameters.Add(strParaName.ToLower().Trim(), objSetValue);
            return 0; //OK code
        }

        /// <summary>
        /// Set new value for Parameter of NCalc class
        ///     + Parameter not exist: Return Error
        ///     + Parameter already exist: set new value for it
        /// </summary>
        /// <param name="strParaName"></param>
        /// <param name="objSetValue"></param>
        /// <returns></returns>
        public object SetParameter(string strParaName, object objSetValue)
        {
            //1. First looking if parameter already exist or not
            bool blFound = false;
            foreach (var item in this.NCalcExpression.Parameters)
            {
                if (item.Key.ToLower().Trim() == strParaName.ToLower().Trim())
                {
                    blFound = true;
                    break;
                }
            }

            if(blFound==false) return "Ncalc SetParameter() error: cannot find parameter name [" + strParaName.Trim() + "]!";

            //If exist, update new value for it
            this.NCalcExpression.Parameters[strParaName.ToLower().Trim()] = objSetValue;
            return 0; //OK code
        }

        /// <summary>
        /// Set new value for Parameter of NCalc class
        ///     + Parameter not exist: Return Error
        ///     + Parameter already exist: Delete it from Ncalc class
        /// </summary>
        /// <param name="strParaName"></param>
        /// <param name="objSetValue"></param>
        /// <returns></returns>
        public object DelParameter(string strParaName)
        {
            //1. First looking if parameter already exist or not
            bool blFound = false;
            foreach (var item in this.NCalcExpression.Parameters)
            {
                if (item.Key.ToLower().Trim() == strParaName.ToLower().Trim())
                {
                    blFound = true;
                    break;
                }
            }

            if (blFound == false) return "Ncalc DelParameter() error: cannot find parameter name [" + strParaName.Trim() + "]!";

            //If exist, update new value for it
            this.NCalcExpression.Parameters.Remove(strParaName.ToLower().Trim());
            return 0; //OK code
        }

        public object ParameterExpressionExecute(string strParaName, NCalc2.ParameterArgs args, out bool blFound)
        {
            object objRet = null;
            blFound = false;
            //Looking for function name
            int i = 0;
            //Looking for Parameter exist or not
            for (i = 0; i < this.NCalcExpression.Parameters.Count; i++)
            {
                if(this.NCalcExpression.Parameters.ContainsKey(strParaName.ToLower().Trim())) //Found
                {
                    blFound = true;
                    args.Result = this.NCalcExpression.Parameters[strParaName.ToLower().Trim()];
                    break;
                }
            }
            //
            return objRet;
        }

        //*****************************************************************************************************************************
        //Constructor
        public clsCommonSpecialControlFunction()
        {
            //Ini for NCalc Expression
            this.NCalcExpression = new NCalc2.Expression();

            //Ini extension module for special control
            LoadingSpecialModule();
        }

    } //End clsCommonSpecialControlFunction

}
