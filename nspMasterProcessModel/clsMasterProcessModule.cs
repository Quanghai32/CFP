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

namespace nspMasterProcessModel
{
    [ModuleExport(typeof(clsMasterProcessModule))]
    class clsMasterProcessModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion("MasterViewRegion", typeof(MasterProcessView));
            this.regionManager.RegisterViewWithRegion("TopBarRegion", typeof(MasterControlPanelView)); //TopBarRegion - SideBarRegion
            this.regionManager.RegisterViewWithRegion("FlashingRegion", typeof(Views.FlashingIndication));
        }
    }
}
