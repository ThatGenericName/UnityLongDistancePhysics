using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LongDistancePhysics.Collections
{
    /// <summary>
    /// An updating set, when you insert a value that already exists,
    /// replaces the original value with the new one.
    ///
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class UpdatingHashSet<TValue> : ISet<TValue>
    {
        // Currently just a wrapper for HashSet<TValue> with the prescribed operations until
        // a more custom implementation is done.
        // The use case is specifically for the way we use sets to store properties.
        
        public UpdatingHashSet()
        {
            _hashSet = new HashSet<TValue>();
        }

        public UpdatingHashSet(int size)
        {
            _hashSet = new HashSet<TValue>(size);
        }

        public UpdatingHashSet(IEnumerable<TValue> items)
        {
            _hashSet = new HashSet<TValue>(items);
        }

        public UpdatingHashSet(IEqualityComparer<TValue> comparer)
        {
            _hashSet = new HashSet<TValue>(comparer);
        }
        
        private HashSet<TValue> _hashSet;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TValue> GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICollection<TValue>.Add(TValue item)
        {
            _hashSet.Remove(item);
            _hashSet.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExceptWith(IEnumerable<TValue> other)
        {
            _hashSet.ExceptWith(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntersectWith(IEnumerable<TValue> other)
        {
            _hashSet.IntersectWith(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSubsetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsProperSupersetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSubsetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSupersetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(IEnumerable<TValue> other)
        {
            return _hashSet.Overlaps(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetEquals(IEnumerable<TValue> other)
        {
            return _hashSet.SetEquals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SymmetricExceptWith(IEnumerable<TValue> other)
        {
            _hashSet.SymmetricExceptWith(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnionWith(IEnumerable<TValue> other)
        {
            _hashSet.UnionWith(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(TValue item)
        {
            _hashSet.Remove(item);
            return _hashSet.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _hashSet.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TValue item)
        {
            return _hashSet.Contains(item);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _hashSet.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TValue item)
        {
            return _hashSet.Remove(item);
        }

        public int Count { get => _hashSet.Count; }
        public bool IsReadOnly { get => false; } // TODO: figure out exactly what this is used for
    }
}