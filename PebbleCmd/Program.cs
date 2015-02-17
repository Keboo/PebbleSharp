
using System;
using System.Linq;
using System.Threading.Tasks;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;
using PebbleSharp.Net45;

namespace PebbleCmd
{
    /// <summary>
    /// A simple console application for testing messages with the Pebble watch.
    /// </summary>
    class Program
    {
        static void Main()
        {
            ShowPebbles().Wait();
        }

        private static async Task ShowPebbles()
        {
            Console.WriteLine( "PebbleCmd" );
            Console.WriteLine( "Select Pebble to connect to:" );
            var pebbles = PebbleNet45.DetectPebbles();
            var options = pebbles.Select( x => x.PebbleID ).Union( new[] { "Exit" } );
            var menu = new Menu( options.ToArray() );
            var result = menu.ShowMenu();
            if ( result >= 0 && result < pebbles.Count )
            {
                var selectedPebble = pebbles[result];
                Console.WriteLine( "Connecting to Pebble " + selectedPebble.PebbleID );
                await selectedPebble.ConnectAsync();
                Console.WriteLine( "Connected" );
                await ShowPebbleMenu( selectedPebble );
            }
        }

        private static async Task ShowPebbleMenu( Pebble pebble )
        {
            var menu = new Menu(
                "Disconnect",
                "Get Time",
                "Set Current Time",
                "Get Firmware Info",
                "Send Ping",
                "Media Commands" );
            while ( true )
            {
                switch ( menu.ShowMenu() )
                {
                    case 0:
                        pebble.Disconnect();
                        return;
                    case 1:
                        var timeResult = await pebble.GetTimeAsync();
                        DisplayResult( timeResult, x => string.Format( "Pebble Time: " + x.Time.ToString( "G" ) ) );
                        break;
                    case 2:
                        await pebble.SetTimeAsync( DateTime.Now );
                        goto case 1;
                    case 3:
                        var firmwareResult = await pebble.GetFirmwareVersionAsync();
                        DisplayResult( firmwareResult,
                            x => string.Join( Environment.NewLine, "Firmware", x.Firmware.ToString(),
                                "Recovery Firmware", x.RecoveryFirmware.ToString() ) );
                        break;
                    case 4:
                        var pingResult = await pebble.PingAsync();
                        DisplayResult( pingResult, x => "Received Ping Response" );
                        break;
                    case 5:
                        ShowMediaCommands( pebble );
                        break;
                }
            }
        }

        private static void ShowMediaCommands( Pebble pebble )
        {
            Console.WriteLine( "Listening for media commands" );
            pebble.RegisterCallback<MusicControlResponse>( result =>
                DisplayResult( result, x => string.Format( "Media Control Response " + x.Command ) ) );

            var menu = new Menu( "Return to menu" );

            while ( true )
            {
                switch ( menu.ShowMenu() )
                {
                    case 0:
                        return;
                }
            }
        }

        private static void DisplayResult<T>( T result, Func<T, string> successData )
            where T : ResponseBase
        {
            if ( result.Success )
            {
                Console.WriteLine( successData( result ) );
            }
            else
            {
                Console.WriteLine( "ERROR" );
                Console.WriteLine( result.ErrorMessage );
                Console.WriteLine( result.ErrorDetails.ToString() );
            }
        }
    }
}
