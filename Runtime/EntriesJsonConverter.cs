using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace Fury
{
    class EntriesJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Entries<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(reader.TokenType == JsonToken.StartArray, "Excepted [");
            reader.Read();
            
            var entryType = objectType.GetGenericArguments()[0];
            var identityType = typeof(Identity<>).MakeGenericType(entryType);
            var addMethod = objectType.GetMethod("Add", new Type[] { identityType, entryType });

            var entrys = new List<object>();
            var ids = new List<object>();

            Contract.Assert(reader.TokenType == JsonToken.StartArray, "Excepted [");
            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                {
                    reader.Read();
                    break;
                }
                var id = serializer.Deserialize(reader, identityType);
                ids.Add(id);
            }

            Contract.Assert(reader.TokenType == JsonToken.StartArray, "Excepted [");
            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                {
                    reader.Read();
                    break;
                }
                var entry = serializer.Deserialize(reader, entryType);
                entrys.Add(entry);
            }

            var args = new object[2];
            for (var i = 0; i < ids.Count; i++)
            {
                args[0] = ids[i];
                args[1] = entrys[i];
                addMethod.Invoke(existingValue, args);
            }

            return existingValue;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueType = value.GetType();
            var idsProp = valueType.GetProperty("Ids");

            writer.WriteStartArray();
            {
                {
                    writer.WriteStartArray();
                    var ids = (IEnumerable)idsProp.GetValue(value);
                    foreach (var id in ids)
                    {
                        serializer.Serialize(writer, id);
                    }
                    writer.WriteEndArray();
                }

                {
                    writer.WriteStartArray();
                    foreach (var e in (IEnumerable)value)
                    {
                        serializer.Serialize(writer, e);
                    }
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndArray();
        }
    }
}