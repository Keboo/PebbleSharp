// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using Windows.UI.Xaml.Navigation;
using PebbleSharp.Core;
using PebbleSharp.Universal.Universal.ViewModels;

namespace PebbleSharp.Universal.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionPage
    {
        public ConnectionPage()
        {
            ViewModel = new ConnectionViewModel( OnConnect );
            InitializeComponent();
        }

        public ConnectionViewModel ViewModel { get; private set; }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo( NavigationEventArgs e )
        {
            await ViewModel.ScanForPairedDevicesAsync();
        }

        private void OnConnect( Pebble pebble )
        {
            Frame.Navigate( typeof( MainPage ), pebble );
        }
    }
}
