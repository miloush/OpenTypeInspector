namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media;

    public partial class GlyphTypefaceInspector
    {
        public abstract class LookupTableInspector : TableInspector
        {
            protected static readonly Type ExtensionLookupTable;

            protected static readonly Type ContextSubtable;
            protected static readonly Type ChainingSubtable;
            protected static readonly Type ReverseChainingSubtable;

            protected static readonly Type GlyphContextSubtable;
            protected static readonly Type ClassContextSubtable;
            protected static readonly Type CoverageContextSubtable;

            protected static readonly Type GlyphChainingSubtable;
            protected static readonly Type ClassChainingSubtable;
            protected static readonly Type CoverageChainingSubtable;

            static LookupTableInspector()
            {
                Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

                ExtensionLookupTable = PresentationCore.GetType("MS.Internal.Shaping.ExtensionLookupTable");

                ContextSubtable = PresentationCore.GetType("MS.Internal.Shaping.ContextSubtable");
                ChainingSubtable = PresentationCore.GetType("MS.Internal.Shaping.ChainingSubtable");
                ReverseChainingSubtable = PresentationCore.GetType("MS.Internal.Shaping.ReverseChainingSubtable");

                GlyphContextSubtable = PresentationCore.GetType("MS.Internal.Shaping.GlyphContextSubtable");
                ClassContextSubtable = PresentationCore.GetType("MS.Internal.Shaping.ClassContextSubtable");
                CoverageContextSubtable = PresentationCore.GetType("MS.Internal.Shaping.CoverageContextSubtable");

                GlyphChainingSubtable = PresentationCore.GetType("MS.Internal.Shaping.GlyphChainingSubtable");
                ClassChainingSubtable = PresentationCore.GetType("MS.Internal.Shaping.ClassChainingSubtable");
                CoverageChainingSubtable = PresentationCore.GetType("MS.Internal.Shaping.CoverageChainingSubtable");
            }

            protected LookupTableInspector(GlyphTypeface typeface) : base(typeface) { }

            protected abstract Type TableHeaderType { get; }
            public IEnumerable<TaggedObject> EnumerateScripts()
            {
                if (!TableIsPresent)
                    yield break;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object scriptList = headerFriend.GetScriptList(Table);
                dynamic scriptListFriend = scriptList.MakeFriend();

                ushort scriptCount = scriptListFriend.GetScriptCount(Table);
                for (ushort scriptIndex = 0; scriptIndex < scriptCount; scriptIndex++)
                    yield return new TaggedObject(scriptListFriend.GetScriptTag(Table, scriptIndex), scriptListFriend.GetScriptTable(Table, scriptIndex));
            }
            public IEnumerable<TaggedObject> EnumerateScripts(params ushort[] scriptIndices)
            {
                if (!TableIsPresent)
                    throw new InvalidOperationException();

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object scriptList = headerFriend.GetScriptList(Table);
                dynamic scriptListFriend = scriptList.MakeFriend();

                ushort scriptCount = scriptListFriend.GetScriptCount(Table);

                for (int i = 0; i < scriptIndices.Length; i++)
                {
                    ushort scriptIndex = scriptIndices[i];
                    if (scriptIndex < 0 || scriptIndex >= scriptCount)
                        throw new ArgumentOutOfRangeException(nameof(scriptIndices));

                    yield return new TaggedObject(scriptListFriend.GetScriptTag(Table, scriptIndex), scriptListFriend.GetScriptTable(Table, scriptIndex));
                }
            }
            public TaggedObject GetScript(ushort scriptIndex)
            {
                if (!TableIsPresent)
                    throw new InvalidOperationException();

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object scriptList = headerFriend.GetScriptList(Table);
                dynamic scriptListFriend = scriptList.MakeFriend();

                ushort scriptCount = scriptListFriend.GetScriptCount(Table);
                if (scriptIndex < 0 || scriptIndex >= scriptCount)
                    throw new ArgumentOutOfRangeException(nameof(scriptIndex));
                
                return new TaggedObject(scriptListFriend.GetScriptTag(Table, scriptIndex), scriptListFriend.GetScriptTable(Table, scriptIndex));
            }
            public int GetScriptCount()
            {
                if (!TableIsPresent)
                    return 0;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object scriptList = headerFriend.GetScriptList(Table);
                dynamic scriptListFriend = scriptList.MakeFriend();

                return scriptListFriend.GetScriptCount(Table);
            }
            public IEnumerable<ushort> EnumerateScriptIndices()
            {
                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object scriptList = headerFriend.GetScriptList(Table);
                dynamic scriptListFriend = scriptList.MakeFriend();

                ushort scriptCount = scriptListFriend.GetScriptCount(Table);
                for (ushort scriptIndex = 0; scriptIndex < scriptCount; scriptIndex++)
                    yield return scriptIndex;
            }

            public IEnumerable<TaggedObject> EnumerateLanguages(object script)
            {
                dynamic scriptFriend = script.MakeFriend();

                if (scriptFriend.IsNull)
                    yield break;

                ushort languageCount = scriptFriend.GetLangSysCount(Table);
                for (ushort languageIndex = 0; languageIndex < languageCount; languageIndex++)
                    yield return new TaggedObject(scriptFriend.GetLangSysTag(Table, languageIndex), scriptFriend.GetLangSysTable(Table, languageIndex));
            }
            public IEnumerable<TaggedObject> EnumerateLanguages(object script, params ushort[] languageIndices)
            {
                dynamic scriptFriend = script.MakeFriend();

                if (scriptFriend.IsNull)
                    yield break;

                ushort languageCount = scriptFriend.GetLangSysCount(Table);

                for (int index = 0; index < languageIndices.Length; index++)
                {
                    ushort languageIndex = languageIndices[index];
                    if (languageIndex < 0 || languageIndex >= languageCount)
                        throw new ArgumentOutOfRangeException(nameof(languageIndices));

                    yield return new TaggedObject(scriptFriend.GetLangSysTag(Table, languageIndex), scriptFriend.GetLangSysTable(Table, languageIndex));
                }

            }
            public TaggedObject? GetDefaultLanguage(object script)
            {
                dynamic scriptFriend = script.MakeFriend();

                if (!scriptFriend.IsNull && scriptFriend.IsDefaultLangSysExists(Table))
                    return new TaggedObject(dflt, scriptFriend.GetDefaultLangSysTable(Table));

                return null;
            }
            public TaggedObject GetLanguage(object script, int languageIndex)
            {
                dynamic scriptFriend = script.MakeFriend();

                if (scriptFriend.IsNull)
                    throw new ArgumentException(nameof(script));

                ushort languageCount = scriptFriend.GetLangSysCount(Table);
                if (languageIndex < 0 || languageIndex >= languageCount)
                    throw new ArgumentOutOfRangeException(nameof(languageIndex));

                return new TaggedObject(scriptFriend.GetLangSysTag(Table, languageIndex), scriptFriend.GetLangSysTable(Table, languageIndex));
            }
            public int GetLanguageCount(object script)
            {
                dynamic scriptFriend = script.MakeFriend();

                if (scriptFriend.IsNull)
                    return 0;

                int languageCount = scriptFriend.IsDefaultLangSysExists(Table) ? 1 : 0;
                languageCount += scriptFriend.GetLangSysCount(Table);

                return languageCount;
            }
            
            public IEnumerable<ushort> EnumerateLanguageFeatures(object language)
            {
                dynamic languageFriend = language.MakeFriend();

                if (languageFriend.IsNull)
                    yield break;

                ushort featureCount = languageFriend.FeatureCount(Table);
                for (ushort featureIndex = 0; featureIndex < featureCount; featureIndex++)
                    yield return languageFriend.GetFeatureIndex(Table, featureIndex);
            }
            public ushort? GetRequiredFeatureIndex(object language)
            {
                dynamic languageFriend = language.MakeFriend();

                if (languageFriend.IsNull)
                    return null;

                ushort featureIndex = TableFriend.GetUShort(languageFriend.offset + 2);

                if (featureIndex == ushort.MaxValue)
                    return null;
                else
                    return featureIndex;
            }

            public IEnumerable<TaggedObject> EnumerateFeatures()
            {
                if (!TableIsPresent)
                    yield break;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object featureList = headerFriend.GetFeatureList(Table);
                dynamic featureListFriend = featureList.MakeFriend();

                int featureCount = featureListFriend.FeatureCount(Table);
                for (ushort featureIndex = 0; featureIndex < featureCount; featureIndex++)
                    yield return new TaggedObject(featureListFriend.FeatureTag(Table, featureIndex), featureListFriend.FeatureTable(Table, featureIndex));
            }
            public IEnumerable<TaggedObject> EnumerateFeatures(params ushort[] featureIndices)
            {
                if (!TableIsPresent)
                    throw new InvalidOperationException();

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object featureList = headerFriend.GetFeatureList(Table);
                dynamic featureListFriend = featureList.MakeFriend();

                int featureCount = featureListFriend.FeatureCount(Table);

                for (int i = 0; i < featureIndices.Length; i++)
                {
                    ushort featureIndex = featureIndices[i];
                    if (featureIndex < 0 || featureIndex >= featureCount)
                        throw new ArgumentOutOfRangeException(nameof(featureIndices));

                    yield return new TaggedObject(featureListFriend.FeatureTag(Table, featureIndex), featureListFriend.FeatureTable(Table, featureIndex));
                }
            }
            public TaggedObject GetFeature(ushort featureIndex)
            {
                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object featureList = headerFriend.GetFeatureList(Table);
                dynamic featureListFriend = featureList.MakeFriend();

                int featureCount = featureListFriend.FeatureCount(Table);
                if (featureIndex >= featureCount)
                    throw new ArgumentOutOfRangeException(nameof(featureIndex));

                return new TaggedObject(featureListFriend.FeatureTag(Table, featureIndex), featureListFriend.FeatureTable(Table, featureIndex));
            }
            public int GetFeatureCount()
            {
                if (!TableIsPresent)
                    return 0;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object featureList = headerFriend.GetFeatureList(Table);
                dynamic featureListFriend = featureList.MakeFriend();

                return featureListFriend.FeatureCount(Table);
            }
            public IEnumerable<ushort> EnumerateFeatureLookupIndices(object feature)
            {
                dynamic featureFriend = feature.MakeFriend();

                if (featureFriend.IsNull)
                    yield break;

                int lookupCount = featureFriend.LookupCount(Table);
                for (ushort lookupIndex = 0; lookupIndex < lookupCount; lookupIndex++)
                    yield return featureFriend.LookupIndex(Table, lookupIndex);
            }

            public IEnumerable EnumerateLookups()
            {
                if (!TableIsPresent)
                    yield break;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object lookupList = headerFriend.GetLookupList(Table);
                dynamic lookupListFriend = lookupList.MakeFriend();

                int lookupCount = lookupListFriend.LookupCount(Table);
                for (ushort lookupIndex = 0; lookupIndex < lookupCount; lookupIndex++)
                    yield return lookupListFriend.Lookup(Table, lookupIndex);
            }
            public object GetLookup(ushort lookupIndex)
            {
                if (!TableIsPresent)
                    throw new InvalidOperationException();

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object lookupList = headerFriend.GetLookupList(Table);
                dynamic lookupListFriend = lookupList.MakeFriend();

                int lookupCount = lookupListFriend.LookupCount(Table);
                if (lookupIndex >= lookupCount)
                    throw new ArgumentOutOfRangeException("lookupIndex");

                return lookupListFriend.Lookup(Table, lookupIndex);
            }
            public int GetLookupCount()
            {
                if (!TableIsPresent)
                    return 0;

                object header = Activator.CreateInstance(TableHeaderType);
                dynamic headerFriend = header.MakeFriend();

                object lookupList = headerFriend.GetLookupList(Table);
                dynamic lookupListFriend = lookupList.MakeFriend();

                return lookupListFriend.LookupCount(Table);
            }

            protected ushort GetLookupType(object lookup)
            {
                dynamic lookupFriend = lookup.MakeFriend();
                return (ushort)lookupFriend.LookupType();
            }
            public object GetLookupTypeEnum(object lookup) //, bool resolveExtension)
            {
                ushort lookupType = GetLookupType(lookup);
                //if (resolveExtension && IsExtensionSubtableType(lookupType))
                //{
                //    dynamic lookupFriend = lookup.MakeFriend();
                //    int subtableOffset = lookupFriend.SubtableOffset(Table, subtableIndex);

                //    object extension = ExtensionLookupTable.Instantiate(subtableOffset);
                //    dynamic extensionFriend = extension.MakeFriend();

                //    lookupType = (ushort)extensionFriend.LookupType(Table);
                //}

                return GetLookupTypeEnum(lookupType);
            }
            protected abstract object GetLookupTypeEnum(ushort lookupType);
            public IEnumerable EnumerateLookupSubtables(object lookup, bool resolveContext = false)
            {
                dynamic lookupFriend = lookup.MakeFriend();
                ushort subtableCount = lookupFriend.SubTableCount();

                for (ushort subtableIndex = 0; subtableIndex < subtableCount; subtableIndex++)
                    yield return GetLookupSubtable(lookup, subtableIndex, resolveContext);
            }
            public object GetLookupSubtable(object lookup, ushort subtableIndex, bool resolveContext = false)
            {
                dynamic lookupFriend = lookup.MakeFriend();

                ushort subtableCount = lookupFriend.SubTableCount();
                if (subtableIndex >= subtableCount)
                    throw new ArgumentOutOfRangeException("subtableIndex");

                int subtableOffset = lookupFriend.SubtableOffset(Table, subtableIndex);
                ushort lookupType = (ushort)lookupFriend.LookupType();

                if (IsExtensionSubtableType(lookupType))
                {
                    object extension = ExtensionLookupTable.Instantiate(subtableOffset);
                    dynamic extensionFriend = extension.MakeFriend();

                    lookupType = (ushort)extensionFriend.LookupType(Table);
                    subtableOffset = extensionFriend.LookupSubtableOffset(Table);
                }

                Type subtableType = GetLookupSubtableType(lookupType);
                if (subtableType == null)
                    throw new NotSupportedException(string.Format("Cannot create instance of {0} subtable.", lookupType));

                object subtable = subtableType.Instantiate(subtableOffset);

                if (resolveContext)
                {
                    bool chaining = subtableType == ChainingSubtable || subtableType == ReverseChainingSubtable;

                    if (IsContextSubtableType(lookupType))
                    {
                        ContextSubtableType contextFormat = (ContextSubtableType)(ushort)subtable.MakeFriend().Format(Table);

                        Type contextType = GetContextSubtableType(contextFormat, chaining);
                        if (contextType == null)
                            throw new NotSupportedException(string.Format("Cannot resolve {0} context format.", contextFormat));

                        if (contextType == CoverageChainingSubtable)
                            subtable = contextType.Instantiate(new object[] { Table, subtableOffset });
                        else
                            subtable = contextType.Instantiate(subtableOffset);
                    }
                    else
                    {
                        // nothing to resolve, glyph format only
                    }
                }

                return subtable;
            }
            public int GetLookupSubtableCount(object lookup)
            {
                dynamic lookupFriend = lookup.MakeFriend();
                return lookupFriend.SubTableCount();
            }
            protected abstract bool IsContextSubtableType(ushort type);
            protected abstract bool IsExtensionSubtableType(ushort type);

            public object GetLookupSubtablePrimaryCoverage(object lookupSubtable)
            {
                dynamic lookupSubtableFriend = lookupSubtable.MakeFriend();
                object primaryCoverage = lookupSubtableFriend.GetPrimaryCoverage(Table);
                return primaryCoverage;
            }

            public IEnumerable EnumerateContextSubRuleSets(object glyphContextSubstitution)
            {
                dynamic glyphContextSubstitutionFriend = glyphContextSubstitution.MakeFriend();
                ushort setCount = TableFriend.GetUShort(glyphContextSubstitutionFriend.offset + 4);

                for (ushort setIndex = 0; setIndex < setCount; setIndex++)
                    yield return glyphContextSubstitutionFriend.RuleSet(Table, setIndex);
            }
            public object GetContextSubRuleSet(object glyphContextSubstitution, ushort ruleSetIndex)
            {
                dynamic glyphContextSubstitutionFriend = glyphContextSubstitution.MakeFriend();
                ushort setCount = TableFriend.GetUShort(glyphContextSubstitutionFriend.offset + 4);

                if (ruleSetIndex >= setCount)
                    throw new ArgumentOutOfRangeException("ruleSetIndex");

                return glyphContextSubstitutionFriend.RuleSet(Table, ruleSetIndex);
            }

            public IEnumerable EnumerateContextSubClassSets(object classContextSubstitution)
            {
                dynamic classContextSubstitutionFriend = classContextSubstitution.MakeFriend();

                ushort setCount = classContextSubstitutionFriend.ClassSetCount(Table);
                for (ushort setIndex = 0; setIndex < setCount; setIndex++)
                    yield return classContextSubstitutionFriend.ClassSet(Table, setIndex);
            }
            public object GetContextSubClassSet(object classContextSubstitution, ushort classSetIndex)
            {
                dynamic classContextSubstitutionFriend = classContextSubstitution.MakeFriend();

                ushort setCount = classContextSubstitutionFriend.ClassSetCount(Table);
                if (classSetIndex >= setCount)
                    throw new ArgumentOutOfRangeException("classSetIndex");

                return classContextSubstitutionFriend.ClassSet(Table, classSetIndex);
            }
            public IEnumerable EnumerateContextCoverages(object coverageContextSubstitution)
            {
                dynamic coverageContextSubstitutionFriend = coverageContextSubstitution.MakeFriend();
                ushort glyphCount = coverageContextSubstitutionFriend.GlyphCount(Table);

                for (ushort glyphIndex = 1; glyphIndex < glyphCount; glyphIndex++)
                    yield return coverageContextSubstitutionFriend.InputCoverage(Table, glyphIndex);
            }

            public IEnumerable EnumerateContextSubClassRules(object contextSubClassSet)
            {
                dynamic contextSubClassSetFriend = contextSubClassSet.MakeFriend();

                if (contextSubClassSetFriend.IsNull)
                    yield break;

                ushort ruleCount = contextSubClassSetFriend.RuleCount(Table);
                for (ushort ruleIndex = 0; ruleIndex < ruleCount; ruleIndex++)
                    yield return contextSubClassSetFriend.Rule(Table, ruleIndex);
            }
            public IEnumerable EnumerateContextSubGlyphRules(object contextSubGlyphSet)
            {
                dynamic contextSubGlyphSetFriend = contextSubGlyphSet.MakeFriend();

                ushort ruleCount = contextSubGlyphSetFriend.RuleCount(Table);
                for (ushort ruleIndex = 0; ruleIndex < ruleCount; ruleIndex++)
                    yield return contextSubGlyphSetFriend.Rule(Table, ruleIndex);
            }
            public IEnumerable<ushort> EnumerateContextSubRuleGlyphs(object contextSubRule)
            {
                dynamic contextSubRuleFriend = contextSubRule.MakeFriend();
                ushort glyphCount = contextSubRuleFriend.GlyphCount(Table);

                for (ushort glyphIndex = 0; glyphIndex < glyphCount; glyphIndex++)
                    yield return contextSubRuleFriend.GlyphId(Table, glyphIndex);
            }
            public IEnumerable<ushort> EnumerateContextSubRuleClasses(object contextSubRule)
            {
                dynamic contextSubRuleFriend = contextSubRule.MakeFriend();
                ushort glyphCount = contextSubRuleFriend.GlyphCount(Table);

                for (ushort glyphIndex = 1; glyphIndex < glyphCount; glyphIndex++)
                    yield return contextSubRuleFriend.ClassId(Table, glyphIndex);
            }

            public IEnumerable<Tuple<ushort, ushort>> EnumerateContextualLookupIndices(object contextSubRuleOrCoverageSubtable)
            {
                dynamic contextSubRuleOrCoverageSubtableFriend = contextSubRuleOrCoverageSubtable.MakeFriend();

                ushort substCount = contextSubRuleOrCoverageSubtableFriend.SubstCount(Table);
                dynamic lookupsFriend = ReflectionExtensions.MakeFriend(contextSubRuleOrCoverageSubtableFriend.ContextualLookups(Table));

                for (ushort substIndex = 0; substIndex < substCount; substIndex++)
                    yield return Tuple.Create(lookupsFriend.SequenceIndex(Table, substIndex), lookupsFriend.LookupIndex(Table, substIndex));
            }

            public object GetContextClassDefinition(object classContextSubstitution)
            {
                dynamic classContextSubstitutionFriend = classContextSubstitution.MakeFriend();

                object classDef = classContextSubstitutionFriend.ClassDef(Table);
                return ValidatedClassDefinition(classDef, classContextSubstitutionFriend);
            }

            public object GetChainingBacktrackClassDefinition(object classChainingSubstitution)
            {
                dynamic classChainingSubstitutionFriend = classChainingSubstitution.MakeFriend();

                object classDef = classChainingSubstitutionFriend.BacktrackClassDef(Table);
                return ValidatedClassDefinition(classDef, classChainingSubstitutionFriend);
            }
            public object GetChainingInputClassDefinition(object classChainingSubstitution)
            {
                dynamic classChainingSubstitutionFriend = classChainingSubstitution.MakeFriend();

                object classDef = classChainingSubstitutionFriend.InputClassDef(Table);
                return ValidatedClassDefinition(classDef, classChainingSubstitutionFriend);
            }
            public object GetChainingLookaheadClassDefinition(object classChainingSubstitution)
            {
                dynamic classChainingSubstitutionFriend = classChainingSubstitution.MakeFriend();

                object classDef = classChainingSubstitutionFriend.LookaheadClassDef(Table);
                return ValidatedClassDefinition(classDef, classChainingSubstitutionFriend);
            }
            private static object ValidatedClassDefinition(object classDef, dynamic substitutionFriend)
            {
                dynamic classDefFriend = classDef.MakeFriend();

                if (classDefFriend.offset == substitutionFriend.offset)
                    return classDefFriend.InvalidClassDef;

                return classDef;
            }

            public IEnumerable<Tuple<ushort, ushort>> EnumerateClassGlyphs(object classDefinition)
            {
                dynamic classDefinitionFriend = classDefinition.MakeFriend();

                if (classDefinitionFriend.IsInvalid)
                    yield break;

                ushort format = classDefinitionFriend.Format(Table);

                if (format == 1)
                {
                    ushort glyphCount = classDefinitionFriend.Format1GlyphCount(Table);
                    ushort startGlyph = classDefinitionFriend.Format1StartGlyph(Table);

                    for (ushort index = 0; index < glyphCount; index++)
                        yield return Tuple.Create((ushort)(startGlyph + index), classDefinitionFriend.Format1ClassValue(Table, index));

                    yield break;
                }

                if (format == 2)
                {
                    ushort rangeCount = classDefinitionFriend.Format2RangeCount(Table);

                    for (ushort rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++)
                    {
                        ushort startGlyph = classDefinitionFriend.Format2RangeStartGlyph(Table, rangeIndex);
                        ushort endGlyph = classDefinitionFriend.Format2RangeEndGlyph(Table, rangeIndex);

                        for (ushort glyph = startGlyph; glyph <= endGlyph; glyph++)
                            yield return Tuple.Create(glyph, classDefinitionFriend.Format2RangeClassValue(Table, rangeIndex));
                    }

                    yield break;
                }

                throw new NotSupportedException(string.Format("Unknown class definition format: {0}.", format));
            }
            public ushort GetGlyphClass(object classDefinition, ushort glyph)
            {
                dynamic classDefinitionFriend = classDefinition.MakeFriend();

                if (classDefinitionFriend.IsInvalid)
                    return 0;

                return classDefinitionFriend.GetClass(Table, glyph);
            }

            public abstract object GetLookupSubtableTypeEnum(object lookupSubtable);
            protected abstract Type GetLookupSubtableType(ushort lookupType);
            private static Type GetContextSubtableType(ContextSubtableType contextFormat, bool chaining)
            {
                if (chaining)
                {
                    switch (contextFormat)
                    {
                        case ContextSubtableType.GlyphBased: return GlyphChainingSubtable;
                        case ContextSubtableType.ClassBased: return ClassChainingSubtable;
                        case ContextSubtableType.CoverageBased: return CoverageChainingSubtable;

                        default:
                            return null;
                    }
                }
                else
                {
                    switch (contextFormat)
                    {
                        case ContextSubtableType.GlyphBased: return GlyphContextSubtable;
                        case ContextSubtableType.ClassBased: return ClassContextSubtable;
                        case ContextSubtableType.CoverageBased: return CoverageContextSubtable;

                        default:
                            return null;
                    }
                }
            }
            public static ContextSubtableType GetContextSubtableType(object contextSubstitution)
            {
                if (contextSubstitution == null)
                    throw new ArgumentNullException("contextSubstitution");

                if (GlyphContextSubtable.IsInstanceOfType(contextSubstitution) ||
                    GlyphChainingSubtable.IsInstanceOfType(contextSubstitution))
                    return ContextSubtableType.GlyphBased;

                if (ClassContextSubtable.IsInstanceOfType(contextSubstitution) ||
                    ClassChainingSubtable.IsInstanceOfType(contextSubstitution))
                    return ContextSubtableType.ClassBased;

                if (CoverageContextSubtable.IsInstanceOfType(contextSubstitution) ||
                    CoverageChainingSubtable.IsInstanceOfType(contextSubstitution))
                    return ContextSubtableType.CoverageBased;

                throw new ArgumentException(string.Format("Unknown context subtable type: {0}.", contextSubstitution.GetType().FullName), nameof(contextSubstitution));
            }
        }
    }
}
