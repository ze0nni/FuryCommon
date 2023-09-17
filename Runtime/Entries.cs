using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fury
{
    [JsonConverter(typeof(EntriesJsonConverter))]
    public sealed partial class Entries<T>
        : IEnumerable<T>
        where T : class
    {
        long _version;
        public long Version => _version;

        public const int CursorsLimit = 64;

        readonly List<(Identity<T> Id, T Entry)> _list = new List<(Identity<T>, T)>();
        readonly Dictionary<Identity<T>, T> _dict = new Dictionary<Identity<T>, T>();

        readonly HashSet<Cursor> _cursors = new HashSet<Cursor>();
        readonly Stack<Cursor> _cursorsPool = new Stack<Cursor>();

        readonly SortedSet<int> _emptyCells = new SortedSet<int>(new IntComparer());
        sealed class IntComparer : IComparer<int>
        {
            public int Compare(int x, int y) => y - x;
        }

        public T this[Identity<T> id] => _dict[id];
        public bool TryGet(Identity<T> id, out T entry) => _dict.TryGetValue(id, out entry);

        public Enumerator GetEnumerator() => new Enumerator(this, GetCursor());
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, GetCursor());
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this, GetCursor());

        public IEnumerable<Identity<T>> Ids => _dict.Keys;

        Cursor GetCursor()
        {
            if (_cursors.Count >= CursorsLimit)
            {
                throw new Exception("To many nested enumerators or you forget call IEnumerator.Dispose()");
            }
            if (!_cursorsPool.TryPop(out var cursor))
            {
                cursor = new Cursor();
            }
            cursor.Reset();
            _cursors.Add(cursor);
            return cursor;
        }

        void ReleaseCursor(Cursor cursor)
        {
            if (!_cursors.Remove(cursor))
            {
                return;
            }
            _cursorsPool.Push(cursor);

            if (_cursors.Count == 0 && _emptyCells.Count > 0)
            {
                RemoveEmptyCells();
            }
        }

        void RemoveEmptyCells()
        {
            foreach (var index in _emptyCells)
            {
                _list.RemoveAt(index);
            }
            _emptyCells.Clear();
        }

        int FindIndexOf(Identity<T> id)
        {
            foreach (var cursor in _cursors)
            {
                if (cursor.Id == id && _list[cursor.Index].Id == id)
                {
                    return cursor.Index;
                }
            }
            for (var i = 0; i < _list.Count; i++)
            {
                var e = _list[i];
                if (e.Id == id)
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException(id.ToString());
        }

        public void Add(Identity<T> id, T entry)
        {
            if (id == Identity<T>.Null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            _dict.Add(id, entry);
            _list.Add((id, entry));

            _version++;
        }

        public bool Remove(Identity<T> id)
        {
            if (!_dict.Remove(id))
            {
                return false;
            }
            var index = FindIndexOf(id);
            if (_cursors.Count == 0)
            {
                _list.RemoveAt(index);
            } else
            {
                _list[index] = default;
                _emptyCells.Add(index);
            }

            _version++;
            return true;
        }

        public void Clear()
        {
            _dict.Clear();
            if (_cursors.Count == 0)
            {
                _list.Clear();
            } else
            {
                for (var i = 0; i < _list.Count; i++)
                {
                    var e = _list[i];
                    if (e.Entry != null)
                    {
                        _list[i] = default;
                        _emptyCells.Add(i);
                    }
                }
            }
            _version++;
        }
    }
}