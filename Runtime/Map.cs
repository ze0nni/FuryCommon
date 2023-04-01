using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Fury
{
    [JsonConverter(typeof(MapJsonConverter))]
    public class Map<TKey, TValue>  : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] {
            get => _dict[key];
            set => _dict[key] = value;
        }

        public ICollection<TKey> Keys => _dict.Keys;
        public ICollection<TValue> Values => _dict.Values;

        public void Add(TKey key, TValue value) => _dict.Add(key, value);
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public bool Remove(TKey key) => _dict.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
    }
}