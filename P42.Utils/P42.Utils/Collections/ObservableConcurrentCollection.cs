using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;

namespace P42.Utils
{
    public class ObservableConcurrentCollection<T> : ObservableCollection<T>
    {
        protected object _lock = new object();

        protected override void ClearItems()
        {
            lock (_lock)
                base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            lock (_lock)
            {
                if (index > Count)
                    index = Count;
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (index >= Count)
                return;
            lock (_lock)
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            lock (_lock)
            {
                if (index > Count)
                    index = Count;
                base.SetItem(index, item);
            }
        }

        public T[] GetArray()
        {
            lock (_lock)
            {
                var result = new T[Count];
                CopyTo(result, 0);
                return result;
            }
        }

        public List<T> GetList()
        {
            lock (_lock)
            {
                return new List<T>(this);
            }
        }

        protected bool _editingRange;
        public virtual NotifyCollectionChangedEventArgs AddRange(IEnumerable<T> range)
        {
            if (!range?.Any() ?? true)
                return null;
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range);
            lock (_lock)
            {
                _editingRange = true;
                AddRangeInner(range);
                _editingRange = false;
            }
            OnCollectionChanged(args);
            return args;
        }

        void AddRangeInner(IEnumerable<T> range)
        {
            var count = Count;
            foreach (var item in range)
                base.InsertItem(count++, item);
        }

        public virtual NotifyCollectionChangedEventArgs RemoveRange(IEnumerable<T> range)
        {
            if (!range?.Any() ?? true)
                return null;
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, range);
            lock (_lock)
            {
                _editingRange = true;
                RemoveRangeInner(range);
                _editingRange = false;
            }
            OnCollectionChanged(args);
            return args;
        }

        void RemoveRangeInner(IEnumerable<T> range)
        {
            int count = Count;
            foreach (var item in range)
            {
                var index = IndexOf(item);
                if (index >= 0 && index < Count)
                {
                    base.RemoveItem(index);
                    count--;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_editingRange)
                base.OnCollectionChanged(e);
        }
    }
}