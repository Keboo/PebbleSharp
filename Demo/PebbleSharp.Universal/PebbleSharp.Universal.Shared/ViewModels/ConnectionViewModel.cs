using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using PebbleSharp.Core;
using PebbleSharp.Universal.Universal.Common;
using PebbleSharp.WinRT;

namespace PebbleSharp.Universal.Universal.ViewModels
{
    public class ConnectionViewModel : ViewModelBase
    {
        private readonly Action<Pebble> _onConnect;
        private readonly ObservableCollection<Pebble> _pebbles = new ObservableCollection<Pebble>();
        private readonly RelayCommand _connectCommand;
        //private readonly NavigationHelper _navigationHelper;

        private Pebble _selectedPebble;

        public ConnectionViewModel( Action<Pebble> onConnect )
        {
            if ( onConnect == null ) throw new ArgumentNullException( "onConnect" );
            _onConnect = onConnect;
            _connectCommand = new RelayCommand( OnConnect, CanConnect );
        }

        public ObservableCollection<Pebble> Pebbles
        {
            get { return _pebbles; }
        }

        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
        }

        public Pebble SelectedPebble
        {
            get { return _selectedPebble; }
            set
            {
                if ( Set( ref _selectedPebble, value ) )
                    _connectCommand.RaiseCanExecuteChanged();
            }
        }

        public async Task ScanForPairedDevicesAsync()
        {
            _pebbles.Clear();
            _pebbles.Clear();
            foreach ( var pebble in await PebbleWinRT.DetectPebbles() )
                _pebbles.Add( pebble );

            if ( _pebbles.Count >= 1 )
                SelectedPebble = _pebbles[0];
        }

        private void OnConnect()
        {
            if ( SelectedPebble != null )
                _onConnect( SelectedPebble );
        }

        private bool CanConnect()
        {
            return SelectedPebble != null;
        }
    }
}