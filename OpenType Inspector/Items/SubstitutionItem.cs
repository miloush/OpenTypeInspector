namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    public class SubstitutionItem
    {
        private static readonly GlyphItem[] EmptyItems = new GlyphItem[0];

        private GlyphTypeface _typeface;

        public ushort LookupIndex { get; set; }
        public ushort LookupSubtableIndex { get; set; }
        public ushort[] PreComponents { get; set; }
        public ushort[] PostComponents { get; set; }

        public virtual string GroupingHeader
        {
            get { return string.Format("Lookup {0}, Subtable {1}", LookupIndex, LookupSubtableIndex); }
        }
        public virtual string ItemHeader
        {
            get { return "Substitution"; }
        }

        private GlyphItem[] _preComponentItems;
        public GlyphItem[] PreComponentItems
        {
            get
            {
                if (_preComponentItems == null)
                {
                    if (PreComponents == null || PreComponents.Length == 0)
                        return EmptyItems;

                    _preComponentItems = new GlyphItem[PreComponents.Length];
                    for (int i = 0; i < _preComponentItems.Length; i++)
                        _preComponentItems[i] = new GlyphItem(PreComponents[i], _typeface);
                }

                return _preComponentItems;
            }
        }

        private GlyphItem[] _postComponentItems;
        public GlyphItem[] PostComponentItems
        {
            get
            {
                if (_postComponentItems == null)
                {
                    if (PostComponents == null || PostComponents.Length == 0)
                        return EmptyItems;

                    _postComponentItems = new GlyphItem[PostComponents.Length];
                    for (int i = 0; i < _postComponentItems.Length; i++)
                        _postComponentItems[i] = new GlyphItem(PostComponents[i], _typeface);
                }

                return _postComponentItems;
            }
        }

        public SubstitutionItem(GlyphTypeface typeface)
        {
            _typeface = typeface;
        }

        public bool Matches(IList<ushort> values)
        {
            if (values == null)
                return true;

            if (PostComponents == null || PreComponents == null)
                return false;

            for (int i = 0; i < values.Count; i++)
            {
                ushort value = values[i];

                if (Array.IndexOf(PostComponents, value) == -1 && Array.IndexOf(PreComponents, value) == -1)
                    return false;
            }

            return true;
        }
    }
}
