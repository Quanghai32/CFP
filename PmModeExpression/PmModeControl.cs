using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyLibrary;
using System.Reactive.Concurrency;
using nspAppStore;

namespace nspPmModeExpress
{
    public class PmCheckSetting
    {
        public bool blTimingControl { get; set; } //TimingControl = 1 (Default): control PM Check timing. TimingControl = 0: No control
        public DateTime dtLastTimeCheck { get; set; } //Last timing do PM check
        public int intTimingControlMode { get; set; }   //TimingControlMode = 0: Daily Check.Ex:need to be done at 8h00, 12h00, 21h00, 0h00 daily
                                                        //TimingControlMode = 1: Timing Check.Ex: Do PM Check after fix amount of time: after 5 days need to calib again...
                                                        //TimingControlMode = 2: Only Start-Up Check.Only required do PM check when start-up program.
        //For Daily Timing control Mode
        public int intDailyTimePointNum { get; set; }
        public List<TimeSpan> lsttsDailyTimePoint { get; set; } //List of Timing Point need to do PM Check
        //For Fix Amount control mode & Daily Timing control Mode
        public double dblTimingAmount { get; set; }
        //PM Check valid timing setting
        public double dblValidCheckTiming { get; set; }
        //Warning time setting
        public double dblWarningTime { get; set; }

        public PmCheckSetting()
        {
            this.blTimingControl = true; //Default is control timing of PM Check
            this.lsttsDailyTimePoint = new List<TimeSpan>();
        }
    }

    public class PmCheckResult
    {
        public bool blResult { get; set; } //Result of PM check process is OK or NG
    }

    public class PmModeControl
    {
        public string strIniFilePath { get; set; }
        public PmCheckSetting pmCheckSetting { get; set; }
        public PmCheckResult pmCheckResult { get; set; }

        //Window & binding
        public Views.wdPmMessage wdPmMsg { get; set; }
        public IDisposable timerSubscription { get; set; }
        public ReactiveProperty<Visibility> visibility { get; private set; } //Control Hide/Show of Window Message
        public ReactiveProperty<string> message { get; private set; } //Message content of PM Mode control

        //Others
        public void ShowMe()
        {
            //Create new window in UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.wdPmMsg = new Views.wdPmMessage(this); //Pass data context also
                //
                this.wdPmMsg.Show();
            }));
        }

        public void HideMe()
        {
            if(this.wdPmMsg.isClosed == false)
            {
                //this.timerSubscription.Dispose();
                this.visibility.Value = Visibility.Hidden;
                //this.message.Value = "";
            }
        }

        public void ResumeMe()
        {
            if(this.wdPmMsg.isClosed == false)
            {
                this.visibility.Value = Visibility.Visible;
                //this.message.Value = "Resume Success!";
                //this.StartObserverControlTiming();
            }
            else
            {
                this.ShowMe();
                this.visibility.Value = Visibility.Visible;
            }
        }

        public void IniMe()
        {
            //Reading ini file to get infor of last time do PM Check
            this.ReadingIniFile();
            //var test = this.pmCheckSetting;

            //If Timing Control Mode is Start-Up Only => Request Stop Normal Mode immediately (For waiting PM Mode check)
            if(this.pmCheckSetting.intTimingControlMode == 2)
            {
               // nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ChangePmModeRequestAction(true));
                this.ChangePmModeRequest(true);
            }

            //Activate timer
            this.StartObserverControlTiming();
        }

        public void SetMessage(string msg)
        {
            this.message.Value = msg;
        }

        //public void SetCheckResult(bool result)
        //{
        //    this.pmCheckResult.blResult = result;
        //    // If result is true, we clear condition that required check in PM Mode => normal checking can be done
        //    if (result == true)
        //    {
        //        clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ChangePmModeRequestAction(false));
        //    }
        //}

        public void SetCheckResult(bool result)
        {
            this.pmCheckResult.blResult = result;
        }

        /// <summary>
        /// Allow change 
        /// </summary>
        /// <param name="request"></param>
        public void ChangePmModeRequest(bool request)
        {
            clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ChangePmModeRequestAction(request));
        }

        public void StartObserverControlTiming()
        {
            this.timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(1000)) //, DispatcherScheduler.Current) //Subscribe event on UI Thread
                .Subscribe(x =>
                {
                    this.TimingControl();
                });
        }

        public void TimingControl()
        {
            //There are some case need to be handle, depend on Control Timing Mode
            switch (this.pmCheckSetting.intTimingControlMode)
            {
                case 0: //Daily check
                    this.TimingControlDailyCheck();
                    break;
                case 1: //Time amount check
                    this.TimingControlTimeAmout();
                    break;
                case 2: //Start-up only
                    this.TimingControlStartUpOnly();
                    break;
                default:
                    break;
            }
        }

        private void TimingControlStartUpOnly()
        {
            if(this.pmCheckResult.blResult==false)
            {
                if(this.wdPmMsg.isClosed == true)
                {
                    this.ShowMe();
                }
                //this.visibility.Value = Visibility.Visible;
                this.ResumeMe();
                //this.message.Value = "Please do PM check when program start up!" + "\r\n" +
                //    "How to do PM check:" + "\r\n" +
                //    "   1. Change Checking mode to 'PM Mode'" + "\r\n" +
                //    "   2. Insert OK PCB & NG PCB and do checking" + "\r\n" +
                //    "   3. After that, please change to normal mode and running";
            }
            else
            {
                //this.visibility.Value = Visibility.Hidden;
                this.HideMe();
                this.message.Value = "";
            }
        }

        private void TimingControlDailyCheck()
        {
            //Get current timing
            DateTime now = DateTime.Now;

            //Convert TimeSpan to DateTime for compare
            //Create DeadLine points need to do PM check
            List<DateTime>  lstDateTimeDeadLinePoint = this.pmCheckSetting.lsttsDailyTimePoint
                .Select(p => DateTime.Now.Date.Add(p))
                .OrderBy(p=>p.TimeOfDay).ToList(); //Order from smallest to biggest
            //Create Valid points for PM check
            List<DateTime> lstDateTimeValidCheckPoint = lstDateTimeDeadLinePoint.Select(p =>
            {
                return p.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblValidCheckTiming));
            }).ToList();

            //Create Warning points
            List<DateTime> lstDateTimeWarningPoint = lstDateTimeDeadLinePoint.Select(p =>
            {
                return p.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblWarningTime));
            }).ToList();

            //Check if last done checking is still valid or not
            var lastDeadlinePoint = lstDateTimeDeadLinePoint.Where(p => p < now).LastOrDefault();
            if (lastDeadlinePoint < lstDateTimeDeadLinePoint[0]) //current point not yet pass any check points => last point is the biggest point of last day
            {
                lastDeadlinePoint = lstDateTimeDeadLinePoint.Last().Subtract(TimeSpan.FromDays(1));
            }


            //OK, Now we need to compare current timing point with above point for check if reaching deadline or not
            var nextDeadlinePoint = lstDateTimeDeadLinePoint.Where(p => p > now).FirstOrDefault();
            if(nextDeadlinePoint<now) //current point already pass all check points => Next point is the smallest point of next day
            {
                nextDeadlinePoint = lstDateTimeDeadLinePoint[0].AddDays(1);
            }


            //Check if last checking is still valid or not. Valid condition: it must bigger than smallest timing point of valid range of last deadline point
            var lastValidRangeSmallPoint = lastDeadlinePoint.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblValidCheckTiming));
            if(this.pmCheckSetting.dtLastTimeCheck<lastValidRangeSmallPoint) //Surely need to do PM Check
            {
                this.ForcePmCheck();
                return;
            }

            //If last time checking is still valid, then we compare with next deadline point
            //Check warning point
            var nextWarningPoint = nextDeadlinePoint.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblWarningTime));
            if(now>nextWarningPoint)
            {
                this.WarningPmCheck(nextDeadlinePoint-now);
                return;
            }
            else //Last checking still valid, nex warning point not reaching => close Message Window
            {
                this.HideMe();
            }
        }

        private void TimingControlTimeAmout()
        {
            //Get current timing
            DateTime now = DateTime.Now;

            //Check last time do checking is still valid or not
            DateTime lastValidRangeSmallPoint = now.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblTimingAmount + this.pmCheckSetting.dblValidCheckTiming));
            if(lastValidRangeSmallPoint>this.pmCheckSetting.dtLastTimeCheck) //Invalid
            {
                this.ForcePmCheck();
                return;
            }

            //If last time checking is still valid => check with next deadline point
            //Calculate next warning point & next Deadline point
            DateTime nextDeadlinePoint = this.pmCheckSetting.dtLastTimeCheck.AddHours(this.pmCheckSetting.dblTimingAmount);
            DateTime nextWarningPoint = nextDeadlinePoint.Subtract(TimeSpan.FromHours(this.pmCheckSetting.dblWarningTime));

            //Check next Deadline point reaching
            if(now>nextDeadlinePoint)
            {
                this.ForcePmCheck();
                return;
            }

            //Check warning point reaching
            if (now > nextWarningPoint)
            {
                this.WarningPmCheck(nextDeadlinePoint - now);
                return;
            }
            else //Last checking still valid, nex warning point not reaching => close Message Window
            {
                this.HideMe();
            }
        }

        public void ForcePmCheck()
        {
            this.message.Value = "Error: Timeout! You have to do PM Check now!";
            this.ResumeMe();
        }

        public void WarningPmCheck(TimeSpan tsRemain)
        {
            if (tsRemain.Ticks < 0) return;

            this.message.Value = "Warning: You should do PM Check now!" + "\r\n" +
                "Time remain: " + (new DateTime(tsRemain.Ticks)).ToString("HH:mm:ss") + "(s)";
            this.ResumeMe();
        }


        public void UpdatePmCheckTime()
        {
            DateTime now = DateTime.Now;
            this.pmCheckSetting.dtLastTimeCheck = now;
            long lngret = WriteFiles.IniWriteValue(this.strIniFilePath, "PM_CHECK_SETTING", "LastDoneTime", now.ToString());
        }

        public void ReadingIniFile()
        {
            int i = 0;

            this.strIniFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "PmMode.ini";

            //Checking File Setting exist or not
            if (ChkExist.CheckFileExist(this.strIniFilePath) == false)
            {
                MessageBox.Show("General Calibration error: Cannot find Calibration.ini file!", "IniGeneralCal() Error");
                Environment.Exit(0);
            }

            //Reading timing Control setting
            string strTemp = "";
            strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "TimingControl", this.strIniFilePath);
            if (strTemp == "error")
            {
                MessageBox.Show("PM Mode Check error: Cannot find 'TimingControl' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                Environment.Exit(0);
            }

            if(strTemp.Trim()=="0") //No control timing
            {
                this.pmCheckSetting.blTimingControl = false;
            }
            else //Default is activate timing control
            {
                this.pmCheckSetting.blTimingControl = true;
            }

            //Reading Last time did PM Check Mode
            strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "LastDoneTime", this.strIniFilePath);
            if (strTemp == "error")
            {
                MessageBox.Show("PM Mode Check error: Cannot find 'LastDoneTime' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                Environment.Exit(0);
            }

            DateTime newDateTime = new DateTime();
            if(DateTime.TryParse(strTemp, out newDateTime) ==false)
            {
                MessageBox.Show("PM Mode Check error: Cannot convert 'LastDoneTime' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                Environment.Exit(0);
            }
            this.pmCheckSetting.dtLastTimeCheck = newDateTime;

            //Reading TimingControlMode 
            strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "TimingControlMode", this.strIniFilePath);
            if (strTemp == "error")
            {
                MessageBox.Show("PM Mode Check error: Cannot find 'TimingControlMode' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                Environment.Exit(0);
            }

            int intTemp = 0;
            if(int.TryParse(strTemp, out intTemp)==false)
            {
                MessageBox.Show("PM Mode Check error: 'TimingControlMode' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not integer!", "ReadingIniFile() Error");
                Environment.Exit(0);
            }
            if ((intTemp != 1) && (intTemp != 2)) intTemp = 0; //Default value
            this.pmCheckSetting.intTimingControlMode = intTemp;

            //PM Check control daily setting
            if(this.pmCheckSetting.intTimingControlMode == 0)
            {
                //Reading DailyTimePointNum 
                strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "DailyTimePointNum", this.strIniFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("PM Mode Check error: Cannot find 'DailyTimePointNum' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }

                if (int.TryParse(strTemp, out intTemp) == false)
                {
                    MessageBox.Show("PM Mode Check error: 'DailyTimePointNum' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not integer!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }
                this.pmCheckSetting.intDailyTimePointNum = intTemp;

                //Reading Timing Points of setting
                for(i=0;i<this.pmCheckSetting.intDailyTimePointNum;i++)
                {
                    //
                    strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "DailyTimePoint" + (i+1).ToString(), this.strIniFilePath);
                    if (strTemp == "error")
                    {
                        MessageBox.Show("PM Mode Check error: Cannot find 'DailyTimePoint" + (i + 1).ToString() + "' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                        Environment.Exit(0);
                    }

                    //DateTime newDateTime = new DateTime();
                    if(DateTime.TryParse(strTemp, out newDateTime) ==false)
                    {
                        MessageBox.Show("PM Mode Check error: 'DailyTimePoint" + (i + 1).ToString() + "' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not valid timing!", "ReadingIniFile() Error");
                        Environment.Exit(0);
                    }
                    this.pmCheckSetting.lsttsDailyTimePoint.Add(newDateTime.TimeOfDay);
                }
            }

            //Reading TimingAmount setting
            double dblTemp = 0;
            if(this.pmCheckSetting.intTimingControlMode != 2) //Start-Up only mode => don't care
            {
                //Reading DailyTimePointNum 
                strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "TimingAmount", this.strIniFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("PM Mode Check error: Cannot find 'TimingAmount' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }

                if (double.TryParse(strTemp, out dblTemp) == false)
                {
                    MessageBox.Show("PM Mode Check error: 'TimingAmount' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not numeric!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }
                this.pmCheckSetting.dblTimingAmount = dblTemp;
            }

            //Reading Valid check timing &  Warning time
            if (this.pmCheckSetting.intTimingControlMode != 2) //Start-Up only mode => don't care
            {
                //Reading ValidCheckTiming
                strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "ValidCheckTiming", this.strIniFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("PM Mode Check error: Cannot find 'ValidCheckTiming' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }

                if (double.TryParse(strTemp, out dblTemp) == false)
                {
                    MessageBox.Show("PM Mode Check error: 'ValidCheckTiming' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not numeric!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }
                this.pmCheckSetting.dblValidCheckTiming = dblTemp;


                //Reading DailyTimePointNum 
                strTemp = ReadFiles.IniReadValue("PM_CHECK_SETTING", "WarningTime", this.strIniFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("PM Mode Check error: Cannot find 'WarningTime' key name of section 'PM_CHECK_SETTING' in PmMode.ini file!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }

                if (double.TryParse(strTemp, out dblTemp) == false)
                {
                    MessageBox.Show("PM Mode Check error: 'WarningTime' key name of section 'PM_CHECK_SETTING' in PmMode.ini file is not numeric!", "ReadingIniFile() Error");
                    Environment.Exit(0);
                }
                this.pmCheckSetting.dblWarningTime = dblTemp;
            }
        }

        //Constructor
        public PmModeControl()
        {
            this.pmCheckSetting = new PmCheckSetting();
            this.pmCheckResult = new PmCheckResult();
            //Default is hidden PM Mode message when start-up
            this.visibility = new ReactiveProperty<Visibility>();
            this.visibility.Value = Visibility.Hidden;
            //
            this.message = new ReactiveProperty<string>();
            this.message.Value = "";

            //Test App Store
            //clsAppStore.AppStore.Dispatch(new ChangePmModeRequestAction(true));
        }
    }
}
