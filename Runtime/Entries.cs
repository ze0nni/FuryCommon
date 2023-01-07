using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fury
{
    [Serializable]
    public sealed class Entries<T> :
        ISerializable,
        IEnumerable<T>,
        IEnumerable
        where T : class
    {
        internal readonly List<T> _list;
        internal readonly List<Identity<T>> _idList;
        internal readonly Dictionary<Identity<T>, T> _map;
        internal readonly HashSet<Identity<T>> _marked = new HashSet<Identity<T>>();

        public Entries()
        {
            _list = new List<T>();
            _idList = new List<Identity<T>>();
            _map = new Dictionary<Identity<T>, T>();
        }

        public void Add(Identity<T> id, T item) {
            _map.Add(id, item);
            _list.Add(item);
            _idList.Add(id);
        }

        public void Insert(int index, Identity<T>id, T item)
        {
            _map.Add(id, item);
            _list.Insert(index, item);
            _idList.Insert(index, id);
        }

        public void Mark(Identity<T> id)
        {
            _marked.Add(id);
        }

        public bool Contains(Identity<T> id)
        {
            return _map.ContainsKey(id);
        }

        public bool TryGet(Identity<T> id, out T item)
        {
            return _map.TryGetValue(id, out item);
        }

        public int Count => _list.Count;

        public T this[Identity<T> id] => _map[id];

        public T this[int index] => _list[index];

        public int IndexOf(T e) => _list.IndexOf(e);

        public int IndexOf(Identity<T> id) => _idList.IndexOf(id);

        public void RemoveMarked()
        {
            if (_marked.Count == 0)
            {
                return;
            }
            foreach (var id in _marked)
            {
                _map.Remove(id, out var item);
                _list.Remove(item);
                _idList.Remove(id);
            }
            _marked.Clear();
        }            

        public void Remove(Identity<T> id)
        {
            _map.Remove(id, out var item);
            _list.Remove(item);
            _idList.Remove(id);
        }

        public void RemoveAt(int index)
        {
            var id = _idList[index];
            _map.Remove(id);
            _list.RemoveAt(index);
            _idList.RemoveAt(index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _list.GetEnumerator();            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public Entries(SerializationInfo info, StreamingContext context)
        {
            _list = (List<T>)info.GetValue(nameof(_list), typeof(List<T>));
            _idList = (List<Identity<T>>)info.GetValue(nameof(_idList), typeof(List<Identity<T>>));
            _map = new Dictionary<Identity<T>, T>();
            for (var i = 0; i < _list.Count; i++)
            {
                _map.Add(_idList[i], _list[i]);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_list), _list);
            info.AddValue(nameof(_idList), _idList);
        }
    }
}
