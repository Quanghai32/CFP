using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace nspSavingData
{
    public class clsSavingData
    {

        public string strPCName { get; set; }
        public string strIPAddress { get; set; }
        public string strMacAddress { get; set; }

        public string strDataSavePath { get; set; }
        public string strProgramVer { get; set; }
        public string strProgramDateCreated { get; set; }

        public List<string> lststrUserPreInfoHeader { get; set; }
        public List<string> lststrUserAfterInfoHeader { get; set; }

        public List<nspChildProcessModel.clsChildProcessModel> lstChildProcessModel { get; set; }

        public void SavingDataIni()
        {
            //1. Check if Data folder is exist or not. If not, then create new data folder for saving
            try
            {
                var clsTemp = new nspFileHandle.ChkExist();
                if (clsTemp.CheckFolderExist(this.strDataSavePath) == false)
                {
                    Directory.CreateDirectory(this.strDataSavePath);
                }
                //2. Each day create 1 csv file to store checking data if not exist
                //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
                string strDataFileName = "";
                DateTime _Now = DateTime.Now;
                strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";

                string strDataFileFullName = this.strDataSavePath + @"\" + strDataFileName;
                //MessageBox.Show(strDataFileName);
                if (clsTemp.CheckFileExist(strDataFileFullName) == false)
                {
                    File.Create(strDataFileFullName).Close();
                }
                //3. Create File Header
                CreateFileHeader(strDataFileFullName);

                //4. Find out some Information of PC
                this.strMacAddress = nspMyFunc.clsMyFunc.GetMACAddress();
                this.strIPAddress = nspMyFunc.clsMyFunc.GetIPv4Address();
                this.strPCName = nspMyFunc.clsMyFunc.GetPcName();
            }
            catch //(Exception ex)
            {
                MessageBox.Show("Can not create folder [" + this.strDataSavePath + "] to save data! The Drive is not exist? Please check!", "SavingDataIni()");
                Environment.Exit(0);
            }
        }

        public void CreateFileHeader(string strFileFullName)
        {
            int i = 0;
            //1. Write Time to creating or start writing to csv File
            string strTimeCreated = "";
            DateTime _Now = DateTime.Now;
            strTimeCreated = "Started Time:" + "," + _Now.ToString();
            AppendCsvFile(strFileFullName, strTimeCreated);
            //2. Write Steplist info
            string strSteplistInfo = "";
            strSteplistInfo = "";
            strSteplistInfo = this.lstChildProcessModel[0].clsChildProgList.strStepListname.Replace(",", ";") + "," +
                               this.lstChildProcessModel[0].clsChildProgList.strStepListVersion.Replace(",", ";") + "," +
                               this.lstChildProcessModel[0].clsChildProgList.strStepListDateCreated.Replace(",", ";");

            AppendCsvFile(strFileFullName, strSteplistInfo);
            //3. Write Program version Info
            string strProgramInfo = "";
            strProgramInfo = "Program Version: " + "," + this.strProgramVer + ","  + this.strProgramDateCreated;
            AppendCsvFile(strFileFullName, strProgramInfo);
            //4.0. Write Test Number 
            string strTestNumberRow = "";

            //Item Number
            strTestNumberRow = "" + ",";
            strTestNumberRow = strTestNumberRow + ","; //PC Name
            strTestNumberRow = strTestNumberRow + ","; //PC IP
            strTestNumberRow = strTestNumberRow + ","; //Mac Address

            //Pre Info
            for (i = 0; i < this.lstChildProcessModel[0].lststrUserPreInfo.Count; i++)
            {
                strTestNumberRow = strTestNumberRow + "" + ",";
            }
            //Time & Result
            strTestNumberRow = strTestNumberRow + "" + ",";
            strTestNumberRow = strTestNumberRow + "" + ",";
            //Error Message
            strTestNumberRow = strTestNumberRow + "" + ",";
            //Test Number
            for (i = 0; i < this.lstChildProcessModel[0].lstChildTotal.Count; i++)
            {
                strTestNumberRow = strTestNumberRow + this.lstChildProcessModel[0].lstChildTotal[i].intTestNumber.ToString().Replace(",", ";") + ",";
            }
            AppendCsvFile(strFileFullName, strTestNumberRow);

            //4.1. Write Info Header that user want to save
            // "Item No" - "PreInfo 1" - ... - "PreInfo n" - "Time" - "Result" - "Error Message" - "Step 1" - ...."Step n" - "AfterInfo 1" - ... - "AfterInfo n"
            
            string strUserInfo = "";

            strUserInfo = "PC Name" + ",";
            strUserInfo = strUserInfo + "IP Add" + ",";
            strUserInfo = strUserInfo + "Mac Add" + ",";

            //Item Number
            strUserInfo = strUserInfo + "Item No" + ",";

            //Pre Info Header
            if (this.lstChildProcessModel[0].lststrUserPreInfo.Count<= this.lststrUserPreInfoHeader.Count)
            {
                for (i = 0; i < this.lstChildProcessModel[0].lststrUserPreInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + this.lststrUserPreInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.lststrUserPreInfoHeader.Count; i++)
                {
                    strUserInfo = strUserInfo + this.lststrUserPreInfoHeader[i] + ",";
                }

                for (i = this.lststrUserPreInfoHeader.Count; i < this.lstChildProcessModel[0].lststrUserPreInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + "PreInfo" + (i + 1).ToString() + ",";
                }
            }


            //Time & Result
            strUserInfo = strUserInfo + "Time" + ",";
            strUserInfo = strUserInfo + "Result" + ",";
            //Error Message
            strUserInfo = strUserInfo + "Error Message" + ",";
            //Test Name
            
            for (i = 0; i < this.lstChildProcessModel[0].lstChildTotal.Count; i++)
            {
                strUserInfo = strUserInfo + this.lstChildProcessModel[0].lstChildTotal[i].strTestName.Replace(",", ";") + ",";
            }

            //After Info
            //for (i = 0; i < this.lstChildProcessModel[0].lststrUserAfterInfo.Count; i++)
            //{
            //    strUserInfo = strUserInfo + "AfterInfo" + (i + 1).ToString() + ",";
            //}

            if (this.lstChildProcessModel[0].lststrUserAfterInfo.Count <= this.lststrUserAfterInfoHeader.Count)
            {
                for (i = 0; i < this.lstChildProcessModel[0].lststrUserAfterInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + this.lststrUserAfterInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.lststrUserAfterInfoHeader.Count; i++)
                {
                    strUserInfo = strUserInfo + this.lststrUserAfterInfoHeader[i] + ",";
                }

                for (i = this.lststrUserAfterInfoHeader.Count; i < this.lstChildProcessModel[0].lststrUserAfterInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + "AfterInfo" + (i + 1).ToString() + ",";
                }
            }

            AppendCsvFile(strFileFullName, strUserInfo);
        }

        public void RecordTestData()
        {
            // "Item No" - "PreInfo 1" - ... - "PreInfo n" - "Time" - "Result" - "Error Message" - "Step 1" - ...."Step n" - "AfterInfo 1" - ... - "AfterInfo n"
            
            //Before saving data, checking needing create new file or not (when enter a new day)
            //Each day create 1 csv file to store checking data if not exist
            //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
            string strDataFileName = "";
            DateTime _Now = DateTime.Now;
            strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";

            string strDataFileFullName = this.strDataSavePath + @"\" + strDataFileName;
            var clsTemp = new nspFileHandle.ChkExist();
            if (clsTemp.CheckFileExist(strDataFileFullName) == false) //Need to create new file
            {
                //Create new file
                File.Create(strDataFileFullName).Close();
                //Create header for file
                CreateFileHeader(strDataFileFullName);
            }
            //Start to saving data
            int i = 0;
            int j = 0;
            int intTemp = 0;
            string  strItemRecordData = "";

            for (i = 0; i < this.lstChildProcessModel.Count; i++)
            {
                strItemRecordData = "";
                //PC Info
                strItemRecordData = strItemRecordData + strPCName + ",";
                strItemRecordData = strItemRecordData + strIPAddress + ",";
                strItemRecordData = strItemRecordData + strMacAddress + ",";

                //Item Number
                strItemRecordData = strItemRecordData + "Item" + (i + 1).ToString() + ",";
                //PreInfo
                for (j = 0; j < this.lstChildProcessModel[i].lststrUserPreInfo.Count; j++)
                {
                    strItemRecordData = strItemRecordData + this.lstChildProcessModel[i].lststrUserPreInfo[j].Replace(",", ";") + ",";
                }
                //Checking Time
                strItemRecordData = strItemRecordData + _Now.ToString() + ",";
                //Checking Result
                string strResult = "";
                if(this.lstChildProcessModel[i].clsItemInfo.blItemCheckingResult == true)
                {
                    strResult = "PASS";
                    strItemRecordData = strItemRecordData + strResult + ",";
                    //Error Message (Clear if PASS)
                    strItemRecordData = strItemRecordData + "" + ",";
                    //If result is PASS, then record all step
                    for (j = 0; j < this.lstChildProcessModel[i].lstChildTotal.Count; j++)
                    {
                        if (this.lstChildProcessModel[i].lstChildTotal[j].strUnitName.ToUpper() != "H") //Not Hexa format
                        {
                            strItemRecordData = strItemRecordData + this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j].ToString() + ",";
                        }
                        else //Hexa format
                        {
                            //Try to convert to Hexa format
                            if (int.TryParse(Math.Round(this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j], 0).ToString(), out intTemp) == true) //Integer => can convert to hexa
                            {
                                strItemRecordData = strItemRecordData + intTemp.ToString("X") + ",";
                            }
                            else
                            {
                                strItemRecordData = strItemRecordData + "Error: cannot convert to Hexa [" + this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j].ToString() + "],";
                            }
                        }
                    }
                }
                else
                {
                    strResult = "FAIL";
                    strItemRecordData = strItemRecordData + strResult + ",";
                    //Error Message (Record if FAIL)
                    string strErrMsg = "";
                    //Looking for error message
                    for (j = 0; j < this.lstChildProcessModel[i].lstChildTotal.Count; j++)
                    {
                        int intStepPos = 0;
                        intStepPos = this.lstChildProcessModel[i].lstChildThreadCheck[j].intTestPos;
                        if ((this.lstChildProcessModel[i].clsItemInfo.lstblStepResult[intStepPos] == false) && (this.lstChildProcessModel[i].lstChildTotal[intStepPos].intTestClass == 3))
                        {
                            strErrMsg = "Step" + this.lstChildProcessModel[i].lstChildTotal[intStepPos].intTestNumber.ToString() + ": ";
                            strErrMsg = strErrMsg + this.lstChildProcessModel[i].clsItemInfo.lststrStepErrMsg[intStepPos];
                            strErrMsg = strErrMsg.Replace(",", ";");
                            break;
                        }
                    }
                    strItemRecordData = strItemRecordData + strErrMsg + ",";
                    //If result is FAIL, then record all pass step, until reach the first fail step
                    for (j = 0; j < this.lstChildProcessModel[i].lstChildTotal.Count; j++)
                    {
                        //strItemRecordData = strItemRecordData + Program.lstChkInfo[i].clsChildProcess.clsItemResult.lstdblStepCheckingData[j].ToString() + ",";

                        if (this.lstChildProcessModel[i].lstChildTotal[j].strUnitName.ToUpper() != "H") //Not Hexa format
                        {
                            strItemRecordData = strItemRecordData + this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j].ToString() + ",";
                        }
                        else //Hexa format
                        {
                            //Try to convert to Hexa format
                            if (int.TryParse(Math.Round(this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j], 0).ToString(), out intTemp) == true) //Integer => can convert to hexa
                            {
                                strItemRecordData = strItemRecordData + intTemp.ToString("X") + ",";
                            }
                            else
                            {
                                strItemRecordData = strItemRecordData + "Error: cannot convert to Hexa [" + this.lstChildProcessModel[i].clsItemInfo.lstdblStepCheckingData[j].ToString() + "],";
                            }
                        }


                        if ((this.lstChildProcessModel[i].clsItemInfo.lstblStepResult[j] == false)
                            && (this.lstChildProcessModel[i].lstChildTotal[j].intTestClass == 3))
                        {
                            break;
                        }
                    }
                    //For all remaining step, just save "" (nothing) character
                    for (int k = j + 1; k < this.lstChildProcessModel[i].lstChildTotal.Count; k++)
                    {
                        strItemRecordData = strItemRecordData + "" + ",";
                    }
                }

                //AfterInfo
                for (j = 0; j < this.lstChildProcessModel[i].lststrUserAfterInfo.Count; j++)
                {
                    strItemRecordData = strItemRecordData + this.lstChildProcessModel[i].lststrUserAfterInfo[j].Replace(",", ";") + ",";
                }
                //Record to csv file
                AppendCsvFile(strDataFileFullName, strItemRecordData);

                //After saving data we have to clear all user Pre-Info & After-Info
                for (j = 0; j < this.lstChildProcessModel[i].lststrUserPreInfo.Count; j++)
                {
                    this.lstChildProcessModel[i].lststrUserPreInfo[j] = "";
                }

                for (j = 0; j < this.lstChildProcessModel[i].lststrUserAfterInfo.Count; j++)
                {
                    this.lstChildProcessModel[i].lststrUserAfterInfo[j] = "";
                }

            } //End for i


        } //End RecordTestData() method

        public void AppendCsvFile(string strFileFullName, string strDataToAppend)
        {
            try
            {
                StreamWriter sWriter;
                sWriter =  File.AppendText(strFileFullName);
                sWriter.WriteLine(strDataToAppend);
                sWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AppendCsvFile()");
            }
        }

    }
}
