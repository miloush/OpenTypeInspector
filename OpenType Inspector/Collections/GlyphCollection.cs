namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Media;

    internal class GlyphCollection : ICollectionViewFactory, IReadOnlyList<GlyphItem>, System.Collections.IList
    {
        private GlyphTypeface _typeface;
        public GlyphTypeface Typeface { get { return _typeface; } }

        private bool _classesRetreived;
        private Dictionary<GlyphClass, int> _classCounts;
        private ExpandableDictionary<ushort, GlyphClass> _classes;
        protected IDictionary<ushort, GlyphClass> Classes
        {
            get { return EnsureClasses() ? _classes : default; }
        }

        private bool EnsureClasses()
        {
            if (_classes == null)
            {
                if (_classesRetreived) return false;

                _classesRetreived = true;
                var classes = new GlyphTypefaceInspector(_typeface).Definitions.GetGlyphClasses();
                if (classes != null)
                {
                    _classes = new ExpandableDictionary<ushort, GlyphClass>(classes);

                    int[] counts = new int[6];
                    foreach (var c in classes.Values)
                        counts[(int)c]++;

                    _classCounts = new Dictionary<GlyphClass, int>();
                    for (int i = 0; i < counts.Length; i++)
                    {
                        _classCounts[(GlyphClass)i] = counts[i];
                    }
                }
            }

            return true;
        }

        public int GetClassCount(GlyphClass c)
        {
            EnsureClasses();
            if (_classCounts?.TryGetValue(c, out int count) == true)
                return count;

            return 0;
        }

        public GlyphCollection(GlyphTypeface typeface)
        {
            if (typeface == null)
                throw new ArgumentNullException("typeface");

            _typeface = typeface;
        }

        public ICollectionView CreateView()
        {
            return new System.Windows.Data.ListCollectionView(this);
            // return new GlyphCollectionView(this);
        }

        public GlyphItem this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");

                return new GlyphItem((ushort)index, _typeface) { Class = Classes?[(ushort)index] ?? default };
            }
        }

        public int Count
        {
            get { return _typeface.GlyphCount; }
        }

        public IEnumerator<GlyphItem> GetEnumerator()
        {
            for (ushort index = 0; index < _typeface.GlyphCount; index++)
                yield return new GlyphItem(index, _typeface) { Class = Classes?[index] ?? default };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IList

        int System.Collections.ICollection.Count
        {
            get { return Count; }
        }

        bool System.Collections.IList.IsFixedSize
        {
            get { return true; }
        }

        bool System.Collections.IList.IsReadOnly
        {
            get { return true; }
        }

        bool System.Collections.IList.Contains(object value)
        {
            return ((GlyphItem)value).GlyphID < Count;
        }

        int System.Collections.IList.IndexOf(object value)
        {
            return ((GlyphItem)value).GlyphID;
        }

        object System.Collections.IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        int System.Collections.IList.Add(object value) { throw new NotSupportedException(); }
        void System.Collections.IList.Clear() { throw new NotSupportedException(); }
        void System.Collections.IList.Insert(int index, object value) { throw new NotSupportedException(); }
        void System.Collections.IList.Remove(object value) { throw new NotSupportedException(); }
        void System.Collections.IList.RemoveAt(int index) { throw new NotSupportedException(); }
        void System.Collections.ICollection.CopyTo(Array array, int index) { throw new NotImplementedException(); }
        bool System.Collections.ICollection.IsSynchronized { get { throw new NotSupportedException(); } }
        object System.Collections.ICollection.SyncRoot { get { throw new NotSupportedException(); } }

        #endregion
    }

}
