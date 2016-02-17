using System;
using System.Linq;
using PebbleSharp.Core.Responses;
namespace PebbleSharp.Core
{
	[Endpoint(Endpoint.AppFetch)]
	public class AppFetchRequestPacket:ResponseBase
	{
		public byte Command { get; set; }
		public UUID UUID { get; set; }
		public int AppId { get; set; }

		public AppFetchRequestPacket()
		{
		}

		protected override void Load(byte[] payload)
		{
			Command = payload[0];
			UUID = new UUID(payload.Skip(1).Take(16).ToArray());


			//this packet is defined as little endian, which is slightly abnormal since 
			//it is coming from the pebble
			//(most packets from he pebble are big endian / network endian)
			//TODO: refactor Util conversions to respect per packet endian attributes
			if (BitConverter.IsLittleEndian)
			{
				AppId = BitConverter.ToInt32(payload, 17);
			}
			else
			{
				//this will actually flip it because it thinks it is big endian.
				AppId = Util.GetInt32(payload, 17);
			}
		}
	}
}

