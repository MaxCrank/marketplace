using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Marketplace.Core.DataTypes.Collections
{
    /// <summary>
    /// The concurrent list.
    /// Credits go to Ronnie Overby (taken from from https://gist.github.com/ronnieoverby/11c8b6b067872db719d7)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    /// <seealso cref="System.IDisposable" />
    public class ConcurrentList<T> : IList<T>, IDisposable
    {
        #region Fields

        /// <summary>
        /// The lock
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The count
        /// </summary>
        private int _count = 0;

        /// <summary>
        /// The array
        /// </summary>
        private T[] _arr;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the length of the internal array.
        /// </summary>
        /// <value>
        /// The length of the internal array.
        /// </value>
        public int InternalArrayLength
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _arr.Length;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity.</param>
        public ConcurrentList(int initialCapacity)
        {
            _arr = new T[initialCapacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        public ConcurrentList() : this(4)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ConcurrentList(IEnumerable<T> items)
        {
            _arr = items.ToArray();
            _count = _arr.Length;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                var newCount = _count + 1;
                EnsureCapacity(newCount);
                _arr[_count] = item;
                _count = newCount;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <exception cref="ArgumentNullException">items</exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            _lock.EnterWriteLock();

            try
            {
                var arr = items as T[] ?? items.ToArray();
                var newCount = _count + arr.Length;
                EnsureCapacity(newCount);
                Array.Copy(arr, 0, _arr, _count, arr.Length);
                _count = newCount;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> was successfully removed from the <see cref="System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original <see cref="System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        public bool Remove(T item)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                var i = IndexOfInternal(item);

                if (i == -1)
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(i);
                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();

            try
            {
                for (int i = 0; i < _count; i++)
                    // deadlocking potential mitigated by lock recursion enforcement
                    yield return _arr[i];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return IndexOfInternal(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }



        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="ArgumentOutOfRangeException">index</exception>
        public void Insert(int index, T item)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                if (index > _count)
                    throw new ArgumentOutOfRangeException("index");

                _lock.EnterWriteLock();
                try
                {
                    var newCount = _count + 1;
                    EnsureCapacity(newCount);

                    // shift everything right by one, starting at index
                    Array.Copy(_arr, index, _arr, index + 1, _count - index);

                    // insert
                    _arr[index] = item;
                    _count = newCount;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index</exception>
        public void RemoveAt(int index)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (index >= _count)
                    throw new ArgumentOutOfRangeException("index");

                _lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(index);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }


        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                Array.Clear(_arr, 0, _count);
                _count = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in the <see cref="System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return IndexOfInternal(item) != -1;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentException">Destination array was not long enough.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                if (_count > array.Length - arrayIndex)
                    throw new ArgumentException("Destination array was not long enough.");

                Array.Copy(_arr, 0, array, arrayIndex, _count);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// index
        /// or
        /// index
        /// </exception>
        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    if (index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    return _arr[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterUpgradeableReadLock();
                try
                {

                    if (index >= _count)
                        throw new ArgumentOutOfRangeException("index");

                    _lock.EnterWriteLock();
                    try
                    {
                        _arr[index] = value;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }

            }
        }

        /// <summary>
        /// Executes the action thread-safely.
        /// </summary>
        /// <param name="action">The action.</param>
        public void DoSync(Action<ConcurrentList<T>> action)
        {
            GetSync(l =>
            {
                action(l);
                return 0;
            });
        }

        /// <summary>
        /// Executes the function thread-safely.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public TResult GetSync<TResult>(Func<ConcurrentList<T>, TResult> func)
        {
            _lock.EnterWriteLock();
            try
            {
                return func(this);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _lock.Dispose();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Ensures the capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        private void EnsureCapacity(int capacity)
        {
            if (_arr.Length >= capacity)
                return;

            int doubled;
            checked
            {
                try
                {
                    doubled = _arr.Length * 2;
                }
                catch (OverflowException)
                {
                    doubled = int.MaxValue;
                }
            }

            var newLength = Math.Max(doubled, capacity);
            Array.Resize(ref _arr, newLength);
        }

        /// <summary>
        /// Index of the internal item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private int IndexOfInternal(T item)
        {
            return Array.FindIndex(_arr, 0, _count, x => x.Equals(item));
        }

        /// <summary>
        /// Removes the internal item at index.
        /// </summary>
        /// <param name="index">The index.</param>
        private void RemoveAtInternal(int index)
        {
            Array.Copy(_arr, index + 1, _arr, index, _count - index - 1);
            _count--;

            // release last element
            Array.Clear(_arr, _count, 1);
        }

        #endregion
    }
}
