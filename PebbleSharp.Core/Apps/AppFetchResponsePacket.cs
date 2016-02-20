using System;
using System.Collections.Generic;
using PebbleSharp.Core.Responses;
namespace PebbleSharp.Core
{
	[Endpoint(Endpoint.AppFetch)]
	public class AppFetchResponsePacket
	{
		public byte Command { get; set; }
		public AppFetchStatus Response { get; set; }

		public AppFetchResponsePacket()
		{
			Command = 1;
		}

		public byte[] GetBytes()
		{
			return new byte[]{Command,(byte)Response};
		}
	}
}

