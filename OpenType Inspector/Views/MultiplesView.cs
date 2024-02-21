namespace OpenTypeInspector
{
    using System.Collections.Generic;
    using System.Linq;

    public class MultiplesView : SubstitutionViewBase<MultipleSubstitutionItem>
    {
        public MultiplesView(GlyphSubstitutionsInspector inspector) : base(inspector) { }
        
        protected override IEnumerable<MultipleSubstitutionItem> CreateItems(SubstitutionLookupType substitutionType, object lookup, ushort lookupIndex, object lookupSubtable, ushort lookupSubtableIndex)
        {
            if (substitutionType == SubstitutionLookupType.Multiple)
            {
                ushort multipleSequenceIndex = 0;
                foreach (object multipleSequence in Inspector.EnumerateMultipleSequences(lookupSubtable))
                {
                    ushort firstGlyphID = Inspector.GetMultipleFirstGlyph(lookupSubtable, multipleSequenceIndex);
                    ushort[] components = Inspector.EnumerateMultiples(multipleSequence).ToArray();

                    yield return new MultipleSubstitutionItem(Typeface)
                    {
                        LookupIndex = lookupIndex,
                        LookupSubtableIndex = lookupSubtableIndex,
                        MultipleSequenceIndex = multipleSequenceIndex,
                        PreComponents = new[] { firstGlyphID },
                        PostComponents = components
                    };

                    multipleSequenceIndex++;
                }
            }
        }
    }
}
