namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Media;

    public partial class GlyphTypefaceInspector
    {
        public abstract class TableInspector
        {
            public abstract string TableName { get; }

            protected static readonly Type FontTable;   
            protected static readonly Type GsubGposTables;

            private object _table;
            public object Table => _table ??= GetFontTable(TableName);

            private dynamic _tableFriend;
            protected dynamic TableFriend => _tableFriend ??= Table.MakeFriend();

            private bool? _tableIsPresent;
            public bool TableIsPresent => _tableIsPresent ??= (bool)TableFriend.IsPresent;

            public GlyphTypeface Typeface { get; }

            private object GetFontTable(string tag)
            {
                object fontFaceLayoutInfo = Typeface.MakeFriend().FontFaceLayoutInfo;

                if (tag == "GSUB" || tag == "GPOS")
                {
                    object font = GsubGposTables.InstantiateNonPublic(fontFaceLayoutInfo);

                    dynamic tagValue = ToOpenTypeTag(tag);
                    return font.MakeFriend().GetFontTable(tagValue);
                }

                object tableTag = ToOpenTypeTableTag(tag);
                dynamic fontFaceLayoutInfoFriend = fontFaceLayoutInfo.MakeFriend();
                byte[] table = fontFaceLayoutInfoFriend.GetFontTable(tableTag);

                return FontTable.Instantiate(table);
            }

            static TableInspector()
            {
                Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

                FontTable = PresentationCore.GetType("MS.Internal.Shaping.FontTable");
                GsubGposTables = PresentationCore.GetType("MS.Internal.FontCache.GsubGposTables");
            }
            protected TableInspector(GlyphTypeface typeface)
            {
                Typeface = typeface;
            }

            public IEnumerable<ushort> EnumerateCoverageGlyphs(object coverageTable)
            {
                dynamic coverageTableFriend = coverageTable.MakeFriend();

                if (coverageTableFriend.IsInvalid)
                    yield break;

                ushort format = coverageTableFriend.Format(Table);

                if (format == 1)
                {
                    ushort glyphCount = coverageTableFriend.Format1GlyphCount(Table);

                    for (ushort index = 0; index < glyphCount; index++)
                        yield return coverageTableFriend.Format1Glyph(Table, index);

                    yield break;
                }

                if (format == 2)
                {
                    ushort rangeCount = coverageTableFriend.Format2RangeCount(Table);

                    // binary search could be used
                    for (ushort rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++)
                    {
                        ushort startGlyph = coverageTableFriend.Format2RangeStartGlyph(Table, rangeIndex);
                        ushort endGlyph = coverageTableFriend.Format2RangeEndGlyph(Table, rangeIndex);

                        for (ushort glyph = startGlyph; glyph <= endGlyph; glyph++)
                            yield return glyph;
                    }

                    yield break;
                }

                Debug.WriteLine($"TERMINATING EnumerateCoverageGlyphs: Unknown coverage format type {format}");
                yield break;

                throw new NotSupportedException(string.Format("Unknown coverage table format: {0}.", format));
            }
            public ushort GetCoverageGlyphAtIndex(object coverageTable, ushort index)
            {
                dynamic coverageTableFriend = coverageTable.MakeFriend();

                ushort format = coverageTableFriend.Format(Table);

                if (format == 1)
                {
                    ushort glyphCount = coverageTableFriend.Format1GlyphCount(Table);
                    if (index >= glyphCount)
                        throw new ArgumentOutOfRangeException("index");

                    return coverageTableFriend.Format1Glyph(Table, index);
                }

                if (format == 2)
                {
                    ushort rangeCount = coverageTableFriend.Format2RangeCount(Table);

                    // binary search could be used
                    for (ushort rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++)
                    {
                        ushort coverageIndex = coverageTableFriend.Format2RangeStartCoverageIndex(Table, rangeIndex);
                        ushort startGlyph = coverageTableFriend.Format2RangeStartGlyph(Table, rangeIndex);
                        ushort endGlyph = coverageTableFriend.Format2RangeEndGlyph(Table, rangeIndex);

                        if ((endGlyph - startGlyph + coverageIndex) >= index && index >= coverageIndex)
                            return (ushort)(startGlyph + index - coverageIndex);
                    }

                    throw new ArgumentOutOfRangeException("index");
                }

                throw new NotSupportedException(string.Format("Unknown coverage table format: {0}.", format));
            }
            public int GetCoverageGlyphCount(object coverageTable)
            {
                dynamic coverageTableFriend = coverageTable.MakeFriend();

                ushort format = coverageTableFriend.Format(Table);

                if (format == 1)
                    return (ushort)coverageTableFriend.Format1GlyphCount(Table);

                if (format == 2)
                {
                    ushort rangeCount = coverageTableFriend.Format2RangeCount(Table);

                    if (rangeCount == 0)
                        return 0;

                    ushort lastRange = (ushort)(rangeCount - 1);

                    ushort coverageIndex = coverageTableFriend.Format2RangeStartCoverageIndex(Table, lastRange);
                    ushort startGlyph = coverageTableFriend.Format2RangeStartGlyph(Table, lastRange);
                    ushort endGlyph = coverageTableFriend.Format2RangeEndGlyph(Table, lastRange);

                    return coverageIndex + endGlyph - startGlyph + 1;
                }

                throw new NotSupportedException(string.Format("Unknown coverage table format: {0}.", format));
            }
        }
    }
}
