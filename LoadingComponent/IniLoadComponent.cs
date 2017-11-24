using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingComponent
{
    /// <summary>
    /// If some component need to ini loading right after program start. We do it here!
    /// </summary>
    public class IniLoadComponent
    {
        public void IniComponent()
        {
            // Base on that Info, initialize components if required
            // 1.Ini for Host Website service
            var state = nspAppStore.clsAppStore.GetCurrentState();
            if (state.GlobalSetting.UsingHostWebsite)
            {
                var test = new HostWebsiteService.HostWebService();
            }
        }
    }
}
