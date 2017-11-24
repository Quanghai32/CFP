using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace nspSpecMsgChart
{
    public class clsGeneralInput
    {
        public Views.wdGeneralInput wdInput { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void IniGeneralInput()
        {
            //Create new reading form in UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.wdInput = new Views.wdGeneralInput();
                this.wdInput.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }));
        }


        //Show window
        public void ShowDialog()
        {
            if (this.wdInput == null) return;

            if (this.wdInput.isClosed == false) //Not yet closed
            {
                this.wdInput.Dispatcher.Invoke(new Action(() =>
                {
                    //this.wdReadingForm.Show();
                    this.wdInput.ShowDialog();
                }));
            }
            else //Already closed - Create new window & show again
            {
                this.IniGeneralInput();
                //
                this.wdInput.Dispatcher.Invoke(new Action(() =>
                {
                    //this.wdReadingForm.Show();
                    this.wdInput.ShowDialog();
                }));
            }
        }
        
    }
}
