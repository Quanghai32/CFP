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

namespace nspPmModeExpress.Views
{
    /// <summary>
    /// Interaction logic for wdPmMessage.xaml
    /// </summary>
    public partial class wdPmMessage : Window
    {
        public bool isClosed { get; set; }
        public wdPmMessage(object objDataContext)
        {
            InitializeComponent();
            //
            this.DataContext = objDataContext;
            this.Closed += WdPmMessage_Closed;
        }

        private void WdPmMessage_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }
    }
}
