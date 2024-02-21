namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    public class ContextItem
    {
        private static readonly GlyphItem[][] EmptyItems = new GlyphItem[0][];

        private GlyphTypeface _typeface;

        public GlyphTypefaceInspector.ContextSubtableType Format { get; set; }
        public ushort LookupIndex { get; set; }
        public ushort LookupSubtableIndex { get; set; }
        public ushort SetIndex { get; set; }
        public ushort RuleIndex { get; set; }
        public ushort[][] ContextComponents { get; set; }
        public Tuple<ushort, ushort>[] Substitutions { get; set; }

        private GlyphItem[][] _contextComponentItems;
        public GlyphItem[][] ContextComponentItems
        {
            get
            {
                if (_contextComponentItems == null)
                {
                    if (ContextComponents == null || ContextComponents.Length == 0)
                        return EmptyItems;

                    _contextComponentItems = new GlyphItem[ContextComponents.Length][];
                    for (int i = 0; i < _contextComponentItems.Length; i++)
                    {
                        if (ContextComponents[i] == null)
                        {
                            _contextComponentItems[i] = new[] { new GlyphItem("ANY\r\nELSE") };
                            continue;
                        }

                        if (ContextComponents[i].Length == 0)
                        {
                            _contextComponentItems[i] = new[] { new GlyphItem("NONE") };
                            continue;
                        }

                        _contextComponentItems[i] = new GlyphItem[ContextComponents[i].Length];

                        for (int j = 0; j < _contextComponentItems[i].Length; j++)
                            _contextComponentItems[i][j] = new GlyphItem(ContextComponents[i][j], _typeface);
                    }
                }

                return _contextComponentItems;
            }
        }

        public virtual string GroupingHeader
        {
            get { return string.Format("Lookup {0}, Subtable {1} ({2})", LookupIndex, LookupSubtableIndex, Format); }
        }
        public virtual string ItemHeader
        {
            get
            {
                switch (Format)
                {
                    case GlyphTypefaceInspector.ContextSubtableType.GlyphBased:
                        return string.Format("Substitution set {0}", SetIndex);
                 
                    case GlyphTypefaceInspector.ContextSubtableType.ClassBased:
                        return string.Format("Substitution rule {0}/{1}", SetIndex, RuleIndex);
                    
                    case GlyphTypefaceInspector.ContextSubtableType.CoverageBased:
                    default:
                        return "Subsitution";
                }

            }
        }

        public ContextItem(GlyphTypeface typeface)
        {
            _typeface = typeface;
        }

        public bool Matches(IList<ushort> values)
        {
            return Matches(values, ContextComponents);
        }

        private bool Matches(IList<ushort> values, ushort[][] ContextComponents)
        {
            if (values == null || values.Count < 1)
                return true;

            if (ContextComponents == null)
                return false;

            int lastIndex = ContextComponents.Length - values.Count;

            int index = FindFirst(values[0], ContextComponents, 0, lastIndex);
            while (index >= 0)
            {
                if (IsMatch(values, ContextComponents, index))
                    return true;

                index = FindFirst(values[0], ContextComponents, index + 1, lastIndex);
            }

            return false;
        }

        private bool IsMatch(IList<ushort> values, ushort[][] ContextComponents, int index)
        {
            // we know values fit starting at index

            for (int i = 1; i < values.Count; i++)
            {
                if (ContextComponents[i + index] == null || ContextComponents[i + index].Length < 1)
                    continue;

                if (Array.IndexOf(ContextComponents[i + index], values[i]) < 1)
                    return false;
            }

            return true;
        }

        private int FindFirst(ushort glyph, ushort[][] ContextComponents, int startIndex, int lastIndex)
        {
            for (int i = startIndex; i <= lastIndex; i++)
            {
                if (ContextComponents[i] == null || ContextComponents[i].Length < 1)
                    return i;

                if (Array.IndexOf(ContextComponents[i], glyph) >= 0)
                    return i;
            }

            return -1;
        }

    }
}
