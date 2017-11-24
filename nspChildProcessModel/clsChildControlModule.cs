using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.MefExtensions.Regions;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;

namespace nspChildProcessModel
{
    [ModuleExport(typeof(clsChildControlModule))]
    class clsChildControlModule: IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion("ProcessViewRegion", typeof(ChildControlView));
            this.regionManager.RegisterViewWithRegion("StepListViewRegion", typeof(Views.StepListView));

            this.regionManager.RegisterViewWithRegion("TopBarRegion", typeof(ChildControlPanelView)); //SideBarRegion - TopBarRegion
            this.regionManager.RegisterViewWithRegion("TopBarRegion", typeof(Views.StepListControlPanelView));
            //this.regionManager.RegisterViewWithRegion("ProcessViewRegion", typeof(ChildControlPanelView));

            this.regionManager.RegisterViewWithRegion("HeaderRegion", typeof(ChildResultView));
            this.regionManager.RegisterViewWithRegion("MainInfoRegion", typeof(ChildMainInfoView));
            this.regionManager.RegisterViewWithRegion("MenuRegion", typeof(MainMenuView));
            this.regionManager.RegisterViewWithRegion("OptionViewRegion", typeof(Views.OptionView));
            //this.regionManager.RegisterViewWithRegion("SideBarRegion", typeof(Views.SystemWatcherView));

            //this.regionManager.RegisterViewWithRegion("UserViewRegion", typeof(Views.WebBrowser));

        }
    }
}
