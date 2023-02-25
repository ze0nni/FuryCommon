using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Fury {
    class VerJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType 
                && objectType.GetGenericTypeDefinition() == typeof(Ver<>);
        }

        private static Type[] _verConsturctorTypes = new Type[] { typeof(int) };
        private static object[] _verConsturctorArgs = new object[1];
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = Convert.ToInt32(reader.Value);
            var newV = objectType.GetConstructor(_verConsturctorTypes);
            _verConsturctorArgs[0] = v;
            return newV.Invoke(_verConsturctorArgs);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vField = value.GetType().GetField("V");
            var v = (int)vField.GetValue(value);
            writer.WriteValue(v);
        }
    }
}
