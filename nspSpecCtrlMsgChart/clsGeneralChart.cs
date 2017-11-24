using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using System.Globalization;
using nspSpecMsgChart;

namespace nspGeneralChart
{
    //*******************************************************************************************************
    public class clsGeneralChart
    {
        //
        //For holding data of Chart's settings 
        public SettingChart settingChart { get; set; }

        //Window Chart Object
        public windowChart windowChart { get; set; }
        public int intNumberDataChartLayer { get; set; }

        //***************************Define user setting**********************************
        public double dblWindowChartWidth { get; set; }
        public double dblWindowChartHeight { get; set; }

        private string _strWindowChartTitle = "CFP General Chart Version 0.001";
        public string strWindowChartTitle 
        {
            get
            {
                return _strWindowChartTitle;
            }
            set
            {
                _strWindowChartTitle = value;
            }
        }

        //*********************************************************************************
        public string CreateChartData(List<object> lstobjInput) //Count from Zero
        {
            string strRet = "0";
            //
            List<Point> lstPointInput = new List<Point>();

            for (int i = 0; i < lstobjInput.Count;i++)
            {
                if (!(lstobjInput[i] is Point)) return "CreateChartData() error: the element number [" + (i+1).ToString() + "] is not Point() data!" ;
                lstPointInput.Add((Point)lstobjInput[i]);
            }

            try
            {
                //this.windowChart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.CreateChartData(lstPointInput)));
                this.windowChart.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.CreateChartData(lstPointInput)));
            }
            catch (Exception ex)
            {
                return "CreateChartData() error: " + ex.Message;
            }

            //everything is ok?
            return strRet;
        }

        //SetDataName
        public string SetDataName(object objNumDataLayerID, object objChartName) //Count from Zero
        {
            //Check valid data
            int intNumDataLayerID = 0;
            if (int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID) == false) return "AddChartData() error: The number of chart layer ID input is not integer: [" + objNumDataLayerID.ToString() + "]";

            string strRet = "0";
            try
            {
                //Display change
                this.windowChart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.SetDataName(intNumDataLayerID, objChartName.ToString())));
            }
            catch (Exception ex)
            {
                return "SetDataName() error: " + ex.Message;
            }

            //everything is ok?
            return strRet;
        }
        //
        public string AddChartData(object objNumDataLayerID, object objInput) //Count from Zero
        {
            //Check valid data
            int intNumDataLayerID = 0;
            if (int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID) == false) return "AddChartData() error: The number of chart layer ID input is not integer: [" + objNumDataLayerID.ToString() + "]";
            //
            string strRet = "0";
            try
            {
                //Display change
                this.windowChart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.AddChartData(intNumDataLayerID, objInput)));
            }
            catch (Exception ex)
            {
                return "AddChartData() error: " + ex.Message;
            }

            //everything is ok?
            return strRet;
        }

        public string UpdateChartData(object objNumDataLayerID, object objInput) //Count from Zero
        {
            //Check valid data
            int intNumDataLayerID = 0;
            if (int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID) == false) return "UpdateChartData() error: The number of chart layer ID input is not integer: [" + objNumDataLayerID.ToString() + "]";
            //
            string strRet = "0";
            try
            {
                //Display change
                this.windowChart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.UpdateChartData(intNumDataLayerID, objInput)));
            }
            catch (Exception ex)
            {
                return "UpdateChartData() error: " + ex.Message;
            }

            //everything is ok?
            return strRet;
        }

        //
       public void AddUserLine(object objLineOrientation, object objUserVal, object objUserLineName)
       {
            //Display change
           this.windowChart.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.windowChart.AddUserLine(objLineOrientation, objUserVal,objUserLineName)));
       }

        //
        private void CreateNewChart()
        {
            //Create new chart window
            this.windowChart = new windowChart(this.settingChart);
            //Apply setting Chart
            //this.windowChart.settingChart = this.settingChart;
            //
            this.windowChart.DataContext = this;

            //Setting chart follow user request
            if (this.strWindowChartTitle == "")
            {
                this.strWindowChartTitle = "CFP General Graph";
            }
            this.windowChart.Title = this.strWindowChartTitle;

            if (this.dblWindowChartWidth == 0)
            {
                this.dblWindowChartWidth = 700; //Default value
            }
            if (this.dblWindowChartHeight == 0)
            {
                this.dblWindowChartHeight = 500; //Default value
            }
            this.windowChart.Width = this.dblWindowChartWidth;
            this.windowChart.Height = this.dblWindowChartHeight;

            // Show window of graph - this must be done before create adorn layer
            this.windowChart.Show();
        }

        //
        private delegate object MyDelegate();
        public string StartChart()
        {
            string strRet = "0";
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() => this.CreateNewChart()));
            }
            catch (Exception ex)
            {
                return "StartChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //*********************************************************************************************
        public string AutoSetChart()
        {
            string strRet = "0";
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() => this.windowChart.AutoSetChart()));
            }
            catch (Exception ex)
            {
                return "Refresh() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //*********************************************************************************************
        public string ShowChart()
        {
            string strRet = "0";
            if (this.windowChart == null) return "ShowChart() error: windowChart is null!";
            try
            {
                //Check if window chart is closed or not
                if(this.windowChart.blIsClosed == true)
                {
                    StartChart();
                }
                else
                {
                    this.windowChart.Dispatcher.Invoke(() => {
                        this.windowChart.Show();
                        this.windowChart.WindowState = WindowState.Normal;
                        });
                }
            }
            catch(Exception ex)
            {
                return "ShowChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //
        public string CloseChart()
        {
            string strRet = "0";
            if (this.windowChart == null) return "CloseChart() error: windowChart is null!";
            try
            {
                this.windowChart.Dispatcher.Invoke(this.windowChart.Close);
            }
            catch (Exception ex)
            {
                return "CloseChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //
        public string HideChart()
        {
            string strRet = "0";
            if (this.windowChart == null) return "HideChart() error: windowChart is null!";
            try
            {
                this.windowChart.Dispatcher.Invoke(this.windowChart.Hide);
            }
            catch (Exception ex)
            {
                return "HideChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }
        //
        public string MinimizeChart()
        {
            string strRet = "0";
            if (this.windowChart == null) return "MinimizeChart() error: windowChart is null!";
            try
            {
                this.windowChart.Dispatcher.Invoke(()=> {
                    this.windowChart.WindowState = WindowState.Minimized;
                    });
            }
            catch (Exception ex)
            {
                return "MinimizeChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //
        public string NormalizeChart()
        {
            string strRet = "0";
            if (this.windowChart == null) return "NormalmizeChart() error: windowChart is null!";
            try
            {
                this.windowChart.Dispatcher.Invoke(() => {
                    this.windowChart.WindowState = WindowState.Normal;
                });
            }
            catch (Exception ex)
            {
                return "NormalmizeChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }



        //*********************************************************************************************
        public string ResizeChart(object objWidth, object objHeight)
        {
            string strRet = "0";
            if (this.windowChart == null) return "ResizeChart() error: windowChart is null!";

            //Check input parameter
            double dblWidth = 0;
            double dblHeight = 0;
            if (double.TryParse(objWidth.ToString(), out dblWidth) == false) return "ResizeChart() Error: The width setting is not numeric: [" + objWidth.ToString() + "]";
            if (double.TryParse(objHeight.ToString(), out dblHeight) == false) return "ResizeChart() Error: The Height setting is not numeric: [" + objHeight.ToString() + "]";

            //Saving to setting chart
            this.settingChart.dblChartWindowWidth = dblWidth;
            this.settingChart.dblChartWindowHeight = dblHeight;

            try
            {
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.ResizeChart(dblWidth, dblHeight)));
            }
            catch (Exception ex)
            {
                return "ResizeChart() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        public string SetChartDirection(object objXDirection, object objYDirection)
        {
            string strRet = "0";
            if (this.windowChart == null) return "SetChartDirection() error: windowChart is null!";

            //Check input parameter
            double dblTempX = 0;
            double dblTempY = 0;
            if (double.TryParse(objXDirection.ToString(), out dblTempX) == false) return "SetChartDirection() Error: The X direction setting is not numeric: [" + objXDirection.ToString() + "]";
            if (double.TryParse(objYDirection.ToString(), out dblTempY) == false) return "SetChartDirection() Error: The Y direction setting is not numeric: [" + objYDirection.ToString() + "]";

            try
            {
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetChartDirection(dblTempX, dblTempY)));
            }
            catch (Exception ex)
            {
                return "SetChartDirection() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //*********************************************************************************************
        public string SetChartRange(object objXSmall, object objXBiggest, object objYSmall, object objYBiggest)
        { 
            string strRet = "0";
            if (this.windowChart == null) return "SetChartRange() error: windowChart is null!";

            try
            {
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetChartRange(objXSmall, objXBiggest, objYSmall, objYBiggest)));
            }
            catch (Exception ex)
            {
                return "SetChartRange() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //*********************************************************************************************
        public string SetChartDivideLine(object objNumLineX, object objNumLineY)
        {
            string strRet = "0";
            if (this.windowChart == null) return "SetChartDivideLine() error: windowChart is null!";

            try
            {
                this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.SetChartDivideLine(objNumLineX, objNumLineY)));
            }
            catch (Exception ex)
            {
                return "SetChartDivideLine() error: " + ex.Message;
            }
            //everything is ok?
            return strRet;
        }

        //*********************************************************************************************
        public string SettingChart(object objNumDataLayerID, object objChartSyleID, object objChartColor, object objMarkerSize)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                //this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.SettingChart(objNumDataLayerID, objChartSyleID, objChartColor, objMarkerSize)));
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SettingChartLayer(objNumDataLayerID, objChartSyleID, objChartColor, objMarkerSize)));
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }
            
            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        /// <summary>
        /// Setting threshold value - color for chart. Allow each point of chart has color depend on range of chart data value
        /// </summary>
        /// <param name="objNumDataLayerID">ID of chart data layer (1 graph can contains some charts)</param>
        /// <param name="objXorY"> 0: setting threshold value for X axis, 1: setting threshold value for Y axis</param>
        /// <param name="lstThresholdValue">List of threshold value</param>
        /// <param name="lstThresholdColor">List of threshold color. Note: lstThresholdColor.Count = lstThresholdValue.Count + 1 </param>
        /// <returns></returns>
        public string ChartThresholdColor(object objNumDataLayerID, object objXorY, object lstThresholdValue, object lstThresholdColor)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                //this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.ChartThresholdColor(objNumDataLayerID, objXorY, lstThresholdValue, lstThresholdColor)));
                this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.ChartThresholdColor(objNumDataLayerID, objXorY, lstThresholdValue, lstThresholdColor)));
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        /// <summary>
        /// Setting for each Point in Chart Layer
        /// </summary>
        /// <param name="objNumDataLayerID">ID of Layer where contains point want to setting</param>
        /// <param name="objPointID"> ID of point want to setting</param>
        /// <param name="objColor">Color want to setting</param>
        /// <returns></returns>
        public string PointSetting(object objNumDataLayerID, object objPointID, object objColor)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.PointSetting(objNumDataLayerID, objPointID, objColor)));
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        public string RemovePointSetting(object objNumDataLayerID, object objPointID)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.RemovePointSetting(objNumDataLayerID, objPointID)));
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string RemoveChart(object objNumDataLayerID)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.RemoveChart(objNumDataLayerID)));
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string ClearChart()
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.Invoke(new Action(() => this.windowChart.ClearChart()));
            }
            catch (Exception ex)
            {
                return "ClearChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string DisplayUserInfo(string strTitle, string strContent)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.DisplayUserInfo(strTitle, strContent)));
            }
            catch (Exception ex)
            {
                return "DisplayUserInfo() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }
        
        //*********************************************************************************************
        public string AddUserInfo(string strTitle, string strContent)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.AddUserInfo(strTitle, strContent)));
            }
            catch (Exception ex)
            {
                return "AddUserInfo() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string UpdateUserInfo(string strTitle, string strContent)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.UpdateUserInfo(strTitle, strContent)));
            }
            catch (Exception ex)
            {
                return "UpdateUserInfo() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string SetContentColor(string strTitle, object objColor)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetContentColor(strTitle, objColor)));
            }
            catch (Exception ex)
            {
                return "UpdateUserInfo() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        public string SetContentSize(string strTitle, object objSize)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetContentSize(strTitle, objSize)));
            }
            catch (Exception ex)
            {
                return "SetContentSize() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        public string SetChartTitle(string strTitle)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetChartTitle(strTitle)));
            }
            catch (Exception ex)
            {
                return "SetChartTitle() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        public string SetChartUnit(string strXUnit, string strYUnit)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Apply change
                this.windowChart.Dispatcher.BeginInvoke(new Action(() => this.windowChart.SetChartUnit(strXUnit, strYUnit)));
            }
            catch (Exception ex)
            {
                return "SetChartUnit() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*********************************************************************************************
        //Constructor
        public clsGeneralChart()
        {
            this.settingChart = new nspSpecMsgChart.SettingChart();
            //Default value for some variables
            this.dblWindowChartWidth = 1000;
            this.dblWindowChartHeight = 650;
        }

        //~destructor
        ~clsGeneralChart()
        {
            ;
        }

    }
    //*******************************************************************************************************
}
