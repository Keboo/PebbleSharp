using System;
using System.Collections.Generic;
namespace PebbleSharp.Core
{
	public class TimelineAction
	{
		public enum TimelineActionType:byte
		{
			AncsDismiss = 0x01,
			Generic = 0x02,
			Response = 0x03,
			Dismiss = 0x04,
			HTTP = 0x05,
			Snooze = 0x06,
			OpenWatchapp = 0x07,
			Empty = 0x08,
			Remove = 0x09,
			OpenPin = 0x0a,
		}
		public byte ActionId { get; set; }
		public TimelineActionType ActionType { get; set; }
		public byte AttributeCount
		{
			get
			{
				if (Attributes != null)
				{
					return (byte)Attributes.Count;
				}
				else 
				{
					return 0;
				}
			}
		}
		public IList<TimelineAttribute> Attributes { get; set; }

		public TimelineAction()
		{
			
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.Add(ActionId);
			bytes.Add((byte)ActionType);
			bytes.Add(AttributeCount);
			if (Attributes != null)
			{
				foreach (var attribute in Attributes)
				{
					bytes.AddRange(attribute.GetBytes());
				}
			}
			return bytes.ToArray();
		}
	}
}

