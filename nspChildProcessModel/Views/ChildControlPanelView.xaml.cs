using System;
using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace nspChildProcessModel
{
    /// <summary>
    /// Interaction logic for ChildControlPanelView.xaml
    /// </summary>
    [Export(typeof(ChildControlPanelView))]
    public partial class ChildControlPanelView : UserControl
    {
        public ChildControlPanelView()
        {
            InitializeComponent();
        }

        [Import(typeof(clsChildControlViewModel))]
        clsChildControlViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        private void cbCheckMode_DropDownClosed(object sender, EventArgs e)
        {
            int intTemp = this.cbCheckMode.SelectedIndex;
            this.cbCheckMode.SelectedIndex = -1;
            //
            if ((intTemp > -1)&&(intTemp <this.cbCheckMode.Items.Count))
            {
                this.cbCheckMode.Text = this.cbCheckMode.Items[intTemp].ToString();
            }
            

        }

        private void btnStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nspAppStore.clsAppStore.AppStore
                .Dispatch(new nspAppStore.AppActions.ManualStartCheck(!nspAppStore.clsAppStore.GetCurrentState().UserInterfaceControl.ManualStartCheck));
        }

        private void btnEnd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nspAppStore.clsAppStore.AppStore.Dispatch(
                    new nspAppStore.AppActions.CancelCheckingProcess(!nspAppStore.clsAppStore.GetCurrentState().UserInterfaceControl.CancelCheckingProcess)
                );
        }
    }
}
