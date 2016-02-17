using System;
using System.Collections.Generic;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;

namespace PebbleSharp.Core.BlobDB
{
	[Endpoint(Endpoint.BlobDB)]
	public class BlobDBResponsePacket :ResponseBase
	{
		public ushort Token { get; private set;}
		public BlobStatus Response { get; private set; }

		public byte[] Payload { get; private set; }
		//token = Uint16()
		//response = Uint8(enum=BlobStatus)

		protected override void Load( byte[] payload )
		{
			if (payload.Length == 0)
			{
				SetError("BlobDB Command failed");
			}
			Payload = payload;
			Token = BitConverter.ToUInt16(payload, 0);
			Response = (BlobStatus)payload[2];
		}
	}


}

