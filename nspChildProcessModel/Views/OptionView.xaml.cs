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

namespace nspChildProcessModel.Views
{
    /// <summary>
    /// Interaction logic for OptionView.xaml
    /// </summary>
    [Export(typeof(OptionView))]
    public partial class OptionView : UserControl
    {
        public OptionView()
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

        private void dataGridOptionView_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true; //Cancel user sorting
        }
    }
}
