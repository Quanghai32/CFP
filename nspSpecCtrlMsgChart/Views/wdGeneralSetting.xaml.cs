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
    /// Interaction logic for wdGeneralSetting.xaml
    /// </summary>
    public partial class wdGeneralSetting : Window
    {
        public bool isClosed { get; set; }

        public wdGeneralSetting()
        {
            InitializeComponent();
            //
            this.Closed += wdGeneralSetting_Closed;
        }

        void wdGeneralSetting_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btnNew = new Button();
            btnNew.Content = "test";
            this.spnlContentSetting.Children.Add(btnNew);
        }

    }
}
