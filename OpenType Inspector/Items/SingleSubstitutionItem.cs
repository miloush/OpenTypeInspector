namespace OpenTypeInspector
{
    using System.Windows.Media;

    public class SingleSubstitutionItem : SubstitutionItem
    {
        public ushort SingleIndex { get; set; }

        public override string ItemHeader
        {
            get { return string.Format("Single {0}/{1}/{2}", LookupIndex, LookupSubtableIndex, SingleIndex); }
        }

        public SingleSubstitutionItem(GlyphTypeface typeface) : base(typeface) { }
    }
}
