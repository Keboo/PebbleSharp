using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;
using PebbleSharp.Universal.Universal.Common;

namespace PebbleSharp.Universal.Universal.ViewModels
{
    public class InfoViewModel : PebbleViewModel
    {
        private readonly ICommand _setTimeCommand;

        public InfoViewModel()
        {
            _setTimeCommand = new RelayCommand( OnSetTime );
            TimeDisplay = "Waiting for Pebble";
        }

        public ICommand SetTimeCommand
        {
            get { return _setTimeCommand; }
        }

        private string _timeDisplay;
        public string TimeDisplay
        {
            get { return _timeDisplay; }
            set { Set( ref _timeDisplay, value ); }
        }

        private FirmwareVersion _Firmware;
        public FirmwareVersion Firmware
        {
            get { return _Firmware; }
            set { Set( ref _Firmware, value ); }
        }

        private FirmwareVersion _RecoveryFirmware;
        public FirmwareVersion RecoveryFirmware
        {
            get { return _RecoveryFirmware; }
            set { Set( ref _RecoveryFirmware, value ); }
        }

        public override async Task RefreshAsync()
        {
            await GetPebbleTimeAsyc();
            await LoadFirmwareAsync();
        }

        private async void OnSetTime()
        {
            if ( Pebble != null )
            {
                TimeDisplay = "Setting Pebble time";
                await Pebble.SetTimeAsync( DateTime.Now );
                await GetPebbleTimeAsyc();
            }
        }

        private async Task GetPebbleTimeAsyc()
        {
            TimeDisplay = "Getting curret Pebble time";
            TimeResponse timeResponse = await Pebble.GetTimeAsync();
            var current = DateTime.Now;
            if ( timeResponse.Success )
            {
                var differece = timeResponse.Time - current;
                if ( differece < TimeSpan.FromSeconds( 2 ) )
                {
                    TimeDisplay = "Pebble time is in sync with the phone";
                }
                else
                {
                    TimeDisplay = string.Format( "Pebble is {0} {1} than the phone", differece.ToString( @"h\:mm\:ss" ),
                        timeResponse.Time > current ? "faster" : "slower" );
                }
            }
            else
            {
                TimeDisplay = "Failed to get time from Pebble: " + timeResponse.ErrorMessage;
            }
        }

        private async Task LoadFirmwareAsync()
        {
            FirmwareVersionResponse firmwareResponse = await Pebble.GetFirmwareVersionAsync();
            if ( firmwareResponse.Success )
            {
                Firmware = firmwareResponse.Firmware;
                RecoveryFirmware = firmwareResponse.RecoveryFirmware;
            }
        }
    }
}