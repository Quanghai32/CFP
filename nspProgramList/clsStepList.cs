using nspCFPInfrastructures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nspProgramList
{
    /// <summary>
    /// This class, for loading Origin Step list from PE follow old format
    /// </summary>
    public class classStepList
    {
        //Define component order in Data Table
        int FirstRowOrder = 13; //Indicate the order of first row of checking step. Note that begin with "0" value

        int TestNumberOrder = 1; //Indicate the order of "Test Number" collumn of checking step. Note that begin with "0" value
        int TestNameOrder = 3; //Indicate the order of "Test Name" collumn of checking step. Note that begin with "0" value  
       
        int LoLimitOrder = 5; //Indicate the order of "Lo Limit" collumn of checking step. Note that begin with "0" value
        int HiLimitOrder = 7; //Indicate the order of "Hi Limit" collumn of checking step. Note that begin with "0" value
        int UnitNameOrder = 8; //Indicate the order of "Unit" collumn of checking step. Note that begin with "0" value

        int SignalNameOrder = 9; //Indicate the order of "Signal Name" collumn of checking step. Note that begin with "0" value
        int MeasurePointOrder = 10; //Indicate the order of "PortMeasurePoint" collumn of checking step. Note that begin with "0" value
        int MeasurePadOrder = 11; //Indicate the order of "PortMeasurePads" collumn of checking step. Note that begin with "0" value
       
        int TransmissionOrder = 12;//Indicate the order of "Transmission" collumn of checking step. Note that begin with "0" value
        int ReceiveOrder = 13;//Indicate the order of "Receive" collumn of checking step. Note that begin with "0" value

        int CommentOrder = 24; //Indicate the order of "Control Spec" collumn of checking step. Note that begin with "0" value
        int NoteOrder = 25; //Indicate the order of "Note!" collumn of checking step. Note that begin with "0" value

        public DataTable MyDataTable { get; set; }
        private clsExcelFileHandle clsExcelStepList { get; set; }
        public List<classStepDataInfor> lstExcelList { get; set; } //Class to represent all data need in 1 program list => A list of structRowData

        //Store some header information
        public string strStepListname { get; set; }
        public string strStepListVersion { get; set; }
        public string strStepListDateCreated { get; set; }

       
        //Struct to represent all data in 1 row of step list
        public classStepDataInfor clsStepRowData { get; set; } //Include all data in 1 collum from excel file

        public string LoadingStepList(DataTable Table, out List<classStepDataInfor> lstStepList)
        {
            string strRet = "0";
            lstStepList = new List<classStepDataInfor>();
            //Start Reading Excel File
            //Reading some special info
            this.strStepListname = ReadCellFromTable(Table, 8, 4);
            this.strStepListVersion = ReadCellFromTable(Table, 10, 7);
            this.strStepListDateCreated = ReadCellFromTable(Table, 9, 3);

            int i = 0;
            int j = 0;
            string strTemp = "";
            int intTmp = 0;
            double dblTmp = 0;

            for (i = FirstRowOrder; i < Table.Rows.Count; i++)
            {
                classStepDataInfor clsTemp = new classStepDataInfor();

                //Check Step Sequence ID
                //Get value of Test Number
                strTemp = ReadCellFromTable(Table, i, TestNumberOrder);
                strTemp = strTemp.Trim();

                if (strTemp == "") //Check if Step list ending checking sequence
                {
                    break;
                }

                //1. Check validity of value Test Number
                if (int.TryParse(strTemp, out intTmp) == true) //valid
                {
                    clsTemp.intStepNumber = Convert.ToInt32(strTemp);
                }
                else //invalid => just exit
                {
                    //MessageBox.Show("The number name of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to integer. Please check!", "Step List Error");
                    //Environment.Exit(0);//End application

                    return "The number name of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to integer. Please check!";
                }

                //2. Get value of Test Name
                clsTemp.strStepName = ReadCellFromTable(Table, i, TestNameOrder);

                //3. Get value of Unit
                clsTemp.strUnitName = ReadCellFromTable(Table, i, UnitNameOrder);

                //5. Get value of Low Limit value
                strTemp = ReadCellFromTable(Table, i, LoLimitOrder);
                //Check validity of value Low Limit
                if (clsTemp.strUnitName.ToUpper() == "H") //Hexa format
                {
                    if (int.TryParse(strTemp, NumberStyles.HexNumber, null, out intTmp) == true) //Valid
                    {
                        clsTemp.objLoLimit = intTmp;
                    }
                    else //Invalid
                    {
                        //MessageBox.Show("The Lo Limit of step at row: " + Convert.ToString(i + 1) + "[" + strTemp + "] cannot convert to hexa number. Please check!", "Step List Error");
                        //Environment.Exit(0);//End application 

                        return "The Lo Limit of step at row: " + Convert.ToString(i + 1) + "[" + strTemp + "] cannot convert to hexa number. Please check!";
                    }
                }
                else //Double format
                {
                    if (double.TryParse(strTemp, out dblTmp) == true) //valid
                    {
                        clsTemp.objLoLimit = Convert.ToDouble(strTemp);
                    }
                    else //invalid
                    {
                        //MessageBox.Show("The Lo Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to number. Please check!", "Step List Error");
                        //Environment.Exit(0);//End application

                        return "The Lo Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to number. Please check!";
                    }
                }

                //6. Get value of Hi Limit value
                strTemp = ReadCellFromTable(Table, i, HiLimitOrder);
                //Check validity of value Hi Limit
                if (clsTemp.strUnitName.ToUpper() == "H") //Hexa format
                {
                    if (int.TryParse(strTemp, NumberStyles.AllowHexSpecifier, null, out intTmp) == true) //Valid
                    {
                        clsTemp.objHiLimit = intTmp;
                    }
                    else //Invalid
                    {
                        //MessageBox.Show("The Hi Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to hexa number. Please check!", "Step List Error");
                        //Environment.Exit(0);//End application

                        return "The Hi Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to hexa number. Please check!";
                    }
                }
                else //Double format
                {
                    if (double.TryParse(strTemp, out dblTmp) == true) //valid
                    {
                        clsTemp.objHiLimit = Convert.ToDouble(strTemp);
                    }
                    else //invalid
                    {
                        //MessageBox.Show("The Hi Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to number. Please check!", "Step List Error");
                        //Environment.Exit(0);//End application 

                        return "The Hi Limit of step at row: " + Convert.ToString(i + 1) + " [" + strTemp + "] cannot convert to number. Please check!";
                    }
                }

                //7. Check if surely Low limit is smaller than High Limit
                if (Convert.ToDouble(clsTemp.objLoLimit) > Convert.ToDouble(clsTemp.objHiLimit))
                {
                    //MessageBox.Show("Step" + Convert.ToString(clsTemp.intStepNumber) + ": Low Limit is greater than High Limit!", "Step List Error");
                    //Environment.Exit(0);//End application

                    return "Step" + Convert.ToString(clsTemp.intStepNumber) + ": Low Limit is greater than High Limit!";
                }

                //10. Get Transmission value
                clsTemp.strTransmisstion = ReadCellFromTable(Table, i, TransmissionOrder);
                clsTemp.strTransmisstionEx = clsTemp.strTransmisstion;

                //11. Get Receive value
                clsTemp.strReceive = ReadCellFromTable(Table, i, ReceiveOrder);

                //38. Get signal name value
                clsTemp.strSignalName = ReadCellFromTable(Table, i, SignalNameOrder);
                //39. Get Measure Point value
                clsTemp.strMeasurePoint = ReadCellFromTable(Table, i, MeasurePointOrder);
                //40. Get Measure Pad value
                clsTemp.strMeasurePad = ReadCellFromTable(Table, i, MeasurePadOrder);
                //41. Get Comment value
                clsTemp.strComment = ReadCellFromTable(Table, i, CommentOrder);
                //42. Get Note value
                clsTemp.strNotes = ReadCellFromTable(Table, i, NoteOrder);

                //If everything OK, then we add row data to collection
                lstStepList.Add(clsTemp);
            }

            //Checking duplicate step in Main sequence ID and user function ID
            for (i = 0; i < lstStepList.Count; i++)
            {
                if (lstStepList[i].intStepSequenceID == -1) continue; //No caring

                int intStepNumber = lstStepList[i].intStepNumber;
                for (j = (i + 1); j < lstStepList.Count; j++)
                {
                    if (lstStepList[j].intStepSequenceID == -1) continue; //No caring

                    if (lstStepList[j].intStepNumber == intStepNumber) //Duplicate step number
                    {
                        MessageBox.Show("Step List Error: Duplicate step number [" + intStepNumber.ToString() +
                            "] at row [" + (i + 14).ToString() + "] & [" + (j + 14).ToString() + "] in Step list file! Please check & modify Step list!", "STEP LIST DUPLICATE STEP ERROR");
                        Environment.Exit(0);
                    }
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Return program list
            //return lstStepList;

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
        public string GetProtectCode()
        {
            string strRet = "";
            //
            if (this.MyDataTable == null) return strRet;
            if (this.MyDataTable.Rows.Count == 0) return strRet;

            int i = 0;
            int j = 0;
            int k = 0;
            int intNumRow = this.MyDataTable.Rows.Count;
            int intNumCol = this.MyDataTable.Columns.Count;

            decimal dcTotalCheck = 0;
            decimal dcRowData = 0;
            decimal dcColData = 0;


            for (i = 0; i < intNumRow; i++)
            {
                dcRowData = 0;
                //
                for (j = 0; j < intNumCol; j++)
                {
                    dcColData = 0;
                    string strTemp = this.MyDataTable.Rows[i][j].ToString();
                    //
                    for (k = 0; k < strTemp.Length; k++)
                    {
                        dcColData += Convert.ToChar(strTemp[k]) * (decimal)Math.Sqrt(k + 2);
                    }
                    //
                    dcRowData += dcColData * (decimal)Math.Sqrt(j + 2);
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
        public classStepList(string FilePath, string SheetName, ref string strReturnCode,int intMaxRow = 0)
        {
            strReturnCode = "";
            //Making new excel file class
            //Currently using Excel file to making Program List. But maybe in the future, other files can be supported also...
            clsExcelStepList = new clsExcelFileHandle();
            //MyDataTable = clsExcelStepList.LoadExcelFile(FilePath, SheetName);
            List<classStepDataInfor> lstStepList = new List<classStepDataInfor>();

            DataTable Table = new DataTable();
            strReturnCode = clsExcelStepList.LoadExcelFile(FilePath, SheetName, ref Table, intMaxRow);

            int intTest = Table.Rows.Count;


            if (strReturnCode == "0")
            {
                this.MyDataTable = Table;
                //this.lstProgramList = this.GetProgramList;
                strReturnCode = this.LoadingStepList(this.MyDataTable, out lstStepList);
                if(strReturnCode=="0") //OK
                {
                    this.lstExcelList = lstStepList;
                }
            }
        }

        public classStepList()
        {
            this.lstExcelList = new List<classStepDataInfor>();
        }
    }
}
