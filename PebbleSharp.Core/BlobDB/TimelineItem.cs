using System;
using System.Collections.Generic;

namespace PebbleSharp.Core
{
	public class TimelineItem
	{
		public enum TimelineItemType:byte
		{
			Notification=1,
			Pin=2,
			Reminder=3
		}

		public UUID ItemId { get; set; }
		public UUID ParentId { get; set; }
		public DateTime TimeStamp { get; set; }
		public ushort Duration { get; set; }
		public TimelineItemType ItemType { get; set; }
		public ushort Flags { get; set; }
		public byte Layout { get; set; }
		public ushort DataLength { get; private set; }
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
		public byte ActionCount 
		{ 
			get
			{
				if (Actions != null)
				{
					return (byte)Actions.Count;
				}
				else
				{
					return 0;
				}
			}
		}
		public IList<TimelineAttribute> Attributes { get; set; }
		public IList<TimelineAction> Actions { get; set; }

		public TimelineItem()
		{
		}

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.AddRange(ItemId.Data);
			bytes.AddRange(ParentId.Data);
			bytes.AddRange(BitConverter.GetBytes(Util.GetTimestampFromDateTime(this.TimeStamp)));
			bytes.AddRange(BitConverter.GetBytes(Duration));
			bytes.Add((byte)this.ItemType);
			bytes.AddRange(BitConverter.GetBytes(this.Flags));
			bytes.Add(Layout);

			var attributeBytes = new List<byte>();
			if (Attributes != null)
			{
				foreach (var attribute in Attributes)
				{
					attributeBytes.AddRange(attribute.GetBytes());
				}
			}

			var actionBytes = new List<byte>();
			if (Actions != null)
			{
				foreach (var action in Actions)
				{
					actionBytes.AddRange(action.GetBytes());
				}
			}

			this.DataLength = (ushort)(attributeBytes.Count + actionBytes.Count);
			bytes.AddRange(BitConverter.GetBytes(DataLength));
			bytes.Add(AttributeCount);
			bytes.Add(ActionCount);
			bytes.AddRange(attributeBytes);
			bytes.AddRange(actionBytes);
			return bytes.ToArray();
		}
	}
}

