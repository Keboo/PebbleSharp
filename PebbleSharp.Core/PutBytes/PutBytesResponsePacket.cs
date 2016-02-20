using System;
namespace PebbleSharp.Core.Responses
{
    [Endpoint( Endpoint.PutBytes )]
    public class PutBytesResponsePacket : ResponseBase
    {
		public PutBytesResult Result { get; set; }
		public uint Token { get; set; }

        protected override void Load( byte[] payload )
        {
			Result = (PutBytesResult)payload[0];
			if (Result == PutBytesResult.Ack)
			{
				Token = Util.GetUInt32(payload, 1);
			}
			else
			{
				SetError(payload);
			}
        }
    }
}