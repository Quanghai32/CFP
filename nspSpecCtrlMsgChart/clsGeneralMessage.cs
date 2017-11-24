using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace nspSpecMsgChart
{
    public class clsGeneralMessage
    {
        public Views.wdMsgForm wdMSG { get; set; }
        public string strMsgContent { get; set; }
        public string strMsgTitle { get; set; }
        public int intTimeOutMs { get; set; }
        public bool blisTimeOut { get; set; }
        public int intStartTick { get; set; } //marking time from Timer start

        //Timer support
        public System.Timers.Timer tmrCountDown { get; set; }

        //Ini
        public void IniGeneralMSG()
        {
            //Create new General Message Window in UI thread
            Application.Current.Dispatcher.Invoke(new Action(() => this.CreateNewWindow()));

            this.wdMSG.Dispatcher.Invoke(new Action(() => 
                {
                    this.wdMSG.Title = this.strMsgTitle;
                    this.wdMSG.tbMsgContent.Text = this.strMsgContent;
                }));
        }

        //
        public void CreateNewWindow() //this should be created in UI thread!!!
        {
            this.wdMSG = new Views.wdMsgForm();
        }

        //Show window
        public string Show()
        {
            if (this.wdMSG == null) return "Error: window message is null";

            if(this.wdMSG.isClosed==true)
            {
                this.IniGeneralMSG();
            }
            //Show message window
            //
            this.wdMSG.Dispatcher.Invoke(new Action(() =>
            {
                this.wdMSG.Show();
                //
                //this.StartCounDownTimer();
            }));


            //
            return "0";
        }

        //Hide window
        public string Hide()
        {
            if (this.wdMSG == null) return "Error: window message is null";
            this.wdMSG.Dispatcher.Invoke(new Action(() =>
            {
                this.wdMSG.Hide();
            }));
            //
            return "0";
        }

        //Close window
        public string Close()
        {
            if (this.wdMSG == null) return "Error: window message is null";

            this.wdMSG.Dispatcher.Invoke(new Action(() =>
            {
                this.wdMSG.Close();
            }));
            //
            return "0";
        }

        //Move window to new position
        public string Move(Point pointNewPos)
        {
            string strRet = "";
            //
            if (this.wdMSG == null) return "Move() Error: wdMSG is null";

            if (this.wdMSG.isClosed ==false)
            {
                this.wdMSG.Dispatcher.Invoke(new Action(() =>
                {
                    this.wdMSG.Left = pointNewPos.X;
                    this.wdMSG.Top = pointNewPos.Y;
                }));
                //
                strRet = "0";
            }
            else
            {
                strRet = "Move() Error: General Message Window is already closed!";
            }
           
            //
            return strRet;
        }

        //
        public string StartCounDownTimer(int intTimeOutMs)
        {
            this.intTimeOutMs = intTimeOutMs;
            //
            this.wdMSG.Dispatcher.Invoke(new Action(() =>
                {
                    this.tmrCountDown.Enabled = true;
                    this.tmrCountDown.Start();
                    this.intStartTick = MyLibrary.ApiDeclaration.GetTickCount();
                }));
            //
            return "0";
        }

        //
        void tmrCountDown_Tick(object sender, EventArgs e)
        {
            this.tmrCountDown.Enabled = false;
            //
            string strTime = "";
            double dblTimeRemain = 0;

            if(this.intTimeOutMs != -1)
            {
                int intCurrentTick = MyLibrary.ApiDeclaration.GetTickCount();
                int intTickPass = intCurrentTick - this.intStartTick;
                int intTimeRemainMs = this.intTimeOutMs - intTickPass;
                dblTimeRemain = (double)intTimeRemainMs / 1000;
                strTime = "Time remain (s): " + dblTimeRemain.ToString();
                if (dblTimeRemain < 0)
                {
                    strTime = (this.intTimeOutMs / 1000).ToString() + "s Time out reached!";
                    blisTimeOut = true;
                }
                else
                {
                    blisTimeOut = false;
                }
            }
            else
            {
                strTime = "Setting no time out. Waiting forever!";
            }
            
            

            string strNewContent = this.strMsgContent + "\r\n" +
                                    "******************************\r\n" +
                                    strTime;

            this.wdMSG.Dispatcher.Invoke(new Action(() =>
                {
                    this.wdMSG.tbMsgContent.Text = strNewContent;

                    if (this.intTimeOutMs != -1)
                    {
                        if (dblTimeRemain < 10)
                        {
                            this.wdMSG.tbMsgContent.Background = System.Windows.Media.Brushes.Red;
                        }
                        else if (dblTimeRemain < 20)
                        {
                            this.wdMSG.tbMsgContent.Background = System.Windows.Media.Brushes.Yellow;
                        }
                    }

                }));
           
            //
            this.tmrCountDown.Enabled = true;
        }

        public string isTimeOut()
        {
            string strRet = "0";
            if (blisTimeOut == true) strRet = "1";
            //
            return strRet;
        }

        //Constructor
        public clsGeneralMessage()
        {
            this.strMsgContent = "strMsgContent";
            this.strMsgTitle = "strMsgTitle";
            //
            this.tmrCountDown = new System.Timers.Timer();
            this.tmrCountDown.Interval = 50;
            this.tmrCountDown.Elapsed += tmrCountDown_Tick;
            //
            this.intTimeOutMs = 30000;
        }

    }
}
