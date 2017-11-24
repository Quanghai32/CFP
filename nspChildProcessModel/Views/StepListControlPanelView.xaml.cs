using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace nspChildProcessModel.Views
{
    /// <summary>
    /// Interaction logic for StepListControlPanelView.xaml
    /// </summary>
    [Export(typeof(StepListControlPanelView))]
    public partial class StepListControlPanelView : UserControl
    {
        public StepListControlPanelView()
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
    }
}
