namespace OpenTypeInspector
{
    using System.Windows.Media;

    public class AlternateSubstitutionItem : SubstitutionItem
    {
        public ushort AlternateSetIndex { get; set; }

        public override string ItemHeader
        {
            get { return string.Format("Alternate set {0}/{1}/{2}", LookupIndex, LookupSubtableIndex, AlternateSetIndex); }
        }

        public AlternateSubstitutionItem(GlyphTypeface typeface) : base(typeface) { }
    }
}
