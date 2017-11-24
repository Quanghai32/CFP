using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using Microsoft.Practices.Prism.Mvvm;
using System.Timers;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;

namespace nspMasterProcessModel
{
    [Export(typeof(clsMasterProcessViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class clsMasterProcessViewModel: BindableBase
    {
        public clsMasterProcessModel objclsMasterModel { get; set; }

        public clsBindingSupport clsBindingView { get; set; }

        public ICommand btnNextCommand { get; set; }
        public ICommand btnEndCommand { get; set; }

        public System.Data.DataTable GetTableView
        {
            get { return this.objclsMasterModel.ViewTable; }
        }

        public List<string> GetCheckModeList
        {
            get { return this.objclsMasterModel.GetMaserRunModeList(); }
        }

        public string _strSelectedItem;
        public string TheSelectedItem
        {
            set 
            { 
                if(value!=null)
                {
                    _strSelectedItem = value;
                    objclsMasterModel.SetMasterRunMode(_strSelectedItem);
                }
            }
        }

        private void btnNextCommandHandle()
        {
            this.objclsMasterModel.blAllowContinueRunning = true;
        }

        //**************************************************************************
        public void UpdateChangeFromModel()
        {
            //Notify change to view
            OnPropertyChanged("clsBindingView");
        }

        //**************************************************************************


        [ImportingConstructor]
        public clsMasterProcessViewModel(clsMasterProcessModel objclsMasterModel)
        {
            //this.objclsMasterModel = new clsMasterProcessModel();
            this.objclsMasterModel = objclsMasterModel;

            //objclsMasterModel.clsMasterExtension.CFPAssemblyCatalog = new AssemblyCatalog(typeof(clsMasterProcessViewModel).Assembly);
            objclsMasterModel.MasterProcessModelIni();
            //objclsMasterModel.MasterProcessMainLoopStart();

            this.btnNextCommand = new DelegateCommand(this.btnNextCommandHandle);

            //Ini for bind view class
            this.clsBindingView = this.objclsMasterModel.clsBindingView;

            //Add notify change event handle
            this.objclsMasterModel.PropertyChanged += (s, e) => { UpdateChangeFromModel(); };
        }

    }



}