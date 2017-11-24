////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Well, It's time to move to better platform: WPF with PRISM framework...
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Modularity;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Reactive.Linq;

namespace CFP_WPF
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    /// 
    [Export(typeof(Shell))] //We need Export Shell here, so in Bootstrapper, it can import Shell => return this.Container.GetExportedValue<Shell>(); ...
    public partial class Shell : Window, IPartImportsSatisfiedNotification
    {
        [Import(AllowRecomposition = false)]
        public IModuleManager ModuleManager;

        [Import(AllowRecomposition = false)]
        public IRegionManager RegionManager;

        public ShellViewModel objShellViewModel { get; set; }

        //Timer
        public DispatcherTimer tmrMainWindow { get; set; }

        //Pending Command list
        List<List<object>> lstlstobjPendingCommand { get; set; }

        public bool blFlagResetProgram { get; set; }


        [ImportingConstructor]
        public Shell(ShellViewModel objShellViewModel)
        {
            InitializeComponent();
            //Set data context
            this.objShellViewModel = objShellViewModel;
            this.DataContext = objShellViewModel;
            this.objShellViewModel.objShell = this;

            //
            this.lstlstobjPendingCommand = new List<List<object>>();
            List<object> lstobjTemp = new List<object>();
            object objTemp = new object();
            lstobjTemp.Add(objTemp);
            this.lstlstobjPendingCommand.Add(lstobjTemp);

            // Subscribe to AppStore
            // Select Tab item handle
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.UserInterfaceControl.SelectShellTabItem })
                .Subscribe(
                    state => this.SelectTabItem(state.UserInterfaceControl.SelectShellTabItem)
                );
            // Reset program handle
            nspAppStore.clsAppStore.AppStore.DistinctUntilChanged(state => new { state.UserInterfaceControl.ResetProgram })
                .Where(state => state.UserInterfaceControl.ResetProgram == true)
                .Subscribe(
                    state =>
                    {
                        //Reset program
                        this.blFlagResetProgram = true;
                        //
                        Application.Current.Exit += delegate (object sender, ExitEventArgs e)
                        {
                            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        };
                        Application.Current.Shutdown();
                    }
                );
        }

        public void SelectTabItem(string tabName)
        {
            switch(tabName)
            {
                case "OptionViewTabItem":
                    this.OptionViewTabItem.RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent));
                    break;
                default:
                    break;
            }
        }

        public void OnImportsSatisfied()
        {
            this.ModuleManager.LoadModuleCompleted += (s, e) =>
               {
                   ; //Do nothing
               };
        }

        public object objReturn { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Give command for master process start
            this.objShellViewModel.StartProcess();
            //
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SystemStart(true));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.WindowClosehandle();
        }

        private void WindowClosehandle()
        {
            //Give command for Master Process to start shut down procedure
            this.objShellViewModel.ShutdownProcess();

            //Finally, force all process terminate? - unless user choose reset program?
            if (this.blFlagResetProgram == false)
            {
                Environment.Exit(0); //Force all process to terminate ?
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            //this.btn1.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            this.MainInfoTabItem.RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent));
        }
    }
}
