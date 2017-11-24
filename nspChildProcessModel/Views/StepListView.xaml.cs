using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace nspChildProcessModel.Views
{
    /// <summary>
    /// Interaction logic for StepListView.xaml
    /// </summary>
    /// 
    [Export(typeof(StepListView))]
    public partial class StepListView : UserControl
    {
        public StepListView()
        {
            InitializeComponent();
        }

        clsChildControlViewModel objViewModel;

        CollectionViewSource itemCollectionViewSource;

        [Import(typeof(clsChildControlViewModel))]
        clsChildControlViewModel ViewModel
        {
            set
            {
                //link business data to CollectionViewSource
                objViewModel = (clsChildControlViewModel)value;

                itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
                itemCollectionViewSource.Source = objViewModel.GetStepListTableView;
            }
        }

        private void UpdateTableViewSource()
        {
            itemCollectionViewSource.Source = objViewModel.GetStepListTableView;
        }

        private void dataGridMaster_Sorting(object sender, DataGridSortingEventArgs e)
        {
            UpdateTableViewSource();
            e.Handled = true;
        }
    }
}
