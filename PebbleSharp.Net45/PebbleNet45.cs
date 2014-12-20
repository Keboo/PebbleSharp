using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using PebbleSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PebbleSharp.Net45
{
    public class PebbleNet45 : Pebble
    {
        public static IList<Pebble> DetectPebbles()
        {
            var client = new BluetoothClient();

            // A list of all BT devices that are paired, in range, and named "Pebble *" 
            var bluetoothDevices = client.DiscoverDevices( 20, true, false, false ).
                Where( bdi => bdi.DeviceName.StartsWith( "Pebble " ) ).ToList();

            return ( from device in bluetoothDevices
                     select (Pebble)new PebbleNet45( new PebbleBluetoothConnection( device ),
                         device.DeviceName.Substring( 7 ) ) ).ToList();
        }

        private PebbleNet45( PebbleBluetoothConnection connection, string pebbleId )
            : base( connection, pebbleId )
        { }

        private sealed class PebbleBluetoothConnection : IBluetoothConnection, IDisposable
        {
            private CancellationTokenSource _tokenSource;

            private readonly BluetoothDeviceInfo _deviceInfo;
            private NetworkStream _networkStream;
            private BluetoothClient _client;
            public event EventHandler<BytesReceivedEventArgs> DataReceived = delegate { };

            public PebbleBluetoothConnection( BluetoothDeviceInfo deviceInfo )
            {
                _deviceInfo = deviceInfo;
            }

            ~PebbleBluetoothConnection()
            {
                Dispose( false );
            }

            public Task OpenAsync()
            {
                return Task.Run( () =>
                {
                    _tokenSource = new CancellationTokenSource();
                    _client = new BluetoothClient();
                    _client.Connect( _deviceInfo.DeviceAddress, BluetoothService.SerialPort );
                    _networkStream = _client.GetStream();
                    Task.Factory.StartNew( CheckForData, _tokenSource.Token, TaskCreationOptions.LongRunning,
                        TaskScheduler.Default );
                } );
            }

            public void Close()
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                }
                
                if ( _client.Connected )
                {
                    _client.Close();
                }
            }

            public void Write( byte[] data )
            {
                if ( _networkStream.CanWrite )
                {
                    _networkStream.Write( data, 0, data.Length );
                }
            }

            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }

            private void Dispose( bool disposing )
            {
                if ( disposing )
                {
                    Close();
                }
            }

            private async void CheckForData()
            {
                try
                {
                    while ( true )
                    {
                        if ( _tokenSource.IsCancellationRequested )
                            return;

                        if ( _networkStream.CanRead && _networkStream.DataAvailable )
                        {
                            var buffer = new byte[256];
                            var numRead = _networkStream.Read( buffer, 0, buffer.Length );
                            Array.Resize( ref buffer, numRead );
                            DataReceived( this, new BytesReceivedEventArgs( buffer ) );
                        }

                        if ( _tokenSource.IsCancellationRequested )
                            return;
                        await Task.Delay( 10 );
                    }
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
    }
}