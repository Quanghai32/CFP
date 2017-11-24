using nspAppStore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SideBarView
{
    /// <summary>
    /// Interaction logic for SideBarMainView.xaml
    /// </summary>
    [Export(typeof(SideBarMainView))]
    public partial class SideBarMainView : UserControl
    {
        public ReactiveProperty<string> MainMessage { get; set; }

        //
        IDisposable sub { get; set; }

        public SideBarMainView()
        {
            InitializeComponent();
            //Ini
            this.MainMessage = new ReactiveProperty<string>();
            this.MainMessage.Value = "";
            this.DataContext = this;
            //Subscribe
            this.sub = clsAppStore.AppStore
                .DistinctUntilChanged(state => new { state.sideBarContent })
                .Subscribe(
                    state => this.MainMessage.Value = state.sideBarContent
                );
        }

        ~SideBarMainView()
        {
            if (this.sub != null) this.sub.Dispose();
        }
    }
}
