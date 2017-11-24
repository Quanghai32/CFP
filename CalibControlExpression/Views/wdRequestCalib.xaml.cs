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

namespace CalibControlExpression.Views
{
    /// <summary>
    /// Interaction logic for wdRequestCalib.xaml
    /// </summary>
    public partial class wdRequestCalib : Window
    {
        public bool isClosed { get; set; }

        public wdRequestCalib(object objDataContextInput)
        {
            InitializeComponent();
            //
            this.Closed += WdRequestCalib_Closed;
            this.DataContext = objDataContextInput;
        }

        private void WdRequestCalib_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }
    }
}
