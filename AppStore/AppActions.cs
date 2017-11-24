using Redux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspAppStore
{
    public static class AppActions
    {
        //**********************************Redux Actions****************************************
        // Splash Screen
        public class SetSplashScreenMessage : IAction
        {
            public string _message { get; private set; }
            public SetSplashScreenMessage(string message)
            {
                this._message = message;
            }
        }

        public class CloseSplashScreen : IAction
        {
            public bool closeSplash { get; private set; }
            public CloseSplashScreen(bool _closeSplash)
            {
                this.closeSplash = _closeSplash;
            }
        }

        // Shell Frame Info
        public class SetShellFrameInfo : IAction
        {
            public ShellFrameInfo info { get; private set; }
            public SetShellFrameInfo(ShellFrameInfo _Info)
            {
                this.info = _Info;
            }
        }


        //Saving Instance Object of HostWebService => All app should use only 1 common instance for saving memory
        public class SavingHostWebServiceInstanceAction : IAction
        {
            public object _objInstance { get; private set; }
            public SavingHostWebServiceInstanceAction(object objInstance)
            {
                this._objInstance = objInstance;
            }
        }


        //Change Host Web App connection status
        public class ChangeHostWebConnectStatusAction : IAction
        {
            public bool _isConnect { get; private set; }
            public ChangeHostWebConnectStatusAction(bool isConnect)
            {
                this._isConnect = isConnect;
            }
        }

        //Change checking Mode - from user action on Main UI
        public class ChangeCheckingModeAction : IAction
        {
            public string _CheckingMode { get; private set; }
            public ChangeCheckingModeAction(string CheckingMode)
            {
                this._CheckingMode = CheckingMode;
            }
        }

        //Change PM Mode request stop => prevent checking if PM mode timing control: timeout!
        public class ChangePmModeRequestAction : IAction
        {
            //nspAppStore.clsAppStore.AppStore.Dispatch(new ChangePmModeRequestAction(true));
            public bool _blRequestStop { get; private set; }
            public ChangePmModeRequestAction(bool blRequestStop)
            {
                this._blRequestStop = blRequestStop;
            }
        }

        //For SideBar display
        public class SetSideBarContent : IAction
        {
            public string _strContent { get; private set; }
            public SetSideBarContent(string strContent)
            {
                this._strContent = strContent;
            }
        }

        //User Interface Control
        public class SystemStart : IAction // After main window loaded
        {
            public bool start { get; private set; }
            public SystemStart(bool _start)
            {
                this.start = _start;
            }
        }

        public class SelectShellTabItem : IAction
        {
            public string _tabName { get; private set; }
            public SelectShellTabItem(string tabName)
            {
                this._tabName = tabName;
            }
        }

        public class SelectOptionViewMode : IAction
        {
            public string _optionView { get; private set; }
            public SelectOptionViewMode(string optionView)
            {
                this._optionView = optionView;
            }
        }

        public class ResetProgram : IAction
        {
            public bool _ResetProgram { get; private set; }
            public ResetProgram(bool ResetProgram)
            {
                this._ResetProgram = ResetProgram;
            }
        }

        public class ManualStartCheck : IAction
        {
            public bool _manualStart { get; private set; }
            public ManualStartCheck(bool manualStart)
            {
                this._manualStart = manualStart;
            }
        }

        public class CancelCheckingProcess : IAction
        {
            public bool _cancel { get; private set; }
            public CancelCheckingProcess(bool cancel)
            {
                this._cancel = cancel;
            }
        }

    }
}
