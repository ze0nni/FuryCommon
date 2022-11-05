using System;
using System.Collections;
using System.Collections.Generic;

namespace Fury
{
    public struct Slot<T> : IEnumerable<T> where T : Delegate
    {
        internal List<T> _actions;

        public IEnumerator<T> GetEnumerator()
        {
            if (_actions == null)
            {
                yield break;
            }

            foreach (var a in _actions)
            {
                yield return a;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public static class SlotTools
    {
        public static void Connect<T>(this ref Slot<T> slot, T action) where T : Delegate
        {
            if (slot._actions == null)
            {
                slot._actions = new List<T>();
            }
            slot._actions.Add(action);
        }
    }
}
