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

namespace nspMasterProcessModel.Views
{
    /// <summary>
    /// Interaction logic for FlashingIndication.xaml
    /// </summary>
    /// 
    [Export(typeof(FlashingIndication))]
    public partial class FlashingIndication : UserControl
    {
        public FlashingIndication()
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

    }
}
