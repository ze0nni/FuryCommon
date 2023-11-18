using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fury
{
    public interface IArchetype<K>
    {
        K Type { get; }
    }

    public class Archetypes<T, K>
        : IEnumerable<T>
        where T : IArchetype<K>
    {
        readonly List<T> _list = new List<T>();
        readonly Dictionary<K, T> _byKind = new Dictionary<K, T>();
        readonly Dictionary<Type, T> _byType = new Dictionary<Type, T>();

        public Archetypes(IEnumerable<T> collection) => Fill(collection);
        public Archetypes(params T[] collection) => Fill(collection);

        private void Fill(IEnumerable<T> collection)
        {
            foreach (var a in collection)
            {
                _list.Add(a);
                _byKind.Add(a.Type, a);
                _byType.Add(a.GetType(), a);
            }
        }

        public T this[K kind] { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _byKind[kind]; }
        public T this[Type type] { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _byType[type]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R Resolve<R>() where R : T => (R)_byType[typeof(R)];

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        static Archetypes()
        {
            Asserts.RequestEquatable<K>("K", typeof(Archetypes<T, K>));
        }
    }
}