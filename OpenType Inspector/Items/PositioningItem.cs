
namespace OpenTypeInspector
{
    using System.Windows.Media;

    public class PositioningItem
    {
        private static readonly GlyphItem[] EmptyItems = new GlyphItem[0];

        private GlyphTypeface _typeface;

        public ushort LookupIndex { get; set; }
        public ushort LookupSubtableIndex { get; set; }

        public virtual string GroupingHeader
        {
            get { return string.Format("Lookup {0}, Subtable {1}", LookupIndex, LookupSubtableIndex); }
        }
        public virtual string ItemHeader
        {
            get { return "Positioning"; }
        }

        public PositioningItem(GlyphTypeface typeface)
        {
            _typeface = typeface;
        }
    }
}
