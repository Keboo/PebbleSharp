using System;
using System.Collections.Generic;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;

namespace PebbleSharp.Core.BlobDB
{
	[Endpoint(Endpoint.BlobDB)]
	public class BlobDBCommandPacket
	{
		public BlobCommand Command {get;set;}
		public ushort Token { get; set; }
		public BlobDatabase Database { get; set; }

		//only used for insert and delete commands
		public byte[] Key { get; set; }
		public byte KeyLength
		{
			get
			{
				return (byte)Key.Length;
			}
		}

		//only used for insert
		public byte[] Value { get; set; }
		public ushort ValueLength
		{
			get
			{
				return (ushort)Value.Length;
			}
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.Add((byte)Command);
			bytes.AddRange(BitConverter.GetBytes(Token));
			bytes.Add((byte)this.Database);
			if (this.Command == BlobCommand.Insert || this.Command == BlobCommand.Delete)
			{
				bytes.Add(this.KeyLength);
				bytes.AddRange(this.Key);
				if (this.Command == BlobCommand.Insert)
				{
					bytes.AddRange(BitConverter.GetBytes(this.ValueLength));
					bytes.AddRange(this.Value);
				}
			}
			return bytes.ToArray();
		}
	}
}