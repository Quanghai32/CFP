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
using System.Windows.Shapes;

namespace nspChildProcessModel.Views
{
    /// <summary>
    /// Interaction logic for wdSystemCommand.xaml
    /// </summary>
    public partial class wdSystemCommand : Window
    {
        private clsChildControlModel objclsChildControlModel { get; set; }

        //Constructor
        public wdSystemCommand(clsChildControlModel objclsChildControlModel)
        {
            //
            InitializeComponent();
            //
            this.objclsChildControlModel = objclsChildControlModel;
            //
            this.tbSearch.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchHandle();
        }

        private void SearchHandle()
        {
            //Getting Request search from user & process analyze and searching
            string strUserInput = this.tbSearch.Text;

            //Process Analyzing
            string strAnswer = this.objclsChildControlModel.SystemCommandAnswer(strUserInput);

            //Display Answer
            this.tbAnswer.Text = "Result for \"" + strUserInput + "\":\r\n" + strAnswer;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SearchHandle();
            }
        }
    }
}
