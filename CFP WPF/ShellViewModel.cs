////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  CHECKER FRAME PROGRAM - CFP WPF
//      Ver1.2.2.3. Refactor HostWebsite connection code. Modify PM mode class. Hoang 23/Oct/2017.
//      Ver1.2.2.2. Replace Thread by Task Control. Hoang 17/Oct/2017.
//      Ver1.2.2.1. Update Nuget Package. Hoang 13/Oct/2017.
//      Ver1.2.2.0. Add SetCheckingMode() function. Hoang 13/Oct/2017.
//      Ver1.2.1.2. Fix FindStepListPLStepPos() function - bug with non-group mode. Hoang 10/Oct/2017.
//      Ver1.2.1.1. Restructure App using Reactive programming & Redux. Hoang 4/Oct/2017.
//      Ver1.2.1.0. Add some features & fix some bugs.
//      Ver1.2.0.0. Breaking Change. Delete, modify, Add some function. Hoang 31/Jul/2017.
//      Ver1.1.7.1. Remove System Watcher.
//                  Add WebAPI access & Offline service
//      Ver1.1.7.0. Add LiteDB as offline database for CFP (NoSQL database like MongoDB)
//      Ver1.1.6.2. Fix SettingPoint() of Chart Control
//      Ver1.1.6.1. Add AutoSetChart() after PointSetting() => apply change immediately
//      Ver1.1.6.0. Add PmMode Handle & Web API Secured Access
//      Ver1.1.5.3. Fix USB_DR() 100-0-13 function (lack of 2 data return)
//      Ver1.1.5.2. Fix Saving Pre-Info & After-Info to Steplist data. Access Secured Web API.
//      Ver1.1.5.1. Add function to Access secured web API.
//      Ver1.1.4.1. Fix Reading Excel with Formular => Thuan-san request
//      Ver1.1.4.0. Add function 1-0-8/1-0-9/1-0-21/11-0-102 => Thuan-san request
//      Ver1.1.3.6. Fix some bugs. 23/Jun/2017. Hoang.
//      Ver1.1.3.5. Fix error related to 1E-5 like data format in judge result pass or fail. Hoang 15/Jun/2017.
//      Ver1.1.3.4. Add minimize chart, normalize chart. Hoang 14/Jun/2017.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Regions;
using nspCFPInfrastructures;
using System.Reactive.Linq;
using System.Windows;

namespace CFP_WPF
{
    /// <summary>
    /// ShellViewModel class. Play "VIEW MODEL" role in MVVM.
    /// </summary>

    [Export(typeof(ShellViewModel))] //We export the View Model. So the ShellViewModel can be injected to the View by MEF
    public class ShellViewModel: BindableBase
    {
        public string strProgramVer = "1.2.2.3"; //SEMVER naming (Big Change[Total rewrite-Breaking change]-Major[Breaking change]-Minor[Add Features. No breaking change]-Patch[Fix bugs only])
    
        public ShellModel objShellModel;
        public Shell objShell;
        public ICommand TabControlSelected { get; set; }

        [Import(AllowRecomposition = false)]
        public IRegionManager RegionManager;

        //************************FOR MASTER PROCESS COMMUNICATION***********************************************************
        //Well, anyway, Child control process need to talking with master process control. We do it through shared service!
        //Note that we do it by "Lazy" type, to prevent crash when all part initialize at the same time!
        [Import(typeof(nspCFPInfrastructures.IMasterProCmdService))]
        Lazy<nspCFPInfrastructures.IMasterProCmdService> MasterProcessCommand;

        //Constructor - Do ini here!
        [ImportingConstructor]
        public ShellViewModel(ShellModel objShellModel)
        {
            this.objShellModel = objShellModel;
            // Splash Screen message
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SetSplashScreenMessage("Loading ShellViewModel..."));
            // Subscribe to start calculate info when splash screen close
            nspAppStore.clsAppStore.AppStore
                .DistinctUntilChanged(state => new { state.CloseSplashScreen })
				.Where(state => state.CloseSplashScreen)
                .Subscribe(state =>
                {
                    // MessageBox.Show("Hello!");
                    //Cal Info of Shell frame
                    var info = new nspAppStore.ShellFrameInfo();
                    info.Version = this.strProgramVer;
                    info.DateCreated = this.strProgramDateCreated();
                    info.ProtectionCode = this.CalProtectionCode();
                    info.FinishCalInfo = true;
                    // Change state
                    nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SetShellFrameInfo(info));
                });
        }

        //***********************************************************************
        public List<List<object>> lstlstobjCommand { get; set; }

        public string CalProtectionCode()
        {
            string strRet = "";
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            string strFilePath = fileinfo.FullName;

            //Calculate Protection ID code
            strRet = this.GetSingleFileInfo(strFilePath);

            //
            return strRet;
        }

        public string GetSingleFileInfo(string strFilePath)
        {
            string strRet = "";
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(strFilePath);
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            string strHash = new FileHasher.HashCalculator(strFilePath).CalculateFileHash();
            strRet = strHash;
            //
            return strRet;
        }


        //Initialization control - give command to Master Process to start running
        public void StartProcess()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<List<object>> lstlstobjReturn = new List<List<object>>();

            List<object> lstobjTemp = new List<object>();
            lstobjTemp.Add("START");
            lstlstobjCommand.Add(lstobjTemp);

            MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);

            //Do navigation?
        }

        //Shutdown proccedure
        public void ShutdownProcess()
        {
            List<List<object>> lstlstobjCommand = new List<List<object>>();
            List<List<object>> lstlstobjReturn = new List<List<object>>();

            List<object> lstobjTemp = new List<object>();
            lstobjTemp.Add("SHUTDOWN");
            lstlstobjCommand.Add(lstobjTemp);

            MasterProcessCommand.Value.MasterProcessCommand(lstlstobjCommand, out lstlstobjReturn);
        }

        private int _TabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get
            {
                return _TabControlSelectedIndex;
            }
            set
            {
                _TabControlSelectedIndex = value;

                //Base on item selected we set corresponding view 
                switch(_TabControlSelectedIndex)
                {
                    case 0: //"Main Info"
                        //Navigate to Child control panel on Top bar region
                        this.RegionManager.RequestNavigate("TopBarRegion", new Uri("ChildControlPanelView", UriKind.Relative));
                        break;

                    case 1: //"Master View"
                        //Navigate to Master control panel on Top bar region
                        this.RegionManager.RequestNavigate("TopBarRegion", new Uri("MasterControlPanelView", UriKind.Relative));
                        break;

                    case 2: //"Process View"
                        //Navigate to Child control panel on Top bar region
                        this.RegionManager.RequestNavigate("TopBarRegion", new Uri("ChildControlPanelView", UriKind.Relative));
                        break;

                    case 3: //"Step List View"
                        this.RegionManager.RequestNavigate("TopBarRegion", new Uri("StepListControlPanelView", UriKind.Relative));
                        break;

                    default:
                        //Navigate to Child control panel on Top bar region
                        this.RegionManager.RequestNavigate("TopBarRegion", new Uri("ChildControlPanelView", UriKind.Relative));
                        break;
                }
            }
        }

        //
        private string _GetWindowTitle { get; set; }
        public string GetWindowTitle 
        {
            get
            {
                _GetWindowTitle = "CFP - Version " + this.strProgramVer + " - Date Created " + strProgramDateCreated();
                return _GetWindowTitle;
            }
            set
            {
                _GetWindowTitle = value;
            }
        }

        //**********************************************************
        public string strProgramDateCreated()
        {
            return GetDateTimeLastModify().ToString();
        }

        //**********************************************************
        public DateTime GetDateTimeLastModify()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(assembly.Location);
            return fileinfo.LastWriteTime;
        }
        //**********************************************************

    }
}
