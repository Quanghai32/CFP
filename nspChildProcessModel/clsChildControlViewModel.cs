using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Data;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows;

namespace nspChildProcessModel
{
    [Export(typeof(clsChildControlViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class clsChildControlViewModel: BindableBase
    {
        public clsChildControlModel objclsChildControlModel { get; set; }

        public clsBindingSupport clsBindingView { get; set; }


        //******************ICOMMAND HANDLE*********************************************************************************

        public ICommand btnNextCommand { get; set; }
        //Handle Next button pressed
        public void btnNextCommandHandle()
        {
            this.objclsChildControlModel.btnNextCommandHandle();
        }


        public ICommand ICommandResetCounter { get; set; }
        public void ICommandResetCounterHandle()
        {
            this.objclsChildControlModel.ResetCounter();
        }

       
        public ICommand ICommandSkipCheckItem { get; set; }
        public void ICommandSkipCheckItemHandle()
        {
            this.objclsChildControlModel.SkipCheckItem();
        }

        public ICommand ICommandMasterSequenceTester { get; set; }
        public void ICommandMasterSequenceTesterHandle()
        {
            this.objclsChildControlModel.MasterSequenceTester();
        }

        public ICommand ICommandChildSequenceTester { get; set; }
        public void ICommandChildSequenceTesterHandle()
        {
            this.objclsChildControlModel.ChildProcessSequenceTester();
        }


        public ICommand ICommandSystemCommandMode { get; set; }
        public void ICommandSystemCommandModeHandle()
        {
            this.objclsChildControlModel.SystemCommandMode();
        }


        public ICommand ICommandClearAllSkip { get; set; }
        public void ICommandClearAllSkipHandle()
        {
            this.objclsChildControlModel.ClearAllSkip();
        }


        public ICommand ICommandSelectProgramList { get; set; }
        public void ICommandSelectProgramListHandle()
        {
            this.objclsChildControlModel.SelectProgramList();
        }

        //
        public ICommand ICommandSelectStepList { get; set; }
        public void ICommandSelectStepListHandle()
        {
            this.objclsChildControlModel.SelectStepList();
        }

        //
        public ICommand ICommandSelectMasterProgramList { get; set; }
        public void ICommandSelectMasterProgramListHandle()
        {
            this.objclsChildControlModel.SelectMasterProgramList();
        }

        //
        public ICommand ICommandExportOptionViewTable { get; set; }
        public void ICommandExportOptionViewTableHandle()
        {
            this.objclsChildControlModel.ExportOptionViewTable();
        }

        public RelayCommand _RelayCommandSelectMode { get; set; }
        public RelayCommand RelayCommandSelectMode
        {
            get
            {
                if (_RelayCommandSelectMode == null)
                {
                    _RelayCommandSelectMode = new RelayCommand((parameter) => RelayCommandSelectModeHandle(parameter));
                }
                return _RelayCommandSelectMode;
            }
        }

        public void RelayCommandSelectModeHandle(object parameter)
        {
            string strTemp = (string)parameter;
            this.objclsChildControlModel.SetUserSelectCheckingMode(strTemp);
            //Notify Change
            OnPropertyChanged("clsBindingView");
        }

        

        //******************ENDING ICOMMAND HANDLE*********************************************************************************

        public ObservableCollection<MenuItem> obsMenuUserUtilities
        {
            get 
            {
                return this.objclsChildControlModel.obsMenuUserUtilities;
            }
            set
            {
                this.objclsChildControlModel.obsMenuUserUtilities = value;
            }
        }

        public List<string> lststrSelectCheckingMode
        {
            get
            {
                return this.objclsChildControlModel.clsMainVar.lststrSelectCheckingMode;
            }
            set
            {
                this.objclsChildControlModel.clsMainVar.lststrSelectCheckingMode = value;
            }
        }

        //All child process need should common a data table
        public DataTable GetTableView
        {
            get 
            { 
                return this.objclsChildControlModel.GetTableView();
            }
        }

        //All child process need should common a data table
        public DataTable GetStepListTableView
        {
            get
            {
                return this.objclsChildControlModel.GetStepListTableView();
            }
        }

        //**************RUNNING MODE HANDLE*******************************
        //For display running mode
        public List<string> GetRunModeList
        {
            get { return this.objclsChildControlModel.GetRunModeList(); }
        }

        //Handle user select running mode
        public string SelectRunMode
        {
            set
            {
                string strTemp = (string)value;

                //Request Password
                string strUserInput = "";
                System.Windows.Forms.DialogResult dlgTemp;

                bool blAllowChange = false;

                while (true)
                {
                    dlgTemp = MyLibrary.clsMyFunc.InputBox("PASSWORD REQUEST", "Becareful when you change running mode! Please input password first:", ref strUserInput);
                    if (dlgTemp == System.Windows.Forms.DialogResult.OK)
                    {
                        //Confirm password
                        if (strUserInput.ToUpper().Trim() == "PED")
                        {
                            blAllowChange = true;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Password Wrong! Please input again or cancel changing running mode!","PASSWORD WRONG");
                        }
                    }
                    else if (dlgTemp == System.Windows.Forms.DialogResult.Cancel)
                    {
                        MessageBox.Show("You chose cancel. Running mode will not be changed!", "PASSWORD WRONG");
                        break;
                    }
                }

                if (blAllowChange == true)
                {
                    //If Password is OK, then we allow changing run mode
                    this.objclsChildControlModel.SetChildRunMode(strTemp);

                    //Additional action
                    switch (this.objclsChildControlModel.eChildRunningMode)
                    {
                        case enumSystemRunningMode.ParallelMode:
                            break;
                        case enumSystemRunningMode.SingleThreadMode:
                            //Set View table for single thread process
                            int i = 0;
                            for (i = 0; i < this.objclsChildControlModel.lstChildProcessModel.Count; i++)
                            {
                                this.objclsChildControlModel.lstChildProcessModel[i].blAllowUpdateViewTable = false;
                            }
                            this.objclsChildControlModel.clsSingleThreadModel.blAllowUpdateViewTable = true;
                            break;
                        case enumSystemRunningMode.SingleProcessMode:
                            break;
                        case enumSystemRunningMode.IndependentMode:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //**************END RUNNING MODE HANDLE****************************

        //**************CHECKKING MODE HANDLE*******************************
        //For display checking mode list
        public List<string> GetCheckModeList
        {
            get { return this.objclsChildControlModel.GetCheckModeList(); }
        }

        //Handle user select checking mode
        public string SelectCheckMode
        {
            set
            {
                if(value != null)
                {
                    string strTemp = (string)value;
                    this.objclsChildControlModel.SetChildCheckMode(strTemp);
                }
            }
        }

        //**************END CHECKKING MODE HANDLE****************************

        //For display child process item list
        public List<string> GetChildItemList
        {
            get { return this.objclsChildControlModel.GetChildItemList(); }
        }

        //Handle user select child process
        public string SelectViewProcess
        {
            set
            {
                string strTemp = (string)value;

                //Check if select SingleThread view?
                if(strTemp=="SingleThread")
                {
                    this.objclsChildControlModel.SetSingleThreadView();
                }
                else //Select child process
                {
                    //Disable singlethread view
                    this.objclsChildControlModel.clsSingleThreadModel.blAllowUpdateViewTable = false;
                    //Convert to integer
                    int intProcessID = 0;
                    if (int.TryParse(strTemp, out intProcessID) == true)
                    {
                        //Marking selected Process ID
                        this.objclsChildControlModel.intProcessIDSelect = intProcessID - 1; //count from 1 but ID count from 0
                        //Set View table for selected process
                        this.objclsChildControlModel.SetChildProcessView(this.objclsChildControlModel.intProcessIDSelect);
                    }
                }
            }
        }

        //For Option View Select
        public string ComboSelectOptionView
        {
            set
            {
                string strTemp = (string) value;
                //Call Select Option View Handle
                this.objclsChildControlModel.ComboSelectOptionView(strTemp);
            }
        }

        //**************************************************************************
        public DataTable GetOptionViewDataTable
        {
            get
            {
                return this.objclsChildControlModel.lstChildProcessModel[0].clsProgramList.MyDataTable;
            }
        }

        //**************************************************************************
        //For display child process item list
        public List<string> GetItemCheckList
        {
            get { return this.objclsChildControlModel.GetItemCheckList(); }
        }

        public string SelectStepListViewItem
        {
            set
            {
                string strTemp = (string)value;
                int intItemID = 0;
                if(int.TryParse(strTemp,out intItemID)==true)
                {
                    intItemID--;
                    this.objclsChildControlModel.SelectStepListViewItem(intItemID);
                }
            }
        }

        //**************************************************************************
        public void UpdateChangeFromModel(object s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Notify change to view
            if (e.PropertyName == "clsBindingView")
            {
                OnPropertyChanged("clsBindingView");
            }
            else if (e.PropertyName == "GetSystemWatcherInfo")
            {
                OnPropertyChanged("strSideBarInfo");
            }
            else if (e.PropertyName == "GetTableView")
            {
                OnPropertyChanged("GetTableView");
            }
        }

        //**************************************************************************
        //For building Main Info table
        public int GetRowNum()
        {
            return this.objclsChildControlModel.clsMainVar.intNumRow;
        }

        public int GetColNum()
        {
            return this.objclsChildControlModel.clsMainVar.intNumCol;
        }

        public System.Windows.Media.SolidColorBrush MyColor { get; set; }

        //**************************
        private string _strSideBarInfo;
        public string strSideBarInfo
        {
            get
            {
                this._strSideBarInfo = this.objclsChildControlModel.GetSystemWatcherInfo;
                return this._strSideBarInfo;
            }
            set
            {
                this._strSideBarInfo = value;
            }
        }

        //**************************************************************************
        [ImportingConstructor]
        public clsChildControlViewModel(clsChildControlModel objclsChildControlModel)
        {
            //ini for child control model
            //this.objclsChildControlModel = new clsChildControlModel();
            this.objclsChildControlModel = objclsChildControlModel;

            //Add notify change event handle
            this.objclsChildControlModel.PropertyChanged += (s, e) => { UpdateChangeFromModel(s,e); };

            //Register for ICommand
            this.btnNextCommand = new DelegateCommand(this.btnNextCommandHandle);

            this.ICommandResetCounter = new DelegateCommand(this.ICommandResetCounterHandle);
            this.ICommandSkipCheckItem = new DelegateCommand(this.ICommandSkipCheckItemHandle);
            this.ICommandMasterSequenceTester = new DelegateCommand(this.ICommandMasterSequenceTesterHandle);
            this.ICommandChildSequenceTester = new DelegateCommand(this.ICommandChildSequenceTesterHandle);
            this.ICommandSystemCommandMode = new DelegateCommand(this.ICommandSystemCommandModeHandle);
            this.ICommandClearAllSkip = new DelegateCommand(this.ICommandClearAllSkipHandle);
            this.ICommandSelectStepList = new DelegateCommand(this.ICommandSelectStepListHandle);
            this.ICommandSelectProgramList = new DelegateCommand(this.ICommandSelectProgramListHandle);
            this.ICommandSelectMasterProgramList = new DelegateCommand(this.ICommandSelectMasterProgramListHandle);
            this.ICommandExportOptionViewTable = new DelegateCommand(this.ICommandExportOptionViewTableHandle);

            //Ini for bind view class
            this.clsBindingView = this.objclsChildControlModel.clsBindingView;
        }
        //**************************************************************************
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _handler;
        private bool _isEnabled;

        public RelayCommand(Action<object> handler)
        {
            _handler = handler;
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            //return IsEnabled;
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _handler(parameter);
        }
    }
}
