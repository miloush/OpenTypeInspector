namespace OpenTypeInspector
{
    using System.Collections.Generic;

    public class ScriptItem
    {
        public GlyphTypefaceInspector.LookupTableInspector Inspector { get; }
        public int ScriptIndex { get; set; }
        public uint ScriptTag { get; set; }

        public LanguageItem DefaultLanguage { get; set; }
        public IList<LanguageItem> Languages { get; set; }

        public IEnumerable<LanguageItem> AllLanguages
        {
            get
            {
                if (DefaultLanguage is LanguageItem defaultItem)
                    yield return defaultItem;

                foreach (LanguageItem item in Languages)
                    yield return item;
            }
        }

        public ScriptItem(GlyphTypefaceInspector.LookupTableInspector inspector)
        {
            Inspector = inspector;
            ScriptIndex = -1;
        }
        public ScriptItem(GlyphTypefaceInspector.LookupTableInspector inspector, int scriptIndex, TaggedObject script, IReadOnlyDictionary<ushort, FeatureItem> features)  : this(inspector)
        {
            ScriptIndex = scriptIndex;
            ScriptTag = script.Tag;

            if (Inspector.GetDefaultLanguage(script.Object) is TaggedObject defaultLanguage)
                DefaultLanguage = new LanguageItem(Inspector, -1, defaultLanguage, features) { Script = this };

            Languages = new List<LanguageItem>();
            int languageIndex = 0;
            foreach (TaggedObject language in Inspector.EnumerateLanguages(script.Object))
                Languages.Add(new LanguageItem(Inspector, languageIndex++, language, features) { Script = this });
        }

        private string _scriptTagString;
        public string ScriptTagString => _scriptTagString ??= GlyphTypefaceInspector.ToTagString(ScriptTag);

        private string _knownName;
        public string KnownScriptName => _knownName ??= GlyphTypefaceInspector.GetKnownScriptName(ScriptTag);
    }
}
