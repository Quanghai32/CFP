using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace nspSavingData
{
    public class clsSavingData
    {
        public string strPCName { get; set; }
        public string strIPAddress { get; set; }
        public string strMacAddress { get; set; }
        public bool blSaveProtectionCode { get; set; }

        //For Child Control Model object reference 
        public nspChildProcessModel.clsChildControlModel objChildControlModel { get; set; }

        public void SavingDataIni()
        {
            //For Program List data saving
            SavingProgramListDataIni();

            //For step list saving data
            if(this.objChildControlModel.lstChildProcessModel[0].clsChildSetting.blUsingOriginSteplist==true)
            {
                SavingSteplistDataIni();
            } 
        }

        //For CFP Data saving
        public void SavingProgramListDataIni()
        {
            //1. Check if Data folder is exist or not. If not, then create new data folder for saving
            try
            {
                if (MyLibrary.ChkExist.CheckFolderExist(this.objChildControlModel.clsMainVar.strDataSavePath) == false)
                {
                    Directory.CreateDirectory(this.objChildControlModel.clsMainVar.strDataSavePath);
                }
                //2. Each day create 1 csv file to store checking data if not exist
                //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
                string strDataFileName = "";
                DateTime _Now = DateTime.Now;
                strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";

                string strDataFileFullName = this.objChildControlModel.clsMainVar.strDataSavePath + @"\" + strDataFileName;
                //MessageBox.Show(strDataFileName);
                if (MyLibrary.ChkExist.CheckFileExist(strDataFileFullName) == false)
                {
                    File.Create(strDataFileFullName).Close();
                }
                //3. Create File Header
                CreateFileHeaderProgramList(strDataFileFullName);

                //4. Find out some Information of PC
                this.strMacAddress = MyLibrary.clsMyFunc.GetMACAddress();
                this.strIPAddress = MyLibrary.clsMyFunc.GetIPv4Address();
                this.strPCName = MyLibrary.clsMyFunc.GetPcName();

            }
            catch (Exception ex)
            {
                MessageBox.Show("SavingProgramListDataIni() error: " + ex.Message);
                Environment.Exit(0);
            }
        }

        //For support old style step list saving data
        public void SavingSteplistDataIni()
        {
            //1. Check if Data folder is exist or not. If not, then create new data folder for saving
            try
            {
                
                if (MyLibrary.ChkExist.CheckFolderExist(this.objChildControlModel.clsMainVar.strStepListDataSavePath) == false)
                {
                    Directory.CreateDirectory(this.objChildControlModel.clsMainVar.strStepListDataSavePath);
                }
                //2. Each day create 1 csv file to store checking data if not exist
                //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
                string strDataFileName = "";
                DateTime _Now = DateTime.Now;
                //strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";
                strDataFileName = _Now.ToString("yyMMdd") + ".csv";

                string strDataFileFullName = this.objChildControlModel.clsMainVar.strStepListDataSavePath + @"\" + strDataFileName;

                //Create new file if it not yet exist
                if (MyLibrary.ChkExist.CheckFileExist(strDataFileFullName) == false)
                {
                    File.Create(strDataFileFullName).Close();
                }
                else //If File already exist, then rename it & create new file
                {
                    //Find new name to remove, add "-1", "-2", "-3"... if necessary
                    string strNewFilePath = "";
                    bool blFound = false;
                    int i = 0;

                    while(blFound==false)
                    {
                        string strNewFileName = _Now.ToString("yyMMdd") + "-" + (i+1).ToString() + ".csv";
                        strNewFilePath = this.objChildControlModel.clsMainVar.strStepListDataSavePath + @"\" + strNewFileName;
                        if (MyLibrary.ChkExist.CheckFileExist(strNewFilePath) == false) //OK
                        {
                            blFound = true;
                            break;
                        }
                        else //Already exist, need to find new name
                        {
                            i = i + 1;
                        }
                    }

                    //Rename old file
                    File.Move(strDataFileFullName, strNewFilePath);

                    //Delete old file
                    File.Delete(strDataFileFullName);
                }

                //3. Create File Header
                CreateFileHeaderStepList(strDataFileFullName);
            }
            catch //(Exception ex)
            {
                MessageBox.Show("Can not create folder [" + this.objChildControlModel.clsMainVar.strStepListDataSavePath + "] to save data! The Drive is not exist? Please check!", "SavingSteplistDataIni()");
                Environment.Exit(0);
            }
        }

        //For CFP Data saving
        public void CreateFileHeaderProgramList(string strFileFullName)
        {
            int i = 0;
            //
            AppendCsvFile(strFileFullName, "");
            //1. Write Time to creating or start writing to csv File
            string strTimeCreated = "";
            DateTime _Now = DateTime.Now;
            strTimeCreated = "Started Time:" + "," + _Now.ToString();
            AppendCsvFile(strFileFullName, strTimeCreated);

            ////Write Protection Code
            //string strProtectionCodeInfo = "";
            //strProtectionCodeInfo = "Protection Code: " + "," + this.objChildControlModel.clsVerifyProtectInfo.strTotalProtectCode + ","+
            //                        "Source Code: " + this.objChildControlModel.clsVerifyProtectInfo.strSourceProtectCode + ",";

            //2. Write Steplist info
            string strProglistInfo = "";
            strProglistInfo = "";
            strProglistInfo = this.objChildControlModel.lstChildProcessModel[0].clsProgramList.strProgramListname.Replace(",", ";") + "," +
                               this.objChildControlModel.lstChildProcessModel[0].clsProgramList.strProgramListVersion.Replace(",", ";") + "," +
                               this.objChildControlModel.lstChildProcessModel[0].clsProgramList.strProgramListDateCreated.Replace(",", ";");

            AppendCsvFile(strFileFullName, strProglistInfo);
            //3. Write Program version Info
            string strTempInfo = "";
            strTempInfo = "Program Version: " + "," + this.objChildControlModel.strProgramVer + "," + this.objChildControlModel.strProgramDateCreated();
            AppendCsvFile(strFileFullName, strTempInfo);

            //3.1. Write checking Mode Info
            strTempInfo = "";
            strTempInfo = strTempInfo + "Run Mode: " + this.objChildControlModel.clsMainVar.strChildRunningMode + ",";
            strTempInfo = strTempInfo + "Checking Mode: " + this.objChildControlModel.lstChildProcessModel[0].strSystemCheckingMode + ",";
            strTempInfo = strTempInfo + "Group Mode: " + this.objChildControlModel.clsMainVar.blGroupMode.ToString() + ",";

            AppendCsvFile(strFileFullName, strTempInfo);
            //4.0. Write Test Number 
            string strTestNumberRow = "";

            //Item Number
            strTestNumberRow = "" + ",";
            strTestNumberRow = strTestNumberRow + ","; //PC Name
            strTestNumberRow = strTestNumberRow + ","; //PC IP
            strTestNumberRow = strTestNumberRow + ","; //Mac Address

            //Pre Info
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count; i++)
            {
                strTestNumberRow = strTestNumberRow + "" + ",";
            }
            //Time & Result
            strTestNumberRow = strTestNumberRow + "" + ","; //time
            strTestNumberRow = strTestNumberRow + "" + ","; //tact time
            strTestNumberRow = strTestNumberRow + "" + ","; //result
            //Error info
            strTestNumberRow = strTestNumberRow + "" + ","; //FAIl STEP
            strTestNumberRow = strTestNumberRow + "" + ","; //FAIl DATA
            //Test Number
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                //Checking step is blank or user function name row
                if (this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepSequenceID==-1)
                {
                    strTestNumberRow = strTestNumberRow + ",";
                    continue;
                }
                strTestNumberRow = strTestNumberRow + this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepNumber.ToString().Replace(",", ";") + ",";
            }
            AppendCsvFile(strFileFullName, strTestNumberRow);

            //4.0.1 Write Function ID
            string strFuncID = "";

            //Item Number
            strFuncID = "" + ",";
            strFuncID = strFuncID + ","; //PC Name
            strFuncID = strFuncID + ","; //PC IP
            strFuncID = strFuncID + ","; //Mac Address

            //Pre Info
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count; i++)
            {
                strFuncID = strFuncID + "" + ",";
            }
            //Time & Result
            strFuncID = strFuncID + "" + ","; //time
            strFuncID = strFuncID + "" + ","; //tact time
            strFuncID = strFuncID + "" + ","; //result
            //Error info
            strFuncID = strFuncID + "" + ","; //FAIl STEP
            strFuncID = strFuncID + "" + ","; //FAIl DATA
            //Test Number
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                //Checking step is blank or user function name row
                if (this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepSequenceID==-1)
                {
                    if(i+1<this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count)
                    {
                        if(this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i+1].intStepSequenceID==1)
                        {
                            strFuncID = strFuncID + "FUNC,";
                            continue;
                        }
                    }

                    strFuncID = strFuncID + ",";
                    continue;
                }

                string strNewTemp = "";
                strNewTemp = this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepClass.ToString().Replace(",", ";") + "-" +
                                this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intJigId.ToString().Replace(",", ";") + "-" +
                                this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intHardwareId.ToString().Replace(",", ";") + "-" +
                                this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intFunctionId.ToString().Replace(",", ";");
                strFuncID = strFuncID + "(" + strNewTemp + "),";
            }
            AppendCsvFile(strFileFullName, strFuncID);


            //4.0.2 Write Lo Spec, Hi Spec & Unit name
            string strSpec = "";

            //Item Number
            strSpec = "" + ",";
            strSpec = strSpec + ","; //PC Name
            strSpec = strSpec + ","; //PC IP
            strSpec = strSpec + ","; //Mac Address

            //Pre Info
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count; i++)
            {
                strSpec = strSpec + "" + ",";
            }
            //Time & Result
            strSpec = strSpec + "" + ","; //time
            strSpec = strSpec + "" + ","; //tact time
            strSpec = strSpec + "" + ","; //result
            //Error info
            strSpec = strSpec + "" + ","; //FAIl STEP
            strSpec = strSpec + "" + ","; //FAIl DATA
            //Test Number
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                //Checking step is blank or user function name row
                if(this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepSequenceID==-1) //No caring
                {
                    strSpec = strSpec + ",";
                    continue;
                }

                string strNewTemp = "";

                if(this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].strUnitName.ToUpper().Trim()=="H")
                {
                    strNewTemp = Convert.ToInt32(this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].objLoLimit.ToString()).ToString("X").Replace(",", ";") + "->" +
                                   Convert.ToInt32(this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].objHiLimit.ToString()).ToString("X").Replace(",", ";") + " " +
                                   this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].strUnitName.Replace(",", ";");
                }
                else
                {
                    strNewTemp = this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].objLoLimit.ToString().Replace(",", ";") + "->" +
                                   this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].objHiLimit.ToString().Replace(",", ";") + " " +
                                   this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].strUnitName.Replace(",", ";");
                }
               
                strSpec = strSpec + "(" + strNewTemp + "),";
            }
            AppendCsvFile(strFileFullName, strSpec);

            //4.1. Write Info Header that user want to save
            // "Item No" - "PreInfo 1" - ... - "PreInfo n" - "Time" - "Result" - "Error Message" - "Step 1" - ...."Step n" - "AfterInfo 1" - ... - "AfterInfo n"
            
            string strUserInfo = "";

            strUserInfo = "PC Name" + ",";
            strUserInfo = strUserInfo + "IP Add" + ",";
            strUserInfo = strUserInfo + "Mac Add" + ",";

            //Item Number
            strUserInfo = strUserInfo + "Item No" + ",";

            //Pre Info Header
            if (this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count<= this.objChildControlModel.clsMainVar.lststrProgramListUserPreInfoHeader.Count)
            {
                for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + this.objChildControlModel.clsMainVar.lststrProgramListUserPreInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.objChildControlModel.clsMainVar.lststrProgramListUserPreInfoHeader.Count; i++)
                {
                    strUserInfo = strUserInfo + this.objChildControlModel.clsMainVar.lststrProgramListUserPreInfoHeader[i] + ",";
                }

                for (i = this.objChildControlModel.clsMainVar.lststrProgramListUserPreInfoHeader.Count; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserPreInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + "PreInfo" + (i + 1).ToString() + ",";
                }
            }


            //Time & Result & Tact time
            strUserInfo = strUserInfo + "Time" + ",";
            strUserInfo = strUserInfo + "Tact time (s)" + ",";
            strUserInfo = strUserInfo + "Result" + ",";
            
            ////Error Message
            //strUserInfo = strUserInfo + "Error Message" + ",";

            strUserInfo = strUserInfo + "STEP FAIL" + ",";
            strUserInfo = strUserInfo + "FAIL DATA" + ",";

            //Test Name
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count; i++)
            {
                //Checking step is blank or user function name row
                if (this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].intStepSequenceID==-1)
                {
                    if (i + 1 < this.objChildControlModel.lstChildProcessModel[0].lstTotalStep.Count)
                    {
                        if (this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i + 1].intStepSequenceID == 1) //User Function Name
                        {
                            strUserInfo = strUserInfo + this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i + 1].strUserFunctionName + ",";
                            continue;
                        }
                    }

                    strUserInfo = strUserInfo + ",";
                    continue;
                }

                string strTest = this.objChildControlModel.lstChildProcessModel[0].lstTotalStep[i].strStepName.Replace(",", ";");
                strTest = strTest.Replace("\n", " ");
                strTest = strTest.Replace("\r", " ");
                //strUserInfo = strUserInfo + this.objChildControlModel.lstChildProcessModel[0].lstChildTotal[i].strTestName.Replace(",", ";") + ",";
                strUserInfo = strUserInfo + strTest + ",";
            }


            if (this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserAfterInfo.Count <= this.objChildControlModel.clsMainVar.lststrProgramListUserAfterInfoHeader.Count)
            {
                for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserAfterInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + this.objChildControlModel.clsMainVar.lststrProgramListUserAfterInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.objChildControlModel.clsMainVar.lststrProgramListUserAfterInfoHeader.Count; i++)
                {
                    strUserInfo = strUserInfo + this.objChildControlModel.clsMainVar.lststrProgramListUserAfterInfoHeader[i] + ",";
                }

                for (i = this.objChildControlModel.clsMainVar.lststrProgramListUserAfterInfoHeader.Count; i < this.objChildControlModel.lstChildProcessModel[0].lststrProgramListUserAfterInfo.Count; i++)
                {
                    strUserInfo = strUserInfo + "AfterInfo" + (i + 1).ToString() + ",";
                }
            }

            AppendCsvFile(strFileFullName, strUserInfo);
        }

        //For support old style step list saving data
        public void CreateFileHeaderStepList(string strFileFullName)
        {
            int i = 0;
            string strSteplistInfo = "";
            string strTemp = "";

            //1. Step List Name
            strSteplistInfo = this.objChildControlModel.lstChildProcessModel[0].clsStepList.strStepListname;
            AppendCsvFile(strFileFullName, strSteplistInfo);

            //2. Step List Version & Step List Step Number
            strSteplistInfo = "";
            //Step list version
            strSteplistInfo = "CodeVer" + "," + this.objChildControlModel.lstChildProcessModel[0].clsStepList.strStepListVersion + ",";
            //
            strSteplistInfo = strSteplistInfo + ","; //Fail - pass collumn
            strSteplistInfo = strSteplistInfo + "Test No." + ","; //Test number label

            //User Pre Info of Step List data saving
            //for (i = 0; i < this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader.Count;i++)
            //{
            //    strSteplistInfo = strSteplistInfo + ",";
            //}

            for (i = 0; i < this.objChildControlModel.clsMainVar.intStepListNumberUserPreInfo; i++)
            {
                strSteplistInfo = strSteplistInfo + ",";
            }


            //Step list number
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList.Count;i++)
            {
                strSteplistInfo = strSteplistInfo + this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].intStepNumber.ToString() + ",";
            }

            //User After Info of Step List data saving
            for (i = 0; i < this.objChildControlModel.clsMainVar.intStepListNumberUserAfterInfo; i++)
            {
                strSteplistInfo = strSteplistInfo + ",";
            }

            //Write to CSV file
            AppendCsvFile(strFileFullName, strSteplistInfo);

            //2. Write Program list version Info & Test name
            strSteplistInfo = "LimitVer" + ",";
            strSteplistInfo = strSteplistInfo + this.objChildControlModel.lstChildProcessModel[0].clsProgramList.strProgramListVersion + ",";

            //
            strSteplistInfo = strSteplistInfo + ","; //Fail - pass collumn
            strSteplistInfo = strSteplistInfo + "Name" + ","; //name of step number

            //User Pre Info of Step List data saving
            if (this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserPreInfo.Count <= this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader.Count)
            {
                for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserPreInfo.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader[i] + ",";
                }

                for (i = this.objChildControlModel.clsMainVar.lststrStepListUserPreInfoHeader.Count; i < this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserPreInfo.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + "PreInfo" + (i + 1).ToString() + ",";
                }
            }


            //Step Name
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList.Count; i++)
            {
                strTemp = this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].strStepName.Replace(",", ";");
                strTemp = strTemp.Replace("\n", " ");
                strTemp = strTemp.Replace("\r", " ");

                strSteplistInfo = strSteplistInfo + strTemp + ",";
            }

            //
            if (this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserAfterInfo.Count <= this.objChildControlModel.clsMainVar.lststrStepListUserAfterInfoHeader.Count)
            {
                for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserAfterInfo.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + this.objChildControlModel.clsMainVar.lststrStepListUserAfterInfoHeader[i] + ",";
                }
            }
            else //Not enough header setting
            {
                for (i = 0; i < this.objChildControlModel.clsMainVar.lststrStepListUserAfterInfoHeader.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + this.objChildControlModel.clsMainVar.lststrStepListUserAfterInfoHeader[i] + ",";
                }

                for (i = this.objChildControlModel.clsMainVar.lststrStepListUserAfterInfoHeader.Count; i < this.objChildControlModel.lstChildProcessModel[0].lststrStepListUserAfterInfo.Count; i++)
                {
                    strSteplistInfo = strSteplistInfo + "AfterInfo" + (i + 1).ToString() + ",";
                }
            }


            //Write to CSV file
            AppendCsvFile(strFileFullName, strSteplistInfo);


            //3. Write Ini Ver & Low limit
            strSteplistInfo = "IniVer" + ",";

            strSteplistInfo = strSteplistInfo + ","; //Tact time collumn
            strSteplistInfo = strSteplistInfo + ","; //Pass Fail collumn

            strSteplistInfo = strSteplistInfo + "Lo Limit" + ",";

            //User Pre Info of Step List data saving
            for (i = 0; i < this.objChildControlModel.clsMainVar.intStepListNumberUserPreInfo; i++)
            {
                strSteplistInfo = strSteplistInfo + ",";
            }

            //Low limit
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList.Count; i++)
            {
                strTemp = "";

                if (this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].strUnitName.ToUpper().Trim() == "H")
                {
                    strTemp = Convert.ToInt32(this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].objLoLimit).ToString("X").Replace(",", ";");
                }
                else
                {
                    strTemp = this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].objLoLimit.ToString().Replace(",", ";");
                }

                strSteplistInfo = strSteplistInfo + strTemp + ",";
            }

            //Write to CSV file
            AppendCsvFile(strFileFullName, strSteplistInfo);

            //4. WorkNo & Hi Limit
            strSteplistInfo = "WorkNo" + ",";

            strSteplistInfo = strSteplistInfo + "Tact" + ","; //Tact time collumn
            strSteplistInfo = strSteplistInfo + "" + ","; //Pass Fail Collumn
            strSteplistInfo = strSteplistInfo + "Hi Limit" + ","; //Hi limit Collumn

            //User Pre Info of Step List data saving
            for (i = 0; i < this.objChildControlModel.clsMainVar.intStepListNumberUserPreInfo; i++)
            {
                strSteplistInfo = strSteplistInfo + ",";
            }

            //Hi limit
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList.Count; i++)
            {
                strTemp = "";

                if (this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].strUnitName.ToUpper().Trim() == "H")
                {
                    strTemp = Convert.ToInt32(this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].objHiLimit).ToString("X").Replace(",", ";");
                }
                else
                {
                    strTemp = this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].objHiLimit.ToString().Replace(",", ";");
                }

                strSteplistInfo = strSteplistInfo + strTemp + ",";
            }

            //Write to CSV file
            AppendCsvFile(strFileFullName, strSteplistInfo);

            //5. Unit name
            strSteplistInfo = "" + ","; //Work No collumn
            strSteplistInfo = strSteplistInfo + "Sec" + ","; //Tact time unit
            strSteplistInfo = strSteplistInfo + "" + ","; //Pass Fail Collumn
            strSteplistInfo = strSteplistInfo + "Unit" + ","; //Unit Row
            //User Pre Info of Step List data saving
            for (i = 0; i < this.objChildControlModel.clsMainVar.intStepListNumberUserPreInfo; i++)
            {
                strSteplistInfo = strSteplistInfo + ",";
            }

            //Unit name
            for (i = 0; i < this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList.Count; i++)
            {
                strSteplistInfo = strSteplistInfo + this.objChildControlModel.lstChildProcessModel[0].clsStepList.lstExcelList[i].strUnitName.Replace(",", ";") + ",";
            }

            //Write to CSV file
            AppendCsvFile(strFileFullName, strSteplistInfo);
        }

        //Record test data
        public void RecordTestData(int intGroupProcessID = -1)
        {
            //1. Record program list data
            this.RecordProgramListTestData(intGroupProcessID);

            //2. Record step list data
            if(this.objChildControlModel.clsMainVar.blUsingOriginSteplist == true)
            {
                this.RecordStepListTestData(intGroupProcessID);
            }

        } //End RecordTestData() method

        public void RecordProgramListTestData(int intProcessID = -1)
        {
            // "Item No" - "PreInfo 1" - ... - "PreInfo n" - "Time" - "Result" - "Error Message" - "Step 1" - ...."Step n" - "AfterInfo 1" - ... - "AfterInfo n"

            //Before saving data, checking needing create new file or not (when enter a new day)
            //Each day create 1 csv file to store checking data if not exist
            //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
            string strDataFileName = "";
            DateTime _Now = DateTime.Now;
            strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";

            string strDataFileFullName = this.objChildControlModel.clsMainVar.strDataSavePath + @"\" + strDataFileName;
            
            if (MyLibrary.ChkExist.CheckFileExist(strDataFileFullName) == false) //Need to create new file
            {
                //Create new file
                File.Create(strDataFileFullName).Close();
                //Create header for file
                CreateFileHeaderProgramList(strDataFileFullName);
            }

            //Save Protection Code
            if (this.blSaveProtectionCode == false)
            {
                if(this.objChildControlModel.clsVerifyProtectInfo.blDone == true)
                {
                    //Write Protection Code
                    string strProtectionCodeInfo = "";
                    strProtectionCodeInfo = "Protection Code: " + "," + this.objChildControlModel.clsVerifyProtectInfo.strTotalProtectCode + "," +
                                            "Source Code: " + this.objChildControlModel.clsVerifyProtectInfo.strSourceProtectCode + ",";
                    AppendCsvFile(strDataFileFullName, strProtectionCodeInfo);
                    this.blSaveProtectionCode = true;
                }
            }

            //Start to saving data
            int i = 0;
            int j = 0;
            int k = 0;
            string strItemRecordData = "";

            for (i = 0; i < this.objChildControlModel.lstChildProcessModel.Count; i++)
            {
                //Check what process need to save data
                if(intProcessID!=-1) // -1 mean save all process data
                {
                    if (i != intProcessID) continue; //Not process want to save ID
                }

                //First saving of group class data
                //Only saving data of what item request saving
                if (this.objChildControlModel.lstChildProcessModel[i].blRequestSavingData == false) continue;

                //***************************Saving Data of child process*********************************************
                strItemRecordData = "";
                //PC Info
                strItemRecordData = strItemRecordData + strPCName + ",";
                strItemRecordData = strItemRecordData + strIPAddress + ",";
                strItemRecordData = strItemRecordData + strMacAddress + ",";

                //Item Number
                strItemRecordData = strItemRecordData + "Process" + (i + 1).ToString() + ",";

                //PreInfo
                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserPreInfo.Count; j++)
                {
                    strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserPreInfo[j].Replace(",", ";") + ",";
                }
                //Checking Time
                strItemRecordData = strItemRecordData + _Now.ToString() + ",";

                //Tact time
                strItemRecordData = strItemRecordData + Math.Round(this.objChildControlModel.lstChildProcessModel[i].clsItemResult.dblItemTactTime, 3).ToString() + ",";

                //If Item was skipped. Then marking and continue on another item data saving
                if (this.objChildControlModel.lstChildProcessModel[i].blSkipModeRequest == true)
                {
                    strItemRecordData = strItemRecordData + "Skipped" + ",";
                    //Record to csv file
                    AppendCsvFile(strDataFileFullName, strItemRecordData);
                    continue;
                }

                //Checking Result
                string strResult = "";
                if (this.objChildControlModel.lstChildProcessModel[i].clsItemResult.blItemCheckingResult == true)
                {
                    //Result
                    strResult = "PASS";
                    strItemRecordData = strItemRecordData + strResult + ",";

                    //Error Message (Clear if PASS)
                    strItemRecordData = strItemRecordData + "" + ","; //Step Fail
                    strItemRecordData = strItemRecordData + "" + ","; //Fail Data
                }
                else
                {
                    strResult = "FAIL";
                    strItemRecordData = strItemRecordData + strResult + ",";
                    //Error Message (Record if FAIL)
                    string strStepFail = "";
                    string strFailData = "";

                    //Looking for error info
                    for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lstTotalStep.Count; j++)
                    {
                        if(this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].blStepChecked==true)
                        {
                            if ((this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].blStepResult == false) && (this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].intStepClass == 3))
                            {
                                strStepFail = this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].intStepNumber.ToString();
                                strFailData = this.objChildControlModel.lstChildProcessModel[i].GetConvertDataResult(j).Replace(",", ";");
                                break;
                            }
                        }
                    }
                    strItemRecordData = strItemRecordData + strStepFail + ",";
                    strItemRecordData = strItemRecordData + strFailData + ",";
                }


                //If any step which already checked, then we save its result
                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lstTotalStep.Count; j++)
                {
                    //Check if Row is blank
                    if (this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].intStepSequenceID == -1) //No caring
                    {
                        strItemRecordData = strItemRecordData + ",";
                        continue;
                    }

                    if (this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[j].blStepChecked == true)
                    {
                        strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].GetConvertDataResult(j).Replace(",", ";") + ",";
                    }
                    else
                    {
                        strItemRecordData = strItemRecordData + ",";
                    }
                }

                //AfterInfo
                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserAfterInfo.Count; j++)
                {
                    strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserAfterInfo[j].Replace(",", ";") + ",";
                }
                //Record to csv file
                AppendCsvFile(strDataFileFullName, strItemRecordData);

                //After saving data we have to clear all user Pre-Info & After-Info
                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserPreInfo.Count; j++)
                {
                    this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserPreInfo[j] = "";
                }

                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserAfterInfo.Count; j++)
                {
                    this.objChildControlModel.lstChildProcessModel[i].lststrProgramListUserAfterInfo[j] = "";
                }

                //***************************Saving Data of Item of child process*********************************************
                //If setting in Group mode, we save data of sub-child process 
                for (j = 0; j < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo.Count; j++)
                {
                    //Only saving data of what item request saving
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].blRequestSavingData == false) continue;

                    //
                    strItemRecordData = "";
                    //PC Info
                    strItemRecordData = strItemRecordData + strPCName + ",";
                    strItemRecordData = strItemRecordData + strIPAddress + ",";
                    strItemRecordData = strItemRecordData + strMacAddress + ",";

                    //Item Number
                    strItemRecordData = strItemRecordData + "Item" + (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID + 1).ToString() + ",";

                    //PreInfo
                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserPreInfo.Count; k++)
                    {
                        strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserPreInfo[k].Replace(",", ";") + ",";
                    }

                    //Checking Time
                    strItemRecordData = strItemRecordData + _Now.ToString() + ",";

                    //Tact time
                    strItemRecordData = strItemRecordData + "" + ",";

                    //If Item was skipped. Then marking and continue on another item data saving
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest == true)
                    {
                        strItemRecordData = strItemRecordData + "Skipped" + ",";
                        //Record to csv file
                        AppendCsvFile(strDataFileFullName, strItemRecordData);
                        continue;
                    }

                    //
                    strResult = "";
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult == true)
                    {
                        strResult = "PASS";
                    }
                    else
                    {
                        strResult = "FAIL";
                    }

                    strItemRecordData = strItemRecordData + strResult + ",";

                    //Write to csv file
                    AppendCsvFile(strDataFileFullName, strItemRecordData);

                    //Reset user after & pre Info
                    //After saving data we have to clear all user Pre-Info & After-Info
                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserPreInfo.Count; k++)
                    {
                        this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserPreInfo[k] = "";
                    }

                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserAfterInfo.Count; k++)
                    {
                        this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrProgramListUserAfterInfo[k] = "";
                    }

                } //End for j

            } //End for i

        } //End RecordTestData() method

        public void RecordStepListTestData(int intProcessID = -1)
        {
            //Before saving data, checking needing create new file or not (when begin a new day)
            //Each day create 1 csv file to store checking data if not exist
            string strDataFileName = "";
            DateTime _Now = DateTime.Now;
            //strDataFileName = _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";
            strDataFileName = _Now.ToString("yyMMdd") + ".csv";

            string strDataFileFullName = this.objChildControlModel.clsMainVar.strStepListDataSavePath + @"\" + strDataFileName;

            
            if (MyLibrary.ChkExist.CheckFileExist(strDataFileFullName) == false) //Need to create new file
            {
                //Create new file
                File.Create(strDataFileFullName).Close();
                //Create header for file
                this.CreateFileHeaderStepList(strDataFileFullName);
            }

            //Start to saving data
            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            string strItemRecordData = "";

            for(i=0;i<this.objChildControlModel.lstChildProcessModel.Count;i++)
            {
                if (intProcessID != -1) // -1 mean save all process data
                {
                    if (i != intProcessID) continue; //Not process want to save ID
                }

                for(j=0;j<this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo.Count;j++)
                {
                    //Check what process need to save data
                    int intItemID = this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].intItemID;

                    //Only saving data of what item request saving
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].blRequestSavingData == false) continue;

                    strItemRecordData = "";
                    strItemRecordData = strItemRecordData + "Item" + (intItemID + 1).ToString() + ","; //Work No - Item1, Item2...!

                    //Tact time
                    strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.dblItemTactTime.ToString() + ",";

                    //If Item was skipped. Then marking and continue on another item data saving
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].blSkipModeRequest == true)
                    {
                        strItemRecordData = strItemRecordData + "Skipped" + ",";
                        //Record to csv file
                        AppendCsvFile(strDataFileFullName, strItemRecordData);
                        continue;
                    }

                    //Result
                    if (this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsItemResult.blItemCheckingResult == true)
                    {
                        strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsChildSetting.strPassLabel + ",";
                    }
                    else
                    {
                        strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsChildSetting.strFailLabel + ",";
                    }

                    //Reset Pass/Fail label to default value
                    this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsChildSetting.strPassLabel = "Pass";
                    this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsChildSetting.strFailLabel = "Fail";

                    //Save timing check
                    strItemRecordData = strItemRecordData + _Now.ToString() + ",";

                    //PreInfo
                    for (k = 0; k < this.objChildControlModel.clsMainVar.intStepListNumberUserPreInfo; k++)
                    {
                        //We need to separate Group Mode or Not
                        if(this.objChildControlModel.lstChildProcessModel[i].blGroupMode==true)
                        {
                            strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserPreInfo[k].Replace(",", ";") + ",";
                        }
                        else
                        {
                            strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lststrStepListUserPreInfo[k].Replace(",", ";") + ",";
                        }
                    }

                    //Step Data
                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsStepList.lstExcelList.Count; k++)
                    {
                        //Find representative step in program list
                        int intStepNum = 0;
                        int intStepPos = -1;
                        intStepNum = this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].clsStepList.lstExcelList[k].intStepNumber;

                        //Group Mode: has both group number & origin step number
                        //Using Origin step list but not in Group Mode: Only has origin step number
                        for(l=0;l<this.objChildControlModel.lstChildProcessModel[i].lstTotalStep.Count;l++)
                        {
                            if(this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[l].strGroupNumber ==(j+1).ToString()) //Same Group Number?
                            {
                                if(this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[l].strOriginStepNumber==intStepNum.ToString()) //Same Origin Step number?
                                {
                                    if(this.objChildControlModel.lstChildProcessModel[i].blGroupMode==false) //Not Group Mode: all group number setting same value = 1 => Need to confirm more
                                    {
                                        if(this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[l].intStepNumber==intStepNum) //The step number in program list must be matching with step list
                                        {
                                            intStepPos = l;
                                            break;
                                        }
                                    }
                                    else //In Group Mode, no need confirm anymore
                                    {
                                        intStepPos = l;
                                        break;
                                    }
                                }
                            }
                        }

                        if (intStepPos == -1) //Cannot Found?
                        {
                            strItemRecordData = strItemRecordData + "Not found!" + ",";
                            continue;
                        }

                        //Only Save data of step which is already checked
                        if (this.objChildControlModel.lstChildProcessModel[i].lstTotalStep[intStepPos].blStepChecked == true)
                        {
                            strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].GetConvertDataResult(intStepPos).Replace(",", ";") + ",";
                        }
                        else
                        {
                            strItemRecordData = strItemRecordData + "" + ",";
                        }
                    }

                    //AfterInfo
                    for (k = 0; k < this.objChildControlModel.clsMainVar.intStepListNumberUserAfterInfo; k++)
                    {
                        //We need to separate Group Mode or not
                        if(this.objChildControlModel.lstChildProcessModel[i].blGroupMode==true)
                        {
                            strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserAfterInfo[k].Replace(",", ";") + ",";
                        }
                        else
                        {
                            strItemRecordData = strItemRecordData + this.objChildControlModel.lstChildProcessModel[i].lststrStepListUserAfterInfo[k].Replace(",", ";") + ",";
                        }
                    }

                    //Write to csv file
                    AppendCsvFile(strDataFileFullName, strItemRecordData);

                    //After saving data we have to clear all user Pre-Info & After-Info
                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserPreInfo.Count; k++)
                    {
                        if(this.objChildControlModel.lstChildProcessModel[i].blGroupMode==true)
                        {
                            this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserPreInfo[k] = "";
                        }
                        else
                        {
                            this.objChildControlModel.lstChildProcessModel[i].lststrStepListUserPreInfo[k] = "";
                        }
                    }

                    for (k = 0; k < this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserAfterInfo.Count; k++)
                    {
                        if (this.objChildControlModel.lstChildProcessModel[i].blGroupMode == true)
                        {
                            this.objChildControlModel.lstChildProcessModel[i].lstclsItemCheckInfo[j].lststrStepListUserAfterInfo[k] = "";
                        }
                        else
                        {
                            this.objChildControlModel.lstChildProcessModel[i].lststrStepListUserAfterInfo[k] = "";
                        }
                    }

                } //End for j

            } //End for i
            

        } //End RecordTestData() method

        private static Mutex mut = new Mutex();

        public void AppendCsvFile(string strFileFullName, string strDataToAppend)
        {
            try
            {
                mut.WaitOne();
                //
                StreamWriter sWriter;
                sWriter =  File.AppendText(strFileFullName);
                sWriter.WriteLine(strDataToAppend);
                sWriter.Close();
                //
                mut.ReleaseMutex();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AppendCsvFile()");
            }
        }

        //Constructor
        public clsSavingData(nspChildProcessModel.clsChildControlModel objChildControlModel)
        {
            //Passing object reference
            this.objChildControlModel = objChildControlModel;
        }
    
    }
}
