using System;
using System.Collections.Generic;
namespace PebbleSharp.Core
{
	public class AppVersionResponse
	{
		/*
		 source: https://github.com/pebble/libpebble2/blob/c1a3a052d07b357abf03f86aa4ab01cf47f33ada/libpebble2/protocol/system.py
		protocol_version = Uint32()  # Unused as of v3.0
	    session_caps = Uint32()  # Unused as of v3.0
	    platform_flags = Uint32()
	    response_version = Uint8(default=2)
	    major_version = Uint8()
	    minor_version = Uint8()
	    bugfix_version = Uint8()
	    protocol_caps = Uint64()*/

		public uint ProtocolVersion { get; set; }
		public uint SessionCaps { get; set; }
		public uint PlatformFlags { get; set; }
		public byte ResponseVersion { get; set; }
		public byte MajorVersion { get; set; }
		public byte MinorVersion { get; set; }
		public byte BugfixVersion { get; set; }
		public ulong ProtocolCaps { get; set; }

		/*
		source: https://github.com/pebble/libpebble2/blob/d9ecce4a345f31217fb510f2f4e840f7cdda235b/libpebble2/communication/__init__.py
		 packet = PhoneAppVersion(message=AppVersionResponse(
            protocol_version=0xFFFFFFFF,
            session_caps=0x80000000,
            platform_flags=50,
            response_version=2,
            major_version=3,
            minor_version=0,
            bugfix_version=0,
            protocol_caps=0xFFFFFFFFFFFFFFFF
        ))
		 */

		public AppVersionResponse()
		{
			ProtocolVersion = 0xFFFFFFFF;
			SessionCaps = 0x80000000;
			PlatformFlags = 50;
			ResponseVersion = 2;
			MajorVersion = 3;
			MinorVersion = 6;
			BugfixVersion = 0;
			ProtocolCaps = 0xFFFFFFFFFFFFFFFF;
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.AddRange(BitConverter.GetBytes(ProtocolVersion));
			bytes.AddRange(BitConverter.GetBytes(SessionCaps));
			bytes.AddRange(BitConverter.GetBytes(PlatformFlags));
			bytes.Add(ResponseVersion);
			bytes.Add(MajorVersion);
			bytes.Add(MinorVersion);
			bytes.Add(BugfixVersion);
			bytes.AddRange(BitConverter.GetBytes(ProtocolCaps));
			return bytes.ToArray();
		}
	}
}

