using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XLibrary.Remote
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        public T[] Buffer;
        public int CurrentPos = -1;
        public int Length;

        public int Capacity
        {
            set
            {
                // copy prev elements
                T[] copy = new T[Length];

                for (int i = 0; i < Length && i < value; i++)
                    copy[i] = this[i];

                // re-init buff
                Buffer = new T[value];
                CurrentPos = -1;
                Length = 0;

                // add back values
                Array.Reverse(copy);
                foreach (T init in copy)
                    Add(init);
            }
            get
            {
                return Buffer.Length;
            }
        }


        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
        }

        public T this[int index]
        {
            get
            {
                return Buffer[ToCircleIndex(index)];
            }
            set
            {
                Buffer[ToCircleIndex(index)] = value;
            }
        }

        int ToCircleIndex(int index)
        {
            // linear index to circular index

            if (CurrentPos == -1)
                throw new Exception("Index value not valid");

            if (index >= Length)
                throw new Exception("Index value exceeds bounds of array");

            int circIndex = CurrentPos - index;

            if (circIndex < 0)
                circIndex = Buffer.Length + circIndex;

            return circIndex;
        }

        public void Add(T value)
        {
            if (Buffer == null || Buffer.Length == 0)
                return;

            CurrentPos++;

            // circle around
            if (CurrentPos >= Buffer.Length)
                CurrentPos = 0;

            Buffer[CurrentPos] = value;

            if (Length <= CurrentPos)
                Length = CurrentPos + 1;
        }


        public void Clear()
        {
            Buffer = new T[Capacity];
            CurrentPos = -1;
            Length = 0;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetNext();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetNext();
        }

        private IEnumerator<T> GetNext()
        {
            if (CurrentPos == -1)
                yield break;

            // iterate from most recent to beginning
            for (int i = CurrentPos; i >= 0; i--)
                yield return Buffer[i];

            // iterate the back down
            if (Length == Buffer.Length)
                for (int i = Length - 1; i > CurrentPos; i--)
                    yield return Buffer[i];
        }
    }
}
