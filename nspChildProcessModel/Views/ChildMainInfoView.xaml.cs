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

namespace nspChildProcessModel
{
    /// <summary>
    /// Interaction logic for ChildMainInfoView.xaml
    /// </summary>
    [Export(typeof(ChildMainInfoView))]
    public partial class ChildMainInfoView : UserControl
    {
        private clsChildControlViewModel objChildControlViewModel;
        private List<TextBlock> lsttextblockItemDetailInfo { get; set; }
        private List<TextBlock> lsttextblockItemDetailInfoView { get; set; }

        public ChildMainInfoView()
        {
            InitializeComponent();
        }

        public void IniTableInfo(int intNumRow, int intNumCol)
        {
            //1. Create table
            int i, j = 0;
            //Create collumns for table
            for(i=0;i<intNumCol;i++)
            {
                this.TableInfo.Columns.Add(new TableColumn());
            }

            //Create RowGroups for table
            this.TableInfo.RowGroups.Add(new TableRowGroup());

            //Create rows for table
            TableRow currentRow = new TableRow();
            for(i=0;i<intNumRow;i++)
            {
                this.TableInfo.RowGroups[0].Rows.Add(new TableRow());

                currentRow = this.TableInfo.RowGroups[0].Rows[i];
                //currentRow.Background = Brushes.LightGreen;
                currentRow.FontSize = 20;
                currentRow.FontWeight = System.Windows.FontWeights.Bold;

                // Add the header row with content
                for (j = 0; j < intNumCol; j++)
                {
                    Run rTemp = new Run();
                    rTemp.Foreground = System.Windows.Media.Brushes.Black;

                    Binding MyBinding = new Binding("clsBindingView.lststrItemResultView[" + (i * intNumCol + j).ToString() + "]");
                    MyBinding.Source = objChildControlViewModel;
                    rTemp.SetBinding(Run.TextProperty, MyBinding);

                    Paragraph pTemp = new Paragraph(rTemp);
                    MyBinding = new Binding("clsBindingView.lstclrItemResultBackGroundView[" + (i * intNumCol + j).ToString() + "]");
                    MyBinding.Source = objChildControlViewModel;
                    pTemp.SetBinding(Paragraph.BackgroundProperty, MyBinding);

                    TableCell tbcellTemp = new TableCell(pTemp);
                    currentRow.Cells.Add(tbcellTemp);

                    // and set the row to span all 6 columns.
                    currentRow.Cells[j].BorderBrush = Brushes.Green;
                    currentRow.Cells[j].BorderThickness = new Thickness(1);
                    currentRow.Cells[j].Background = Brushes.Lavender;
                }
            }
        }

        public void IniTableDatail(int intNumCol, int intNumItem)
        {
            int i,j = 0;
            TableRow currentRow = new TableRow();

            for(i=0;i<intNumItem;i++)
            {
                this.TableDetail.RowGroups[0].Rows.Add(new TableRow());

                currentRow = this.TableDetail.RowGroups[0].Rows[i+1];
                //currentRow.Background = Brushes.Silver;
                currentRow.Background = Brushes.Transparent;
                //currentRow.FontSize = 18;
                currentRow.FontWeight = System.Windows.FontWeights.Normal;

                //Add the header row with content
                for (j = 0; j < intNumCol; j++)
                {
                    //Adjust collumn span setting for collumns
                    if(j==0) //item number
                    {
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run((i + 1).ToString()))));
                        currentRow.Cells[j].ColumnSpan = 1;
                        currentRow.Cells[j].FontWeight = FontWeights.Bold;
                        currentRow.Cells[j].FontSize = 28;
                    }
                    else if(j==1) //info
                    {
                        //Set binding
                        Run rTemp = new Run();
                        rTemp.Foreground = System.Windows.Media.Brushes.Black;

                        Binding MyBinding = new Binding("clsBindingView.lststrItemInfo[" + (i).ToString() + "]");
                        MyBinding.Source = objChildControlViewModel;
                        rTemp.SetBinding(Run.TextProperty, MyBinding);

                        this.lsttextblockItemDetailInfoView[i] = new TextBlock();
                        this.lsttextblockItemDetailInfoView[i].Text = "Hehe";

                        //Paragraph pTemp = new Paragraph(rTemp);
                        Paragraph pTemp = new Paragraph();
                        pTemp.Inlines.Add(this.lsttextblockItemDetailInfoView[i]);

                        TableCell tbcellTemp = new TableCell(pTemp);
                        currentRow.Cells.Add(tbcellTemp);

                        currentRow.Cells[j].ColumnSpan = 6;
                    }
                    else if(j==2) //Notes
                    {
                        //Set binding
                        Run rTemp = new Run();
                        rTemp.Foreground = System.Windows.Media.Brushes.Black;

                        Binding MyBinding = new Binding("clsBindingView.lststrItemNotes[" + (i).ToString() + "]");
                        MyBinding.Source = objChildControlViewModel;
                        rTemp.SetBinding(Run.TextProperty, MyBinding);

                        Paragraph pTemp = new Paragraph(rTemp);
   

                        TableCell tbcellTemp = new TableCell(pTemp);
                        currentRow.Cells.Add(tbcellTemp);

                        currentRow.Cells[j].ColumnSpan = 4;
                    }

                    // and set the row to span all columns.
                    currentRow.Cells[j].BorderBrush = Brushes.Green;
                    currentRow.Cells[j].BorderThickness = new Thickness(1);

                    if (i % 2 == 0)
                    {
                        //currentRow.Cells[j].Background = Brushes.LightYellow;
                        currentRow.Cells[j].Background = Brushes.Transparent;
                    }
                    else
                    {
                        //currentRow.Cells[j].Background = Brushes.LightGreen;
                        currentRow.Cells[j].Background = Brushes.Transparent;
                    }

                }
            }
        }

        [Import(typeof(clsChildControlViewModel))]
        clsChildControlViewModel ViewModel
        {
            set
            {
                this.objChildControlViewModel = value;
                this.DataContext = value;

                //Ini for list of text block
                this.lsttextblockItemDetailInfo = new List<TextBlock>();
                this.lsttextblockItemDetailInfoView = new List<TextBlock>();
                int intNumChildPro = this.objChildControlViewModel.objclsChildControlModel.clsMainVar.intNumItem;
                int i = 0;
                for(i=0;i<intNumChildPro;i++)
                {
                    TextBlock temp = new TextBlock();
                    //Set binding
                    Binding MyBinding = new Binding("clsBindingView.lststrItemInfo[" + (i).ToString() + "]");
                    MyBinding.Source = this.objChildControlViewModel;
                    MyBinding.NotifyOnTargetUpdated = true;
                    temp.SetBinding(TextBlock.TextProperty, MyBinding);
                    temp.TargetUpdated += MyTextBlock_TargetUpdated;

                    this.lsttextblockItemDetailInfo.Add(temp);

                    temp = new TextBlock();
                    this.lsttextblockItemDetailInfoView.Add(temp);
                }

                //Ini for table Info
                IniTableInfo(this.objChildControlViewModel.GetRowNum(), this.objChildControlViewModel.GetColNum());

                //Ini for table detail
                IniTableDatail(3, this.objChildControlViewModel.objclsChildControlModel.clsMainVar.intNumItem);
            }
        }

        private void MyTextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            //throw new NotImplementedException();
            e.Handled = true;

            //Looking for sender
            int intSenderID = 0;
            bool blFound = false;
            for (int i = 0; i < this.objChildControlViewModel.objclsChildControlModel.clsMainVar.intNumItem;i++ )
            {
                if(this.lsttextblockItemDetailInfo[i] == sender)
                {
                    intSenderID = i;
                    blFound = true;
                }
            }

            if (blFound == true)
            {
                SetTextFormat(intSenderID, this.lsttextblockItemDetailInfo[intSenderID].Text);
            }
                
        }

        private void SetTextFormat(int intProcessID, string strUpdateInfo)
        {
            //this.lsttextblockItemDetailInfoView[intProcessID].Text = strUpdateInfo;

            string strTextInput = strUpdateInfo;

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

            //Clear old content first
            this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Clear();

            //Update new Info
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

                bool blCheckingResult = false;

                for (j = 0; j < lstTemp.Count; j++)
                {
                    if (j == 0) //First element
                    {
                        if (lstTemp[j].ToUpper().Trim() == "RESULT")
                        {
                            blCheckingResult = true;
                        }
                        this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Add(new Run(lstTemp[j] + ": ") {Foreground= System.Windows.Media.Brushes.Black});
                    }
                    else if (j == (lstTemp.Count - 1)) //Last element
                    {
                        if (blCheckingResult == true)
                        {
                            if (lstTemp[j].ToUpper().Trim() == "PASS")
                            {
                                this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Blue });
                            }
                            else
                            {
                                this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold,  Foreground = System.Windows.Media.Brushes.Red });
                            }
                        }
                        else
                        {
                            this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Add(new Run(lstTemp[j] + "\r\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Black });
                        }
                    }
                    else //Middle element
                    {
                        this.lsttextblockItemDetailInfoView[intProcessID].Inlines.Add(new Run(lstTemp[j]) { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Black });
                    }
                }

            }

        }

    }
}
