namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;

    internal class GlyphCollectionView : ICollectionView
    {
        private class Deferer : IDisposable
        {
            GlyphCollectionView _view;

            public Deferer(GlyphCollectionView collection)
            {
                _view = collection;
            }

            public void Dispose()
            {
                _view.OnCollectionReset();
            }
        }

        private GlyphCollection _collection;
        private int _count;
        private bool _needsReset;

        public GlyphCollectionView(GlyphCollection collection)
        {
            _collection = collection;
            _count = collection.Count;
            _needsReset = true;
        }

        public CultureInfo Culture { get; set; }
        
        public bool CanFilter { get { return false; } }
        public bool CanGroup { get { return false; } }
        public bool CanSort { get { return false; } }

        public bool Contains(object item) { return ((GlyphItem)item).GlyphID < _count; }

        private ushort? _currentGlyphID;      
        public object CurrentItem { get { return _currentGlyphID.HasValue ? new GlyphItem(_currentGlyphID.Value, _collection.Typeface) : (object)null; } }
        public int CurrentPosition { get { return _currentGlyphID ?? -1; } }

        public Predicate<object> Filter { get { return null; } set { throw new NotSupportedException(); } }
        public SortDescriptionCollection SortDescriptions { get { return null; } }
        public ObservableCollection<GroupDescription> GroupDescriptions {get { return null; } }
        public ReadOnlyObservableCollection<object> Groups { get { return null; } }

        public bool IsCurrentAfterLast { get { return _currentGlyphID == null; } }
        public bool IsCurrentBeforeFirst { get { return _currentGlyphID == null; } }
        public bool IsEmpty { get { return _count < 1; } }

        public bool MoveCurrentTo(object item) { return MoveCurrentToPosition(((GlyphItem)item).GlyphID); }
        public bool MoveCurrentToFirst() { return MoveCurrentToPosition(_count < 1 ? -1 : 0); }
        public bool MoveCurrentToLast() { return MoveCurrentToPosition(_count - 1); }
        public bool MoveCurrentToNext() { return MoveCurrentToPosition((_currentGlyphID ?? -1) + 1); }
        public bool MoveCurrentToPrevious() { return MoveCurrentToPosition((_currentGlyphID ?? 0) - 1); }
        public bool MoveCurrentToPosition(int index)
        {
            if (index < -1 || index >= _count)
                throw new ArgumentOutOfRangeException("index");

            ushort? newGlyphID;
            bool inView;

            if (index == -1)
            {
                newGlyphID = null;
                inView = false;
            }
            else
            {
                newGlyphID = (ushort)index;
                inView = true;
            }

            if (_currentGlyphID != newGlyphID && OnCurrentChanging())
            {
                _currentGlyphID = newGlyphID;
                OnCurrentChanged();
            }

            return inView;
        }

        public IDisposable DeferRefresh() { return new Deferer(this); }
        public void Refresh() { }

        public IEnumerable SourceCollection { get { return _collection; } }
        public IEnumerator GetEnumerator() { return _collection.GetEnumerator(); }

        public event EventHandler CurrentChanged;
        public event CurrentChangingEventHandler CurrentChanging;

        private bool OnCurrentChanging()
        {
            CurrentChangingEventHandler changing = CurrentChanging;

            if (changing == null)
                return true;

            CurrentChangingEventArgs args = new CurrentChangingEventArgs(true);
            changing(this, args);

            return args.Cancel == false;
        }
        private void OnCurrentChanged()
        {
            EventHandler changed = CurrentChanged;

            if (changed != null)
                changed(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionReset()
        {
            if (!_needsReset)
                return;
            else
                _needsReset = false;

            NotifyCollectionChangedEventHandler changed = CollectionChanged;

            if (changed != null)
                changed(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
