using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PebbleSharp.Universal.Universal.Common;

namespace PebbleSharp.Universal.Universal.ViewModels
{
    public class AppsViewModel : PebbleViewModel
    {
        private readonly ICommand _uninstallAppCommand;

        public AppsViewModel()
        {
            _uninstallAppCommand = new RelayCommand<Core.App?>(OnUninstallApp);
            Apps = new ObservableCollection<Core.App>();
        }

        public ObservableCollection<Core.App> Apps { get; private set; }

        public ICommand UninstallCommand
        {
            get { return _uninstallAppCommand; }
        }

        public override async Task RefreshAsync()
        {
            await LoadAppsAsync();
        }

        private async Task LoadAppsAsync()
        {
            var appBankContents = await Pebble.InstallClient.GetAppbankContentsAsync();
            Apps.Clear();
            if (appBankContents != null && appBankContents.AppBank != null && appBankContents.AppBank.Apps != null)
            {
                foreach (var app in appBankContents.AppBank.Apps)
                {
                    Apps.Add(app);
                }
            }
        }

        private async void OnUninstallApp( Core.App? app )
        {
            if (app == null)
                return;

            var response = await Pebble.RemoveAppAsync(app.Value);
            if (response.Success)
            {
                await LoadAppsAsync();
            }
        }
        
    }
}