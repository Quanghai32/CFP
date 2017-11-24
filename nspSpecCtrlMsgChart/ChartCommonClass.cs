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

namespace nspSpecMsgChart
{
    //*******************************************************************************************************
    //Adorner Control class
    public class ControlAdorner : Adorner
    {
        private Control _child;

        public ControlAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            //Testing
            //Adorner[] myadorner = adornedElement.geta
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return _child;
        }

        public Control Child
        {
            get
            {
                return _child;
            }
            set
            {
                if (_child != null)
                {
                    RemoveVisualChild(_child);
                }
                _child = value;
                if (_child != null)
                {
                    AddVisualChild(_child);
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(_child.ActualWidth, _child.ActualHeight);
        }
    }

    //*******************************************************************************************************
    public class ThresholdColor
    {
        public List<double> lstdblThresholdValue { get; set; }
        public List<System.Windows.Media.SolidColorBrush> lstclrThresholdColor { get; set; }

        public ThresholdColor()
        {
            this.lstdblThresholdValue = new List<double>();
            this.lstclrThresholdColor = new List<SolidColorBrush>();
        }
    }

    //User Control - Display chart data on background
    public class classDisplayDataChartLayer : UserControl
    {
        //
        public ControlAdorner MyAdorner { get; set; }

        //For General color
        public System.Windows.Media.SolidColorBrush clrMyColor { get; set; }
        //For Threshhold color setting
        public SettingChartLayer settingChartLayer { get; set; }

        //Private setting for some kind of chart
        public double dblMarkerSize { get; set; } //Size of circle in Scatter style of "Line with marker" style

        //*******************************************************************************************************
        public int intChartXDirection { get; set; } //0 - Default (left to right). 1 - Reverse: Right to left
        public int intChartYDirection { get; set; } //0 - Default (Bot to Top). 1 - Reverse: Top to Bot
        public double dblXLolimitCor { get; set; } //The limit of chart area
        public double dblXHilimitCor { get; set; }

        public double dblYLolimitCor { get; set; } //The limit of chart area
        public double dblYHilimitCor { get; set; }

        public bool isInsideLimitArea(Point clsPointInput)
        {
            if ((clsPointInput.X < this.dblXLolimitCor) || (clsPointInput.X > this.dblXHilimitCor))
            {
                return false;
            }

            if ((clsPointInput.Y < this.dblYLolimitCor) || (clsPointInput.Y > this.dblYHilimitCor))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public string FindLineEquation(Point P1, Point P2, out double a, out double b, out double c)
        {
            a = 0;
            b = 0;
            c = 0;

            //Check if 2 lines input is valid
            if ((P1.X == P2.X) && (P1.Y == P2.Y)) return "Error: Line1 not define because P1 & P2 is same point!";

            //Find equation of Line 1 (P11--------P12) : a*x + b*y + c = 0
            //a*x1 + b*y1 + c = 0 
            //a*x2 + b*y2 + c = 0
            // => a(x1-x2) + b(y1-y2) = 0

            if (P1.X == P2.X) //x1=x2 => b(y1-y2) = 0 => b = 0 (because y1 != y2)
            {
                b = 0; //a*x + c = 0; a != 0 => a*x1 + c = 0 => c = -a*x1
                //select a = 1 => c = -x1
                a = 1;
                c = -P1.X;
            }
            else if (P1.Y == P2.Y) // a = 0;
            {
                a = 0; //b*y1+c = 0. Select b = 1 => c = - y1
                b = 1;
                c = -P1.Y;
            }
            else // b/a = -(x1-x2)/(y1-y2); Select a = 1 => b = -(x1-x2)/(y1-y2). c = -(a*x1+b*y1) = -[x1 - (x1-x2)/(y1-y2)*y1]
            {
                a = 1;
                b = (P2.X - P1.X) / (P1.Y - P2.Y);
                c = P1.Y * (P1.X - P2.X) / (P1.Y - P2.Y) - P1.X;
            }

            return "0";
        }

        /// <summary>
        /// Return a point of intersection of 2 line if have (if not, return...)
        /// </summary>
        /// <param name="P11"></param>
        /// <param name="P12"></param>
        /// <param name="P21"></param>
        /// <param name="P22"></param>
        /// <returns></returns>
        public object FindIntersection2Lines(Point P11, Point P12, Point P21, Point P22)
        {
            string strTest = "";
            //Check if 2 lines input is valid
            double a1 = 0;
            double b1 = 0;
            double c1 = 0;
            strTest = FindLineEquation(P11, P12, out a1, out b1, out c1);
            if (strTest != "0") return "Error: Cannot find equation of line 1";

            double a2 = 0;
            double b2 = 0;
            double c2 = 0;
            strTest = FindLineEquation(P21, P22, out a2, out b2, out c2);
            if (strTest != "0") return "Error: Cannot find equation of line 2";

            //OK, we already got 2 equation of 2 lines. Now find intersection point of 2 lines!
            // a1*x + b1*y + c1 = 0
            // a2*x + b2*y + c2 = 0

            //Convert to use avaible equation
            // a1*x + b1*y = -c1
            // a2*x + b2*y = -c2

            double D = a1 * b2 - a2 * b1;
            double Dx = -c1 * b2 + c2 * b1;
            double Dy = -a1 * c2 + a2 * c1;

            if (D == 0) return "Cannot find point of Intersection!";

            double x = Dx / D;
            double y = Dy / D;

            //we need to verify if intersection point is inside two line or not
            bool blInside = true;
            //Check line1 
            if (P11.X >= P12.X)
            {
                if ((x > P11.X) || (x < P12.X)) blInside = false;
            }
            else
            {
                if ((x > P12.X) || (x < P11.X)) blInside = false;
            }
            if (P11.Y >= P12.Y)
            {
                if ((y > P11.Y) || (y < P12.Y)) blInside = false;
            }
            else
            {
                if ((y > P12.Y) || (y < P11.Y)) blInside = false;
            }
            //Check line 2
            if (P21.X >= P22.X)
            {
                if ((x > P21.X) || (x < P22.X)) blInside = false;
            }
            else
            {
                if ((x > P22.X) || (x < P21.X)) blInside = false;
            }
            if (P21.Y >= P22.Y)
            {
                if ((y > P21.Y) || (y < P22.Y)) blInside = false;
            }
            else
            {
                if ((y > P22.Y) || (y < P21.Y)) blInside = false;
            }

            //
            if (blInside == false) return "Intersection point is outside 2 lines!";

            //Point pRet = new Point(x, y);
            Point pRet = new Point(x, y);
            return pRet;
        }
        public object DrawLineInsideRect(Point LineP1, Point LineP2)
        {
            Line line = new Line();

            //Find Rectangle limit area inside point & outside point of Line Input
            bool blTemp1 = isInsideLimitArea(LineP1);
            bool blTemp2 = isInsideLimitArea(LineP2);

            if ((blTemp1 == true) && (blTemp2 == true)) //inside limit area => draw all
            {
                //Drawing line from point 1 to point 2
                line = new Line();
                //line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.X1 = LineP1.X;
                line.Y1 = LineP1.Y;
                line.X2 = LineP2.X;
                line.Y2 = LineP2.Y;

                line.Visibility = System.Windows.Visibility.Visible;
                line.Stroke = this.clrMyColor;

                return line;
            }

            if ((blTemp1 == false) && (blTemp2 == false)) //Outside limit area => No need draw
            {
                return "No Line. All point is outside limit area!";
            }

            //The line cut rectangle
            Point pointInside = new Point(0, 0);
            Point pointOutSide = new Point(0, 0);

            if (blTemp1 == true)
            {
                pointInside = LineP1;
                pointOutSide = LineP2;
            }
            else
            {
                pointInside = LineP2;
                pointOutSide = LineP1;
            }

            //4 point of rectangle
            Point RectP1 = new Point(this.dblXLolimitCor, this.dblYHilimitCor);
            Point RectP2 = new Point(this.dblXLolimitCor, this.dblYLolimitCor);
            Point RectP3 = new Point(this.dblXHilimitCor, this.dblYLolimitCor);
            Point RectP4 = new Point(this.dblXHilimitCor, this.dblYHilimitCor);

            //Check intersection of Line input with 4 line of rectangle to find intersection line
            object objRet = new object();
            objRet = FindIntersection2Lines(pointInside, pointOutSide, RectP1, RectP2);
            if (objRet is Point) //Found => Draw line & return
            {
                Point pointOnEdge = (Point)objRet;

                //if ((pointOnEdge.Y >= this.dblYLolimitCor) && (pointOnEdge.Y <= this.dblYHilimitCor) && (pointOnEdge.X <= pointInside.X) && (pointOnEdge.X >= pointOutSide.X)) //Valid value
                //{
                line = new Line();
                //line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.X1 = pointOnEdge.X;
                line.Y1 = pointOnEdge.Y;
                line.X2 = pointInside.X;
                line.Y2 = pointInside.Y;

                line.Visibility = System.Windows.Visibility.Visible;
                line.Stroke = this.clrMyColor;

                return line;
                //}
            }

            objRet = FindIntersection2Lines(pointInside, pointOutSide, RectP2, RectP3);
            if (objRet is Point) //Found => Draw line & return
            {
                Point pointOnEdge = (Point)objRet;

                //if ((pointOnEdge.X >= this.dblXLolimitCor) && (pointOnEdge.X <= this.dblXHilimitCor) && (pointOnEdge.Y <= pointInside.Y) && (pointOnEdge.Y >= pointOutSide.Y)) //Valid value
                //{
                line = new Line();
                //line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.X1 = pointOnEdge.X;
                line.Y1 = pointOnEdge.Y;
                line.X2 = pointInside.X;
                line.Y2 = pointInside.Y;

                line.Visibility = System.Windows.Visibility.Visible;
                line.Stroke = this.clrMyColor;

                return line;
                // }
            }

            objRet = FindIntersection2Lines(pointInside, pointOutSide, RectP3, RectP4);
            if (objRet is Point) //Found => Draw line & return
            {
                Point pointOnEdge = (Point)objRet;

                //if ((pointOnEdge.Y >= this.dblYLolimitCor) && (pointOnEdge.Y <= this.dblYHilimitCor) && (pointOnEdge.X >= pointInside.X) && (pointOnEdge.X <= pointOutSide.X)) //Valid value
                //{
                line = new Line();
                //line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.X1 = pointOnEdge.X;
                line.Y1 = pointOnEdge.Y;
                line.X2 = pointInside.X;
                line.Y2 = pointInside.Y;

                line.Visibility = System.Windows.Visibility.Visible;
                line.Stroke = this.clrMyColor;

                return line;
                //}
            }

            objRet = FindIntersection2Lines(pointInside, pointOutSide, RectP4, RectP1);
            if (objRet is Point) //Found => Draw line & return
            {
                Point pointOnEdge = (Point)objRet;

                //if ((pointOnEdge.X >= this.dblXLolimitCor) && (pointOnEdge.X <= this.dblXHilimitCor) && (pointOnEdge.Y >= pointInside.Y) && (pointOnEdge.Y <= pointOutSide.Y)) //Valid value
                //{
                line = new Line();
                //line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.X1 = pointOnEdge.X;
                line.Y1 = pointOnEdge.Y;
                line.X2 = pointInside.X;
                line.Y2 = pointInside.Y;

                line.Visibility = System.Windows.Visibility.Visible;
                line.Stroke = this.clrMyColor;

                return line;
                //}

            }

            return "No intersection line found!";
        }

        //*******************************************************************************************************
        //Create some chart style
        public Canvas ChartStyleLine(List<Point> lstCoordinateInput, List<Point> lstPointOriginChartData)
        {
            //This Line style chart: just include simple line
            Canvas canvasRet = new Canvas();
            int i = 0;

            //Check list coordinate input - must be contain at least 2 point
            if (lstCoordinateInput.Count < 2) return canvasRet;
            //If OK, then we draw chart line on canvas and return
            for (i = 0; i < lstCoordinateInput.Count - 1; i++)
            {
                Point clsCor1 = lstCoordinateInput[i];
                Point clsCor2 = lstCoordinateInput[i + 1];

                object objResult = DrawLineInsideRect(clsCor1, clsCor2);

                if (objResult is Line) //Found & need to draw
                {
                    Line line = (Line)objResult;
                    canvasRet.Children.Add(line);
                }
            }

            //Test scale tranform
            return canvasRet;
        }
        public Canvas ChartStyleScatter(List<Point> lstCoordinateInput, List<Point> lstPointOriginChartData)
        {
            //This Line style chart: just include simple line
            Canvas canvasRet = new Canvas();
            int i = 0;

            //Check list coordinate input - must be contain at least 2 point
            //if (lstCoordinateInput.Count < 2) return canvasRet;

            //If OK, then we draw chart line on canvas and return
            for (i = 0; i < lstCoordinateInput.Count; i++)
            {
                if (this.isInsideLimitArea(lstCoordinateInput[i]) == true)
                {
                    Ellipse CirclePoint = new Ellipse();
                    CirclePoint.Width = this.dblMarkerSize;
                    CirclePoint.Height = this.dblMarkerSize;

                    //Apply Threshold setting for color follow value
                    //For X axis
                    SolidColorBrush clrTemp1 = this.CalThresholdColor(this.settingChartLayer.clsThresHoldColorX, lstPointOriginChartData[i].X, this.clrMyColor);
                    //For Y axis
                    SolidColorBrush clrTemp2 = this.CalThresholdColor(this.settingChartLayer.clsThresHoldColorY, lstPointOriginChartData[i].Y, clrTemp1);

                    //Apply for Invidual Point
                    SolidColorBrush clrTemp3 = clrTemp2;
                    foreach(var item in this.settingChartLayer.lstSinglePointSetting)
                    {
                        if(item.ID == i)
                        {
                            clrTemp3 = item.solidColor;
                            break;
                        }
                    }

                    CirclePoint.Stroke = clrTemp3;
                    CirclePoint.Fill = clrTemp3;

                    canvasRet.Children.Add(CirclePoint);

                    Canvas.SetLeft(CirclePoint, (lstCoordinateInput[i].X - CirclePoint.Width / 2));
                    Canvas.SetTop(CirclePoint, (lstCoordinateInput[i].Y - CirclePoint.Height / 2));
                }
            }

            return canvasRet;
        }
        public Canvas ChartStyleLineWithMarker(List<Point> lstCoordinateInput, List<Point> lstPointOriginChartData)
        {
            //This Line style chart: just include simple line
            Canvas canvasRet = new Canvas();
            int i = 0;

            //Draw Markers
            for (i = 0; i < lstCoordinateInput.Count; i++)
            {
                if (this.isInsideLimitArea(lstCoordinateInput[i]) == true)
                {
                    Ellipse CirclePoint = new Ellipse();
                    CirclePoint.SnapsToDevicePixels = true; CirclePoint.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                    CirclePoint.Width = this.dblMarkerSize;
                    CirclePoint.Height = this.dblMarkerSize;

                    //Apply Threshold setting for color follow value
                    //For X axis
                    SolidColorBrush clrTemp1 = this.CalThresholdColor(this.settingChartLayer.clsThresHoldColorX, lstPointOriginChartData[i].X, this.clrMyColor);
                    //For Y axis
                    SolidColorBrush clrTemp2 = this.CalThresholdColor(this.settingChartLayer.clsThresHoldColorY, lstPointOriginChartData[i].Y, clrTemp1);

                    //Apply for Invidual Point
                    SolidColorBrush clrTemp3 = clrTemp2;
                    foreach (var item in this.settingChartLayer.lstSinglePointSetting)
                    {
                        if (item.ID == i)
                        {
                            clrTemp3 = item.solidColor;
                            break;
                        }
                    }

                    CirclePoint.Stroke = clrTemp3;
                    CirclePoint.Fill = clrTemp3;

                    canvasRet.Children.Add(CirclePoint);

                    Canvas.SetLeft(CirclePoint, (lstCoordinateInput[i].X - CirclePoint.Width / 2));
                    Canvas.SetTop(CirclePoint, (lstCoordinateInput[i].Y - CirclePoint.Height / 2));
                }
            }

            //Check list coordinate input - must be contain at least 2 point
            if (lstCoordinateInput.Count < 2) return canvasRet;
            //If OK, then we draw chart line on canvas and return

            //Draw Lines
            for (i = 0; i < lstCoordinateInput.Count - 1; i++)
            {
                Point clsCor1 = lstCoordinateInput[i];
                Point clsCor2 = lstCoordinateInput[i + 1];

                //Drawing line from point 1 to point 2
                object objResult = DrawLineInsideRect(clsCor1, clsCor2);

                if (objResult is Line) //Found & need to draw
                {
                    Line line = (Line)objResult;
                    canvasRet.Children.Add(line);
                }
            }

            return canvasRet;
        }

        //
        public SolidColorBrush CalThresholdColor(ThresholdColor thresholdColorInput, double dblInputVal, SolidColorBrush clrDefault)
        {
            SolidColorBrush clrRet = clrDefault;
            //
            if (thresholdColorInput.lstdblThresholdValue.Count > 0) //There is setting for Threshold value-color
            {
                if (thresholdColorInput.lstclrThresholdColor.Count == thresholdColorInput.lstdblThresholdValue.Count + 1) //Correct setting
                {
                    //Looking for range of Y coordinate value
                    if (dblInputVal <= thresholdColorInput.lstdblThresholdValue[0]) //Smaller than the smallest
                    {
                        clrRet = thresholdColorInput.lstclrThresholdColor[0]; //Get the first color if value less than the smallest threshold vlaue
                    }
                    else if (dblInputVal >= thresholdColorInput.lstdblThresholdValue[thresholdColorInput.lstdblThresholdValue.Count - 1]) //Bigger than the biggest
                    {
                        clrRet = thresholdColorInput.lstclrThresholdColor[thresholdColorInput.lstclrThresholdColor.Count - 1]; //Get the last color
                    }
                    else //Need to find correct range
                    {
                        for (int j = 0; j < thresholdColorInput.lstdblThresholdValue.Count; j++)
                        {
                            if (j == thresholdColorInput.lstdblThresholdValue.Count - 1) //reach the last one
                            {
                                clrRet = thresholdColorInput.lstclrThresholdColor[j + 1];
                                break;
                            }
                            //
                            if ((dblInputVal >= thresholdColorInput.lstdblThresholdValue[j]) &&
                                (dblInputVal <= thresholdColorInput.lstdblThresholdValue[j + 1])) //Found range
                            {
                                clrRet = thresholdColorInput.lstclrThresholdColor[j + 1];
                                break;
                            }
                        }
                    }
                }
            }

            //
            return clrRet;
        }

        //
        public void DrawChartOnLayer(List<Point> lstclsCoordinate, List<Point> lstPointOriginChartData, int intChartStyleID = 0)
        {
            if (this.HasContent == true)
            {
                this.Content = null;
            }

            switch (intChartStyleID)
            {
                case 0: //Line only
                    this.AddChild(this.ChartStyleLine(lstclsCoordinate, lstPointOriginChartData));
                    break;
                case 1: //Scatter chart
                    this.AddChild(this.ChartStyleScatter(lstclsCoordinate, lstPointOriginChartData));
                    break;
                case 2:
                    this.AddChild(this.ChartStyleLineWithMarker(lstclsCoordinate, lstPointOriginChartData));
                    break;
                default:
                    this.AddChild(this.ChartStyleLine(lstclsCoordinate, lstPointOriginChartData));
                    break;
            }
        }
        //Constructor
        public classDisplayDataChartLayer()
        {
            //Default value
            this.clrMyColor = System.Windows.Media.Brushes.Black;
            this.dblMarkerSize = 5;
            //
            this.MyAdorner = new ControlAdorner(this)
            {
                Child = this,
            };
            //
            this.settingChartLayer = new SettingChartLayer();
        }
    }

    //*******************************************************************************************************
    public class clsLegendData
    {
        public string strChartName { get; set; }
        public System.Windows.Media.SolidColorBrush clrMyColor { get; set; }

        public clsLegendData()
        {
            this.strChartName = "";
            this.clrMyColor = new SolidColorBrush(Colors.Black);
        }
    }

    public class classDisplayChartLegendLayer : UserControl
    {
        //
        public ControlAdorner MyAdorner { get; set; }

        public int intNumChart { get; set; } //How many Chart exist
        public List<clsLegendData> lstclsLegendData { get; set; }

        public double dblXLolimitCor { get; set; } //The limit of chart legend area
        public double dblXHilimitCor { get; set; }

        public double dblYLolimitCor { get; set; } //The limit of chart legend area
        public double dblYHilimitCor { get; set; }

        public double dblRectSize { get; set; }

        //
        public Canvas ChartLegendClassic()
        {
            Canvas canvasRet = new Canvas();

            if (this.lstclsLegendData == null) return canvasRet;
            if (this.lstclsLegendData.Count == 0) return canvasRet;
            //**************************************************

            //Cal how many chart legend can be display on 1 row
            double dblRowWidth = this.dblXHilimitCor - this.dblXLolimitCor;

            //find max length need for 1 legend
            int i, j = 0;
            Label myLabel = new System.Windows.Controls.Label();
            double dblMaxLength = 0;

            dblMaxLength = this.dblRectSize;
            myLabel.Content = this.lstclsLegendData[0].strChartName;
            dblMaxLength += this.MeasureLabelWidth(myLabel) + 10;

            for (i = 0; i < this.lstclsLegendData.Count; i++)
            {
                myLabel = new System.Windows.Controls.Label();
                myLabel.Content = this.lstclsLegendData[i].strChartName;

                //Cal
                double dblMyLength = this.dblRectSize + this.MeasureLabelWidth(myLabel) + 10;
                if (dblMyLength > dblMaxLength) dblMaxLength = dblMyLength;
            }

            //Cal how many legend chart can be display on 1 row 
            int intNumLegendOnRow = Convert.ToInt32(dblRowWidth / dblMaxLength);
            if (intNumLegendOnRow < 1) intNumLegendOnRow = 1;

            //Cal how many row need
            double dblTemp = (double)this.lstclsLegendData.Count / (double)intNumLegendOnRow;
            int intNumRowLegend = Convert.ToInt32(Math.Ceiling((double)dblTemp));

            //Draw all chart Legend 
            for (i = 0; i < intNumRowLegend; i++)
            {
                for (j = 0; j < intNumLegendOnRow; j++)
                {
                    int intChartID = i * intNumLegendOnRow + j;
                    if (intChartID > (this.lstclsLegendData.Count - 1)) break;

                    //
                    Rectangle rect = new Rectangle();

                    rect.Width = this.dblRectSize;
                    rect.Height = this.dblRectSize;

                    rect.StrokeThickness = 1;
                    rect.Fill = this.lstclsLegendData[intChartID].clrMyColor;

                    canvasRet.Children.Add(rect);

                    double dblLeft = this.dblXLolimitCor + j * dblMaxLength;
                    double dblTop = this.dblYLolimitCor + i * (this.dblRectSize + 10);

                    Canvas.SetLeft(rect, dblLeft);
                    Canvas.SetTop(rect, dblTop);

                    //2. Add label
                    myLabel = new System.Windows.Controls.Label();

                    myLabel.Content = this.lstclsLegendData[intChartID].strChartName;
                    myLabel.Foreground = System.Windows.Media.Brushes.Black;
                    canvasRet.Children.Add(myLabel);

                    double dbllabelLeft = dblLeft + this.dblRectSize;
                    double dbllabelTop = dblTop + this.dblRectSize - this.MeasureLabelHeight(myLabel) - 2;

                    Canvas.SetLeft(myLabel, dbllabelLeft);
                    Canvas.SetTop(myLabel, dbllabelTop);
                }
            }

            //**************************************************
            return canvasRet;
        }

        //
        public void DrawChartLegend()
        {
            this.AddChild(this.ChartLegendClassic());
        }
        //Constructor
        public classDisplayChartLegendLayer()
        {
            //
            this.MyAdorner = new ControlAdorner(this)
            {
                Child = this,
            };
            //
            this.lstclsLegendData = new List<clsLegendData>();
            this.dblRectSize = 10; //Default value
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

            return (formattedText.Height);
            //return formattedText.Height;
        }
    }

    //*******************************************************************************************************
    public class classUserLineData
    {
        public string strLineName { get; set; } //Name of user line
        public System.Windows.Media.SolidColorBrush clrMyColor { get; set; } //Color of user line
        public int intLineStyleID { get; set; } // 0 - Scatter line (Default). 1 - continuous line
        public int intLineOrientationID { get; set; } // 0 - Vertical Line (Default). 1 - Hozital line
        public double dblUserCorX { get; set; } //this will be used to identify line position
        public double dblUserCorY { get; set; } //this will be used to identify line position

        public double dblUserValX { get; set; } //this will be used to identify line position
        public double dblUserValY { get; set; } //this will be used to identify line position

        public classUserLineData()
        {
            this.strLineName = "";
            this.clrMyColor = new SolidColorBrush(Colors.Black);
            this.intLineStyleID = 0;
            this.intLineOrientationID = 0;
        }
    }

    public class classUserLineLayer : UserControl
    {
        //
        public ControlAdorner MyAdorner { get; set; }

        public int intNumUserLine { get; set; } //How many User Line exist
        public List<classUserLineData> lstclsUserLineData { get; set; }

        public double dblXLolimitCor { get; set; } //The limit of chart legend area
        public double dblXHilimitCor { get; set; }

        public double dblYLolimitCor { get; set; } //The limit of chart legend area
        public double dblYHilimitCor { get; set; }

        //
        public bool isInsideLimitArea(Point clsPointInput)
        {
            if ((clsPointInput.X < this.dblXLolimitCor) || (clsPointInput.X > this.dblXHilimitCor))
            {
                return false;
            }

            if ((clsPointInput.Y < this.dblYLolimitCor) || (clsPointInput.Y > this.dblYHilimitCor))
            {
                return false;
            }

            return true;
        }
        //
        public Canvas UserLineClassic()
        {
            Canvas canvasRet = new Canvas();

            if (this.lstclsUserLineData == null) return canvasRet;
            if (this.lstclsUserLineData.Count == 0) return canvasRet;
            //**************************************************

            //Now draw user line on canvas
            int i = 0;
            for (i = 0; i < this.lstclsUserLineData.Count; i++)
            {
                Line line = new Line();
                if (this.lstclsUserLineData[i].intLineOrientationID == 0) //Vertical line
                {
                    line.X1 = this.lstclsUserLineData[i].dblUserCorX;
                    line.Y1 = this.dblYLolimitCor;
                    line.X2 = line.X1;
                    line.Y2 = this.dblYHilimitCor;
                }
                else //Horizontal line
                {
                    line.X1 = this.dblXLolimitCor;
                    line.Y1 = this.lstclsUserLineData[i].dblUserCorY;
                    line.X2 = this.dblXHilimitCor;
                    line.Y2 = line.Y1;
                }

                line.SnapsToDevicePixels = true; line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.Visibility = System.Windows.Visibility.Visible;

                if (this.lstclsUserLineData[i].intLineStyleID == 1) //Continuous line
                {

                }
                else //Default - Scatter line
                {
                    DoubleCollection dashed = new DoubleCollection();
                    dashed = new DoubleCollection();
                    dashed.Add(10); dashed.Add(10);
                    line.StrokeDashArray = dashed;
                    line.StrokeDashOffset = 0;
                }

                //Add user line name label
                //2. Add label
                var label = new System.Windows.Controls.Label();

                label.Content = this.lstclsUserLineData[i].strLineName;
                label.Foreground = System.Windows.Media.Brushes.Black;

                double dbllabelLeft = 0;
                double dbllabelTop = 0;

                if (this.lstclsUserLineData[i].intLineOrientationID == 0) //Vertical line - label need to adjust orientation 90 degree
                {
                    // Rotate if desired.
                    label.LayoutTransform = new RotateTransform(90);
                    dbllabelLeft = this.lstclsUserLineData[i].dblUserCorX;
                    dbllabelTop = this.dblYHilimitCor - this.MeasureLabelWidth(label) - 10;
                }
                else //Horizontal line
                {
                    dbllabelLeft = this.dblXLolimitCor + 5;
                    dbllabelTop = this.lstclsUserLineData[i].dblUserCorY - this.MeasureLabelHeight(label) - 10;
                }

                Canvas.SetLeft(label, dbllabelLeft);
                Canvas.SetTop(label, dbllabelTop);

                //We will add line to canvas if line is inside chart area
                if ((this.isInsideLimitArea(new Point(line.X1, line.Y1)) == true) && (this.isInsideLimitArea(new Point(line.X2, line.Y2)) == true))
                {
                    canvasRet.Children.Add(line);
                    canvasRet.Children.Add(label);
                }
            }
            //**************************************************
            return canvasRet;
        }

        //
        public void DrawUserLine()
        {
            this.AddChild(this.UserLineClassic());
        }
        //Constructor
        public classUserLineLayer()
        {
            //
            this.MyAdorner = new ControlAdorner(this)
            {
                Child = this,
            };
            //
            this.lstclsUserLineData = new List<classUserLineData>();
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

            return (formattedText.Height);
            //return formattedText.Height;
        }
    }

    //******************************************************************************************************
    /// <summary>
    /// Contains all setting for Chart
    /// </summary>
    public class SettingChart
    {
        public string strChartTitle { get; set; } //The title of chart
        public string strXUnitName { get; set; } //Unit name of X axis
        public string strYUnitName { get; set; } //Unit name of Y axis

        //
        public double dblChartWindowWidth { get; set; } //The width of all window chart
        public double dblChartWindowHeight { get; set; } //The height of all window chart

        //How many divide line appear on chart window (Default is 10)
        public int intNumXdivide { get; set; } //The number of divide distance on X axis 
        public int intNumYdivide { get; set; } //The number of divide distance on Y axis

        public int intChartXDirection { get; set; } //0 - Default (left to right). 1 - Reverse: Right to left
        public int intChartYDirection { get; set; } //0 - Default (Bot to Top). 1 - Reverse: Top to Bot

        //Config to Fix range for all chart (disable auto set)
        //With Auto set (blFixRange = false) all X, Y range will be calculated automatically
        public bool blFixRange { get; set; } 
        public double dblXSmallest { get; set; }
        public double dblXBiggest { get; set; }
        public double dblYSmallest { get; set; }
        public double dblYBiggest { get; set; }

        public SettingChart()
        {
            this.strChartTitle = "CFP Genenral Chart";
            this.strXUnitName = "X";
            this.strYUnitName = "Y";
            //
            this.dblChartWindowWidth = 800;
            this.dblChartWindowHeight = 600;
        }
    }


    /// <summary>
    /// Holding some setting of each chart layer such as Threshold color-value, invidual point color setting
    /// These setting will be kept & applied when redering chart
    /// </summary>
    public class SettingChartLayer
    {
        //Threshold Color - Value setting
        public ThresholdColor clsThresHoldColorX { get; set; } //Setting color of point chart depend on value range - for X axis
        public ThresholdColor clsThresHoldColorY { get; set; } //Setting color of point chart depend on value range - for Y axis

        //For Config invidual point color setting
        public List<SinglePointSetting> lstSinglePointSetting { get; set; } //can config for many points

        public SettingChartLayer()
        {
            this.clsThresHoldColorX = new ThresholdColor();
            this.clsThresHoldColorY = new ThresholdColor();
            //
            this.lstSinglePointSetting = new List<SinglePointSetting>();
        }
    }

    public class SinglePointSetting
    {
        public int ID { get; set; } //ID of point, count from 0
        public SolidColorBrush solidColor { get; set; } //setting Color for point
    }


    //******************************************************************************************************
    public class clsDataChartLayer
    {
        //*******************************************************************************
        public string strChartName { get; set; }
        public int intChartStyleID { get; set; }
        //
        public int intChartXDirection { get; set; } //0 - Default (left to right). 1 - Reverse: Right to left
        public int intChartYDirection { get; set; } //0 - Default (Bot to Top). 1 - Reverse: Top to Bot
        public double dblChartOriginXVal { get; set; } //The X value of origin point in chart (normal is 0)
        public double dblChartOriginYVal { get; set; } //The Y value of origin point in chart (normal is 0)

        //Actual data handle
        public double dblXRange { get; set; } //The Range (Max - min) value can be displayed in X axis
        public double dblYRange { get; set; } //The Range (Max - min) value can be displayed in Y axis

        public double dblXscale { get; set; } //  dblXscale = dblChartWidth/dblXRange
        public double dblYscale { get; set; } //  dblYscale = dblChartHeight/dblYRange

        public classDisplayDataChartLayer DisplayDatalayer { get; set; }

        //To hold user chart data
        public List<Point> lstclsDataChart { get; set; }

        //constructor
        public clsDataChartLayer()
        {
            this.DisplayDatalayer = new classDisplayDataChartLayer();
            this.lstclsDataChart = new List<Point>();
            this.strChartName = "strChartName";
        }

    }
    //******************************************************************************************************
    public class clsUserInfoDisplay
    {
        //Title of Info
        public string strTitle { get; set; }
        public Color clrTitle { get; set; }

        //Content of Info
        public string strContent { get; set; }
        public Color clrContent { get; set; }
        public double dblContentFontSize { get; set; }

        //Constructor
        public clsUserInfoDisplay()
        {
            strTitle = "";
            clrTitle = Colors.Black;
            //
            strContent = "";
            clrContent = Colors.Blue;
            dblContentFontSize = 16;
        }
    }
    //******************************************************************************************************

}
