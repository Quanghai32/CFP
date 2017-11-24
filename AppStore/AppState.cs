using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLibrary;
using System.Windows;

namespace nspAppStore
{
    /// <summary>
    /// Combine all state of application to only 1 store. Follow Redux paradims
    /// </summary>
    public class AppState
    {
        public string LoadingMessage { get; set; } // Message show on splash screen
        public bool CloseSplashScreen { get; set; } //Request close Splash Screen
        public clsGlobalSetting GlobalSetting { get; set; } // Hold all setting of App

        public clsGlobalInfo GlobalInfo { get; set; } // Hold all global info of system (counter, passrate...)

        public ShellFrameInfo shellFrameInfo { get; set; }
        public MasterProcessInformation MasterProcessInfo { get; set; }

        public HostWebsite hostWebsite { get; set; } //Hold all setting from user

        public string CheckingMode { get; set; } //Checking Mode of CFP: Normal - Service - PmMode...
        public bool PmModeRequestStop { get; set; } // In PM Mode, control if time out reaching

        //For Sidebar display
        public string sideBarContent { get; set; } //Content of side bar display

        //For UI Interaction
        public UIControl UserInterfaceControl { get; set; }

        // For Master Process info


        public AppState()
        {
            this.LoadingMessage = "Ini AppState...";
            this.GlobalSetting = new clsGlobalSetting();
            this.GlobalInfo = new clsGlobalInfo();
            this.shellFrameInfo = new ShellFrameInfo();
            this.MasterProcessInfo = new MasterProcessInformation();
            this.hostWebsite = new HostWebsite();
            //
            this.CheckingMode = "";
            this.PmModeRequestStop = false;
            //
            this.UserInterfaceControl = new UIControl();
        }
    }


    public class ShellFrameInfo
    {
        //Info 
        public string Version { get; set; } // Version of program
        public string DateCreated { get; set; } // Date create program
        public string ProtectionCode { get; set; } // Protection Code
        public bool FinishCalInfo { get; set; } // Finish calculation info of Shell Frame (protection code...) when start-up
    }

    /// <summary>
    /// Hold all information about master process
    /// </summary>
    public class MasterProcessInformation
    {
        public object objMasterProcess { get; set; } // Hold instance of Master Process itself
        public string Version { get; set; }
        public string DateCreated { get; set; } // Date create program
        public string ProtectionCode { get; set; } // Protection Code
    }


    public class UIControl
    {
        public bool SystemStart { get; set; } //After main window load => start system

        public string SelectShellTabItem { get; set; } // User press on Tab Item on Shell frame
        public string SelectOptionViewMode { get; set; } //View Child Program List - Master Program List...
        public bool ResetProgram { get; set; } // User request reset program

        public bool ManualStartCheck { get; set; } // User request manual start checking
        public bool CancelCheckingProcess { get; set; } // User click "STOP" button & want to stop checking process

        public UIControl()
        {
            this.ManualStartCheck = false;
        }

    }

    /// <summary>
    /// For holding all Sytem setting & User setting
    /// </summary>
    public class clsGlobalSetting
    {
        //StartUp Path
        public string strStartUpPath { get; set; }
        public string strSystemIniPath { get; set; }
        public string strUserIniPath { get; set; }

        //For user select mode
        public List<string> lststrSelectCheckingMode { get; set; }
        public string strSaveUserSelectMode { get; set; }

        // For Host Website service
        public bool UsingHostWebsite { get; set; }
        public string HostWebsiteAddress { get; set; }
        public string HostWebsiteUserName { get; set; }
        public string HostWebsitePassWord { get; set; }

        /// <summary>
        /// Reading System ini file
        /// </summary>
        private void ReadingIniSettingFile()
        {
            // MessageBox.Show("Hello! clsGlobalSetting");
            //1. Check if file is exist or not
            string strAppPath = "";
            string iniSystemFileName = "SystemIni.ini";
            string iniUserFileName = "UserIni.ini";
            string strSystemIniFilePath = "";
            string strTemp = "";
            int intTemp = 0;
            bool blTemp = false;

            strAppPath = System.AppDomain.CurrentDomain.BaseDirectory;
            strSystemIniFilePath = strAppPath + iniSystemFileName;

            this.strStartUpPath = strAppPath;
            this.strSystemIniPath = strSystemIniFilePath;
            this.strUserIniPath = strAppPath + iniUserFileName;

            //User Select checking Mode
            this.lststrSelectCheckingMode = new List<string>();
            this.lststrSelectCheckingMode.Add("NormalMode"); //Default
            this.lststrSelectCheckingMode.Add("ServiceMode"); //Default
            this.lststrSelectCheckingMode.Add("PmMode"); //Default

            strTemp = ReadFiles.IniReadValue("DISPLAY_SETTING", "UserSelectMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'UserSelectMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                //Environment.Exit(0);
            }
            else
            {
                var tmpArr = strTemp.Split(',');

                for (intTemp = 0; intTemp < (tmpArr.GetUpperBound(0) + 1); intTemp++)
                {
                    strTemp = tmpArr[intTemp].Trim();
                    this.lststrSelectCheckingMode.Add(strTemp);
                }
            }

            //Reading user save select mode
            strTemp = ReadFiles.IniReadValue("DISPLAY_SETTING", "SaveUserSelectMode", strSystemIniFilePath);
            if (strTemp.ToLower() == "error")
            {
                MessageBox.Show("Error: cannot find 'SaveUserSelectMode' config in 'DISPLAY_SETTING' of System.ini file!", "ReadSystemIniFile()");
                //Environment.Exit(0);
                this.strSaveUserSelectMode = this.lststrSelectCheckingMode[0]; //Default is normal mode
            }
            else
            {
                //Looking for all supported checking mode
                bool blFound = false;
                for (int i = 0; i < this.lststrSelectCheckingMode.Count; i++)
                {
                    if (this.lststrSelectCheckingMode[i] == strTemp)
                    {
                        blFound = true;
                        this.strSaveUserSelectMode = this.lststrSelectCheckingMode[i];
                        break;
                    }
                }
                if (blFound == false)
                {
                    this.strSaveUserSelectMode = this.lststrSelectCheckingMode[0];
                }
            }

            //*******************8Reading Host Website Config
            //Using or not - Default is using
            strTemp = ReadFiles.IniReadValue("HOST_WEBSITE", "UsingHostWebsite", strSystemIniFilePath); //true or false
            if (strTemp.ToLower() != "error")
            {
                if (bool.TryParse(strTemp, out blTemp) == true)
                {
                    this.UsingHostWebsite = blTemp;
                }
            }
            //Host Website Address
            strTemp = ReadFiles.IniReadValue("HOST_WEBSITE", "HostWebsiteAddress", strSystemIniFilePath);
            if (strTemp.ToLower() != "error")
            {
                this.HostWebsiteAddress = strTemp;
            }
            // Host Website UserName
            strTemp = ReadFiles.IniReadValue("HOST_WEBSITE", "UserName", strSystemIniFilePath);
            if (strTemp.ToLower() != "error")
            {
                this.HostWebsiteUserName = strTemp;
            }
            // Host Website PassWord
            strTemp = ReadFiles.IniReadValue("HOST_WEBSITE", "PassWord", strSystemIniFilePath);
            if (strTemp.ToLower() != "error")
            {
                this.HostWebsitePassWord = strTemp;
            }

        }
        
        // Constructor
        public clsGlobalSetting()
        {
            this.ReadingIniSettingFile();
        }
    }

    public class clsGlobalInfo
    {
        //For Protection Code
        public string TotalProtectCode { get; set; } // Protection Code contain all info: Source Code + Program List
        public string SourceProtectCode { get; set; } // Protection Code of Source Code

        //For Total result checking
        public int intTotal_CheckCount { get; set; } //How many object had been checked
        public int intTotal_CheckPass { get; set; } //How many Object had "PASS" checking resulted
        public double dblTotal_PassRate { get; set; } //Pass rate of checking process
    }


    /// <summary>
    /// Hold all setting from user
    /// </summary>
    public class HostWebsite
    {
        public object objHostWebService { get; set; } //Saving instance of HostWebService for sharing through all app
    }
}
