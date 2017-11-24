///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                    PROGRAM LIST PROJECT
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  Ver1.00: Replace Microsoft access database by NPOI to Load excel file. Hoang CVN. 15/Dec/2016.
//           + No need install Microsoft access database to run program
//           + The maximum character can be read in 1 cell now not limited by 255!
//
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using nspCFPInfrastructures;

namespace nspProgramList
{
    /// <summary>
    /// Program List Module. Reponsible for Reading Excel file...and create Program List.
    /// Receive a DataTable & analize to create Program List
    /// </summary>
    public class classProgramList
    {
        //Define component order in Data Table
        int FirstRowOrder = 13; //Indicate the order of first row of checking step. Note that begin with "0" value

        int OriginStepOrder = 0; //The first collumn contains step number from origin step from PE

        int TestNumberOrder = 1; //Indicate the order of "Test Number" collumn of checking step. Note that begin with "0" value
        int TestNameOrder = 2; //Indicate the order of "Test Name" collumn of checking step. Note that begin with "0" value  
        int TestClassOrder = 3;//Indicate the order of "Test Class" collumn of checking step. Note that begin with "0" value 

        int LoLimitOrder = 4; //Indicate the order of "Lo Limit" collumn of checking step. Note that begin with "0" value
        int HiLimitOrder = 5; //Indicate the order of "Hi Limit" collumn of checking step. Note that begin with "0" value
        int UnitNameOrder = 6; //Indicate the order of "Unit" collumn of checking step. Note that begin with "0" value

        int JigIDOrder = 7;//Indicate the order of "JigID" collumn of checking step. Note that begin with "0" value
        int HardWareIDOrder = 8;//Indicate the order of "HardwareID" collumn of checking step. Note that begin with "0" value
        int FunctionIDOrder = 9; //Indicate the order of "Function Id" collumn of checking step. Note that begin with "0" value

        int TransmissionOrder = 10;//Indicate the order of "Transmission" collumn of checking step. Note that begin with "0" value
        int ReceiveOrder = 11;//Indicate the order of "Receive" collumn of checking step. Note that begin with "0" value

        int Para1Order = 12; //Indicate the order of "Para1" collumn of checking step. All other 19 Parameter is consecutive follow Para1. Note that begin with "0" value

        int SpecialControlOrder = 32; //Indicate the order of "SPECIAL CONTROL" collumn of checking step. Note that begin with "0" value

        int SignalNameOrder = 33; //Indicate the order of "Signal Name" collumn of checking step. Note that begin with "0" value
        int MeasurePointOrder = 34; //Indicate the order of "PortMeasurePoint" collumn of checking step. Note that begin with "0" value
        int MeasurePadOrder = 35; //Indicate the order of "PortMeasurePads" collumn of checking step. Note that begin with "0" value
        int CommentOrder = 36; //Indicate the order of "Control Spec" collumn of checking step. Note that begin with "0" value
        int NoteOrder = 37; //Indicate the order of "Note!" collumn of checking step. Note that begin with "0" value

        public DataTable MyDataTable { get; set; }
        private clsExcelFileHandle clsExcelProgramList { get; set; }
        public List<classStepDataInfor> lstProgramList { get; set; } //Class to represent all data need in 1 program list => A list of structRowData

        //Store some header information
        public string strProgramListname { get; set; }
        public string strProgramListVersion { get; set; }
        public string strProgramListDateCreated { get; set; }

        //For MiniCompiler
        public List<List<object>> lstlstobjStepPara = new List<List<object>>(); //Input to mini compiler for analyzer
        public List<string> lststrSpecialCmd = new List<string>(); //For analyze special command area
        public List<string> lststrTransAreaSpecialCmd = new List<string>(); //For analyze special command in Transmission area

        //Struct to represent all data in 1 row of program list
        public classStepDataInfor clsStepRowData { get; set; } //Include all data in 1 collum from excel file

        public List<classUserFunction> lstclsUserFunction { get; set; }

        public object SearchOneRow(int intRowIndex, int intFirstRowOrder, DataTable Table, int intSequenceID = 0, string strUserFunctionName = "")
        {
            //
            classStepDataInfor clsTemp = new classStepDataInfor();
            clsTemp.lstobjParameter = new List<object>();
            clsTemp.lstobjParameterEx = new List<object>();

            string strTemp = "";
            int intTmp = 0;

            #region _SearchStep

            clsTemp.intStepSequenceID = intSequenceID;
            clsTemp.strUserFunctionName = strUserFunctionName;

            //Step row number
            clsTemp.intRowNumber = intRowIndex + 1;

            //0. Cal Test Position
            clsTemp.intStepPos = intRowIndex - intFirstRowOrder; //Count from 0

            //1. Get value of Test Number
            strTemp = ReadCellFromTable(Table, intRowIndex, TestNumberOrder);

            //Check validity of value Test Number
            if (int.TryParse(strTemp, out intTmp) == true) //valid
            {
                clsTemp.intStepNumber = Convert.ToInt32(strTemp);
            }
            else //invalid => just exit. This one, maybe blank row or User Function Name Row
            {
                if ((intSequenceID==0)||(intSequenceID==1)) //Main Sequence or User Function Sequence
                {
                    //MessageBox.Show("The number name of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!", "SearchOneRow()");
                    //Environment.Exit(0);//End application

                    return "The number name of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!";
                }
                else //Another sequence ID => No caring
                {
                    clsTemp.intStepNumber = -clsTemp.intStepPos; //Marking not valid Row
                }
            }

            //1.2 Get value of origin step number
            //clsTemp.strOriginStepNumber = ReadCellFromTable(Table, intRowIndex, OriginStepOrder);
            clsTemp.strGroupData = ReadCellFromTable(Table, intRowIndex, OriginStepOrder);

            //Analyze group data to get Group Number & Origin Step Number (from step list)
            string[] strTest = clsTemp.strGroupData.Split(',');
            if(strTest.Length>1) //Group Mode - need 2 number input (Group Number - Origin step)
            {
                clsTemp.strGroupNumber = strTest[0].Trim();
                clsTemp.strOriginStepNumber = strTest[1].Trim();
            }
            else //Not Group Mode
            {
                clsTemp.strGroupNumber = ""; //Default value is 1. All step belong to 1 group, 1 item. 
                clsTemp.strOriginStepNumber = strTest[0].Trim();
            }

            //2. Get value of Test Name
            clsTemp.strStepName = ReadCellFromTable(Table, intRowIndex, TestNameOrder);

            //3. Get value of Test Class
            strTemp = ReadCellFromTable(Table, intRowIndex, TestClassOrder);
            //Check validity of value Test Number
            if (int.TryParse(strTemp, out intTmp) == true) //valid
            {
                clsTemp.intStepClass = Convert.ToInt32(strTemp);
            }
            else //invalid
            {
                if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                {
                    //MessageBox.Show("The Test Class of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!", "SearchOneRow()");
                    //Environment.Exit(0);//End application

                    return "The Test Class of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!";
                }
            }

            //4. Get value of Unit
            clsTemp.strUnitName = ReadCellFromTable(Table, intRowIndex, UnitNameOrder).Trim();


            //5. Get value of Low Limit value & High Limit value
            string strLowLimit = ReadCellFromTable(Table, intRowIndex, LoLimitOrder);
            string strHiLimit = ReadCellFromTable(Table, intRowIndex, HiLimitOrder);

            //Check validity of value Low Limit
            if ((clsTemp.strUnitName.ToUpper() == "ANY")||((clsTemp.strUnitName.ToUpper() == "STR"))) //Anything accepted
            {
                //No need check anything
                clsTemp.objLoLimit = strLowLimit;
                clsTemp.objHiLimit = strHiLimit;
            }
            else if ((clsTemp.strUnitName.ToUpper() == "BOOL") || ((clsTemp.strUnitName.ToUpper() == "BOOLEAN"))) //Boolean type
            {
                bool blLowLimit = false;
                bool blHiLimit = false;
                if(bool.TryParse(strLowLimit, out blLowLimit)==false)
                {
                    if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                        return "The Lo Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strLowLimit + "] cannot convert to boolean. Please check!";
                }

                if (bool.TryParse(strHiLimit, out blHiLimit) == false)
                {
                    if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                        return "The Hi Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strHiLimit + "] cannot convert to boolean. Please check!";
                }

                clsTemp.objLoLimit = blLowLimit;
                clsTemp.objHiLimit = blHiLimit;
            }
            else //Numeric type
            {
                decimal dcLowLimit = 0;
                decimal dcHiLimit = 0;


                if (clsTemp.strUnitName.ToUpper() == "H") //Hexa format
                {
                    int intLowLimit = 0;
                    int intHiLimit = 0;
                    if (int.TryParse(strLowLimit, System.Globalization.NumberStyles.HexNumber, null, out intLowLimit) == false)
                    {
                        if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                            return "The Lo Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strLowLimit + "] cannot convert to numeric. Please check!";
                    }

                    if (int.TryParse(strHiLimit, System.Globalization.NumberStyles.HexNumber, null, out intHiLimit) == false)
                    {
                        if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                            return "The Lo Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strHiLimit + "] cannot convert to numeric. Please check!";
                    }

                    //
                    dcLowLimit = intLowLimit;
                    dcHiLimit = intHiLimit;
                }
                else //Not hexa format
                {
                    if (decimal.TryParse(strLowLimit, System.Globalization.NumberStyles.Float, null, out dcLowLimit) == false)
                    {
                        if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                            return "The Lo Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strLowLimit + "] cannot convert to numeric. Please check!";
                    }

                    if (decimal.TryParse(strHiLimit, System.Globalization.NumberStyles.Float, null, out dcHiLimit) == false)
                    {
                        if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                            return "The Lo Limit of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strHiLimit + "] cannot convert to numeric. Please check!";
                    }
                }


                //Check if surely Low limit is smaller than High Limit
                if (dcLowLimit > dcHiLimit)
                {
                    if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                        return "Step" + Convert.ToString(clsTemp.intStepNumber) + ": Low Limit is greater than High Limit!";
                }

                clsTemp.objLoLimit = dcLowLimit;
                clsTemp.objHiLimit = dcHiLimit;
            }

            //7. Get value of JigID
            strTemp = ReadCellFromTable(Table, intRowIndex, JigIDOrder);
            //Check validity of value Test Number
            if (int.TryParse(strTemp, out intTmp) == true) //valid
            {
                clsTemp.intJigId = Convert.ToInt32(strTemp);
            }
            else //invalid
            {
                if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                {
                    //MessageBox.Show("The Jig ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!", "SearchOneRow()");
                    //Environment.Exit(0);//End application

                    return "The Jig ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!";
                }
            }

            //8. Get value of HardwareID
            strTemp = ReadCellFromTable(Table, intRowIndex, HardWareIDOrder);
            //Check validity of value HardwareID
            if (int.TryParse(strTemp, out intTmp) == true) //valid
            {
                clsTemp.intHardwareId = Convert.ToInt32(strTemp);
            }
            else //invalid
            {
                if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                {
                    //MessageBox.Show("The Hardware ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!", "SearchOneRow()");
                    //Environment.Exit(0);//End application

                    return "The Hardware ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!";
                }
            }

            //9. Get value of Function ID value
            strTemp = ReadCellFromTable(Table, intRowIndex, FunctionIDOrder);
            //Check validity of value Function ID
            if (int.TryParse(strTemp, out intTmp) == true) //valid
            {
                clsTemp.intFunctionId = Convert.ToInt32(strTemp);
            }
            else //invalid
            {
                if ((intSequenceID == 0) || (intSequenceID == 1)) //Main Sequence or User Function Sequence
                {
                    //MessageBox.Show("The Function ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!", "SearchOneRow()");
                    //Environment.Exit(0);//End application

                    return "The Function ID of step at row: " + Convert.ToString(intRowIndex + 1) + "[" + strTemp + "] cannot convert to integer. Please check!";
                }  
            }

            //10. Get Transmission value
            clsTemp.strTransmisstion = ReadCellFromTable(Table, intRowIndex, TransmissionOrder);
            clsTemp.strTransmisstionEx = clsTemp.strTransmisstion;

            //11. Get Receive value
            clsTemp.strReceive = ReadCellFromTable(Table, intRowIndex, ReceiveOrder);


            //12-31. Get Parameter 1->20 value
            int j = 0;
            for (j = 0; j < 20; j++)
            {
                clsTemp.lstobjParameter.Add(ReadCellFromTable(Table, intRowIndex, (Para1Order + j)));
            }

            //Transfer to executed list parameter
            for (j = 0; j < clsTemp.lstobjParameter.Count; j++)
            {
                string strParaTemp = clsTemp.lstobjParameter[j].ToString();
                clsTemp.lstobjParameterEx.Add(strParaTemp);
            }

            ////32. Get Jump Control Command
            clsTemp.strSpecialControl = ReadCellFromTable(Table, intRowIndex, SpecialControlOrder);

            //38. Get signal name value
            clsTemp.strSignalName = ReadCellFromTable(Table, intRowIndex, SignalNameOrder);
            //39. Get Measure Point value
            clsTemp.strMeasurePoint = ReadCellFromTable(Table, intRowIndex, MeasurePointOrder);
            //40. Get Measure Pad value
            clsTemp.strMeasurePad = ReadCellFromTable(Table, intRowIndex, MeasurePadOrder);
            //41. Get Comment value
            clsTemp.strComment = ReadCellFromTable(Table, intRowIndex, CommentOrder);
            //42. Get Note value
            clsTemp.strNotes = ReadCellFromTable(Table, intRowIndex, NoteOrder);

            #endregion


            //If everything is OK, then assign return data sucessful code "0"

            return clsTemp;
        }

        //NEW - FOR RELOADING PROGRAM LIST
        public string LoadingProgramList(DataTable Table, out List<classStepDataInfor> lstProgList)
        {
            string strRet = "0";
            //
            //List<clsProgramListRowData> lstProgList = new List<clsProgramListRowData>();
            lstProgList = new List<classStepDataInfor>();

            //Start Reading Excel File
            //Reading some special info
            this.strProgramListname = ReadCellFromTable(Table, 10, 2);
            this.strProgramListVersion = ReadCellFromTable(Table, 10, 3);
            this.strProgramListDateCreated = ReadCellFromTable(Table, 10, 4);

            int i = 0;
            int j = 0;
            //int intStartSearchFunc = 0;
            string strTemp = "";
            int intSequenceID = 0;
            bool blFoundNewUserFunc = false;
            string strUserFunctionName = "";
            this.lstclsUserFunction = new List<classUserFunction>();
            classUserFunction clsNewUserFunc = new classUserFunction();


            //Searching main sequence area in step list
            for (i = FirstRowOrder; i < Table.Rows.Count; i++)
            {
                classStepDataInfor clsTemp = new classStepDataInfor();

                //Check Step Sequence ID
                //Get value of Test Number
                strTemp = ReadCellFromTable(Table, i, TestNumberOrder);
                strTemp = strTemp.Trim();

                if (strTemp == "") //End of Main sequence
                {
                    break;
                }

                //clsTemp = this.SearchOneRow(i, FirstRowOrder, Table, intSequenceID, strUserFunctionName);

                object objTemp = this.SearchOneRow(i, FirstRowOrder, Table, intSequenceID, strUserFunctionName);
                if (!(objTemp is classStepDataInfor)) //Loading fail
                {
                    return objTemp.ToString();
                }

                clsTemp = (classStepDataInfor)objTemp;

                //If everything OK, then we add row data to collection
                lstProgList.Add(clsTemp);

                //Add for minicompiler
                lstlstobjStepPara.Add(clsTemp.lstobjParameter);
                lststrSpecialCmd.Add(clsTemp.strSpecialControl);
                lststrTransAreaSpecialCmd.Add(clsTemp.strTransmisstion);
            }


            //Searching program list function area in step list: start with "FUNC" and ending with "END"
            int intStartSearch = i;
            for (i = intStartSearch; i < Table.Rows.Count; i++)
            {
                //Get value of Test Number
                strTemp = ReadCellFromTable(Table, i, TestNumberOrder);
                strTemp = strTemp.Trim();

                if(strTemp.ToUpper()=="FUNC") //Found new function
                {
                    blFoundNewUserFunc = true;
                    strUserFunctionName = ReadCellFromTable(Table, i, (TestNumberOrder + 1)).Trim();
                    clsNewUserFunc = new classUserFunction();
                    clsNewUserFunc.strUserFunctionName = strUserFunctionName;
                    intSequenceID = -1; //No caring this row
                }
                else if (strTemp.ToUpper() == "END") //Marking end of user function
                {
                    blFoundNewUserFunc = false; //Ending User function. Reset for next searching
                    strUserFunctionName = "";
                    this.lstclsUserFunction.Add(clsNewUserFunc);
                    intSequenceID = -1; //No caring this row
                }
                else
                {
                    if (blFoundNewUserFunc == true)
                    {
                        object objTemp = this.SearchOneRow(i, FirstRowOrder, Table, intSequenceID, strUserFunctionName);
                        if (!(objTemp is classStepDataInfor)) //Loading fail
                        {
                            return objTemp.ToString();
                        }
                        classStepDataInfor clsTemp = (classStepDataInfor)objTemp;
                        clsTemp.intStepSequenceID = 1; //program list user function area

                        //Add row data to new user function class
                        clsNewUserFunc.lstclsStepRowData.Add(clsTemp);
                    }
                }
            }

            //Now append Program list function to the end of main sequence step area
            int intFuncStepPos = intStartSearch - FirstRowOrder;
            for (i = 0; i < this.lstclsUserFunction.Count;i++ )
            {
                //Add 2 empty row to separate view
                for(j=0;j<2;j++)
                {
                    classStepDataInfor clsTemp = new classStepDataInfor();
                    clsTemp.intStepSequenceID = -1;
                    clsTemp.intStepPos = intFuncStepPos;
                    lstProgList.Add(clsTemp);

                    lstlstobjStepPara.Add(clsTemp.lstobjParameter);
                    lststrSpecialCmd.Add(clsTemp.strSpecialControl);
                    lststrTransAreaSpecialCmd.Add(clsTemp.strTransmisstion);
                    //
                    intFuncStepPos++;
                }

                //Add real function step row
                for(j=0;j<this.lstclsUserFunction[i].lstclsStepRowData.Count;j++)
                {
                    //Adjust step position
                    this.lstclsUserFunction[i].lstclsStepRowData[j].intStepPos = intFuncStepPos;
                    //Add to list of program list
                    lstProgList.Add(this.lstclsUserFunction[i].lstclsStepRowData[j]);
                    //Add to mini compiler
                    //Add for minicompiler
                    lstlstobjStepPara.Add(this.lstclsUserFunction[i].lstclsStepRowData[j].lstobjParameter);
                    lststrSpecialCmd.Add(this.lstclsUserFunction[i].lstclsStepRowData[j].strSpecialControl);
                    lststrTransAreaSpecialCmd.Add(this.lstclsUserFunction[i].lstclsStepRowData[j].strTransmisstion);

                    //
                    intFuncStepPos++;
                }
            }


            //Checking duplicate step in Main sequence ID and user function ID
            for (i = 0; i < lstProgList.Count; i++)
            {
                if (lstProgList[i].intStepSequenceID == -1) continue; //No caring

                int intStepNumber = lstProgList[i].intStepNumber;
                for (j = (i + 1); j < lstProgList.Count; j++)
                {
                    if (lstProgList[j].intStepSequenceID == -1) continue; //No caring

                    if (lstProgList[j].intStepNumber == intStepNumber) //Duplicate step number
                    {
                        return "Program List Error: Duplicate step number [" + intStepNumber.ToString() +
                            "] at row [" + lstProgList[i].intRowNumber.ToString() + "] & [" + lstProgList[j].intRowNumber.ToString() + "] in program list file! Please check & modify program list!";
                    }
                }
            }

            //Confirm if User Function Name is duplicate or not
            for (i = 0; i < this.lstclsUserFunction.Count; i++)
            {
                string strTempName = this.lstclsUserFunction[i].strUserFunctionName;
                for (j = (i + 1); j < this.lstclsUserFunction.Count; j++)
                {
                    if (strTempName.ToUpper().Trim() == this.lstclsUserFunction[j].strUserFunctionName.ToUpper().Trim())
                    {
                        return "Program List Error: Duplicate User Function Name [" + strTempName + "] in program list file! Please check & modify program list!";
                    }
                }
            }

            //
            return strRet;
        }

        //****************************************************************************************************************
        public string ReadCellFromTable(DataTable Table, int intRow, int intCol)
        {
            string strRet = "";
            try
            {
                strRet = Table.Rows[intRow].ItemArray[intCol].ToString();
            }
            catch (Exception ex)
            {
                strRet = "error";
                MessageBox.Show(ex.Message, "ReadCellFromExcelTable()");
            }
            return strRet;
        }


        //****************************************************************************************************************
        public void ReAssignInfo()
        {
            int i = 0;
            //Need to re-assign transmission area
            this.lststrTransAreaSpecialCmd = new List<string>();

            for(i=0;i<this.lstProgramList.Count;i++)
            {
                this.lststrTransAreaSpecialCmd.Add(this.lstProgramList[i].strTransmisstion);
            }
        }

        //****************************************************************************************************************
        public string GetProtectCode()
        {
            string strRet = "";
            //
            if (this.MyDataTable == null) return strRet;
            if (this.MyDataTable.Rows.Count == 0) return strRet;


            //string strTestHashCode = this.MyDataTable.GetHashCode().ToString();


            int i = 0;
            int j = 0;
            int k = 0;
            int intNumRow = this.MyDataTable.Rows.Count;
            int intNumCol = this.MyDataTable.Columns.Count;

            decimal dcTotalCheck = 0;
            decimal dcRowData = 0;
            decimal dcColData = 0;


            for (i = 0; i < intNumRow;i++)
            {
                dcRowData = 0;
                //
                for(j=0;j<intNumCol;j++)
                {
                    dcColData = 0;
                    string strTemp = this.MyDataTable.Rows[i][j].ToString();
                    //
                    for(k=0;k<strTemp.Length;k++)
                    {
                        dcColData += Convert.ToChar(strTemp[k]) * (decimal)Math.Sqrt(k+2);
                    }
                    //
                    dcRowData += dcColData * (decimal)Math.Sqrt(j+2);
                }

                dcTotalCheck += dcRowData * (decimal)Math.Sqrt(i + 2);// *(decimal)Math.Exp(dblTemp);
            }

            //Reduce value
            string strResult = dcTotalCheck.ToString();

            if (strResult.Length > 8)
            {
                strRet = strResult.Substring(strResult.Length - 8, 8);
            }
            else
            {
                strRet = strResult;
            }

            //
            return strRet;
        }
        //****************************************************************************************************************
        //Constructor
        public classProgramList(string FilePath, string SheetName, ref string strReturnCode)
        {
            strReturnCode = "";
            List<classStepDataInfor> lstProgList = new List<classStepDataInfor>();
            //Making new excel file class
            //Currently using Excel file to making Program List. But maybe in the future, other files can be supported also...
            clsExcelProgramList = new clsExcelFileHandle();
            //MyDataTable = clsExcelProgramList.LoadExcelFile(FilePath, SheetName);

            DataTable Table = new DataTable();
            strReturnCode = clsExcelProgramList.LoadExcelFile(FilePath, SheetName, ref Table);

            if (strReturnCode == "0")
            {
                this.MyDataTable = Table;
                //this.lstProgramList = this.GetProgramList;
                int intTest = this.MyDataTable.Rows.Count;


                strReturnCode = this.LoadingProgramList(this.MyDataTable, out lstProgList);
                if (strReturnCode == "0") //OK
                {
                    this.lstProgramList = lstProgList;
                }
            }
        }

        public classProgramList()
        {
            this.lstProgramList = new List<classStepDataInfor>();
        }
    }

    /// <summary>
    /// For Function declared directly in program list
    /// </summary>
    public class classUserFunction
    {
        public string strUserFunctionName { get; set; }
        public List<classStepDataInfor> lstclsStepRowData { get; set; }
        public List<object> lstobjParameter { get; set; }
        public object objReturnData { get; set; }

        //Constructor
        public classUserFunction()
        {
            this.strUserFunctionName = "";
            this.lstclsStepRowData = new List<classStepDataInfor>();
            this.lstclsStepRowData = new List<classStepDataInfor>();
            this.objReturnData = new object();
        }
    }
}
