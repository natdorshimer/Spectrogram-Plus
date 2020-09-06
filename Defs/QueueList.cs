using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Spectrogram_Structures
{

    //A linked list that implements all functions of List and can be used as such
    //Pretty bad overall time complexity except for adding to the beginning and ending, which is good for revolving data
    //This is essentially a queue that can be used as a list
    public class QueueList<T> : IList<T>
    {
        public int maxSize { get; private set; }

        private LinkedList<T> data;

        public int Count { get => data.Count; }

        public bool IsReadOnly => false;

        public  T this[int index] { get => data.ElementAt(index); set => this.Insert(index, value); }

        public QueueList(int maxSize) : base()
        {
            data = new LinkedList<T>();
            this.maxSize = maxSize;
        }

        public int IndexOf(T item)
        {
            LinkedListNode<T> iter = data.First;

            for(int i = 0; iter != null; i++)
            {
                if (iter.Value.Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index == 0)
                data.AddFirst(item);
            else
            {
                LinkedListNode<T> iter = GetNodeAt(index);
                if(iter == null)
                    this.Add(item);
                else
                    data.AddAfter(iter, item);
            }
            updateSize();
        }

        private void updateSize()
        {
            if (data.Count >= maxSize)
                data.RemoveFirst();
        }

        public void RemoveAt(int index)
        {
            LinkedListNode<T> iter = GetNodeAt(index);
            if (iter == null)
                throw new IndexOutOfRangeException();
            data.Remove(iter);
        }


        public void RemoveRange(int index, int count)
        {
            var iter = GetNodeAt(index);
            for(int i = 0; i < count; i++)
            {
                var delete_node = iter;
                iter = iter.Next;
                data.Remove(delete_node);
            }
        }

        public void Add(T item)
        {
            data.AddLast(item);
            updateSize();
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        //O(n/2) ~= O(n) still
        private LinkedListNode<T> GetNodeAt(int index)
        {
            if (index >= Count || index < 0)
                throw new IndexOutOfRangeException();

            if (Count - index >= index)
            {
                LinkedListNode<T> iter = data.First;
                for (int i = 0; i < index && iter != null; i++)
                    iter = iter.Next;
                return iter;
            }
            else
            {
                LinkedListNode<T> iter = data.Last;
                for (int i = Count - 1; i > index && iter != null; i--)
                    iter = iter.Previous;
                return iter;
            }
        }
    }
}
