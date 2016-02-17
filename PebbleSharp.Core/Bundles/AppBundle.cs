using System;
using System.IO;
using PebbleSharp.Core.Serialization;
using System.Runtime.Serialization.Json;

namespace PebbleSharp.Core.Bundles
{
    public class AppBundle : BundleBase
    {
        public byte[] App { get; private set; }

        public ApplicationMetadata AppMetadata { get; private set; }
        public PebbleSharp.Core.NonPortable.AppInfo AppInfo { get; private set; }
        
        protected override void LoadData( IZip zip )
        {
            if ( string.IsNullOrWhiteSpace( Manifest.Application.Filename ) )
                throw new InvalidOperationException( "Bundle does not contain pebble app" );

			using ( Stream binStream = zip.OpenEntryStream( this.PlatformSubdirectory()+Manifest.Application.Filename ) )
            {
                if ( binStream == null )
                    throw new Exception( string.Format( "App file {0} not found in archive", Manifest.Application.Filename ) );

                App = Util.GetBytes( binStream );

                AppMetadata = BinarySerializer.ReadObject<ApplicationMetadata>( App );
            }
			//note, appinfo.json is NOT under the platform subdir
            using (Stream appinfoStream = zip.OpenEntryStream("appinfo.json"))
            {
                if (appinfoStream != null)
                {
                    var serializer = new DataContractJsonSerializer(typeof(PebbleSharp.Core.NonPortable.AppInfo));
                    AppInfo = (PebbleSharp.Core.NonPortable.AppInfo)serializer.ReadObject(appinfoStream);
                }
            }
        }

        public override string ToString()
        {
            return string.Format( "watch app {0}", AppMetadata );
        }
    }
}