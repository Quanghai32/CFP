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
using System.Windows.Shapes;

namespace nspMasterProcessModel.Views
{
    /// <summary>
    /// Interaction logic for wdMasterSequenceTest.xaml
    /// </summary>
    public partial class wdMasterSequenceTest : Window
    {
        public wdMasterSequenceTest(clsMasterProcessModel clsMasterModel)
        {
            InitializeComponent();

            //setting Data Context
            this.DataContext = new classMasterSequenceTester(clsMasterModel);
        }
    }
}
