using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace MSFBlitzBot
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public sealed class AsyncObservableCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection, INotifyCollectionChanged, INotifyPropertyChanged, ISerializable
    {
        private sealed class ThreadView
        {
            public readonly List<EventArgs> waitingEvents = new();

            public bool dissalowReenterancy;

            private readonly int _threadId;

            private readonly AsyncObservableCollection<T> _owner;

            private readonly WeakReference<List<T>> _snapshot;

            private int _listVersion;

            private int _snapshotId;

            private int _enumeratingCurrentSnapshot;

            public ThreadView(AsyncObservableCollection<T> owner)
            {
                _owner = owner;
                _threadId = Thread.CurrentThread.ManagedThreadId;
                _snapshot = new WeakReference<List<T>>(null);
            }

            public List<T> getSnapshot()
            {
                if (!_snapshot.TryGetTarget(out var target) || _listVersion != _owner._version)
                {
                    int enumeratingCurrentSnapshot = _enumeratingCurrentSnapshot;
                    _snapshotId++;
                    _enumeratingCurrentSnapshot = 0;
                    _owner._lock.EnterReadLock();
                    try
                    {
                        _listVersion = _owner._version;
                        if (target == null || enumeratingCurrentSnapshot > 0)
                        {
                            target = new List<T>(_owner._collection);
                            _snapshot.SetTarget(target);
                            return target;
                        }
                        target.Clear();
                        target.AddRange(_owner._collection);
                        return target;
                    }
                    finally
                    {
                        _owner._lock.ExitReadLock();
                    }
                }
                return target;
            }

            public int enterEnumerator()
            {
                _enumeratingCurrentSnapshot++;
                return _snapshotId;
            }

            public void exitEnumerator(int oldId)
            {
                if (Thread.CurrentThread.ManagedThreadId == _threadId && _snapshotId == oldId)
                {
                    _enumeratingCurrentSnapshot--;
                }
            }
        }

        private sealed class EnumeratorImpl : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly ThreadView _view;

            private readonly int _myId;

            private List<T>.Enumerator _enumerator;

            private bool _isDisposed;

            object IEnumerator.Current => Current;

            public T Current
            {
                get
                {
                    if (_isDisposed)
                    {
                        throwDisposedException();
                    }
                    return _enumerator.Current;
                }
            }

            public EnumeratorImpl(List<T> list, ThreadView view)
            {
                _enumerator = list.GetEnumerator();
                _view = view;
                _myId = view.enterEnumerator();
            }

            public bool MoveNext()
            {
                if (_isDisposed)
                {
                    throwDisposedException();
                }
                return _enumerator.MoveNext();
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _enumerator.Dispose();
                    _isDisposed = true;
                    _view.exitEnumerator(_myId);
                }
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException("This enumerator doesn't support Reset()");
            }

            private static void throwDisposedException()
            {
                throw new ObjectDisposedException("The enumerator was disposed");
            }
        }

        private readonly ObservableCollection<T> _collection;

        private readonly ThreadLocal<ThreadView> _threadView;

        private readonly ReaderWriterLockSlim _lock;

        private volatile int _version;

        private readonly AsyncDispatcherEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs> _collectionChanged = new();

        private readonly AsyncDispatcherEvent<PropertyChangedEventHandler, PropertyChangedEventArgs> _propertyChanged = new();

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _collection.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _collection[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                ThreadView value2 = _threadView.Value;
                if (value2.dissalowReenterancy)
                {
                    throwReenterancyException();
                }
                _lock.EnterWriteLock();
                try
                {
                    _version++;
                    _collection[index] = value;
                }
                catch (Exception)
                {
                    value2.waitingEvents.Clear();
                    throw;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                dispatchWaitingEvents(value2);
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        object ICollection.SyncRoot => throw new NotSupportedException("AsyncObservableCollection doesn't need external synchronization");

        bool ICollection.IsSynchronized => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                if (value == null)
                {
                    return;
                }
                _lock.EnterWriteLock();
                try
                {
                    if (_collectionChanged.IsEmpty)
                    {
                        _collection.CollectionChanged += new NotifyCollectionChangedEventHandler(onCollectionChangedInternal);
                    }
                    _collectionChanged.add(value);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            remove
            {
                if (value == null)
                {
                    return;
                }
                _lock.EnterWriteLock();
                try
                {
                    _collectionChanged.remove(value);
                    if (_collectionChanged.IsEmpty)
                    {
                        _collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(onCollectionChangedInternal);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                if (value == null)
                {
                    return;
                }
                _lock.EnterWriteLock();
                try
                {
                    if (_propertyChanged.IsEmpty)
                    {
                        ((INotifyPropertyChanged)_collection).PropertyChanged += new PropertyChangedEventHandler(onPropertyChangedInternal);
                    }
                    _propertyChanged.add(value);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            remove
            {
                if (value == null)
                {
                    return;
                }
                _lock.EnterWriteLock();
                try
                {
                    _propertyChanged.remove(value);
                    if (_propertyChanged.IsEmpty)
                    {
                        ((INotifyPropertyChanged)_collection).PropertyChanged -= new PropertyChangedEventHandler(onPropertyChangedInternal);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public AsyncObservableCollection()
        {
            _collection = new ObservableCollection<T>();
            _lock = new ReaderWriterLockSlim();
            _threadView = new ThreadLocal<ThreadView>(() => new ThreadView(this));
        }

        public AsyncObservableCollection(IEnumerable<T> collection)
        {
            _collection = new ObservableCollection<T>(collection);
            _lock = new ReaderWriterLockSlim();
            _threadView = new ThreadLocal<ThreadView>(() => new ThreadView(this));
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _collection.IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Add(T item)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                _collection.Add(item);
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public void AddRange(IEnumerable<T> items)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                foreach (T item in items)
                {
                    _collection.Add(item);
                }
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        int IList.Add(object value)
        {
            ThreadView value2 = _threadView.Value;
            if (value2.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            int result;
            try
            {
                _version++;
                result = ((IList)_collection).Add(value);
            }
            catch (Exception)
            {
                value2.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value2);
            return result;
        }

        public void Insert(int index, T item)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                _collection.Insert(index, item);
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                foreach (T item in items)
                {
                    _collection.Insert(index, item);
                }
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public bool Remove(T item)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            bool result;
            try
            {
                _version++;
                result = _collection.Remove(item);
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
            return result;
        }

        public void RemoveAt(int index)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                _collection.RemoveAt(index);
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public void Clear()
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                _collection.Clear();
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public void Move(int oldIndex, int newIndex)
        {
            ThreadView value = _threadView.Value;
            if (value.dissalowReenterancy)
            {
                throwReenterancyException();
            }
            _lock.EnterWriteLock();
            try
            {
                _version++;
                _collection.Move(oldIndex, newIndex);
            }
            catch (Exception)
            {
                value.waitingEvents.Clear();
                throw;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            dispatchWaitingEvents(value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            ThreadView value = _threadView.Value;
            return new EnumeratorImpl(value.getSnapshot(), value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _threadView.Value.getSnapshot().CopyTo(array, arrayIndex);
        }

        public T[] ToArray()
        {
            return _threadView.Value.getSnapshot().ToArray();
        }

        private void onCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs args)
        {
            _threadView.Value.waitingEvents.Add(args);
        }

        private void onPropertyChangedInternal(object sender, PropertyChangedEventArgs args)
        {
            _threadView.Value.waitingEvents.Add(args);
        }

        private void dispatchWaitingEvents(ThreadView view)
        {
            List<EventArgs> waitingEvents = view.waitingEvents;
            try
            {
                if (waitingEvents.Count == 0)
                {
                    return;
                }
                if (view.dissalowReenterancy)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    throwReenterancyException();
                }
                view.dissalowReenterancy = true;
                foreach (EventArgs item in waitingEvents)
                {
                    if (item is NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
                    {
                        _collectionChanged.raise(this, notifyCollectionChangedEventArgs);
                        continue;
                    }
                    if (item is PropertyChangedEventArgs propertyChangedEventArgs)
                    {
                        _propertyChanged.raise(this, propertyChangedEventArgs);
                    }
                }
            }
            finally
            {
                view.dissalowReenterancy = false;
                waitingEvents.Clear();
            }
        }

        private static void throwReenterancyException()
        {
            throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed -- don't modify the collection during callbacks from it!");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        private AsyncObservableCollection(SerializationInfo info, StreamingContext context)
            : this((T[])info.GetValue("values", typeof(T[])))
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("values", ToArray(), typeof(T[]));
        }
    }
}
