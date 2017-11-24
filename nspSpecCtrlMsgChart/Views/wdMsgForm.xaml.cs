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

namespace nspSpecMsgChart.Views
{
    /// <summary>
    /// Interaction logic for wdMsgForm.xaml
    /// </summary>
    public partial class wdMsgForm : Window
    {
        public bool isClosed { get; set; }

        public wdMsgForm()
        {
            InitializeComponent();
            //
            this.Closed += wdMsgForm_Closed;
        }

        void wdMsgForm_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }
    }
}
