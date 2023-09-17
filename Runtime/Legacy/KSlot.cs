using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Fury.Legacy
{
    [Obsolete]
    public struct KSlot<K, T> where T : Delegate
    {
        internal Dictionary<K, List<T>> _actions;

        public IEnumerable<T> Of(K key)
        {
            if (_actions == null || !_actions.TryGetValue(key, out var list))
            {
                yield break;
            }

            foreach (var a in list)
            {
                yield return a;
            }
        }

        public T Single(K key)
        {
            var list = _actions[key];
            Contract.Assert(list.Count == 1);
            return list[0];
        }
    }

    public static class KSlotTools
    {
        public static void Connect<K, T>(this ref KSlot<K, T> slot, K key, T action) where T : Delegate
        {
            if (slot._actions == null)
            {
                slot._actions = new Dictionary<K, List<T>>();
            }
            if (!slot._actions.TryGetValue(key, out var list))
            {
                list = new List<T>();
                slot._actions.Add(key, list);
            }
            list.Add(action);
        }
    }
}
