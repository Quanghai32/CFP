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
using System.Windows.Threading;

namespace nspNECWireless.Views
{
    /// <summary>
    /// Interaction logic for wdNecCommand.xaml
    /// </summary>
    public partial class wdNecCommand : Window
    {
        public bool isClosed { get; set; }
        public DispatcherTimer myTimer { get; set; }

        public wdNecCommand()
        {
            InitializeComponent();
            //
            this.Closed += wdNecCommand_Closed;
            //
            this.myTimer = new DispatcherTimer();
            this.myTimer.Interval = TimeSpan.FromMilliseconds(1);
        }

        void wdNecCommand_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }
    }
}
