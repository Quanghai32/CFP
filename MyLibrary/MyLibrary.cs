using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MyLibrary
{
    public class ApiDeclaration
    {
        //GetTickCount: return millisecond number
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        //Reading Ini file
        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //Write Ini file
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

    }

    public class clsApiFunc
    {
        public static int GetTickCount()
        {
            return ApiDeclaration.GetTickCount();
        }
    }

    //Checking File or Folder exist or not
    public class ChkExist
    {
        //*********************************************************************************************************
        //Check if 1 file exist or not
        public static bool CheckFileExist(string strFileName)
        {
            return File.Exists(strFileName);
        }
        //*********************************************************************************************************
        //Check if 1 folder exist or not
        public static bool CheckFolderExist(string strFolderName)
        {
            //return File.Exists(strFolderName);
            return Directory.Exists(strFolderName);
        }
        //*********************************************************************************************************

    }
    //Reading files
    public class ReadFiles
    {
        //*********************************************************************************************************
        public static string IniReadValue(string Section, string Key, string strFileFullPath)
        {
            // If string greater than 254 characters (255th spot is null-terminator),
            // string will be truncated.
            if (ChkExist.CheckFileExist(strFileFullPath) == false)
            {
                //System.Windows.Forms.MessageBox.Show(strFileFullPath + " file not exist!", "GetPrivateProfile() Error");
                return "error"; //Error Code
            }
            else
            {
                const int capacity = 1023;
                StringBuilder temp = new StringBuilder(capacity);
                int i = ApiDeclaration.GetPrivateProfileString(Section, Key, "error", temp, capacity, strFileFullPath);
                return temp.ToString();
            }
        }
        //*********************************************************************************************************

    }

    public class WriteFiles
    {
        //Write to ini file
        public static long IniWriteValue(string strFileFullPath, string strSectionName, string strItemName, string strDataToWrite)
        {
            // If string greater than 254 characters (255th spot is null-terminator),
            // string will be truncated.
            if (ChkExist.CheckFileExist(strFileFullPath) == false)
            {
                //System.Windows.Forms.MessageBox.Show(strFileFullPath + " file not exist!", "GetPrivateProfile() Error");
                return 9999; //Error Code
            }
            else
            {
                long lngret = 0;
                lngret = ApiDeclaration.WritePrivateProfileString(strSectionName, strItemName, strDataToWrite, strFileFullPath);
                return lngret;
            }
        }
    }//End WriteFiles class

    public class CheckNumber
    {
        public bool IsNumeric(string strTest)
        {
            double i = 0;
            if (double.TryParse(strTest, out i) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class StringHandle
    {
        /// <summary>
        /// This Function, Find how many occerences of 1 character in a string
        /// </summary>
        /// <param name="chrToFind"></param>
        /// <param name="strTarget"></param>
        /// <returns></returns>
        public static int FindNumCharInString(char chrToFind, string strTarget)
        {
            int count = 0;
            for (int i = 0; i < strTarget.Length; i++) if (strTarget[i] == chrToFind) count++;
            return count;
        }

        public static int FindCharInString(char chrToFind, string strTarget, ref List<int> lstPosition)
        {
            int count = 0;
            lstPosition = new List<int>();
            for (int i = 0; i < strTarget.Length; i++)
            {
                if (strTarget[i] == chrToFind)
                {
                    count++;
                    lstPosition.Add(i);
                }
            }
            return count;
        }
    }

    public class clsMyFunc
    {
        public static string GetPcName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
        }

        public static string GetMACAddress()
        {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces() where nic.OperationalStatus == OperationalStatus.Up select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            return macAddr.ToString();
        }

        public static string GetIPv4Address()
        {
            try
            {
                string strRet = string.Empty;
                string strHostName = System.Net.Dns.GetHostName();
                System.Net.IPHostEntry iphe = System.Net.Dns.GetHostEntry(strHostName);

                foreach (System.Net.IPAddress ipheal in iphe.AddressList)
                {
                    if (ipheal.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        strRet = ipheal.ToString();
                    }
                }
                return strRet;
            }
            catch
            {
                return "error";
            }
        }


        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            form.TopMost = true;
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            //textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 15, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(System.Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

    }

}
