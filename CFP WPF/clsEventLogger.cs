using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CheckerFrameProgram
{
    public class clsEventLogger
    {
        public string strPathEventLogFolder { get; set; } //Where to save event log file
        public string strPathEventLogFile { get; set; } //What file to save log data

        public string strPCName { get; set; }
        public string strIPAddress { get; set; }
        public string strMacAddress { get; set; }


        public void IniLogger()
        {
            strPathEventLogFolder = Application.StartupPath + @"\EventLog"; //Default save path

            //1. Check if folder is exist or not, if not, then we create new one
            if(System.IO.Directory.Exists(strPathEventLogFolder)==false)
            {
                System.IO.Directory.CreateDirectory(strPathEventLogFolder);
            }

            //2. Check if file logger of current date is exist or not. If not, then create new csv file
            //   File format: "Year_Month_Date.csv" . Example: 2014_9_6.csv
            DateTime _Now = DateTime.Now;
            strPathEventLogFile =  "EventLog" + _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";
            strPathEventLogFile = strPathEventLogFolder + @"\" + strPathEventLogFile;

            if (MyLibrary.ChkExist.CheckFileExist(strPathEventLogFile) == false)
            {
                System.IO.File.Create(strPathEventLogFile).Close();

                //3. Create File Header
                CreateFileHeader(strPathEventLogFile);
            }

            //4. Find out some Information of PC
            strMacAddress = MyLibrary.clsMyFunc.GetMACAddress();
            strIPAddress = MyLibrary.clsMyFunc.GetIPv4Address();
            strPCName = MyLibrary.clsMyFunc.GetPcName();
        }

        public void CreateFileHeader(string strFileFullName)
        {
            //Header Format
            //PC Name - IP Address - Mac Address - Event Name - Event Data - Event Note
            string strDataToWrite = "";
            strDataToWrite += "PC Name" + ",";
            strDataToWrite += "IP Add"  + ",";
            strDataToWrite += "Mac Add"  + ",";

            strDataToWrite += "Event Time" + ",";

            strDataToWrite += "Event Name"  + ",";
            strDataToWrite += "Event Data"  + ",";
            strDataToWrite += "Event Note"  + ",";

            AppendCsvFile(strPathEventLogFile, strDataToWrite);
        }

        public void AppendCsvFile(string strFileFullName, string strDataToAppend)
        {
            try
            {
                StreamWriter sWriter;
                sWriter = File.AppendText(strFileFullName);
                sWriter.WriteLine(strDataToAppend);
                sWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AppendCsvFile()");
            }
        }


        public void WriteLogData(string strEventName, string strEventData, string strEventNote)
        {
            try
            {
                //Check if need to create new file - new date arrival?

                DateTime _Now = DateTime.Now;
                strPathEventLogFile = "EventLog" + _Now.Year.ToString() + "_" + _Now.Month.ToString() + "_" + _Now.Day.ToString() + ".csv";
                strPathEventLogFile = strPathEventLogFolder + @"\" + strPathEventLogFile;

                if (MyLibrary.ChkExist.CheckFileExist(strPathEventLogFile) == false)
                {
                    System.IO.File.Create(strPathEventLogFile).Close();

                    //3. Create File Header
                    CreateFileHeader(strPathEventLogFile);
                }

                string strDataToWrite = "";
                strDataToWrite += strPCName + ",";
                strDataToWrite += strIPAddress + ",";
                strDataToWrite += strMacAddress + ",";

                strDataToWrite += DateTime.Now.ToString() + ",";


                strEventName = strEventName.Replace(",", " ");
                strDataToWrite += strEventName + ",";

                strEventData = strEventData.Replace(",", " ");
                strDataToWrite += strEventData + ",";

                strEventNote = strEventNote.Replace(",", " ");
                strDataToWrite += strEventNote + ",";


                AppendCsvFile(strPathEventLogFile, strDataToWrite);
            }
            catch (Exception ex)
            {
                //Do nothing
                string strEx = ex.Message;
            }
        }

    }
}
