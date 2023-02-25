using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Fury {
    public class IdentityJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(Identity<>);
        }

        private static Type[] _verConsturctorTypes = new Type[] { typeof(int) };
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = Convert.ToInt32(reader.Value);
            var newV = objectType.GetConstructor(_verConsturctorTypes);
            return newV.Invoke(new object[] { v });
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vField = value.GetType().GetField("Value");
            var v = (int)vField.GetValue(value);
            writer.WriteValue(v);
        }
    }
}
