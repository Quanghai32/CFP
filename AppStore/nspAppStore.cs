using Redux;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nspAppStore
{
    /// <summary>
    /// Redux Store
    /// </summary>
    public static class clsAppStore
    {
        public static IStore<AppState> AppStore { get; private set; }

        /// <summary>
        /// Create new App Store Instance
        /// </summary>
        public static void IniStore()
        {
            AppStore = new Store<AppState>(reducer: AppReducer.Execute, initialState: new AppState());
        }

        /// <summary>
        /// Allow User easily getting latest App State
        /// </summary>
        /// <returns></returns>
        public static AppState GetCurrentState()
        {
            AppState state = null;
            var test = AppStore.Take(1).Subscribe(s => state = s);
            test.Dispose(); //Unsubscribe to prevent memory leak
            return state;
        }
    }

}
