using System;
using System.Collections.Generic;
namespace PebbleSharp.Core
{
	public class TimelineAttribute
	{
		public byte AttributeId { get; set; }
		public ushort Length
		{
			get
			{
				if (Content != null)
				{
					return (ushort)Content.Length;
				}
				else 
				{
					return 0;
				}
			}
		}
		public byte[] Content { get; set; }
		public TimelineAttribute()
		{
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.Add(AttributeId);
			bytes.AddRange(BitConverter.GetBytes(Length));
			bytes.AddRange(Content);
			return bytes.ToArray();
		}
	}
}

