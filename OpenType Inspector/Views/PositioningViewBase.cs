using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace OpenTypeInspector
{
    public abstract class PositioningViewBase<T> : ViewBase<T> where T : PositioningItem
    {
        protected GlyphTypeface Typeface => Inspector.Typeface;
        protected GlyphPositioningInspector Inspector { get; }

        private volatile bool _loaded;

        private Dispatcher _dispatcher;
        private ObservableCollection<T> _items;
        private ListCollectionView _itemsView;

        private IList<ushort> _filter;
        private static UShortIListConverter _filterConverter = new UShortIListConverter();

        public override IReadOnlyList<T> Items
        {
            get
            {
                InitializeItems();
                return _items;
            }
        }
        public CollectionView ItemsView
        {
            get
            {
                InitializeItems();
                return _itemsView;
            }
        }

        public string Filter
        {
            get
            {
                if (_filter == null)
                    return null;

                return _filterConverter.ConvertToString(_filter);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (_filter == null)
                        return;

                    _filter = null;
                }
                else
                    _filter = (IList<ushort>)_filterConverter.ConvertFromString(value);

                _itemsView.Refresh();
            }
        }
 
        public PositioningViewBase(GlyphPositioningInspector inspector)
        {
            Inspector = inspector;
            _dispatcher = Dispatcher.CurrentDispatcher;

            _items = new ObservableCollection<T>();
            _itemsView = new ListCollectionView(_items);
            _itemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PositioningItem.GroupingHeader)));
            //_itemsView.Filter = o => ((T)o).Matches(_filter);
        }

        private void InitializeItems()
        {
            if (_loaded)
                return;

            lock (this)
            {
                if (_loaded)
                    return;
                else
                    _loaded = true;

                new Thread(PopulateItems) { IsBackground = true }.Start();
            }
        }
        private void PopulateItems()
        {
            ushort lookupIndex = 0;
            foreach (object lookup in Inspector.EnumerateLookups())
            {
                ushort lookupSubtableIndex = 0;
                foreach (object lookupSubtable in Inspector.EnumerateLookupSubtables(lookup))
                {
                    PositioningLookupType substitutionType = GlyphPositioningInspector.GetLookupSubtableType(lookupSubtable);
                    IEnumerable<T> substitutionItems = CreateItems(substitutionType, lookup, lookupIndex, lookupSubtable, lookupSubtableIndex);

                    if (substitutionItems != null)
                        foreach (T substitutionItem in substitutionItems)
                            _dispatcher.Invoke(() => _items.Add(substitutionItem), DispatcherPriority.Background);

                    lookupSubtableIndex++;
                }
                lookupIndex++;
            }
        }

        protected abstract IEnumerable<T> CreateItems(PositioningLookupType substitutionType, object lookup, ushort lookupIndex, object lookupSubtable, ushort lookupSubtableIndex);
    }
}
