using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;

namespace Fury
{
    [Serializable]
    public sealed class Components : ISerializable, IEnumerable
    {
        readonly Dictionary<Type, object> _map;

        public T Get<T>() where T : class, new()
        {
            if (!_map.TryGetValue(typeof(T), out var result))
            {
                result = new T();
                _map.Add(typeof(T), result);
            }
            return (T)result;
        }

        public bool TryGet<T>(out T result)
        {
            if (!_map.TryGetValue(typeof(T), out var r))
            {
                result = default;
                return false;
            }
            result = (T)r;
            return true;
        }

        public void Set<T>(T component) where T : class
        {
            _map[typeof(T)] = component;
        }

        public void Drop<T>() where T : class
        {
            _map.Remove(typeof(T));
        }

        public Components()
        {
            _map = new Dictionary<Type, object>();
        }

        public Components(SerializationInfo info, StreamingContext context)
        {
            _map = new Dictionary<Type, object>();

            var types = info.GetValue("types", typeof(string[])) as string[];
            foreach (var typeName in types)
            {
                var type = Type.GetType(typeName, false);
                if (type != null)
                {
                    _map[type] = info.GetValue(typeName, type);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var types = _map.Keys.Select(x => x.FullName).ToArray();
            info.AddValue("types", types);
            foreach (var (type, value) in _map)
            {
                info.AddValue(type.FullName, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.Values.GetEnumerator();
        }
    }
}
