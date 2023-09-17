using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Fury
{
    public sealed class EntriesJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Entries<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(reader.TokenType == JsonToken.StartArray, "Excepted [");

            var entryType = objectType.GetGenericArguments()[0];
            var identityType = typeof(Identity<>).MakeGenericType(entryType);
            var addMethod = objectType.GetMethod("Add", new Type[] { identityType, entryType });

            var entrys = new List<object>();
            var ids = new List<object>();

            var args = new object[2];
            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
                var id = serializer.Deserialize(reader, identityType);
                reader.Read();
                var entry = serializer.Deserialize(reader, entryType);
                args[0] = id;
                args[1] = entry;
                addMethod.Invoke(existingValue, args);
            }

            return existingValue;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueType = value.GetType();
            var idsProp = valueType.GetProperty("Ids");

            var ids = ((IEnumerable)idsProp.GetValue(value)).GetEnumerator();
            var values = ((IEnumerable)value).GetEnumerator();
            writer.WriteStartArray();
            {
                while (ids.MoveNext() && values.MoveNext())
                {
                    serializer.Serialize(writer, ids.Current);
                    serializer.Serialize(writer, values.Current);
                }
            }
            writer.WriteEndArray();
        }
    }
}
