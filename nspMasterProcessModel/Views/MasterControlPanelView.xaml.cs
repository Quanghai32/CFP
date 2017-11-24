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
using nspCFPInfrastructures;
using System.Data;
using Microsoft.Practices.Prism.PubSubEvents;


namespace nspMasterProcessModel
{
    /// <summary>
    /// Interaction logic for MasterControlPanelView.xaml
    /// </summary>
    [Export(typeof(MasterControlPanelView))]
    public partial class MasterControlPanelView : UserControl
    {
        public MasterControlPanelView()
        {
            InitializeComponent();
        }

        [Import(typeof(clsMasterProcessViewModel))]
        clsMasterProcessViewModel ViewModel
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
            if ((intTemp > -1) && (intTemp < this.cbCheckMode.Items.Count))
            {
                this.cbCheckMode.Text = this.cbCheckMode.Items[intTemp].ToString();
            }
            
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) // Manual Start
        {
            nspAppStore.clsAppStore.AppStore
                .Dispatch(new nspAppStore.AppActions.ManualStartCheck(!nspAppStore.clsAppStore.GetCurrentState().UserInterfaceControl.ManualStartCheck));
        }
    }
}
