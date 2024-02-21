namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class LazyArray<T> : IReadOnlyList<T> where T : class
    {
        private T[] _allItems;
        private Func<int, T> _instantiate;

        public LazyArray(Func<int, T> instantiate, int count)
        {
            if (instantiate == null)
                throw new ArgumentNullException("instantiate");

            _allItems = new T[count];
            _instantiate = instantiate;
        }

        public int Count { get { return _allItems.Length; } }
        public T this[int index] { get { return _allItems[index] ?? (_allItems[index] = _instantiate(index)); } }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Range(0, _allItems.Length).Select(index => this[index]).GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
