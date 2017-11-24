using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;
using System.Windows;
using System.Collections;
using nspChildProcessModel;
using nspMasterProcessModel;
using System.Windows.Media;

namespace nspSpecCtrlCSharpSimul
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,CSharpSimul")]
    public class clsSpecCtrlCSharpSimul : nspINTERFACE.IPluginExecute
    {
        public List<clsSettingForCommand> lstSettingCommand; //contain all supported special command

        //[Import(typeof(nspCFPInfrastructures.ISystemCmdService))]
        //Lazy<nspCFPInfrastructures.ISystemCmdService> ShellCommand;

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
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 25/Aug/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add Null() function. But be careful with it!"; lstobjInfo.Add(strTemp);

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

            //C# like function & support
            lstSettingCommand.Add(new clsSettingForCommand("NEWVAR", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("SETVAR", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("DELVAR", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("SET", 2, 0)); //1
            lstSettingCommand.Add(new clsSettingForCommand("CALL", 2, 0)); //2
            lstSettingCommand.Add(new clsSettingForCommand("GET", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("ARRAY", 0, 0)); //3
            lstSettingCommand.Add(new clsSettingForCommand("LIST", 0, 0)); //3
            lstSettingCommand.Add(new clsSettingForCommand("SUBLIST", 3, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("GETITEM", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("COUNT", 1, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("POINT", 2, 0)); //4
            lstSettingCommand.Add(new clsSettingForCommand("COLOR", 1, 0)); //4
            lstSettingCommand.Add(new clsSettingForCommand("LISTPOINT", 2, 0)); //5
            lstSettingCommand.Add(new clsSettingForCommand("CHAR", 1, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("TOSTRING", 1, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("TODECIMAL", 1, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("TOINT", 1, 0)); //

            lstSettingCommand.Add(new clsSettingForCommand("NULL", 0, 0)); //5
            lstSettingCommand.Add(new clsSettingForCommand("MSG", 1, 0)); //6

        }

        public object ExecuteSpecialFunction(ref clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "NEWVAR":
                    return cmdNEWVAR(ref clsInput);
                case "SETVAR":
                    return cmdSETVAR(ref clsInput);
                case "DELVAR":
                    return cmdDELVAR(ref clsInput);
                case "SET":
                    return cmdSET(ref clsInput);
                case "CALL":
                    return cmdCALL(ref clsInput);
                case "GET":
                    return cmdGET(ref clsInput);
                case "ARRAY":
                    return cmdARRAY(ref clsInput);
                case "LIST":
                    return cmdLIST(ref clsInput);
                case "SUBLIST":
                    return cmdSUBLIST(ref clsInput);
                case "GETITEM":
                    return cmdGETITEM(ref clsInput);
                case "COUNT":
                    return cmdCOUNT(ref clsInput);
                case "POINT":
                    return cmdPOINT(ref clsInput);
                case "COLOR":
                    return cmdCOLOR(ref clsInput);
                case "LISTPOINT":
                    return cmdLISTPOINT(ref clsInput);
                case "CHAR":
                    return cmdCHAR(ref clsInput);

                case "TOSTRING":
                    return cmdTOSTRING(ref clsInput);
                case "TODECIMAL":
                    return cmdTODECIMAL(ref clsInput);
                case "TOINT":
                    return cmdTOINT(ref clsInput);


                case "NULL":
                    return cmdNULL(ref clsInput);
                case "MSG":
                    return cmdMSG(ref clsInput);
                default:
                    return "PluginSpecialControl,CSharpSimul - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //****************************************************
        public class classVarCollection
        {
            public string strVarName { get; set; }
            public object objVarValue { get; set; }
        }
        public List<classVarCollection> lstclsVarCollection = new List<classVarCollection>();

        public object cmdNEWVAR(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "NEWVAR() Error: not enough parameter!";

            if (clsInput.objSources is clsChildProcessModel)
            {
                var clsChildProcess = (clsChildProcessModel)clsInput.objSources;
                return clsChildProcess.AddParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString(), clsInput.args.Parameters[1].Evaluate());
            }
            else if(clsInput.objSources is clsMasterProcessModel)
            {
                var clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
                return clsMasterProcess.AddParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString(), clsInput.args.Parameters[1].Evaluate());
            }
            else
            {
                return "cmdNEWVAR() error: not support class";
            }
        }

        public object cmdSETVAR(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "SETVAR() Error: not enough parameter!";

            if (clsInput.objSources is clsChildProcessModel)
            {
                var clsChildProcess = (clsChildProcessModel)clsInput.objSources;
                return clsChildProcess.SetParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString(), clsInput.args.Parameters[1].Evaluate());
            }
            else if (clsInput.objSources is clsMasterProcessModel)
            {
                var clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
                return clsMasterProcess.SetParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString(), clsInput.args.Parameters[1].Evaluate());
            }
            else
            {
                return "SETVAR() error: not support class";
            }
        }

        public object cmdDELVAR(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "DELVAR() Error: not enough parameter!";

            if (clsInput.objSources is clsChildProcessModel)
            {
                var clsChildProcess = (clsChildProcessModel)clsInput.objSources;
                return clsChildProcess.DelParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString());
            }
            else if (clsInput.objSources is clsMasterProcessModel)
            {
                var clsMasterProcess = (clsMasterProcessModel)clsInput.objSources;
                return clsMasterProcess.DelParameterToNcalc(clsInput.args.Parameters[0].Evaluate().ToString());
            }
            else
            {
                return "DELVAR() error: not support class";
            }
        }

        //****************************************************
        public object cmdSET(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 2) return "SET() Error: not enough parameter!";
            if (clsInput.args.Parameters.Length < 2) return "SET() Error: not enough 2 parameter!";

            object objTarget = clsInput.args.Parameters[0].Evaluate();
            string strProperty = clsInput.args.Parameters[1].Evaluate().ToString();
            object objNewVal = "";

            if (clsInput.args.Parameters.Length > 2) //optional new value is set
            {
                objNewVal = clsInput.args.Parameters[2].Evaluate();
            }

            try
            {
                if (objTarget.GetType().GetProperty(strProperty) != null)
                {
                    objTarget.GetType().GetProperty(strProperty).SetValue(objTarget, objNewVal, null);
                }
                else
                {
                    return "SET() Error: cannot find property [" + strProperty + "] of object type [" + objTarget.GetType().ToString() +"]";
                }
              
            }
            catch (Exception ex)
            {
                return "SET() Error: " + ex.Message;
            }

            return 0; //OK code
        }

        //****************************************************
        public object cmdCALL(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters == null) return "CALL() Error: null parameter!";
            if (clsInput.args.Parameters.Length < 2) return "CALL() Error: not enough parameter!";


            object objTarget = clsInput.args.Parameters[0].Evaluate();
            string strMethodName = clsInput.args.Parameters[1].Evaluate().ToString();
            object[] objPara;

            if (clsInput.args.Parameters.Length > 2) //optional new value is set
            {
                objPara = new object[clsInput.args.Parameters.Length - 2];
                for (int i = 0; i < objPara.Length; i++)
                {
                    objPara[i] = clsInput.args.Parameters[i + 2].Evaluate();
                }
            }
            else //No parameter method
            {
                objPara = null;
            }

            try
            {
                if (objTarget.GetType().GetMethod(strMethodName) != null)
                {
                    return objTarget.GetType().GetMethod(strMethodName).Invoke(objTarget, objPara);
                }
                else
                {
                    return "CALL() Error: cannot find method [" + strMethodName + "] of object type [" + objTarget.GetType().ToString() + "]";
                }
            }
            catch (Exception ex)
            {
                return "CALL() Error: " + ex.Message;
            }

            //return "0"; //OK code
        }

        //****************************************************
        public object cmdGET(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "GET() Error: not enough 2 parameter!";

            object objTarget = clsInput.args.Parameters[0].Evaluate();
            string strProperty = clsInput.args.Parameters[1].Evaluate().ToString();

            try
            {
                if (objTarget.GetType().GetProperty(strProperty) != null)
                {
                    return objTarget.GetType().GetProperty(strProperty).GetValue(objTarget, null);
                }
                else
                {
                    return "GETVALUE() Error: cannot find property [" + strProperty + "] of object type [" + objTarget.GetType().ToString() + "]";
                }

            }
            catch (Exception ex)
            {
                return "GETVALUE() Error: " + ex.Message;
            }
        }

        //****************************************************
        public object cmdARRAY(ref clsCommonCommandGuider clsInput)
        {
            //return clsInput.lstobjCmdPara;
            if (clsInput.args.Parameters == null) return null;
            List<object> lstobjRet = new List<object>();
            for (int i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                lstobjRet.Add(clsInput.args.Parameters[i].Evaluate());
            }
            //
            return lstobjRet.ToArray();
        }

        //****************************************************
        public object cmdLIST(ref clsCommonCommandGuider clsInput)
        {
            //return clsInput.lstobjCmdPara;
            if (clsInput.args.Parameters == null) return null;
            List<object> lstobjRet = new List<object>();
            for(int i = 0;i<clsInput.args.Parameters.Length;i++)
            {
                lstobjRet.Add(clsInput.args.Parameters[i].Evaluate());
            }
            //
            return lstobjRet;
        }

        //****************************************************
        public object cmdSUBLIST(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 3) return "SUBLIST error: There is not enough parameter";
            if (clsInput.args.Parameters.Length < 3) return "SUBLIST error: There is not enough 3 parameter";

            double dblLowIndex = 0;
            double dblHiIndex = 0;

            object objRet = clsInput.args.Parameters[0].Evaluate();
            if(this.isGenericList(objRet) ==false)
            {
                return "SUBLIST error: the 1st parameter input is not List of object";
            }

            IList lstobjInput = (IList)objRet;

            string strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (double.TryParse(strTemp.Trim(), out dblLowIndex) == false)
            {
                return "SUBLIST error: Low Index input [" + strTemp + "] is not numeric";
            }
            if (dblLowIndex < 0) return "SUBLIST error: Low Index input cannot be Negative";

            strTemp = clsInput.args.Parameters[2].Evaluate().ToString();
            if (double.TryParse(strTemp.Trim(), out dblHiIndex) == false)
            {
                return "SUBLIST error: High Index input [" + strTemp + "] is not numeric";
            }
            if ((int)dblHiIndex < 0) return "SUBLIST error: High Index input cannot be Negative";

            if (dblHiIndex < dblLowIndex) return "SUBLIST error: High Index input less than Low Index";

            if ((int)dblLowIndex > (lstobjInput.Count - 1)) return "SUBLIST error: Low Index [" + ((int)dblLowIndex).ToString() + "] out of range. Max Index is [" + (lstobjInput.Count - 1).ToString() + "]";
            if ((int)dblHiIndex > (lstobjInput.Count - 1)) return "SUBLIST error: High Index [" + ((int)dblHiIndex).ToString() + "] out of range. Max Index is [" + (lstobjInput.Count - 1).ToString() + "]";

            //OK. Now get sub list from origin list
            List<object> lstobjRet = new List<object>();

            //Get sub list & return
            int i = 0;
            for (i = (int)dblLowIndex; i <= (int)dblHiIndex; i++)
            {
                lstobjRet.Add(lstobjInput[i]);
            }
            return lstobjRet;
        }
        //****************************************************
        public object cmdGETITEM(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 2) return "cmdGETITEM error: There is not enough parameter";
            if (clsInput.args.Parameters.Length < 2) return "cmdGETITEM error: There is not enough 2 parameter";

            double dblIndex = 0;

            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (this.isGenericList(objRet) == false)
            {
                return "cmdGETITEM error: the 1st parameter input is not List of object";
            }

            IList lstobjInput = (IList)objRet;

            string strtemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (double.TryParse(strtemp.Trim(), out dblIndex) == false)
            {
                return "cmdGETITEM error: Index input [" + strtemp + "] is not numeric";
            }
            
            //
            if ((int)dblIndex > lstobjInput.Count - 1) return "cmdGETITEM error: Index [" + ((int)dblIndex).ToString() + "] out of range. Max Index is [" + (lstobjInput.Count - 1).ToString() + "]";
            //If everything is OK. Then get item value and return

            return lstobjInput[(int)dblIndex];
        }

        //****************************************************
        public object cmdCOUNT(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count == 0) return "cmdCOUNT error: There is no parameter";
            if (clsInput.args.Parameters.Length == 0) return "cmdCOUNT error: There is no parameter";
            //
            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (this.isGenericList(objRet) == true)
            {
                IList lsttest = (IList)objRet;
                return lsttest.Count;
            }
            else
            {
                return 1;
            }
        }

        public bool isGenericList(object objInput)
        {
            bool blRet = false;
            if (objInput.GetType().IsGenericType == true)
            {
                if (objInput.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    blRet = true;
                }
            }

            return blRet;
        }

        //****************************************************
        public object cmdPOINT(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "POINT() Error: not enough 2 parameter";

            double dblX = 0;
            double dblY = 0;
            string strtemp = "";
            strtemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (double.TryParse(strtemp, out dblX) == false) return "POINT() Error: First parameter input[" + strtemp + "] is not numeric!";
            strtemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (double.TryParse(strtemp, out dblY) == false) return "POINT() Error: Second parameter input[" + strtemp + "] is not numeric!";

            System.Windows.Point pRet = new System.Windows.Point(dblX, dblY);

            return pRet;
        }

        //****************************************************
        public object cmdCOLOR(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdCOLOR() Error: not enough parameter required (1)";

            string strtemp = "";
            strtemp = clsInput.args.Parameters[0].Evaluate().ToString();

            Color MyColor = new Color();
            MyColor = (Color)ColorConverter.ConvertFromString(strtemp.Trim());

            return MyColor;
        }

        //****************************************************
        public object cmdLISTPOINT(ref clsCommonCommandGuider clsInput)
        {
            //This Function support some input mode
            // 1. Input directly "ListPoint(1,5,3,2,70,4)" => Point(1,5) - Point(3,2) - Point(70,4)
            // 2. Input List of X coordinate & Y Codinate "ListPoint((1,3,70), (5,2,4))" => Point(1,5) - Point(3,2) - Point(70,4)

            //List<System.Windows.Point> lstpointRet = new List<System.Windows.Point>();
            List<object> lstpointRet = new List<object>();
            //************************************************************************

            if (clsInput.args.Parameters.Length < 2) return "LISTPOINT() Error: not enough parameter!";

            int intInputMode = 0; //Default - Input directly

            //Detect what input mode
            object obj0 = clsInput.args.Parameters[0].Evaluate();
            object obj1 = clsInput.args.Parameters[1].Evaluate();
            if (this.isGenericList(obj0) == true)
            {
                intInputMode = 1; 
            }

            if(intInputMode==1)
            {
                if (!(this.isGenericList(obj1) == true))
                {
                    return "LISTPOINT() Error: Y coordinate series invalid!";
                }

                IList lstXInput = (IList)obj0;
                IList lstYInput = (IList)obj1;

                int intNumPoint = lstXInput.Count;
                if (intNumPoint != lstYInput.Count) return "LISTPOINT() Error: List of X & List of Y coordinate doesn't have same number of element!";

                //checking & Transfer value
                List<double> lstdblX = new List<double>();
                List<double> lstdblY = new List<double>();

                double dblX = 0;
                double dblY = 0;
                int i = 0;
                for(i = 0;i<intNumPoint;i++)
                {
                    if (double.TryParse(lstXInput[i].ToString(), out dblX) == false) return "LISTPOINT() Error: X element [" + lstXInput[i].ToString() + "] is not numeric!";
                    if (double.TryParse(lstYInput[i].ToString(), out dblY) == false) return "LISTPOINT() Error: Y element [" + lstYInput[i].ToString() + "] is not numeric!";
                    //
                    System.Windows.Point pRet = new System.Windows.Point(dblX, dblY);
                    lstpointRet.Add(pRet);
                }
            }
            else//Default - Input directly
            {
                if ((clsInput.args.Parameters.Length % 2) != 0) return "LISTPOINT() Error: The quantity of number input is not even!";

                int intNumPoint = clsInput.args.Parameters.Length / 2;

                int i = 0;
                double dblX = 0;
                double dblY = 0;
                string strtemp = "";
                for (i = 0; i < intNumPoint; i++)
                {
                    int intXID = 2 * i;
                    int intYID = 2 * i + 1;
                    //Check numeric
                    strtemp = clsInput.args.Parameters[intXID].Evaluate().ToString();
                    if (double.TryParse(strtemp, out dblX) == false) return "LISTPOINT() Error: the parameter input [" + strtemp + "] is not numeric!";
                    strtemp = clsInput.args.Parameters[intYID].Evaluate().ToString();
                    if (double.TryParse(strtemp, out dblY) == false) return "LISTPOINT() Error: the parameter input [" + strtemp + "] is not numeric!";

                    System.Windows.Point pRet = new System.Windows.Point(dblX, dblY);
                    lstpointRet.Add(pRet);
                }
            }

            //************************************************************************
            return lstpointRet;
        }

        //
        //****************************************************
        public object cmdCHAR(ref clsCommonCommandGuider clsInput)
        {
            char chrRet = new char();
            //
            if (clsInput.args.Parameters.Length == 0) return "Char() Error: No parameter";

            byte byteTemp = 0;
            string strtemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (byte.TryParse(strtemp, out byteTemp) == false)
            {
                return "Char() error: Input data[" + strtemp + "] cannot convert to byte";
            }

            chrRet = Convert.ToChar(byteTemp);
            //
            return chrRet;
        }

        //Convert object to string
        public object cmdTOSTRING(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters == null) return "null";
            if (clsInput.args.Parameters.Length == 0) return "null";
            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (objRet == null) return "null";
            return objRet.ToString();
        }

        //Convert object to decimal
        public object cmdTODECIMAL(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters == null) return "null";
            if (clsInput.args.Parameters.Length == 0) return "null";
            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (objRet == null) return "null";

            decimal dcTemp = 0;
            if (decimal.TryParse(objRet.ToString(), System.Globalization.NumberStyles.Float, null, out dcTemp) == false) return "Error: cannot convert [" + objRet.ToString() + "] to decimal.";
            return dcTemp;
        }

        //Convert object to Integer
        public object cmdTOINT(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters == null) return "null";
            if (clsInput.args.Parameters.Length == 0) return "null";
            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (objRet == null) return "null";

            int intTemp = 0;
            if (int.TryParse(objRet.ToString(), out intTemp) == false) return "Error: cannot convert [" + objRet.ToString() + "] to integer.";
            return intTemp;
        }

        //
        public object cmdNULL(ref clsCommonCommandGuider clsInput)
        {
            return null;
        }

        //****************************************************
        public object cmdMSG(ref clsCommonCommandGuider clsInput)
        {
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                MessageBox.Show(clsInput.args.Parameters[i].Evaluate().ToString(), "cmdMSG()");
            }

            if (clsInput.args.Parameters.Length > 0)
            {
                return clsInput.args.Parameters.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}
