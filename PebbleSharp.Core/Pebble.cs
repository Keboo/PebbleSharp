using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.NonPortable.AppMessage;
using PebbleSharp.Core.Responses;
using PebbleSharp.Core.BlobDB;
using PebbleSharp.Core.Install;

namespace PebbleSharp.Core
{
    /// <summary>
    ///     Represents a (connection to a) Pebble.
    ///     PebbleProtocol is blissfully unaware of the *meaning* of anything,
    ///     all that is handled here.
    /// </summary>
    public abstract class Pebble
    {
		public FirmwareVersion Firmware { get; private set;}
		private readonly PebbleProtocol _PebbleProt;

        private readonly Dictionary<Type, List<CallbackContainer>> _callbackHandlers;
        private readonly ResponseManager _responseManager = new ResponseManager();
        
		public BlobDBClient BlobDBClient { get; private set;}
		public PutBytesClient PutBytesClient { get; private set;}
		public InstallClient InstallClient { get; private set;}

        /// <summary>
        ///     Create a new Pebble
        /// </summary>
        /// <param name="connection">The port to use to connect to the pebble</param>
        /// <param name="pebbleId">
        ///     The four-character Pebble ID, based on its BT address.
        ///     Nothing explodes when it's incorrect, it's merely used for identification.
        /// </param>
        protected Pebble( IBluetoothConnection connection, string pebbleId )
        {
			ResponseTimeout = TimeSpan.FromSeconds( 5 );
            PebbleID = pebbleId;
			this.BlobDBClient = new BlobDBClient(this);
			this.PutBytesClient = new PutBytesClient(this);
			this.InstallClient = new InstallClient(this);

            _callbackHandlers = new Dictionary<Type, List<CallbackContainer>>();

            _PebbleProt = new PebbleProtocol( connection );
            _PebbleProt.RawMessageReceived += RawMessageReceived;

			RegisterCallback<AppMessagePacket>( OnApplicationMessageReceived );
        }

        /// <summary>
        ///     The four-char ID for the Pebble, based on its BT address.
        /// </summary>
        public string PebbleID { get; private set; }

        /// <summary>
        ///     The port the Pebble is on.
        /// </summary>
        public IBluetoothConnection Connection
        {
            get { return _PebbleProt.Connection; }
        }

        public bool IsAlive { get; private set; }

        public TimeSpan ResponseTimeout { get; set; }

        /// <summary>
        ///     Connect with the Pebble.
        /// </summary>
        /// <exception cref="System.IO.IOException">Passed on when no connection can be made.</exception>
        public async Task ConnectAsync()
        {
            PhoneVersionResponse response;
            //PhoneVersionResponse is received immediately after connecting, and we must respond to it before making any other calls
            using ( IResponseTransaction<PhoneVersionResponse> responseTransaction =
                    _responseManager.GetTransaction<PhoneVersionResponse>() )
            {
                await _PebbleProt.ConnectAsync();
                response = responseTransaction.AwaitResponse( ResponseTimeout );
            }

            if ( response != null )
            {
				var message = new AppVersionResponse();
				await SendMessageNoResponseAsync( Endpoint.PhoneVersion, message.GetBytes() );
                IsAlive = true;

				//get the firmware details, we'll need to know the platform and version for possible future actions
				var firmwareResponse = await this.GetFirmwareVersionAsync();
				this.Firmware = firmwareResponse.Firmware;
            }
            else
            {
                Disconnect();
            }
		}



        /// <summary>
        ///     Disconnect from the Pebble, if a connection existed.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _PebbleProt.Close();
            }
            finally
            {
                // If closing the serial port didn't work for some reason we're still effectively 
                // disconnected, although the port will probably be in an invalid state.  Need to 
                // find a good way to handle that.
                IsAlive = false;
            }
        }

        public void RegisterCallback<T>( Action<T> callback ) where T : IResponse, new()
        {
            if ( callback == null ) throw new ArgumentNullException( "callback" );

            List<CallbackContainer> callbacks;
            if ( _callbackHandlers.TryGetValue( typeof( T ), out callbacks ) == false )
                _callbackHandlers[typeof( T )] = callbacks = new List<CallbackContainer>();

            callbacks.Add( CallbackContainer.Create( callback ) );
        }

        public bool UnregisterCallback<T>( Action<T> callback ) where T : IResponse
        {
            if ( callback == null ) throw new ArgumentNullException( "callback" );
            List<CallbackContainer> callbacks;
            if ( _callbackHandlers.TryGetValue( typeof( T ), out callbacks ) )
                return callbacks.Remove( callbacks.FirstOrDefault( x => x.IsMatch( callback ) ) );
            return false;
        }

        /// <summary> Send the Pebble a ping. </summary>
        /// <param name="pingData"></param>
        public async Task<PingResponse> PingAsync( uint pingData = 0 )
        {
            // No need to worry about endianness as it's sent back byte for byte anyway.
            byte[] data = Util.CombineArrays( new byte[] { 0 }, Util.GetBytes( pingData ) );

            return await SendMessageAsync<PingResponse>( Endpoint.Ping, data );
        }

        /// <summary> Generic notification support.  Shouldn't have to use this, but feel free. </summary>
        /// <param name="type">Notification type.  So far we've got 0 for mail, 1 for SMS.</param>
        /// <param name="parts">Message parts will be clipped to 255 bytes.</param>
        private async Task NotificationAsync( byte type, params string[] parts )
        {
            string timeStamp = Util.GetTimestampFromDateTime( DateTime.Now ).ToString( CultureInfo.InvariantCulture );

            //TODO: This needs to be refactored
            parts = parts.Take( 2 ).Concat( new[] { timeStamp } ).Concat( parts.Skip( 2 ) ).ToArray();

            byte[] data = { type };
            data = parts.Aggregate( data, ( current, part ) => current.Concat( Util.GetBytes( part ) ).ToArray() );
            await SendMessageNoResponseAsync( Endpoint.Notification, data );
        }

        /// <summary>
        ///     Send an email notification.  The message parts are clipped to 255 bytes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public async Task NotificationMailAsync( string sender, string subject, string body )
        {
            await NotificationAsync( 0, sender, body, subject );
        }

        /// <summary>
        ///     Send an SMS notification.  The message parts are clipped to 255 bytes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="body"></param>
        public async Task NotificationSmsAsync( string sender, string body )
        {
            await NotificationAsync( 1, sender, body );
        }

        public async Task NotificationFacebookAsync( string sender, string body )
        {
            await NotificationAsync( 2, sender, body );
        }

        public async Task NotificationTwitterAsync( string sender, string body )
        {
            await NotificationAsync( 3, sender, body );
        }

        /// <summary>
        ///     Send "Now playing.." metadata to the Pebble.
        ///     The track, album and artist should each not be longer than 255 bytes.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="album"></param>
        /// <param name="artist"></param>
        public async Task SetNowPlayingAsync( string artist, string album, string track )
        {
            byte[] artistBytes = Util.GetBytes( artist );
            byte[] albumBytes = Util.GetBytes( album );
            byte[] trackBytes = Util.GetBytes( track );

            byte[] data = Util.CombineArrays( new byte[] { 16 }, artistBytes, albumBytes, trackBytes );

            await SendMessageNoResponseAsync( Endpoint.MusicControl, data );
        }

        /// <summary> Set the time on the Pebble. Mostly convenient for syncing. </summary>
        /// <param name="dateTime">The desired DateTime.  Doesn't care about timezones.</param>
        public async Task SetTimeAsync( DateTime dateTime )
        {
            byte[] timestamp = Util.GetBytes( Util.GetTimestampFromDateTime( dateTime ) );
            byte[] data = Util.CombineArrays( new byte[] { 2 }, timestamp );
            await SendMessageNoResponseAsync( Endpoint.Time, data );
        }

        /// <summary> Send a malformed ping (to trigger a LOGS response) </summary>
        public async Task<PingResponse> BadPingAsync()
        {
            byte[] cookie = { 1, 2, 3, 4, 5, 6, 7 };
            return await SendMessageAsync<PingResponse>( Endpoint.Ping, cookie );
        }

        

        public async Task<FirmwareVersionResponse> GetFirmwareVersionAsync()
        {
            return await SendMessageAsync<FirmwareVersionResponse>( Endpoint.FirmwareVersion, new byte[] { 0 } );
        }

        /// <summary>
        ///     Get the time from the connected Pebble.
        /// </summary>
        /// <returns>A TimeReceivedEventArgs with the time, or null.</returns>
        public async Task<TimeResponse> GetTimeAsync()
        {
            return await SendMessageAsync<TimeResponse>( Endpoint.Time, new byte[] { 0 } );
        }

        /// <summary>
        ///     Remove an app from the Pebble, using an App instance retrieved from the Appbank.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public async Task<AppbankInstallResponse> RemoveAppAsync( App app )
        {
            byte[] msg = Util.CombineArrays( new byte[] { 2 },
                                            Util.GetBytes( app.ID ),
                                            Util.GetBytes( app.Index ) );

            return await SendMessageAsync<AppbankInstallResponse>( Endpoint.AppManager, msg );
        }

        public async Task<SystemMessageResponse> SendSystemMessageAsync( SystemMessage message )
        {
            byte[] data = { 0, (byte)message };
            return await SendMessageAsync<SystemMessageResponse>( Endpoint.SystemMessage, data );
        }

        public async Task<T> SendMessageAsync<T>( Endpoint endpoint, byte[] payload )
            where T : class, IResponse, new()
        {
            return await Task.Run( () =>
                                      {
                                          try
                                          {
                                              lock ( _PebbleProt )
                                              {
                                                  using (
                                                      IResponseTransaction<T> responseTransaction =
                                                          _responseManager.GetTransaction<T>() )
                                                  {
                                                      _PebbleProt.SendMessage( (ushort)endpoint, payload );
                                                      return responseTransaction.AwaitResponse( ResponseTimeout );
                                                  }
                                              }
                                          }
                                          catch ( TimeoutException )
                                          {
                                              var result = new T();
                                              result.SetError( "TimeoutException occurred" );
                                              Disconnect();
                                              return result;
                                          }
                                          catch ( Exception e )
                                          {
                                              var result = new T();
                                              result.SetError( e.Message );
                                              return result;
                                          }
                                      } );
        }

        public Task SendMessageNoResponseAsync( Endpoint endpoint, byte[] payload )
        {
            return Task.Run( () =>
                                {
                                    try
                                    {
                                        lock ( _PebbleProt )
                                        {
                                            _PebbleProt.SendMessage( (ushort)endpoint, payload );
                                        }
                                    }
                                    catch ( TimeoutException )
                                    {
                                        Disconnect();
                                    }
                                    catch ( Exception )
                                    {
                                    }
                                } );
        }

        private void RawMessageReceived( object sender, RawMessageReceivedEventArgs e )
        {
			var endpoint = (Endpoint)e.Endpoint;
			IResponse response = _responseManager.HandleResponse( endpoint, e.Payload );

			if (response != null)
			{
				if (e.Endpoint == (ushort)Endpoint.PhoneVersion)
				{
					var message = new AppVersionResponse();
					SendMessageNoResponseAsync(Endpoint.PhoneVersion, message.GetBytes()).Wait();
				}

				//Check for callbacks
				List<CallbackContainer> callbacks;
				if (_callbackHandlers.TryGetValue(response.GetType(), out callbacks))
				{
					foreach (CallbackContainer callback in callbacks)
						callback.Invoke(response);
				}

			}

        }

		public async Task<BlobDBResponsePacket> SendBlobDBMessage(BlobDBCommandPacket command)
		{
			//TODO: I'm not sure we should assume that the first blobdb response we get is the one that 
			//corresponds to this request, we probably need to do extra work here to match up the token
			var bytes = command.GetBytes();

			return await SendMessageAsync<BlobDBResponsePacket>(Endpoint.BlobDB,bytes );
		}

		public async Task<AppMessagePacket> SendApplicationMessage(AppMessagePacket data)
        {
            //DebugMessage(data.GetBytes());
			return await SendMessageAsync<AppMessagePacket>(Endpoint.ApplicationMessage, data.GetBytes());
        }

        //self._pebble.send_packet(AppRunState(data=AppRunStateStart(uuid=app_uuid)))
        public async Task LaunchApp(UUID uuid)
        {
            var data = new AppMessagePacket();
            data.ApplicationId = uuid;
            data.Command = (byte)Command.Push;
            data.TransactionId = 1;
            data.Values.Add(new AppMessageUInt8() { Key=1,Value = 1 });//this one is key 0, doesn't actually do anything
            
            await SendMessageNoResponseAsync(Endpoint.Launcher, data.GetBytes());
        }

		private void OnApplicationMessageReceived( AppMessagePacket response )
        {
            SendMessageNoResponseAsync( Endpoint.ApplicationMessage, new byte[] { 0xFF, response.Values!=null ? response.TransactionId :(byte)0} );
		}

        private void DebugMessage(byte[] bytes)
        {
            StringBuilder payloadDebugger = new StringBuilder();
            foreach (var b in bytes)
            {
                payloadDebugger.Append(string.Format("{0}:", b));
            }

            Console.WriteLine(payloadDebugger.ToString());
        }

        public override string ToString()
        {
            return string.Format( "Pebble {0} on {1}", PebbleID, Connection );
        }

        private class CallbackContainer
        {
            private readonly Delegate _delegate;

            private CallbackContainer( Delegate @delegate )
            {
                _delegate = @delegate;
            }

            public bool IsMatch<T>( Action<T> callback )
            {
                return _delegate == (Delegate)callback;
            }

            public void Invoke( IResponse response )
            {
                _delegate.DynamicInvoke( response );
            }

            public static CallbackContainer Create<T>( Action<T> callback ) where T : IResponse, new()
            {
                return new CallbackContainer( callback );
            }
        }

		public void Reset(ResetCommand command)
		{
			_PebbleProt.SendMessage((ushort)Endpoint.Reset, new byte[] { (byte)command });
		}

    }
}