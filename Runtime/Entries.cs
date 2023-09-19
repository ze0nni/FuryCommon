using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

namespace Fury
{
    public enum InsertMode
    {
        /// <summary>
        /// Always inserts entries at the end of the list
        /// </summary>
        Tail,
        /// <summary>
        /// Inserts entries into empty cells
        /// </summary>
        Mixed
    }

    public enum JournalMode
    {
        /// <summary>
        /// Addition and removal entries immediate
        /// </summary>
        Never,
        /// <summary>
        /// Addition and removal entries when entries not readed
        /// </summary>
        OnRead,
        /// <summary>
        /// Manual call ApplyJournal() to apply changes
        /// </summary>
        Allways
    }

    [JsonConverter(typeof(EntriesJsonConverter))]
    public sealed partial class Entries<T>
        : IEnumerable<T>
        where T : class
    {
        long _version;
        public long Version => _version;

        readonly InsertMode _insertMode;
        readonly JournalMode _journalMode;
        readonly bool _isJournalled;

        public const int CursorsLimit = 64;

        readonly List<(Identity<T> Id, T Entry)> _list = new List<(Identity<T>, T)>();
        readonly Dictionary<Identity<T>, T> _dict = new Dictionary<Identity<T>, T>();

        readonly HashSet<Cursor> _cursors = new HashSet<Cursor>();
        readonly Stack<Cursor> _cursorsPool = new Stack<Cursor>();

        readonly List<int> _emptyCells = new List<int>();
        readonly Dictionary<Identity<T>, int> _indexCache = new Dictionary<Identity<T>, int>();
        readonly List<(Identity<T>, T Entry)> _journal = new List<(Identity<T>, T Entry)>();

        int _count;
        public int Count => _count;
        public int Capacity => _list.Capacity;
        public int EmptyCellsCount => _emptyCells.Count;
        public int JournalSize => _journal.Count;

        public Entries(InsertMode insertMode, JournalMode journalMode)
        {
            _insertMode = insertMode;
            _journalMode = journalMode;
            _isJournalled = journalMode == JournalMode.OnRead || _journalMode == JournalMode.Allways;
        }

        public T this[Identity<T> id] => _dict[id];
        public bool TryGet(Identity<T> id, out T entry) => _dict.TryGetValue(id, out entry);

        public Enumerator GetEnumerator() => new Enumerator(this, GetCursor());
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, GetCursor());
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this, GetCursor());

        public IEnumerable<Identity<T>> Ids => _list.Where(e => e.Entry != null).Select(e => e.Id);

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

            if (_cursors.Count == 0 && 
                (_journalMode == JournalMode.OnRead || _journalMode == JournalMode.Never))
            {
                ApplyJournal();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int FindIndexOf(Identity<T> id)
        {
            if (_isJournalled && _indexCache.TryGetValue(id, out var index))
            {
                return index;
            }
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
                if (_isJournalled)
                {
                    _indexCache[e.Id] = i;
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
            switch (_journalMode)
            {
                case JournalMode.Never:
                    InternalAdd(id, entry);
                    _version++;
                    break;

                case JournalMode.OnRead:
                    if (_cursors.Count == 0)
                    {
                        InternalAdd(id, entry);
                        _version++;
                    } else
                    {
                        _journal.Add((id, entry));
                    }
                    break;

                case JournalMode.Allways:
                    _journal.Add((id, entry));
                    break;
            }
        }

        public void Remove(Identity<T> id)
        {
            if (id == Identity<T>.Null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            switch (_journalMode)
            {
                case JournalMode.Never:
                    InternalRemove(id);
                    _version++;
                    break;

                case JournalMode.OnRead:
                    if (_cursors.Count == 0)
                    {
                        InternalRemove(id);
                        _version++;
                    } else
                    {
                        _journal.Add((id, null));
                    }
                    break;

                case JournalMode.Allways:
                    _journal.Add((id, null));
                    break;
            }
        }

        public void Clear()
        {
            if (_cursors.Count > 0)
            {
                throw new InvalidOperationException("Collection is reading");
            }

            _journal.Clear();
            if (_count == 0)
            {
                return;
            }

            _dict.Clear();
            if (_insertMode == InsertMode.Tail)
            {
                _list.Clear();
                _emptyCells.Clear();
            }
            else
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
            _count = 0;
            _version++;
        }

        readonly static string SampleNameApplyJournal = $"Entries<{typeof(T).Name}>.ApplyJournal()";
        readonly static string SampleNameInternallAdd = $"Entries<{typeof(T).Name}>.InternallAdd()";
        readonly static string SampleNameInternallRemove = $"Entries<{typeof(T).Name}>.InternallRemove()";

        public void ApplyJournal()
        {
            if (_cursors.Count > 0)
            {
                throw new InvalidOperationException("Collection is reading");
            }

            Profiler.BeginSample(SampleNameApplyJournal);

            if (_journal.Count > 0)
            {
                _version++;
            }
            foreach (var (id, entry) in _journal)
            {
                Profiler.BeginSample("Journal");
                if (entry != null)
                {
                    InternalAdd(id, entry);
                } else
                {
                    InternalRemove(id);
                }
                Profiler.EndSample();
            }
            _journal.Clear();

            if (_insertMode == InsertMode.Tail && _emptyCells.Count > 0)
            {
                Profiler.BeginSample("Remove empty cells");
                _emptyCells.Sort((a, b) => b - a);
                foreach (var i in _emptyCells)
                {
                    _list.RemoveAt(i);
                }
                _emptyCells.Clear();
                Profiler.EndSample();
            }

            Profiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool PopEmptyCell(out int index)
        {
            if (_emptyCells.Count == 0)
            {
                index = default;
                return false;
            }
            var lastIndex = _emptyCells.Count - 1;
            index = _emptyCells[lastIndex];
            _emptyCells.RemoveAt(lastIndex);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalAdd(Identity<T> id, T entry)
        {
            Profiler.BeginSample(SampleNameInternallAdd);
            if (_insertMode == InsertMode.Tail || _cursors.Count > 0)
            {
                InternalAddTail(id, entry);
            } else
            {
                InternalAddMixed(id, entry);
            }
            Profiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalAddMixed(Identity<T> id, T entry)
        {
            if (PopEmptyCell(out var index))
            {
                _dict.Add(id, entry);
                _list[index] = (id, entry);
                if (_isJournalled)
                {
                    _indexCache[id] = index;
                }
                _count++;
            } else
            {
                InternalAddTail(id, entry);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalAddTail(Identity<T> id, T entry)
        {
            _dict.Add(id, entry);
            _list.Add((id, entry));
            if (_isJournalled)
            {
                _indexCache[id] = _list.Count - 1;
            }
            _count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalRemove(Identity<T> id)
        {
            Profiler.BeginSample(SampleNameInternallRemove);
            if (_dict.Remove(id))
            {
                var index = FindIndexOf(id);
                if (_cursors.Count == 0 && _insertMode == InsertMode.Tail)
                {
                    _list.RemoveAt(index);
                    if (_isJournalled)
                    {
                        _indexCache.Clear();
                    }
                }
                else
                {
                    _list[index] = default;
                    _emptyCells.Add(index);
                    if (_isJournalled)
                    {
                        _indexCache.Remove(id);
                    }
                }
                _count--;
            }
            Profiler.EndSample();
        }
    }
}