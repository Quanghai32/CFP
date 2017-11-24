using nspProgramList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nspVEChecker
{
    /// <summary>
    /// Program List Module. Reponsible for Reading Excel file...and create Program List.
    /// Receive a DataTable & analize to create Program List
    /// </summary>
    public class clsProgramList
    {
        public DataTable MyDataTable { get; set; }
        private clsExcelFileHandle clsExcelProgramList { get; set; }
        public List<clsDestinationRowData> lstclsDestination { get; set; } //Class to represent all data need in 1 program list => A list of structRowData
        public string strShimukeVer { get; set; }

        //Struct to represent all data in 1 row of program list
        //public struct structProgramListRowData //Include all data in 1 collum from excel file
        public class clsDestinationRowData //Include all data in 1 collum from excel file
        {
            public string strIndex { get; set; }
            public string strOrder { get; set; }
            public string strDomesticOrAbroad { get; set; }
            public string strModel { get; set; }
            public string strA4LTR { get; set; }
            public string strShimukeData { get; set; }
            public string strLanguage { get; set; }
            public string strLanguageData { get; set; }
            //public string strVer { get; set; }

        }

        //Now analyze DataTable to create Program List
        private List<clsDestinationRowData> AnalyzeDataTable(DataTable Table)
        {
            List<clsDestinationRowData> lstTemp = new List<clsDestinationRowData>();

            //
            int FirstRowOrder = 1; //Note that begin with "0" value
            //
            int intInDexCol = 0;
            int intOrderCol = 1;
            int intDomesticOrAbroadCol = 2;
            int intModelCol = 3;
            int intA4LTRCol = 4;
            int intShimukeDataCol = 5;
            int intLanguageCol = 6;
            int intLanguageDataCol = 7;
            int intVerCol = 9;

            //Reading Shimuke Version
            this.strShimukeVer = ReadCellFromTable(Table, FirstRowOrder, intVerCol);

            int i = 0;
            for (i = FirstRowOrder; i < Table.Rows.Count; i++)
            {
                clsDestinationRowData clsTemp = new clsDestinationRowData();
                string strTemp = "";

                //1. strIndex
                strTemp = ReadCellFromTable(Table, i, intInDexCol);
                //Check if reach final Index or not
                if (strTemp == "") //No more illegal step. Stop Searching
                {
                    break;
                }

                //Reading all data
                clsTemp.strIndex = ReadCellFromTable(Table, i, intInDexCol);
                clsTemp.strOrder = ReadCellFromTable(Table, i, intOrderCol);
                clsTemp.strDomesticOrAbroad = ReadCellFromTable(Table, i, intDomesticOrAbroadCol);
                clsTemp.strModel = ReadCellFromTable(Table, i, intModelCol);
                clsTemp.strA4LTR = ReadCellFromTable(Table, i, intA4LTRCol);
                clsTemp.strShimukeData = ReadCellFromTable(Table, i, intShimukeDataCol);
                clsTemp.strLanguage = ReadCellFromTable(Table, i, intLanguageCol);
                clsTemp.strLanguageData = ReadCellFromTable(Table, i, intLanguageDataCol);

                //
                lstTemp.Add(clsTemp);
            }

            //
            return lstTemp;
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

        //Constructor
        public clsProgramList(string FilePath, string SheetName)
        {
            //Making new excel file class
            //Currently using Excel file to making Program List. But maybe in the future, other files can be supported also...
            clsExcelProgramList = new clsExcelFileHandle();
            DataTable Table = new DataTable();
            var result = clsExcelProgramList.LoadExcelFile(FilePath, SheetName, ref Table);
            if (result == "0") //OK
            {
                this.MyDataTable = Table;
                this.lstclsDestination = this.AnalyzeDataTable(MyDataTable);
            }
            else
            {
                MessageBox.Show("VEDestination: Cannot load excel file. Return data: " + result);
            }
        }
    }

}
