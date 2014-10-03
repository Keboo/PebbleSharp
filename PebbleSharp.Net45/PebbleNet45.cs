using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using PebbleSharp.Core;

namespace PebbleSharp.Net45
{
    public class PebbleNet45 : Pebble
    {
        public static IList<Pebble> DetectPebbles()
        {
            var client = new BluetoothClient();

            // A list of all BT devices that are paired, in range, and named "Pebble *" 
            var bluetoothDevices = client.DiscoverDevices( 20, true, false, false ).
                Where( bdi => bdi.DeviceName.StartsWith( "Pebble " ) );

            // A list of all available serial ports with some metadata including the PnP device ID,
            // which in turn contains a BT device address we can search for.
            var portListCollection = ( new ManagementObjectSearcher( "SELECT * FROM Win32_SerialPort" ) ).Get();
            var portList = new ManagementBaseObject[portListCollection.Count];
            portListCollection.CopyTo( portList, 0 );

            return ( from device in bluetoothDevices
                     from port in portList
                     where ( (string)port["PNPDeviceID"] ).Contains( device.DeviceAddress.ToString() )
                     select (Pebble)new PebbleNet45( new PebbleBluetoothConnection( (string)port["DeviceID"] ),
                         device.DeviceName.Substring( 7 ) ) ).ToList();
        }

        private PebbleNet45( PebbleBluetoothConnection connection, string pebbleId )
            : base( connection, pebbleId )
        { }

        private sealed class PebbleBluetoothConnection : IBluetoothConnection, IDisposable
        {
            private readonly SerialPort _SerialPort;
            public event EventHandler<BytesReceivedEventArgs> DataReceived = delegate { };

            public PebbleBluetoothConnection( string port )
            {
                _SerialPort = new SerialPort( port, 19200 );
                _SerialPort.ReadTimeout = 500;
                _SerialPort.WriteTimeout = 500;

                _SerialPort.DataReceived += SerialPortOnDataReceived;
            }

            ~PebbleBluetoothConnection()
            {
                Dispose( false );
            }

            public Task OpenAsync()
            {
                _SerialPort.Open();
                return Task.FromResult( (object)null );
            }

            public void Close()
            {
                _SerialPort.Close();
            }

            public void Write( byte[] data )
            {
                _SerialPort.Write( data, 0, data.Length );
            }

            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }

            private void Dispose( bool disposing )
            {
                if (disposing)
                {
                    _SerialPort.Dispose();
                }
            }

            private void SerialPortOnDataReceived( object sender, SerialDataReceivedEventArgs e )
            {
                int bytesToRead = _SerialPort.BytesToRead;
                if ( bytesToRead > 0 )
                {
                    var bytes = new byte[bytesToRead];
                    _SerialPort.Read( bytes, 0, bytesToRead );
                    DataReceived( this, new BytesReceivedEventArgs( bytes ) );
                }
            }
        }
    }
}