using System;
using System.Collections.Generic;
using PebbleSharp.Core.Responses;
namespace PebbleSharp.Core
{
	[Endpoint( Endpoint.AppRunState)]
	public class AppRunStatePacket
	{
		public AppRunState Command { get; set; }
		public UUID UUID { get; set; }

		public AppRunStatePacket()
		{
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.Add((byte)Command);
			if (Command == AppRunState.Start || Command == AppRunState.Stop)
			{
				bytes.AddRange(UUID.Data);
			}
			return bytes.ToArray();
		}
	}
}

