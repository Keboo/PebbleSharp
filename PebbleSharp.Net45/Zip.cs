using System;
using System.IO;
using System.Linq;
using Ionic.Zip;
using PebbleSharp.Core;

namespace PebbleSharp.Net45
{
    public sealed class Zip : IZip
    {
        private ZipFile _zipFile;
        private Stream _zipStream;

        ~Zip()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( _zipStream != null )
                {
                    if ( _zipStream.CanSeek )
                        _zipStream.Seek( 0, SeekOrigin.Begin );
                    _zipStream = null;
                }
                if ( _zipFile != null )
                {
                    _zipFile.Dispose();
                    _zipFile = null;
                }
            }
        }

        public bool Open( Stream zipStream )
        {
            _zipStream = zipStream;
            _zipFile = ZipFile.Read( zipStream );
            return true;
        }

        public Stream OpenEntryStream( string zipEntryName )
        {
            ZipEntry entry = _zipFile.Entries.FirstOrDefault( x => x.FileName == zipEntryName );
            if ( entry != null )
                return entry.OpenReader();
            return null;
        }
    }
}