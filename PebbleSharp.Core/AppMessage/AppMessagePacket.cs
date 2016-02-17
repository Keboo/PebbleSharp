using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PebbleSharp.Core.Responses;

namespace PebbleSharp.Core.NonPortable.AppMessage
{
    //modeled after https://github.com/pebble/libpebble2/blob/master/libpebble2/services/appmessage.py
	[Endpoint(Endpoint.ApplicationMessage)]
	public class AppMessagePacket: ResponseBase
    {
        //TODO: the key names are in the manifest.... do we need them for anything?

        //byte layout is as follows
        //command (always 1?) byte
        //transactionid       byte
        //uuid                byte[16]
        //tuplecount          byte
        //tuple               
        //  k                 uint      key
        //  t                 byte      type 
        //  l                 ushort    length

        //1:250:34:162:123:154:11:7:71:175:173:135:178:194:147:5:186:182:1:0:0:0:0:2:1:0:1
        //sample message
        //1:        command
        //250:      transaction id
        //34:162:123:154:11:7:71:175:173:135:178:194:147:5:186:182: //uuid
        //1:    //tuple count
        //0:0:0:0: //first tuple, key
        //2:        //type
        //1:0:      //length
        //1         //value

        //sample outbound message
        //1:
        //0:
        //11:7:71:175:173:135:178:194:147:5:186:182:255:255:255:255:
        //1:
        //0:0:0:0:
        //2:
        //4:0:
        //87:195:209:30:

        public byte Command { get; set; }

        public byte TransactionId { get; set; }
        public UUID ApplicationId { get; set; }

        public AppMessagePacket()
        {
            Values = new List<IAppMessageDictionaryEntry>();
        }



        public AppMessagePacket(byte[] bytes)
        {
			Load(bytes);
        }

		protected override void Load(byte[] bytes)
		{
			Values = new List<IAppMessageDictionaryEntry>();
			int index = 0;
			var command = bytes[index];
			index++;

			Command = command;
			//if (command == Command)
			{
				TransactionId = bytes[index];
				index++;

				ApplicationId = new UUID(bytes.Skip(index).Take(16).ToArray());
				index += 16;

				int tupleCount = bytes[index];
				index++;

				for (int i = 0; i < tupleCount; i++)
				{
					uint k;
					byte t;
					ushort l;

					k = BitConverter.ToUInt32(bytes, index);
					index += 4;

					t = bytes[index];
					index++;

					l = BitConverter.ToUInt16(bytes, index);
					index += 2;

					IAppMessageDictionaryEntry entry = null;
					if (t == (byte)PackedType.Bytes)
					{
						entry = new AppMessageBytes() { Value = bytes.Skip(index).Take(l).ToArray() };
					}
					else if (t == (byte)PackedType.Signed)
					{
						if (l == 1)
						{
							entry = new AppMessageInt8() { Value = Convert.ToSByte(bytes[index]) };
						}
						else if (l == 2)
						{
							entry = new AppMessageInt16() { Value = BitConverter.ToInt16(bytes, index) };
						}
						else if (l == 4)
						{
							entry = new AppMessageInt32() { Value = BitConverter.ToInt32(bytes, index) };
						}
						else
						{
							throw new InvalidOperationException("Invalid signed integer length");
						}
					}
					else if (t == (byte)PackedType.String)
					{
						entry = new AppMessageString() { Value = System.Text.Encoding.UTF8.GetString(bytes, index, l) };
					}
					else if (t == (byte)PackedType.Unsigned)
					{
						if (l == 1)
						{
							entry = new AppMessageUInt8() { Value = bytes[index] };
						}
						else if (l == 2)
						{
							entry = new AppMessageUInt16() { Value = BitConverter.ToUInt16(bytes, index) };
						}
						else if (l == 4)
						{
							entry = new AppMessageUInt32() { Value = BitConverter.ToUInt32(bytes, index) };
						}
						else
						{
							throw new InvalidOperationException("Invalid signed integer length");
						}
					}
					else
					{
						throw new InvalidOperationException("Unknown tuple type");
					}
					index += l;
					entry.Key = k;
					Values.Add(entry);
				}
			}
		}

        public IList<IAppMessageDictionaryEntry> Values { get; set; }

        public byte[] GetBytes()
        {
            if (Values != null && Values.Any())
            {
                var bytes = new List<byte>();
                bytes.Add(Command);
                bytes.Add(TransactionId);
                bytes.AddRange(ApplicationId.Data);
                bytes.Add((byte)Values.Count);
                foreach (var tuple in Values)
                {
                    bytes.AddRange(tuple.PackedBytes);
                }
                return bytes.ToArray();
            }
            else
            {
                return new byte[0];
            }
        }
    }

    public interface IAppMessageDictionaryEntry
    {
        uint Key { get; set; }
        PackedType PackedType { get; }
        ushort Length { get; }
        byte[] ValueBytes { get; set; }
        byte[] PackedBytes { get; }
    }

    public enum Command:byte
    {
        Push=1
    }

    public enum PackedType:byte
    {
        Bytes=0,
        String =1,
        Unsigned =2,
        Signed = 3
    }

    public abstract class AppMessageDictionaryEntry<T> : IAppMessageDictionaryEntry
    {
        public uint Key { get; set; }
        public abstract PackedType PackedType { get; }
        public abstract ushort Length { get; }

        public virtual T Value { get; set; }
        public abstract byte[] ValueBytes { get; set; }

        public byte[] PackedBytes
        {
            get
            {
                var bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(Key));
                bytes.Add((byte)PackedType);
                bytes.AddRange(BitConverter.GetBytes(Length));
                bytes.AddRange(ValueBytes);
                return bytes.ToArray();
            }
        }
    }

    public class AppMessageUInt8 : AppMessageDictionaryEntry<byte>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Unsigned; }
        }

        public override ushort Length
        {
            get { return sizeof(byte); }
        }

        public override byte[] ValueBytes
        {
            get { return new byte[] {Value}; }
            set
            {
                if (value.Length == Length)
                {
                    Value = value[0];
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageUInt16 : AppMessageDictionaryEntry<UInt16>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Unsigned; }
        }

        public override ushort Length
        {
            get { return sizeof(UInt16); }
        }

        public override byte[] ValueBytes
        {
            get { return BitConverter.GetBytes(Value); }
            set
            {
                if (value.Length == Length)
                {
                    Value = BitConverter.ToUInt16(value,0);
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageUInt32 : AppMessageDictionaryEntry<UInt32>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Unsigned; }
        }

        public override ushort Length
        {
            get { return sizeof(UInt32); }
        }

        public override byte[] ValueBytes
        {
            get { return BitConverter.GetBytes(Value); }
            set
            {
                if (value.Length == Length)
                {
                    Value = BitConverter.ToUInt32(value, 0);
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageInt8 : AppMessageDictionaryEntry<sbyte>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Signed; }
        }

        public override ushort Length
        {
            get { return sizeof(sbyte); }
        }

        public override byte[] ValueBytes
        {
            get { return new byte[] { Convert.ToByte(Value) }; }
            set
            {
                if (value.Length == Length)
                {
                    Value = Convert.ToSByte(value);
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageInt16 : AppMessageDictionaryEntry<Int16>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Signed; }
        }

        public override ushort Length
        {
            get { return sizeof(Int16); }
        }

        public override byte[] ValueBytes
        {
            get { return BitConverter.GetBytes(Value); }
            set
            {
                if (value.Length == Length)
                {
                    Value = BitConverter.ToInt16(value, 0);
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageInt32 : AppMessageDictionaryEntry<Int32>
    {
        public override PackedType PackedType
        {
            get { return PackedType.Signed; }
        }

        public override ushort Length
        {
            get { return sizeof(Int32); }
        }

        public override byte[] ValueBytes
        {
            get { return BitConverter.GetBytes(Value); }
            set
            {
                if (value.Length == Length)
                {
                    Value = BitConverter.ToInt32(value, 0);
                }
                else
                {
                    throw new InvalidOperationException("Incorrect # of bytes");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageString : AppMessageDictionaryEntry<string>
    {
        public override PackedType PackedType
        {
            get { return PackedType.String; }
        }

        public override ushort Length
        {
            get { return (ushort)ValueBytes.Length; }
        }

        public override string Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if(value!=null && value.EndsWith("\0"))
                {
                    base.Value = value.Substring(0, value.Length - 1);
                }
                else
                {
                    base.Value = value;
                }
            }
        }

        public override byte[] ValueBytes
        {
            get { return System.Text.UTF8Encoding.UTF8.GetBytes(Value+"\0"); }
            set
            {
                if (value.Length <= ushort.MaxValue)
                {
                    Value = System.Text.UTF8Encoding.UTF8.GetString(value);
                }
                else
                {
                    throw new OverflowException("Specified string is too large for length to fit in a ushort");
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class AppMessageBytes : AppMessageDictionaryEntry<byte[]>
    {
        public override PackedType PackedType
        {
			get { return PackedType.Bytes; }
        }

        public override ushort Length
        {
            get { return (ushort)Value.Length; }
        }

        public override byte[] ValueBytes
        {
            get { return Value; }
            set
            {
                if (value.Length <= ushort.MaxValue)
                {
                    Value = value;
                }
                else
                {
                    throw new OverflowException("Specified array is too large for length to fit in a ushort");
                }
            }
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            foreach(var b in Value)
            {
                s.Append(b.ToString());
                s.Append(",");
            }
            return s.ToString();
        }
    }
}
