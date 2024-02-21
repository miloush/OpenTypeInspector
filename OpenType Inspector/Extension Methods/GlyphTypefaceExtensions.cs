using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using IEnumerable = System.Collections.IEnumerable;

namespace OpenTypeInspector
{
    internal static class GlyphTypefaceExtensions
    {
        //private static Type ExtensionLookupTable = typeof(GlyphTypeface).Assembly.GetType("MS.Internal.Shaping.ExtensionLookupTable");
        //private const ushort ExtensionLookupType = 7;

        private static CultureInfo enUS = CultureInfo.GetCultureInfo("en-US");
        public static string Localize(this IDictionary<CultureInfo, string> dict)
        {
            return dict[enUS];
        }

        private static readonly IReadOnlyDictionary<int, int> EmptyDictionary = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>(0));
        private static readonly IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> EmptyContextSubstitutionDictionary = new ReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int>(new Dictionary<GlyphTypefaceInspector.ContextSubtableType, int>(0));
        private static readonly IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> EmptyContextPositioningDictionary = new ReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int>(new Dictionary<GlyphTypefaceInspector.ContextSubtableType, int>(0));
        
        public static IReadOnlyDictionary<int, int> GetSubstitutionTableCounts(this GlyphTypeface typeface)
        {
            GlyphSubstitutionsInspector inspector = new GlyphTypefaceInspector(typeface).Substitutions;

            if (!inspector.TableIsPresent)
                return EmptyDictionary;

            ExpandableDictionary<int, int> counts = new ExpandableDictionary<int, int>(new Dictionary<int, int>(9));
           
            foreach (object lookup in inspector.EnumerateLookups())
            {
                foreach (object lookupSubtable in inspector.EnumerateLookupSubtables(lookup, true))
                {
                    SubstitutionLookupType lookupType = GlyphSubstitutionsInspector.GetLookupSubtableType(lookupSubtable);

                    int coverageCount = GetSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType);

                    counts[(int)lookupType] += coverageCount;
                }
            }

            return counts;
        }
        public static IReadOnlyDictionary<int, int> GetPositioningTableCounts(this GlyphTypeface typeface)
        {
            GlyphPositioningInspector inspector = new GlyphTypefaceInspector(typeface).Positioning;

            if (!inspector.TableIsPresent)
                return EmptyDictionary;

            ExpandableDictionary<int, int> counts = new ExpandableDictionary<int, int>(new Dictionary<int, int>(9));

            foreach (object lookup in inspector.EnumerateLookups())
            {
                foreach (object lookupSubtable in inspector.EnumerateLookupSubtables(lookup, true))
                {
                    PositioningLookupType lookupType = GlyphPositioningInspector.GetLookupSubtableType(lookupSubtable);

                    int coverageCount = GetSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType);

                    counts[(int)lookupType] += coverageCount;
                }
            }

            return counts;
        }

        public static IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetContextSubstitutionFormatCounts(this GlyphTypeface typeface, SubstitutionLookupType contextLookupType)
        {
            if (contextLookupType != SubstitutionLookupType.Context && contextLookupType != SubstitutionLookupType.Chaining)
                throw new ArgumentException($"{nameof(GlyphTypefaceInspector.ContextSubtableType)} is available only for {nameof(SubstitutionLookupType.Context)} and {nameof(SubstitutionLookupType.Chaining)} lookup tables.");

            GlyphSubstitutionsInspector inspector = new GlyphTypefaceInspector(typeface).Substitutions;

            if (!inspector.TableIsPresent)
                return EmptyContextSubstitutionDictionary;

            ExpandableDictionary<GlyphTypefaceInspector.ContextSubtableType, int> counts = new ExpandableDictionary<GlyphTypefaceInspector.ContextSubtableType, int>(new Dictionary<GlyphTypefaceInspector.ContextSubtableType, int>(3));

            foreach (object lookup in inspector.EnumerateLookups())
            {
                foreach (object lookupSubtable in inspector.EnumerateLookupSubtables(lookup, true))
                {
                    SubstitutionLookupType lookupType = GlyphSubstitutionsInspector.GetLookupSubtableType(lookupSubtable);

                    if (lookupType == contextLookupType)
                    {
                        GlyphTypefaceInspector.ContextSubtableType contextFormat = GlyphTypefaceInspector.LookupTableInspector.GetContextSubtableType(lookupSubtable);

                        counts[contextFormat] += GetContextSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType, contextFormat);
                    }
                }
            }

            return counts;
        }
        public static IReadOnlyDictionary<GlyphTypefaceInspector.ContextSubtableType, int> GetContextPositioningFormatCounts(this GlyphTypeface typeface, PositioningLookupType contextLookupType)
        {
            if (contextLookupType != PositioningLookupType.Context && contextLookupType != PositioningLookupType.Chaining)
                throw new ArgumentException($"{nameof(GlyphTypefaceInspector.ContextSubtableType)} is available only for {nameof(PositioningLookupType.Context)} and {nameof(PositioningLookupType.Chaining)} lookup tables.");

            GlyphPositioningInspector inspector = new GlyphTypefaceInspector(typeface).Positioning;

            if (!inspector.TableIsPresent)
                return EmptyContextPositioningDictionary;

            ExpandableDictionary<GlyphTypefaceInspector.ContextSubtableType, int> counts = new ExpandableDictionary<GlyphTypefaceInspector.ContextSubtableType, int>(new Dictionary<GlyphTypefaceInspector.ContextSubtableType, int>(3));

            foreach (object lookup in inspector.EnumerateLookups())
            {
                foreach (object lookupSubtable in inspector.EnumerateLookupSubtables(lookup, true))
                {
                    PositioningLookupType lookupType = GlyphPositioningInspector.GetLookupSubtableType(lookupSubtable);

                    if (lookupType == contextLookupType)
                    {
                        GlyphTypefaceInspector.ContextSubtableType contextFormat = GlyphTypefaceInspector.LookupTableInspector.GetContextSubtableType(lookupSubtable);

                        counts[contextFormat] += GetContextSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType, contextFormat);
                    }
                }
            }

            return counts;
        }

        private static int GetSubtableTotalCoverageCount(GlyphSubstitutionsInspector inspector, object lookupSubtable, SubstitutionLookupType lookupType)
        {
            dynamic subtableFriend = lookupSubtable.MakeFriend();

            switch (lookupType)
            {
                case SubstitutionLookupType.Single:
                case SubstitutionLookupType.Multiple:
                case SubstitutionLookupType.Alternate:
                    {
                        object coverageTable = subtableFriend.Coverage(inspector.Table);
                        return inspector.GetCoverageGlyphCount(coverageTable);
                    }

                case SubstitutionLookupType.Ligature:
                    {
                        int count = 0;
                        foreach (object ligatureSet in inspector.EnumerateLigatureSets(lookupSubtable))
                            count += (ushort)ligatureSet.MakeFriend().LigatureCount(inspector.Table);
                        return count;
                    }

                case SubstitutionLookupType.Context:
                case SubstitutionLookupType.Chaining:
                    {                     
                        GlyphTypefaceInspector.ContextSubtableType contextFormat = GlyphTypefaceInspector.LookupTableInspector.GetContextSubtableType(lookupSubtable);
                        return GetContextSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType, contextFormat);
                    }

                case SubstitutionLookupType.ReverseChaining:
                    {
                        if (subtableFriend.Format(inspector.Table) == 1)
                            return 1;

                        return 1;
                    }
                
                default:
                    return -1;
            }
        }
        private static int GetContextSubtableTotalCoverageCount(GlyphSubstitutionsInspector inspector, object lookupSubtable, SubstitutionLookupType lookpType, GlyphTypefaceInspector.ContextSubtableType contextFormat)
        {
            switch (contextFormat)
            {
                case GlyphTypefaceInspector.ContextSubtableType.GlyphBased:
                    {
                        int count = 0;
                        foreach (object subRuleSet in inspector.EnumerateContextSubRuleSets(lookupSubtable))
                            count += (ushort)subRuleSet.MakeFriend().RuleCount(inspector.Table);

                        return count;
                    }

                case GlyphTypefaceInspector.ContextSubtableType.ClassBased:
                    {
                        int count = 0;
                        foreach (object subClassSet in inspector.EnumerateContextSubClassSets(lookupSubtable))
                        {
                            dynamic subClassSetFriend = subClassSet.MakeFriend();

                            if (subClassSetFriend.IsNull)
                                continue;

                            count += (ushort)subClassSetFriend.RuleCount(inspector.Table);
                        }
                        return count;
                    }

                case GlyphTypefaceInspector.ContextSubtableType.CoverageBased:
                    {
                        if (lookpType == SubstitutionLookupType.Context)
                            return lookupSubtable.MakeFriend().SubstCount(inspector.Table);
                        else
                            return 1; // inspector.GSUBTable.MakeFriend().GetUShort(lookupSubtable.MakeFriend().offset + 14);
                    }

                default:
                    return 1;
            }
        }

        private static int GetSubtableTotalCoverageCount(GlyphPositioningInspector inspector, object lookupSubtable, PositioningLookupType lookupType)
        {
            dynamic subtableFriend = lookupSubtable.MakeFriend();

            switch (lookupType)
            {
                case PositioningLookupType.SingleAdjustment:
                case PositioningLookupType.PairAdjustment:
                case PositioningLookupType.CursiveAttachment:
                    {
                        object coverageTable = subtableFriend.Coverage(inspector.Table);
                        return inspector.GetCoverageGlyphCount(coverageTable);
                    }

                case PositioningLookupType.MarkToBaseAttachment: // Mark+Base Coverage
                case PositioningLookupType.MarkToLigatureAttachment: // Mark+Ligature Coverage
                    {
                        object coverageTable = subtableFriend.MarkCoverage(inspector.Table);
                        return inspector.GetCoverageGlyphCount(coverageTable);
                    }
                case PositioningLookupType.MarkToMarkAttachment: // Mark1+Mark2 Coverage
                    {
                        object coverageTable = subtableFriend.Mark1Coverage(inspector.Table);
                        return inspector.GetCoverageGlyphCount(coverageTable);
                    }

                case PositioningLookupType.Context:
                case PositioningLookupType.Chaining:
                    {
                        GlyphTypefaceInspector.ContextSubtableType contextFormat = GlyphTypefaceInspector.LookupTableInspector.GetContextSubtableType(lookupSubtable);
                        return GetContextSubtableTotalCoverageCount(inspector, lookupSubtable, lookupType, contextFormat);
                    }

                default:
                    return -1;
            }
        }
        private static int GetContextSubtableTotalCoverageCount(GlyphPositioningInspector inspector, object lookupSubtable, PositioningLookupType lookpType, GlyphTypefaceInspector.ContextSubtableType contextFormat)
        {
            switch (contextFormat)
            {
                case GlyphTypefaceInspector.ContextSubtableType.GlyphBased:
                    {
                        int count = 0;
                        foreach (object subRuleSet in inspector.EnumerateContextSubRuleSets(lookupSubtable))
                            count += (ushort)subRuleSet.MakeFriend().RuleCount(inspector.Table);

                        return count;
                    }

                case GlyphTypefaceInspector.ContextSubtableType.ClassBased:
                    {
                        int count = 0;
                        foreach (object subClassSet in inspector.EnumerateContextSubClassSets(lookupSubtable))
                        {
                            dynamic subClassSetFriend = subClassSet.MakeFriend();

                            if (subClassSetFriend.IsNull)
                                continue;

                            count += (ushort)subClassSetFriend.RuleCount(inspector.Table);
                        }
                        return count;
                    }

                case GlyphTypefaceInspector.ContextSubtableType.CoverageBased:
                    {
                        if (lookpType == PositioningLookupType.Context)
                            return lookupSubtable.MakeFriend().SubstCount(inspector.Table);
                        else
                            return 1; // inspector.GSUBTable.MakeFriend().GetUShort(lookupSubtable.MakeFriend().offset + 14);
                    }

                default:
                    return 1;
            }
        }

        public static FontFamily ToFontFamily(this GlyphTypeface typeface)
        {
            return new FontFamily(typeface.FontUri, typeface.FamilyNames[enUS]);
        }
    }
}
