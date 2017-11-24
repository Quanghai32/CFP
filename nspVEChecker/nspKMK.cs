using nspProgramList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nspKMK
{
    /// <summary>
    /// Program List Module. Reponsible for Reading Excel file...and create Program List.
    /// Receive a DataTable & analize to create Program List
    /// </summary>
    public class clsProgramList
    {
        public DataTable MyData { get; set; }
        private clsExcelFileHandle clsExcelProgramList { get; set; }
        public List<ExcelFile> lstExcelFile { get; set; } //Class to represent all data need in 1 program list => A list of structRowData

        //Struct to represent all data in 1 row of program list
        //public struct structProgramListRowData //Include all data in 1 collum from excel file
        public class ExcelFile //Include all data in 1 collum from excel file
        {
            //public int index;
            public string[] Col = new string [50];

        }
        //List<ExcelFile> listCol = new List<ExcelFile>
        //{
        //    new ExcelFile {Col ={1,2,3}} 
        //};
        //Now analyze DataTable to create Program List
        private List<ExcelFile> AnalyzeData(DataTable Table, int maxCol)
        {
            List<ExcelFile> lstTemp = new List<ExcelFile>();

            //
            //int FirstRowOrder = 1; //Note that begin with "0" value
            //
            int j;
            int[] intCol= new int[50];
            for (j = 0; j < maxCol; j++)
            {
                intCol[j] = j;
            }


            int i = 0;
            for (i = 0; i < Table.Rows.Count; i++)
            {
                ExcelFile clsTemp = new ExcelFile();
                string strTemp = "";
                //1. strIndex
                strTemp = ReadCellFromTable(Table, i, intCol[0]);
                //Check if reach final Index or not
                if (strTemp == "") //No more illegal step. Stop Searching
                {
                    break;
                }
                //Read All Data
                for (j = 0; j < maxCol; j++)
                {
                    clsTemp.Col[j] = ReadCellFromTable(Table, i, intCol[j]);
                }

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
        public clsProgramList(string FilePath, string SheetName, int maxCol)
        {
            //Making new excel file class
            //Currently using Excel file to making Program List. But maybe in the future, other files can be supported also...
            clsExcelProgramList = new clsExcelFileHandle();
            //MyData = clsExcelProgramList.LoadExcelFile(FilePath, SheetName);
            DataTable Table = new DataTable();
            var result = clsExcelProgramList.LoadExcelFile(FilePath, SheetName, ref Table);
            if (result == "0") //OK
            {
                this.MyData = Table;
                //this.lstclsDestination = this.AnalyzeDataTable(MyDataTable);
                this.lstExcelFile = this.AnalyzeData(MyData, maxCol);
            }
            else
            {
                MessageBox.Show("nspKMK: Cannot load excel file. Return data: " + result);
            }
        }
    }
}
