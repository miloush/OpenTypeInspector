using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenTypeInspector
{
    public class FeaturesView
    {
        private List<FeatureItem> _items;
        private ListCollectionView _itemsView;
        private List<FeatureItem> Items => _items;
        public ListCollectionView ItemsView => _itemsView ??= CreateCollectionView(Items);

        private string _filter;
        private List<string> _stringFilter = new List<string>();
        private List<ushort> _ushortFilter = new List<ushort>();

        public FeaturesView(IEnumerable<FeatureItem> items)
        {
            _items = items.ToList();
        }

        protected ListCollectionView CreateCollectionView(List<FeatureItem> items)
        {
            ListCollectionView itemsView = new ListCollectionView(items);
            itemsView.Filter = ItemsFilter;
            return itemsView;
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                _stringFilter.Clear();
                _ushortFilter.Clear();

                if (!string.IsNullOrEmpty(_filter))
                {
                    foreach (string token in _filter.Split(null as char[], StringSplitOptions.RemoveEmptyEntries))
                    {
                        ushort number;
                        if (ushort.TryParse(token, out number))
                            _ushortFilter.Add(number);
                        
                        else
                            _stringFilter.Add(token);
                    }
                }

                _itemsView?.Refresh();
            }
        }

        private bool ItemsFilter(object obj)
        {
            if (string.IsNullOrEmpty(_filter))
                return true;

            FeatureItem item = (FeatureItem)obj;

            foreach (ushort lookupIndex in _ushortFilter)
                if (!item.Lookups.Any(lookup => lookup.LookupIndex == lookupIndex))
                    return false;

            foreach (string tag in _stringFilter)
                if (item.TagString != tag && (item.KnownName.IndexOf(tag, StringComparison.OrdinalIgnoreCase) == -1))
                    return false;

            return true;
        }
    }
}
