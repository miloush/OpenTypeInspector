namespace OpenTypeInspector
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media.Converters;

    public class LigaturesView : SubstitutionViewBase<LigatureSubstitutionItem>
    {
        public LigaturesView(GlyphSubstitutionsInspector inspector) : base(inspector) { }

        protected override IEnumerable<LigatureSubstitutionItem> CreateItems(SubstitutionLookupType substitutionType, object lookup, ushort lookupIndex, object lookupSubtable, ushort lookupSubtableIndex)
        {
            if (substitutionType == SubstitutionLookupType.Ligature)
            {
                ushort ligatureSetIndex = 0;
                foreach (object ligatureSet in Inspector.EnumerateLigatureSets(lookupSubtable))
                {
                    ushort firstGlyphID = Inspector.GetLigatureFirstGlyph(lookupSubtable, ligatureSetIndex);

                    ushort ligatureIndex = 0;
                    foreach (object ligature in Inspector.EnumerateLigatures(ligatureSet))
                    {
                        ushort glyphID = Inspector.GetLigatureGlyph(ligature);
                        ushort[] components = Inspector.EnumerateLigatureComponents(ligature).ToArray();

                        ushort[] allComponents = new ushort[components.Length + 1];
                        allComponents[0] = firstGlyphID;
                        components.CopyTo(allComponents, 1);

                        yield return new LigatureSubstitutionItem(Typeface)
                        {
                            LookupIndex = lookupIndex,
                            LookupSubtableIndex = lookupSubtableIndex,
                            LigatureSetIndex = ligatureSetIndex,
                            LigatureIndex = ligatureIndex,
                            PreComponents = allComponents,
                            PostComponents = new[] { glyphID },
                        };

                        ligatureIndex++;
                    }
                    ligatureSetIndex++;
                }
            }
        }
    }
}