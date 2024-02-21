namespace OpenTypeInspector
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;

    public class LanguageItem
    {
        public GlyphTypefaceInspector.LookupTableInspector Inspector { get; }
        public int LanguageIndex { get; set; }
        public uint LanguageTag { get; set; }
        public ScriptItem Script { get; set; }

        public LanguageItem(GlyphTypefaceInspector.LookupTableInspector inspector, int languageIndex, TaggedObject language, IReadOnlyDictionary<ushort, FeatureItem> features)
        {
            Inspector = inspector;
            LanguageIndex = languageIndex;
            LanguageTag = language.Tag;

            if (inspector.GetRequiredFeatureIndex(language.Object) is ushort requiredFeatureIndex)
                RequiredFeature = features[requiredFeatureIndex];

            Features = new List<FeatureItem>();
            foreach (ushort featureIndex in inspector.EnumerateLanguageFeatures(language.Object))
                Features.Add(features[featureIndex]);
        }

        private string _languageTagString;
        public string LanguageTagString => _languageTagString ??= GlyphTypefaceInspector.ToTagString(LanguageTag);

        private string _knownName;
        public string KnownName => _knownName ??= GlyphTypefaceInspector.GetKnownLanguageName(LanguageTag);

        public FeatureItem RequiredFeature { get; set; }
        public IList<FeatureItem> Features { get; set; }

        public string FeaturesString
        {
            get
            {
                if (Features == null)
                    return null;

                return string.Join(", ", Features.Select(f => f.FeatureIndex));
            }
        }

        public TextBlock RequiredFeatureBlock
        {
            get
            {
                if (RequiredFeature == null)
                    return null;

                TextBlock block = new TextBlock();

                Hyperlink link = new Hyperlink(new Run(RequiredFeature.FeatureIndex.ToString()));
                link.Command = ApplicationCommands.Find;
                link.CommandParameter = RequiredFeature;
                block.Inlines.Add(link);

                return block;
            }
        }


        public TextBlock FeaturesBlock
        {
            get
            {
                if (Features == null)
                    return null;

                TextBlock block = new TextBlock();

                for (int i = 0; i < Features.Count; i++)
                {
                    Hyperlink link = new Hyperlink(new Run(Features[i].FeatureIndex.ToString()));
                    link.Command = ApplicationCommands.Find;
                    link.CommandParameter = Features[i];
                    block.Inlines.Add(link);

                    if (i != Features.Count - 1)
                        block.Inlines.Add(new Run(", "));
                }

                return block;
            }
        }

        public string GroupingHeader
        {
            get
            {
                if (Script == null)
                    return KnownName;

                string scriptName = Script.KnownScriptName;
                if (scriptName != null) scriptName = " (" + scriptName + ")";
                return $"{Script.ScriptIndex} Script: {Script.ScriptTagString}{scriptName}";
            }
        }
    }
}
