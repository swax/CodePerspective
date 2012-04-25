using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XLibrary
{
    // this is a dictionary where values can be added, for fast look up dynamically without needing a lock
    // one thread needs to be able to write values fast
    // while another threads need to be able to read values fast
    class SharedDictionary<T> : IEnumerable<T>
        where T : class
    {
        internal int Length;
        internal T[] Values;

        Dictionary<int, int> Map = new Dictionary<int, int>();

        internal SharedDictionary(int keyMax)
        {
            Values = new T[keyMax];
        }

        internal bool Contains(int hash)
        {
            return Map.ContainsKey(hash);
        }

        internal bool TryGetValue(int hash, out T call)
        {
            int index;
            if (Map.TryGetValue(hash, out index))
            {
                call = Values[index];
                return true;
            }

            call = null;
            return false;
        }

        internal void Add(int hash, T call)
        {
            // locking isnt so bad because once app is running, add won't be called so much
            lock (this)
            {
                if (Length >= Values.Length)
                {
                    T[] resized = new T[Values.Length * 2];
                    Values.CopyTo(resized, 0);
                    Values = resized;
                }

                int index = Length;
                Map[hash] = index;
                Values[index] = call;

                Length++;
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return Values[i];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return Values[i];
        }

        #endregion
    }

}
