
// @Author : Arpan Jati
// @Date: 26th Aug 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Protocol
{
    public enum PDataType { PD_BYTE_VECTOR, PD_BYTE, PD_INT16, PD_INT32, PD_INT64, PD_FLOAT, PD_DOUBLE, PD_BOOL, PD_STRING, PD_VARINT };

    public struct ProtocolDataType
    {
        // Any Identifier based on protocol
        public byte NameType;
        // It can be INT, FLOAT, LONG, BYTE[], 
        public PDataType DataType;
        public byte[] Data;

        public ProtocolDataType(byte NameType, PDataType DataType, byte[] Data)
        {
            this.NameType = NameType;
            this.DataType = DataType;
            this.Data = Data;
        }
    };

    public class ProtocolPackager
    {
        static byte[] IntToBytes(int paramInt)
        {
            byte[] arrayOfByte = new byte[3];
            arrayOfByte[0] = (byte)(paramInt);
            arrayOfByte[1] = (byte)(paramInt >> 8);
            arrayOfByte[2] = (byte)(paramInt >> 16);
            return arrayOfByte;
        }

        static int BytesToInt(List<byte> paramInt)
        {
            int val = (int)paramInt[0];
            val |= ((int)paramInt[1]) << 8;
            val |= ((int)paramInt[2]) << 16;
            return val;
        }

        static ProtocolDataType GenericPack(PDataType DataType, byte nameType)
        {
            ProtocolDataType PDType = new ProtocolDataType();
            PDType.DataType = DataType;
            PDType.NameType = nameType;
            return PDType;
        }

        // Make Variable Size Encoding Decoding  

        static byte[] PackSingle(ProtocolDataType data)
        {
            List<byte> pack = new List<byte>();
            pack.Add(data.NameType);
            pack.Add((byte)data.DataType);
            byte[] vLen = Varint2.Encode(data.Data.Length);
            pack.AddRange(vLen);
            pack.AddRange(data.Data);
            return pack.ToArray();
        }

        public static byte[] PackRaw(List<ProtocolDataType> packets)
        {
            List<byte> pack = new List<byte>();
            foreach (ProtocolDataType packet in packets)
            {
                byte[] F = PackSingle(packet);
                pack.AddRange(F);
            }
            return pack.ToArray();
        }

        public static List<ProtocolDataType> UnPackRaw(byte[] packedData)
        {
            List<ProtocolDataType> packets = new List<ProtocolDataType>();

            int index = 0;
            while (true)
            {
                if (packedData.Length - index >= 3)
                {
                    byte NameType = packedData[index + 0];
                    byte DataType = packedData[index + 1];

                    int readDelta;
                    long Length = Varint2.Decode(packedData, index+2, out readDelta);
                    index += readDelta;

                    if (packedData.Length - index - Length >= 2)
                    {
                        byte[] Data = new byte[Length];
                        for (int i = 0; i < Length; i++)
                        {
                            Data[i] = packedData[index + 2 + i];
                        }

                        ProtocolDataType packet = new ProtocolDataType();
                        packet.NameType = NameType;
                        packet.DataType = (PDataType)DataType;
                        packet.Data = Data;
                        packets.Add(packet);
                    }
                    else
                    {
                        break;
                    }

                    index += (int)(Length + 2);
                }
                else
                {
                    break;
                }
            }

            return packets;
        }

        public static void TestPP()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();

            int count = 10000000;

            int[] datas = new int[count];

            Random r = new Random();

            for (int i = 0; i < count; i++)
            {
                int n = r.Next();

                datas[i] = n;
                PDTs.Add(ProtocolPackager.PackVarint(n, 0));
            }

            byte[] packed = ProtocolPackager.PackRaw(PDTs);

            List<ProtocolDataType> l = ProtocolPackager.UnPackRaw(packed);

            int fIndex = 0;

            foreach (ProtocolDataType p in l)
            {
                long R = 0;
                ProtocolPackager.UnpackVarint(p, 0, ref R);

                if(datas[fIndex] != R)
                {
                    Console.WriteLine("FAIL AT : " + fIndex + ", Value: " + R);
                }

                //Console.WriteLine("" + p.DataType.ToString() + " : " + p.NameType + " : " + R);


                fIndex++;
            }

            Console.WriteLine("\nVarint Test Complete ...");
        }

        public static List<ProtocolDataType> UnPackRaw_Old(byte[] packedData)
        {
            List<ProtocolDataType> packets = new List<ProtocolDataType>();

            int index = 0;
            while (true)
            {
                if (packedData.Length - index >= 5)
                {
                    List<byte> L_Bytes = new List<byte>();
                    byte NameType = packedData[index + 0];
                    byte DataType = packedData[index + 1];
                    L_Bytes.Add(packedData[index + 2]);
                    L_Bytes.Add(packedData[index + 3]);
                    L_Bytes.Add(packedData[index + 4]);

                    int Length = BytesToInt(L_Bytes);

                    if (packedData.Length - index - Length >= 5)
                    {
                        byte[] Data = new byte[Length];
                        for (int i = 0; i < Length; i++)
                        {
                            Data[i] = packedData[index + 5 + i];
                        }

                        ProtocolDataType packet = new ProtocolDataType();
                        packet.NameType = NameType;
                        packet.DataType = (PDataType)DataType;
                        packet.Data = Data;
                        packets.Add(packet);
                    }
                    else
                    {
                        break;
                    }

                    index += (Length + 5);
                }
                else
                {
                    break;
                }
            }

            return packets;
        }

        public static ProtocolDataType Pack(Hash hashValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_BYTE_VECTOR, nameType);
            PDType.Data = hashValue.Hex;
            return PDType;
        }


        public static ProtocolDataType Pack(byte[] vectorValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_BYTE_VECTOR, nameType);
            PDType.Data = vectorValue;
            return PDType;
        }

        public static ProtocolDataType Pack(byte byteValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_BYTE, nameType);
            byte[] F = new byte[1];
            F[0] = byteValue;

            PDType.Data = F;
            return PDType;
        }

        public static ProtocolDataType Pack(short shortValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_INT16, nameType);
            PDType.Data = Conversions.Int16ToVector(shortValue);
            return PDType;
        }

        public static ProtocolDataType Pack(int intValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_INT32, nameType);
            PDType.Data = Conversions.Int32ToVector(intValue);
            return PDType;
        }

        public static ProtocolDataType Pack(long longValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_INT64, nameType);
            PDType.Data = Conversions.Int64ToVector(longValue);
            return PDType;
        }

        public static ProtocolDataType Pack(float floatValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_FLOAT, nameType);
            PDType.Data = Conversions.FloatToVector(floatValue);
            return PDType;
        }

        public static ProtocolDataType Pack(double doubleValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_DOUBLE, nameType);
            PDType.Data = Conversions.DoubleToVector(doubleValue);
            return PDType;
        }

        public static ProtocolDataType PackVarint(long varintValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_VARINT, nameType);
            PDType.Data = Varint2.Encode(varintValue);
            return PDType;
        }

        public static ProtocolDataType Pack(bool boolValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_BOOL, nameType);
            byte[] F = new byte[1];
            F[0] = (byte)(boolValue ? 1 : 0);
            PDType.Data = F;
            return PDType;
        }

        public static ProtocolDataType Pack(string stringValue, byte nameType)
        {
            ProtocolDataType PDType = GenericPack(PDataType.PD_STRING, nameType);
            PDType.Data = Encoding.GetEncoding(28591).GetBytes(stringValue);
            return PDType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool UnpackHash(ProtocolDataType packedData, byte nameType, ref Hash Data)
        {
            if ((nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_BYTE_VECTOR))
            {
                Data.Hex = packedData.Data;
                return true;
            }
            else return false;
        }

        public static bool UnpackByteVector(ProtocolDataType packedData, byte nameType, ref byte[] Data)
        {
            if ((nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_BYTE_VECTOR))
            {
                Data = packedData.Data;
                return true;
            }
            else return false;
        }

        public static bool UnpackByteVector_s(ProtocolDataType packedData, byte nameType, int ExpectedSize, ref byte[] Data)
        {
            if (packedData.Data.Length == ExpectedSize && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_BYTE_VECTOR))
            {
                Data = packedData.Data;
                return true;
            }
            else return false;
        }

        public static bool UnpackByte(ProtocolDataType packedData, byte nameType, ref byte Data)
        {
            if ((packedData.Data.Length == 1) && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_BYTE))
            {
                Data = packedData.Data[0];
                return true;
            }
            else return false;
        }

        public static bool UnpackInt16(ProtocolDataType packedData, byte nameType, ref short Data)
        {
            if (packedData.Data.Length == 2 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_INT16))
            {
                Data = Conversions.VectorToInt16(packedData.Data);
                return true;
            }
            else return false;
        }

        public static bool UnpackInt32(ProtocolDataType packedData, byte nameType, ref int Data)
        {
            if (packedData.Data.Length == 4 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_INT32))
            {
                Data = Conversions.VectorToInt32(packedData.Data);
                return true;
            }
            else return false;
        }

        public static bool UnpackInt64(ProtocolDataType packedData, byte nameType, ref long Data)
        {
            if (packedData.Data.Length == 8 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_INT64))
            {
                Data = Conversions.VectorToInt64(packedData.Data);
                return true;
            }
            else return false;
        }

        public static bool UnpackFloat(ProtocolDataType packedData, byte nameType, ref float Data)
        {
            if (packedData.Data.Length == 4 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_FLOAT)) // Floats have 4 bytes
            {
                Data = Conversions.VectorToFloat(packedData.Data);
                return true;
            }
            else return false;
        }

        public static bool UnpackDouble(ProtocolDataType packedData, byte nameType, ref double Data)
        {
            if (packedData.Data.Length == 8 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_DOUBLE)) // Doubles have 8 bytes
            {
                Data = Conversions.VectorToDouble(packedData.Data);
                return true;
            }
            else return false;
        }

        public static bool UnpackVarint(ProtocolDataType packedData, byte nameType, ref long Data)
        {
            if (packedData.Data.Length > 0 && packedData.Data.Length <= 9 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_VARINT)) // PD_VARINT has 1-9 bytes
            {
                try
                {
                    Data = Varint2.Decode(packedData.Data);
                }
                catch { return false; }
                return true;
            }
            else return false;
        }

        public static bool UnpackBool(ProtocolDataType packedData, byte nameType, ref bool Data)
        {
            if (packedData.Data.Length == 1 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_BOOL))
            {
                Data = (packedData.Data[0] == 0) ? false : true;
                return true;
            }
            else return false;
        }

        public static bool UnpackString(ProtocolDataType packedData, byte nameType, ref string Data)
        {
            if (packedData.Data.Length >= 0 && (nameType == packedData.NameType) && (packedData.DataType == PDataType.PD_STRING))
            {
                Data = Encoding.GetEncoding(28591).GetString(packedData.Data);
                return true;
            }
            else return false;
        }

    }
}
