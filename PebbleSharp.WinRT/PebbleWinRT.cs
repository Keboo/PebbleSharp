using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using PebbleSharp.Core;

namespace PebbleSharp.WinRT
{
    public class PebbleWinRT : Pebble
    {
        public static async Task<IList<Pebble>> DetectPebbles()
        {
            // Configure PeerFinder to search for all paired devices.
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            return ( await PeerFinder.FindAllPeersAsync() ).Select(
                    x => (Pebble)new PebbleWinRT( new PebbleBluetoothConnection( x ), x.DisplayName ) ).ToList();
        }

        private PebbleWinRT( PebbleBluetoothConnection connection, string pebbleId )
            : base( connection, pebbleId )
        {
        }

        private class PebbleBluetoothConnection : IBluetoothConnection
        {
            public event EventHandler<BytesReceivedEventArgs> DataReceived = delegate { };

            private readonly PeerInformation _peerInformation;
            private StreamSocket _socket;
            private StreamWatcher _streamWatcher;

            public PebbleBluetoothConnection( PeerInformation peerInformation )
            {
                if ( peerInformation == null ) throw new ArgumentNullException( "peerInformation" );
                _peerInformation = peerInformation;
            }

            public async Task OpenAsync()
            {
                try
                {
                    _socket = new StreamSocket();
                    //TODO: HostName only exists on windows phone. Need WinRT solution.
                    await _socket.ConnectAsync( _peerInformation.HostName, "1" );
                    _streamWatcher = new StreamWatcher( _socket.InputStream );
                    _streamWatcher.DataAvailible += StreamWatcherOnDataAvailible;
                }
                catch ( Exception e )
                {
                    Debug.WriteLine( e.ToString() );
                }
            }

            private void StreamWatcherOnDataAvailible( object sender, DataAvailibleEventArgs e )
            {
                DataReceived( this, new BytesReceivedEventArgs( e.Data ) );
            }

            public void Close()
            {
                if ( _streamWatcher != null )
                {
                    _streamWatcher.DataAvailible -= StreamWatcherOnDataAvailible;
                    _streamWatcher.Stop();
                    _streamWatcher = null;
                }

                if ( _socket != null )
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }

            public async void Write( byte[] data )
            {
                await _socket.OutputStream.WriteAsync( data.AsBuffer() );
            }
        }
    }
}
