using Redux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspAppStore
{
    //Redux Reducers
    public static class AppReducer
    {
        public static AppState Execute(AppState state, IAction action)
        {
            if (action is AppActions.SetSplashScreenMessage)
            {
                state.LoadingMessage = ((AppActions.SetSplashScreenMessage)action)._message;
            }
            else if (action is AppActions.CloseSplashScreen)
            {
                state.CloseSplashScreen = ((AppActions.CloseSplashScreen)action).closeSplash;
            }
            else if (action is AppActions.SetShellFrameInfo)
            {
                state.shellFrameInfo = ((AppActions.SetShellFrameInfo)action).info;
            }
            else if (action is AppActions.SavingHostWebServiceInstanceAction)
            {
                state.hostWebsite.objHostWebService = ((AppActions.SavingHostWebServiceInstanceAction)action)._objInstance;
            }
            else if(action is AppActions.ChangeCheckingModeAction)
            {
                state.CheckingMode = ((AppActions.ChangeCheckingModeAction)action)._CheckingMode;
            }
            else if (action is AppActions.ChangePmModeRequestAction)
            {
                state.PmModeRequestStop = ((AppActions.ChangePmModeRequestAction)action)._blRequestStop;
            }
            else if(action is AppActions.SetSideBarContent)
            {
                state.sideBarContent = ((AppActions.SetSideBarContent)action)._strContent;
            }
            else if (action is AppActions.SystemStart)
            {
                state.UserInterfaceControl.SystemStart = ((AppActions.SystemStart)action).start;
            }
            else if (action is AppActions.SelectShellTabItem)
            {
                state.UserInterfaceControl.SelectShellTabItem = ((AppActions.SelectShellTabItem)action)._tabName;
            }
            else if (action is AppActions.SelectOptionViewMode)
            {
                state.UserInterfaceControl.SelectOptionViewMode = ((AppActions.SelectOptionViewMode)action)._optionView;
            }
            else if (action is AppActions.ResetProgram)
            {
                state.UserInterfaceControl.ResetProgram = ((AppActions.ResetProgram)action)._ResetProgram;
            }
            else if (action is AppActions.ManualStartCheck)
            {
                state.UserInterfaceControl.ManualStartCheck = ((AppActions.ManualStartCheck)action)._manualStart;
            }
            else if (action is AppActions.CancelCheckingProcess)
            {
                state.UserInterfaceControl.CancelCheckingProcess = ((AppActions.CancelCheckingProcess)action)._cancel;
            }

            return state;
        }
    }
}
