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

namespace nspKeyenceEthernet
{
    /// <summary>
    /// Interaction logic for windowPLCSetting.xaml
    /// </summary>
    public partial class windowPLCSetting : Window
    {
        public int intProcessID { get; set; }

        public windowPLCSetting()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "PLC Setting - Process ID " + this.intProcessID.ToString();
        }
    }
}
