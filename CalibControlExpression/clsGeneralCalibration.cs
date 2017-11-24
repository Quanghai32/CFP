using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MyLibrary;
using Reactive.Bindings;

namespace CalibControlExpression
{
    public class CalibSetting
    {
        //public int NumStepCalib { get; set; } //How many step calibration need to do

        //Option to request user confirm again each setting point after calibration finish
        public bool blRequestConfirm { get; set; }

        public CalibSetting()
        {
            this.blRequestConfirm = true;
        }
    }

    public class CalibBinding
    {
        //For display info on label (warning + user guide)
        public string warning { get; set; } //lblWarning
        public string userGuide { get; set; } //lblUserGuide

        //For display info in text box
        public string valueSetting { get; set; }   //tbValueSetting - Y setting value
        public string actualValue { get; set; }//tbActualValue - X raw data view
        public string message { get; set; }//tbMessage

        public CalibBinding()
        {
            this.warning = "";
            this.userGuide = "";
            this.valueSetting = "";
            this.actualValue = "";
            this.message = "";
        }
    }

    public class CalibControlProcess: BindableBase
    {
        public clsGeneralCalibration generalCalParent { get; set; } //Should assign from parent class

        public CalibSetting calibSetting { get; set; }
        public CalibBinding calibBinding { get; set; }

        public List<clsLinearModeCalResult> lstclsLinearModeCalResult { get; set; } //This must be assigned form parent class!

        public List<clsCalPointValueSetting> lstCalPointInfo { get; set; } //Contain all info of each cali point need to do
                                                                           //This list must be imported from outside class
        public bool blSetPress { get; set; } //Set button pressed?
        public bool blNextPress { get; set; } //Next Button Pressed?
        public bool blBackPress { get; set; } //Back Button Pressed?
        public bool blCancelPress { get; set; } //Back Button Pressed?
        public bool blOKPress { get; set; } //OK Button pressed?
            
            
        //Do calibration on separate threading
        System.Threading.Thread thrCalibProcess { get; set; }

        public void StartConfirmThread()
        {
            this.thrCalibProcess = new Thread(this.ConfirmProcess);
            this.thrCalibProcess.Start();
        }

        public bool GetConfirmResult()
        {
            foreach (var item in this.lstCalPointInfo)
            {
                if (item.blConfirmResult == false)
                {
                    return false;
                }
            }
            return true;
        }

        public void ConfirmProcess()
        {
            int i = 0;
            bool blFlagJump = false; //Mark when Back press / Next Press
            //
            for (i = 0; i < this.lstCalPointInfo.Count; i++)
            {
                //Display info
                //User Guide Info
                this.calibBinding.userGuide = "Confirm for " + this.lstCalPointInfo[i].strNameCalObject + "\r\n" +
                    "Step " + (i + 1).ToString() + ": Please do checking at point [" + this.lstCalPointInfo[i].dblYCalValue.ToString() + "]. " +
                    "Spec: " + this.lstCalPointInfo[i].dblCaliConfirmLowSpecY.ToString() + "~" + this.lstCalPointInfo[i].dblCaliConfirmHiSpecY.ToString();
           
                //Value setting
                this.calibBinding.valueSetting = this.lstCalPointInfo[i].dblYCalValue.ToString();
                //Clear last actual value display
                this.calibBinding.actualValue = "";
                //Update change
                OnPropertyChanged("calibControlProcess");
                //Waiting until Set button pressed
                while (this.blSetPress == false)
                {
                    DoEvents();
                    //Handle if people click on other button: Back/Next/Cancel...
                    if (this.blBackPress == true) //Jumping & Back to previous step
                    {
                        if (i >= 1) i = i - 2; //Back to previous step
                        this.blBackPress = false;
                        blFlagJump = true;
                        break;
                    }

                    if (this.blNextPress == true) //Jumping to next step
                    {
                        if (i < this.lstCalPointInfo.Count - 1) //Do not allow jumping if at last step - Prevent user continously Next until finish calib!
                        {
                            this.blNextPress = false;
                            blFlagJump = true;
                            break;
                        }
                    }
                }
                //Reset status first
                this.blSetPress = false;

                if (blFlagJump == true) //No need to get data
                {
                    blFlagJump = false;
                    continue; //Reject get raw data
                }

                //Get actual X raw data
                object objRet = this.CalibStepExecute(i);

                //Find ID of Objective
                int intObjectiveID = this.generalCalParent.FindCalObjectiveID(i);
                //Get Adjusted Value
                object objAdjustedValueY = this.generalCalParent.GetAdjustValue(intObjectiveID, objRet);

                //Try to convert to double
                double dblTemp = 0;
                this.lstCalPointInfo[i].blConfirmResult = false;
                if (double.TryParse(objAdjustedValueY.ToString(), out dblTemp) == false) //Cannot convert to double => Result Fail
                {
                    //Display error message
                    this.calibBinding.actualValue = "Error: " + objRet.ToString();
                }
                else //Can convert to double => compare with spec
                {
                    //Compare with Low spec & High spec of each point
                    if ((dblTemp >= this.lstCalPointInfo[i].dblCaliConfirmLowSpecY)
                        && (dblTemp <= this.lstCalPointInfo[i].dblCaliConfirmHiSpecY)) //Result is OK
                    {
                        this.lstCalPointInfo[i].blConfirmResult = true;
                    }
                    //Display result
                    if(this.lstCalPointInfo[i].blConfirmResult == true)
                    {
                        this.calibBinding.actualValue = objAdjustedValueY.ToString() + " [PASS]";
                    }
                    else
                    {
                        this.calibBinding.actualValue = objAdjustedValueY.ToString() + " [FAIL]";
                    }
                   
                }

                //Update change
                OnPropertyChanged("calibControlProcess");
                //Confirm if Total Result is OK?
                if (this.GetConfirmResult() == true) //Confirm process Finish
                {
                    this.calibBinding.message = "Status: All Confirm step done & OK!";
                    //break;
                }
                else
                {
                    //Looking for OK step & Remaining step
                    string strOkStep = "";
                    string strRemainStep = "";
                    for (int j = 0; j < this.lstCalPointInfo.Count; j++)
                    {
                        if (this.lstCalPointInfo[j].blConfirmResult == true)
                        {
                            strOkStep += (j + 1).ToString() + ",";
                        }
                        else
                        {
                            strRemainStep += (j + 1).ToString() + ",";
                        }
                    }
                    this.calibBinding.message = "OK Step: " + strOkStep + ". Remain Step: " + strRemainStep;
                }
                //Update change
                OnPropertyChanged("calibControlProcess");

                //Now, Wait for user pressed Next Button to go to next step
                while (this.blNextPress == false)
                {
                    DoEvents();
                    //Handle if people click on other button: Back/Next/Cancel...
                    if (this.blSetPress == true) //User want to set again with current step
                    {
                        i = i - 1; //Keep running at current step
                        //this.blSetPress = false; //If we set false here, user need to click 2 times later!
                        break;
                    }

                    if (blBackPress == true) //User want to back to previous step
                    {
                        if (i >= 1)
                        {
                            i = i - 2; //Back to previous step
                            this.blBackPress = false;
                            break;
                        }
                    }
                }
                //Reset status
                this.blNextPress = false;

                //If remain step, then request user calib again these step
                if (i == this.lstCalPointInfo.Count - 1) //Only do confirm when reaching last step
                {
                    if (this.GetConfirmResult() == false)
                    {
                        i = i - 1; //Keep user at last step, do not allow finish if there is remaining step
                    }
                }
            }

            this.calibBinding.userGuide = "Confirm Process Completed! Please click OK to finish and update timing record!";
            //Update change
            OnPropertyChanged("ConfirmCompleted");

            //waiting user press OK Button
            while (this.blOKPress == false)
            {
                DoEvents();
            }
            this.blOKPress = false;

            //Calibration process finish task
            this.ConfirmFinishTask();
        }

        public void ConfirmFinishTask()
        {
            //Update change
            OnPropertyChanged("ConfirmFinishTask");
        }

        public void StartCalibrationThread()
        {
            this.thrCalibProcess = new Thread(this.CalibProcess);
            this.thrCalibProcess.Start();
        }

        public void ForceEndCalibThread()
        {
            if(this.thrCalibProcess!=null)
            {
                if(this.thrCalibProcess.IsAlive==true)
                {
                    this.thrCalibProcess.Abort();
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        public void CalibProcess()
        {
            int i = 0;
            bool blFlagJump = false; //Mark when Back press / Next Press
            //
            for(i=0;i<this.lstCalPointInfo.Count;i++)
            {
                //Display info
                //User Guide Info
                this.calibBinding.userGuide = this.lstCalPointInfo[i].strCaliUserGuide;
                if (this.calibBinding.userGuide.Trim() == "")
                {
                    this.calibBinding.userGuide = "Calibration for " + this.lstCalPointInfo[i].strNameCalObject + "\r\n" +
                        "Step " + (i + 1).ToString() + ": Please do calibration at point [" + this.lstCalPointInfo[i].dblYCalValue.ToString() + "]. Spec for X value: "+
                        this.lstCalPointInfo[i].dblCaliLowSpecXRawData.ToString() + "~" + this.lstCalPointInfo[i].dblCaliHiSpecXRawData.ToString();
                }
                //Value setting
                this.calibBinding.valueSetting = this.lstCalPointInfo[i].dblYCalValue.ToString();
                //Clear last actual value display
                this.calibBinding.actualValue = "";
                //Update change
                OnPropertyChanged("calibControlProcess");
                //Waiting until Set button pressed
                while (this.blSetPress == false)
                {
                    DoEvents();
                    //Handle if people click on other button: Back/Next/Cancel...
                    if(this.blBackPress == true) //Jumping & Back to previous step
                    {
                        if (i >= 1) i = i - 2; //Back to previous step
                        this.blBackPress = false;
                        blFlagJump = true;
                        break;
                    }

                    if (this.blNextPress == true) //Jumping to next step
                    {
                        if (i < this.lstCalPointInfo.Count-1) //Do not allow jumping if at last step - Prevent user continously Next until finish calib!
                        {
                            this.blNextPress = false;
                            blFlagJump = true;
                            break;
                        }
                    }
                }
                //Reset status first
                this.blSetPress = false;

                if(blFlagJump==true) //No need to get data
                {
                     blFlagJump = false;
                     continue; //Reject get raw data
                }

                //Get actual X raw data
                object objRet = this.CalibStepExecute(i);

                double dblTemp = 0;
                if (double.TryParse(objRet.ToString(), out dblTemp) == false) //Cannot convert to double => Result Fail
                {
                    this.lstCalPointInfo[i].blCalibResult = false;
                    //Display error message
                    this.calibBinding.actualValue = "Error: " + objRet.ToString();
                }
                else //Can convert to double => need to compare with low spec & Hi spec of X raw data
                {
                    //this.lstCalPointInfo[i].blCalibResult = true;
                    this.lstCalPointInfo[i].dblXRawValue = dblTemp; //This value need to be saved for separate range of x value in running time.
                    //Display measure result
                    if((dblTemp>=this.lstCalPointInfo[i].dblCaliLowSpecXRawData)&&
                        (dblTemp<=this.lstCalPointInfo[i].dblCaliHiSpecXRawData))
                    {
                        this.lstCalPointInfo[i].blCalibResult = true;
                        this.calibBinding.actualValue = objRet.ToString() + " [" + "PASS]";
                    }
                    else //Fail
                    {
                        this.lstCalPointInfo[i].blCalibResult = false;
                        this.calibBinding.actualValue = objRet.ToString() + " [" + "FAIL]";
                    }
                    
                }
                //Update change
                OnPropertyChanged("calibControlProcess");
                //Confirm if Total Result is OK?
                if (this.GetCalibResult() == true) //Calibration Finish
                {
                    this.calibBinding.message = "Status: All Calibration step done & OK!";
                    //break;
                }
                else
                {
                    //Looking for OK step & Remaining step
                    string strOkStep = "";
                    string strRemainStep = "";
                    for (int j = 0; j < this.lstCalPointInfo.Count; j++)
                    {
                        if (this.lstCalPointInfo[j].blCalibResult == true)
                        {
                            strOkStep += (j + 1).ToString() + ",";
                        }
                        else
                        {
                            strRemainStep += (j + 1).ToString() + ",";
                        }
                    }

                    this.calibBinding.message = "OK Step: " + strOkStep + ". Remain Step: " + strRemainStep;
                }
                //Update change
                OnPropertyChanged("calibControlProcess");

                //Now, Wait for user pressed Next Button to go to next step
                while (this.blNextPress == false)
                {
                    DoEvents();
                    //Handle if people click on other button: Back/Next/Cancel...
                    if(this.blSetPress==true) //User want to set again with current step
                    {
                        i = i - 1; //Keep running at current step
                        //this.blSetPress = false; //If we set false here, user need to click 2 times later!
                        break;
                    }

                    if(blBackPress == true) //User want to back to previous step
                    {
                        if(i>=1)
                        {
                            i = i - 2; //Back to previous step
                            this.blBackPress = false;
                            break;
                        }
                    }
                }
                //Reset status
                this.blNextPress = false;

                //If remain step, then request user calib again these step
                if(i==this.lstCalPointInfo.Count-1) //Only do confirm when reaching last step
                {
                    if(this.GetCalibResult()==false)
                    {
                        i = i - 1; //Keep user at last step, do not allow finish if there is remaining step
                    }
                }

            }

            //Inform Calibration completed & 
            this.calibBinding.userGuide = "Calibration Completed. Please click OK to saving calib data & finish!";
            //Update change
            OnPropertyChanged("CalibCompleted");

            //waiting user press OK Button
            while(this.blOKPress==false)
            {
                DoEvents();
            }
            this.blOKPress = false;

            //Calibration process finish task
            this.CalibFinishTask();
        }

        public bool GetCalibResult()
        {
            foreach(var item in this.lstCalPointInfo)
            {
                if(item.blCalibResult==false)
                {
                    return false;
                }
            }
            return true;
        }

        public object CalibStepExecute(int StepID)
        {
            if (StepID > lstCalPointInfo.Count - 1) return "CalibStepExecute() Error: StepID [" + StepID.ToString() + "] is out of range!";
            //If user click press, then do call function to get X raw data
            //Test Call Method from actual object - Cal value of X variable
            object objRet = this.CallActualObject(this.lstCalPointInfo[StepID].objMeasureObjectSource,
                                                  this.lstCalPointInfo[StepID].strMethodName,
                                                  this.lstCalPointInfo[StepID].arrObjPara);
            //
            return objRet;
        }

        public void CalibFinishTask()
        {
            //Update change
            OnPropertyChanged("CalibFinishTask");
        }

        //Call method from Actual Object
        private static Mutex mutex = new Mutex();

        /// <summary>
        /// This function use to call a function inside an object to get actual X value
        /// </summary>
        /// <param name="objSource">The class instance (object) hold function need to call inside</param>
        /// <param name="objMethodName">The method name need to call to get X raw value</param>
        /// <param name="objPara">The parameters for method name to get X raw value</param>
        /// <returns></returns>
        public object CallActualObject(object objSource, object objMethodName, object[] objPara)
        {
            mutex.WaitOne();
            //
            string strMethodName = objMethodName.ToString().Trim();
            try
            {
                if (objSource.GetType().GetMethod(strMethodName) != null)
                {
                    return objSource.GetType().GetMethod(strMethodName).Invoke(objSource, objPara);
                }
                else
                {
                    return "CALL() Error: cannot find method [" + strMethodName + "] of object type [" + objSource.GetType().ToString() + "]";
                }
            }
            catch (Exception ex)
            {
                return "CALL() Error: " + ex.Message;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
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

        public CalibControlProcess(clsGeneralCalibration generalCalParent)
        {
            this.generalCalParent = generalCalParent;
        }
    }

    public class clsGeneralCalibration: BindableBase
    {
        public System.Windows.Controls.MenuItem menuItemUserSetting { get; set; } //This menu item should add to main menu
        public Views.wdRequestCalib wdRequestCalib { get; set; }
        public Views.wdGeneralCalibration wdGeneralCal { get; set; }
        public Views.wdValObserver wdAdjustedValObserver { get; set; } //For observe Adjusted value after do calibration
        public Views.wdUserControl wdUserControlPanel { get; set; }
        public List<clsObjectiveCali> lstclsObjectiveCal { get; set; }
        public List<clsUserControlData> lstclsUserControlData { get; set; }
        public string strCaliFilePath { get; set; }

        //For control Calibration Process
        public int intNumStepCali { get; set; } //How many step need to do calibration
        public int intStepCalibration { get; set; } //Control the progress of Calibration process
        public bool blTotalCalibResult { get; set; } //Indicate all process

        public System.Timers.Timer myTimer { get; set; } //For Adjusted Value observer

        //For Calibration Timing Control 
        public int intTimingControl { get; set; } //Need (1) or no need (0) control calib time
        public int intCalibControlMode { get; set; } //What is calib control mode: Daily (0) or fix time (1)
        public int intCalibDailyTimePointNum { get; set; } //How many time in a day need to calibration
        public List<TimeSpan> lstDateTimeCalib { get; set; } //List of Timing Point need to calibration
        public double dblCalibTimingAmount { get; set; } //After how many time (hours) need to calibration again - using in Fix time mode
        public double dblCalibWarningTime { get; set; } //Before force user do calib, push out message for warning - unit Hours

        public System.Timers.Timer TimerCalibTimeControl { get; set; } //For Calibration Timing Control

        //Re-structure for binding MVVM
        public CalibControlProcess calibControlProcess { get; set; }

        //For support user call function from programlist
        public object objProcess { get; set; } // Process contains function name want to call
        public string PLFuncName { get; set; } // Function Name which want to call from Program List
        public object[] objArrPara { get; set; } // Parameter

        public ReactiveProperty<string> RequestMessage { get; set; } //Request Calibration message
        public ReactiveProperty<System.Windows.Media.SolidColorBrush> RequestMessageColor { get; set; } // Color of Request message
        public ReactiveCommand ShowCalibFormCommand { get; private set; } //When user click on button => Show calib form

        //
        public void IniGeneralCal()
        {
            this.strCaliFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Calibration.ini";

            //Checking File Setting exist or not
            //MyLibrary.ChkExist clsCheckFile = new MyLibrary.ChkExist();

            if(MyLibrary.ChkExist.CheckFileExist(strCaliFilePath)==false)
            {
                MessageBox.Show("General Calibration error: Cannot find Calibration.ini file!", "IniGeneralCal() Error");
                return;
            }

            //if file found, then try to load setting
            int intNumberCalObject = 0;
            string strTemp = "";
            strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "NumberCalObject", strCaliFilePath);
            if(strTemp=="error")
            {
                MessageBox.Show("General Calibration error: Cannot find 'NumberCalObject' key name of section 'CAL_SETTING' in Calibration.ini file!", "IniGeneralCal() Error");
                return;
            }

            if(int.TryParse(strTemp, out intNumberCalObject)==false)
            {
                MessageBox.Show("General Calibration error: Setting Number object calibration [" + strTemp + "] is not integer!", "IniGeneralCal() Error");
                return;
            }

            //Ini for list of class
            int i, j = 0;
            int intTemp = 0;
            double dblTemp = 0;

            for(i=0;i<intNumberCalObject;i++)
            {
                clsObjectiveCali clsTemp = new clsObjectiveCali();
                //Reading setting
                //Reading name of object
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "NameCalObject" + (i+1).ToString(), strCaliFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("General Calibration error: Cannot find [" + "NameCalObject" + (i+1).ToString() +  "] key name of section 'CAL_SETTING' in Calibration.ini file!", "IniGeneralCal() Error");
                    return;
                }
                clsTemp.strNameCalObject = strTemp;

                //Number of Calibration value of object i
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "NumberCalValue" + (i + 1).ToString(), strCaliFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("General Calibration error: Cannot find [" + "NumberCalValue" + (i + 1).ToString() + "] key name of section 'CAL_SETTING' in Calibration.ini file!", "IniGeneralCal() Error");
                    return;
                }
                if(int.TryParse(strTemp, out intTemp)==false)
                {
                    MessageBox.Show("General Calibration error: NumberCalValue" + (i+1).ToString() + " setting [" + strTemp + "] is not integer!", "IniGeneralCal() Error");
                    return;
                }
                clsTemp.intNumberCalValue = intTemp;


                //Reading Low value & Hi value... of each object
                for (j = 0; j < intTemp;j++)
                {

                    clsCalPointValueSetting clsCalValTemp = new clsCalPointValueSetting();

                    //Calibration value setting
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "CalObjectValue" + (i + 1).ToString() + (j+1).ToString(), strCaliFilePath);
                    if (strTemp == "error")
                    {
                        MessageBox.Show("General Calibration error: Cannot find [" + "CalObjectValue" + (i + 1).ToString() + (j+1).ToString() + "] key name of section 'CAL_SETTING' in Calibration.ini file!", "IniGeneralCal() Error");
                        return;
                    }

                    if(double.TryParse(strTemp, out dblTemp)==false)
                    {
                        MessageBox.Show("General Calibration error: Setting value " + "CalObjectValue" + (i + 1).ToString() + (j + 1).ToString() + "[" + strTemp + "] is not numeric!", "IniGeneralCal() Error");
                        return;
                    }

                    clsCalValTemp.dblYCalValue = dblTemp;

                    //Loading user Guide?

                    //Loading last time cali X raw value
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_RESULT", "CalObjectXRawValue" + (i + 1).ToString(), strCaliFilePath);
                    List<string> lstXRaws = strTemp.Split(';').ToList();

                    double dblXRaw = 0;
                    if(j<lstXRaws.Count)
                    {
                        if(double.TryParse(lstXRaws[j],out dblXRaw)==true)
                        {
                            clsCalValTemp.dblXRawValue = dblXRaw;
                        }
                    }

                    //Add to list
                    clsTemp.lstclsCalPointValSetting.Add(clsCalValTemp);
                }

                //Reading current saving calibration result
                int intNumCalResultData = clsTemp.intNumberCalValue - 1; //If cali at 2 point => 1 data (gain & offset). If Cali at n point => need (n-1) data (Gain & Offset)
                string strGainKeyname = "";
                string strOffSetname = "";

                if (intNumCalResultData == 1) //Only 1 couple => (CalObjectGainValue1 & CalObjectOffsetValue1),  (CalObjectGainValue2 & CalObjectOffsetValue2)...
                {
                    strGainKeyname = "CalObjectGainValue" + (i + 1).ToString();
                    strOffSetname = "CalObjectOffsetValue" + (i + 1).ToString();

                    //
                    clsLinearModeCalResult clsCalTemp = new clsLinearModeCalResult();

                    //Loading result data
                    //Gain
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_RESULT", strGainKeyname, strCaliFilePath);
                    if (strTemp == "error")
                    {
                        MessageBox.Show("General Calibration error: Cannot find [" + strGainKeyname + "] key name of section 'CAL_RESULT' in Calibration.ini file!", "IniGeneralCal() Error");
                        return;
                    }
                    if (double.TryParse(strTemp, out dblTemp) == false)
                    {
                        MessageBox.Show("General Calibration error: Setting value " + strGainKeyname + "[" + strTemp + "] is not numeric!", "IniGeneralCal() Error");
                        return;
                    }
                    clsCalTemp.dblGainValue = dblTemp;

                    //Offset
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_RESULT", strOffSetname, strCaliFilePath);
                    if (strTemp == "error")
                    {
                        MessageBox.Show("General Calibration error: Cannot find [" + strOffSetname + "] key name of section 'CAL_RESULT' in Calibration.ini file!", "IniGeneralCal() Error");
                        return;
                    }
                    if (double.TryParse(strTemp, out dblTemp) == false)
                    {
                        MessageBox.Show("General Calibration error: Setting value " + strOffSetname + "[" + strTemp + "] is not numeric!", "IniGeneralCal() Error");
                        return;
                    }
                    clsCalTemp.dblOffsetValue = dblTemp;

                    //Add to list
                    clsTemp.lstclsLinearModeCalResult.Add(clsCalTemp);
                    
                }
                else //Cali at more than 2 point => there are more than 1 couple of data (Gain & Offset)
                {
                    for(j=0;j<intNumCalResultData;j++)
                    {
                        strGainKeyname = "CalObjectGainValue" + (i + 1).ToString() + "_" + (j+1).ToString();
                        strOffSetname = "CalObjectOffsetValue" + (i + 1).ToString() + "_" + (j + 1).ToString();

                        //
                        clsLinearModeCalResult clsCalTemp = new clsLinearModeCalResult();

                        //Loading result data
                        //Gain
                        strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_RESULT", strGainKeyname, strCaliFilePath);
                        if (strTemp == "error")
                        {
                            MessageBox.Show("General Calibration error: Cannot find [" + strGainKeyname + "] key name of section 'CAL_RESULT' in Calibration.ini file!", "IniGeneralCal() Error");
                            return;
                        }
                        if (double.TryParse(strTemp, out dblTemp) == false)
                        {
                            MessageBox.Show("General Calibration error: Setting value " + strGainKeyname + "[" + strTemp + "] is not numeric!", "IniGeneralCal() Error");
                            return;
                        }
                        clsCalTemp.dblGainValue = dblTemp;

                        //Offset
                        strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_RESULT", strOffSetname, strCaliFilePath);
                        if (strTemp == "error")
                        {
                            MessageBox.Show("General Calibration error: Cannot find [" + strOffSetname + "] key name of section 'CAL_RESULT' in Calibration.ini file!", "IniGeneralCal() Error");
                            return;
                        }
                        if (double.TryParse(strTemp, out dblTemp) == false)
                        {
                            MessageBox.Show("General Calibration error: Setting value " + strOffSetname + "[" + strTemp + "] is not numeric!", "IniGeneralCal() Error");
                            return;
                        }
                        clsCalTemp.dblOffsetValue = dblTemp;

                        //Add to list
                        clsTemp.lstclsLinearModeCalResult.Add(clsCalTemp);
                    }
                }

                //Add to list
                this.lstclsObjectiveCal.Add(clsTemp);
            }

            ///////////////////////////////For user control////////////////////////////////////////////////////
            strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "NumberUserControl", strCaliFilePath);
            if (strTemp == "error")
            {
                MessageBox.Show("General Calibration error: Cannot find [" + "NumberUserControl" + "] key name of section 'CAL_SETTING' in Calibration.ini file!", "IniGeneralCal() Error");
                return;
            }

            intTemp = 0;
            if (int.TryParse(strTemp, out intTemp) == false)
            {
                MessageBox.Show("General Calibration error: NumberUserControl Setting value [" + strTemp + "] is not integer!", "IniGeneralCal() Error");
                return;
            }

            for (i = 0; i < intTemp;i++)
            {
                clsUserControlData clsTemp = new clsUserControlData();
                //
                //Load name of user control
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_SETTING", "NameUserControl" + (i+1).ToString(), strCaliFilePath);
                if (strTemp != "error")
                {
                    clsTemp.strNameUserControl = strTemp;
                }

                //
                this.lstclsUserControlData.Add(clsTemp);
            }

            //Reading Calibration Timing control setting
            //Timing Control setting
            strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "TimingControl", strCaliFilePath);
            if (strTemp == "error")
            {
                MessageBox.Show("General Calibration error: Cannot find [" + "TimingControl" + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                return;
            }
            if(int.TryParse(strTemp.Trim(), out intTemp)==false)
            {
                MessageBox.Show("Warning: TimingControl setting" + strTemp.Trim() + " is not integer! Auto setting to control calibration mode [1]", "IniGeneralCal() Warning");
                this.intTimingControl = 1;
            }
            this.intTimingControl = intTemp;

            //Only care another setting if setting is control calib timing
            if (this.intTimingControl == 1)
            {
                //CalibControlMode 
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "CalibControlMode", strCaliFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("General Calibration error: Cannot find [" + "CalibControlMode" + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                    return;
                }
                if (int.TryParse(strTemp.Trim(), out intTemp) == false)
                {
                    MessageBox.Show("Warning: CalibControlMode setting" + strTemp.Trim() + " is not integer! Auto setting to Daily calibration mode [0]", "IniGeneralCal() Warning");
                    this.intCalibControlMode = 0;
                }
                this.intCalibControlMode = intTemp;

                if ((this.intCalibControlMode != 0) && (this.intCalibControlMode != 1))
                {
                    MessageBox.Show("General Calibration error: [" + this.intCalibControlMode.ToString() + "] is invalid Calibration Control Mode! Accepted value is 0 or 1 only!", "IniGeneralCal() Error");
                    return;
                }

                //Depend on Calibration setting => need to check some setting
                if (this.intCalibControlMode == 0) //Daily Calib
                {
                    //CalibDailyTimePointNum 
                    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "CalibDailyTimePointNum", strCaliFilePath);
                    if (strTemp == "error")
                    {
                        MessageBox.Show("General Calibration error: Cannot find [" + "CalibDailyTimePointNum" + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                        return;
                    }
                    if (int.TryParse(strTemp.Trim(), out intTemp) == false)
                    {
                        MessageBox.Show("Error: CalibDailyTimePointNum setting" + strTemp.Trim() + " is not integer!", "IniGeneralCal() Error");
                        return;
                    }
                    this.intCalibDailyTimePointNum = intTemp;
                    //DailyTimePoint
                    this.lstDateTimeCalib = new List<TimeSpan>();
                    for(i=0;i<this.intCalibDailyTimePointNum;i++)
                    {
                        strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "DailyTimePoint" + (i+1).ToString(), strCaliFilePath);
                        if (strTemp == "error")
                        {
                            MessageBox.Show("General Calibration error: Cannot find [" + "DailyTimePoint" + (i + 1).ToString() + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                            return;
                        }

                        DateTime dtTemp = new DateTime();
                        if(DateTime.TryParse(strTemp.Trim(),out dtTemp)==false)
                        {
                            MessageBox.Show("General Calibration error: Time Point setting [" + strTemp.ToString() + "] is not Date Time format!", "IniGeneralCal() Error");
                            return;
                        }
                        //Add to list
                        this.lstDateTimeCalib.Add(dtTemp.TimeOfDay);
                    }
                }
                //else if(this.intCalibControlMode == 1) //Fix time Calib
                //{
                //    //CalibTimingAmount 
                //    strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "CalibTimingAmount", strCaliFilePath);
                //    if (strTemp == "error")
                //    {
                //        MessageBox.Show("General Calibration error: Cannot find [" + "CalibTimingAmount" + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                //        return;
                //    }
                //    if (double.TryParse(strTemp.Trim(), out dblTemp) == false)
                //    {
                //        MessageBox.Show("Error: CalibTimingAmount setting" + strTemp.Trim() + " is not numeric!", "IniGeneralCal() Error");
                //        return;
                //    }
                //    this.dblCalibTimingAmount = dblTemp;
                //}

                //CalibTimingAmount 
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "CalibTimingAmount", strCaliFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("General Calibration error: Cannot find [" + "CalibTimingAmount" + "] key name of section 'CAL_TIMING' in Calibration.ini file!", "IniGeneralCal() Error");
                    return;
                }
                if (double.TryParse(strTemp.Trim(), out dblTemp) == false)
                {
                    MessageBox.Show("Error: CalibTimingAmount setting" + strTemp.Trim() + " is not numeric!", "IniGeneralCal() Error");
                    return;
                }
                this.dblCalibTimingAmount = dblTemp;

                //Reading warning time setting
                strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "CalibWarningTime", strCaliFilePath);
                if (strTemp == "error")
                {
                    MessageBox.Show("General Calibration error: Cannot find [" + "CalibWarningTime" + "] key name of section 'CAL_TIMING' in Calibration.ini file! Auto setting to 0.5 Hours", "IniGeneralCal() Error");
                    strTemp = "0.5";
                }
                if (double.TryParse(strTemp.Trim(), out dblTemp) == false)
                {
                    MessageBox.Show("Error: CalibWarningTime setting" + strTemp.Trim() + " is not numeric! Auto setting to 0.5 Hours", "IniGeneralCal() Error");
                    return;
                }
                this.dblCalibWarningTime = dblTemp;


                //Create new reading form in UI Thread
                this.CreateWindow();
                //this.IniForBinding();

                //If everything is OK, then start Timer to observe timing point
                this.TimerCalibTimeControl = new System.Timers.Timer();
                this.TimerCalibTimeControl.Interval = 100;
                this.TimerCalibTimeControl.Elapsed += TimerCalibTimeControl_Elapsed;
                this.TimerCalibTimeControl.Enabled = true;
                this.TimerCalibTimeControl.Start();

            } //End if (this.intTimingControl == 1)

            //Cal number step of calibration process
            this.intNumStepCali = 0;

            for(i=0;i<this.lstclsObjectiveCal.Count;i++)
            {
                this.intNumStepCali += this.lstclsObjectiveCal[i].intNumberCalValue;
            }
        }

        //
        public void ResetNewCalProcess()
        {
            if (this.lstclsObjectiveCal == null) return;

            //Stop Thread Calibration if still in process
            if (this.calibControlProcess != null)
            {
                this.calibControlProcess.ForceEndCalibThread();
                this.calibControlProcess.calibBinding = new CalibBinding();
                OnPropertyChanged("calibControlProcess");
            }

            //Reset all last result
            for (int i = 0;i<this.lstclsObjectiveCal.Count;i++)
            {
                this.lstclsObjectiveCal[i].blCaliResult = false;
                for(int j=0;j<this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count;j++)
                {
                    this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].ResetNewCal();
                }
            }

            //Reset all Button state
            IniButtonState();
        }

        void IniButtonState()
        {
            this.btnConfirmEnable = true; OnPropertyChanged("btnConfirmEnable");
            this.btnCalibAgainEnable = true; OnPropertyChanged("btnCalibAgainEnable");
            this.btnValObserverEnable = true; OnPropertyChanged("btnValObserverEnable");
            this.btnUserControlEnable = true; OnPropertyChanged("btnUserControlEnable");

            this.btnOkEnable = false; OnPropertyChanged("btnOkEnable");
            this.btnSetEnable = false; OnPropertyChanged("btnSetEnable");
            this.btnNextEnable = false; OnPropertyChanged("btnNextEnable");
            this.btnBackEnable = false; OnPropertyChanged("btnBackEnable");
        }

        void TimerCalibTimeControl_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.TimerCalibTimeControl.Enabled = false;
            //
            int i = 0;
            TimeSpan tsTemp = new TimeSpan();
            //Get current timing
            DateTime now = DateTime.Now;

            //Read Last time Calib from setting file
            string strTemp = "";
            strTemp = MyLibrary.ReadFiles.IniReadValue("CAL_TIMING", "LastCalTime", this.strCaliFilePath);
            if (strTemp == "error")
            {
                this.TimerCalibTimeControl.Enabled = true;
                return;
            }

            DateTime dtLastTime = new DateTime();
            if(DateTime.TryParse(strTemp.Trim(), out dtLastTime)==false)
            {
                this.TimerCalibTimeControl.Enabled = true;
                return;
            }

            if(this.intCalibControlMode==0) //Daily calib
            {
                //Convert TimeSpan to DateTime for compare
                List<DateTime> lstDateTimeCalib = new List<DateTime>();

                for(i=0;i<this.lstDateTimeCalib.Count;i++)
                {
                    DateTime dtTemp = new DateTime();
                    dtTemp = now.Date.Add(this.lstDateTimeCalib[i]);
                    lstDateTimeCalib.Add(dtTemp);
                }

                //Re-arrange from smallest to biggest
                lstDateTimeCalib.Sort();

                //Looking for nearest point of time need to do calibration
                int intIndex = 0;
                for(i=0;i<lstDateTimeCalib.Count;i++)
                {
                    if(now<lstDateTimeCalib[i]) //Next time not yet pass?
                    {
                        break;
                    }
                }

                intIndex = i;

                //Find last time & next time need to do Calib
                DateTime dtCalibNextPoint = new DateTime();
                DateTime dtCalibLastPoint = new DateTime();
                if(intIndex==lstDateTimeCalib.Count) //All Calib point within day already pass => need to switch to compare with next day nearest calib point
                {
                    dtCalibNextPoint = lstDateTimeCalib[0].Add(new TimeSpan(24, 0, 0)); //smallest point add 24 hours for next day calib point
                    dtCalibLastPoint = lstDateTimeCalib[lstDateTimeCalib.Count - 1]; //Last point of time
                }
                else //Not yet pass all calib point within day
                {
                    dtCalibNextPoint = lstDateTimeCalib[intIndex];
                    if (intIndex>0)
                    {
                        dtCalibLastPoint = lstDateTimeCalib[intIndex - 1]; //Last point of time
                    }
                    else
                    {
                        dtCalibLastPoint = lstDateTimeCalib[0]; //Last point of time
                    }
                }

                //Check if Last time calib is still remain effect or not
                // 1st Condition: Last time point calib is already done
                if (dtLastTime > dtCalibLastPoint.Subtract(TimeSpan.FromHours(this.dblCalibTimingAmount))) //Confirm already calib for last time
                {
                    // 2nd Condition: now < next point - warning time
                    if(now<dtCalibNextPoint.Subtract(TimeSpan.FromHours(this.dblCalibWarningTime))) //Still has effect & no need warning?
                    {
                        //No need to do anything
                        this.TimerCalibTimeControl.Enabled = true;
                        this.ShowCalibTimeRemain(dtCalibNextPoint - now);
                        return;
                    }
                }
                else //Not yet calib for last time => surely need to do Calib!
                {
                    this.RequestCalib();
                    this.TimerCalibTimeControl.Enabled = true;
                    return;
                }

                //Check if already calib for next point of time
                if(dtLastTime>dtCalibNextPoint.Subtract(TimeSpan.FromHours(this.dblCalibTimingAmount)))
                {
                    //No need to do anything more
                    this.TimerCalibTimeControl.Enabled = true;
                    this.ShowCalibTimeRemain(dtCalibNextPoint - now);
                    return;
                }

                //Forcing point confirm
                if(now>dtCalibNextPoint)
                {
                    //surely request calib
                    this.RequestCalib();
                    this.TimerCalibTimeControl.Enabled = true;
                    return;
                }

                //From calib point of time, cal back to find warning point
                DateTime dtWarningPoint = new DateTime();
                dtWarningPoint = dtCalibNextPoint.Subtract(TimeSpan.FromHours(this.dblCalibWarningTime));

                //Check if warning time point reached?
                tsTemp = dtCalibNextPoint - now;
                if (now>dtWarningPoint)
                {
                    //cal time remaining
                    this.WarningCalib(tsTemp);
                }

            }
            else if (this.intCalibControlMode==1) //Fix time Calib
            {
                //Cal Amount of Time from Last time calib & Current point
                tsTemp = now - dtLastTime;

                //Forcing do calibration
                if (tsTemp.TotalHours > this.dblCalibTimingAmount)
                {
                    this.RequestCalib();
                }
                else
                {
                    //Warning message
                    if (tsTemp.TotalHours > (this.dblCalibTimingAmount - this.dblCalibWarningTime))
                    {
                        this.WarningCalib(dtLastTime.Add(TimeSpan.FromHours(this.dblCalibTimingAmount)) - now);
                    }
                    else
                    {
                        this.ShowCalibTimeRemain(dtLastTime.Add(TimeSpan.FromHours(this.dblCalibTimingAmount)) - now);
                    }
                }
            }

            //
            this.TimerCalibTimeControl.Enabled = true;
        }

        public void RequestCalib()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.RequestMessage.Value = "Error: Time up. You have to do calibration now!\r\nPlease start Calibration...";
                if (this.wdRequestCalib.isClosed == true) //If user close, then we have to create new windows again!
                {
                    this.wdRequestCalib = new Views.wdRequestCalib(this);
                }
                this.wdRequestCalib.Show();
            });
        }

        public void CloseRequestCalibWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.RequestMessage.Value = "";
                if (this.wdRequestCalib.isClosed == false) //If user close, then we have to create new windows again!
                {
                    this.wdRequestCalib.Close();
                }
            });
        }

        public void ShowCalibForm()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.objProcess != null)
                {
                    this.CallActualObject(this.objProcess, this.PLFuncName, this.objArrPara);
                }

                this.wdGeneralCal.Hide(); //Prepare to show dialog
                //this.Show(1, "Error: Time up. You have to do calibration now!\r\nPlease start Calibration...");
                this.ShowModal("Error: Time up. You have to do calibration now!\r\nPlease start Calibration...");
            });
        }

        public void WarningCalib(TimeSpan tsRemainTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.calibControlProcess.calibBinding.warning = "Warning: Please do calibration now!\r\nTime remain: " +
                tsRemainTime.Hours.ToString() + ":" + tsRemainTime.Minutes.ToString() + ":" + tsRemainTime.Seconds.ToString();
                this.ShowWarning();
                OnPropertyChanged("calibControlProcess");
            });
        }

        public void ShowCalibTimeRemain(TimeSpan tsRemainTime)
        {
            this.calibControlProcess.calibBinding.warning = "Time remain: " +
               tsRemainTime.Hours.ToString() + ":" + tsRemainTime.Minutes.ToString() + ":" + tsRemainTime.Seconds.ToString();

            OnPropertyChanged("calibControlProcess");
        }

        //Show window
        public void ShowIni()
        {
            this.ResetNewCalProcess();
            //
            if (this.wdGeneralCal == null) return;

            if (this.wdGeneralCal.isClosed == false) //Not yet closed
            {
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                   this.wdGeneralCal.Show();
                }));
            }
            else //Already closed - Create new window & show again
            {
                CreateWindow();
                //
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                    this.wdGeneralCal.Show();
                }));
            }
        }

        //Show window when display warning
        public void ShowWarning()
        {
            if (this.wdGeneralCal == null) return;

            if (this.wdGeneralCal.isClosed == false) //Not yet closed
            {
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                    this.wdGeneralCal.Show();
                }));
            }
            else //Already closed - Create new window & show again
            {
                CreateWindow();
                //
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                    this.wdGeneralCal.Show();
                }));
            }
        }

        /// <summary>
        /// Show when force user has to do calibration
        /// </summary>
        /// <param name="intShowModal"></param>
        /// <param name="strMessage"></param>
        public void ShowModal(string strMessage = "")
        {
            if (this.wdGeneralCal == null) return;

            if (this.wdGeneralCal.isClosed == false) //Not yet closed
            {
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                    this.ResetNewCalProcess();
                    //this.wdGeneralCal.lblWarning.Content = strMessage;
                    this.calibControlProcess.calibBinding.warning = strMessage;
                    OnPropertyChanged("calibControlProcess");
                    this.wdGeneralCal.ShowDialog();
                }));
            }
            else //Already closed - Create new window & show again
            {
                CreateWindow();
                //
                this.wdGeneralCal.Dispatcher.Invoke(new Action(() =>
                {
                    this.calibControlProcess.calibBinding.warning = strMessage;
                    OnPropertyChanged("calibControlProcess");
                    this.wdGeneralCal.ShowDialog();
                }));
            }
        }

        public void CreateWindow()
        {
            //Create new reading form in UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                IniForCalibControlProcess();

                this.wdGeneralCal = new Views.wdGeneralCalibration(this);
                this.wdGeneralCal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //
                this.wdGeneralCal.Closed += WdGeneralCal_Closed;
                //
                this.ResetNewCalProcess();
            }));
        }

        private void WdGeneralCal_Closed(object sender, EventArgs e)
        {
            //Try to force end Calib Threading
            this.calibControlProcess.ForceEndCalibThread();
        }

        public void IniForCalibControlProcess()
        {
            //Ini for class binding
            if (this.calibControlProcess.calibBinding == null)
            {
                this.calibControlProcess.calibBinding = new CalibBinding();
                this.calibControlProcess.calibBinding.warning = "";
                this.calibControlProcess.calibBinding.userGuide = "";

                this.calibControlProcess.calibSetting = new CalibSetting();
                this.calibControlProcess.calibSetting.blRequestConfirm = true;
                //this.calibControlProcess.calibSetting.NumStepCalib = 0;
                this.calibControlProcess.lstCalPointInfo = new List<clsCalPointValueSetting>();
                for(int i=0;i<this.lstclsObjectiveCal.Count;i++)
                {
                    for(int j=0;j<this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count;j++)
                    {
                        this.calibControlProcess.lstCalPointInfo.Add(this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j]);
                    }
                }
            }
        }

        void btnUserControl_Click()
        {
            this.wdUserControlPanel = new Views.wdUserControl();
            this.wdUserControlPanel.Show();

            //Add Info
            int i = 0;
            for (i = 0; i < this.lstclsUserControlData.Count; i++)
            {
                this.lstclsUserControlData[i].grbUserControl = new System.Windows.Controls.GroupBox();
                this.lstclsUserControlData[i].btnUserControl = new System.Windows.Controls.Button();
                this.lstclsUserControlData[i].grbUserControl.Header = this.lstclsUserControlData[i].strNameUserControl;

                this.lstclsUserControlData[i].grbUserControl.Width = 360;
                this.lstclsUserControlData[i].grbUserControl.FontWeight = FontWeights.Bold;
                this.lstclsUserControlData[i].grbUserControl.BorderBrush = System.Windows.Media.Brushes.Black;
                this.lstclsUserControlData[i].grbUserControl.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsUserControlData[i].grbUserControl.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsUserControlData[i].grbUserControl.Margin = new Thickness(10, 10, 0, 0);

                this.lstclsUserControlData[i].btnUserControl.Width = 100;
                this.lstclsUserControlData[i].btnUserControl.Height = 30;
                this.lstclsUserControlData[i].btnUserControl.Background = System.Windows.Media.Brushes.LightGreen;
                this.lstclsUserControlData[i].btnUserControl.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsUserControlData[i].btnUserControl.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsUserControlData[i].btnUserControl.Margin = new Thickness(5, 5, 5, 5);
                this.lstclsUserControlData[i].btnUserControl.Content = this.lstclsUserControlData[i].strNameUserControl;
                this.lstclsUserControlData[i].btnUserControl.Click += btnUserControlHandle_Click;
                this.lstclsUserControlData[i].grbUserControl.Content = this.lstclsUserControlData[i].btnUserControl;
                //
                this.wdUserControlPanel.spnlContent.Children.Add(this.lstclsUserControlData[i].grbUserControl);
            }

        }

        void btnUserControlHandle_Click(object sender, RoutedEventArgs e)
        {
            //Looking for sender
            int i = 0;
            bool blFound = false;
            for(i=0;i<this.lstclsUserControlData.Count;i++)
            {
                if(sender == this.lstclsUserControlData[i].btnUserControl)
                {
                    blFound = true;
                    break;
                }
            }
            if (blFound == false) return;

            //
            //MessageBox.Show("Button " + (i + 1).ToString());
            if (this.lstclsUserControlData[i].objTarget == null) return;

            object objTemp = this.CallActualObject(this.lstclsUserControlData[i].objTarget,
                                                    this.lstclsUserControlData[i].strMethodName,
                                                    this.lstclsUserControlData[i].arrObjPara);
        }

        void btnValObserver_Click()
        {
            //Show Actual value observer
            this.wdAdjustedValObserver = new Views.wdValObserver();
            this.wdAdjustedValObserver.Show();

            //Add Info
            int i = 0;
            for(i=0;i<this.lstclsObjectiveCal.Count;i++)
            {
                this.lstclsObjectiveCal[i].grbUserSetting = new System.Windows.Controls.GroupBox();
                this.lstclsObjectiveCal[i].tbUserSetting = new System.Windows.Controls.TextBox();
                this.lstclsObjectiveCal[i].grbUserSetting.Header = this.lstclsObjectiveCal[i].strNameCalObject;

                this.lstclsObjectiveCal[i].grbUserSetting.Width = 360;
                this.lstclsObjectiveCal[i].grbUserSetting.FontWeight = FontWeights.Bold;
                this.lstclsObjectiveCal[i].grbUserSetting.BorderBrush = System.Windows.Media.Brushes.Black;
                this.lstclsObjectiveCal[i].grbUserSetting.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsObjectiveCal[i].grbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsObjectiveCal[i].grbUserSetting.Margin = new Thickness(10, 10, 0, 0);

                this.lstclsObjectiveCal[i].tbUserSetting.Width = 330;
                this.lstclsObjectiveCal[i].tbUserSetting.Background = System.Windows.Media.Brushes.LemonChiffon;
                this.lstclsObjectiveCal[i].tbUserSetting.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsObjectiveCal[i].tbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsObjectiveCal[i].tbUserSetting.Margin = new Thickness(5, 5, 5, 5);
                //this.lstclsUserCalData[i].tbUserSetting.Text = "";


                this.lstclsObjectiveCal[i].grbUserSetting.Content = this.lstclsObjectiveCal[i].tbUserSetting;
                //
                this.wdAdjustedValObserver.spnlContent.Children.Add(this.lstclsObjectiveCal[i].grbUserSetting);
                //
                this.wdAdjustedValObserver.btnStopContinue.Click += btnStopContinue_Click;
                this.wdAdjustedValObserver.btnOK.Click += BtnOK_Click;
            }

            //
            this.myTimer.Elapsed += myTimer_Elapsed;

            this.myTimer.Start();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.myTimer.Enabled = false;
            this.wdAdjustedValObserver.Close();
        }

        private void btnStopContinue_Click(object sender, RoutedEventArgs e)
        {
            this.myTimer.Enabled = !this.myTimer.Enabled;
            if(this.myTimer.Enabled)
            {
                this.wdAdjustedValObserver.btnStopContinue.Content = "STOP";
            }
            else
            {
                this.wdAdjustedValObserver.btnStopContinue.Content = "CONTINUE";
            }
        }

        /// <summary>
        /// For Real Data Observer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.myTimer.Stop();
            //Update Info
            for (int i = 0; i < this.lstclsObjectiveCal.Count; i++)
            {
                //1. Try to read actual X value first
                // Note: we have a proposed condition that: all cali points of a calibration objective use same object, same method, same parameter to get X raw data!
                //Try to reading X raw data
                object objXValueReading = new object();
                if (this.lstclsObjectiveCal[i].valueObserver == null) //No setting
                {
                    objXValueReading = this.CallActualObject(this.lstclsObjectiveCal[i].objMeasureObjectSource,
                                        this.lstclsObjectiveCal[i].lstclsCalPointValSetting[0].strMethodName, // 0: because all cali points of an objective cali use same function, same para of same object source
                                        this.lstclsObjectiveCal[i].lstclsCalPointValSetting[0].arrObjPara);

                }
                else //Separate setting for Value Observer
                {
                    objXValueReading = this.CallActualObject(this.lstclsObjectiveCal[i].valueObserver.objTarget,
                                        this.lstclsObjectiveCal[i].valueObserver.strMethodName, // 0: because all cali points of an objective cali use same function, same para of same object source
                                        this.lstclsObjectiveCal[i].valueObserver.arrObjPara);
                }

                object objRet = this.GetAdjustValue(i, objXValueReading);
                //
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.lstclsObjectiveCal[i].tbUserSetting.Text = objRet.ToString();
                }));
            }

            //
            this.myTimer.Start();
        }
     
        public System.Windows.Controls.MenuItem GetMenuItem()
        {
            return this.menuItemUserSetting;
        }

        void menuItemUserSetting_Click(object sender, RoutedEventArgs e)
        {
            this.ShowIni();
        }

        //
        public bool isGenericList(object objInput)
        {
            bool blRet = false;
            if (objInput.GetType().IsGenericType == true)
            {
                if (objInput.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    blRet = true;
                }
            }

            return blRet;
        }

        //Call method from Actual Object
        private static Mutex mutex = new Mutex();

        /// <summary>
        /// This function use to call a function inside an object to get actual X value
        /// </summary>
        /// <param name="objSource">The class instance (object) hold function need to call inside</param>
        /// <param name="objMethodName">The method name need to call to get X raw value</param>
        /// <param name="objPara">The parameters for method name to get X raw value</param>
        /// <returns></returns>
        public object CallActualObject(object objSource, object objMethodName, object[] objPara)
        {
            mutex.WaitOne();
            //
            string strMethodName = objMethodName.ToString().Trim();
            try
            {
                if (objSource.GetType().GetMethod(strMethodName) != null)
                {
                    return objSource.GetType().GetMethod(strMethodName).Invoke(objSource, objPara);
                }
                else
                {
                    return "CALL() Error: cannot find method [" + strMethodName + "] of object type [" + objSource.GetType().ToString() + "]";
                }
            }
            catch (Exception ex)
            {
                return "CALL() Error: " + ex.Message;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Assign value for Actual Calibration Object.
        /// We will call this object out when we need get actual value (AD, Voltage...) to calibration
        /// </summary>
        /// <param name="objInput">The object (class) contains function to take out value need to get. 
        /// Normally, it's child process (this()) to allow user define function directly on program list.  </param>
        /// <param name="objTargetID">The calibration can contains different multi calibration objective.
        /// Each objective assinged unique ID for identify. So, we need point out what ID of cal objective want to assigned.
        /// If objTargetID = -1, then we assign for all cal objective same object (class) to call out value</param>
        /// <returns></returns>
        public string AssignObject(object objInput, object objTargetID)
        {
            string strRet = "0";
            int i = 0;
            //
            int intTargetID = 0;
            if (int.TryParse(objTargetID.ToString().Trim(), out intTargetID) == false)
            {
                return "AssignObject() error: Not recognize Target ID" + objTargetID.ToString() + "!";
            }

            if (intTargetID == -1) //Assign for all object
            {
                //
                for (i = 0; i < this.lstclsObjectiveCal.Count; i++)
                {
                    this.lstclsObjectiveCal[i].objMeasureObjectSource = objInput;
                    for(int j =0;j<this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count;j++)
                    {
                        this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].objMeasureObjectSource = objInput;
                        this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].strNameCalObject = this.lstclsObjectiveCal[i].strNameCalObject;
                    }
                }
            }
            else //Assign for only 1 object
            {
                if ((intTargetID < -1) || (intTargetID > (this.lstclsObjectiveCal.Count - 1))) //invalid
                {
                    return "AssignObject() error: intTargetID" + objTargetID.ToString() + "is out of range!";
                }
                //If OK, then assign value
                this.lstclsObjectiveCal[intTargetID].objMeasureObjectSource = objInput;
                for (int j = 0; j < this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting.Count; j++)
                {
                    this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[j].objMeasureObjectSource = objInput;
                    this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[j].strNameCalObject = this.lstclsObjectiveCal[intTargetID].strNameCalObject;
                }
            }

            //
            return strRet;
        }

        /// <summary>
        /// Assign value for Actual Calibration Object
        /// </summary>
        /// <param name="objTargetID">The ID of Calibration Objective (Calibration can support multi-objective calibration)</param>
        /// <param name="objCalSettingID">Each calibration objective can contains some calibration points => we need point out ID of point which want to apply setting. If ID = -1 => all point same method! </param>
        /// <param name="objMethodName">The method name of assigned object (class) by AssignObject() to call out value</param>
        /// <param name="objPara">Each calibration points may need separate parameters => we assigned here</param>
        /// <returns></returns>
        public string AssignMethod(object objTargetID, object objCalSettingID, object objMethodName, object objPara)
        {
            string strRet = "0";
            int i, j = 0;
            //
            int intTargetID = 0;
            if (int.TryParse(objTargetID.ToString().Trim(), out intTargetID) == false)
            {
                return "AssignMethod() error: Target ID" + objTargetID.ToString() + " is not integer!";
            }

            int intCalSettingID = 0;
            if (int.TryParse(objCalSettingID.ToString().Trim(), out intCalSettingID) == false)
            {
                return "AssignMethod() error: Calibration Value Setting ID" + objCalSettingID.ToString() + " is not integer!";
            }

            //Check Parameter input
            //Check if input is an array?
            object[] ArrPara = new object[1];

            if (objPara != null)
            {
                Type typeTest = objPara.GetType();
                if (typeTest.IsArray == true) //Confirm object is an array input?
                {
                    ArrPara = (object[])objPara;
                }
                else //all other type, include List type => just treat as 1 object only, not array of Object!
                {
                    ArrPara = new object[1];
                    ArrPara[0] = objPara;
                }
            }


            //Assign Method & parameter
            if (intTargetID == -1) //Assign for all object using same method
            {
                //
                for (i = 0; i < this.lstclsObjectiveCal.Count; i++)
                {
                    if (intCalSettingID == -1) //Assign for all cali val setting
                    {
                        for (j = 0; j < this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count; j++)
                        {
                            this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].strMethodName = objMethodName.ToString().Trim();
                            if (objPara != null) this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].arrObjPara = ArrPara;
                        }
                    }
                    else //Apply for only 1 Cali Point Setting
                    {
                        if ((intCalSettingID < -1) || (intCalSettingID > this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count))
                        {
                            return "AssignMethod() error: Calibration Point Value Setting ID" + objCalSettingID.ToString() + " is out of range!";
                        }
                        //
                        this.lstclsObjectiveCal[i].lstclsCalPointValSetting[intCalSettingID].strMethodName = objMethodName.ToString().Trim();
                        if (objPara != null) this.lstclsObjectiveCal[i].lstclsCalPointValSetting[intCalSettingID].arrObjPara = ArrPara;
                    }
                }
            }
            else //Assign for only 1 object
            {
                if ((intTargetID < -1) || (intTargetID > (this.lstclsObjectiveCal.Count - 1))) //invalid
                {
                    return "AssignMethod() error: intTargetID" + objTargetID.ToString() + "is out of range!";
                }
                //If OK, then assign value
                if (intCalSettingID == -1) //Assign for all cali val setting
                {
                    for (j = 0; j < this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting.Count; j++)
                    {
                        this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[j].strMethodName = objMethodName.ToString().Trim();
                        if (objPara != null) this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[j].arrObjPara = ArrPara;
                    }
                }
                else //Apply for only 1 Cali Point Setting
                {
                    if ((intCalSettingID < -1) || (intCalSettingID > this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting.Count))
                    {
                        return "AssignMethod() error: Calibration Point Value Setting ID" + objCalSettingID.ToString() + " is out of range!";
                    }
                    //
                    this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[intCalSettingID].strMethodName = objMethodName.ToString().Trim();
                    if (objPara != null) this.lstclsObjectiveCal[intTargetID].lstclsCalPointValSetting[intCalSettingID].arrObjPara = ArrPara;
                }
            }

            //
            return strRet;
        }

        /// <summary>
        /// Getting adjusted value which already included calibration calculate 
        ///  Ex: VE DH BK Negative Pressure = Gain*AD_Reading_Value + Offset
        /// </summary>
        /// <param name="ObjectiveID">The calibration objective ID, count from 0 (Ex: VE BK Negative pressure,VE CL Negative pressure, KMK value...)</param>
        /// <param name="objXRawData"> Input Raw data (X value) and from that raw data, we need to calculate Adjusted value (Y value)</param>
        /// <returns></returns>
        public object GetAdjustValue(object ObjectiveID, object objXRawData)
        {
            // How to get adjusted value (actual value + calibration => adjusted value)
            //  1. Since we do calibration => we already identify properties relation:
            //      + y = Gain1*x + Offset1 (if x belong to [-∞,x1])
            //      + y = Gain2*x + Offset2 (if x belong to [x1,x2])
            //      + y = Gain3*x + Offset3 (if x belong to [x2,x3])
            //      + ...
            //  2. So, we need to save each value [x1,x2,x3...] (x1<x2<x3...),
            //     Then, each time we get value x and need to calculate coressponding y, 
            //     We need identify x belong to what range of value and use right formular with right (Gain, Offset) value
            //          [y = Gain*x + Offset]
            //

            //First do inputs validations
            int intObjectiveID = 0;
            double dblXRawData = 0;

            if (int.TryParse(ObjectiveID.ToString(), out intObjectiveID) == false) return "Error: Input Object ID [" + ObjectiveID.ToString() + "] is not integer!";
            if (double.TryParse(objXRawData.ToString(), out dblXRawData) == false) return "Error: Input raw data (X value) - [" + objXRawData.ToString() + "] is not numeric!";

            //Return value
            object objRet = new object();

            ////1. Try to read actual X value first
            //// Note: we have a proposed condition that: all cali points of a calibration objective use same object, same method, same parameter to get X raw data!
            ////Try to reading X raw data
            //object objXValue = this.CallActualObject(this.lstclsObjectiveCal[intObjectiveID].objMeasureObjectSource,
            //                        this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[0].strMethodName, // 0: because all cali points of an objective cali use same function, same para of same object source
            //                        this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[0].objPara);

            ////Check value is numeric (OK) or not
            //double dblXValue = 0;
            //if (double.TryParse(objXValue.ToString(), out dblXValue) == false) return "Error: X raw data reading fail with return value [" + objXValue.ToString() + "]";




            //2. OK, now we need to identify with X raw value like above, what kind of formular of proper range need to apply to get adjusted value
            //   On the other hand, we find correct (gain, offset) value (1 objective calibration like KMK can has more than 1 (gain, offset) couple - cali at 3 points)
            //   Note:
            //      - The direction of cali point can be reverse. For example 0Kpa (point 1) > -17Kpa (point 2)
            //        => we need to identify "direction" of this ordering before do searching
            bool blDirection = true; //Default is from smallest to biggest (true), else => false
            if (this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[0].dblXRawValue > this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1].dblXRawValue)
            {
                blDirection = false;
            }

            //Do searching
            int i = 0;
            //double dblGainResult = 1;
            //double dblOffsetResult = 0;
            clsLinearModeCalResult clsGainOffsetResult = new clsLinearModeCalResult();
            int indexResult = 0;

            if (blDirection==true) //Smallest to biggest
            {
                if(dblXRawData <= this.lstclsObjectiveCal[intObjectiveID]
                    .lstclsCalPointValSetting[0].dblXRawValue) //Out of range and smaller than all value
                {
                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult[0]; //Use first range
                }
                else if(dblXRawData >= this.lstclsObjectiveCal[intObjectiveID]
                    .lstclsCalPointValSetting[this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1]
                    .dblXRawValue)//Out of range and Bigger than all value
                {
                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID]
                        .lstclsLinearModeCalResult[this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult.Count - 1]; //Use last range
                }
                else //Need to find correct range
                {
                    //double startSearchX = this.lstclsObjectiveCal[intObjectiveID]
                    //.lstclsCalPointValSetting[0].dblXRawValue; //Search from smallest to biggest
                    
                    for(i=0;i< this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count;i++)
                    {
                        if(i==(this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count-1)) //last item, no need search anymore
                        {
                            indexResult = i;
                            break;
                        }

                        //
                        if ((dblXRawData >= this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[i].dblXRawValue)&&
                                (dblXRawData <= this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[i+1].dblXRawValue)) //Found
                        {
                            indexResult = i;
                            break; //No need search more
                        }
                    }

                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult[indexResult];
                }
            }
            else //Biggesst to smallest
            {
                if (dblXRawData >= this.lstclsObjectiveCal[intObjectiveID]
                                .lstclsCalPointValSetting[0].dblXRawValue) //Out of range and Bigger than all value
                {
                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult[0]; //Use first range
                }
                else if (dblXRawData <= this.lstclsObjectiveCal[intObjectiveID]
                    .lstclsCalPointValSetting[this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1]
                    .dblXRawValue)//Out of range and Smaller than all value
                {
                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID]
                        .lstclsLinearModeCalResult[this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult.Count - 1]; //Use last range
                }
                else //Need to find correct range
                {
                    //double startSearchX = this.lstclsObjectiveCal[intObjectiveID]
                    //.lstclsCalPointValSetting[0].dblXRawValue; //Search from biggest to smallest

                    for (i = 0; i < this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count; i++)
                    {
                        if (i == (this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1)) //last item, no need search anymore
                        {
                            indexResult = i;
                            break;
                        }

                        //
                        if ((dblXRawData <= this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[i].dblXRawValue) &&
                                (dblXRawData >= this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[i + 1].dblXRawValue)) //Found
                        {
                            indexResult = i;
                            break; //No need search more
                        }
                    }

                    clsGainOffsetResult = this.lstclsObjectiveCal[intObjectiveID].lstclsLinearModeCalResult[indexResult];
                }
            }

            //3. OK, now we got (Gain, Offset) value => do calculate for Adjust value (Y=Gain*X + Offset)
            objRet = clsGainOffsetResult.dblGainValue * dblXRawData + clsGainOffsetResult.dblOffsetValue;

            //
            return objRet;
        }

        /// <summary>
        /// Assign control utilities for Calibration
        /// For example when do calibration we need some action like control solenoid valve...
        /// By Adding user control(button) on calibration form, we allow user to do desired actions.
        /// </summary>
        /// <param name="objControlID"></param>
        /// <param name="objControlObject"></param>
        /// <param name="objMethodName"></param>
        /// <param name="objPara"></param>
        /// <returns></returns>
        public string AssignControl(object objControlID, object objControlObject, object objMethodName, object objPara)
        {
            string strRet = "0";
            //
            int intControlID = 0;
            if (int.TryParse(objControlID.ToString().Trim(), out intControlID) == false)
            {
                return "AssignControl() error: Control ID" + objControlID.ToString() + " is not integer!";
            }
            if (intControlID > (this.lstclsUserControlData.Count - 1))
            {
                return "AssignControl() error: Control ID input out of range! Maximum value = " + (this.lstclsUserControlData.Count - 1).ToString() + "!";
            }

            //Check Parameter input
            //Check if input is an array?
            object[] ArrPara = new object[1];

            if (objPara != null)
            {
                Type typeTest = objPara.GetType();
                if (typeTest.IsArray == true) //Confirm object is an array input?
                {
                    ArrPara = (object[])objPara;
                }
                else //all other type, include List type => just treat as 1 object only, not array of Object!
                {
                    ArrPara = new object[1];
                    ArrPara[0] = objPara;
                }
            }

            //Assign object control
            if (objControlObject == null)
            {
                return "AssignControl() error: object control input is null!";
            }
            this.lstclsUserControlData[intControlID].objTarget = objControlObject;

            //Assign Method name
            if (objMethodName == null)
            {
                return "AssignControl() error: method name input is null!";
            }
            this.lstclsUserControlData[intControlID].strMethodName = objMethodName.ToString().Trim();

            //Assign Parameter
            if (objPara != null) this.lstclsUserControlData[intControlID].arrObjPara = ArrPara;

            //
            return strRet;
        }

        /// <summary>
        /// Separate setting for Value Observer class
        /// Because maybe in Observable mode need to use another function to take & display compare with when Calibration mode.
        /// </summary>
        /// <param name="objCaliObjectiveID">ID of calibration Objective</param>
        /// <param name="objControlObject">Source Object</param>
        /// <param name="objMethodName">Method Name</param>
        /// <param name="objPara">Array of Object parameters</param>
        /// <returns></returns>
        public string AssignObserver(object objCaliObjectiveID, object objControlObject, object objMethodName, object objPara)
        {
            string strRet = "0";
            //
            int intCaliObjectiveID = 0;
            if (int.TryParse(objCaliObjectiveID.ToString().Trim(), out intCaliObjectiveID) == false)
            {
                return "AssignObserver() error: CaliObjectiveID" + objCaliObjectiveID.ToString() + " is not integer!";
            }
            if (intCaliObjectiveID > (this.lstclsObjectiveCal.Count - 1))
            {
                return "AssignObserver() error: CaliObjectiveID input out of range! Maximum value = " + (this.lstclsObjectiveCal.Count - 1).ToString() + "!";
            }

            //Ini
            this.lstclsObjectiveCal[intCaliObjectiveID].valueObserver = new ValueObserver();

            //Check Parameter input
            //Check if input is an array?
            object[] ArrPara = new object[1];

            if (objPara != null)
            {
                Type typeTest = objPara.GetType();
                if (typeTest.IsArray == true) //Confirm object is an array input?
                {
                    ArrPara = (object[])objPara;
                }
                else //all other type, include List type => just treat as 1 object only, not array of Object!
                {
                    ArrPara = new object[1];
                    ArrPara[0] = objPara;
                }
            }

            //Assign Parameter
            if (objPara != null) this.lstclsObjectiveCal[intCaliObjectiveID].valueObserver.arrObjPara = ArrPara;

            //Assign object control
            if (objControlObject == null)
            {
                return "AssignObserver() error: object control input is null!";
            }
            this.lstclsObjectiveCal[intCaliObjectiveID].valueObserver.objTarget = objControlObject;

            //Assign Method name
            if (objMethodName == null)
            {
                return "AssignObserver() error: method name input is null!";
            }
            this.lstclsObjectiveCal[intCaliObjectiveID].valueObserver.strMethodName = objMethodName.ToString().Trim();

            //
            return strRet;
        }

        /// <summary>
        /// Assign Spec (Y value) for Confirm Calibration result 
        /// </summary>
        /// <param name="ObjectiveID">ID of Calib Objective</param>
        /// <param name="CalPointID"> ID of Calib Point of Objective</param>
        /// <param name="LowSpec"> Low Spec setting</param>
        /// <param name="HiSpec">High Spec setting</param>
        /// <returns></returns>
        public object AssignConfirmSpec(object ObjectiveID, object CalPointID, object LowSpec, object HiSpec)
        {
            string strRet = "0";
            //First do inputs validations
            int intObjectiveID = 0;
            if (int.TryParse(ObjectiveID.ToString(), out intObjectiveID) == false) return "Error: Input Object ID [" + ObjectiveID.ToString() + "] is not integer!";
            if (intObjectiveID > this.lstclsObjectiveCal.Count - 1) return "AssignConfirmSpec() Error: Objective ID is out of range!";

            int intCalPointID = 0;
            if (int.TryParse(CalPointID.ToString(), out intCalPointID) == false) return "Error: Input Calib Point ID [" + CalPointID.ToString() + "] is not integer!";
            if (intCalPointID > this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1) return "AssignConfirmSpec() Error: Calib Point ID is out of range!";

            double dblLowSpec = 0;
            if (double.TryParse(LowSpec.ToString(), out dblLowSpec) == false) return "Error: Low spec setting [" + LowSpec.ToString() + "] is not numeric!";

            double dblHiSpec = 0;
            if (double.TryParse(HiSpec.ToString(), out dblHiSpec) == false) return "Error: Hi spec setting [" + HiSpec.ToString() + "] is not numeric!";

            //Setting spec if everything is OK
            this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[intCalPointID].dblCaliConfirmLowSpecY = dblLowSpec;
            this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[intCalPointID].dblCaliConfirmHiSpecY = dblHiSpec;

            //
            return strRet;
        }

        /// <summary>
        /// Assign Spec (X value) for Calibration process 
        /// </summary>
        /// <param name="ObjectiveID">ID of Calib Objective</param>
        /// <param name="CalPointID">ID of Calib Point</param>
        /// <param name="LowSpec">Low Spec for X raw data</param>
        /// <param name="HiSpec">Hi Spec for X raw data</param>
        /// <returns></returns>
        public object AssignCalibSpec(object ObjectiveID, object CalPointID, object LowSpec, object HiSpec)
        {
            string strRet = "0";
            //First do inputs validations
            int intObjectiveID = 0;
            if (int.TryParse(ObjectiveID.ToString(), out intObjectiveID) == false) return "Error: Input Object ID [" + ObjectiveID.ToString() + "] is not integer!";
            if (intObjectiveID > this.lstclsObjectiveCal.Count - 1) return "AssignConfirmSpec() Error: Objective ID is out of range!";

            int intCalPointID = 0;
            if (int.TryParse(CalPointID.ToString(), out intCalPointID) == false) return "Error: Input Calib Point ID [" + CalPointID.ToString() + "] is not integer!";
            if (intCalPointID > this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting.Count - 1) return "AssignConfirmSpec() Error: Calib Point ID is out of range!";

            double dblLowSpec = 0;
            if (double.TryParse(LowSpec.ToString(), out dblLowSpec) == false) return "Error: Low spec setting [" + LowSpec.ToString() + "] is not numeric!";

            double dblHiSpec = 0;
            if (double.TryParse(HiSpec.ToString(), out dblHiSpec) == false) return "Error: Hi spec setting [" + HiSpec.ToString() + "] is not numeric!";

            //Setting spec if everything is OK
            this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[intCalPointID].dblCaliLowSpecXRawData = dblLowSpec;
            this.lstclsObjectiveCal[intObjectiveID].lstclsCalPointValSetting[intCalPointID].dblCaliHiSpecXRawData = dblHiSpec;

            //
            return strRet;
        }

        //************************************************************************************************
        public object AssignPLFunction(object objControlObject, object objMethodName, object objPara)
        {
            string strRet = "0";

            //Check Parameter input
            //Check if input is an array?
            object[] ArrPara = new object[1];

            if (objPara != null)
            {
                Type typeTest = objPara.GetType();
                if (typeTest.IsArray == true) //Confirm object is an array input?
                {
                    ArrPara = (object[])objPara;
                }
                else //all other type, include List type => just treat as 1 object only, not array of Object!
                {
                    ArrPara = new object[1];
                    ArrPara[0] = objPara;
                }
            }

            //Assign object control
            if (objControlObject == null)
            {
                return "AssignPLFunction() error: object control input is null!";
            }
            this.objProcess = objControlObject;

            //Assign Method name
            if (objMethodName == null)
            {
                return "AssignPLFunction() error: method name input is null!";
            }
            this.PLFuncName = objMethodName.ToString().Trim();

            //Assign Parameter
            if (objPara != null) this.objArrPara = ArrPara;

            //
            return strRet;
        }
        
        //************************************************************************************************
        /// <summary>
        /// Calculate Gain & Offset value
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="gain"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string CalculateCaliGainAndOffset(double x1, double y1, double x2, double y2, ref double gain, ref double offset)
        {
            // y1= a*x1 + b
            // y2 = a*x2 + b
            // (a: Gain, b: Offset)
            // => b = y2 - a*x2
            //    y1 = a*x1 + y2 - a*x2
            //    y1 - y2 = a*(x1 - x2)
            //    If(x1 == x2) => If(y1!=y2) => There is no result
            //                 => if(y1==y2) => there are infinitive number of result
            //Example
            //   Pressure = Gain*Volt + Offset;
            //   -17 = Gain*Volt1 + Offset; //Volt1 is actual voltage of sensor when measure at cali point -17 Kpa
            //    0 = Gain*Volt2 + Offset; //Volt2 is actual voltage of sensor when measure at cali point 0 Kpa

            string strRet = "0";
            //
            if (x1 == x2) return "CalculateCaliGainAndOffset Error: cannot find any result because x1 = x2";

            //
            gain = (y1 - y2) / (x1 - x2);
            offset = y1 - gain * x1;

            //
            return strRet;
        }

        /// <summary>
        /// Saving Calib Result (When calib result already OK)
        /// </summary>
        public void SavingCalibResult()
        {
            int i, j = 0;
            long lngret = 0;
            string strTimeCalib = "";
            DateTime now = DateTime.Now;

            for (i = 0; i < this.lstclsObjectiveCal.Count; i++)
            {
                this.lstclsObjectiveCal[i].blCaliResult = true;

                int intNumCoupleResult = this.lstclsObjectiveCal[i].intNumberCalValue - 1;
                for (j = 0; j < intNumCoupleResult; j++)
                {
                    string strRet = "";
                    double gain = 0;
                    double offset = 0;
                    double y1 = this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].dblYCalValue;
                    double x1 = this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].dblXRawValue;
                    double y2 = this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j + 1].dblYCalValue;
                    double x2 = this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j + 1].dblXRawValue;

                    strRet = this.CalculateCaliGainAndOffset(x1, y1, x2, y2, ref gain, ref offset);
                    if (strRet == "0") //Cal OK
                    {
                        this.lstclsObjectiveCal[i].lstclsLinearModeCalResult[j].dblGainValue = gain;
                        this.lstclsObjectiveCal[i].lstclsLinearModeCalResult[j].dblOffsetValue = offset;

                        //MessageBox.Show("Gain = " + gain.ToString() + ". Offset = " + offset.ToString());

                        //Saving data to Calibration ini file
                        //Cal Item name to save data
                        string strGainName = "CalObjectGainValue";
                        string strOffsetName = "CalObjectOffsetValue";

                        if (intNumCoupleResult <= 1)
                        {
                            strGainName = strGainName + (i + 1).ToString();
                            strOffsetName = strOffsetName + (i + 1).ToString();
                        }
                        else
                        {
                            strGainName = strGainName + (i + 1).ToString() + "_" + (j + 1).ToString();
                            strOffsetName = strOffsetName + (i + 1).ToString() + "_" + (j + 1).ToString();
                        }

                        //
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.strCaliFilePath, "CAL_RESULT", strGainName, this.lstclsObjectiveCal[i].lstclsLinearModeCalResult[j].dblGainValue.ToString());
                        lngret = MyLibrary.WriteFiles.IniWriteValue(this.strCaliFilePath, "CAL_RESULT", strOffsetName, this.lstclsObjectiveCal[i].lstclsLinearModeCalResult[j].dblOffsetValue.ToString());
                    }
                    else //NG
                    {
                        this.lstclsObjectiveCal[i].blCaliResult = false;
                        MessageBox.Show("Calibration for " + this.lstclsObjectiveCal[i].strNameCalObject + " is NG. Cannot calculate Gain & Offset data. Return message: " + strRet, "Calibration Fail");
                    }
                }

                //Update last time cali for each object if result is OK
                if (this.lstclsObjectiveCal[i].blCaliResult == true)
                {
                    strTimeCalib = "LastCalTime" + (i + 1).ToString();
                    now = DateTime.Now;
                    lngret = MyLibrary.WriteFiles.IniWriteValue(this.strCaliFilePath, "CAL_TIMING", strTimeCalib, now.ToString());
                }

                //Save CalObjectXRawValue for each cali point
                string strXRawValues = "";
                for (j = 0; j < this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count; j++)
                {
                    strXRawValues = strXRawValues + this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].dblXRawValue.ToString();
                    if (j != this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count - 1)
                    {
                        strXRawValues = strXRawValues + ";";
                    }
                }
                //
                lngret = MyLibrary.WriteFiles.IniWriteValue(
                        this.strCaliFilePath,
                        "CAL_RESULT",
                        "CalObjectXRawValue" + (i + 1).ToString(),
                        strXRawValues
                    );
            }

            //If all calibration of all object is OK, then we save last time cali for all process
            //Only saving if no Confirm after calib require!
            if(this.calibControlProcess.calibSetting.blRequestConfirm == false)
            {
                strTimeCalib = "LastCalTime";
                now = DateTime.Now;
                lngret = MyLibrary.WriteFiles.IniWriteValue(
                    this.strCaliFilePath,
                    "CAL_TIMING", strTimeCalib,
                    now.ToString());

                //Clear warning message content
                this.calibControlProcess.calibBinding.warning = "";
                this.calibControlProcess.calibBinding.userGuide = "Calibration Finish!";
            }
            else //Need confirm again calibration value
            {
                //Clear warning message content
                this.calibControlProcess.calibBinding.userGuide = "Please confirm again result. Click 'CONFIRM' button to start...";
            }
            
            OnPropertyChanged("calibControlProcess");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CalPointID"></param>
        /// <returns></returns>
        public int FindCalObjectiveID (int CalPointID)
        {
            int index = 0;
            for(int i =0;i<this.lstclsObjectiveCal.Count;i++)
            {
                for(int j =0;j<this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count;j++)
                {
                    if(index==CalPointID)
                    {
                        return i;
                    }
                    index++;
                }
            }
            return -1;
        }

        //*****************************BUTTON CLICK HANDLE*******************************************
        public bool btnCalibAgainEnable { get; set; }
        public ICommand btnCalibAgainCommand { get; set; }
        public void btnCalibAgainCommandHandle()
        {
            //Disable Button
            this.btnCalibAgainEnable = false;
            OnPropertyChanged("btnCalibAgainEnable");

            //Reset calib binding class
            this.calibControlProcess.calibBinding.valueSetting = "";
            this.calibControlProcess.calibBinding.actualValue = "";
            this.calibControlProcess.calibBinding.message = "";
            this.calibControlProcess.blNextPress = false;
            this.calibControlProcess.blSetPress = false;

            //Reset last result calib of all step
            foreach(var item in this.calibControlProcess.lstCalPointInfo)
            {
                item.blCalibResult = false;
            }

            //Enable for some button need to use when calibration
            this.btnSetEnable = true; OnPropertyChanged("btnSetEnable");
            this.btnNextEnable = true; OnPropertyChanged("btnNextEnable");
            this.btnBackEnable = true; OnPropertyChanged("btnBackEnable");

            //Disable some button should not use when do calibration
            this.btnConfirmEnable = false; OnPropertyChanged("btnConfirmEnable");
            this.btnValObserverEnable = false; OnPropertyChanged("btnValObserverEnable");
            this.btnUserControlEnable = false; OnPropertyChanged("btnUserControlEnable");

            //Update change on View
            OnPropertyChanged("calibControlProcess");
            //
            this.calibControlProcess.StartCalibrationThread();
        }

        public bool btnSetEnable { get; set; }
        public ICommand btnSetCommand { get; set; }
        public void btnSetCommandHandle()
        {
            this.calibControlProcess.blSetPress = true;
        }

        public bool btnNextEnable { get; set; }
        public ICommand btnNextCommand { get; set; }
        public void btnNextCommandHandle()
        {
            this.calibControlProcess.blNextPress = true;
        }

        public bool btnBackEnable { get; set; }
        public ICommand btnBackCommand { get; set; }
        public void btnBackCommandHandle()
        {
            this.calibControlProcess.blBackPress = true;
        }

        public bool btnOkEnable { get; set; }
        public ICommand btnOkCommand { get; set; }
        public void btnOkCommandHandle()
        {
            this.calibControlProcess.blOKPress = true;
        }

        public bool btnValObserverEnable { get; set; }
        public ICommand btnValObserverCommand { get; set; }
        public void btnValObserverCommandHandle()
        {
            this.btnValObserver_Click();
        }

        public bool btnUserControlEnable { get; set; }
        public ICommand btnUserControlCommand { get; set; }
        public void btnUserControlCommandHandle()
        {
            this.btnUserControl_Click();
        }

        public bool btnConfirmEnable { get; set; }
        public ICommand btnConfirmCommand { get; set; }
        public void btnConfirmCommandHandle()
        {
            //Disable Button
            this.btnConfirmEnable = false; OnPropertyChanged("btnConfirmEnable");

            //Reset calib binding class
            this.calibControlProcess.calibBinding.valueSetting = "";
            this.calibControlProcess.calibBinding.actualValue = "";
            this.calibControlProcess.calibBinding.message = "";
            this.calibControlProcess.blNextPress = false;
            this.calibControlProcess.blSetPress = false;

            //Reset last result confirm of all step
            foreach (var item in this.calibControlProcess.lstCalPointInfo)
            {
                item.blConfirmResult = false;
            }

            //Enable for some button need to use when calibration
            this.btnSetEnable = true; OnPropertyChanged("btnSetEnable");
            this.btnNextEnable = true; OnPropertyChanged("btnNextEnable");
            this.btnBackEnable = true; OnPropertyChanged("btnBackEnable");

            //Disable some button should not use when do calibration
            this.btnCalibAgainEnable = false; OnPropertyChanged("btnCalibAgainEnable");
            this.btnValObserverEnable = false; OnPropertyChanged("btnValObserverEnable");
            this.btnUserControlEnable = false; OnPropertyChanged("btnUserControlEnable");

            //Update change on View
            OnPropertyChanged("calibControlProcess");
            //
            this.calibControlProcess.StartConfirmThread();
        }

        //*******************************************************************************************
        public void UpdateChangeFromModel(object s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CalibCompleted")
            {
                //Need waiting user press OK Button
                this.btnSetEnable = false; OnPropertyChanged("btnSetEnable");
                this.btnNextEnable = false; OnPropertyChanged("btnNextEnable");
                this.btnBackEnable = false; OnPropertyChanged("btnBackEnable");
                this.btnOkEnable = true; OnPropertyChanged("btnOkEnable");
            }
            else if (e.PropertyName == "CalibFinishTask")
            {
                //Restore ini state for buttons
                this.IniButtonState();
                //
                //Confirm saving result if ok
                bool blTotalCalibResult = true;
                for (int i = 0; i < this.lstclsObjectiveCal.Count; i++)
                {
                    for (int j = 0; j < this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count; j++)
                    {
                        if (this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].blCalibResult == false)
                        {
                            blTotalCalibResult = false;
                            break;
                        }
                    }
                }
                if(blTotalCalibResult==true)
                {
                    //Saving Calibration Result
                    this.SavingCalibResult();
                }
            }
            else if (e.PropertyName == "ConfirmCompleted")
            {
                //Need waiting user press OK Button
                this.btnSetEnable = false; OnPropertyChanged("btnSetEnable");
                this.btnNextEnable = false; OnPropertyChanged("btnNextEnable");
                this.btnBackEnable = false; OnPropertyChanged("btnBackEnable");
                this.btnOkEnable = true; OnPropertyChanged("btnOkEnable");
            }
            else if(e.PropertyName == "ConfirmFinishTask")
            {
                //Confirm saving result if ok
                bool blTotalConfirmResult = true;
                for(int i = 0;i<this.lstclsObjectiveCal.Count;i++)
                {
                    for(int j=0;j<this.lstclsObjectiveCal[i].lstclsCalPointValSetting.Count;j++)
                    {
                        if(this.lstclsObjectiveCal[i].lstclsCalPointValSetting[j].blConfirmResult==false)
                        {
                            blTotalConfirmResult = false;
                            break;
                        }
                    }
                }
                //
                if(blTotalConfirmResult == true)
                {
                    //Saving confirm timing (Calibration last time)
                    string strTimeCalib = "LastCalTime";
                    DateTime now = DateTime.Now;
                    long lngret = MyLibrary.WriteFiles.IniWriteValue(
                        this.strCaliFilePath,
                        "CAL_TIMING", strTimeCalib,
                        now.ToString());
                    //Clear warning message
                    this.calibControlProcess.calibBinding.warning = "";
                    //Return Ini state for button
                    this.IniButtonState();
                    //Close openning Request Calib Message
                    this.CloseRequestCalibWindow();
                }

                //Restore ini state for buttons
                this.IniButtonState();
                //Inform everything is finish
                this.calibControlProcess.calibBinding.userGuide = "Confirm process finish!";
            }

            OnPropertyChanged("calibControlProcess");
        }

        //Constructor
        public clsGeneralCalibration()
        {
            this.wdAdjustedValObserver = new Views.wdValObserver();
            this.wdUserControlPanel = new Views.wdUserControl();
            this.wdRequestCalib = new Views.wdRequestCalib(this);
            this.RequestMessage = new ReactiveProperty<string>();
            this.RequestMessageColor = new ReactiveProperty<System.Windows.Media.SolidColorBrush>(System.Windows.Media.Brushes.Red);
            this.ShowCalibFormCommand = new ReactiveCommand();
            this.ShowCalibFormCommand.Subscribe(_ =>
            {
                // MessageBox.Show("Hello!");
                this.ShowCalibForm();
            });

            //Menu Item
            this.menuItemUserSetting = new System.Windows.Controls.MenuItem();
            this.menuItemUserSetting.Header = "General Calibration";
            this.menuItemUserSetting.Click += menuItemUserSetting_Click;

            this.calibControlProcess = new CalibControlProcess(this);
            this.lstclsObjectiveCal = new List<clsObjectiveCali>();
            this.lstclsUserControlData = new List<clsUserControlData>();
            this.intStepCalibration = 1;
            this.strCaliFilePath = "";
            this.blTotalCalibResult = false;
            //
            this.myTimer = new System.Timers.Timer();
            this.myTimer.Interval = 100; //Default is 100ms
            //
            this.btnCalibAgainCommand = new DelegateCommand(this.btnCalibAgainCommandHandle);
            this.btnSetCommand = new DelegateCommand(this.btnSetCommandHandle);
            this.btnNextCommand = new DelegateCommand(this.btnNextCommandHandle);
            this.btnBackCommand = new DelegateCommand(this.btnBackCommandHandle);
            this.btnConfirmCommand = new DelegateCommand(this.btnConfirmCommandHandle);
            this.btnOkCommand = new DelegateCommand(this.btnOkCommandHandle);
            this.btnValObserverCommand = new DelegateCommand(this.btnValObserverCommandHandle);
            this.btnUserControlCommand = new DelegateCommand(this.btnUserControlCommandHandle);

            this.btnCalibAgainEnable = true; //Initialization enable for "Calib Again" button
            this.btnValObserverEnable = true;
            this.btnUserControlEnable = true;

            //Add notify change event handle
            this.calibControlProcess.PropertyChanged += (s, e) => { UpdateChangeFromModel(s, e); };
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The class - Representative of each calibration objective like: 
    /// VE BK Negative pressure, VE CL Negative pressure, KMK Value...
    /// </summary>
    public class clsObjectiveCali
    {
        //For Get actual data
        public object objMeasureObjectSource { get; set; } //Actual object of each calibration item where X raw value get from
                                                        // Normally this is Child Process itself in CFP - So user can call functions to do calibration directly on Program List

        //For Calibration setting & Result
        public string strNameCalObject { get; set; } //BK negative pressure - CL negative pressure - KMK Value...
        public int intNumberCalValue { get; set; } //With each cal object need how many point need to do cali 
                                                    //(BK negative pressure (2): 0 & -17 Kpa. CL negative pressure (2): 0 & -22 Kpa. KMK (3): -200 & 0 & +200 um
        public List<clsCalPointValueSetting> lstclsCalPointValSetting { get; set; } //Method name, parameters here...
        public List<clsLinearModeCalResult> lstclsLinearModeCalResult { get; set; } // set of (Gain, Offset) result
        public bool blCaliResult { get; set; }

        //For Adjusted value observer
        public System.Windows.Controls.GroupBox grbUserSetting { get; set; }
        public System.Windows.Controls.TextBox tbUserSetting { get; set; }
        public ValueObserver valueObserver { get; set; }

        //Constructor
        public clsObjectiveCali()
        {
            this.objMeasureObjectSource = new object();
            this.objMeasureObjectSource = "";

            this.lstclsCalPointValSetting = new List<clsCalPointValueSetting>();
            this.lstclsLinearModeCalResult = new List<clsLinearModeCalResult>();

            this.blCaliResult = false;

            //
            this.grbUserSetting = new System.Windows.Controls.GroupBox();
            this.tbUserSetting = new System.Windows.Controls.TextBox();

            //Adjust position
            this.grbUserSetting.Width = 360;
            this.grbUserSetting.FontWeight = FontWeights.Bold;
            this.grbUserSetting.BorderBrush = System.Windows.Media.Brushes.Black;
            this.grbUserSetting.VerticalAlignment = VerticalAlignment.Top;
            this.grbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
            this.grbUserSetting.Margin = new Thickness(10, 10, 0, 0);

            this.tbUserSetting.Width = 330;
            this.tbUserSetting.Background = System.Windows.Media.Brushes.LemonChiffon;
            this.tbUserSetting.VerticalAlignment = VerticalAlignment.Top;
            this.tbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
            this.tbUserSetting.Margin = new Thickness(5, 5, 5, 5);
        }
    }

    public class clsLinearModeCalResult
    { 
        //Calibration result
        // y = ax + b
        //      a: Gain
        //      b: Offset
        public double dblGainValue { get; set; }
        public double dblOffsetValue { get; set; }

        public clsLinearModeCalResult()
        {
            this.dblGainValue = 1; //Default value
            this.dblOffsetValue = 0; //Default value
        }
    }

    /// <summary>
    /// Note with each Calibration point of an objective, we need a condition that
    ///         "All calibration points of an objective use same object, method name & parameters to get X raw data"
    /// For example with KMK: -200, 0, +200 points, but all use common function of a class to get X raw data (sensor value)
    /// </summary>
    public class clsCalPointValueSetting
    {
        public string strNameCalObject { get; set; } //Name of Calibration which Cali Point belong to
        //For Get actual data
        public object objMeasureObjectSource { get; set; } //Actual object of each calibration item where X raw value get from
                                                           // Normally this is Child Process itself in CFP - So user can call functions to do calibration directly on Program List
        //
        public string strMethodName { get; set; } //When need to get value from actual sensor, this Method name will be call
        public object[] arrObjPara; //This is parameter array passing to Method name when calling from actual object

        //For calibration guide
        public string strCaliUserGuide { get; set; } //With each step of calibration, we need display info to guide user how to do (Optional)
        public double dblYCalValue { get; set; } //The value of Y (y=ax+b), Y: can be input from Master (while calibration) or through calculate from X raw data & calibration result (in normal run time)
        public double dblXRawValue { get; set; } //The actual raw value of X (y=ax+b)

        //Marking Calib result of each step
        public bool blCalibResult { get; set; } //Indicate result of this calibration point is success or fail

        //Setting spec for cali point
        //Spec when do calibration
        public double dblCaliLowSpecXRawData { get; set; } //Low spec for X raw data reading
        public double dblCaliHiSpecXRawData { get; set; } //Hi spec for X raw data reading

        //Marking Confirm result of each step
        public bool blConfirmResult { get; set; } //Indicate result of this calibration point is success or fail

        //Spec when confirm again after calibration
        public double dblCaliConfirmLowSpecY { get; set; } //Low spec for Y when do confirm (compare with setting cali point value)
        public double dblCaliConfirmHiSpecY { get; set; } //Hi spec for Y when do confirm (compare with setting cali point value)

        public void ResetNewCal()
        {
            this.blConfirmResult = false;
            this.blCalibResult = false;
            //this.dblXRawValue = 0;
        }

        //
        public clsCalPointValueSetting()
        {
            this.strCaliUserGuide = "";
            this.strMethodName = "";
            this.blCalibResult = false;
            this.blConfirmResult = false;
        }
    }

    /// <summary>
    /// Setting for Optional User Control of Calibration
    /// </summary>
    public class clsUserControlData
    {
        //For User Control Panel
        public System.Windows.Controls.GroupBox grbUserControl { get; set; }
        public System.Windows.Controls.Button btnUserControl { get; set; }

        public string strNameUserControl { get; set; }
        public object objTarget { get; set; }
        public string strMethodName { get; set; }
        public object[] arrObjPara { get; set; }
        

        public clsUserControlData()
        {
            this.grbUserControl = new System.Windows.Controls.GroupBox();
            this.btnUserControl = new System.Windows.Controls.Button();
            this.strNameUserControl = "";
        }
    }

    //
    public class ValueObserver
    {
        //For calling Function to display data on Value Observer window
        public object objTarget { get; set; }
        public string strMethodName { get; set; }
        public object[] arrObjPara { get; set; }

        public ValueObserver()
        {
            this.objTarget = new object();
            this.strMethodName = "";
            this.arrObjPara = new object[1];
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
}
