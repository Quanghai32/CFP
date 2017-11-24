using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.MefExtensions;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using System.Diagnostics;

namespace CFP_WPF
{
    /// <summary>
    /// Creating Bootstrapper for CFP program. Using MEF.
    /// </summary>
    public class clsCFPBootstrapper: MefBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return this.Container.GetExportedValue<Shell>();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(clsCFPBootstrapper).Assembly));

            DirectoryCatalog catalog = new DirectoryCatalog("Extensions"); //select folder same with exe file
            this.AggregateCatalog.Catalogs.Add(catalog);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            //
            App.Current.MainWindow = (Shell)this.Shell;
			// Close Splash Screen 
			nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.CloseSplashScreen(true));
			// Show Main Window
			App.Current.MainWindow.Show();

            ////Close Splash Screen 
            //nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.CloseSplashScreen(true));
        }

    }
}
