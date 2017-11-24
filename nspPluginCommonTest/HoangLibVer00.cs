//*****************************************************************************************************************************
//  C SHARP LIBRARY - HOANG DO VAN
//*****************************************************************************************************************************
//  Desc: for reuse created C# code. Save time for future projects.
//  Ver0.00: New Module. 2013/Jun/29.
//*****************************************************************************************************************************

//*****************************************************************************************************************************
//  FILE HANDLE NAMESPACE
//  Desc: For checking file or folder exist. Creating, copy new file, folder...
//*****************************************************************************************************************************

using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;

namespace nspApiDeclare
{
    class ApiDeclaration
    {
        //GetTickCount: return millisecond number
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        //Reading Ini file
        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section,string key, string def, StringBuilder retVal, int size, string filePath);
    }

    class clsApiFunc
    {
        public static int GetTickCount()
        {
            return ApiDeclaration.GetTickCount();
        }
    }

}

namespace nspFileHandle
{
    //Checking File or Folder exist or not
    class ChkExist
    {
        //*********************************************************************************************************
        //Check if 1 file exist or not
        public bool CheckFileExist(string strFileName)
        {
            return File.Exists(strFileName);
        }
        //*********************************************************************************************************
        //Check if 1 folder exist or not
        public bool CheckFolderExist(string strFolderName)
        {
            //return File.Exists(strFolderName);
            return Directory.Exists(strFolderName);
        }
        //*********************************************************************************************************

    }
    //Reading files
    class ReadFiles
    {
        //*********************************************************************************************************
        public static string IniReadValue(string Section, string Key, string strFileFullPath)
        {
            // If string greater than 254 characters (255th spot is null-terminator),
            // string will be truncated.
            nspFileHandle.ChkExist clsTest = new nspFileHandle.ChkExist();

            if (clsTest.CheckFileExist(strFileFullPath) == false)
            {
                MessageBox.Show(strFileFullPath + " file not exist!", "GetPrivateProfile() Error");
                return "error"; //Error Code
            }
            else
            {
                const int capacity = 1023;
                StringBuilder temp = new StringBuilder(capacity);
                int i = nspApiDeclare.ApiDeclaration.GetPrivateProfileString(Section, Key, "error", temp, capacity, strFileFullPath);
                return temp.ToString();
            }
        }
        //*********************************************************************************************************

    }

}

namespace nspNumericHandle
{
    class CheckNumber
    {
        public bool IsNumeric(string strTest)
        {
            double i = 0;
            if(double.TryParse(strTest, out i) == true)
            {
                return true; 
            }
            else
            {
                return false;
            }
        }
    }

}


//namespace nspGetWindowsInfo
//{
//    class display
//    {
//        /////////////////////////////////////////////
//        public int GetTaskBarHeight()
//        {
//            int TaskBarHeight = Screen.PrimaryScreen.Bounds.Bottom - Screen.PrimaryScreen.WorkingArea.Bottom;
//            return TaskBarHeight;
//        }
//        /////////////////////////////////////////////
//        public int GetUserScreenResolutionX()
//        {
//            return Screen.PrimaryScreen.Bounds.Width;
//        }
//        /////////////////////////////////////////////
//        public int GetUserScreenResolutionY()
//        {
//            return Screen.PrimaryScreen.Bounds.Height;
//        }
//        /////////////////////////////////////////////
//    }
//}


namespace nspStringHandle
{
     class StringHandle
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

        public static int FindCharInString(char chrToFind, string strTarget,ref List<int> lstPosition)
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
}














