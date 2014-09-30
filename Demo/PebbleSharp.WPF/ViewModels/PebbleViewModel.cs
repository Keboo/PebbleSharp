using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PebbleSharp.Core;
using PebbleSharp.WPF.Messages;

namespace PebbleSharp.WPF.ViewModels
{
    public class PebbleViewModel : ViewModelBase
    {
        private readonly RelayCommand _toggleConnectionCommand;

        private readonly Pebble _pebble;

        public PebbleViewModel( Pebble pebble )
        {
            if ( pebble == null ) throw new ArgumentNullException( "pebble" );
            _pebble = pebble;
            
            _toggleConnectionCommand = new RelayCommand( OnToggleConnect );
        }

        public string PebbleId
        {
            get { return _pebble.PebbleID; }
        }

        private bool _IsConnected;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { Set(() => IsConnected, ref _IsConnected, value); }
        }

        public ICommand ToggleConnectionCommand
        {
            get { return _toggleConnectionCommand; }
        }

        private async void OnToggleConnect( )
        {
            if (IsConnected)
            {
                _pebble.Disconnect();
                MessengerInstance.Send(new PebbleDisconnected(_pebble));
            }
            else
            {
                await _pebble.ConnectAsync();
                MessengerInstance.Send(new PebbleConnected(_pebble));
            }
            IsConnected = !IsConnected;
        }
    }
}