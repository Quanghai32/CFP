using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CFP_WPF
{
    public class clsSplash
    {
        //
        public InsightSplash.SplashWindow wdSplash { get; set; }
        public System.Threading.Thread thrSplash { get; set; }

        public bool blRequestClose { get; set; }
        public void ShowSplash()
        {
            //Create new chart window
            //Safety for dispatcher - create our context & install it
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            this.wdSplash = new InsightSplash.SplashWindow();

            //We need prevent dispatcher still alive if window is closed => out of memory!
            this.wdSplash.Closed += (s, e) => this.wdSplash.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

            this.wdSplash.Show();

            //Run dispatcher for window - keep form is responsive
            System.Windows.Threading.Dispatcher.Run();
        }

        public void CloseSplash()
        {
            this.wdSplash.Dispatcher.Invoke(new Action(() =>
            {
                this.wdSplash.Close();
            }));
        }


        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        //constructor
        public clsSplash()
        {
            //this.wdSplash = new InsightSplash.SplashWindow();
        }

    }
}
