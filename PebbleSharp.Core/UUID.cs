using System;
using System.Linq;

namespace PebbleSharp.Core
{
    public class UUID
    {
        public const int SIZE = 16;
        private readonly byte[] _data;

        public UUID( byte[] data )
        {
            if ( data == null ) throw new ArgumentNullException( "data" );
            if ( data.Length != SIZE ) throw new ArgumentException( string.Format( "UUID data must be {0} bytes", SIZE ), "data" );
            _data = data;
        }

        public UUID(string s)
        {
            _data = FromString(s);
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public override bool Equals( object obj )
        {
            var other = obj as UUID;
            if ( other != null )
                return Equals( other );
            return false;
        }

        protected bool Equals( UUID other )
        {
            if ( other == null )
                return false;
            return !_data.Where((t, i) => t != other._data[i]).Any();
        }

        public override int GetHashCode()
        {
            return ( _data != null ? _data.GetHashCode() : 0 );
        }

        public byte[] FromString(string s)
        {
            if (s.Length != 36)
            {
                throw new ArgumentException("Invalid uuid string");
            }
            //22a27b9a-0b07-47af-ad87-b2c29305bab6
            s = s.Replace("-", "");
            var result= StringToByteArray(s);
            return result;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "{0:x2}{1:x2}{2:x2}{3:x2}-{4:x2}{5:x2}-{6:x2}{7:x2}-{8:x2}{9:x2}-{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}",
                    _data[0], _data[1], _data[2], _data[3], _data[4], _data[5], _data[6], _data[7], _data[8], _data[9],
                    _data[10],
                    _data[11], _data[12], _data[13], _data[14], _data[15] );
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}