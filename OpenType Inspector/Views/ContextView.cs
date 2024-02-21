namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Converters;
    using System.Windows.Threading;

    // TODO: support positionining context, this is only substitution

    public class ContextView : ViewBase<ContextItem>
    {
        protected GlyphTypeface Typeface => Inspector.Typeface;
        protected GlyphSubstitutionsInspector Inspector { get; }

        private volatile bool _loaded;

        private Dispatcher _dispatcher;
        private ObservableCollection<ContextItem> _items;
        private ListCollectionView _itemsView;

        private IList<ushort> _filter;
        private static UShortIListConverter _filterConverter = new UShortIListConverter();

        public override IReadOnlyList<ContextItem> Items
        {
            get
            {
                InitializeItems();
                return _items;
            }
        }
        public CollectionView ItemsView
        {
            get
            {
                InitializeItems();
                return _itemsView;
            }
        }

        public string Filter
        {
            get
            {
                if (_filter == null)
                    return null;

                return _filterConverter.ConvertToString(_filter);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (_filter == null)
                        return;

                    _filter = null;
                }
                else
                    _filter = (IList<ushort>)_filterConverter.ConvertFromString(value);

                _itemsView.Refresh();
            }
        }

        public ContextView(GlyphSubstitutionsInspector inspector)
        {
            Inspector = inspector;
            _dispatcher = Dispatcher.CurrentDispatcher;

            _items = new ObservableCollection<ContextItem>();
            _itemsView = new ListCollectionView(_items);
            _itemsView.GroupDescriptions.Add(new PropertyGroupDescription("GroupingHeader"));
            _itemsView.Filter = o => ((ContextItem)o).Matches(_filter);
        }

        private void InitializeItems()
        {
            if (_loaded)
                return;

            lock (this)
            {
                if (_loaded)
                    return;
                else
                    _loaded = true;

                new Thread(PopulateItems) { IsBackground = true }.Start();
            }
        }
        private void PopulateItems()
        {
            ushort lookupIndex = 0;
            foreach (object lookup in Inspector.EnumerateLookups())
            {
                ushort lookupSubtableIndex = 0;
                foreach (object lookupSubtable in Inspector.EnumerateLookupSubtables(lookup, true))
                {
                    SubstitutionLookupType substitutionType = GlyphSubstitutionsInspector.GetLookupSubtableType(lookupSubtable);
                    if (substitutionType == SubstitutionLookupType.Context)
                    {
                        GlyphTypefaceInspector.ContextSubtableType contextFormat = GlyphTypefaceInspector.LookupTableInspector.GetContextSubtableType(lookupSubtable);
                        object primaryCoverage = Inspector.GetLookupSubtablePrimaryCoverage(lookupSubtable);

                        switch (contextFormat)
                        {
                            case GlyphTypefaceInspector.ContextSubtableType.GlyphBased:
                                {
                                    ushort setIndex = 0;
                                    foreach (ushort firstGlyph in Inspector.EnumerateCoverageGlyphs(primaryCoverage))
                                    {
                                        object ruleSet = Inspector.GetContextSubRuleSet(lookupSubtable, setIndex);

                                        foreach (object subRule in Inspector.EnumerateContextSubGlyphRules(ruleSet))
                                            PopulateItems(new[] { firstGlyph }, from glyph in Inspector.EnumerateContextSubRuleGlyphs(subRule)
                                                                                select new ushort[] { glyph }, lookupIndex, lookupSubtableIndex, contextFormat, subRule, setIndex);
                                        setIndex++;
                                    }
                                }
                                break;

                            case GlyphTypefaceInspector.ContextSubtableType.ClassBased:
                                {
                                    object classDef = Inspector.GetContextClassDefinition(lookupSubtable);
                                    var classGlyphs = Inspector.EnumerateClassGlyphs(classDef).ToLookup(gc => gc.Item2);

                                    var setIndices = Inspector.EnumerateCoverageGlyphs(primaryCoverage).Select(g => Inspector.GetGlyphClass(classDef, g)).Distinct();

                                    foreach (ushort setIndex in setIndices)
                                    {
                                        object classSet = Inspector.GetContextSubClassSet(lookupSubtable, setIndex);

                                        ushort ruleIndex = 0;
                                        foreach (object subRule in Inspector.EnumerateContextSubClassRules(classSet))
                                        {
                                            PopulateItems(classGlyphs[setIndex].Select(gc => gc.Item1), from classIndex in Inspector.EnumerateContextSubRuleClasses(subRule)
                                                                                                        select classIndex == 0 ? null : classGlyphs[classIndex].Select(gc => gc.Item1), lookupIndex, lookupSubtableIndex, contextFormat, subRule, setIndex, ruleIndex);
                                            ruleIndex++;
                                        }
                                    }
                                }
                                break;

                            case GlyphTypefaceInspector.ContextSubtableType.CoverageBased:
                                PopulateItems(Inspector.EnumerateCoverageGlyphs(primaryCoverage), from object coverage in Inspector.EnumerateContextCoverages(lookupSubtable)
                                                                                                               select Inspector.EnumerateCoverageGlyphs(coverage), lookupIndex, lookupSubtableIndex, contextFormat, lookupSubtable);
                                break;

                            default:
                                break;
                        }
                    }
                    lookupSubtableIndex++;
                }
                lookupIndex++;
            }
        }

        private void PopulateItems(IEnumerable<ushort> firstGlyphs, IEnumerable<IEnumerable<ushort>> glyphs, ushort lookupIndex, ushort lookupSubtableIndex, GlyphTypefaceInspector.ContextSubtableType contextFormat, object substSource, ushort setIndex = 0, ushort ruleIndex = 0)
        {
            ContextItem item = new ContextItem(Typeface);

            item.LookupIndex = lookupIndex;
            item.LookupSubtableIndex = lookupSubtableIndex;
            item.Format = contextFormat;
            item.SetIndex = setIndex;
            item.RuleIndex = ruleIndex;

            item.Substitutions = Inspector.EnumerateContextualLookupIndices(substSource).ToArray();
            item.ContextComponents = new[] { firstGlyphs.ToArray() }.Concat(glyphs.Select(g => g == null ? null : g.ToArray())).ToArray();

            _dispatcher.Invoke(() => _items.Add(item), DispatcherPriority.Background);
        }
    }
}
