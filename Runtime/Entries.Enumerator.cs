using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

namespace Fury
{
    public sealed partial class Entries<T>
    {
        internal sealed class Cursor
        {
            public Identity<T> Id;
            public int Index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                Id = Identity<T>.Null;
                Index = -1;
            }
        }

        readonly static string SampleNameForeach = $"Entries<{typeof(T).Name}> foreach";

        public struct Enumerator : IEnumerator<T>
        {
            readonly Entries<T> _entries;
            readonly Cursor _cursor;
            bool _disposed;

            T _current;
            int _index;

            internal Enumerator(Entries<T> entries, Cursor cursor)
            {
                _entries = entries;
                _cursor = cursor;
                _disposed = false;
                _current = null;
                _index = -1;
            }

            public T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }
            object IEnumerator.Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current; }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                _entries.ReleaseCursor(_cursor);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var list = _entries._list;
                var count = list.Count;
                while (true)
                {
                    if (_index == -1)
                    {
                        Profiler.BeginSample(SampleNameForeach);
                    }
                    _index++;
                    if (_index >= count)
                    {
                        _current = null;
                        _cursor.Reset();
                        Profiler.EndSample();
                        return false;
                    }
                    var e = list[_index];
                    if (e.Entry == null)
                    {
                        continue;
                    }
                    _current = e.Entry;
                    _cursor.Id = e.Id;
                    _cursor.Index = _index;
                    return true;
                }
            }

            public void Reset()
            {
                _index = -1;
                _cursor.Reset();
            }
        }
    }
}