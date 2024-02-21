namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public class CharactersView
    {
        private FontItem _fontItem;
        private CharacterItem[] _allItems;
        private ListCollectionView _itemsView;
        private static string _filter; // should be moved to app-wide VM
        private static string _nameFilter;
        private SortedSet<int> _intFilter = new SortedSet<int>();
        private SortedSet<ushort> _ushortFilter = new SortedSet<ushort>();

        private CharacterItem[] AllItems
        {
            get
            {
                if (_allItems == null)
                {
                    _allItems = _fontItem.Typeface.CharacterToGlyphMap.Keys.Select(codepoint => new CharacterItem(codepoint, _fontItem.Typeface)).ToArray();
                }

                return _allItems;
            }
        }
        public ListCollectionView ItemsView
        {
            get
            {
                if (_itemsView == null)
                {
                    _itemsView = new ListCollectionView(AllItems);
                    _itemsView.Filter = ItemsFilter;
                }

                return _itemsView;
            }
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                _intFilter.Clear();
                _ushortFilter.Clear();

                if (!string.IsNullOrEmpty(_filter))
                    foreach (string token in _filter.Split(null as char[], StringSplitOptions.RemoveEmptyEntries))
                    {
                        int number;
                        if (int.TryParse(token, out number) && number >= 0)
                        {
                            _intFilter.Add(number);

                            if (number <= ushort.MaxValue)
                                _ushortFilter.Add((ushort)number);
                        }

                        if (int.TryParse(token, NumberStyles.AllowHexSpecifier, null, out number) && number >= 0)
                        {
                            _intFilter.Add(number);

                            if (number <= ushort.MaxValue)
                                _ushortFilter.Add((ushort)number);
                        }
                    }

                if (_itemsView != null)
                    _itemsView.Refresh();
            }
        }
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                if (value != _nameFilter)
                {
                    _nameFilter = value;

                    if (_itemsView != null)
                        _itemsView.Refresh();
                }
            }
        }

        private bool ItemsFilter(object obj)
        {
            CharacterItem item = (CharacterItem)obj;

            if (string.IsNullOrEmpty(_filter))
            {
                if (string.IsNullOrEmpty(_nameFilter))
                    return true;

                string charName = item.CharacterName;
                if (charName == null)
                    return false;

                return charName.IndexOf(_nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                if (!string.IsNullOrEmpty(_nameFilter))
                {
                    string charName = item.CharacterName;

                    if (charName != null && charName.IndexOf(_nameFilter, StringComparison.OrdinalIgnoreCase) == -1)
                        return false;
                }

                if (_intFilter.Contains(item.Codepoint))
                    return true;

                if (_ushortFilter.Contains(item.GlyphID))
                    return true;

                if (_filter != null && _filter.IndexOf(item.Character, StringComparison.Ordinal) >= 0)
                    return true;

                return false;
            }
        }

        public CharactersView(FontItem fontItem)
        {
            _fontItem = fontItem;
            Filter = _filter;
            NameFilter = _nameFilter;
        }
    }
}
