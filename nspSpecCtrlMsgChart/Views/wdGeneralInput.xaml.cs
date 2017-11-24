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
    /// Interaction logic for wdGeneralInput.xaml
    /// </summary>
    public partial class wdGeneralInput : Window
    {
        public bool isClosed { get; set; }
        public bool isMainWinDowClosed { get; set; }

        public string strTitle { get; set; }
        public string strRequestInfo { get; set; }
        public string strOption { get; set; }

        public string strUserInput { get; set; }
        public bool blFinishInput { get; set; }

        public wdGeneralInput()
        {
            InitializeComponent();

            //
            this.Closing += wdGeneralInput_Closing;
            this.Closed += wdGeneralInput_Closed;
            this.Loaded += wdGeneralInput_Loaded;
            this.Topmost = true;

            //
            this.strTitle = "Input Box";
            this.strRequestInfo = "requested Info";

            //
            this.tbInput.Focus();
            this.tbInput.LostFocus += tbInput_LostFocus;
            //
            this.blFinishInput = false;
        }


        void wdGeneralInput_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = this.strTitle;
            this.tbMessage.Text = this.strRequestInfo + ":";

            //Default - button
            this.btnOK.Visibility = System.Windows.Visibility.Hidden;
            this.btnCancel.Visibility = System.Windows.Visibility.Hidden;
            this.btnBackGround.Visibility = System.Windows.Visibility.Hidden;

            //User setting
            if (strOption.ToUpper() == "OK")
            {
                this.btnOK.Visibility = System.Windows.Visibility.Visible;
            }
            else if (strOption.ToUpper() == "CANCEL")
            {
                this.btnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else if (strOption.ToUpper() == "BACKGROUND")
            {
                this.btnBackGround.Visibility = System.Windows.Visibility.Visible;
            }
            else if (strOption.ToUpper() == "OKCANCEL") //Hide Background button
            {
                this.btnOK.Visibility = System.Windows.Visibility.Visible;
                this.btnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else if (strOption.ToUpper() == "OKBACKGROUND") //Hide Background button
            {
                this.btnOK.Visibility = System.Windows.Visibility.Visible;
                this.btnBackGround.Visibility = System.Windows.Visibility.Visible;
            }
            else if (strOption.ToUpper() == "OKCANCELBACKGROUND") //Hide Background button
            {
                this.btnOK.Visibility = System.Windows.Visibility.Visible;
                this.btnCancel.Visibility = System.Windows.Visibility.Visible;
                this.btnBackGround.Visibility = System.Windows.Visibility.Visible;
            }
            else //No button
            {
                ;
            }

        }

        void wdGeneralInput_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((this.blFinishInput == false) && (this.isMainWinDowClosed == false))
            {
                //MessageBox.Show("You should not close me if not yet finish input! :)","Close cancel!");
                this.tbMessage.Text = "Do not close me if not yet finish input!\r\n" + this.strRequestInfo;
                e.Cancel = true;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        void tbInput_LostFocus(object sender, RoutedEventArgs e)
        {
            //this.tbInput.Focus();
        }

        void wdGeneralInput_Closed(object sender, EventArgs e)
        {
            this.isClosed = true;
        }

        private void tbInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.strUserInput = this.tbInput.Text.Trim();
                this.blFinishInput = true;
                //Then, we close input form
                this.Dispatcher.Invoke(new Action(() => this.Close()));
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.strUserInput = this.tbInput.Text.Trim();
            this.blFinishInput = true;
            //Then, we close input form
            this.Dispatcher.Invoke(new Action(() => this.Close()));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.strUserInput = "cancel";
            this.blFinishInput = true;
            //Then, we close input form
            this.Dispatcher.Invoke(new Action(() => this.Close()));
        }

        private void btnBackGround_Click(object sender, RoutedEventArgs e)
        {
            //Change from "ShowDialog" to "Show" mode - not block Main Window - only working if this window on another thread!!!
            this.Topmost = false;
            this.Dispatcher.Invoke(new Action(() => this.Hide()));
            this.Dispatcher.Invoke(new Action(() => this.Show()));
        }

    }
}
