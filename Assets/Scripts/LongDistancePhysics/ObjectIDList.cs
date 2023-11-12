using System;
using System.Collections;
using System.Collections.Generic;

namespace LongDistancePhysics
{
    /// <summary>
    /// A special List type that tracks used indices.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectIDList<T> : IEnumerable<T>
    {
        private List<T> ObjectList = new List<T>();

        private SortedSet<int> ReleasedIDs; 
        // Preferably use a PriorityQueue, however Unity's Mono while syntactically the same
        // as C#9.0, is closer to DotNet 4 in terms of features and lacks the PriorityQueue datatype.
        // If performance becomes an issue, I will make my queue type.

        public IEnumerator<T> GetEnumerator()
        {
            return ObjectList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Add(T item)
        {
            if (ReleasedIDs.Count == 0)
            {
                ObjectList.Add(item);
                return ObjectList.Count - 1;
            }
            int index = ReleasedIDs.Min;
            ReleasedIDs.Remove(index);
            ObjectList[index] = item;
            return index;
        }

        public void Clear()
        {
            ObjectList.Clear();
            ReleasedIDs.Clear();;
        }

        public bool Contains(T item)
        {
            return ObjectList.Contains(item);
        }

        public int Count => ObjectList.Count;

        public int IndexOf(T item)
        {
            return ObjectList.IndexOf(item);
        }

        /// <summary>
        /// Pushes down the released indices so that the end of the list
        /// can be moved up.
        /// </summary>
        private void PushDownReleasedIndices(bool skipCheck = false)
        {
            int currentIndex = ObjectList.Count - 1;
            if (skipCheck || ReleasedIDs.Max == currentIndex)
            {
                while (ObjectList[currentIndex] == null)
                {
                    ReleasedIDs.Remove(currentIndex);
                    ObjectList.RemoveAt(currentIndex);
                    currentIndex--;
                }
            }
        }

        void RemoveAt(int index)
        {
            if (index == ObjectList.Count - 1)
            {
                // last item in list, we can simply remove
                ObjectList.RemoveAt(index);
                if (ReleasedIDs.Max == index - 1)
                {
                    PushDownReleasedIndices(true);    
                }
            }
            else
            {
                ObjectList[index] = default(T);
                ReleasedIDs.Add(index);
            }
        }

        public T this[int index]
        {
            get => ObjectList[index];
        }
    }
}