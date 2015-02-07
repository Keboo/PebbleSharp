using System;
using System.Collections.Generic;

namespace PebbleSharp.Core.Responses
{
    [Endpoint( Endpoint.ApplicationMessage )]
    [Endpoint( Endpoint.Launcher )]
    public class ApplicationMessageResponse : ResponseBase
    {
        private readonly Dictionary<string, object> _ParsedData = new Dictionary<string, object>();
        public Dictionary<string, object> ParsdData
        {
            get { return _ParsedData; }
        }

        private UUID _UUID;
        public UUID TargetUUID
        {
            get { return _UUID; }
        }

        protected override void Load( byte[] payload )
        {
            //byte[] data;
            //if ( payload.Length > 0 )
            //{
            //    data = payload.Skip( 1 ).ToArray();
            //}
            //else
            //{
            //    data = new byte[0];
            //}

            //if ( Enum.IsDefined( typeof( AppMessage ), payload[0] ) )
            //{
            //    AppMessage message = (AppMessage)payload[0];
            //    var messageBytes = Util.GetBytes( message.ToString().ToUpperInvariant(), false );
            //    int messageLength = messageBytes.Length;
            //    Array.Resize( ref messageBytes, messageLength + data.Length );
            //    Array.Copy( data, 0, messageBytes, messageLength, data.Length );
            //    data = messageBytes;
            //}
            var data = payload;

            byte command;
            byte tid;

            var index = data.UnpackLE( 0, out command, out tid );

            if ( command == 1 )
            {
                byte tupleCount;
                index = data.UnpackLE( index, out _UUID, out tupleCount );

                for ( int i = 0; i < tupleCount; i++ )
                {
                    uint k;
                    byte t;
                    ushort l;
                    index = data.UnpackLE( index, out k, out t, out l );

                    object v = null;
                    switch ( t )
                    {
                        case 0:
                            var byteArray = new byte[l];
                            Array.Copy( data, index, byteArray, 0, l );
                            v = byteArray;
                            index += l;
                            break;
                        case 1:
                            break;
                        case 2:
                            switch ( l )
                            {
                                case 1:
                                    byte @byte;
                                    index = data.UnpackLE( index, out @byte );
                                    v = @byte;
                                    break;
                                case 2:
                                    ushort @ushort;
                                    index = data.UnpackLE( index, out @ushort );
                                    v = @ushort;
                                    break;
                                case 4:
                                    uint @uint;
                                    index = data.UnpackLE( index, out @uint );
                                    v = @uint;
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }
                            break;
                        case 3:
                            switch ( l )
                            {
                                case 1:
                                    sbyte @sbyte;
                                    index = data.UnpackLE( index, out @sbyte );
                                    v = @sbyte;
                                    break;
                                case 2:
                                    short unsignedShort;
                                    index = data.UnpackLE( index, out unsignedShort );
                                    v = unsignedShort;
                                    break;
                                case 4:
                                    int unsignedInt;
                                    index = data.UnpackLE( index, out unsignedInt );
                                    v = unsignedInt;
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    _ParsedData[k.ToString()] = v;
                    //TODO: App keys
                }
            }
        }
    }
}