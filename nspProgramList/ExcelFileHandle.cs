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
    /// This class. Reading Excel file & Return a DataTable for analyzing Program List
    /// </summary>
    public class clsExcelFileHandle
    {
        //****************************************************************************************************************
        public string LoadExcelFile(string strFullPathProgListFile, string strExcelSheetName, ref DataTable dtTable, int intMaxRow = 0)
        {
            string strRet = "0";
            dtTable = new DataTable(); //For later view on data gridview

            if (System.IO.File.Exists(strFullPathProgListFile) == false)
            {
                //MessageBox.Show("File: " + strFullPathProgListFile + " is not exist!", "LoadExcelFile()");
                //Environment.Exit(0);
                return "File: " + strFullPathProgListFile + " is not exist!";
            }
            //Load all data to data set
            //string strTemp = GlbLoadDtbase(strFullPathProgListFile, "[" + strExcelSheetName + "$]", dbAdapter, dbDataset, dbConnection, intMaxRow);
            strRet = DataTable_To_Excel(strFullPathProgListFile, strExcelSheetName, ref dtTable, intMaxRow);

            //If OK, then return OK code
            return strRet;
        }

        //****************************************************************************************************************
        /// <summary>
        /// Create new excel file
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="strSheetName"></param>
        public void CreateNewExcelWorkBook(string strFileName, string strSheetName)
        {
            HSSFWorkbook hssfWB = new HSSFWorkbook();
            hssfWB.CreateSheet(strSheetName);
            //Write the stream data of workbook to the root directory
            FileStream file = new FileStream(strFileName, FileMode.Create);
            hssfWB.Write(file);
            file.Close();
        }

        /// <summary>
        /// Load excel file and transfer data to data table
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <param name="strExcelSheetName"></param>
        /// <param name="dtTable"></param>
        /// <param name="intMaxRow"></param>
        /// <returns></returns>
        private string DataTable_To_Excel(string strFilePath, string strExcelSheetName, ref DataTable dtTable, int intMaxRow = 0)
        {
            try
            {
                dtTable = new DataTable();
                IWorkbook workbook = null;
                ISheet worksheet = null;
                int i, j = 0;

                using (FileStream stream = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
                {
                    string strExtension = System.IO.Path.GetExtension(strFilePath); //Get extension
                    switch (strExtension.ToLower())
                    {
                        case ".xls":
                            HSSFWorkbook workbookH = new HSSFWorkbook(stream);
                            NPOI.HPSF.DocumentSummaryInformation dsi = NPOI.HPSF.PropertySetFactory.CreateDocumentSummaryInformation();
                            dsi.Company = "Canon"; dsi.Manager = "PED";
                            workbookH.DocumentSummaryInformation = dsi;
                            workbook = workbookH;
                            break;

                        case ".xlsx":
                            workbook = new XSSFWorkbook(stream);
                            break;
                    }

                    worksheet = workbook.GetSheet(strExcelSheetName);

                    //Find all column has in worksheet include missing cell or blank cell
                    //Note that NPOI auto-remove missing cell! => we have to do manual
                    int NumCol = 0;
                    for (i = 0; i <= worksheet.LastRowNum; i++) //Looking through all row
                    {
                        int maxCol = 0;
                        int searchIndex = 0;
                        var row = worksheet.GetRow(i);
                        if (row != null)
                        {
                            searchIndex = 2 * row.Cells.Count; //Searching area is 2 times bigger than number of cell count in row
                            for (j = 0; j < searchIndex; j++)
                            {
                                try
                                {
                                    ICell cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                                    if (cell != null) maxCol = j + 1;
                                }
                                catch (Exception ex)
                                {
                                    var strTest = ex.Message;
                                }
                            }
                            //
                            if (NumCol < maxCol)
                            {
                                NumCol = maxCol;
                            }
                        }
                    }

                    //add neccessary columns
                    //if (dtTable.Columns.Count < worksheet.GetRow(0).Cells.Count)
                    if (dtTable.Columns.Count < NumCol)
                    {
                        //for (j = 0; j < worksheet.GetRow(0).Cells.Count; j++)
                        for (j = 0; j < NumCol; j++)
                        {
                            //dtTable.Columns.Add("", typeof(string));
                            dtTable.Columns.Add("", typeof(object));
                        }
                    }

                    //Loading all row to data table
                    for (i = 0; i <= worksheet.LastRowNum; i++)
                    {
                        //Check option to limit row loading
                        if ((intMaxRow > 0) && (i > intMaxRow - 1)) //No need load anymore
                        {
                            break;
                        }

                        //
                        if (worksheet.GetRow(i) != null)
                        {
                            object objTest = worksheet.GetRow(i);

                            // add row
                            dtTable.Rows.Add();

                            // write row value
                            //for (int j = 0; j < worksheet.GetRow(i).Cells.Count; j++)
                            for (j = 0; j < dtTable.Columns.Count; j++)
                            {
                                var cell = worksheet.GetRow(i).GetCell(j);

                                if (cell != null)
                                {
                                    //Treat all data input as string type
                                    if (cell.CellType == CellType.Formula) //Formular type
                                    {
                                        var test = cell.CachedFormulaResultType;
                                        var test2 = "";

                                        switch(test)
                                        {
                                            case CellType.String:
                                                test2 = cell.StringCellValue;
                                                break;
                                            case CellType.Boolean:
                                                test2 = cell.BooleanCellValue.ToString();
                                                break;
                                            case CellType.Numeric:
                                                test2 = cell.NumericCellValue.ToString();
                                                break;
                                            default:
                                                test2 = cell.ToString();
                                                break;
                                        }

                                        dtTable.Rows[i][j] = test2;
                                    }
                                    else //Not formular
                                    {
                                        dtTable.Rows[i][j] = cell.ToString(); //Treat all data input as string type
                                    }
                                }
                                else
                                {
                                    dtTable.Rows[i][j] = "";
                                }
                            }
                        }
                        else
                        {
                            // add row
                            dtTable.Rows.Add();

                            // write row value
                            for (j = 0; j < dtTable.Columns.Count; j++)
                            {
                                dtTable.Rows[i][j] = "";
                            }
                        }
                    }

                    stream.Close();
                }

                //Return 0 if everything is OK
                return "0";
            }
            catch (Exception ex)
            {
                return "Unexpected error happened. Error Message: " + ex.Message;
            }
        }

        //**********************************************************************************************************
        /// <summary>
        /// Load excel file using Microsoft access database
        /// </summary>
        /// <param name="PthDtbaseFile"></param>
        /// <param name="tblSource"></param>
        /// <param name="dbAdapt"></param>
        /// <param name="dbDtSet"></param>
        /// <param name="dbConnect"></param>
        /// <param name="intMaxRow"></param>
        /// <returns></returns>
        private string GlbLoadDtbase(string PthDtbaseFile, string tblSource, System.Data.OleDb.OleDbDataAdapter dbAdapt, System.Data.DataSet dbDtSet, System.Data.OleDb.OleDbConnection dbConnect, int intMaxRow = 0)
        {
            string dbProvider = "";   //create Provider for access database Miccrosoft Jet 4.0
            //string dbPassword = ""; //The Password protect database file, only author of this program know!
            string dbSource = "";    //The Full database source file name
            string dbSql = ""; //SQL command string
            string strExcelTableDataName = "ExcelTable"; //The name assign to excel table

            try
            {
                string strFileExtension = "";
                strFileExtension = System.IO.Path.GetExtension(PthDtbaseFile);
                strFileExtension = strFileExtension.ToLower();

                if (strFileExtension == ".xlsx")
                {
                    dbProvider = "PROVIDER=Microsoft.ACE.OLEDB.12.0;"; //For working with Ms office 2007 & later version
                    dbSource = "Data Source = " + PthDtbaseFile + ";";
                    //dbPassword = "Jet OLEDB:Database Password=mypassword;";

                    dbConnect.ConnectionString = dbProvider + dbSource + "Extended Properties=\"Excel 12.0;HDR=NO;\"";
                }
                else if (strFileExtension == ".xls")
                {
                    //dbProvider = "PROVIDER=Microsoft.Jet.OLEDB.4.0;"; //For working with Ms office 2007 erlier version
                    //dbSource = "Data Source = " + PthDtbaseFile + ";";
                    ////dbPassword = "Jet OLEDB:Database Password=mypassword;";

                    //dbConnect.ConnectionString = dbProvider + dbSource + "Extended Properties=\"Excel 8.0;HDR=NO;\"";


                    dbProvider = "PROVIDER=Microsoft.ACE.OLEDB.12.0;"; //For working with Ms office 2007 & later version
                    dbSource = "Data Source = " + PthDtbaseFile + ";";
                    //dbPassword = "Jet OLEDB:Database Password=mypassword;";

                    dbConnect.ConnectionString = dbProvider + dbSource + "Extended Properties=\"Excel 12.0;HDR=NO;\"";
                }
                else
                {
                    MessageBox.Show("Cannot recognize excel file with extension: " + strFileExtension + " !", "GlbLoadDtbase");
                }

                dbConnect.Open();

                dbSql = "SELECT * FROM " + tblSource;
                dbAdapt = new System.Data.OleDb.OleDbDataAdapter(dbSql, dbConnect);

                if (intMaxRow == 0)
                {
                    dbAdapt.Fill(dbDtSet, strExcelTableDataName);
                }
                else
                {
                    dbAdapt.Fill(dbDtSet, 0, intMaxRow, strExcelTableDataName);
                }

                //return "0"; //OK code
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Cannot loading program list [" + PthDtbaseFile + "].\r\nError message: \r\n" + ex.Message + "\r\nAre you opening program list in another program? \r\nPlease check and restart program!", "GlbLoadDtbase()");
                Environment.Exit(0);
                return "ex.Message";
            }
            finally
            {
                dbConnect.Close();
            }

            return "0"; //OK code
        }

        //****************************
        //Constructor
        public clsExcelFileHandle()
        {
            //
        }
    }
}
