using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using PebbleSharp.Core;
using PebbleSharp.Universal.Universal.ViewModels;

namespace PebbleSharp.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly MainPageViewModel _viewModel = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public MainPageViewModel ViewModel
        {
            get { return _viewModel; }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo( NavigationEventArgs e )
        {
            var pebble = (Pebble)e.Parameter;

            await _viewModel.SetPebbleAsync( pebble );

            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void PebbleAppClicked( object sender, ItemClickEventArgs e )
        {

        }

        private void AppsPivotLoaded( object sender, RoutedEventArgs e )
        {
            //TODO: Delay loading the apps until here
        }
    }
}
