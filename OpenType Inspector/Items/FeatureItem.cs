namespace OpenTypeInspector
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;

    public class FeatureItem
    {
        public GlyphTypefaceInspector.LookupTableInspector Inspector { get; }
        public int FeatureIndex { get; set; }
        public uint Tag { get; set; }
        public IList<LookupItem> Lookups { get; set; }

        public FeatureItem(GlyphTypefaceInspector.LookupTableInspector inspector)
        {
            Inspector = inspector;
            FeatureIndex = -1;
        }
        public FeatureItem(GlyphTypefaceInspector.LookupTableInspector inspector, ushort featureIndex) : this(inspector)
        {
            FeatureIndex = featureIndex;

            TaggedObject feature = inspector.GetFeature(featureIndex);
            Tag = feature.Tag;

            Lookups = new List<LookupItem>();
            foreach (ushort lookupIndex in Inspector.EnumerateFeatureLookupIndices(feature.Object))
                Lookups.Add(new LookupItem(Inspector, lookupIndex));
        }
        public FeatureItem(GlyphTypefaceInspector.LookupTableInspector inspector, int featureIndex, TaggedObject feature, IReadOnlyDictionary<ushort, LookupItem> lookups) : this(inspector)
        {
            FeatureIndex = featureIndex;

            Tag = feature.Tag;
            Lookups = new List<LookupItem>();
            foreach (ushort lookupIndex in Inspector.EnumerateFeatureLookupIndices(feature.Object))
                Lookups.Add(lookups[lookupIndex]);
        }

        private string _tagString;
        public string TagString => _tagString ??= GlyphTypefaceInspector.ToTagString(Tag);

        private string _knownName;
        public string KnownName => _knownName ??= GlyphTypefaceInspector.GetKnownFeatureName(Tag) ?? string.Empty;

        public string LookupsString
        {
            get
            {
                if (Lookups == null)
                    return null;

                return string.Join(", ", Lookups.Select(l => l.LookupIndex));
            }
        }

        public TextBlock LookupsBlock
        {
            get
            {
                if (Lookups == null)
                    return null;

                TextBlock block = new TextBlock();

                for (int i = 0; i < Lookups.Count; i++)
                {
                    Hyperlink link = new Hyperlink(new Run(Lookups[i].LookupIndex.ToString()));
                    link.Command = ApplicationCommands.Find;
                    link.CommandParameter = Lookups[i];
                    block.Inlines.Add(link);

                    if (i != Lookups.Count - 1)
                        block.Inlines.Add(new Run(", "));
                }

                return block;
            }
        }
    }
}
