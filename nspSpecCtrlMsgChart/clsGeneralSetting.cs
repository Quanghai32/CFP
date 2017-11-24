using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace nspSpecMsgChart
{
    public class clsGeneralSetting
    {
        public System.Windows.Controls.MenuItem menuItemUserSetting { get; set; } //This menu item should add to main menu
        public Views.wdGeneralSetting wdSetting { get; set; }
        public string strSettingName { get; set; } //Name of user setting - should be display on main menu
        public string strSettingFilePath { get; set; } //Full Path of Setting File
        public string strSettingSection { get; set; } //Section of Setting File

        public List<clsUserSettingData> lstclsUserSetting { get; set; }

        public void IniGeneralInput()
        {
            //Create new reading form in UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.wdSetting = new Views.wdGeneralSetting();
                this.wdSetting.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.wdSetting.btnApplySetting.Click += btnApplySetting_Click;
                //Create event handle for combo box
                //
                if(this.strSettingName != "")
                {
                    this.menuItemUserSetting.Header = this.strSettingName;
                    this.wdSetting.Title = this.strSettingName;
                }

            }));

            //Checking File Setting exist or not
            bool blFound = false;

            //Try to looking with assuming that input data is file name & that file same location with exe file
            string strTemp = System.AppDomain.CurrentDomain.BaseDirectory + this.strSettingFilePath;
            if (MyLibrary.ChkExist.CheckFileExist(strTemp) == true)
            {
                this.strSettingFilePath = strTemp;
                blFound = true;
            }
            else
            {
                if (MyLibrary.ChkExist.CheckFileExist(this.strSettingFilePath) == true)
                {
                    blFound = true;
                }
            }


            if (blFound == false) return; //Not found setting file

            //if file found, then try to load setting
            int i = 0;
            for(i=0;i<this.lstclsUserSetting.Count;i++)
            {
                //confirm if user setting is combo box or not (cb-Name) format
                if (this.lstclsUserSetting[i].strUserSettingName.IndexOf("cb-")==0) //combo box
                {
                    this.lstclsUserSetting[i].intClassSetting = 1; //Combo box
                    //separate user key name
                    this.lstclsUserSetting[i].strUserSettingName = this.lstclsUserSetting[i].strUserSettingName.Substring(3, this.lstclsUserSetting[i].strUserSettingName.Length - 3);
                }

                //Create event handle for all combo box
                this.lstclsUserSetting[i].cbUserSetting.SelectionChanged += cbUserSetting_SelectionChanged;

                
                //
                strTemp = MyLibrary.ReadFiles.IniReadValue(this.strSettingSection, this.lstclsUserSetting[i].strUserSettingName, this.strSettingFilePath);
                if(strTemp!= "error")
                {
                    this.lstclsUserSetting[i].strValueSetting = strTemp;
                    this.lstclsUserSetting[i].tbUserSetting.Text = this.lstclsUserSetting[i].strValueSetting;
                    this.lstclsUserSetting[i].cbUserSetting.Text = this.lstclsUserSetting[i].strValueSetting;

                    this.lstclsUserSetting[i].grbUserSetting.Header = this.lstclsUserSetting[i].strUserSettingName;
                    if (this.lstclsUserSetting[i].intClassSetting == 1) //Combo box
                    {
                        this.lstclsUserSetting[i].grbUserSetting.Content = this.lstclsUserSetting[i].cbUserSetting;
                    }
                    else //text box default
                    {
                        this.lstclsUserSetting[i].grbUserSetting.Content = this.lstclsUserSetting[i].tbUserSetting;
                    }

                    //
                    this.wdSetting.spnlContentSetting.Children.Add(this.lstclsUserSetting[i].grbUserSetting);
                }
            }

        }

        void cbUserSetting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //Find sender
            bool blFound = false;
            int i = 0;
            int intIndex = 0;
            for(i=0;i<this.lstclsUserSetting.Count;i++)
            {
                if(sender == this.lstclsUserSetting[i].cbUserSetting)
                {
                    blFound = true;
                    intIndex = i;
                    break;
                }
            }
            if (blFound == false) return;

            //Update value setting
            this.lstclsUserSetting[intIndex].strValueSetting = this.lstclsUserSetting[intIndex].cbUserSetting.SelectedItem.ToString();
        }

        //
        void btnApplySetting_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Hello!");
            //Writing all current user setting to setting file
            int i = 0;
            for(i=0;i<this.lstclsUserSetting.Count;i++)
            {
                //Update new data from window setting
                if(this.lstclsUserSetting[i].intClassSetting==1) //Combo box
                {
                    this.lstclsUserSetting[i].strValueSetting = this.lstclsUserSetting[i].cbUserSetting.Text;
                }
                else
                {
                    this.lstclsUserSetting[i].strValueSetting = this.lstclsUserSetting[i].tbUserSetting.Text;
                }

                //Writting to setting file
                long lngRet = 0;
                lngRet = MyLibrary.WriteFiles.IniWriteValue(this.strSettingFilePath, this.strSettingSection, this.lstclsUserSetting[i].strUserSettingName, this.lstclsUserSetting[i].strValueSetting);
            }
            //
            MessageBox.Show("Apply Setting Finish!", "btnApplySetting_Click");
        }

        //Show window
        public void Show()
        {
            if (this.wdSetting == null) return;

            if (this.wdSetting.isClosed == false) //Not yet closed
            {
                this.wdSetting.Dispatcher.Invoke(new Action(() =>
                {
                    //this.wdReadingForm.Show();
                    this.wdSetting.Show();
                }));
            }
            else //Already closed - Create new window & show again
            {
                //
                ReloadWindow();
                //
                this.wdSetting.Dispatcher.Invoke(new Action(() =>
                {
                    //this.wdReadingForm.Show();
                    this.wdSetting.Show();
                }));
            }
        }

        public void ReloadWindow()
        {
            //Create new reading form in UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.wdSetting = new Views.wdGeneralSetting();
                this.wdSetting.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.wdSetting.btnApplySetting.Click += btnApplySetting_Click;
                //
                if (this.strSettingName != "")
                {
                    this.menuItemUserSetting.Header = this.strSettingName;
                    this.wdSetting.Title = this.strSettingName;
                }

            }));

            //Add again data of group box, text box, combo box
            int i = 0;
            for (i = 0; i < this.lstclsUserSetting.Count; i++)
            {
                this.lstclsUserSetting[i].grbUserSetting = new System.Windows.Controls.GroupBox();
                this.lstclsUserSetting[i].tbUserSetting = new System.Windows.Controls.TextBox();
                this.lstclsUserSetting[i].cbUserSetting = new System.Windows.Controls.ComboBox();
                this.lstclsUserSetting[i].grbUserSetting.Header = this.lstclsUserSetting[i].strUserSettingName;

                this.lstclsUserSetting[i].grbUserSetting.Width = 360;
                this.lstclsUserSetting[i].grbUserSetting.FontWeight = FontWeights.Bold;
                this.lstclsUserSetting[i].grbUserSetting.BorderBrush = System.Windows.Media.Brushes.Black;
                this.lstclsUserSetting[i].grbUserSetting.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsUserSetting[i].grbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsUserSetting[i].grbUserSetting.Margin = new Thickness(10, 10, 0, 0);

                this.lstclsUserSetting[i].tbUserSetting.Width = 330;
                this.lstclsUserSetting[i].tbUserSetting.Background = System.Windows.Media.Brushes.LemonChiffon;
                this.lstclsUserSetting[i].tbUserSetting.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsUserSetting[i].tbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsUserSetting[i].tbUserSetting.Margin = new Thickness(5, 5, 5, 5);
                this.lstclsUserSetting[i].tbUserSetting.Text = this.lstclsUserSetting[i].strValueSetting;

                this.lstclsUserSetting[i].cbUserSetting.Width = 330;
                this.lstclsUserSetting[i].cbUserSetting.Background = System.Windows.Media.Brushes.LemonChiffon;
                this.lstclsUserSetting[i].cbUserSetting.VerticalAlignment = VerticalAlignment.Top;
                this.lstclsUserSetting[i].cbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
                this.lstclsUserSetting[i].cbUserSetting.Margin = new Thickness(5, 5, 5, 5);
                this.lstclsUserSetting[i].cbUserSetting.IsEditable = true;
                this.lstclsUserSetting[i].cbUserSetting.Text = this.lstclsUserSetting[i].strValueSetting;
                //Create event handle for all combo box
                this.lstclsUserSetting[i].cbUserSetting.SelectionChanged += cbUserSetting_SelectionChanged;

                if (this.lstclsUserSetting[i].intClassSetting == 1) //Combo box
                {
                    this.lstclsUserSetting[i].grbUserSetting.Content = this.lstclsUserSetting[i].cbUserSetting;
                    //Add combo box list
                    int j = 0;
                    for(j=0;j<this.lstclsUserSetting[i].lstobjComboItem.Count;j++)
                    {
                        this.lstclsUserSetting[i].cbUserSetting.Items.Add(this.lstclsUserSetting[i].lstobjComboItem[j]);
                    }
                }
                else //text box default
                {
                    this.lstclsUserSetting[i].grbUserSetting.Content = this.lstclsUserSetting[i].tbUserSetting;
                }

                //
                this.wdSetting.spnlContentSetting.Children.Add(this.lstclsUserSetting[i].grbUserSetting);
            }
        }

        //
        public void AddList(object objUserSettingName, object objInput)
        {
            //Add list of item to combo list
            IList lstobjInput = new List<object>();
            string strUserSettingName = objUserSettingName.ToString().Trim();

            if(this.isGenericList(objInput) == true)
            {
                lstobjInput = (IList)objInput;
            }
            else
            {
                lstobjInput.Add(objInput);
            }

            //Looking for Combo box target
            bool blFound = false;
            int i = 0;
            int intIndex = 0;
            for(i=0;i<this.lstclsUserSetting.Count;i++)
            {
                if (this.lstclsUserSetting[i].intClassSetting != 1) continue; //Not combo box
                //
                if(this.lstclsUserSetting[i].strUserSettingName.ToUpper().Trim() == strUserSettingName.ToUpper().Trim()) //Matching
                {
                    blFound = true;
                    intIndex = i;
                    break;
                }
            }

            if (blFound == false) return;

            //Add to Combo List
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                for(i=0;i<lstobjInput.Count;i++)
                {
                    this.lstclsUserSetting[intIndex].lstobjComboItem.Add(lstobjInput[i]);
                    this.lstclsUserSetting[intIndex].cbUserSetting.Items.Add(lstobjInput[i]);
                }
            }));

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

        //
        public string GetSetting(object objUserSettingName)
        {
            string strRet = "";
            //
            int i = 0;
            for (i = 0; i < this.lstclsUserSetting.Count;i++)
            {
                if(objUserSettingName.ToString().ToUpper().Trim() == this.lstclsUserSetting[i].strUserSettingName.ToUpper().Trim())
                {
                    strRet = this.lstclsUserSetting[i].strValueSetting;
                    break;
                }
            }

            //
            return strRet;
        }

        //For Menu Item
        public System.Windows.Controls.MenuItem GetMenuItem()
        {
            return this.menuItemUserSetting;
        }

        void menuItemUserSetting_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        //Constructor
        public clsGeneralSetting()
        {
            this.strSettingName = "";

            this.menuItemUserSetting = new System.Windows.Controls.MenuItem();
            this.menuItemUserSetting.Header = "User Menu Item";
            this.menuItemUserSetting.Click += menuItemUserSetting_Click;

            this.lstclsUserSetting = new List<clsUserSettingData>();
        }  
    
        //Testing for General Calibration value
        private int intTestCal = 0;
        public object TestCal(object objPara)
        {
            intTestCal++;
            if (intTestCal > 10) intTestCal = 0;
            //return objPara.ToString();
            return intTestCal;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    public class clsUserSettingData
    {
        public string strUserSettingName { get; set; } //Key name of user setting
        public string strValueSetting { get; set; } //Value of text box or combo box
        public int intClassSetting { get; set; } //0 - Textbox (Default). 1 - ComboBox.
        public List<object> lstobjComboItem { get; set; } //If user setting is combo box - this is list of child item will be add to combo box

        public System.Windows.Controls.GroupBox grbUserSetting { get; set; }
        public System.Windows.Controls.TextBox tbUserSetting { get; set; }
        public System.Windows.Controls.ComboBox cbUserSetting { get; set; }

        //Constructor
        public clsUserSettingData()
        {
            this.strUserSettingName = "";
            this.strValueSetting = "";
            this.intClassSetting = 0;
            this.lstobjComboItem = new List<object>();

            this.grbUserSetting = new System.Windows.Controls.GroupBox();
            this.tbUserSetting = new System.Windows.Controls.TextBox();
            this.cbUserSetting = new System.Windows.Controls.ComboBox();

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

            this.cbUserSetting.Width = 330;
            this.cbUserSetting.Background = System.Windows.Media.Brushes.LemonChiffon;
            this.cbUserSetting.VerticalAlignment = VerticalAlignment.Top;
            this.cbUserSetting.HorizontalAlignment = HorizontalAlignment.Left;
            this.cbUserSetting.Margin = new Thickness(5, 5, 5, 5);
            this.cbUserSetting.IsEditable = true;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
}
