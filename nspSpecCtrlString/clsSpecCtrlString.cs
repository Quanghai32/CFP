using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;

namespace nspSpecCtrlString
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,String")]
    public class clsSpecCtrlString : nspINTERFACE.IPluginExecute
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
            strTemp = "Version, 1.02"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 9/Sep/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add CONTAINS() function"; lstobjInfo.Add(strTemp);

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
            object objResult = ExecuteSpecialFunction(clsInput);

            return objResult;
        }

        #endregion


        public void IniSpecialControl()
        {
            lstSettingCommand = new List<clsSettingForCommand>();

            //String Function
            lstSettingCommand.Add(new clsSettingForCommand("EMPTY", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("NEWLINE", 0, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("SUBSTR", 3, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("CONTAINS", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("ADDSTR", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("ISSAMEFORMAT", 2, 0)); //
            lstSettingCommand.Add(new clsSettingForCommand("BSTR", 2, 0)); //
           
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "EMPTY":
                    return cmdEMPTY(clsInput);
                case "NEWLINE":
                    return cmdNEWLINE(clsInput);
                case "SUBSTR":
                    return cmdSUBSTR(clsInput);
                case "CONTAINS":
                    return cmdCONTAINS(clsInput);
                case "ADDSTR":
                    return cmdADDSTR(clsInput);
                case "ISSAMEFORMAT":
                    return cmdISSAMEFORMAT(clsInput);
                case "BSTR":
                    return cmdBSTR(clsInput);
                default:
                    return "PluginSpecialControl,String - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdEMPTY(clsCommonCommandGuider clsInput)
        {
            return "";
        }
        //*****************************************************************************************************************************
        public object cmdNEWLINE(clsCommonCommandGuider clsInput)
        {
            return "\r\n";
        }
        //****************************************************************************************************************
        public object cmdSUBSTR(clsCommonCommandGuider clsInput)
        {
            //if (clsInput.lstobjCmdPara.Count < 3) return "SUBSTR() Error: not enough parameter!";
            if (clsInput.args.Parameters.Length != 3) return "SUBSTR() Error: only valid if 3 parameter input!";


            //string strInput = clsInput.lstobjCmdPara[0].ToString();
            string strInput = clsInput.args.Parameters[0].Evaluate().ToString();
            //string strBeginPos = clsInput.lstobjCmdPara[1].ToString();
            string strBeginPos = clsInput.args.Parameters[1].Evaluate().ToString();
            //string strNewLength = clsInput.lstobjCmdPara[2].ToString();
            string strNewLength = clsInput.args.Parameters[2].Evaluate().ToString();
            
            //SUBSTR(ABCD,1,3) => "BCD" "1001C1"
            int intNewLength = 0;
            int intBeginPos = 0;

            //Check if parameter is a number or not
            if (Int32.TryParse(strNewLength, out intNewLength) == false) return "Error SUBSTR(): New length number input[" + strNewLength + "] is not numerical!";

            if (Int32.TryParse(strBeginPos, out intBeginPos) == false) return "Error SUBSTR(): Begin position number input[" + strBeginPos + "] is not numerical!";

            //OK, now try to cut string
            int intLen = 0;
            intLen = strInput.Length;
            //check illegal substring or not
            if (intNewLength >= intLen) return strInput;
            if ((intBeginPos + intNewLength) > intLen) return "Error SUBSTR(): out of range substring!";

            //Cut a string
            string strNewString = "";
            //strNewString = strInput.Substring((intLen - intNewLength), intNewLength);
            strNewString = strInput.Substring(intBeginPos, intNewLength);

            //If OK, then return new value
            return strNewString;
        }

        //****************************************************************************************************************
        public object cmdCONTAINS(clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length != 2) return "CONTAINS() Error: only valid if 2 parameter input!";
            //
            string strObjective = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            string strTest = clsInput.args.Parameters[1].Evaluate().ToString().Trim();

            bool blResult = strObjective.Contains(strTest);
            return blResult;
        }

        //****************************************************************************************************************
        public object cmdADDSTR(clsCommonCommandGuider clsInput)
        {
            string strResult = "";
            int i = 0;
            for (i = 0; i < clsInput.args.Parameters.Length; i++)
            {
                strResult = strResult + clsInput.args.Parameters[i].Evaluate().ToString();
            }
            return strResult;
        }

        //****************************************************************************************************************
        public object cmdISSAMEFORMAT(clsCommonCommandGuider clsInput)
        {
            //Compare 2 strings and judge them are same format or not (same length - same numeric or character at every position)

            //Check if there is enough 2 parameter
            if (clsInput.args.Parameters.Length != 2) return "cmdISSAMEFORMAT Error: only valid if 2 parameter input!";

            //string strFirst = clsInput.lstobjCmdPara[0].ToString().Trim();
            //string strSecond = clsInput.lstobjCmdPara[1].ToString().Trim();
            string strFirst = clsInput.args.Parameters[0].Evaluate().ToString().Trim();
            string strSecond = clsInput.args.Parameters[1].Evaluate().ToString().Trim();

            //Check same length or not
            if (strFirst.Length != strSecond.Length) return "cmdISSAMEFORMAT Error: Not same length";

            int intLen = strFirst.Length;
            int i = 0;

            //compare each character at each position
            for (i = 0; i < intLen;i++)
            {
                if(Char.IsNumber(strFirst[i])==true) //Numeric
                {
                    if(Char.IsNumber(strSecond[i])==false)
                    {
                        return "cmdISSAMEFORMAT Error: Characters number " + (i+1).ToString() + " are not both numeric";
                    }
                }
                else if(Char.IsLetter(strFirst[i])==true) //Letter
                {
                    if (Char.IsLetter(strSecond[i]) == false)
                    {
                        return "cmdISSAMEFORMAT Error: Characters number " + (i + 1).ToString() + " are not both letter";
                    }
                }
                else //If not numeric or letter => 2 character must be exactly same!
                {
                    if(strFirst[i] != strSecond[i])
                    {
                        return "cmdISSAMEFORMAT Error: Characters number " + (i + 1).ToString() + " are not numeric or letter but not same";
                    }
                }
            }

            //If 2 string is same format, then return true
            return true;
        }

        //****************************************************************************************************************
        public object cmdBSTR(clsCommonCommandGuider clsInput)
        {
            if (clsInput.args.Parameters.Length < 1) return "BSTR() Error: the number of operand is not enough!";

            string strSeparator = clsInput.args.Parameters[0].Evaluate().ToString();

            string strResult = "";
            int i = 0;
            for (i = 1; i < clsInput.args.Parameters.Length; i++)
            {
                if (i != (clsInput.args.Parameters.Length - 1))
                {
                    strResult = strResult + clsInput.args.Parameters[i].Evaluate().ToString() + strSeparator;
                }
                else //The last one do not add separator
                {
                    strResult = strResult + clsInput.args.Parameters[i].Evaluate().ToString();
                }
            }
            return strResult;
        }
        //****************************************************************************************************************
       
    }
}
