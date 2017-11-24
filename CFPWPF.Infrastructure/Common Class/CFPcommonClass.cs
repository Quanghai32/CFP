using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nspCFPInfrastructures
{
    public class FunctionCatalog
    {
        public int intPartID { get; set; } //ID of part to get function
        public string strPartDesc { get; set; } //Contain description of Parts in metadata

        //For normal Function ID register
        public int intJigID { get; set; } //ID of Jig ID collection where Hard ID collection which have Function ID located
        public int intHardID { get; set; } //ID of Hard ID collection where Function ID located
        public int intFuncID { get; set; } //ID of Function

        //for special control
        public string strSpecialCmdCode { get; set; } 
        
        //
        public string strFileFullName { get; set; }

        public FunctionCatalog()
        {
            this.strFileFullName = "";
        }
    }

    //Define for User utilities class
    public class clsUserUtility
    {
        //public System.Windows.Forms.ToolStripMenuItem UserUtlToolStrip { get; set; } //For User form or other utility
        public System.Windows.Controls.MenuItem UserMenuItem { get; set; } //for display on Menu
        public int intUserUltID { get; set; } //In one DLL file maybe contain more than 1 user utilities (Form...)
        public FunctionCatalog UserUltFuncCatalog { get; set; } //For record 
        public List<List<object>> lstlstobjInput { get; set; } //For sending command back to child process
    }

    public class clsStepDataReturn
    {
        //Each Part (MEF) has its own sets of data return. This list prepare for it 
        public List<List<object>> lstlstobjDataReturn { get; set; }
    }

    //Struct to represent all data in 1 row of program list
    //public struct structProgramListRowData //Include all data in 1 collum from excel file
    public class classStepDataInfor //Include all data in 1 collum from excel file
    {
        public int intStepSequenceID; //Indicate that step is inside main sequence or in user function area or no need caring
        //    intStepSequenceID = 0: Step is inside main sequence
        //    intStepSequenceID = -1: Step is "free" standalone in separate area - no need caring
        //    intStepSequenceID = 1: Step is in User Function area
        public string strUserFunctionName; //If step is belong to certain User Function, then marking here

        //Declaration for Item Test Info
        public string strGroupData;
        public string strOriginStepNumber; //String because it can be Number, "N/A"
        public string strGroupNumber; //In group Mode, this is group Number

        public int intRowNumber; //The row number of step in program list

        public int intStepPos; //Position of step in program list (count from 0)
        public int intStepNumber; //1st collumn of steplist, indicate number of checking step 
        public string strStepName; //2nd collumn of steplist, indicate name of checking step
        public int intStepClass; //3rd collumn of steplist, indicate what kind of test class: User Ini, checking process, Test end process...

        //Declaration for Item Test Limit Setting        
        public object objLoLimit; //4th collumn. Indicate Low limit of a checking step
        public object objHiLimit; //5th collumn. Indicate Hi Limit of a checking step
        public string strUnitName; //6th collumn. Indicate what unit of data return in checking step

        //Declaration for Item ID Info
        public int intJigId;//7rd collumn. Indicate what kind of checking Jig ID
        public int intHardwareId;//8rd collumn. Indicate what kind of checking Hardware ID
        public int intFunctionId; //9rd collumn. Indicate what kind of checking Func ID

        //Declaration for  Item Factory Command
        public string strTransmisstion;//10
        public string strTransmisstionEx; //using in running time
        public string strReceive;//11

        public List<object> lstobjParameter; //12th->31st Original parameter reading from Excel File: should not changed!
        public List<object> lstobjParameterEx; //List of parameter using in running time: can be changed!

        //Declaration for Jumping Control Command
        public string strSpecialControl; //32th collumn.

        //Declaration for Item Signal Info
        public string strSignalName;//33
        public string strMeasurePoint;//34
        public string strMeasurePad;//35

        //Declaration for Item Control Spec Comment
        public string strComment;//36

        //Declaration for Item Notes
        public string strNotes;//37

        //For list of step Number in Program List which implemented for this step
        public List<int> lstintSubProgramListStep;

        /// <summary>
        /// Marking step in program list which coresspond to 1 step in step list must be Pass
        /// </summary>
        public bool blSTLPassCondition { get; set; }
        
        //*******************For Record step checking result*************************
        //For each step result
        public bool blStepChecked { get; set; } //Step is checked or not checked when complete checking process?
        public int intExecuteTimes { get; set; } //Step is checked or not checked when complete checking process?
        public int intStartTickMark { get; set; } //What timing to start checking each step
        public bool blStepResult { get; set; } //Step is OK or NG - Need initialize for this collection
        public object objStepCheckingData { get; set; } //Contain the original return value of step function, not yet converted to double
        public double dblStepTactTime { get; set; } //Tact time of step - Need initialize for this collection
        public string strStepErrMsg { get; set; } //Error Message of each step
        //For each step data return
        public clsStepDataReturn clsStepDataRet { get; set; } //Include all return data from each step 

        //Constructor
        public classStepDataInfor()
        {
            this.lstintSubProgramListStep = new List<int>();
            this.clsStepDataRet = new clsStepDataReturn();
        }
    }

    public class clsItemResultInformation
    {
        //For Item total result
        public bool blIniCheckingResult { get; set; } //Indicate result of Ini process of each item
        public bool blItemCheckingResult { get; set; } //Indicate last checking total result of item - for 1 item
        public double dblItemTactTime { get; set; } //Indicate of total tact time of last checking - for 1 item
        public int intItemNumberCheck { get; set; } //Indicate how many times item checked - for 1 item
        public int intItemNumberPass { get; set; } //Indicate how many passed time - for 1 item
        public double dblItemPassRate { get; set; } //Indicate pass rate of all checking history - for 1 item
        public int intStepFailPos { get; set; } //For Marking Step Fail
        public DateTime dateTimeChecking { get; set; }  //For record Date & time checking


        ////For each step result
        //public List<bool> lstblStepChecked { get; set; } //Step is checked or not checked when complete checking process?
        //public List<int> lstintExecuteTimes { get; set; } //Step is checked or not checked when complete checking process?
        //public List<int> lstintStartTickMark { get; set; } //What timing to start checking each step
        //public List<bool> lstblStepResult { get; set; } //Step is OK or NG - Need initialize for this collection
        //public List<object> lstobjStepCheckingData { get; set; } //Contain the original return value of step function, not yet converted to double
        //public List<double> lstdblStepTactTime { get; set; } //Tact time of step - Need initialize for this collection
        //public List<string> lststrStepErrMsg { get; set; } //Error Message of each step
        ////For each step data return
        //public List<clsStepDataReturn> lstclsStepDataRet { get; set; } //Include all return data from each step 

        ////Constructor
        //public clsItemResultInformation()
        //{
        //    this.lstblStepChecked = new List<bool>();
        //    this.lstintExecuteTimes = new List<int>();
        //    this.lstintStartTickMark = new List<int>();
        //    this.lstblStepResult = new List<bool>();
        //    this.lstobjStepCheckingData = new List<object>();
        //    this.lstdblStepTactTime = new List<double>();
        //    this.lststrStepErrMsg = new List<string>();
        //    this.lstclsStepDataRet = new List<clsStepDataReturn>();
        //}

    }

    public class clsProcessSettingReading
    {
        //For Origin Steplist using
        //For decide using PE Origin Step List or not
        public bool blUsingOriginSteplist { get; set; } //For reference to origin step list making by PE1 or PE2 - Default is YES
        public string strOriginStepListFileName { get; set; } //File Name of origin step list from PE
        public string strOriginStepListSheetName { get; set; } //Sheet name of origin steplist using "kikaku"

        public string strPassLabel { get; set; } //Since we need "Pass" - Default. But sometime is "Pass2"...
        public string strFailLabel { get; set; } //Default is "Fail" but sometime is "Fail2"...


        //For Child Process Program List
        public string strProgramListFileName { get; set; }
        public string strProgramListSheetName { get; set; }

        //For getting from FRAME PROGRAM
        public int intNumChecker { get; set; }
        public int intNumRow { get; set; }
        public int intNumCol { get; set; }
        public int intAllignMode { get; set; }
        public int intRoundShapeMode { get; set; }
        public int intOrgPosition { get; set; }

        public clsProcessSettingReading()
        {
            this.strPassLabel = "Pass";
            this.strFailLabel = "Fail";
        }
    }


    /// <summary>
    /// This class, with the aim to unify Master Process Model & Child Process Model
    /// </summary>
    public class classCommonMethod
    {
        /// <summary>
        /// Step result Pass or Fail Judgement method
        /// + Case 1: objUnit = "ANY" or "STR" => always return true.
        /// + Case 2: objUnit = "BOOL" or "BOOLEAN" => return convert to bool of objResult and compare with low spec or high spec
        /// + Default: result will be converted to Decimal and compare with low spec & Hi Spec
        /// </summary>
        /// <param name="objResult"></param>
        /// <param name="objLowSpec"></param>
        /// <param name="objHiSpec"></param>
        /// <param name="objUnit"></param>
        /// <returns></returns>
        public bool JudgeStepResult(object objResult, object objLowSpec, object objHiSpec, object objUnit)
        {
            bool blRet = false;
            //
            string strUnit = objUnit.ToString().ToLower().Trim();

            if((strUnit=="any")||(strUnit == "str")) //anything accepted
            {
                return true;
            }
            else if ((strUnit == "bool") || (strUnit == "boolean")) //boolean type
            {
                //Verify result is boolean type or not
                bool bltemp = false;
                if (bool.TryParse(objResult.ToString(), out bltemp) == false) return false;
                
                //Verify low spec and hi spec is boolean type or not
                bool blLowSpec = false;
                bool blHiSpec = false;
                if (bool.TryParse(objLowSpec.ToString(), out blLowSpec) == false) return false;
                if (bool.TryParse(objHiSpec.ToString(), out blHiSpec) == false) return false;

                //Compare result with low spec and high spec
                if ((bltemp == blLowSpec) || (bltemp == blHiSpec)) return true;
            }
            else //Default case is Numeric type
            {
                //Verify result is numeric or not
                decimal dctemp = 0;
                if (decimal.TryParse(objResult.ToString(), System.Globalization.NumberStyles.Float, null, out dctemp) == false) return false;

                //Verify low spec and hi spec is numeric or not
                decimal dcLowSpec = 0;
                decimal dcHiSpec = 0;
                if (decimal.TryParse(objLowSpec.ToString(), System.Globalization.NumberStyles.Float, null, out dcLowSpec) == false) return false;
                if (decimal.TryParse(objHiSpec.ToString(), System.Globalization.NumberStyles.Float, null, out dcHiSpec) == false) return false;

                //Compare result with low spec and high spec
                if ((dctemp >= dcLowSpec) && (dctemp <= dcHiSpec)) return true;
            }

            //
            return blRet;
        }
        
        
        
        
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class classSequenceTestData
    {
        public List<bool> lstblForceChange = new List<bool>();
        public List<object> lstobjNewValue = new List<object>();

        //Constructor
        public classSequenceTestData()
        {
            this.lstblForceChange = new List<bool>();
            this.lstobjNewValue = new List<object>();
        }

    }

}
