using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Profiling;

namespace ObjectZoning
{
    // This class is NOT thread safe.
    public class StructAllocator<TStruct>
        where TStruct : struct, IStructAllocatorObject
    {
        private SANode[] _items;
        private int[] _indicesQueue;
        public int TotalCount => _items.Length;
        public int ActiveCount { get; private set; }
        public int InactiveCount => TotalCount - ActiveCount;

        public const int InitSize = 64;

        // 0 is no concurrency, 1 is 1 lock per element, 2 is 2 locks per element, etc
        private int _concurrencyRatio;
        
        private struct SANode
        {
            public TStruct Value;
            public bool Active;
            public int Reverse;
        }
        
        public StructAllocator() : this(InitSize, 0)
        {
            
        }

        public StructAllocator(int size, int concurrencyRatio)
        {
            _items = new SANode[size];
            _indicesQueue = new int[size];
            ActiveCount = 0;
            Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Init()
        {
            InitFrom(0);
        }

        private void InitFrom(int i)
        {
            while (i < _indicesQueue.Length)
            {
                _indicesQueue[i] = i++;
            }
        }

        private void ResizeDouble()
        {
            int newSize = TotalCount * 2;
            int oldSize = TotalCount;

            SANode[] newItemsArray = new SANode[newSize];
            int[] newQueue = new int[newSize];
            
            _items.CopyTo(newItemsArray, 0);
            _indicesQueue.CopyTo(newQueue, 0);

            _items = newItemsArray;
            _indicesQueue = newQueue;
            InitFrom(oldSize);
        }

        public void ResizeTo(int newSize)
        {
            
            int oldSize = TotalCount;

            SANode[] newItemsArray = new SANode[newSize];
            int[] newQueue = new int[newSize];
            
            _items.CopyTo(newItemsArray, 0);
            _indicesQueue.CopyTo(newQueue, 0);

            _items = newItemsArray;
            _indicesQueue = newQueue;
            InitFrom(oldSize);
        }
        
        
        /*
         * ID Queueing Operations
         * 
         */

        /// <summary>
        /// Gets the next ID and returns the index of the ID for SANode.Reverse
        /// </summary>
        /// <param name="nextID"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNextIDAndIncrement(out int nextID)
        {
            if (ActiveCount == TotalCount)
            {
                ResizeDouble();
            }
            nextID = _indicesQueue[ActiveCount];
            return ActiveCount++;
        }

        /// <summary>
        /// Deallocates the ID at the target index, returns the new index
        /// of the SANode that it was swapped with.
        ///
        /// Precondition: the ID at the index points to an active struct
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int DeallocateIDAtIndex(int index)
        {
            int id = _indicesQueue[index];
            _items[id].Value.Destroy();
            _items[id].Active = false;
            
            int other = ActiveCount-- - 1;
            if (other == index)
            {
                return index;
            }
            (_indicesQueue[other], _indicesQueue[index]) = (_indicesQueue[index], _indicesQueue[other]);
            return other;
        }

        private int GetIDAtIndex(int index)
        {
            if (index > _indicesQueue.Length)
            {
                throw IndexOutOfRange();
            }
            return _indicesQueue[index];
        }

        #region Allocators

        /// <summary>
        /// Allocates a single instance of this struct
        /// </summary>
        /// <returns></returns>
        public ref TStruct AllocateStruct()
        {
            int rev = GetNextIDAndIncrement(out var id);
            _items[id].Reverse = rev;
            _items[id].Active = true;
            _items[id].Value.Init();
            _items[id].Value.StructID = id;
            return ref _items[id].Value;
        }
        
        private static readonly ProfilerMarker PostallocMarkers = new("StructAllocator.PostAllocateStruct");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structs"></param>
        public int[] PostAllocateStruct(TStruct[] structs)
        {
            PostallocMarkers.Begin();
            int size = structs.Length;
            if (InactiveCount <= size)
            {
                ResizeTo(size + TotalCount);
            }

            int[] newIDs = new int[size];
            for (int i = 0; i < size; i++)
            {
                var newStruct = structs[i];
                int rev = GetNextIDAndIncrement(out var id);
                _items[id].Active = true;
                _items[id].Value = newStruct;
                _items[id].Value.StructID = id;
                _items[id].Reverse = rev;
                newIDs[i] = id;
            }

            PostallocMarkers.End();
            return newIDs;
        }

        #endregion


        /// <summary>
        /// Deallocates the struct with the given ID.
        ///
        /// Throws an exception if the id is out of range or belongs to an already inactive struct
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void DeallocateStruct(int id)
        {
            if (id >= _items.Length || id < 0)
            {
                throw IDError("Attempted to deallocated an invalid ID, out of range");
            }
            if (!_items[id].Active)
            {
                // TODO: Show invalid index in message

                throw IDError("Attempted to deallocate an inactive Struct");
            }
            
            int index = _items[id].Reverse;
            DeallocateIDAtIndex(index);
            int otherID = _indicesQueue[index];
            (_items[otherID].Reverse, _items[id].Reverse) = (_items[id].Reverse, _items[otherID].Reverse);
        }

        /// <summary>
        /// Deallocates a struct with the given id. Still throws an out of range exception however
        /// simply has no effect if the struct is inactive.
        ///
        /// Note: This is not a solution to poor handling of IDs, it's used for situations where
        /// you would reasonably temporarily have duplicate IDs that need to be deallocated and
        /// the overhead of set datastructures and duplicate checking is undesirable.
        ///
        /// Leaving the duplicate checking to here (by checking if we're trying to deallocate an
        /// inactive node) would be much faster.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void DeallocateStructIfActive(int id)
        {
            if (id >= _items.Length || id < 0)
            {
                throw IDError("Attempted to deallocated an invalid ID, out of range");
            }
            if (!_items[id].Active)
            {
                return;
            }
            int index = _items[id].Reverse;
            DeallocateIDAtIndex(index);
            int otherID = _indicesQueue[index];
            (_items[otherID].Reverse, _items[id].Reverse) = (_items[id].Reverse, _items[otherID].Reverse);
        }

        public void DeallocIf(Func<TStruct, bool> condition, Func<TStruct, bool> cleanupFunc)
        {
            for (int i = 0; i < ActiveCount; i++)
            {
                int id = _indicesQueue[i];
                if (condition(_items[id].Value))
                {
                    cleanupFunc(_items[id].Value);
                    DeallocateStruct(id);
                }
            }
        }

        public void DeallocAll()
        {
            for (int i = 0; i < ActiveCount; i++)
            {
                int id = _indicesQueue[i];
                _items[id].Value.Destroy();
                _items[id].Active = false;
                _indicesQueue[i] = i;
            }

            for (int i = ActiveCount; i < _indicesQueue.Length; i++)
            {
                _indicesQueue[i] = i;
            }

            ActiveCount = 0;
        }

        public ref TStruct GetStruct(int id)
        {
            if (id >= _items.Length)
            {
                throw IDOutOfRange();
            }
            SANode node = _items[id];
            if (node.Active)
            {
                return ref _items[id].Value;
            }

            throw IDInvalid();
        }

        public bool IsActive(int id)
        {
            if (id >= _items.Length || id < 0)
            {
                throw IDOutOfRange();
            }

            return _items[id].Active;
        }

        /// <summary>
        /// Gets an item via it's ID.
        /// </summary>
        /// <param name="id"></param>
        public TStruct this[int id] => GetStruct(id);


        /// <summary>
        /// Returns an array of the active structs. This returns
        /// copies of the structs
        ///
        /// This is the recommended method for performing enumeration operations.
        /// </summary>
        /// <returns></returns>
        public TStruct[] GetActiveStructs()
        {
            TStruct[] returnArray = new TStruct[ActiveCount];
            for (int i = 0; i < ActiveCount; i++)
            {
                int id = _indicesQueue[i];
                returnArray[i] = _items[id].Value;
            }
            return returnArray;
        }
    
        #region Exception Throwing

        private const string BaseIDErrorString =
            "Attempted to access an invalid index";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IDError(string addition = null)
        {
            if (addition == null)
            {
                return new IndexOutOfRangeException(BaseIDErrorString +$": {addition}");
            }
            return new IndexOutOfRangeException(BaseIDErrorString);
        }
        
        private const string BaseIDOutOfRangeString =
            "Attempted to access an invalid ID: ID out of range";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IDOutOfRange(string addition = null)
        {
            if (addition == null)
            {
                throw new IndexOutOfRangeException(BaseIDOutOfRangeString + $" {addition}");
            }
            throw new IndexOutOfRangeException(BaseIDOutOfRangeString);
        }
        
        private const string BaseIDInvAllocString =
            "Attempted to access an invalid ID: ID might not have been allocated or deallocated";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IDInvalid(string addition = null)
        {
            if (addition == null)
            {
                return new IndexOutOfRangeException(BaseIDInvAllocString + $" {addition}");
            }
            return new IndexOutOfRangeException(BaseIDInvAllocString);
        }

        private const string BaseIndexErrorString =
            "Attempted to access an invalid index";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IndexError(string addition = null)
        {
            if (addition == null)
            {
                return new IndexOutOfRangeException(BaseIndexErrorString +$": {addition}");
            }
            return new IndexOutOfRangeException(BaseIndexErrorString);
        }
        
        private const string BaseIndexOutOfRangeString =
            "Attempted to access an invalid index: index out of range";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IndexOutOfRange(string addition = null)
        {
            if (addition == null)
            {
                return new IndexOutOfRangeException(BaseIndexOutOfRangeString +$" {addition}");
            }
            return new IndexOutOfRangeException(BaseIndexOutOfRangeString);
        }
        
        private const string BaseIndexInvAllocString =
            "Attempted to access an invalid ID: ID might not have been allocated or deallocated";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IndexOutOfRangeException IndexInvalid(string addition = null)
        {
            if (addition == null)
            {
                return new IndexOutOfRangeException(BaseIndexInvAllocString +$" {addition}");
            }
            return new IndexOutOfRangeException(BaseIndexInvAllocString);
        }

        #endregion


    }
}