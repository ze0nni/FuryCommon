using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fury
{
    class MapJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Map<,>);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(reader.TokenType == JsonToken.StartArray, "Excepted [");

            var addMethod = objectType.GetMethod("Add");
            var keyType = objectType.GetGenericArguments()[0];
            var valueType = objectType.GetGenericArguments()[1];

            var args = new object[2];
            while (true)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
                var key = serializer.Deserialize(reader, keyType);
                reader.Read();
                var value = serializer.Deserialize(reader, valueType);
                args[0] = key;
                args[1] = value;
                addMethod.Invoke(existingValue, args);
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var mapType = value.GetType();
            var keysProp = mapType.GetProperty("Keys");
            var keys = ((IEnumerable)keysProp.GetValue(value)).GetEnumerator();
            var valuesProp = mapType.GetProperty("Values");
            var values = ((IEnumerable)valuesProp.GetValue(value)).GetEnumerator();
            writer.WriteStartArray();

            while (keys.MoveNext() && values.MoveNext())
            {
                serializer.Serialize(writer, keys.Current);
                serializer.Serialize(writer, values.Current);
            }
            keys.Reset();
            values.Reset();

            writer.WriteEndArray();
        }
    }
}