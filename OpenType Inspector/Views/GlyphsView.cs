namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media.Converters;

    public class GlyphsView : ViewBase<GlyphItem>
    {
        private GlyphCollection _items;

        private IList<ushort> _filter;
        private static UShortIListConverter _filterConverter = new UShortIListConverter();

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
                _filter = (IList<ushort>)_filterConverter.ConvertFromString(value);

                if (_filter.Any(index => index >= _items.Count))
                    throw new ArgumentException();

                OnItemsChanged();
            }
        }

        public override IReadOnlyList<GlyphItem> Items
        {
            get
            {
                if (_filter == null || _filter.Count < 1)
                    return _items;

                return _filter.Select(index => _items[index]).ToArray();
            }
        }

        public GlyphsView(FontItem fontItem)
        {
            _items = new GlyphCollection(fontItem.Typeface);
        }

        public string BaseCount => $"Base ({_items.GetClassCount(GlyphClass.Base)})";
        public string MarkCount => $"Mark ({_items.GetClassCount(GlyphClass.Mark)})";
        public string LigatureCount => $"Ligature ({_items.GetClassCount(GlyphClass.Ligature)})";
        public string ComponentCount => $"Component ({_items.GetClassCount(GlyphClass.Component)})";
    }
}
