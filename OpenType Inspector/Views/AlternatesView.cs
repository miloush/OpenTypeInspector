namespace OpenTypeInspector
{
    using System.Collections.Generic;
    using System.Linq;

    public class AlternatesView : SubstitutionViewBase<AlternateSubstitutionItem>
    {
        public AlternatesView(GlyphSubstitutionsInspector inspector) : base(inspector) { }
        
        protected override IEnumerable<AlternateSubstitutionItem> CreateItems(SubstitutionLookupType substitutionType, object lookup, ushort lookupIndex, object lookupSubtable, ushort lookupSubtableIndex)
        {
            if (substitutionType == SubstitutionLookupType.Alternate)
            {
                ushort alternateSetIndex = 0;
                foreach (object alternateSet in Inspector.EnumerateAlternateSets(lookupSubtable))
                {
                    ushort firstGlyphID = Inspector.GetAlternateFirstGlyph(lookupSubtable, alternateSetIndex);
                    ushort[] components = Inspector.EnumerateAlternates(alternateSet).ToArray();

                    yield return new AlternateSubstitutionItem(Typeface)
                    {
                        LookupIndex = lookupIndex,
                        LookupSubtableIndex = lookupSubtableIndex,
                        AlternateSetIndex = alternateSetIndex,
                        PreComponents = new[] { firstGlyphID },
                        PostComponents = components
                    };

                    alternateSetIndex++;
                }
            }
        }
    }
}
