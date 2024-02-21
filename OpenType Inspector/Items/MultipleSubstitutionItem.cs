namespace OpenTypeInspector
{
    using System.Windows.Media;

    public class MultipleSubstitutionItem : SubstitutionItem
    {
        public ushort MultipleSequenceIndex { get; set; }

        public override string ItemHeader
        {
            get { return string.Format("Multiple sequence {0}/{1}/{2}", LookupIndex, LookupSubtableIndex, MultipleSequenceIndex); }
        }

        public MultipleSubstitutionItem(GlyphTypeface typeface) : base(typeface) { }
    }
}
