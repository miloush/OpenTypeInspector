using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenTypeInspector
{
    public class FontItem
    {
        public class FormatsCount : IComparable
        {
            private IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> _counts;

            internal FormatsCount(IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> counts)
            {
                _counts = counts;
            }

            public int CompareTo(object other)
            {
                return this.Total.CompareTo(((FormatsCount)other).Total);
            }

            public int Total
            {
                get { return _counts.Values.Sum(); }
            }

            public override string ToString()
            {
                return string.Join("/",
                    from format in Enum.GetValues(typeof(GlyphTypefaceInspector.ContextSubtableType)).Cast<GlyphTypefaceInspector.ContextSubtableType>()
                    orderby (int)format
                    select _counts.ContainsKey(format) ? _counts[format] : 0);
            }
        }

        public class DefinitionCounts
        {
            private readonly FontItem _item;

            public DefinitionCounts(FontItem item)
            {
                _item = item;
            }

            private int? _attachmentListsCount;

            public int AttachmentListsCount { get { return (int)(_attachmentListsCount ?? (_attachmentListsCount = _item._inspector.Definitions.GetGlyphWithAttachmentPointsCount())); } }
        }

        public abstract class LookupTableCounts
        {
            protected GlyphTypefaceInspector.LookupTableInspector Inspector;

            private IReadOnlyDictionary<ushort, LookupItem> _lookupItems;
            private IReadOnlyDictionary<ushort, FeatureItem> _featureItems;
            private IReadOnlyList<ScriptItem> _scriptItems;

            private IReadOnlyDictionary<int, int> _counts;
            private IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> _contextFormatCounts;
            private IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> _chainingFormatCounts;

            private WeakReference<LanguagesView> _scriptsView = new WeakReference<LanguagesView>(null);
            private WeakReference<FeaturesView> _featuresView = new WeakReference<FeaturesView>(null);

            private int? _scriptCount;
            private int? _featureCount;
            private int? _lookupCount;

            public int ScriptCount { get { return _scriptCount ??= Inspector.GetScriptCount(); } }
            public int FeatureCount { get { return _featureCount ??= Inspector.GetFeatureCount(); } }
            public int LookupCount { get { return _lookupCount ??= Inspector.GetLookupCount(); } }

            public LookupTableCounts(GlyphTypefaceInspector.LookupTableInspector inspector)
            {
                Inspector = inspector;
            }

            public FormatsCount ContextualSubstitutionFormatCounts => new FormatsCount(_contextFormatCounts ??= Inspector.Typeface.GetContextSubstitutionFormatCounts(SubstitutionLookupType.Context));
            public FormatsCount ChainingSubstitutionFormatCounts => new FormatsCount(_chainingFormatCounts ??= Inspector.Typeface.GetContextSubstitutionFormatCounts(SubstitutionLookupType.Chaining));

            public IReadOnlyDictionary<int, int> Counts { get { return _counts ??= Inspector.Typeface.GetSubstitutionTableCounts(); } }

            protected abstract IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetContextFormatCounts();
            protected abstract IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetChainingFormatCounts();
            protected abstract IReadOnlyDictionary<int, int> GetTableCounts();

            protected IReadOnlyDictionary<ushort, LookupItem> LookupItems => _lookupItems ??= GetLookupItems();
            protected IReadOnlyDictionary<ushort, FeatureItem> FeatureItems => _featureItems ??= GetFeatureItems();
            protected IReadOnlyList<ScriptItem> ScriptItems => _scriptItems ??= GetScriptItems();

            private IReadOnlyDictionary<ushort, LookupItem> GetLookupItems()
            {
                SortedDictionary<ushort, LookupItem> items = new SortedDictionary<ushort, LookupItem>();

                ushort lookupIndex = 0;
                foreach (var lookup in Inspector.EnumerateLookups())
                {
                    items[lookupIndex] = new LookupItem(Inspector, lookup) { LookupIndex = lookupIndex };
                    lookupIndex++;
                }

                return items;
            }
            private IReadOnlyDictionary<ushort, FeatureItem> GetFeatureItems()
            {
                SortedDictionary<ushort, FeatureItem> items = new SortedDictionary<ushort, FeatureItem>();

                ushort featureIndex = 0;
                foreach (var feature in Inspector.EnumerateFeatures())
                {
                    items[featureIndex] = new FeatureItem(Inspector, featureIndex, feature, LookupItems);
                    featureIndex++;
                }

                return items;
            }
            private IReadOnlyList<ScriptItem> GetScriptItems()
            {
                List<ScriptItem> items = new List<ScriptItem>();

                int scriptIndex = 0;
                foreach (TaggedObject script in Inspector.EnumerateScripts())
                    items.Add(new ScriptItem(Inspector, scriptIndex++, script, FeatureItems));

                return items;
            }

            public LanguagesView LanguagesView { get { return GetOrCreate(_scriptsView, () => new LanguagesView(ScriptItems)); } }
            public FeaturesView FeaturesView { get { return GetOrCreate(_featuresView, () => new FeaturesView(FeatureItems.Values)); } }
        }

        public class SubstitutionCounts : LookupTableCounts
        {
            protected GlyphSubstitutionsInspector SubstitutionsInspector => (GlyphSubstitutionsInspector)Inspector;

            private WeakReference<SinglesView> _singlesView = new WeakReference<SinglesView>(null);
            private WeakReference<MultiplesView> _multiplesView = new WeakReference<MultiplesView>(null);
            private WeakReference<AlternatesView> _alternatesView = new WeakReference<AlternatesView>(null);
            private WeakReference<LigaturesView> _ligaturesView = new WeakReference<LigaturesView>(null);
            private WeakReference<ContextView> _contextsView = new WeakReference<ContextView>(null);

            public SubstitutionCounts(GlyphSubstitutionsInspector inspector) : base(inspector) { }

            protected override IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetContextFormatCounts() => Inspector.Typeface.GetContextSubstitutionFormatCounts(SubstitutionLookupType.Context);
            protected override IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetChainingFormatCounts() => Inspector.Typeface.GetContextSubstitutionFormatCounts(SubstitutionLookupType.Chaining);
            protected override IReadOnlyDictionary<int, int> GetTableCounts() => Inspector.Typeface.GetSubstitutionTableCounts();

            public SinglesView SinglesView { get { return GetOrCreate(_singlesView, () => new SinglesView(SubstitutionsInspector)); } }
            public MultiplesView MultiplesView { get { return GetOrCreate(_multiplesView, () => new MultiplesView(SubstitutionsInspector)); } }
            public AlternatesView AlternatesView { get { return GetOrCreate(_alternatesView, () => new AlternatesView(SubstitutionsInspector)); } }
            public LigaturesView LigaturesView { get { return GetOrCreate(_ligaturesView, () => new LigaturesView(SubstitutionsInspector)); } }
            public ContextView ContextsView { get { return GetOrCreate(_contextsView, () => new ContextView(SubstitutionsInspector)); } }
        }
        public class PositioningCounts : LookupTableCounts
        {
            public PositioningCounts(GlyphPositioningInspector inspector) : base(inspector) { }

            protected override IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetContextFormatCounts() => Inspector.Typeface.GetContextPositioningFormatCounts(PositioningLookupType.Context);
            protected override IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetChainingFormatCounts() => Inspector.Typeface.GetContextPositioningFormatCounts(PositioningLookupType.Chaining);
            protected override IReadOnlyDictionary<int, int> GetTableCounts() => Inspector.Typeface.GetPositioningTableCounts();
        }

        private readonly GlyphTypeface _typeface;
        private readonly GlyphTypefaceInspector _inspector;
        private readonly DefinitionCounts _definitions;
        private readonly SubstitutionCounts _substitutions;
        private readonly PositioningCounts _positioning;

        private string _familyName;
        private string _faceName;
        private FileInfo? _fileInfo;

        private WeakReference<GlyphsView> _glyphsView = new WeakReference<GlyphsView>(null);
        private WeakReference<CharactersView> _charactersView = new WeakReference<CharactersView>(null);

        public GlyphTypeface Typeface { get { return _typeface; } }
        public GlyphTypefaceInspector TypefaceInspector { get { return _inspector; } }
        public DefinitionCounts Definitions => _definitions;
        public SubstitutionCounts Substitutions => _substitutions;
        public PositioningCounts Positioning => _positioning;

        public string FamilyName { get { return _familyName ?? (_familyName = _typeface.FamilyNames.Localize()); } }
        public string FaceName { get { return _faceName ?? (_faceName = _typeface.FaceNames.Localize()); } }
        public FileInfo FileInfo { get { return _fileInfo ??= new FileInfo(Typeface.FontUri.LocalPath); } }

        public GlyphsView GlyphsView { get { return GetOrCreate(_glyphsView, () => new GlyphsView(this)); } }
        public CharactersView CharactersView { get { return GetOrCreate(_charactersView, () => new CharactersView(this)); } }

        public FontItem(GlyphTypeface typeface)
        {
            _typeface = typeface;
            _inspector = new GlyphTypefaceInspector(typeface);

            _definitions = new DefinitionCounts(this);
            _substitutions = new SubstitutionCounts(_inspector.Substitutions);
            _positioning = new PositioningCounts(_inspector.Positioning);
        }

        private static T GetOrCreate<T>(WeakReference<T> reference, Func<T> creator) where T : class
        {
            T target;

            if (!reference.TryGetTarget(out target))
            {
                target = creator();
                reference.SetTarget(target);
            }

            return target;
        }

        //internal object FindSubstutionLookup(ushort lookupIndex)
        //{
        //    object lookup;
        //    try { lookup = _inspector.Substitutions.GetLookup(lookupIndex); }
        //    catch (ArgumentOutOfRangeException) { return null; }

        //    lookup = _inspector.Substitutions.EnumerateLookupSubtables(lookup).OfType<object>().FirstOrDefault();
        //    if (lookup == null)
        //        return null;

        //    switch (GlyphTypefaceInspector.SubstitutionsInspector.GetLookupSubtableType(lookup))
        //    {
        //        case GlyphTypefaceInspector.SubstitutionLookupType.Alternate:
        //            return AlternatesView;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.Context:
        //            return ContextsView;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.Chaining:
        //            return null;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.Ligature:
        //            return LigaturesView;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.Multiple:
        //            return MultiplesView;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.ReverseChaining:
        //            return null;

        //        case GlyphTypefaceInspector.SubstitutionLookupType.Single:
        //            return SinglesView;

        //        default:
        //            return null;
        //    }
    }
}
