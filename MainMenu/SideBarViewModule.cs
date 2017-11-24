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

namespace SideBarView
{
    [ModuleExport(typeof(SideBarViewModule))]
    class SideBarViewModule : IModule
    {
        [Import]
        public IRegionManager regionManager;

        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion("SideBarRegion", typeof(SideBarMainView));
        }
    }
}
