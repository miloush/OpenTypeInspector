namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Media;

    public enum SubstitutionLookupType : ushort
    {
        Single = 1,
        Multiple = 2,
        Alternate = 3,
        Ligature = 4,
        Context = 5,
        Chaining = 6,
        Extension = 7,
        ReverseChaining = 8
    }

    public class GlyphSubstitutionsInspector : GlyphTypefaceInspector.LookupTableInspector
    {
        private static readonly Type GSUBHeader;
        private static readonly Type SingleSubstitutionSubtable;
        private static readonly Type MultipleSubstitutionSubtable;
        private static readonly Type AlternateSubstitutionSubtable;
        private static readonly Type LigatureSubstitutionSubtable;

        protected override Type TableHeaderType => GSUBHeader;
        public override string TableName => "GSUB";

        static GlyphSubstitutionsInspector()
        {
            Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

            GSUBHeader = PresentationCore.GetType("MS.Internal.Shaping.GSUBHeader");

            SingleSubstitutionSubtable = PresentationCore.GetType("MS.Internal.Shaping.SingleSubstitutionSubtable");
            MultipleSubstitutionSubtable = PresentationCore.GetType("MS.Internal.Shaping.MultipleSubstitutionSubtable");
            AlternateSubstitutionSubtable = PresentationCore.GetType("MS.Internal.Shaping.AlternateSubstitutionSubtable");
            LigatureSubstitutionSubtable = PresentationCore.GetType("MS.Internal.Shaping.LigatureSubstitutionSubtable");
        }

        public GlyphSubstitutionsInspector(GlyphTypeface typeface) : base(typeface) { }

        protected override object GetLookupTypeEnum(ushort lookupType)
        {
            return (PositioningLookupType)lookupType;
        }
        protected override bool IsContextSubtableType(ushort type)
        {
            return (SubstitutionLookupType)type is SubstitutionLookupType.Context or SubstitutionLookupType.Chaining;
        }
        protected override bool IsExtensionSubtableType(ushort type)
        {
            return (SubstitutionLookupType)type == SubstitutionLookupType.Extension;
        }

        public IEnumerable<Tuple<ushort, ushort>> EnumerateSingles(object singleSubstitution)
        {
            dynamic singleSubstitutionFriend = singleSubstitution.MakeFriend();

            object coverageTable = singleSubstitutionFriend.Coverage(Table);
            ushort format = singleSubstitutionFriend.Format(Table);

            if (format == 1)
            {
                short deltaGlyphID = singleSubstitutionFriend.Format1DeltaGlyphId(Table);

                foreach (ushort glyph in EnumerateCoverageGlyphs(coverageTable))
                    yield return Tuple.Create(glyph, (ushort)(glyph + deltaGlyphID));

                yield break;
            }

            if (format == 2)
            {
                ushort glyphCount = TableFriend.GetUShort(singleSubstitutionFriend.offset + 4);

                ushort index = 0;
                foreach (ushort glyph in EnumerateCoverageGlyphs(coverageTable))
                {
                    yield return Tuple.Create(glyph, singleSubstitutionFriend.Format2SubstituteGlyphId(Table, index));

                    if (++index >= glyphCount)
                        break;
                }

                yield break;
            }

            throw new NotSupportedException(string.Format("Unknown single substitition format: {0}.", format));
        }

        public IEnumerable EnumerateMultipleSequences(object multipleSubstitution)
        {
            dynamic multipleSubstitutionFriend = multipleSubstitution.MakeFriend();
            ushort sequenceCount = TableFriend.GetUShort(multipleSubstitutionFriend.offset + 4);

            for (ushort sequenceIndex = 0; sequenceIndex < sequenceCount; sequenceIndex++)
                yield return multipleSubstitutionFriend.Sequence(Table, sequenceIndex);
        }
        public ushort GetMultipleFirstGlyph(object multipleSubstitution, ushort multipleSequenceIndex)
        {
            dynamic multipleSubstitutionFriend = multipleSubstitution.MakeFriend();
            ushort sequenceCount = TableFriend.GetUShort(multipleSubstitutionFriend.offset + 4);

            if (multipleSequenceIndex >= sequenceCount)
                throw new ArgumentOutOfRangeException("multipleSequenceIndex");

            object coverageTable = multipleSubstitutionFriend.Coverage(Table);
            return GetCoverageGlyphAtIndex(coverageTable, multipleSequenceIndex);
        }
        public IEnumerable<ushort> EnumerateMultiples(object multipleSequence)
        {
            dynamic multipleSequenceFriend = multipleSequence.MakeFriend();
            ushort multipleCount = multipleSequenceFriend.GlyphCount(Table);

            for (ushort multipleIndex = 0; multipleIndex < multipleCount; multipleIndex++)
                yield return multipleSequenceFriend.Glyph(Table, multipleIndex);
        }

        public IEnumerable EnumerateAlternateSets(object alternateSubstitution)
        {
            dynamic alternateSubstitutionFriend = alternateSubstitution.MakeFriend();
            ushort setCount = TableFriend.GetUShort(alternateSubstitutionFriend.offset + 4);

            for (ushort setIndex = 0; setIndex < setCount; setIndex++)
                yield return alternateSubstitutionFriend.AlternateSet(Table, setIndex);
        }
        public ushort GetAlternateFirstGlyph(object alternateSubstitution, ushort alternateSetIndex)
        {
            dynamic alternateSubstitutionFriend = alternateSubstitution.MakeFriend();
            ushort setCount = TableFriend.GetUShort(alternateSubstitutionFriend.offset + 4);

            if (alternateSetIndex >= setCount)
                throw new ArgumentOutOfRangeException("alternateSetIndex");

            object coverageTable = alternateSubstitutionFriend.Coverage(Table);
            return GetCoverageGlyphAtIndex(coverageTable, alternateSetIndex);
        }
        public IEnumerable<ushort> EnumerateAlternates(object alternateSet)
        {
            dynamic alternateSetFriend = alternateSet.MakeFriend();
            ushort alternateCount = alternateSetFriend.GlyphCount(Table);

            for (ushort alternateIndex = 1; alternateIndex <= alternateCount; alternateIndex++)
                yield return alternateSetFriend.Alternate(Table, alternateIndex);
        }

        public IEnumerable EnumerateLigatureSets(object ligatureSubstitution)
        {
            dynamic ligatureSubstitutionFriend = ligatureSubstitution.MakeFriend();
            ushort setCount = ligatureSubstitutionFriend.LigatureSetCount(Table);

            for (ushort setIndex = 0; setIndex < setCount; setIndex++)
                yield return ligatureSubstitutionFriend.LigatureSet(Table, setIndex);
        }
        public ushort GetLigatureFirstGlyph(object ligatureSubstitution, ushort ligatureSetIndex)
        {
            dynamic ligatureSubstitutionFriend = ligatureSubstitution.MakeFriend();
            ushort setCount = ligatureSubstitutionFriend.LigatureSetCount(Table);

            if (ligatureSetIndex >= setCount)
                throw new ArgumentOutOfRangeException("ligatureSetIndex");

            object coverageTable = ligatureSubstitutionFriend.Coverage(Table);
            return GetCoverageGlyphAtIndex(coverageTable, ligatureSetIndex);
        }
        public IEnumerable EnumerateLigatures(object ligatureSet)
        {
            dynamic ligatureSetFriend = ligatureSet.MakeFriend();
            ushort ligatureCount = ligatureSetFriend.LigatureCount(Table);

            for (ushort ligatureIndex = 0; ligatureIndex < ligatureCount; ligatureIndex++)
                yield return ligatureSetFriend.Ligature(Table, ligatureIndex);
        }
        public IEnumerable<ushort> EnumerateLigatureComponents(object ligature)
        {
            dynamic ligatureFriend = ligature.MakeFriend();
            ushort componentCount = ligatureFriend.ComponentCount(Table);

            for (ushort componentIndex = 1; componentIndex < componentCount; componentIndex++)
                yield return ligatureFriend.Component(Table, componentIndex);
        }
        public ushort GetLigatureGlyph(object ligature)
        {
            dynamic ligatureFriend = ligature.MakeFriend();

            return ligatureFriend.LigatureGlyph(Table);
        }

        protected override Type GetLookupSubtableType(ushort lookupType)
        {
            return GetLookupSubtableType((SubstitutionLookupType)lookupType);
        }
        private static Type GetLookupSubtableType(SubstitutionLookupType lookupType)
        {
            switch (lookupType)
            {
                case SubstitutionLookupType.Single: return SingleSubstitutionSubtable;
                case SubstitutionLookupType.Multiple: return MultipleSubstitutionSubtable;
                case SubstitutionLookupType.Alternate: return AlternateSubstitutionSubtable;
                case SubstitutionLookupType.Ligature: return LigatureSubstitutionSubtable;
                case SubstitutionLookupType.Context: return ContextSubtable;
                case SubstitutionLookupType.Chaining: return ChainingSubtable;
                case SubstitutionLookupType.ReverseChaining: return ReverseChainingSubtable;

                default:
                    return null;
            }
        }
        public override object GetLookupSubtableTypeEnum(object lookupSubtable)
        {
            return GetLookupSubtableType(lookupSubtable);
        }
        public static SubstitutionLookupType GetLookupSubtableType(object lookupSubtable)
        {
            if (lookupSubtable == null)
                throw new ArgumentNullException("lookupSubtable");

            if (SingleSubstitutionSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Single;

            if (MultipleSubstitutionSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Multiple;

            if (AlternateSubstitutionSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Alternate;

            if (LigatureSubstitutionSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Ligature;

            if (ContextSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Context;

            if (ChainingSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Chaining;

            if (ReverseChainingSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.ReverseChaining;

            if (GlyphContextSubtable.IsInstanceOfType(lookupSubtable) ||
                ClassContextSubtable.IsInstanceOfType(lookupSubtable) ||
                CoverageContextSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Context;

            if (GlyphChainingSubtable.IsInstanceOfType(lookupSubtable) ||
                ClassChainingSubtable.IsInstanceOfType(lookupSubtable) ||
                CoverageChainingSubtable.IsInstanceOfType(lookupSubtable))
                return SubstitutionLookupType.Chaining;

            throw new ArgumentException(string.Format("Unknown substitution lookup type: {0}.", lookupSubtable.GetType().FullName), "lookupSubtable");
        }
    }
}
