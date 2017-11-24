using nspSpecMsgChart;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace nspGeneralChart
{
    /// <summary>
    /// Interaction logic for windowChart.xaml
    /// </summary>
    public partial class windowChart : Window
    {
        //*******************************************************************************
        //For holding data of Chart's settings 
        public SettingChart settingChart { get; set; }

        //This data will be refer when redering charts (Threshold Value, Invidual point color...)
        public List<SettingChartLayer> lstclsSettingChartLayer { get; set; }

        //*******************************************************************************
        public double dblChartWidth { get; set; } //The width of chart (only chart area)
        public double dblChartHeight { get; set; } //The height of chart (only chart area)


        public double dblMarginChartBorder { get; set; } //The distance between form border to chart axis
        public double dblChartAndLegendSpaceWidth { get; set; }

        public double dblChartOriginX { get; set; } //The Chart "Zero point" X coordinate
        public double dblChartOriginY { get; set; } //The Chart "Zero point" Y coordinate

        //setting for background scale
        public double dblBgrdXRange { get; set; } //The Range (Max - min) value can be displayed in X axis
        public double dblBgrdYRange { get; set; } //The Range (Max - min) value can be displayed in Y axis

        public double dblBgrdXscale { get; set; } //  dblXscale = dblChartWidth/dblXRange
        public double dblBgrdYscale { get; set; } //  dblYscale = dblChartHeight/dblYRange

        public double dblBgrdXOriginVal { get; set; } //The X value of Chart Origin point - for Background layer
        public double dblBgrdYOriginVal { get; set; } //The Y value of Chart Origin point - for Background layer

        //
        public AdornerLayer MyCanvasAdornerLayer { get; set; }
        //Timer
        public DispatcherTimer myTimer { get; set; }

        //For Data chart layer
        public List<clsDataChartLayer> lstclsDataChartLayer { get; set; }
        //Chart Legend Layer
        public classDisplayChartLegendLayer clsChartLegendLayer { get; set; }
        
        //For User Line Layer
        public List<classUserLineData> lstclsUserLineData { get; set; } //For store origin user setting
        public classUserLineLayer clsUserLineLayer { get; set; } //For display and convert
        //*******************************************************************************
       
        public bool blChartReady { get; set; }
        public bool blIsClosed { get; set; }
        //*******************************************************************************
        int intPanMode { get; set; } //What direction want to pan
        const int intPanXY = 0;
        const int intPanX = 1;
        const int intPanY = 2;

        int intZoomMode { get; set; } //What direction want to zoom
        const int intZoomXYPlus = 0;
        const int intZoomXYMinus = 1;
        const int intZoomXPlus = 2;
        const int intZoomXMinus = 3;
        const int intZoomYPlus = 4;
        const int intZoomYMinus = 5;

        //*******************************************************************************
        private static Mutex myMutex = new Mutex();

        public windowChart(nspSpecMsgChart.SettingChart settingChart)
        {
            myMutex.WaitOne();
            /////////////////////////////////////////
            InitializeComponent();

            //For protect...
            this.blIsClosed = false;
            this.blChartReady = false;

            //
            this.settingChart = settingChart;
            this.lstclsSettingChartLayer = new List<SettingChartLayer>();


            //Add event handle
            this.MyCanvas.SizeChanged += MyCanvas_SizeChanged;

            //Ini for Timer
            this.myTimer = new DispatcherTimer();
            this.myTimer.Interval = TimeSpan.FromMilliseconds(10);
            this.myTimer.Tick += myTimer_Tick;

            //Ini for background setting
            this.dblBgrdXRange = 20; //Range default is -10 to 10
            this.dblBgrdYRange = 20; //Range default is -10 to 10
            
            //Testing User Info display
            this.lstclsUserInfo = new List<clsUserInfoDisplay>();
            this.clsChartLegendLayer = new classDisplayChartLegendLayer();

            this.lstclsUserLineData = new List<classUserLineData>();
            this.clsUserLineLayer = new classUserLineLayer();

            //////////////////////////////////////////
            myMutex.ReleaseMutex();
        }

        //*******************************************************************************
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get canvas Adorner Layer
            this.MyCanvasAdornerLayer = AdornerLayer.GetAdornerLayer(this.MyCanvas);

            //Cal basic dimension
            CalGraphRelativeDimension();

            //Display chart with autoset mode
            AutoSetChart();

            //Draw background
            DrawChartBackGround();

            //
            this.blChartReady = true;
        }

        void myTimer_Tick(object sender, EventArgs e)
        {
            this.myTimer.Stop();
            ////////////////////////////////////
            AutoSetChart();
            ///////////////////////////////////
            this.myTimer.Start();
        }

        //
        public List<Point> CreateCoordinate(List<Point> lstclsDataInput, double dblOriginXCor, double dblOriginYCor, double dblXscale, double dblYscale, double dblXOriginVal, double dblYOriginVal)
        {
            // Convert Data value to coodinate value (compare with Zero point)
            // Input: Real Data value
            // Output: List of Graph coordinate of coressponding point (compare with zero point)
            //
            // - lstclsDataInput: Real data value
            // - dblOriginXCor: The X Coordinate or Chart Origin point compare with canvas graph
            // - dblOriginYCor: The Y Coordinate or Chart Origin point compare with canvas graph
            // - dblXscale: The X Scale of data chart layer setting (from user)
            // - dblYscale: The Y Scale of data chart layer setting (from user)
            // - dblXOriginVal: The X value of origin point correspond to data chart layer
            // - dblYOriginVal: The Y value of origin point correspond to data chart layer

            List<Point> lstclsRet = new List<Point>();

            int i = 0;
            for (i = 0; i < lstclsDataInput.Count; i++)
            {
                Point clsTemp = new Point(0, 0);

                //double dblTempX = (lstclsDataInput[i].X - dblXOriginVal) * dblXscale;
                double dblTempX = 0;
                if (this.settingChart.intChartXDirection == 1) //Reverse
                {
                    dblTempX = (dblXOriginVal + this.dblChartWidth/dblXscale - lstclsDataInput[i].X) * dblXscale;
                }
                else //No reverse
                {
                    dblTempX = (lstclsDataInput[i].X - dblXOriginVal) * dblXscale;
                }

                //double dblTempY = (lstclsDataInput[i].Y - dblYOriginVal) * dblYscale;
                double dblTempY = 0;
                if (this.settingChart.intChartYDirection == 1) //Reverse
                {
                    dblTempY = (dblYOriginVal + this.dblChartHeight/dblYscale - lstclsDataInput[i].Y) * dblYscale;
                }
                else //No reverse
                {
                    dblTempY = (lstclsDataInput[i].Y - dblYOriginVal) * dblYscale;
                }


                clsTemp.X = dblOriginXCor + dblTempX;
                clsTemp.Y = dblOriginYCor - dblTempY;

                lstclsRet.Add(clsTemp);
            }

            return lstclsRet;
        }

        //
        public void CalGraphRelativeDimension()
        {
            //Adjust chart size follow the size of window chart
            if (this.dblMarginChartBorder == 0)
            {
                this.dblMarginChartBorder = 60; //Default value
            }

            if (this.settingChart.intNumXdivide == 0)
            {
                this.settingChart.intNumXdivide = 10; //Default value
            }

            if (this.settingChart.intNumYdivide == 0)
            {
                this.settingChart.intNumYdivide = 10; //Default value
            }

            if (this.dblBgrdXRange == 0)
            {
                this.dblBgrdXRange = 10; //Range default is -10 to 10
            }

            if (this.dblBgrdYRange == 0)
            {
                this.dblBgrdYRange = 10; //Range default is -10 to 10
            }

            this.dblChartWidth = this.MyCanvas.ActualWidth - 2 * this.dblMarginChartBorder;
            this.dblChartHeight = this.MyCanvas.ActualHeight - 2 * this.dblMarginChartBorder;

            //Decide the coordinate of Chart Origin point
            this.dblChartOriginX = this.dblMarginChartBorder;
            this.dblChartOriginY = this.dblChartHeight + this.dblMarginChartBorder;

            //Cal Scale
            this.dblBgrdXscale = this.dblChartWidth / this.dblBgrdXRange;
            this.dblBgrdYscale = this.dblChartHeight / this.dblBgrdYRange;
        }

        //
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var x = Math.Pow(2, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine); //0.79 & 1.26
            //MyCanvas.Scale *= x;

            //When mouse wheel, we change the scale but have to keep the relative position of chart to mouse pointer
            //Have to cal dblNewOriginXVal & dblNewOriginYVal to ensure relative position not change

            var position = e.GetPosition(this);

            //If mouse is outside chart limit area, we will not do anything
            if (this.isInsideLimitArea(position) == false) return;

            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            //double dblXVal = this.dblBgrdXOriginVal + (position.X - relativePoint.X - this.dblChartOriginX) / this.dblBgrdXscale;
            //double dblYVal = this.dblBgrdYOriginVal + (this.dblChartOriginY + relativePoint.Y - position.Y) / this.dblBgrdYscale;
            //this.btnTest.Content = dblXVal.ToString() + "-" + dblYVal.ToString();

            //When Change to new scale, we need to keep this point position in new coordinate
            double dblXNewScale = x * this.dblBgrdXscale;
            double dblYNewScale = x * this.dblBgrdYscale;

            //double dblOriginXNewVal = this.dblBgrdXOriginVal + (position.X - relativePoint.X - this.dblChartOriginX) * (1 / this.dblBgrdXscale - 1 / dblXNewScale);
            double dblOriginXNewVal = 0;
            if (this.settingChart.intChartXDirection == 1) //Reverse
            {
                dblOriginXNewVal = this.dblBgrdXOriginVal + (this.dblChartOriginX + this.dblChartWidth - (position.X - relativePoint.X)) * (1 / this.dblBgrdXscale - 1 / dblXNewScale);
            }
            else //No reverse
            {
                dblOriginXNewVal = this.dblBgrdXOriginVal + (position.X - relativePoint.X - this.dblChartOriginX) * (1 / this.dblBgrdXscale - 1 / dblXNewScale);
            }
            
            
            //double dblOriginYNewVal = this.dblBgrdYOriginVal + (this.dblChartOriginY + relativePoint.Y - position.Y) * (1 / this.dblBgrdYscale - 1 / dblYNewScale);
            double dblOriginYNewVal = 0;
            if(this.settingChart.intChartYDirection==1) //Reverse
            {
                //this.dblBgrdYOriginVal + (position.Y - relativePoint.Y - (this.dblChartOriginY - this.dblChartHeight)) / this.dblBgrdYscale
                //   = dblOriginYNewVal + (position.Y - relativePoint.Y - (this.dblChartOriginY - this.dblChartHeight)) / dblYNewScale
                dblOriginYNewVal = this.dblBgrdYOriginVal + ((position.Y - relativePoint.Y) - (this.dblChartOriginY - this.dblChartHeight)) * (1 / this.dblBgrdYscale - 1 / dblYNewScale);
            }
            else //No reverse
            {
                //this.dblBgrdYOriginVal + (this.dblChartOriginY + relativePoint.Y - position.Y) / this.dblBgrdYscale
                //   = dblOriginYNewVal + (this.dblChartOriginY + relativePoint.Y - position.Y) / dblYNewScale
                dblOriginYNewVal = this.dblBgrdYOriginVal + (this.dblChartOriginY - (position.Y - relativePoint.Y) ) * (1 / this.dblBgrdYscale - 1 / dblYNewScale);
            }

            //Assign new origin value
            //this.dblBgrdXOriginVal = dblOriginXNewVal;
            //this.dblBgrdYOriginVal = dblOriginYNewVal;

            //
            switch(this.intZoomMode)
            {
                case intZoomXYPlus:
                    this.dblBgrdXOriginVal = dblOriginXNewVal;
                    this.dblBgrdYOriginVal = dblOriginYNewVal;
                    this.ChangeChartScale(x * this.dblBgrdXscale, x * this.dblBgrdYscale);
                    break;
                case intZoomXYMinus:
                    this.dblBgrdXOriginVal = dblOriginXNewVal;
                    this.dblBgrdYOriginVal = dblOriginYNewVal;
                    this.ChangeChartScale(x * this.dblBgrdXscale, x * this.dblBgrdYscale);
                    break;
                case intZoomXPlus:
                    this.dblBgrdXOriginVal = dblOriginXNewVal;
                    this.ChangeChartXScale(x * this.dblBgrdXscale);
                    break;
                case intZoomXMinus:
                    this.dblBgrdXOriginVal = dblOriginXNewVal;
                    this.ChangeChartXScale(x * this.dblBgrdXscale);
                    break;
                case intZoomYPlus:
                    this.dblBgrdYOriginVal = dblOriginYNewVal;
                    this.ChangeChartYScale(x * this.dblBgrdYscale);
                    break;
                case intZoomYMinus:
                    this.dblBgrdYOriginVal = dblOriginYNewVal;
                    this.ChangeChartYScale(x * this.dblBgrdYscale);
                    break;
                default:
                    this.dblBgrdXOriginVal = dblOriginXNewVal;
                    this.dblBgrdYOriginVal = dblOriginYNewVal;
                    this.ChangeChartScale(x * this.dblBgrdXscale, x * this.dblBgrdYscale);
                    break;
            }
        }

        private Point LastMousePosition;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.blChartReady == false) return;

            var position = e.GetPosition(this);

            //Display Mouse coordinate
            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            //double dblCurrentValX = this.dblBgrdXOriginVal + ((position.X - relativePoint.X - this.dblChartOriginX) / this.dblBgrdXscale);
            double dblCurrentValX = 0;
            if (this.settingChart.intChartXDirection == 1) //Reverse
            {
                dblCurrentValX = this.dblBgrdXOriginVal + ((this.dblChartOriginX + this.dblChartWidth - (position.X - relativePoint.X)) / this.dblBgrdXscale);
            }
            else //No reverse
            {
                 dblCurrentValX = this.dblBgrdXOriginVal + ((position.X - relativePoint.X - this.dblChartOriginX) / this.dblBgrdXscale);
            }

            //double dblCurrentValY = this.dblBgrdYOriginVal + ((this.dblChartOriginY - position.Y + relativePoint.Y) / this.dblBgrdYscale);
            double dblCurrentValY = 0;
            if(this.settingChart.intChartYDirection==1) //Reverse
            {
                dblCurrentValY = this.dblBgrdYOriginVal + ((position.Y - (this.dblChartOriginY + relativePoint.Y - this.dblChartHeight)) / this.dblBgrdYscale);
            }
            else //No reverse
            {
                dblCurrentValY = this.dblBgrdYOriginVal + ((this.dblChartOriginY - position.Y + relativePoint.Y) / this.dblBgrdYscale);
            }



            this.tbMousePos.Text = "(" + this.Sci(dblCurrentValX, 3).ToString() + "," + this.Sci(dblCurrentValY, 3).ToString() + ")" + "-" +
                "(" + position.X.ToString() + "," + position.Y.ToString() + ")";


            //If mouse is outside chart limit area, we will not do anything
            if (this.isInsideLimitArea(position) == false) return;
            
            //Convert data chart to coordinate
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //MyCanvas.Offset -= position - LastMousePosition;
                //How many amount movement we decide?
                //Same as "pan" function - The position of chart compare to mouse position have to keep the same: last position & new position

                //Cal amount of mouse movement
                double dblMouseXChangeAmount = position.X - LastMousePosition.X;
                double dblMouseYChangeAmount = position.Y - LastMousePosition.Y;

                //Wee need calculate new dblChartOriginXVal & dblChartOriginYVal value, so the position of chart line is constant with mouse position
                // mouse move a unit => the movement length of chart has to be same => a 
                // (New origin X val - current origin x val)* X Scale = mouse move X => New origin X Val = (mouse move X /X Scale) + current origin x val
                //double dblNewOriginXVal = this.dblBgrdXOriginVal - (dblMouseXChangeAmount / this.dblBgrdXscale);
                double dblNewOriginXVal = 0;
                if (this.settingChart.intChartXDirection == 1) //reverse
                {
                    dblNewOriginXVal = this.dblBgrdXOriginVal + (dblMouseXChangeAmount / this.dblBgrdXscale);
                }
                else
                {
                    dblNewOriginXVal = this.dblBgrdXOriginVal - (dblMouseXChangeAmount / this.dblBgrdXscale);
                }

                //double dblNewOriginYVal = (dblMouseYChangeAmount / this.dblBgrdYscale) + this.dblBgrdYOriginVal;
                double dblNewOriginYVal;
                if(this.settingChart.intChartYDirection==1) //reverse
                {
                    dblNewOriginYVal = -(dblMouseYChangeAmount / this.dblBgrdYscale) + this.dblBgrdYOriginVal;
                }
                else
                {
                    dblNewOriginYVal = (dblMouseYChangeAmount / this.dblBgrdYscale) + this.dblBgrdYOriginVal;
                }

                //this.MoveChartPosition(this.dblChartOriginXVal - dblXchangeAmount, this.dblChartOriginYVal + dblYchangeAmount);
                //this.MoveChartPosition(dblNewOriginXVal, dblNewOriginYVal);

                switch(this.intPanMode)
                {
                    case intPanXY:
                        this.MoveChartPosition(dblNewOriginXVal, dblNewOriginYVal);
                        break;
                    case intPanX:
                        this.MoveChartPositionX(dblNewOriginXVal);
                        break;
                    case intPanY:
                        this.MoveChartPositionY(dblNewOriginYVal);
                        break;
                    default:
                        this.MoveChartPosition(dblNewOriginXVal, dblNewOriginYVal);
                        break;
                }
            }

            LastMousePosition = position;
        }

        public bool isInsideLimitArea(Point clsPointInput)
        {
            //Detect if mouse is inside limit area or not
            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));
            double dblXLolimitCor = relativePoint.X + this.dblChartOriginX;
            double dblXHilimitCor = dblXLolimitCor + this.dblChartWidth;
            double dblYHilimitCor = relativePoint.Y + this.dblChartOriginY;
            double dblYLolimitCor = dblYHilimitCor - this.dblChartHeight;

            if ((clsPointInput.X < dblXLolimitCor) || (clsPointInput.X > dblXHilimitCor))
            {
                return false;
            }

            if ((clsPointInput.Y < dblYLolimitCor) || (clsPointInput.Y > dblYHilimitCor))
            {
                return false;
            }

            return true;
        }

        //
        void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyCanvasResizeHandle();
        }

        //
        void MyCanvasResizeHandle()
        {
            //MessageBox.Show("Hello!");
            double dblTemp1 = 0;
            double dblTemp2 = 0;

            //When window size change. We keep scale of chart but adjust the Range of Chart
            //Calculate again the size of Chart

            dblTemp1 = this.MyCanvas.ActualWidth - 2 * this.dblMarginChartBorder;
            dblTemp2 = this.MyCanvas.ActualHeight - 2 * this.dblMarginChartBorder;

            if ((dblTemp1 < 0) || (dblTemp2 < 0)) return;

            this.dblChartWidth = this.MyCanvas.ActualWidth - 2 * this.dblMarginChartBorder;
            this.dblChartHeight = this.MyCanvas.ActualHeight - 2 * this.dblMarginChartBorder;

            //Decide the coordinate of Zero point
            this.dblChartOriginX = this.dblMarginChartBorder;
            this.dblChartOriginY = this.dblChartHeight + this.dblMarginChartBorder;

            //Clear all canvas
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();

            //Draw again background
            //this.DrawChartBackGround();

            //Display chart with autoset mode
            AutoSetChart();
        }

        //*******************************************************************************
        //Control display chart on background
        public void DisplayChartLine(int intDataChartID, int intChartStyleID = 0)
        {
            //Check out range
            if (intDataChartID > (this.lstclsDataChartLayer.Count - 1)) return;

            //Clear old data first
            RemoveChartLineLayer(intDataChartID);

            //Display new data
            DisplayUserDataLayerAdorner(
                this.lstclsDataChartLayer[intDataChartID].DisplayDatalayer, 
                this.lstclsDataChartLayer[intDataChartID].lstclsDataChart,
                this.lstclsSettingChartLayer[intDataChartID],
                intChartStyleID);
        }

        //Control display chart on background
        public void DisplayAllChartLine()
        {
            if (this.lstclsDataChartLayer == null) return;
            //Check out range
            int i = 0;
            for (i = 0; i < this.lstclsDataChartLayer.Count;i++)
            {
                //Clear old display
                RemoveChartLineLayer(i);

                //Display new data
                DisplayUserDataLayerAdorner(this.lstclsDataChartLayer[i].DisplayDatalayer, 
                    this.lstclsDataChartLayer[i].lstclsDataChart,
                    this.lstclsSettingChartLayer[i],
                    this.lstclsDataChartLayer[i].intChartStyleID);
            }
        }

        public void DisplayUserDataLayerAdorner(classDisplayDataChartLayer userChartLayer, List<Point> lstclsDataChart, SettingChartLayer settingChartLayer, int intChartStyleID = 0)
        {
            //Adjust relative coordinate of Adonrner layer compare to Window Chart
            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            //Add a Adorners layer to canvas
            System.Windows.Media.SolidColorBrush clrTemp = userChartLayer.clrMyColor;

            //Setting chart rectangle limit area
            userChartLayer.dblXLolimitCor = relativePoint.X + this.dblChartOriginX;
            userChartLayer.dblXHilimitCor = userChartLayer.dblXLolimitCor + this.dblChartWidth;
            userChartLayer.dblYLolimitCor = relativePoint.Y + this.dblChartOriginY - this.dblChartHeight;
            userChartLayer.dblYHilimitCor = userChartLayer.dblYLolimitCor + this.dblChartHeight;

            List<Point> lstclsCoordinate = new List<Point>();

            //Convert data chart to coordinate
            lstclsCoordinate = this.CreateCoordinate(lstclsDataChart, (this.dblChartOriginX + relativePoint.X), (this.dblChartOriginY + relativePoint.Y), 
                                                        this.dblBgrdXscale, this.dblBgrdYscale,this.dblBgrdXOriginVal,this.dblBgrdYOriginVal);

            //Setting chart direction
            userChartLayer.intChartXDirection = this.settingChart.intChartXDirection;
            userChartLayer.intChartYDirection = this.settingChart.intChartYDirection;


            //Apply Setting for Chart Layer item
            userChartLayer.settingChartLayer = settingChartLayer;

            //Draw chart data
            userChartLayer.DrawChartOnLayer(lstclsCoordinate, lstclsDataChart, intChartStyleID);

            //
            DisplayUserChartDataLayer(userChartLayer.MyAdorner);
        }

        /// <summary>
        /// Adding user adorner to canvas graph 
        /// </summary>
        /// <param name="UserAdorner"></param>
        public void DisplayUserChartDataLayer(ControlAdorner UserAdorner)
        {
            //
            if (this.MyCanvasAdornerLayer == null) return;
            //
            this.MyCanvasAdornerLayer = AdornerLayer.GetAdornerLayer(this.MyCanvas);
            this.MyCanvasAdornerLayer.Add(UserAdorner);
        }

        public void RemoveChartLineLayer(int intDataChartID)
        {
            //Check null
            if (this.MyCanvasAdornerLayer == null) return;

            //Check out of range
            if (intDataChartID > (this.lstclsDataChartLayer.Count - 1)) return;

            //Remove chart data layer
            if (this.lstclsDataChartLayer[intDataChartID].DisplayDatalayer.MyAdorner == null) return;
            this.MyCanvasAdornerLayer.Remove(this.lstclsDataChartLayer[intDataChartID].DisplayDatalayer.MyAdorner);
        }

        public void RemoveAllChartLineLayer()
        {
            //Check null
            if (this.MyCanvasAdornerLayer == null) return;
            if (this.lstclsDataChartLayer == null) return;

            //Remove chart data layer
            int i = 0;
            for (i = 0; i < this.lstclsDataChartLayer.Count;i++)
            {
                if (this.lstclsDataChartLayer[i].DisplayDatalayer.MyAdorner == null) continue;
                this.MyCanvasAdornerLayer.Remove(this.lstclsDataChartLayer[i].DisplayDatalayer.MyAdorner);
            }
        }

        //For Chart Legend control
        public void DisplayChartLegend()
        {
            //Create new legend layer class
            this.clsChartLegendLayer = new classDisplayChartLegendLayer();
            //Transfer data to new class
            int i = 0;
            for (i = 0; i < this.lstclsDataChartLayer.Count; i++)
            {
                clsLegendData clsNew = new clsLegendData();

                clsNew.strChartName = this.lstclsDataChartLayer[i].strChartName;
                clsNew.clrMyColor = this.lstclsDataChartLayer[i].DisplayDatalayer.clrMyColor;
                this.clsChartLegendLayer.lstclsLegendData.Add(clsNew);
            }

            //Setting chart Legend rectangle limit area
            //Adjust relative coordinate of Adonrner layer compare to Window Chart
            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            this.clsChartLegendLayer.dblXLolimitCor = relativePoint.X + this.dblChartOriginX;
            this.clsChartLegendLayer.dblXHilimitCor = this.clsChartLegendLayer.dblXLolimitCor + this.dblChartWidth;
            this.clsChartLegendLayer.dblYLolimitCor = this.dblChartOriginY + this.dblChartAndLegendSpaceWidth + this.clsChartLegendLayer.dblRectSize + 10;
            this.clsChartLegendLayer.dblYHilimitCor = this.clsChartLegendLayer.dblYLolimitCor + 300;

            this.clsChartLegendLayer.DrawChartLegend();

            //
            DisplayUserChartDataLayer(this.clsChartLegendLayer.MyAdorner);
        }

        public void RemoveChartLegendLayer()
        {
            //Check null
            if (this.MyCanvasAdornerLayer == null) return;
            if (this.lstclsDataChartLayer == null) return;

            //Remove chart data layer
            this.MyCanvasAdornerLayer.Remove(this.clsChartLegendLayer.MyAdorner);
        }

        //For Chart Legend control
        public void DisplayUserLine()
        {
            //Create new user line layer class - Always create new to prevent multi item add to 1 control...
            this.clsUserLineLayer = new classUserLineLayer();

            //Setting chart Legend rectangle limit area
            //Adjust relative coordinate of Adonrner layer compare to Window Chart
            Point relativePoint = this.MyCanvas.TransformToAncestor(this).Transform(new Point(0, 0));

            this.clsUserLineLayer.dblXLolimitCor = relativePoint.X + this.dblChartOriginX;
            this.clsUserLineLayer.dblXHilimitCor = this.clsUserLineLayer.dblXLolimitCor + this.dblChartWidth;
            this.clsUserLineLayer.dblYHilimitCor = relativePoint.Y + this.dblChartOriginY;
            this.clsUserLineLayer.dblYLolimitCor = this.clsUserLineLayer.dblYHilimitCor - this.dblChartHeight;


            //Transfer data from window chart class with user value already adjust follow window chart
            this.clsUserLineLayer.lstclsUserLineData = this.lstclsUserLineData;
            //this.clsUserLineLayer.lstclsUserLineData = this.lstclsUserLineData.ToList();

            //Convert from user value to Real Coordinate
            List<Point> lstTemp = new List<Point>();
            for (int i = 0; i < this.clsUserLineLayer.lstclsUserLineData.Count; i++)
            {
                Point pTemp = new Point(this.clsUserLineLayer.lstclsUserLineData[i].dblUserValX, this.clsUserLineLayer.lstclsUserLineData[i].dblUserValY);
                lstTemp.Add(pTemp);
            }

            lstTemp = this.CreateCoordinate(lstTemp, (this.dblChartOriginX + relativePoint.X), (this.dblChartOriginY + relativePoint.Y),
                                                        this.dblBgrdXscale, this.dblBgrdYscale, this.dblBgrdXOriginVal, this.dblBgrdYOriginVal);

            for (int i = 0; i < this.clsUserLineLayer.lstclsUserLineData.Count; i++)
            {
                this.clsUserLineLayer.lstclsUserLineData[i].dblUserCorX = lstTemp[i].X;
                this.clsUserLineLayer.lstclsUserLineData[i].dblUserCorY = lstTemp[i].Y;
            }


            //
            this.clsUserLineLayer.DrawUserLine();

            //
            DisplayUserChartDataLayer(this.clsUserLineLayer.MyAdorner);
        }

        public void RemoveUserLineLayer()
        {
            //Check null
            if (this.MyCanvasAdornerLayer == null) return;
            if (this.lstclsDataChartLayer == null) return;

            //Remove chart data layer
            this.MyCanvasAdornerLayer.Remove(this.clsUserLineLayer.MyAdorner);
        }

        public void AddUserLine(object objLineOrientation, object objUserVal, object objUserLineName)
        {
            //Checking
            double dblLineOrient = 0;
            if(double.TryParse(objLineOrientation.ToString(), out dblLineOrient)==false)
            {
                return;
            }

            double dblUserVal = 0;
            if (double.TryParse(objUserVal.ToString(), out dblUserVal) == false)
            {
                return;
            }

            //Testing
            if (this.clsUserLineLayer == null) this.clsUserLineLayer = new classUserLineLayer();

            classUserLineData clsTemp = new classUserLineData();
            clsTemp.intLineOrientationID = (int)dblLineOrient;
            clsTemp.dblUserValX = dblUserVal;
            clsTemp.dblUserValY = dblUserVal;
            clsTemp.strLineName = objUserLineName.ToString();

            //this.clsUserLineLayer.lstclsUserLineData.Add(clsTemp);
            this.lstclsUserLineData.Add(clsTemp);
            //
            this.AutoSetChart();
        }


        //Draw chart background
        public void DrawChartBackGround()
        {
            int i = 0;
            System.Windows.Controls.Label myLabel = new System.Windows.Controls.Label();
            Line line = new Line();
            DoubleCollection dashed = new DoubleCollection();
            double dbllabelLeft = 0;
            double dbllabelTop = 0;
            double dblVal = 0;

            this.MyCanvas.Children.Clear();

            //*******************************DRAW LIMIT AREA - CHART RECTANGLE*****************************************************
            //Draw X axis - Bottom
            line = new Line();
            line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX; line.Y1 = this.dblChartOriginY;
            line.X2 = this.dblChartOriginX + this.dblChartWidth; line.Y2 = line.Y1;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 2;
            line.Stroke = System.Windows.Media.Brushes.Red;
            MyCanvas.Children.Add(line);

            //Draw X axis - Top
            line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX; line.Y1 = this.dblChartOriginY - this.dblChartHeight;
            line.X2 = this.dblChartOriginX + this.dblChartWidth; line.Y2 = line.Y1;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 2;
            line.Stroke = System.Windows.Media.Brushes.Red;
            MyCanvas.Children.Add(line);

            //Draw Y axis - Left
            line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX; line.Y1 = this.dblChartOriginY - this.dblChartHeight;
            line.X2 = line.X1; line.Y2 = this.dblChartOriginY;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 2;
            line.Stroke = System.Windows.Media.Brushes.Red;
            MyCanvas.Children.Add(line);

            //Draw Y axis - Right
            line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX + this.dblChartWidth; line.Y1 = this.dblChartOriginY - this.dblChartHeight;
            line.X2 = line.X1; line.Y2 = this.dblChartOriginY;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 2;
            line.Stroke = System.Windows.Media.Brushes.Red;
            MyCanvas.Children.Add(line);
            //*********************************************************************************************************************

            //Chart Title
            //Cal position for display Chart title
            myLabel = new System.Windows.Controls.Label();
            myLabel.Content = this.settingChart.strChartTitle;
            myLabel.Foreground = System.Windows.Media.Brushes.Blue;
            myLabel.FontSize = 20;
            myLabel.FontStyle = FontStyles.Italic;
            myLabel.FontWeight = FontWeights.Bold;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX + this.dblChartWidth/2 - this.MeasureLabelWidth(myLabel)/2;
            dbllabelTop = this.dblChartOriginY - this.dblChartHeight - this.MeasureLabelHeight(myLabel) - 20;
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);


            //*******************************DRAW CENTER LINE OF X AXIS & Y LINE***************************************************
            //Draw X axis center line
            line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX;
            line.Y1 = this.dblChartOriginY - this.dblChartHeight / 2;
            line.X2 = this.dblChartOriginX + this.dblChartWidth;
            line.Y2 = line.Y1;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 1;
            line.Stroke = System.Windows.Media.Brushes.Green;
            MyCanvas.Children.Add(line);

            //Adding labels
            //Cal value to display
            if(this.settingChart.intChartYDirection==1) //Reverse
            {
                dblVal = (this.dblChartOriginY - line.Y1) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
            }
            else //No reverse
            {
                dblVal = (this.dblChartOriginY - line.Y1) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
            }
           

            //Cal position for display value
            myLabel = new System.Windows.Controls.Label();
            myLabel.Content = this.Sci(dblVal, 3);
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) - 10;
            dbllabelTop = line.Y1 - this.MeasureLabelHeight(myLabel);
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);

            
            //Draw Y axis center line
            line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            line.X1 = this.dblChartOriginX + this.dblChartWidth / 2;
            line.Y1 = this.dblChartOriginY;
            line.X2 = line.X1;
            line.Y2 = this.dblChartOriginY - this.dblChartHeight;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 1;
            line.Stroke = System.Windows.Media.Brushes.Green;
            MyCanvas.Children.Add(line);

            //Adding labels
            //Cal value to display
            //dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;

            if (this.settingChart.intChartXDirection == 1) //Reverse
            {
                dblVal = (this.dblChartOriginX + this.dblChartWidth - line.X1) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
            }
            else //No reverse
            {
                dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
            }


            //Cal position for display value
            myLabel = new System.Windows.Controls.Label();
            myLabel.Content = this.Sci(dblVal, 3);
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = line.X1 - this.MeasureLabelWidth(myLabel) / 2 - 5;
            dbllabelTop = this.dblChartOriginY + this.MeasureLabelHeight(myLabel) / 2 - 10;
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);

            //*********************************************************************************************************************

            //*******************************DRAW GRID LINE ON X AXIS & Y LINE*****************************************************
            //Cal how many grid line on each axis on both side compare with 
            int intNumGridX = this.settingChart.intNumXdivide / 2;
            int intNumGridY = this.settingChart.intNumYdivide / 2;

            this.settingChart.intNumXdivide = 2 * intNumGridX;
            this.settingChart.intNumYdivide = 2 * intNumGridY;


            //Draw X grid lines which above of X axis
            for (i = 1; i < intNumGridX; i++)
            {
                line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                line.X1 = this.dblChartOriginX;
                line.Y1 = this.dblChartOriginY - this.dblChartHeight / 2 - this.dblChartHeight / (2 * intNumGridX) * i;

                line.X2 = this.dblChartOriginX + this.dblChartWidth;
                line.Y2 = line.Y1;

                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                dashed = new DoubleCollection();
                dashed.Add(2); dashed.Add(2);
                line.StrokeDashArray = dashed;
                line.StrokeDashOffset = 5;
                line.Stroke = System.Windows.Media.Brushes.Blue;

                MyCanvas.Children.Add(line);

                //Adding labels
                //Cal value to display
                if(this.settingChart.intChartYDirection==1) //Reverse
                {
                    dblVal = (line.Y1 - (this.dblChartOriginY-this.dblChartHeight)) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
                }
                else //No reverse
                {
                    dblVal = (this.dblChartOriginY - line.Y1) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
                }
                

                //Cal position for display value
                myLabel = new System.Windows.Controls.Label();
                myLabel.Content = this.Sci(dblVal, 3);
                myLabel.Foreground = System.Windows.Media.Brushes.Black;
                MyCanvas.Children.Add(myLabel);
                dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) - 10;
                dbllabelTop = line.Y1 - this.MeasureLabelHeight(myLabel);
                Canvas.SetLeft(myLabel, dbllabelLeft);
                Canvas.SetTop(myLabel, dbllabelTop);
            }

            //Draw X grid lines which behind of X axis
            for (i = 1; i < intNumGridX; i++)
            {
                line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                line.X1 = this.dblChartOriginX;
                line.Y1 = this.dblChartOriginY - this.dblChartHeight / 2 + this.dblChartHeight / (2 * intNumGridX) * i;

                line.X2 = this.dblChartOriginX + this.dblChartWidth;
                line.Y2 = line.Y1;

                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                dashed = new DoubleCollection();
                dashed.Add(2); dashed.Add(2);
                line.StrokeDashArray = dashed;
                line.StrokeDashOffset = 5;
                line.Stroke = System.Windows.Media.Brushes.Blue;

                MyCanvas.Children.Add(line);

                //Adding labels
                //Cal value to display
                //dblVal = (this.dblChartOriginY - line.Y1) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
                if (this.settingChart.intChartYDirection == 1) //Reverse
                {
                    dblVal = (line.Y1 - (this.dblChartOriginY - this.dblChartHeight)) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
                }
                else //No reverse
                {
                    dblVal = (this.dblChartOriginY - line.Y1) / this.dblBgrdYscale + this.dblBgrdYOriginVal;
                }


                //Cal position for display value
                myLabel = new System.Windows.Controls.Label();
                myLabel.Content = this.Sci(dblVal, 3);
                myLabel.Foreground = System.Windows.Media.Brushes.Black;
                MyCanvas.Children.Add(myLabel);
                dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) - 10;
                dbllabelTop = line.Y1 - this.MeasureLabelHeight(myLabel);
                Canvas.SetLeft(myLabel, dbllabelLeft);
                Canvas.SetTop(myLabel, dbllabelTop);
            }

            //Draw Y grid lines which in left side of Y axis
            for (i = 1; i < intNumGridY;i++)
            {
                line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                line.X1 = this.dblChartOriginX + (this.dblChartWidth / 2) - (this.dblChartWidth / (2*intNumGridY)) * i;
                line.Y1 = this.dblChartOriginY;

                line.X2 = line.X1;
                line.Y2 = line.Y1 - this.dblChartHeight;

                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                dashed = new DoubleCollection();
                dashed.Add(2); dashed.Add(2);
                line.StrokeDashArray = dashed;
                line.StrokeDashOffset = 5;
                line.Stroke = System.Windows.Media.Brushes.Blue;

                MyCanvas.Children.Add(line);

                //Adding labels
                //Cal value to display
               // dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;

                if (this.settingChart.intChartXDirection == 1) //Reverse
                {
                    dblVal = (this.dblChartOriginX + this.dblChartWidth - line.X1) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
                }
                else //No reverse
                {
                    dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
                }


                //Cal position for display value
                myLabel = new System.Windows.Controls.Label();
                myLabel.Content = this.Sci(dblVal, 3);
                myLabel.Foreground = System.Windows.Media.Brushes.Black;
                MyCanvas.Children.Add(myLabel);
                dbllabelLeft = line.X1 - this.MeasureLabelWidth(myLabel) / 2 - 5;
                dbllabelTop = this.dblChartOriginY + this.MeasureLabelHeight(myLabel) / 2 - 10;
                Canvas.SetLeft(myLabel, dbllabelLeft);
                Canvas.SetTop(myLabel, dbllabelTop);

                this.dblChartAndLegendSpaceWidth = 1.5*this.MeasureLabelHeight(myLabel);

            }

            //Draw Y grid lines which in right side of Y axis
            for (i = 1; i < intNumGridY; i++)
            {
                line = new Line(); line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                line.X1 = this.dblChartOriginX + (this.dblChartWidth / 2) + (this.dblChartWidth / (2 * intNumGridY)) * i;
                line.Y1 = this.dblChartOriginY;

                line.X2 = line.X1;
                line.Y2 = line.Y1 - this.dblChartHeight;

                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                dashed = new DoubleCollection();
                dashed.Add(2); dashed.Add(2);
                line.StrokeDashArray = dashed;
                line.StrokeDashOffset = 5;
                line.Stroke = System.Windows.Media.Brushes.Blue;

                MyCanvas.Children.Add(line);

                //Adding labels
                //Cal value to display
                //dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;

                if (this.settingChart.intChartXDirection == 1) //Reverse
                {
                    dblVal = (this.dblChartOriginX + this.dblChartWidth - line.X1) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
                }
                else //No reverse
                {
                    dblVal = (line.X1 - this.dblChartOriginX) / this.dblBgrdXscale + this.dblBgrdXOriginVal;
                }


                //Cal position for display value
                myLabel = new System.Windows.Controls.Label();
                myLabel.Content = this.Sci(dblVal, 3);
                myLabel.Foreground = System.Windows.Media.Brushes.Black;
                MyCanvas.Children.Add(myLabel);
                dbllabelLeft = line.X1 - this.MeasureLabelWidth(myLabel) / 2 - 5;
                dbllabelTop = this.dblChartOriginY + this.MeasureLabelHeight(myLabel) / 2 - 10;
                Canvas.SetLeft(myLabel, dbllabelLeft);
                Canvas.SetTop(myLabel, dbllabelTop);
            }

            //////////////////Draw label of original point//////////////////////////

            //X label - origin point
            myLabel = new System.Windows.Controls.Label();

            //dblVal = this.dblBgrdYOriginVal;
            if (this.settingChart.intChartYDirection == 1) //Reverse
            {
                dblVal = this.dblChartHeight / this.dblBgrdYscale + this.dblBgrdYOriginVal;
            }
            else //No reverse
            {
                dblVal = this.dblBgrdYOriginVal;
            }

            myLabel.Content = this.Sci(dblVal, 3);
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) - 10;
            dbllabelTop = this.dblChartOriginY - this.MeasureLabelHeight(myLabel);
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);

            //Y label - origin point
            myLabel = new System.Windows.Controls.Label();

            //dblVal = this.dblBgrdXOriginVal;
            if (this.settingChart.intChartXDirection == 1) //Reverse
            {
                dblVal = this.dblBgrdXOriginVal + this.dblChartWidth / this.dblBgrdXscale;
            }
            else //No reverse
            {
                dblVal = this.dblBgrdXOriginVal;
            }

            myLabel.Content = this.Sci(dblVal, 3);
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) / 2 - 5;
            dbllabelTop = this.dblChartOriginY + this.MeasureLabelHeight(myLabel) / 2 - 10;
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);

            //////////////////Draw label of Unit name//////////////////////////
            //Y Unit label
            myLabel = new System.Windows.Controls.Label();
            myLabel.Content = this.settingChart.strYUnitName;
            myLabel.FontWeight = FontWeights.Bold;
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX - this.MeasureLabelWidth(myLabel) - 10;
            dbllabelTop = this.dblChartOriginY - this.dblChartHeight - this.MeasureLabelHeight(myLabel);
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);

            //X Unit label
            myLabel = new System.Windows.Controls.Label();
            myLabel.Content = this.settingChart.strXUnitName;
            myLabel.FontWeight = FontWeights.Bold;
            myLabel.Foreground = System.Windows.Media.Brushes.Black;
            MyCanvas.Children.Add(myLabel);
            dbllabelLeft = this.dblChartOriginX + this.dblChartWidth - this.MeasureLabelWidth(myLabel) / 2 - 5;
            dbllabelTop = this.dblChartOriginY + this.MeasureLabelHeight(myLabel) / 2 - 10;
            Canvas.SetLeft(myLabel, dbllabelLeft);
            Canvas.SetTop(myLabel, dbllabelTop);
        }

        //Change scale of chart
        public void ChangeChartXScale(double dblNewXScale)
        {
            //Assign new value for X & Y scale 
            this.dblBgrdXscale = dblNewXScale;
            this.dblBgrdXRange = this.dblBgrdXscale * this.dblChartWidth;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();

            //Draw background again
            this.DrawChartBackGround();

            //Display chart line again
            this.DisplayAllChartLine();

            this.DisplayUserLine();
        }

        public void ChangeChartYScale(double dblNewYScale)
        {
            //Assign new value for X & Y scale 
            this.dblBgrdYscale = dblNewYScale;
            this.dblBgrdYRange = this.dblBgrdYscale * this.dblChartHeight;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();

            //Draw background again
            this.DrawChartBackGround();

            //Display chart line again
            this.DisplayAllChartLine();
            this.DisplayUserLine();
        }

        public void ChangeChartScale(double dblNewXScale, double dblNewYScale)
        {
            //Assign new value for X & Y scale 
            this.dblBgrdXscale = dblNewXScale;
            this.dblBgrdXRange = this.dblBgrdXscale * this.dblChartWidth;
            this.dblBgrdYscale = dblNewYScale;
            this.dblBgrdYRange = this.dblBgrdYscale * this.dblChartHeight;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();

            //Draw background again
            this.DrawChartBackGround();

            //Display chart line again
            this.DisplayAllChartLine();
            this.DisplayUserLine();
        }

        //Shift chart to new position ()
        public void MoveChartPosition(double dblNewXOriginVal, double dblNewYOriginVal)
        {
            //When move chart to new position. We just change the offset in X & Y axis and redraw background & chart layer also
            this.dblBgrdXOriginVal = dblNewXOriginVal;
            this.dblBgrdYOriginVal = dblNewYOriginVal;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();
            //Draw background again
            this.DrawChartBackGround();

            //Draw chart layer again
            this.DisplayAllChartLine();
            this.DisplayUserLine();
        }

        public void MoveChartPositionX(double dblNewXOriginVal)
        {
            //When move chart to new position. We just change the offset in X & Y axis and redraw background & chart layer also
            this.dblBgrdXOriginVal = dblNewXOriginVal;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();
            //Draw background again
            this.DrawChartBackGround();

            //Draw chart layer again
            this.DisplayAllChartLine();
            this.DisplayUserLine();
        }

        public void MoveChartPositionY(double dblNewYOriginVal)
        {
            //When move chart to new position. We just change the offset in X & Y axis and redraw background & chart layer also
            this.dblBgrdYOriginVal = dblNewYOriginVal;

            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();
            //Draw background again
            this.DrawChartBackGround();

            //Draw chart layer again
            this.DisplayAllChartLine();
            this.DisplayUserLine();
        }

        public void AutoSetChart()
        {
            try
            {
                lock (this)
                {
                    //////////////////////////////////////////////////////////////////////////////
                    if (this.lstclsDataChartLayer == null) return;
                    if (this.lstclsDataChartLayer.Count == 0) return;
                    if (this.lstclsDataChartLayer[0].lstclsDataChart.Count == 0) return;

                    if(this.settingChart.blFixRange == false) //Allow Auto set range
                    {
                        //Bring all setting to auto set mode
                        //1. Cal X Range & Y Range base on user chart data range
                        double dblXSmallest = this.lstclsDataChartLayer[0].lstclsDataChart[0].X;
                        double dblXBiggest = this.lstclsDataChartLayer[0].lstclsDataChart[0].X;
                        double dblYSmallest = this.lstclsDataChartLayer[0].lstclsDataChart[0].Y;
                        double dblYBiggest = this.lstclsDataChartLayer[0].lstclsDataChart[0].Y;

                        //Looking for biggest & smallest
                        int i = 0;
                        int j = 0;
                        for (i = 0; i < this.lstclsDataChartLayer.Count; i++) //Looking in all data chart
                        {
                            //Finding smallest & Biggest value
                            for (j = 0; j < this.lstclsDataChartLayer[i].lstclsDataChart.Count; j++)
                            {
                                if (dblXSmallest > this.lstclsDataChartLayer[i].lstclsDataChart[j].X) dblXSmallest = this.lstclsDataChartLayer[i].lstclsDataChart[j].X;
                                if (dblXBiggest < this.lstclsDataChartLayer[i].lstclsDataChart[j].X) dblXBiggest = this.lstclsDataChartLayer[i].lstclsDataChart[j].X;
                                if (dblYSmallest > this.lstclsDataChartLayer[i].lstclsDataChart[j].Y) dblYSmallest = this.lstclsDataChartLayer[i].lstclsDataChart[j].Y;
                                if (dblYBiggest < this.lstclsDataChartLayer[i].lstclsDataChart[j].Y) dblYBiggest = this.lstclsDataChartLayer[i].lstclsDataChart[j].Y;
                            }
                        }

                        //Set range
                        this.dblBgrdXRange = (dblXBiggest - dblXSmallest) * 1.2;
                        this.dblBgrdYRange = (dblYBiggest - dblYSmallest) * 1.2;

                        this.dblChartWidth = this.MyCanvas.ActualWidth - 2 * this.dblMarginChartBorder;
                        this.dblChartHeight = this.MyCanvas.ActualHeight - 2 * this.dblMarginChartBorder;

                        this.dblBgrdXscale = this.dblChartWidth / this.dblBgrdXRange; //1 unit of data correspond to how long of display X axis
                        this.dblBgrdYscale = this.dblChartHeight / this.dblBgrdYRange; //1 unit of data correspond to how long of display Y axis

                        this.dblBgrdXOriginVal = dblXSmallest - 0.1 * Math.Abs(dblXSmallest);
                        this.dblBgrdYOriginVal = dblYSmallest - 0.1 * Math.Abs(dblYSmallest);

                    }
                    else //User setting for Range
                    {
                        //Bring all setting to auto set mode
                        //1. Cal X Range & Y Range base on user chart data range
                        double dblXSmallest = this.settingChart.dblXSmallest;
                        double dblXBiggest = this.settingChart.dblXBiggest;
                        double dblYSmallest = this.settingChart.dblYSmallest;
                        double dblYBiggest = this.settingChart.dblYBiggest;

                        //Set range
                        this.dblBgrdXRange = dblXBiggest - dblXSmallest;
                        this.dblBgrdYRange = dblYBiggest - dblYSmallest;

                        this.dblChartWidth = this.MyCanvas.ActualWidth - 2 * this.dblMarginChartBorder;
                        this.dblChartHeight = this.MyCanvas.ActualHeight - 2 * this.dblMarginChartBorder;

                        this.dblBgrdXscale = this.dblChartWidth / this.dblBgrdXRange; //1 unit of data correspond to how long of display X axis
                        this.dblBgrdYscale = this.dblChartHeight / this.dblBgrdYRange; //1 unit of data correspond to how long of display Y axis

                        this.dblBgrdXOriginVal = dblXSmallest;
                        this.dblBgrdYOriginVal = dblYSmallest;
                    }

                    //Display on control panel
                    this.tbLowX.Text = this.dblBgrdXOriginVal.ToString();
                    this.tbHiX.Text = (this.dblBgrdXOriginVal + this.dblBgrdXRange).ToString();
                    this.tbLowY.Text = this.dblBgrdYOriginVal.ToString();
                    this.tbHiY.Text = (this.dblBgrdYOriginVal + this.dblBgrdYRange).ToString();

                    this.tbDivX.Text = this.settingChart.intNumXdivide.ToString();
                    this.tbDivY.Text = this.settingChart.intNumYdivide.ToString();

                    //Draw chart again
                    //Clear all chart and draw again
                    //this.RemoveAllChartLineLayer();
                    this.MyCanvas.Children.Clear();

                    //Draw background again
                    this.DrawChartBackGround();

                    //Display chart line again

                    this.DisplayAllChartLine();

                    //Chart legend
                    this.RemoveChartLegendLayer();
                    this.DisplayChartLegend();

                    //User Line
                    this.RemoveUserLineLayer();
                    this.DisplayUserLine();

                    //Resize chart follow input setting
                    this.ResizeChart(this.settingChart.dblChartWindowWidth, this.settingChart.dblChartWindowHeight);

                    //////////////////////////////////////////////////////////////////////////////
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AutoSetChart() error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //RemoveAllChartLineLayer();
            ResizeChart(200, 100);
        }

        private void btnAutoSet_Click(object sender, RoutedEventArgs e)
        {
            AutoSetChart();
        }

        //*******************************************************************************************************
        public void SetChartDirection(object objXDirection, object objYDirection)
        {
            double dblTempX = 0;
            double dblTempY = 0;
            //
            if (double.TryParse(objXDirection.ToString().Trim(), out dblTempX) == false) return;
            if (double.TryParse(objYDirection.ToString().Trim(), out dblTempY) == false) return;
            //
            this.settingChart.intChartXDirection = (int)dblTempX;
            this.settingChart.intChartYDirection = (int)dblTempY;
        }
        
        //*******************************************************************************************************
        public string SetChartRange(object objXSmall, object objXBiggest, object objYSmall, object objYBiggest)
        {
            string strRet = "";
            //
            double dblXSmall = 0;
            if (double.TryParse(objXSmall.ToString(), out dblXSmall) == false) return "SetChartRange() Error: X small input is not numeric!";

            double dblXBiggest = 0;
            if (double.TryParse(objXBiggest.ToString(), out dblXBiggest) == false) return "SetChartRange() Error: X Biggest input is not numeric!";

            double dblYSmall = 0;
            if (double.TryParse(objYSmall.ToString(), out dblYSmall) == false) return "SetChartRange() Error: Y small input is not numeric!";

            double dblYBiggest = 0;
            if (double.TryParse(objYBiggest.ToString(), out dblYBiggest) == false) return "SetChartRange() Error: Y Biggest input is not numeric!";

            this.settingChart.blFixRange = true;
            this.settingChart.dblXSmallest = dblXSmall;
            this.settingChart.dblXBiggest = dblXBiggest;
            this.settingChart.dblYSmallest = dblYSmall;
            this.settingChart.dblYBiggest = dblYBiggest;
            
            //
            return strRet;
        }

        //*******************************************************************************************************
        public string SetChartDivideLine(object objNumLineX, object objNumLineY)
        {
            string strRet = "0";
            //
            int intNumLineX = 0;
            if (int.TryParse(objNumLineX.ToString(), out intNumLineX) == false) return "SetChartDivideLine() Error: Number Line of X axis input is not numeric!";

            int intNumLineY = 0;
            if (int.TryParse(objNumLineY.ToString(), out intNumLineY) == false) return "SetChartDivideLine() Error: Number Line of Y axis input is not numeric!";

            this.settingChart.intNumXdivide = intNumLineX;
            this.settingChart.intNumYdivide = intNumLineY;

            //
            return strRet;
        }

        //*******************************************************************************************************

        public void ResizeChart(double dblWidth, double dblHeight)
        {
            if((this.Width != dblWidth)||(this.Height != dblHeight))
            {
                this.Width = dblWidth;
                this.Height = dblHeight;
            }
        }
        //*********************************************************************************************
        public void CreateChartData(List<Point> lstPointInput) //Count from Zero
        {
            //Adding new chart data layer
            if (this.lstclsDataChartLayer == null)
            {
                this.lstclsDataChartLayer = new List<clsDataChartLayer>();
            }
            //
            clsDataChartLayer clsNew = new clsDataChartLayer();
            clsNew.lstclsDataChart = new List<Point>(lstPointInput);
            this.lstclsDataChartLayer.Add(clsNew);

            //When adding new chart layer, we also create new setting for it
            SettingChartLayer clsNewSettingChartLayer = new SettingChartLayer();
            this.lstclsSettingChartLayer.Add(clsNewSettingChartLayer);

            //
            this.AutoSetChart();
        }
        //*********************************************************************************************
        public string SetDataName(int intNumDataLayerID, string strName)
        {
            //Check if null
            if (this.lstclsDataChartLayer == null) return "SetDataName() Error: lstclsDataChartLayer is null!";
            //Check if out of range or not
            if (intNumDataLayerID > (this.lstclsDataChartLayer.Count - 1)) return "SetDataName() Error: the ID want to set name is out of range!";

            //Assign new value for list
            this.lstclsDataChartLayer[intNumDataLayerID].strChartName = strName;

            //Display change
            this.AutoSetChart();

            //
            return "0"; //OK code
        }
        //*********************************************************************************************
        public string AddChartData(int intNumDataLayerID, object objInput) //Count from Zero
        {
            string strRet = "";

            //Check null condition
            if (this.lstclsDataChartLayer == null) return "AddChartData() Error: lstclsDataChartLayer is null!";

            //Check if out of range or not
            if (intNumDataLayerID > (this.lstclsDataChartLayer.Count - 1)) return "AddChartData() Error: DataLayerID is out of range!";

            //
            //Check list input is List of Point or not
            List<Point> lstPointInput = new List<Point>();
            if(objInput is Point)
            {
                Point ptNew = (Point) objInput;
                //Assign new value for list
                if (this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart == null)
                {
                    this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart = new List<Point>(lstPointInput);
                }
                this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart.Add(ptNew);
            }
            else
            {
                if (!(this.isGenericList(objInput) == true)) return "AddChartData() error: Data input is not List of Point!";

                IList lstobjInput = (IList)objInput;

                for (int i = 0; i < lstobjInput.Count; i++)
                {
                    if (!(lstobjInput[i] is Point)) return "CreateChartData() error: the element number [" + (i + 1).ToString() + "] is not Point() data!";
                    lstPointInput.Add((Point)lstobjInput[i]);
                }

                 //Assign new value for list
                if (this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart == null)
                {
                    this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart = new List<Point>(lstPointInput);
                }
                this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart.AddRange(lstPointInput);
            }

            //Display change
            this.AutoSetChart();

            //
            return strRet;
        }

        //*********************************************************************************************
        /// <summary>
        /// Remove all old chart data and repplace by new chart data
        /// </summary>
        /// <param name="intNumDataLayerID"></param>
        /// <param name="objInput"></param>
        /// <returns></returns>
        public string UpdateChartData(int intNumDataLayerID, object objInput) //Count from Zero
        {
            string strRet = "";

            //Check null condition
            if (this.lstclsDataChartLayer == null) return "UpdateChartData() Error: lstclsDataChartLayer is null!";

            //Check if out of range or not
            if (intNumDataLayerID > (this.lstclsDataChartLayer.Count - 1)) return "UpdateChartData() Error: DataLayerID is out of range!";

            //Remove old chart data
            this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart = new List<Point>();

            //Check list input is List of Point or not
            List<Point> lstPointInput = new List<Point>();
            if (objInput is Point)
            {
                Point ptNew = (Point)objInput;
                //Assign new value for list
                this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart.Add(ptNew);
            }
            else
            {
                if (!(this.isGenericList(objInput) == true)) return "UpdateChartData() error: Data input is not List of Point!";

                IList lstobjInput = (IList)objInput;

                for (int i = 0; i < lstobjInput.Count; i++)
                {
                    if (!(lstobjInput[i] is Point)) return "UpdateChartData() error: the element number [" + (i + 1).ToString() + "] is not Point() data!";
                    lstPointInput.Add((Point)lstobjInput[i]);
                }

                //Assign new value for list
                this.lstclsDataChartLayer[intNumDataLayerID].lstclsDataChart.AddRange(lstPointInput);
            }

            //Display change
            this.AutoSetChart();

            //
            return strRet;
        }

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

        //*********************************************************************************************
        public string SettingChartLayer(object objNumDataLayerID, object objChartSyleID, object objChartColor, object objMarkerSize)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                lock(this)
                {
                    int intNumDataLayerID = 0;
                    if (int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID) == false) return "SettingChart() Error: The NumDataLayerID setting is not integer: [" + objNumDataLayerID.ToString() + "]";
                    if (this.lstclsDataChartLayer == null) return "SettingChart() Error: lstclsDataChartLayer is null!";
                    if ((this.lstclsDataChartLayer.Count - 1) < intNumDataLayerID) return "SettingChart() Error: The ID of Data Layer is out of range!";

                    int intChartSyleID = 0;
                    if (int.TryParse(objChartSyleID.ToString(), out intChartSyleID) == false) return "SettingChart() Error: The Chart style ID setting is not integer: [" + objChartSyleID.ToString() + "]";
                    this.lstclsDataChartLayer[intNumDataLayerID].intChartStyleID = intChartSyleID;

                    Color MyColor = new Color();
                    if (!(objChartColor is Color))
                    {
                        MyColor = (Color)ColorConverter.ConvertFromString(objChartColor.ToString());
                    }
                    this.lstclsDataChartLayer[intNumDataLayerID].DisplayDatalayer.clrMyColor = new SolidColorBrush(MyColor);

                    double dblMarkerSize = 5; //Default value
                    if (double.TryParse(objMarkerSize.ToString(), out dblMarkerSize) == false) return "SettingChart() Error: The Marker size setting is not numeric: [" + objChartSyleID.ToString() + "]";
                    this.lstclsDataChartLayer[intNumDataLayerID].DisplayDatalayer.dblMarkerSize = dblMarkerSize;

                    //Apply change
                    this.AutoSetChart();
                }
                
            }
            catch (Exception ex)
            {
                return "SettingChart() error: " + ex.Message;
            }

            //*********************************************
            return strRet;
        }

        //*******************************************************************************************************
        public string ChartThresholdColor(object objNumDataLayerID, object objXorY, object objlstThresholdValue, object objlstThresholdColor)
        {
            string strRet = "0";
            //
            try
            {
                int intNumDataLayerID = 0;
                if(int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID)==false)
                {
                    return "ChartThresholdColor() Error: NumDataLayerID input is not integer!";
                }
                int intXorY = 0;
                if (int.TryParse(objXorY.ToString(), out intXorY) == false)
                {
                    return "ChartThresholdColor() Error: X or Y axis setting  input is not integer!";
                }

                List<object> lstobjVal = (List<object>)objlstThresholdValue;
                List<double> lstVal = lstobjVal.Select(item => double.Parse(item.ToString())).ToList();

                List<object> lstobjColor = (List<object>)objlstThresholdColor;
                List<SolidColorBrush> lstColor = lstobjColor.Select(item => new SolidColorBrush((Color)item)).ToList();

                if (intXorY == 1) //Setting for Y axis
                {
                    this.lstclsSettingChartLayer[intNumDataLayerID].clsThresHoldColorY.lstdblThresholdValue = lstVal;
                    this.lstclsSettingChartLayer[intNumDataLayerID].clsThresHoldColorY.lstclrThresholdColor = lstColor;

                }
                else //Default is setting for X axis
                {
                    this.lstclsSettingChartLayer[intNumDataLayerID].clsThresHoldColorX.lstdblThresholdValue = lstVal;
                    this.lstclsSettingChartLayer[intNumDataLayerID].clsThresHoldColorX.lstclrThresholdColor = lstColor;
                }
            }
            catch(Exception ex)
            {
                return "ChartThresholdColor() Error: " + ex.Message;
            }

            //
            return strRet;
        }

        //*******************************************************************************************************
        public string PointSetting(object objDataLayerID, object objPointID, object objColor)
        {
            string strRet = "0";
            //
            try
            {
                int intDataLayerID = 0;
                if (int.TryParse(objDataLayerID.ToString(), out intDataLayerID) == false)
                {
                    return "PointSetting() Error: NumDataLayerID input is not integer!";
                }

                int intPointID = 0;
                if (int.TryParse(objPointID.ToString(), out intPointID) == false)
                {
                    return "PointSetting() Error: Point ID input is not integer!";
                }

                Color clr = (Color)objColor;
                SolidColorBrush clrPoint = new SolidColorBrush(clr);


                //Create if not yet exist setting for point.
                //If point setting already exist => Modify it, not create!
                var test = this.lstclsSettingChartLayer[intDataLayerID]
                    .lstSinglePointSetting.Where(item => item.ID == intPointID).SingleOrDefault();
                if(test==null) //Not yet exist => create new one
                {
                    this.lstclsSettingChartLayer[intDataLayerID].lstSinglePointSetting.Add(new SinglePointSetting
                    {
                        ID = intPointID,
                        solidColor = clrPoint
                    });
                }
                else //Already exist => modify existing one
                {
                    test.solidColor = clrPoint;
                }

                //Refresh chart
                this.AutoSetChart();
            }
            catch (Exception ex)
            {
                return "PointSetting() Error: " + ex.Message;
            }

            //
            return strRet;
        }

        public string RemovePointSetting(object objDataLayerID, object objPointID)
        {
            string strRet = "0";
            //
            try
            {
                int intDataLayerID = 0;
                if (int.TryParse(objDataLayerID.ToString(), out intDataLayerID) == false)
                {
                    return "RemovePointSetting() Error: NumDataLayerID input is not integer!";
                }

                int intPointID = 0;
                if (int.TryParse(objPointID.ToString(), out intPointID) == false)
                {
                    return "RemovePointSetting() Error: Point ID input is not integer!";
                }

                foreach(var item in this.lstclsSettingChartLayer[intDataLayerID].lstSinglePointSetting)
                {
                    if(item.ID == intPointID)
                    {
                        this.lstclsSettingChartLayer[intDataLayerID].lstSinglePointSetting.Remove(item);
                        break;
                    }
                }

                //Refresh chart
                this.AutoSetChart();
            }
            catch (Exception ex)
            {
                return "RemovePointSetting() Error: " + ex.Message;
            }

            //
            return strRet;
        }
        //*******************************************************************************************************
        public string RemoveChart(object objNumDataLayerID)
        {
            string strRet = "0";
            //*********************************************
            try
            {
                //Remove chart line
                int intNumDataLayerID = 0;
                if (int.TryParse(objNumDataLayerID.ToString(), out intNumDataLayerID) == false) return "SettingChart() Error: The NumDataLayerID setting is not integer: [" + objNumDataLayerID.ToString() + "]";
                if (this.lstclsDataChartLayer == null) return "SettingChart() Error: lstclsDataChartLayer is null!";
                
                if(intNumDataLayerID == -1) //Option to remove all
                {
                    this.RemoveAllChartLineLayer();
                    for(int i= 0;i<this.lstclsDataChartLayer.Count;i++)
                    {
                        this.lstclsDataChartLayer.RemoveAt(i);
                    }
                    this.lstclsDataChartLayer = new List<clsDataChartLayer>();
                }
                else //remove only 1 Data Layer
                {
                    if ((this.lstclsDataChartLayer.Count - 1) < intNumDataLayerID) return "SettingChart() Error: The ID of Data Layer is out of range!";
                    //Remove chart line
                    this.RemoveChartLineLayer(intNumDataLayerID);
                    this.lstclsDataChartLayer.RemoveAt(intNumDataLayerID);
                }

                //Apply change
                this.AutoSetChart();
            }
            catch (Exception ex)
            {
                return "RemoveChart() error: " + ex.Message;
            }
            //*********************************************
            return strRet;
        }

        //*******************************************************************************************************
        public string ClearChart()
        {
            string strRet = "";
            //
            //Check null
            if (this.MyCanvasAdornerLayer == null) return "Error: CanvasAdornerLayer is null";

            //Clear all content on canvas
            //this.MyCanvas.Children.Clear();

            //Remove all adorner layer
            this.RemoveAllChartLineLayer();
            this.RemoveChartLegendLayer();
            this.RemoveUserLineLayer();

            //
            this.lstclsUserLineData = new List<classUserLineData>();
            this.lstclsUserInfo = new List<clsUserInfoDisplay>();

            this.clsChartLegendLayer = new classDisplayChartLegendLayer();
            this.clsUserLineLayer = new classUserLineLayer();
            this.lstclsDataChartLayer = new List<clsDataChartLayer>();

            //Clear all content on canvas
            this.MyCanvas.Children.Clear();

            //
            return strRet;
        }

        
        //*******************************************************************************************************
        public List<clsUserInfoDisplay> lstclsUserInfo { get; set; }
        //public string strMyUserInfo = "";
        public string DisplayUserInfo(object objTitle, object strContent)
        {
            string strRet = "0";
            //*********************************************
            clsUserInfoDisplay clsTemp = new clsUserInfoDisplay();
            clsTemp.strTitle = objTitle.ToString();
            clsTemp.strContent = strContent.ToString();

            lstclsUserInfo = new List<clsUserInfoDisplay>();
            lstclsUserInfo.Add(clsTemp);

            //*********************************************
            SetUserInfoFormatText();
            return strRet;
        }

        public string AddUserInfo(object objTitle, object objContent)
        {
            string strRet = "0";
            //*********************************************
            if(lstclsUserInfo==null)
            {
                lstclsUserInfo = new List<clsUserInfoDisplay>();
            }

            //
            clsUserInfoDisplay clsTemp = new clsUserInfoDisplay();
            clsTemp.strTitle = objTitle.ToString();
            clsTemp.strContent = objContent.ToString();

            lstclsUserInfo.Add(clsTemp);
            //*********************************************
            SetUserInfoFormatText();
            return strRet;
        }

        public string UpdateUserInfo(object objTitle, object strContent)
        {
            string strRet = "0";
            //*********************************************
            //Looking for user info has title which want to change content
            int i = 0;
            bool blFound = false;
            for (i = 0; i < this.lstclsUserInfo.Count;i++)
            {
                if(this.lstclsUserInfo[i].strTitle.Trim().ToUpper() == objTitle.ToString().Trim().ToUpper()) //found
                {
                    blFound = true;
                    break;
                }
            }

            if (blFound == false) return "UpdateUserInfo(): cannot find Title [" + objTitle.ToString() + "]";
            //Update info if everything is OK
            this.lstclsUserInfo[i].strContent = strContent.ToString();

            //*********************************************
            SetUserInfoFormatText();
            return strRet;
        }

        public string SetContentColor(object objTitle, object objColor)
        {
            string strRet = "0";
            //*********************************************

            //Looking for user info has title which want to change content
            int i = 0;
            bool blFound = false;
            for (i = 0; i < this.lstclsUserInfo.Count; i++)
            {
                if (this.lstclsUserInfo[i].strTitle.Trim().ToUpper() == objTitle.ToString().Trim().ToUpper()) //found
                {
                    blFound = true;
                    break;
                }
            }

            if (blFound == false) return "SettingContentInfo(): cannot find Title [" + objTitle.ToString() + "]";

            //Update info if everything is OK
            Color MyColor = new Color();
            if (!(objColor is Color))
            {
                MyColor = (Color)ColorConverter.ConvertFromString(objColor.ToString());
            }
            this.lstclsUserInfo[i].clrContent = MyColor;

            //*********************************************
            SetUserInfoFormatText();
            return strRet;
        }

        public string SetContentSize(object strTitle, object objSize)
        {
            string strRet = "0";
            //*********************************************

            //Looking for user info has title which want to change content
            int i = 0;
            bool blFound = false;
            for (i = 0; i < this.lstclsUserInfo.Count; i++)
            {
                if (this.lstclsUserInfo[i].strTitle.Trim().ToUpper() == strTitle.ToString().Trim().ToUpper()) //found
                {
                    blFound = true;
                    break;
                }
            }

            if (blFound == false) return "SetContentSize(): cannot find Title [" + strTitle.ToString() + "]";

            //Update info if everything is OK
            double dblNewSize = 0;
            if (double.TryParse(objSize.ToString(), out dblNewSize) == false) return "SetContentSize(): New Size input is not numeric!";
            this.lstclsUserInfo[i].dblContentFontSize = dblNewSize;

            //*********************************************
            SetUserInfoFormatText();
            return strRet;
        }

        public void SetUserInfoFormatText()
        {
            //Default format:
            //    Tittle info (Bold, Italic, underline) + ":"
            //    Content info (Bold, Blue)
            //    "__________"
            int i = 0;

            //Clear old contents
            this.UserInfoTextBlock.Inlines.Clear();

            //Add again new update information
            for(i=0;i<this.lstclsUserInfo.Count;i++)
            {
                this.UserInfoTextBlock.Inlines.Add(new Run(this.lstclsUserInfo[i].strTitle + ": \r\n") { FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(this.lstclsUserInfo[i].clrTitle)});
                this.UserInfoTextBlock.Inlines.Add(new Run(this.lstclsUserInfo[i].strContent + "\r\n") { FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(this.lstclsUserInfo[i].clrContent),
                                                                                                           FontSize = this.lstclsUserInfo[i].dblContentFontSize});
                this.UserInfoTextBlock.Inlines.Add("________\r\n");
            }

        }

        //Chart title
        public void SetChartTitle(object objTitle)
        {
            //
            this.settingChart.strChartTitle = objTitle.ToString();
            //
            this.AutoSetChart();
        }

        public void SetChartUnit(string strXUnit, string strYUnit)
        {
            this.settingChart.strXUnitName = strXUnit;
            this.settingChart.strYUnitName = strYUnit;
            //
            this.AutoSetChart();
        }


        //**************************** SUPPORT FUNCTION *********************************************************
        #region _supportFunction

        /// <summary>
        /// Format a number with scientific exponents and specified sigificant digits.
        /// </summary>
        /// <param name="x">The value to format</param>
        /// <param name="significant_digits">The number of siginicant digits to show</param>
        /// <returns>The fomratted string</returns>
        public string Sci(double x, int significant_digits)
        {
            //Check for special numbers and non-numbers
            if (double.IsInfinity(x) || double.IsNaN(x) || x == 0)
            {
                return x.ToString();
            }

            //Check if x is not so much digit? If so, just keep x format
            if (x.ToString().Length <= (significant_digits+1)) return x.ToString(); //+1 for negative nunber - character

            //Check if x is in range (0.001 -> 1000): Default setting is not convert to exponential value
            if ((x >= 0.001) && (x <= 999)) return Math.Round(x, significant_digits).ToString();
            if ((x >= -999) && (x <= -0.001)) return Math.Round(x, significant_digits).ToString();

            // extract sign so we deal with positive numbers only
            int sign = Math.Sign(x);
            x = Math.Abs(x);
            // get scientific exponent, 10^3, 10^6, ...
            int sci = (int)Math.Floor(Math.Log(x, 10) / 3) * 3;
            // scale number to exponent found
            x = x * Math.Pow(10, -sci);
            // find number of digits to the left of the decimal
            int dg = (int)Math.Floor(Math.Log(x, 10)) + 1;
            // adjust decimals to display
            int decimals = Math.Min(significant_digits - dg, 15);
            // format for the decimals
            string fmt = new string('0', decimals);
            if (sci == 0)
            {
                //no exponent
                return string.Format("{0}{1:0." + fmt + "}",
                    sign < 0 ? "-" : string.Empty,
                    Math.Round(x, decimals));
            }
            int index = sci / 3 + 6;
            // with 10^exp format
            return string.Format("{0}{1:0." + fmt + "}e{2}",
                sign < 0 ? "-" : string.Empty,
                Math.Round(x, decimals),
                sci);
        }

        private Size MeasureString(string strInput, FontFamily fFamily, FontStyle fStyle, FontWeight fWeight, FontStretch fStretch, double fSize)
        {
            var formattedText = new FormattedText(
                strInput,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(fFamily, fStyle, fWeight, fStretch),
                fSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private double MeasureLabelWidth(Label labelInput)
        {
            var formattedText = new FormattedText(
                labelInput.Content.ToString(),
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(labelInput.FontFamily, labelInput.FontStyle, labelInput.FontWeight, labelInput.FontStretch),
                labelInput.FontSize,
                Brushes.Black);

            return formattedText.Width;
        }

        private double MeasureLabelHeight(Label labelInput)
        {
            var formattedText = new FormattedText(
                labelInput.Content.ToString(),
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(labelInput.FontFamily, labelInput.FontStyle, labelInput.FontWeight, labelInput.FontStretch),
                labelInput.FontSize,
                Brushes.Black);

            return formattedText.Height;
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.lstclsDataChartLayer == null) return;
            //
            int i = 0;
            for(i=0;i<this.lstclsDataChartLayer.Count;i++)
            {
                this.lstclsDataChartLayer[i].lstclsDataChart.Clear();
                this.lstclsDataChartLayer[i].lstclsDataChart = null;
                this.lstclsDataChartLayer[i].DisplayDatalayer.MyAdorner = null;
            }
            this.lstclsDataChartLayer.Clear();
            this.lstclsDataChartLayer = null;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.blIsClosed = true;
        }

        private bool blRunStop { get; set; }
        private void btnRunStop_Click(object sender, RoutedEventArgs e)
        {
            //blRunStop = !(blRunStop);
            //if (blRunStop)
            //{
            //    this.myTimer.Start();
            //}
            //else
            //{
            //    this.myTimer.Stop();
            //}
        }

        private void btnPanX_Click(object sender, RoutedEventArgs e)
        {
            this.intPanMode = intPanX;
        }

        private void btnPanY_Click(object sender, RoutedEventArgs e)
        {
            this.intPanMode = intPanY;
        }

        private void btnPanXY_Click(object sender, RoutedEventArgs e)
        {
            this.intPanMode = intPanXY;
        }

        private void btnZoomXYMinus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomXYMinus;
            //
            double x = 0.79;
            this.ChangeChartScale(x * this.dblBgrdXscale, x * this.dblBgrdYscale);
        }

        private void btnZoomXYPlus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomXYPlus;
            //
            double x = 1.26;
            this.ChangeChartScale(x * this.dblBgrdXscale, x * this.dblBgrdYscale);
        }

        private void btnZoomXMinus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomXMinus;
            //
            double x = 0.79;
            this.ChangeChartXScale(x * this.dblBgrdXscale);
        }

        private void btnZoomXPlus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomXPlus;
            //
            double x = 1.26;
            this.ChangeChartXScale(x * this.dblBgrdXscale);
        }

        private void btnZoomYMinus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomYMinus;
            //
            double x = 0.79;
            this.ChangeChartYScale(x * this.dblBgrdYscale);
        }

        private void btnZoomYPlus_Click(object sender, RoutedEventArgs e)
        {
            this.intZoomMode = intZoomYPlus;
            //
            double x = 1.26;
            this.ChangeChartYScale(x * this.dblBgrdYscale);
        }

        private void btnApplySetting_Click(object sender, RoutedEventArgs e)
        {
            double dblTemp = 0;
            double dblHiX = 0;
            double dblHiY = 0;
            //
            if (double.TryParse(this.tbLowX.Text, out dblTemp) == false)
            {
                MessageBox.Show("Low limit of X setting is not numeric!", "Apply Manual Setting Fail");
                return;
            }
            this.dblBgrdXOriginVal = dblTemp;
            //
            if (double.TryParse(this.tbLowY.Text, out dblTemp) == false)
            {
                MessageBox.Show("Low limit of Y setting is not numeric!", "Apply Manual Setting Fail");
                return;
            }
            this.dblBgrdYOriginVal = dblTemp;
            //
            if (double.TryParse(this.tbHiX.Text, out dblHiX) == false)
            {
                MessageBox.Show("High limit of X setting is not numeric!", "Apply Manual Setting Fail");
                return;
            }
            if(dblHiX<=this.dblBgrdXOriginVal)
            {
                MessageBox.Show("High limit of X setting is not greater than Low limit!", "Apply Manual Setting Fail");
                return;
            }
            //
            if (double.TryParse(this.tbHiY.Text, out dblHiY) == false)
            {
                MessageBox.Show("High limit of Y setting is not numeric!", "Apply Manual Setting Fail");
                return;
            }
            if (dblHiY <= this.dblBgrdYOriginVal)
            {
                MessageBox.Show("High limit of Y setting is not greater than Low limit!", "Apply Manual Setting Fail");
                return;
            }
            //
            this.dblBgrdXRange = dblHiX - this.dblBgrdXOriginVal;
            this.dblBgrdXscale = this.dblChartWidth / this.dblBgrdXRange;

            this.dblBgrdYRange = dblHiY - this.dblBgrdYOriginVal;
            this.dblBgrdYscale = this.dblChartHeight / this.dblBgrdYRange;
            
            //
            int intTemp = 0;
            if (int.TryParse(this.tbDivX.Text, out intTemp) == false)
            {
                MessageBox.Show("Divide of X setting is not integer!", "Apply Manual Setting Fail");
                return;
            }
            this.settingChart.intNumYdivide = intTemp;

            if (int.TryParse(this.tbDivY.Text, out intTemp) == false)
            {
                MessageBox.Show("Divide of Y setting is not integer!", "Apply Manual Setting Fail");
                return;
            }
            this.settingChart.intNumXdivide = intTemp;

            //////////////////////////////////////////////////////////////
            //Draw chart again
            //Clear all chart and draw again
            this.RemoveAllChartLineLayer();
            this.RemoveUserLineLayer();
            this.MyCanvas.Children.Clear();

            //Draw background again
            this.DrawChartBackGround();

            //Display chart line again

            this.DisplayAllChartLine();

            //Chart legend
            this.RemoveChartLegendLayer();
            this.DisplayChartLegend();
            this.DisplayUserLine();
        }

        //******************************************************************************************************
    }

}
