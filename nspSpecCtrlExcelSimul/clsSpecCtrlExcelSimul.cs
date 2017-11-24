using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;
using nspMasterProcessModel;
using nspChildProcessModel;
using System.Collections;


namespace nspSpecCtrlLogical
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,ExcelSimul")]
    public class clsSpecCtrlExcelSimul : nspINTERFACE.IPluginExecute
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
            strTemp = "Note, building Logical function for special control"; lstobjInfo.Add(strTemp);

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

            //
            lstSettingCommand.Add(new clsSettingForCommand("ABS", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("AVERAGE", 1, 0));

            lstSettingCommand.Add(new clsSettingForCommand("ISNUMBER", 1, 0));

            lstSettingCommand.Add(new clsSettingForCommand("MEDIAN", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("MEDIANBLUR", 2, 0));

            lstSettingCommand.Add(new clsSettingForCommand("MAX",1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("MAXINDEX", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("MIN", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("MININDEX", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("FINDINDEX", 2, 0));
            
            lstSettingCommand.Add(new clsSettingForCommand("ROUND", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("ROUNDUP", 1, 0));
            lstSettingCommand.Add(new clsSettingForCommand("ROUNDDOWN", 1, 0));
        }

        public object ExecuteSpecialFunction(ref clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "ABS":
                    return cmdABS(ref clsInput);
                case "AVERAGE":
                    return cmdAVERAGE(ref clsInput);
                case "ISNUMBER":
                    return cmdISNUMBER(ref clsInput);

                case "MEDIAN":
                    return cmdMEDIAN(ref clsInput);
                case "MEDIANBLUR":
                    return cmdMEDIANBLUR(ref clsInput);
                case "MAX":
                    return cmdMAX(ref clsInput);
                case "MAXINDEX":
                    return cmdMAXINDEX(ref clsInput);
                case "MIN":
                    return cmdMIN(ref clsInput);
                case "MININDEX":
                    return cmdMININDEX(ref clsInput);
                case "FINDINDEX":
                    return cmdFINDINDEX(ref clsInput);

                case "ROUND":
                    return cmdROUND(ref clsInput);
                case "ROUNDUP":
                    return cmdROUNDUP(ref clsInput);
                case "ROUNDDOWN":
                    return cmdROUNDDOWN(ref clsInput);
                
                default:
                    return "PluginSpecialControl,ExcelSimul - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //****************************************************************************************************************************
        public object cmdABS(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdABS error: There is not enough parameter";
            //
            //
            decimal dcmNumber = 0;
            string strtemp = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            if (decimal.TryParse(strtemp, System.Globalization.NumberStyles.Float,null, out dcmNumber) == false)
            {
                return "cmdABS error: Number Input [" + strtemp + "] is not numeric";
            }
            
            //
            return Math.Abs(dcmNumber);
        }
        

        //****************************************************************************************************************************
        public object cmdAVERAGE(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdAVERAGE error: There is not enough parameter";

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++) //
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input
                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdAVERAGE error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    string strtemp = clsInput.args.Parameters[i].Evaluate().ToString().Trim();
                    if (double.TryParse(strtemp, out dblTemp) == false)
                    {
                        return "cmdAVERAGE error: Parameter " + (i + 1).ToString() + " [" + strtemp + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }
            //OK, we got a list of value. Find min value & return
            double dblAverage = lstdblVal.Average();
            return dblAverage;
        }
        
        //****************************************************************************************************************
        public object cmdISNUMBER(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length == 0) return "cmdISNUMERIC error: There is no parameter";

            //
            decimal dcTemp = 0;
            bool blRet = true;
            string strtemp = clsInput.args.Parameters[0].Evaluate().ToString();
            
            //Check if format is float with "E"
            if(decimal.TryParse(strtemp.Trim(),System.Globalization.NumberStyles.Float,null, out dcTemp)==false)
            {
                blRet = false;
            }
            
            //
            return blRet;
        }

        //****************************************************************************************************************
        public object cmdMEDIAN(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length == 0) return "cmdMEDIAN error: There is no parameter";
            //
            //
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input

                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for(int j =0;j<lstTemp.Count;j++)
                    {
                        if(double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp)==false)
                        {
                            //Just like excel, if input is not numeric => not count to median value
                            //return "cmdMEDIAN error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        else
                        {
                            dblTemp = double.Parse(lstTemp[j].ToString().Trim());
                            lstdblTemp.Add(dblTemp);
                        }
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        //Just like excel, if input is not numeric => not count to median value
                        //return "cmdMEDIAN error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    else
                    {
                        dblTemp = double.Parse(objRet.ToString().Trim());
                        lstdblTemp.Add(dblTemp);
                    }
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }

            //Cal medial filer value
            object objResult = MedianFilterCal(lstdblVal);

            //
            return objResult;
        }

        //****************************************************************************************************************
        public object cmdMEDIANBLUR(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "cmdMEDIANBLUR error: There is not enough parameter";
            //
            List<object> lstdblResult = new List<object>();
               
            //The last value is Kernel size of Median Blurring function, check it out
            int intKernelSize = 0;
            string strtemp = "";
            strtemp = clsInput.args.Parameters[clsInput.args.Parameters.Length - 1].Evaluate().ToString().Trim();
            if(int.TryParse(strtemp, out intKernelSize)==false)
            {
                return "cmdMEDIANBLUR error: The kernel size input [" + strtemp + "] cannot converted to integer";
            }

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length - 1; i++) //The last one is kernel size value
            {
                List<double> lstdblTemp = new List<double>();

                object objRet = clsInput.args.Parameters[i].Evaluate();
                //1. Check if each parameter is list of number or just single number input
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdMEDIANBLUR error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        return "cmdMEDIANBLUR error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }

            //OK, now we got a list of value & kernell size. Let's do Median Blurring
            if(lstdblVal.Count<=intKernelSize) //Well, not enough element, just return origin list
            {
                return lstdblVal;
            }
            else
            {
                //Do median filter for each kernel size number in List value, and add each result to create new list
                for(i=0;i<lstdblVal.Count;i++)
                {
                    //Create new list
                    List<double> lstdblTemp = new List<double>();
                    if ((i+intKernelSize)<=(lstdblVal.Count-1)) //not yet reach the last round
                    {
                        lstdblTemp = lstdblVal.GetRange(i, intKernelSize);
                    }
                    else //Reach the last round
                    {
                        lstdblTemp = lstdblVal.GetRange(i, lstdblVal.Count - i);
                    }

                    //Sub list is ready, do median filter for it
                    object objResult = MedianFilterCal(lstdblTemp);

                    //Try to convert to double before add to new list
                    if (double.TryParse(objResult.ToString(), out dblTemp) == false)
                    {
                        return "cmdMEDIANBLUR error: median result " + (i + 1).ToString() + " [" + objResult.ToString() + "] is not numeric";
                    }
                    //if everything is OK, add to new list to return
                    lstdblResult.Add(dblTemp);
                }
            }

            if (lstdblResult.Count != lstdblVal.Count) return "cmdMEDIANBLUR error: result has the number of value [" + lstdblResult.Count.ToString() + "] is not same with origin [" + lstdblVal.Count.ToString() + "]";

            //
            return lstdblResult;
        }

        //****************************************************************************************************************************
        public object cmdMAX(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 1) return "cmdMAX error: There is not enough parameter";
            if (clsInput.args.Parameters.Length < 1) return "cmdMAX error: There is not enough parameter";

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++) //
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input
                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdMAX error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        return "cmdMAX error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }
            //OK, we got a list of value. Find max value & return
            double dblMax = lstdblVal.Max();
            return dblMax;
        }

        //****************************************************************************************************************************
        public object cmdMAXINDEX(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 1) return "cmdMAXINDEX error: There is not enough parameter";
            if (clsInput.args.Parameters.Length < 1) return "cmdMAXINDEX error: There is not enough parameter";

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++) //
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input
                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdMAXINDEX error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        return "cmdMAXINDEX error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }
            //OK, we got a list of value. Find max value & return
            double dblMax = lstdblVal.Max();
            int intTemp = lstdblVal.FindIndex(x=>x==dblMax);
            if (intTemp == -1) return "cmdMAXINDEX error: cannot find value [" + dblMax.ToString() + "]";
            return intTemp;
        }

        //****************************************************************************************************************************
        public object cmdMIN(ref clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 1) return "cmdMIN error: There is not enough parameter";
            if (clsInput.args.Parameters.Length < 1) return "cmdMIN error: There is not enough parameter";

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++) //
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input
                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdMIN error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        return "cmdMIN error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }
            //OK, we got a list of value. Find min value & return
            double dblMin = lstdblVal.Min();
            return dblMin;
        }

        //****************************************************************************************************************************
        public object cmdMININDEX(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdMININDEX error: There is not enough parameter";

            //Now get all value input
            List<double> lstdblVal = new List<double>();
            double dblTemp = 0;

            //Check if value input is numeric or not
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++) //
            {
                List<double> lstdblTemp = new List<double>();
                //1. Check if each parameter is list of number or just single number input
                object objRet = clsInput.args.Parameters[i].Evaluate();
                if (this.isGenericList(objRet) == true) //Input parameter is list of value
                {
                    IList lstTemp = (IList)objRet;
                    for (int j = 0; j < lstTemp.Count; j++)
                    {
                        if (double.TryParse(lstTemp[j].ToString().Trim(), out dblTemp) == false)
                        {
                            return "cmdMININDEX error: List value " + (i + 1).ToString() + " has value number " + (j + 1).ToString() + " [" + lstTemp[j].ToString() + "] is not numeric";
                        }
                        //
                        lstdblTemp.Add(dblTemp);
                    }
                }
                else //Just a single number
                {
                    if (double.TryParse(objRet.ToString().Trim(), out dblTemp) == false)
                    {
                        return "cmdMININDEX error: Parameter " + (i + 1).ToString() + " [" + objRet.ToString() + "] is not numeric";
                    }
                    //
                    lstdblTemp.Add(dblTemp);
                }

                //Add all to list of value
                lstdblVal.AddRange(lstdblTemp);
            }

            //OK, we got a list of value. Find min value index & return
            double dblMin = lstdblVal.Min();
            int intTemp = lstdblVal.FindIndex(x => x == dblMin);
            if (intTemp == -1) return "cmdMAXINDEX error: cannot find value [" + dblMin.ToString() + "]";
            return intTemp;
        }

        //****************************************************************************************************************************
        public object cmdFINDINDEX(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "cmdFINDINDEX error: There is not enough parameter";

            //
            //1. Check if each parameter is list of object or just single object
            object objRet = clsInput.args.Parameters[0].Evaluate();
            if (this.isGenericList(objRet) == false) //Input parameter is list of object
            {
                return "cmdFINDINDEX error: First parameter input is not list of object";
            }

            //
            IList lstobjInput = (IList)objRet;
            int i = 0;
            List<object> lstTemp = new List<object>();
            for (i = 0; i < lstobjInput.Count; i++)
            {
                lstTemp.Add(lstobjInput[i]);
            }

            //Searching
            object objTemp = clsInput.args.Parameters[1].Evaluate();
            int intTemp = lstTemp.FindIndex(x => x.ToString() == objTemp.ToString());

            //
            if (intTemp == -1) return "cmdFINDINDEX error: cannot find object [" + objTemp + "] in list";
            //
            return intTemp;
        }


        //************************************************************************f****************************************************
        public object cmdROUND(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 2) return "cmdROUND error: There is not enough parameter";
            //
            int intNumDigit = 0;
            object objRet = clsInput.args.Parameters[1].Evaluate();
            if(int.TryParse(objRet.ToString().Trim(),out intNumDigit)==false)
            {
                return "cmdROUND error: Number of Digits [" + objRet.ToString() + "] is not numeric";
            }
            
            //
            decimal dcmNumber = 0;
            object objNumber = clsInput.args.Parameters[0].Evaluate();
            if (decimal.TryParse(objNumber.ToString().Trim(), System.Globalization.NumberStyles.Float,null, out dcmNumber) == false)
            {
                return "cmdROUND error: Number Input [" + objNumber.ToString() + "] is not numeric";
            }
            

            //Ok, now round it
            decimal dcmResult = Math.Round(dcmNumber, intNumDigit);
            return dcmResult;
        }

        //****************************************************************************************************************************
        public object cmdROUNDUP(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdROUNDUP error: There is not enough parameter";

            //
            int intNumDigit = 0;
            if (clsInput.args.Parameters.Length >= 2)
            {
                object objDigit = clsInput.args.Parameters[1].Evaluate();
                if (int.TryParse(objDigit.ToString().Trim(), out intNumDigit) == false)
                {
                    return "cmdROUNDUP error: Number of Digits [" + objDigit.ToString() + "] is not numeric";
                }
            }
            //
            double dblNumber = 0;
            object objNumber = clsInput.args.Parameters[0].Evaluate();
          
            //If cannot Parse, then check again if data input is Float format which contains "E" character
            if (double.TryParse(objNumber.ToString().Trim(), System.Globalization.NumberStyles.Float, null, out dblNumber) == false)
            {
                return "cmdROUNDUP error: Number Input [" + objNumber.ToString() + "] is not numeric";
            }
            
            //Ok, now round it
            double dblResult = Math.Ceiling(dblNumber * Math.Pow(10, intNumDigit)) / Math.Pow(10, intNumDigit);
            return dblResult;
        }

        //****************************************************************************************************************************
        public object cmdROUNDDOWN(ref clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "cmdROUNDDOWN error: There is not enough parameter";
            //
            int intNumDigit = 0;
            if (clsInput.args.Parameters.Length >= 2)
            {
                object objDigit = clsInput.args.Parameters[1].Evaluate();
                if (int.TryParse(objDigit.ToString().Trim(), out intNumDigit) == false)
                {
                    return "cmdROUNDDOWN error: Number of Digits [" + objDigit.ToString() + "] is not numeric";
                }
            }
            //
            double dblNumber = 0;
            object objNumber = clsInput.args.Parameters[0].Evaluate();
         
            //If cannot Parse, then check again if data input is Float format which contains "E" character
            if (double.TryParse(objNumber.ToString().Trim(), System.Globalization.NumberStyles.Float, null, out dblNumber) == false)
            {
                return "cmdROUNDDOWN error: Number Input [" + objNumber.ToString() + "] is not numeric";
            }
            

            //Ok, now round it
            double dblResult = Math.Floor(dblNumber * Math.Pow(10, intNumDigit)) / Math.Pow(10, intNumDigit);
            return dblResult;
        }

        //****************************************************************************************************************************
        public object MedianFilterCal(List<double> lstdblInput)
        {
            if (lstdblInput.Count == 0) return "Error: There is no value!";

            double dblResult = 0;
            List<double> lstTemp = new List<double>(lstdblInput);

            //sorting list from smallest to biggest value
            lstTemp.Sort();

            if ((lstTemp.Count % 2) == 0)  // count is even, average two middle elements
            {
                // count is even, average two middle elements
                dblResult = (lstTemp[(lstTemp.Count / 2) - 1] + lstTemp[lstTemp.Count / 2]) / 2;
            }
            else // count is odd, return the middle element
            {
                dblResult = lstTemp[(int)(lstTemp.Count / 2)];
            }

            return dblResult;
        }
        //****************************************************************************************************************************
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
        //****************************************************************************************************************************
    
    }
}
