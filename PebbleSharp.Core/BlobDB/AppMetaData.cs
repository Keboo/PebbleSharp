using System;
using System.Collections.Generic;
using PebbleSharp.Core.Bundles;

namespace PebbleSharp.Core.BlobDB
{
	public class AppMetaData
	{
		public UUID UUID { get; set; }
		public UInt32 Flags { get; set; }
		public UInt32 Icon { get; set; }
		public byte AppVersionMajor { get; set; }
		public byte AppVersionMinor { get; set; }
		public byte SdkVersionMajor { get; set; }
		public byte SdkVersionMinor { get; set; }
		public byte AppFaceBackgroundColor { get; set; }
		public byte AppFaceTemplateId { get; set; }
		public string Name { get; set; }/*Fixed length 96*/

		public byte[] GetBytes()
		{
			var bytes = new List<byte>();
			bytes.AddRange(this.UUID.Data);
			bytes.AddRange(BitConverter.GetBytes(Flags));
			bytes.AddRange(BitConverter.GetBytes(Icon));
			bytes.Add(AppVersionMajor);
			bytes.Add(AppVersionMinor);
			bytes.Add(SdkVersionMajor);
			bytes.Add(SdkVersionMinor);
			bytes.Add(AppFaceBackgroundColor);
			bytes.Add(AppFaceTemplateId);
			var name = Name;

			//TODO: build "fixed" type strings into pebblesharp core
			if (name.Length > 96)
			{
				name = name.Substring(0, 96);
			}
			name = name.PadRight(96, '\0');

			var nameBytes = Util.GetBytes(name, false);

			bytes.AddRange(nameBytes);
			return bytes.ToArray();
		}

		public static AppMetaData FromAppBundle(AppBundle bundle, byte appFaceTemplateId = 0, byte appFaceBackgroundColor = 0)
		{
			var meta = new PebbleSharp.Core.BlobDB.AppMetaData();
			meta.AppFaceTemplateId = appFaceTemplateId;
			meta.AppFaceBackgroundColor = appFaceBackgroundColor;
			meta.AppVersionMajor = bundle.AppMetadata.AppMajorVersion;
			meta.AppVersionMinor = bundle.AppMetadata.AppMinorVersion;
			meta.SdkVersionMajor = bundle.AppMetadata.SDKMajorVersion;
			meta.SdkVersionMinor = bundle.AppMetadata.SDKMinorVersion;
			meta.Flags = bundle.AppMetadata.Flags;
			meta.Icon = bundle.AppMetadata.IconResourceID;
			meta.UUID = bundle.AppMetadata.UUID;
			meta.Name = bundle.AppMetadata.AppName;

			return meta;
		}
	}
}

