
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
            var menu = new Menu( "Disconnect", "Get Time" );
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
