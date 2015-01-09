
// @Author: Arpan Jati
// @Date: 9th Jan 2015

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Ref: http://stackoverflow.com/questions/11829035/newton-soft-json-jsonserializersettings-for-object-with-property-as-byte-array
// Modified to use HexUtil
// Write tested / read not yet tested

namespace TNetD.Json.REST
{
    public class BytesToHexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var hex = serializer.Deserialize<string>(reader);
                if (!string.IsNullOrEmpty(hex))
                {
                    return HexUtil.GetBytes(hex); /*Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();*/
                }
            }
            return Enumerable.Empty<byte>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bytes = value as byte[];
            var @string = BitConverter.ToString(bytes).Replace("-", string.Empty);
            serializer.Serialize(writer, @string);
        }
    }
}
