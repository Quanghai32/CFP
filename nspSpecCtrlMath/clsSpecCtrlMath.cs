using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using nspINTERFACE;
using nspCFPExpression;

namespace nspSpecCtrlMath
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSpecialControl,Math")]
    public class clsSpecCtrlMath: nspINTERFACE.IPluginExecute
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
            strTemp = "Version, 2.0.0"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 29/Jul/2017"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Deleted unecessary functions"; lstobjInfo.Add(strTemp);

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
            for (i = 0; i < lstlstobjInput.Count;i++)
            {
                lstobjTemp = lstlstobjInput[i];

                for(j=0;j<lstobjTemp.Count;j++)
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

            //Math Function
            lstSettingCommand.Add(new clsSettingForCommand("MOD", 2, 0)); //5
            lstSettingCommand.Add(new clsSettingForCommand("LOG", 1, 0)); //7

            lstSettingCommand.Add(new clsSettingForCommand("HEX", 1, 0)); //8
            lstSettingCommand.Add(new clsSettingForCommand("HEXTYPE", 1, 0)); //9
            lstSettingCommand.Add(new clsSettingForCommand("ASCII", 1, 0)); //8

            lstSettingCommand.Add(new clsSettingForCommand("GETBIT", 2, 0)); //17
        }

        public object ExecuteSpecialFunction(clsCommonCommandGuider clsInput)
        {
            object objResult = new object();

            switch (clsInput.clsSettingCommand.strDetectCode)
            {
                case "MOD":
                    return cmdMOD(clsInput);

                case "LOG":
                    return cmdLOG(clsInput);

                case "HEX":
                    return cmdHEX(clsInput);
                case "HEXTYPE":
                    return cmdHEXTYPE(clsInput);
                case "ASCII":
                    return cmdASCII(clsInput);

                case "GETBIT":
                    return cmdGETBIT(clsInput);

                default:
                    return "PluginSpecialControl,Math - Error: cannot recognize special command [" + clsInput.clsSettingCommand.strDetectCode + "]";
            }
        }

        //*****************************************************************************************************************************
        public object cmdMOD(clsCommonCommandGuider clsInput)
        {
            //  MOD(X,Y) : 
            //      X (numeric): 1st  number
            //      Y (numeric): 2nd  number
            //  Return: X % Y

            int intRet = 0;
            int intFirstNum = 0;
            int intSecondNum = 0;
            string strTemp = "";

            if (clsInput.args.Parameters.Length != 2) return "MOD() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameters is invalid! MOD() require exactly 2 number.";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (int.TryParse(strTemp, out intFirstNum) == false) return "MOD() Error: First parameter [" + strTemp + "] input is not integer!";

            strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
            if (int.TryParse(strTemp, out intSecondNum) == false) return "MOD() Error: Second parameter [" + strTemp + "] input is not integer!";
            if (intSecondNum == 0) return "MOD() Error: the second number is 0! Cannot divide to 0!"; ;

            intRet = intFirstNum % intSecondNum;
            //
            return intRet;
        }

        //*****************************************************************************************************************************
        public object cmdLOG(clsCommonCommandGuider clsInput)
        {
            //  LOG(X,Y) / LOG(X)
            //      X (numeric): 1st  number
            //      Y (numeric): 2nd  number
            //  Return: LogyX    LOG(81,3) = 4
            //  Return: Log10X   LOG(100,10) = 2


            double dblRet = 0;
            double dblFirstNum = 0;
            double dblSecondNum = 0;
            string strTemp = "";

            if (clsInput.args.Parameters.Length < 1) return "LOG() Error: not enough parameter to do LOG() function!";
            if (clsInput.args.Parameters.Length > 2) return "LOG() Error: input " + clsInput.args.Parameters.Length.ToString() + " parameter is invalid!";

            strTemp = clsInput.args.Parameters[0].Evaluate().ToString();
            if (double.TryParse(strTemp, out dblFirstNum) == false) return "LOG() Error: First parameter input[" + strTemp + "] is not numeric!";

            if (dblFirstNum == 0) return "LOG() Error: logarit does not accept 0 value input!";

            if (clsInput.args.Parameters.Length == 1) //Do logarit with default base is 10
            {
                dblRet = Math.Log10(dblFirstNum);
            }
            else //custom logarit base
            {
                strTemp = clsInput.args.Parameters[1].Evaluate().ToString();
                if (double.TryParse(strTemp, out dblSecondNum) == false) return "LOG() Error: Second parameter input[" + strTemp + "] is not numeric!";
                if (dblSecondNum == 0) return "LOG() Error: logarit base cannot be 0!";
                if (dblSecondNum == 1) return "LOG() Error: logarit base cannot be 1!";

                dblRet = Math.Log(dblFirstNum, dblSecondNum);
            }

            return dblRet;
        }
        
        //*****************************************************************************************************************************
        public object cmdHEX(clsCommonCommandGuider clsInput)
        {
            //  HEX(X) : Convert decimal X to Hexa format
            //      X (Decimal): Number want to convert

            if (clsInput.args.Parameters.Length != 1) return "HEX() Error: only accept 1 number input!";

            string strNumber = clsInput.args.Parameters[0].Evaluate().ToString();

            double dblNumber = 0;
            int intNumber = 0; //Number to get bit

            //1. Check if parameter is numeric or not
            if (double.TryParse(strNumber, out dblNumber) == false)
            {
                //MessageBox.Show("HEX() error: First parameter input [" + strNumber + "] is not numeric!");
                return "HEX() error: First parameter input [" + strNumber + "] is not numeric!";
            }

            //Convert to integer
            intNumber = Convert.ToInt32(dblNumber);

            //Convert to hexa format
            string strRet = "";
            strRet = intNumber.ToString("X");

            return strRet;
        }

        //****************************************************************************************************************
        public object cmdHEXTYPE(clsCommonCommandGuider clsInput)
        {
            //  HEXTYPE(X) : Indicate X is hexa format, not decimal format
            //      X (Decimal): Number want to convert
      
            if (clsInput.args.Parameters.Length != 1) return "HEXTYPE() Error: only accept 1 number input!";

            string strNumber = clsInput.args.Parameters[0].Evaluate().ToString();


            int intNumber = 0; //
            //Check if parameter is hexa number or not
            if (Int32.TryParse(strNumber, System.Globalization.NumberStyles.HexNumber, null, out intNumber) == false) return "Error HEXTYPE(): number input[" + strNumber + "] is not Hexa number!";

            //If OK, then return new value
            return intNumber;
        }
        //*****************************************************************************************************************************
        public object cmdASCII(clsCommonCommandGuider clsInput)
        {
            //  ASCII(X) : Convert string X to ASCII code
            //      X (string): string or character want to convert
            //      If input more than 1 character => return list of Ascii code (bytes)
            if (clsInput.args.Parameters.Length != 1) return "ASCII() Error: only accept 1 parameter input!";

            string strNumber = clsInput.args.Parameters[0].Evaluate().ToString();
            //Convert to Ascii code
            byte[] bytes = Encoding.ASCII.GetBytes(strNumber);

            if (bytes.Length == 1)
            {
                return bytes[0];
            }
            else
            {
                List<byte> lstbyteRet = new List<byte>(bytes);
                return lstbyteRet;
            }
        }
        //****************************************************************************************************************
        public object cmdGETBIT(clsCommonCommandGuider clsInput)
        {
            //  GetBit(X,Y) : Return bit position Y of byte X
            //      X (int/byte): byte to get bit
            //      Y (int/byte): position to get bit

            if (clsInput.args.Parameters.Length!=2) return "GETBIT() Error: number of input paramter is not 2!";

            string strNumber = clsInput.args.Parameters[0].Evaluate().ToString();
            string strPosition = clsInput.args.Parameters[1].Evaluate().ToString();

            int intNumber = 0; //Number to get bit
            int intPos = 0; //Position to get bit

            //1. Check if 2 parameter is numeric or not
            if (int.TryParse(strNumber, out intPos) == false)
            {
                //MessageBox.Show("GETBIT() error: First parameter input [" + strNumber + "] is not integer format!");
                return "GETBIT() error: First parameter input [" + strNumber + "] is not integer format!";
            }

            if (int.TryParse(strPosition, out intPos) == false)
            {
                //MessageBox.Show("GETBIT() error: Second parameter input [" + strPosition + "] is not integer format!");
                return "GETBIT() error: Second parameter input [" + strPosition + "] is not integer format!";
            }

            intNumber = int.Parse(strNumber);
            intPos = int.Parse(strPosition);

            if (intPos > 31)
            {
                //MessageBox.Show("GETBIT() error: position[" + strPosition + "] want to get bit is over maximum 31!");
                return "GETBIT() error: position[" + strPosition + "] want to get bit is over maximum 31!";
            }

            //3. OK, now start to get bit algorithm
            if ((intNumber & (1 << intPos)) != 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        
        //*****************************************************************************************************************************

    } //End class
}
