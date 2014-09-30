using GalaSoft.MvvmLight;
using PebbleSharp.Core;
using PebbleSharp.WPF.Messages;

namespace PebbleSharp.WPF.ViewModels
{
    public class PebbleViewModelBase : ViewModelBase
    {
        protected Pebble _pebble;

        protected PebbleViewModelBase()
        {
            MessengerInstance.Register<PebbleConnected>( this, OnPebbleConnected );
            MessengerInstance.Register<PebbleDisconnected>( this, OnPebbleDisconnected );
        }

        protected virtual void OnPebbleConnected( PebbleConnected pebbleConnected )
        {
            _pebble = pebbleConnected.Pebble;
        }

        protected virtual void OnPebbleDisconnected( PebbleDisconnected pebbleDisconnected )
        {
            if ( pebbleDisconnected.Pebble == _pebble )
            {
                _pebble = null;
            }
        }
    }
}