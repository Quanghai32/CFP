using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for MasterProcess.xaml
    /// </summary>
    [Export(typeof(MasterProcessView))]
    public partial class MasterProcessView : UserControl
    {

        public MasterProcessView()
        {
            InitializeComponent();
        }

        [Import(typeof(clsMasterProcessViewModel))]
        clsMasterProcessViewModel ViewModel
        {
            set 
            {
                //link business data to CollectionViewSource
                clsMasterProcessViewModel clsTemp = (clsMasterProcessViewModel)value;
                CollectionViewSource itemCollectionViewSource;
                itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
                itemCollectionViewSource.Source = clsTemp.GetTableView;

                //this.DataContext = value; 
            }
        }

        private void dataGridMaster_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true; //Cancel user sorting
        }  

    }
}
