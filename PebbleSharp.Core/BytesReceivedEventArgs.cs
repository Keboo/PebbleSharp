using System;

namespace PebbleSharp.Core
{
    //TODO: Better namespace
    public class BytesReceivedEventArgs : EventArgs
    {
        public BytesReceivedEventArgs()
            : this ( new byte[0] )
        { }

        public BytesReceivedEventArgs( byte[] bytes )
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; set; }
    }
}