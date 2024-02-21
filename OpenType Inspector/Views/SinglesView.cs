namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SinglesView : SubstitutionViewBase<SingleSubstitutionItem>
    {
        public SinglesView(GlyphSubstitutionsInspector inspector) : base(inspector) { }
        
        protected override IEnumerable<SingleSubstitutionItem> CreateItems(SubstitutionLookupType substitutionType, object lookup, ushort lookupIndex, object lookupSubtable, ushort lookupSubtableIndex)
        {
            if (substitutionType == SubstitutionLookupType.Single)
            {
                ushort singleIndex = 0;
                foreach (Tuple<ushort, ushort> single in Inspector.EnumerateSingles(lookupSubtable))
                {
                    yield return new SingleSubstitutionItem(Typeface)
                    {
                        LookupIndex = lookupIndex,
                        LookupSubtableIndex = lookupSubtableIndex,
                        SingleIndex = singleIndex,
                        PreComponents = new[] { single.Item1 },
                        PostComponents = new[] { single.Item2 }
                    };

                    singleIndex++;
                }
            }
        }
    }
}
