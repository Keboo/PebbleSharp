using System;
using System.Threading.Tasks;

namespace PebbleSharp.Core.BlobDB
{
	public class BlobDBClient
	{
		private Pebble _pebble;
		private Random _random;

		public BlobDBClient(Pebble pebble)
		{
			_pebble = pebble;
			_random = new Random();
		}
		public async Task<BlobDBResponsePacket> Insert(BlobDatabase database, byte[] key, byte[] value)
		{
			var insertCommand = new BlobDBCommandPacket()
			{
				Token = GenerateToken(),
				Database = database,
				Command = BlobCommand.Insert,
				Key = key,
				Value = value
			};
			return await Send(insertCommand);
		}
		public async Task<BlobDBResponsePacket> Delete(BlobDatabase database, byte[] key)
		{
			var deleteCommand = new BlobDBCommandPacket()
			{
				Token = GenerateToken(),
				Database = database,
				Command = BlobCommand.Delete,
				Key = key,
			};
			return await Send(deleteCommand);
		}
		public async Task<BlobDBResponsePacket> Clear(BlobDatabase database)
		{
			var clearCommand = new BlobDBCommandPacket()
			{
				Token = GenerateToken(),
				Database = database,
				Command = BlobCommand.Clear
			};
			return await Send(clearCommand);
		}
		private async Task<BlobDBResponsePacket> Send(BlobDBCommandPacket command)
		{
			return await _pebble.SendBlobDBMessage(command);
		}
		public ushort GenerateToken()
		{
			//this is how libpebble2 does it...random.randrange(1, 2**16 - 1, 1)
			return (ushort)_random.Next(1, (2 ^ 16) - 1);
		}
	}
}

