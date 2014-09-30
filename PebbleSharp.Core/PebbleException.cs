using System;

namespace PebbleSharp.Core
{
    public class PebbleException : Exception
    {
        public PebbleException()
        {
        }

        public PebbleException( string message )
            : base(message)
        {
        }

        public PebbleException( string message, Exception innerException )
            : base(message, innerException)
        {
        }
    }
}