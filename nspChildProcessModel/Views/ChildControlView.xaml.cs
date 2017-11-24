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

namespace nspChildProcessModel
{
    /// <summary>
    /// Interaction logic for ChildControlView.xaml
    /// </summary>
    [Export(typeof(ChildControlView))]
    public partial class ChildControlView : UserControl
    {
        public ChildControlView()
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
                itemCollectionViewSource.Source = objViewModel.GetTableView;
            }
        }

        private void UpdateTableViewSource()
        {
            itemCollectionViewSource.Source = objViewModel.GetTableView;
        }


        private void dataGridMaster_Sorting(object sender, DataGridSortingEventArgs e)
        {
            UpdateTableViewSource();
            e.Handled = true;
        }
    }
}
