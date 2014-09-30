using System;
using System.IO;

namespace PebbleSharp.Core
{
    public interface IZip : IDisposable
    {
        bool Open( Stream zipStream );
        Stream OpenEntryStream( string zipEntryName );
    }
}