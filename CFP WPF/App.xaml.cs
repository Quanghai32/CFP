
using nspAppStore;
using Redux;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace CFP_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// This function is the "entry point" of all applicaton. When we override it, we can add our desired activities to control application start-up sequence
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //Testing area


            //******************INI REDUX APP STORE***********************************************************************
            clsAppStore.IniStore(); //Create new App Store

            //******************INI FOR BACKUP OFFLINE DATABASE***********************************
            var myDatabase = new LiteDBDatabase.LiteDBHandle();
            myDatabase.IniDatabase();

            //******************************************************************************************************
            //Showing Splash
            clsSplash clsNewSplash = new clsSplash();
            clsNewSplash.thrSplash = new System.Threading.Thread(clsNewSplash.ShowSplash);
            clsNewSplash.thrSplash.SetApartmentState(System.Threading.ApartmentState.STA);
            clsNewSplash.thrSplash.Start();

            // CFP Ini Loading component
            LoadingComponent.IniLoadComponent iniLoading = new LoadingComponent.IniLoadComponent();
            iniLoading.IniComponent();

            //Prevent 2 instances of program running together
            string strThisProcess = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == strThisProcess) > 1)
            {
                //If there is an instance already running, we will try to wait 4 second for that instance terminated. 
                //After that, if that instance still running, push message out and terminated

                int intStartTick = MyLibrary.clsApiFunc.GetTickCount();
                while ((MyLibrary.clsApiFunc.GetTickCount() - intStartTick) < 4000)
                {
                    if (Process.GetProcesses().Count(p => p.ProcessName == strThisProcess) <= 1) break;
                }

                if (Process.GetProcesses().Count(p => p.ProcessName == strThisProcess) > 1)
                {
                    //MessageBox.Show("Warning: This program already opened! Please check!", "Multi-Instance of program opened Error");
                    var testError = new wdError();
                    testError.ShowDialog();
                    //clsEventLog.WriteLogData("Start Failed", "Another Instance Exist", "");
                    Environment.Exit(0);
                    return;
                }
            }

            //**************************************************************************************************************************************************
            //Change application startup to Boots Trapper
            clsCFPBootstrapper clsBootstrapper = new clsCFPBootstrapper();
            try
            {
                clsBootstrapper.Run();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unexpected Error happen. Error message: " + ex.Message, "CFP clsBootstrapper.Run() fail!");
                Environment.Exit(0);
            }
        }
    }
}