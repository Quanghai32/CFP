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
using System.Windows.Shapes;

namespace nspNECWireless
{
    /// <summary>
    /// Interaction logic for windowWirelessSetting.xaml
    /// </summary>
    public partial class windowWirelessSetting : Window
    {
        /////////////////////////////////////////////////////////////////
        public int intProcessID { get; set; }

        /////////////////////////////////////////////////////////////////
        /// <summary>
        /// constructor
        /// </summary>
        public windowWirelessSetting()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "NEC Wireless Setting - Process ID " + this.intProcessID.ToString(); 
        }
    }
}
