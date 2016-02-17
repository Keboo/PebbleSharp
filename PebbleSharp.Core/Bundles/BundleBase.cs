using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace PebbleSharp.Core.Bundles
{
    /// <summary> Represents a Pebble app bundle (.pbw file). </summary>
    public abstract class BundleBase
    {
        public virtual bool HasResources { get; private set; }
        public BundleManifest Manifest { get; private set; }
        public virtual byte[] Resources { get; private set; }
		public Platform Platform { get; private set;}

        protected abstract void LoadData(IZip zip);

		protected string PlatformSubdirectory()
		{
			var platformSubdirectory = (Platform == Platform.UNKNOWN ? "" : Platform.ToString().ToLower()+"/");
			return platformSubdirectory;
		}

		private BundleManifest LoadManifest(IZip zip)
		{
			using (var manifestStream = zip.OpenEntryStream(PlatformSubdirectory() + "manifest.json"))
			{
				var serializer = new DataContractJsonSerializer(typeof(BundleManifest));
				return (BundleManifest)serializer.ReadObject(manifestStream);
			}
		}

        /// <summary>
        ///     Create a new PebbleBundle from a .pwb file and parse its metadata.
        /// </summary>
        /// <param name="bundle">The stream to the bundle.</param>
        /// <param name="zip">The zip library implementation.</param>
		public void Load(IZip zip, Platform platform)
        {
			Platform = platform;
            Manifest = LoadManifest (zip);

            HasResources = (Manifest.Resources.Size != 0);

            if (HasResources)
            {
				using (Stream resourcesBinary = zip.OpenEntryStream(PlatformSubdirectory()+Manifest.Resources.Filename))
                {
                    if (resourcesBinary == null)
                        throw new PebbleException("Could not find resource entry in the bundle");

                    Resources = Util.GetBytes(resourcesBinary);
                }
            }

            LoadData(zip);
        }
    }
}