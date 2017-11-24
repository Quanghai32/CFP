using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostWebsiteService
{
    /// <summary>
    /// This class, combine AccessWebApi class with offline Database class for backup
    /// </summary>
    public class HostWebService
    {
        public IDisposable timerSubscription { get; set; }
        public IDisposable checkConnectSubscription { get; set; }
        public List<IDisposable> lstWebApiBackupSubscription { get; set; }
        public AccessSecuredWebApi.AccessWebApi accessWebApi { get; set; }
        public ReactiveProperty<string> mainMsg { get; private set; } //Message content of PM Mode control
        public string hostWebConnectStatus { get; set; }
        public string timeInfo { get; set; }

        public void IniHostWebService()
        {
            var state = nspAppStore.clsAppStore.GetCurrentState();

            this.accessWebApi = new AccessSecuredWebApi.AccessWebApi(
                    state.GlobalSetting.HostWebsiteAddress,
                    state.GlobalSetting.HostWebsiteUserName,
                    state.GlobalSetting.HostWebsitePassWord
                );
            this.StartObserverTiming();
        }

        public void StartObserverTiming()
        {
            this.timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(1000)) //, DispatcherScheduler.Current) //Subscribe event on UI Thread
                .Subscribe(x =>
                {
                    this.ObserverTask();
                    this.CreateMainMsg();
                });

            this.checkConnectSubscription = Observable.Interval(TimeSpan.FromMilliseconds(5000))
                .Subscribe(x  =>
                {
                    this.checkWebhostConnect();
                    this.CreateMainMsg();
                });
        }

        public void ObserverTask()
        {
            this.timeInfo = DateTime.Now.ToString();
        }

        public void checkWebhostConnect()
        {
            //Check connection with host website
            bool testConnect = this.accessWebApi.TestServerConnection();
            if (testConnect == true)
            {
                this.hostWebConnectStatus = "Host Website connect OK!" + "\r\n";
            }
            else
            {
                this.hostWebConnectStatus = "Host Website connect NG." + "\r\n";
            }
            this.hostWebConnectStatus += "Last Update: " + DateTime.Now.ToString("HH:mm:ss");

            //Update to Redux App Store
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SetSideBarContent(this.hostWebConnectStatus));
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.ChangeHostWebConnectStatusAction(testConnect));
        }

        public void CreateMainMsg()
        {
            this.mainMsg.Value = this.timeInfo + "\r\n" +
                "********************" + "\r\n" +
                this.hostWebConnectStatus;
        }

        //Create new web api with back up item service
        public void newWebApiBackup(string infoName, string uriBase, string requestPath, List<object> queryParams, int interval)
        {
            //Check if Offline database already contain entity with name strInfoName
            var mydatabase = new LiteDBDatabase.LiteDBHandle();
            if (mydatabase.isBackupInfoExist(infoName) == false) //Not Exist. Need to create new 
            {
                mydatabase.CreateBackupInfo(infoName, null);
            }
            //
            var subscription = Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Subscribe(x =>
                {
                    //Call GET() method & do back up
                    this.CallWebApiGetAndBackup(infoName, uriBase, requestPath, queryParams);
                });
            this.lstWebApiBackupSubscription.Add(subscription);
        }

        public void CallWebApiGetAndBackup(string infoName, string uriBase, string requestPath, List<object> queryParams)
        {
            var ret = this.accessWebApi.GetApiCall(requestPath, queryParams);
            if(ret!=null) //success
            {
                //Do backup
                var mydatabase = new LiteDBDatabase.LiteDBHandle();
                mydatabase.UpdateBackupInfo(infoName, ret);
            }
        }


        //Constructor
        public HostWebService()
        {
            this.lstWebApiBackupSubscription = new List<IDisposable>();
            this.mainMsg = new ReactiveProperty<string>();
            this.mainMsg.Value = "Ini for Main Message...";

            //First, we need reading configuration of Store (which config from setting file)
            //If setting is allow to use Host Website => Ini for host website service
            //Dispatch to save instance of HostWebService to App Store
            nspAppStore.clsAppStore.AppStore.Dispatch(new nspAppStore.AppActions.SavingHostWebServiceInstanceAction(this));
            //
            this.IniHostWebService();
        }

        //Destructor
        ~HostWebService()
        {
            if(this.timerSubscription!=null) this.timerSubscription.Dispose();
            if (this.checkConnectSubscription != null) this.checkConnectSubscription.Dispose();
        }

    }
}
