using System;
using System.Collections.Generic;

namespace Fury.Legacy
{
    [Obsolete]
    public sealed class EntriesView<TEntry, TView> where TEntry : class
    {
        public delegate TView AddView(Identity<TEntry> id, TEntry model);
        public delegate void RemoveView(Identity<TEntry> id, TView view);
        public delegate void UpdateView(TEntry model, TView view);

        readonly AddView _add;
        readonly RemoveView _remove;
        readonly UpdateView _update;
        readonly Dictionary<Identity<TEntry>, TView> _views = new Dictionary<Identity<TEntry>, TView>();
        readonly HashSet<Identity<TEntry>> _marked = new HashSet<Identity<TEntry>>();

        public EntriesView(
            AddView add,
            RemoveView remove,
            UpdateView update)
        {
            _add = add;
            _remove = remove;
            _update = update;
        }
       
        public void Update(Entries<TEntry> source)
        {
            foreach (var sourceId in source._idList)
            {
                if (_views.ContainsKey(sourceId))
                {
                    continue;
                }
                var view = _add.Invoke(sourceId, source._map[sourceId]);
                _views.Add(sourceId, view);
            }

            _marked.Clear();
            foreach (var viewId in _views.Keys)
            {
                if (source._map.ContainsKey(viewId))
                {
                    continue;
                }
                _marked.Add(viewId);
            }
            foreach (var id in _marked)
            {
                _views.Remove(id, out var view);
                _remove(id, view);
            }

            foreach (var id in source._idList)
            {
                _update(source._map[id], _views[id]);
            }
        }
    }
}