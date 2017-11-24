using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;

namespace nspChildProcessModel
{
    /// <summary>
    /// Interaction logic for MainMenuView.xaml
    /// </summary>
    [Export(typeof(MainMenuView))]
    public partial class MainMenuView : UserControl
    {
        private clsChildControlViewModel objChildControlViewModel { get; set; }

        public MainMenuView()
        {
            InitializeComponent();
        }

        [Import(typeof(clsChildControlViewModel))]
        clsChildControlViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                this.objChildControlViewModel = value;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //View Program folder
            try
            {
                string strFolderPath = "";
                strFolderPath = System.AppDomain.CurrentDomain.BaseDirectory;
                System.Diagnostics.Process.Start(strFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "");
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string strTemp = "This program was made with purpose to create a common platform\r\n" +
                             "using for all checker program, no matter what kind of checker! \r\n" +
                             "I hope it will be useful for you & thank for using my program!\r\n" +
                             "If you have any question or comment feel free contact me:\r\n" +
                             "        - Author: Hoang Do Van \r\n" +
                             "        - Email: pde-adm11@canon-cvn.canon.co.jp \r\n" +
                             "        - Tel: (+84)904809771 \r\n" +
                             "        - PHS: 6609-1405 \r\n" +
                             "**************************************************************\r\n" +
                             "Hanoi, 2016";

            wdAbout wdMyAboutBox = new wdAbout();
            wdMyAboutBox.tblInfo.Text = strTemp;
            wdMyAboutBox.Show();
        }

        private void ViewCheckingProgramList(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select view Child Program list
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("ChildProgramList"));
        }

        private void ViewMasterProgramList(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select view Master Program list
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("MasterProgramList"));
        }

        private void ViewOriginStepList(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select view OriginStepList
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("OriginStepList"));
        }

        private void ViewData(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select view Checking Data
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("ViewData"));
        }

        private void ViewVersionInfo(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select ViewVersionInfo
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("ViewVersionInfo"));
        }

        private void ViewUserFunctionInfo(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select ViewUserFunctionInfo
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("ViewUserFunctionInfo"));
        }

        private void ViewUserExpressionInfo(object sender, RoutedEventArgs e)
        {
            // Set Shell (Main UI) goto tab item "OptionViewTabItem"
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectShellTabItem("OptionViewTabItem"));
            // Select ViewUserExpressionInfo
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SelectOptionViewMode("ViewUserExpressionInfo"));
        }

        private void EditSystemSetting(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get AppState
                var state = nspAppStore.clsAppStore.GetCurrentState();

                MessageBox.Show("Please Reset program after finish Edit System File!", "Edit System Setting Note");
                System.Diagnostics.Process.Start(state.GlobalSetting.strSystemIniPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ICommandEditSystemSettingHandle()");
            }
        }

        private void EditUserSetting(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get AppState
                var state = nspAppStore.clsAppStore.GetCurrentState();
                MessageBox.Show("Please Reset program after finish Edit User setting File!", "Edit User Setting Note");
                System.Diagnostics.Process.Start(state.GlobalSetting.strUserIniPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ICommandEditUserSettingHandle()");
            }
        }

        private void ResetProgram(object sender, RoutedEventArgs e)
        {
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ResetProgram(true));
        }
    }
}
