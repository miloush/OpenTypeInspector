namespace OpenTypeInspector
{
    using System.Windows.Media;

    public class LigatureSubstitutionItem : SubstitutionItem
    {
        public ushort LigatureSetIndex { get; set; }
        public ushort LigatureIndex { get; set; }

        public override string GroupingHeader
        {
            get { return string.Format("Lookup {0}, Subtable {1}, Set {2}", LookupIndex, LookupSubtableIndex, LigatureSetIndex); }
        }
        public override string ItemHeader
        {
            get { return string.Format("Ligature {0}/{1}/{2}/{3}", LookupIndex, LookupSubtableIndex, LigatureSetIndex, LigatureIndex); }
        }

        public LigatureSubstitutionItem(GlyphTypeface typeface) : base(typeface) { }
    }
}
