namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Media;

    public enum PositioningLookupType : ushort
    {
        SingleAdjustment = 1,
        PairAdjustment = 2,
        CursiveAttachment = 3,
        MarkToBaseAttachment = 4,
        MarkToLigatureAttachment = 5,
        MarkToMarkAttachment = 6,
        Context = 7,
        Chaining = 8,
        Extension = 9
    }

    public class GlyphPositioningInspector : GlyphTypefaceInspector.LookupTableInspector
    {
        private static readonly Type GPOSHeader;
        private static readonly Type SinglePositioningSubtable;
        private static readonly Type PairPositioningSubtable;
        private static readonly Type CursivePositioningSubtable;
        private static readonly Type MarkToBasePositioningSubtable;
        private static readonly Type MarkToLigaturePositioningSubtable;
        private static readonly Type MarkToMarkPositioningSubtable;

        protected override Type TableHeaderType => GPOSHeader;
        public override string TableName => "GPOS";

        static GlyphPositioningInspector()
        {
            Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

            GPOSHeader = PresentationCore.GetType("MS.Internal.Shaping.GPOSHeader");

            SinglePositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.SinglePositioningSubtable");
            PairPositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.PairPositioningSubtable");
            CursivePositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.CursivePositioningSubtable");
            MarkToBasePositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.MarkToBasePositioningSubtable");
            MarkToLigaturePositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.MarkToLigaturePositioningSubtable");
            MarkToMarkPositioningSubtable = PresentationCore.GetType("MS.Internal.Shaping.MarkToMarkPositioningSubtable");
        }
        public GlyphPositioningInspector(GlyphTypeface typeface) : base(typeface) { }

        protected override object GetLookupTypeEnum(ushort lookupType)
        {
            return (PositioningLookupType)lookupType;
        }
        protected override bool IsContextSubtableType(ushort lookupType)
        {
            return (PositioningLookupType)lookupType is PositioningLookupType.Context or PositioningLookupType.Chaining;
        }
        protected override bool IsExtensionSubtableType(ushort lookupType)
        {
            return (PositioningLookupType)lookupType == PositioningLookupType.Extension;
        }

        private ValueRecord GetValueRecord(object valueRecordTable, ValueFormat format)
        {
            dynamic valueRecordTableFriend = valueRecordTable.MakeFriend();
            int offset = valueRecordTableFriend.offset;

            ValueRecord valueRecord = new ValueRecord();

            if ((format & ValueFormat.XPlacement) != 0)
            {
                valueRecord.XPlacement = (short)TableFriend.GetShort(offset);
                offset += 2;
            }

            if ((format & ValueFormat.YPlacement) != 0)
            {
                valueRecord.YPlacement = (short)TableFriend.GetShort(offset);
                offset += 2;
            }

            if ((format & ValueFormat.XAdvance) != 0)
            {
                valueRecord.XAdvance = (short)TableFriend.GetShort(offset);
                offset += 2;
            }

            if ((format & ValueFormat.YAdvance) != 0)
            {
                valueRecord.YAdvance = (short)TableFriend.GetShort(offset);
                offset += 2;
            }

            // TODO: support for device offsets

            return valueRecord;
        }

        public IEnumerable<Tuple<ushort, ValueRecord>> EnumerateSingles(object singlePositioning)
        {
            dynamic singlePositioningFriend = singlePositioning.MakeFriend();

            object coverageTable = singlePositioningFriend.Coverage(Table);
            ushort format = singlePositioningFriend.Format(Table);

            ValueFormat valueFormat = (ValueFormat)singlePositioningFriend.ValueFormat(Table);

            if (format == 1)
            {
                object valueRecordTable = singlePositioningFriend.Format1ValueRecord(Table);
                ValueRecord valueRecord = GetValueRecord(valueRecordTable, valueFormat);

                foreach (ushort glyph in EnumerateCoverageGlyphs(coverageTable))
                    yield return Tuple.Create(glyph, valueRecord);

                yield break;
            }

            if (format == 2)
            {
                ushort valueCount = TableFriend.GetUShort(singlePositioningFriend.offset + 4);

                ushort index = 0;
                foreach (ushort glyph in EnumerateCoverageGlyphs(coverageTable))
                {
                    object valueRecordTable = singlePositioningFriend.Format2ValueRecord(Table, index);
                    ValueRecord valueRecord = GetValueRecord(valueRecordTable, valueFormat);

                    yield return Tuple.Create(glyph, valueRecord);

                    if (++index >= valueCount)
                        break;
                }

                yield break;
            }

            throw new NotSupportedException(string.Format("Unknown single positioning format: {0}.", format));

        }

        protected override Type GetLookupSubtableType(ushort lookupType)
        {
            return GetLookupSubtableType((PositioningLookupType)lookupType);
        }
        private static Type GetLookupSubtableType(PositioningLookupType lookupType)
        {
            switch (lookupType)
            {
                case PositioningLookupType.SingleAdjustment: return SinglePositioningSubtable;
                case PositioningLookupType.PairAdjustment: return PairPositioningSubtable;
                case PositioningLookupType.CursiveAttachment: return CursivePositioningSubtable;
                case PositioningLookupType.MarkToBaseAttachment: return MarkToBasePositioningSubtable;
                case PositioningLookupType.MarkToLigatureAttachment: return MarkToLigaturePositioningSubtable;
                case PositioningLookupType.MarkToMarkAttachment: return MarkToMarkPositioningSubtable;
                case PositioningLookupType.Context: return ContextSubtable;
                case PositioningLookupType.Chaining: return ChainingSubtable;

                default:
                    return null;
            }
        }
        public override object GetLookupSubtableTypeEnum(object lookupSubtable)
        {
            return GetLookupSubtableType(lookupSubtable);
        }
        public static PositioningLookupType GetLookupSubtableType(object lookupSubtable)
        {
            if (lookupSubtable == null)
                throw new ArgumentNullException(nameof(lookupSubtable));

            if (SinglePositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.SingleAdjustment;

            if (PairPositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.PairAdjustment;

            if (CursivePositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.CursiveAttachment;

            if (MarkToBasePositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.MarkToBaseAttachment;

            if (MarkToLigaturePositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.MarkToLigatureAttachment;

            if (MarkToMarkPositioningSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.MarkToMarkAttachment;

            if (ContextSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.Context;

            if (ChainingSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.Chaining;

            if (GlyphContextSubtable.IsInstanceOfType(lookupSubtable) ||
                ClassContextSubtable.IsInstanceOfType(lookupSubtable) ||
                CoverageContextSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.Context;

            if (GlyphChainingSubtable.IsInstanceOfType(lookupSubtable) ||
                ClassChainingSubtable.IsInstanceOfType(lookupSubtable) ||
                CoverageChainingSubtable.IsInstanceOfType(lookupSubtable))
                return PositioningLookupType.Chaining;

            throw new ArgumentException(string.Format("Unknown positionining lookup type: {0}.", lookupSubtable.GetType().FullName), nameof(lookupSubtable));
        }

        [Flags]
        private enum ValueFormat
        {
            XPlacement = 1,
            YPlacement = 2,
            XAdvance = 4,
            YAdvance = 8,
            XPlacementDevice = 0x10,
            YPlacementDevice = 0x20,
            XAdvanceDevice = 0x040,
            YAdvanceDevice = 0x80
        }
    }
}
