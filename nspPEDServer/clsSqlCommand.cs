/////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////Module Name : SQL SERVER MODULE
//////Athor : Ho Duc Hieu
//////Version : 1.00
//////Date : 13.Apr.16
//////Content Function : + 1. Get Connection
//////                   + 2. Insert Data to Server       
//////                   + 3. Confirm Data from Server
/////                    + 4. Update Data to Server
/////                    + 5. Delete Data From Server  
////                     + 6. View Data 
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nspINTERFACE;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;


namespace nspSqlCommand
{
    [Export(typeof(nspINTERFACE.IPluginExecute))]
    [ExportMetadata("IPluginInfo", "PluginSqlCommand,203")]
    public class clsSqlCommand : nspINTERFACE.IPluginExecute
    {
        #region _Interface_implement
        public void IGetPluginInfo(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjInfo)
        {
            lstlstobjInfo = new List<List<object>>();
            var lstobjInfo = new List<object>();
            string strTemp = "";

            //Inform to Host program which Function this plugin support
            strTemp = "203,0,0,1"; lstobjInfo.Add(strTemp); //Jig ID,HardwareID,FunctionID
            //Inform to Host program about Extension version, Date create, Note & Author Infor
            strTemp = "Author, Hieu CVN PED"; lstobjInfo.Add(strTemp);
            strTemp = "Version, 1.01"; lstobjInfo.Add(strTemp);
            strTemp = "Date, 6/May/2016"; lstobjInfo.Add(strTemp);
            strTemp = "Note, Add more Extension Information"; lstobjInfo.Add(strTemp);

            lstlstobjInfo.Add(lstobjInfo);
        }

        public object IFunctionExecute(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            if (lstlstobjInput.Count < 1) return "Not enough Info input";
            if (lstlstobjInput[0].Count < 11) return "error 1"; //Not satify minimum length "Application startup path - Process ID - ... - JigID-HardID-FuncID"

            int intJigID = 0;
            if (int.TryParse(lstlstobjInput[0][8].ToString(), out intJigID) == false) return "error 2"; //Not numeric error
            intJigID = int.Parse(lstlstobjInput[0][8].ToString());
            switch (intJigID) //Select JigID
            {
                case 203:
                    return SelectHardIDFromJigID203(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize JigID: " + intJigID.ToString();
            }
        }
        #endregion

        public object SelectHardIDFromJigID203(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intHardID = 0;
            if (int.TryParse(lstlstobjInput[0][9].ToString(), out intHardID) == false) return "error 1"; //Not numeric error
            intHardID = int.Parse(lstlstobjInput[0][9].ToString());
            switch (intHardID) //Select HardID
            {
                case 0:
                    return SelectFuncIDFromHardID0(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize HardID: " + intHardID.ToString();
            }
        }
        public object SelectFuncIDFromHardID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            //Check string input
            int intFuncID = 0;
            if (int.TryParse(lstlstobjInput[0][10].ToString(), out intFuncID) == false) return "error 1"; //Not numeric error
            intFuncID = int.Parse(lstlstobjInput[0][10].ToString());
            switch (intFuncID) //Select FuncID
            {
                case 0: //Get Connection String
                    return PluginPEDServerFuncID0(lstlstobjInput, out lstlstobjOutput);
                case 1: //Insert Data From Checker to Server
                    return PluginPEDServerFuncID1(lstlstobjInput, out lstlstobjOutput);
                default:
                    return "Unrecognize FuncID: " + intFuncID.ToString();
            }
        }

        /// <summary>
        /// Get Setting information to Connect
        ///   +PRM1 : Input Setting File 
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginPEDServerFuncID0(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();
            //List<string> Connection = new List<string>();

            char charSeparatorKey = Convert.ToChar(lstlstobjInput[0][14].ToString());  //Define Separator Key
            string strInput = lstlstobjInput[0][13].ToString(); //Get String Connection From Steplist

            string[] strTmp = strInput.Split(charSeparatorKey);

            List<string> lststrConnection = new List<string>(strTmp);
            if (lststrConnection.Count != 8)
            {
                return "User Input don't have enough parameter , Number input :" + lststrConnection.Count.ToString();
            }

            string strConnection = @"Server =" + lststrConnection[0] + ";" +
                                   "User ID=" + lststrConnection[1] + ";" +
                                   "PassWord =" + lststrConnection[2] + ";" +
                                   "DataBase = " + lststrConnection[3] + ";" +
                                   "Connection Reset =" + lststrConnection[4] + ";" +
                                   "Connection Lifetime =" + lststrConnection[5] + ";" +
                                   "Min Pool Size =" + lststrConnection[6] + ";" +
                                   "Max Pool Size =" + lststrConnection[7];

            lststrTemp.Add("Server"); //Add Key Word
            lststrTemp.Add(strConnection); //Add String Connection
            lstlstobjOutput.Add(lststrTemp);
            return "0";
        }


        /// <summary>
        /// Send Data to Server
        /// Prm13,Prm14,Prm15,Prm16,Prm17 : StrConnection - StrSeparatorKey -StrTable -StrColumnName -StrValueInsert
        /// </summary>
        /// <param name="lstlstobjInput"></param>
        /// <param name="lstlstobjOutput"></param>
        /// <returns></returns>
        public object PluginPEDServerFuncID1(List<List<object>> lstlstobjInput, out List<List<object>> lstlstobjOutput)
        {
            lstlstobjOutput = new List<List<object>>();
            var lststrTemp = new List<object>();

            List<string> lstColumn = new List<string>();
            List<string> lstValueInsert = new List<string>();

            string strConnection = lstlstobjInput[0][13].ToString();
            string strSeparatorKey = lstlstobjInput[0][14].ToString();
            string strTable = lstlstobjInput[0][15].ToString();
            string strColumnName = lstlstobjInput[0][16].ToString();
            string strValueInsert = lstlstobjInput[0][17].ToString();

            string strSectionTable = "";
            string strValueInsertTable = "";
            string strTableName = "";
            string strValueName = "";
            string[] strTmp;

            if (strConnection.Trim() == "")
            {
                return "Func ID 203-0-1 Error: Don't have parameter connection";
            }

            if (strSeparatorKey.Trim() == "")
            {
                return "Func ID 203-0-1 Error: Don't have parameter Separatorkey";
            }

            if (strTable.Trim() == "")
            {
                return "Func ID 203-0-1 Error: Don't have parameter Table";
            }

            if (strColumnName.Trim() == "")
            {
                return "Func ID 203-0-1 Error: Don't have parameter ColumnName";
            }

            if (strValueInsert.Trim() == "")
            {
                return "Func ID 203-0-1 Error: Don't have parameter ValueInsert";
            }

            char charSeparatorKey = Convert.ToChar(strSeparatorKey); // Separator Key
            strTmp = strColumnName.Split(charSeparatorKey);
            for (int i = 0; i < strTmp.Length; i++)
            {
                lstColumn.Add(strTmp[i]);
                strSectionTable = strSectionTable + lstColumn[i] + ",";
            }
            strTableName = strSectionTable.Remove(strSectionTable.Length - 1);
            strTmp = strValueInsert.Split(charSeparatorKey);

            for (int i = 0; i < strTmp.Length; i++)
            {
                lstValueInsert.Add(strTmp[i]);
                strValueInsertTable = strValueInsertTable + "N'" + lstValueInsert[i] + "',";
            }
            strValueName = strValueInsertTable.Remove(strValueInsertTable.Length - 1);
            //Load data for Column Value
            string StrInsert = @"INSERT INTO " + "" + strTable + "(" + strTableName + ")" + " " + "VALUES" + "(" + strValueName + ")";

            try
            {
                SqlConnection Connect = new SqlConnection(strConnection);
                Connect.Open();
                SqlCommand Cmd = new SqlCommand(StrInsert, Connect);
                Cmd.ExecuteNonQuery();
                Connect.Close();
                lststrTemp.Add("OK");
                lstlstobjOutput.Add(lststrTemp);
            }
            catch (Exception ex)
            {
                return "Func ID 203-0-1 Error: Can Not Connect To Server. Message error: " + ex.Message;
            }

            return "0";
        }
    }
}

