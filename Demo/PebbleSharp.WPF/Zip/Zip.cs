using System.IO;
using System.Linq;
using Ionic.Zip;
using PebbleSharp.Core;

namespace PebbleSharp.WPF.Zip
{
    public class Zip : IZip
    {
        private ZipFile _zipFile;
        private Stream _zipStream;

        public void Dispose()
        {
            if (_zipStream != null)
            {
                if (_zipStream.CanSeek)
                    _zipStream.Seek(0, SeekOrigin.Begin);
                _zipStream = null;
            }
            if (_zipFile != null)
            {
                _zipFile.Dispose();
                _zipFile = null;
            }
        }

        public bool Open(Stream zipStream)
        {
            _zipStream = zipStream;
            _zipFile = ZipFile.Read(zipStream);
            return true;
        }

        public Stream OpenEntryStream(string zipEntryName)
        {
            ZipEntry entry = _zipFile.Entries.FirstOrDefault(x => x.FileName == zipEntryName);
            if (entry != null)
                return entry.OpenReader();
            return null;
        }
    }
}