using System;

namespace PebbleSharp.Core
{
    public static class Unpack
    {
        public static int UnpackLE<T1>( this byte[] bytes, int index, out T1 item1 )
        {
            return UnpackImpl( bytes.Copy(), index, false, out item1 );
        }

        public static int UnpackLE<T1, T2>( this byte[] bytes, int index, out T1 item1, out T2 item2 )
        {
            bytes = bytes.Copy();
            index += UnpackImpl( bytes, index, false, out item1 );
            index += UnpackImpl( bytes, index, false, out item2 );
            return index;
        }

        public static int UnpackLE<T1, T2, T3>( this byte[] bytes, int index, out T1 item1, out T2 item2,
           out T3 item3 )
        {
            bytes = bytes.Copy();
            index += UnpackImpl( bytes, index, false, out item1 );
            index += UnpackImpl( bytes, index, false, out item2 );
            index += UnpackImpl( bytes, index, false, out item3 );
            return index;
        }

        private static int UnpackImpl<T>( byte[] bytes, int index, bool reverse, out T item )
        {
            //TODO: check that buffer is wide enough
            //TODO: remove boxing of variables
            var type = typeof( T );
            if ( type == typeof( uint ) )
            {
                item = (T)(object)GetUInt32( bytes, index, reverse );
                return sizeof( uint );
            }
            if ( type == typeof( ushort ) )
            {
                item = (T)(object)GetUInt16( bytes, index, reverse );
                return sizeof( ushort );
            }
            if ( type == typeof( byte ) )
            {
                item = (T)(object)bytes[index];
                return sizeof( byte );
            }
            if ( type == typeof( UUID ) )
            {
                var uuid = new byte[UUID.SIZE];
                Array.Copy( bytes, index, uuid, 0, UUID.SIZE );
                item = (T)(object)new UUID( uuid );
                return UUID.SIZE;
            }
            throw new InvalidOperationException( "Cannot unpack type " + type.FullName );
        }

        private static uint GetUInt32( byte[] bytes, int index, bool reverse )
        {
            if ( reverse )
            {
                Array.Reverse( bytes, index, sizeof( uint ) );
            }
            return BitConverter.ToUInt32( bytes, index );
        }

        private static ushort GetUInt16( byte[] bytes, int index, bool reverse )
        {
            if ( reverse )
            {
                Array.Reverse( bytes, index, sizeof( ushort ) );
            }
            return BitConverter.ToUInt16( bytes, index );
        }

        private static byte[] Copy( this byte[] bytes )
        {
            var rv = new byte[bytes.Length];
            Array.Copy( bytes, rv, bytes.Length );
            return rv;
        }
    }
}