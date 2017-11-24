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
using System.ComponentModel.Composition;
using nspCFPInfrastructures;
using System.Data;
using System.Windows.Markup;

namespace nspChildProcessModel
{
    /// <summary>
    /// Interaction logic for ChildResultView.xaml
    /// </summary>
    [Export(typeof(ChildResultView))]
    public partial class ChildResultView : UserControl
    {
        private clsChildControlViewModel objChildControlViewModel { get; set; }

        private string strLastString { get; set; }
        private TextBlock MyTextBlock { get; set; }

        private string strLastStringSystem { get; set; }
        private TextBlock MyTextBlockSystem { get; set; }

        public ChildResultView()
        {
            InitializeComponent();
        }

        public void SetTextFormat()
        {
            string strTextInput = this.MyTextBlock.Text;

            if (this.strLastString == strTextInput) return;

            //Now separate Text input to list of string to analyze
            string[] tmpArr;
            tmpArr = strTextInput.Split(System.Environment.NewLine.ToCharArray());

            //Transfer to list to further handle
            List<string> lststrInput = new List<string>(tmpArr);

            //Reject all empty character
            int i = 0;
            for (i = 0; i < lststrInput.Count; i++)
            {
                if(lststrInput[i].Trim() == "")
                {
                    lststrInput.RemoveAt(i);
                }
            }


            //Set Bold text
            this.ChildHeaderInfoTextBlock.Inlines.Clear();

            int j = 0;
            for (i = 0; i < lststrInput.Count; i++)
            {
                if (i == (lststrInput.Count - 1))
                {
                    int intTest = lststrInput[i].Length;
                    if (intTest == 0) continue;
                }

                tmpArr = lststrInput[i].Split(':');
                List<string> lstTemp = new List<string>(tmpArr);

                string strKeyMarker = "";

                for (j = 0; j < lstTemp.Count; j++)
                {
                    if (j == 0) //First element
                    {
                        strKeyMarker = lstTemp[j].ToUpper();

                        this.ChildHeaderInfoTextBlock.Inlines.Add(lstTemp[j] + ": ");
                    }
                    else if (j == (lstTemp.Count - 1)) //Last element
                    {
                        if (strKeyMarker == "CHECKING MODE")
                        {
                            if (lstTemp[j].ToUpper().Trim() == "NORMALMODE")
                            {
                                this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Blue, Background = System.Windows.Media.Brushes.Transparent});
                            }
                            else
                            {
                                this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Red, Background = System.Windows.Media.Brushes.Yellow });
                            }
                        }
                        else if (strKeyMarker == "RUN MODE")
                        {
                            if (lstTemp[j].ToUpper().Trim() == "SINGLEPROCESS")
                            {
                                this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Red, Background = System.Windows.Media.Brushes.Yellow });
                            }
                            else
                            {
                                this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Blue, Background = System.Windows.Media.Brushes.Transparent });
                            }
                        }
                        else
                        {
                            this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold});
                        }
                    }
                    else //Middle element
                    {
                        this.ChildHeaderInfoTextBlock.Inlines.Add(new Run(lstTemp[j]) { FontWeight = FontWeights.Bold});
                    }
                }

            }

            this.strLastString = strTextInput;
        }

        public void SetTextFormatSystem()
        {
            string strTextInput = this.MyTextBlockSystem.Text;

            if (this.strLastStringSystem == strTextInput) return;

            //Now separate Text input to list of string to analyze
            string[] tmpArr;
            tmpArr = strTextInput.Split(System.Environment.NewLine.ToCharArray());

            //Transfer to list to further handle
            List<string> lststrInput = new List<string>(tmpArr);

            //Reject all empty character
            int i = 0;
            for (i = 0; i < lststrInput.Count; i++)
            {
                if (lststrInput[i].Trim() == "")
                {
                    lststrInput.RemoveAt(i);
                }
            }

            //Set Bold text
            this.ChildHeaderSysTextBlock.Inlines.Clear();

            int j = 0;
            for (i = 0; i < lststrInput.Count; i++)
            {
                if (i == (lststrInput.Count - 1))
                {
                    int intTest = lststrInput[i].Length;
                    if (intTest == 0) continue;
                }

                tmpArr = lststrInput[i].Split(':');
                List<string> lstTemp = new List<string>(tmpArr);

                for (j = 0; j < lstTemp.Count; j++)
                {
                    if (j == 0) //First element
                    {
                        if(j==lstTemp.Count-1)
                        {
                            //this.ChildHeaderSysTextBlock.Inlines.Add(lstTemp[j] + "\r\n");
                            this.ChildHeaderSysTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold });
                        }
                        else
                        {
                            this.ChildHeaderSysTextBlock.Inlines.Add(lstTemp[j] + ": ");
                        }
                    }
                    else if (j == (lstTemp.Count - 1)) //Last element
                    {
                        this.ChildHeaderSysTextBlock.Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold});
                    }
                    else //Middle element
                    {
                        this.ChildHeaderSysTextBlock.Inlines.Add(new Run(lstTemp[j]) { FontWeight = FontWeights.Bold});
                    }
                }

            }

            this.strLastStringSystem = strTextInput;

        }


        [Import(typeof(clsChildControlViewModel))]
        clsChildControlViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                this.objChildControlViewModel = value;

                //Set binding
                this.MyTextBlock = new TextBlock();
                Binding MyBinding = new Binding("clsBindingView.strChildHeaderInfo");
                MyBinding.Source = this.objChildControlViewModel;
                MyBinding.NotifyOnTargetUpdated = true;
                this.MyTextBlock.SetBinding(TextBlock.TextProperty, MyBinding);
                this.MyTextBlock.TargetUpdated +=MyTextBlock_TargetUpdated;

                //Set binding
                this.MyTextBlockSystem = new TextBlock();
                MyBinding = new Binding("clsBindingView.strChildHeaderSysInfo");
                MyBinding.Source = this.objChildControlViewModel;
                MyBinding.NotifyOnTargetUpdated = true;
                this.MyTextBlockSystem.SetBinding(TextBlock.TextProperty, MyBinding);
                this.MyTextBlockSystem.TargetUpdated += SysTextBlock_TargetUpdated;
            }
        }

        private void MyTextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            //throw new NotImplementedException();
            e.Handled = true;
            SetTextFormat();
        }

        private void SysTextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            //throw new NotImplementedException();
            e.Handled = true;
            SetTextFormatSystem();
        }

    }
}
