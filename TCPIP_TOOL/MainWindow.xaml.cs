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
using System.Windows.Threading;

namespace TCPIP_TOOL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        clsTcpIpHandle clsMyTCP { get; set; }
        DispatcherTimer MyTimer { get; set; }
        public int intSentTimes { get; set; }
        public int intSentOK { get; set; }

        //
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string strRet = "";
            //
            this.clsMyTCP = new clsTcpIpHandle();
            strRet = this.clsMyTCP.SocketIniFileReading();
            strRet = this.clsMyTCP.SocketServerStart();
            //
            if(strRet!="0")
            {
                MessageBox.Show("Error: TCPIP server start fail. Error message: " + strRet,"TCP/IP Server Start Fail");
            }

            //
            this.MyTimer = new DispatcherTimer();
            this.MyTimer.Interval = TimeSpan.FromMilliseconds(10);
            this.MyTimer.Tick += MyTimer_Tick;
        }

        void MyTimer_Tick(object sender, EventArgs e)
        {
            this.MyTimer.Stop();
            //
            SendCommand();
            //
            this.MyTimer.Start();
        }

        //
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        private void SendCommand()
        {
            string strReceive = "";
            string strError = "";
            int intRet = 0;

            intRet = this.clsMyTCP.SocketServerWR(this.clsMyTCP.strTargetIPAddress, this.clsMyTCP.intTargetPort, this.tbSend.Text.Trim(), ref strReceive, ref strError);

            if(this.intSentTimes>1000000000)
            {
                this.intSentTimes = 0;
                this.intSentOK = 0;
            }
            this.intSentTimes++;


            if (intRet == 0) //Sending OK
            {
                this.tbReceive.Text = strReceive;
                this.intSentOK++;
            }
            else //Sending NG
            {
                this.tbReceive.Text = strError;
            }

            //Display Message
            this.tbMessage.Text = "Sent: " + this.intSentTimes.ToString() + ". OK: " + this.intSentOK.ToString() +
                ". Pass rate: " + Math.Round(100*(double)this.intSentOK/(double)this.intSentTimes,2) + "%";
        }

        private bool blToggle = false;
        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            blToggle = !(blToggle);
            if(blToggle==true)
            {
                this.MyTimer.IsEnabled = true;
                this.MyTimer.Start();
            }
            else
            {
                this.MyTimer.IsEnabled = false;
                this.MyTimer.Stop();
            }
        }
    }
}
