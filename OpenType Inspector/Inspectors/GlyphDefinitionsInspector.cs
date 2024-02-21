namespace OpenTypeInspector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Media;

    public enum GlyphClass : ushort
    {
        Base = 1,
        Ligature = 2,
        Mark = 3,
        Component = 4,
    }

    public class GlyphDefinitionsInspector : GlyphTypefaceInspector.TableInspector
    {
        private static readonly Type GDEFHeader;
        private static readonly Type CoverageTable;

        public override string TableName => "TTO_GDEF";

        static GlyphDefinitionsInspector()
        {
            Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

            GDEFHeader = PresentationCore.GetType("MS.Internal.Shaping.GDEFHeader");
            CoverageTable = PresentationCore.GetType("MS.Internal.Shaping.CoverageTable");
        }

        public GlyphDefinitionsInspector(GlyphTypeface typeface) : base(typeface) { }

        public GlyphClass GetGlyphClass(ushort glyph)
        {
            object header = Activator.CreateInstance(GDEFHeader, 0);
            dynamic headerFriend = header.MakeFriend();

            if (!TableIsPresent)
                return 0;

            object classDefTable = headerFriend.GetGlyphClassDef(Table);
            dynamic classDefTableFriend = classDefTable.MakeFriend();

            if (classDefTableFriend.IsInvalid)
                return 0;
                
            return (GlyphClass)(ushort)classDefTableFriend.GetClass(Table, glyph);
        }
        public IDictionary<ushort, GlyphClass> GetGlyphClasses()
        {
            object header = Activator.CreateInstance(GDEFHeader, 0);
            dynamic headerFriend = header.MakeFriend();

            if (!TableIsPresent)
                return null;

            object classDefTable = headerFriend.GetGlyphClassDef(Table);
            dynamic classDefTableFriend = classDefTable.MakeFriend();

            if (classDefTableFriend.IsInvalid)
                return null;

            ushort format = classDefTableFriend.Format(Table);
            switch (format)
            {
                case 1:
                    ushort glyphCount = classDefTableFriend.Format1GlyphCount(Table);
                    ushort startGlyph = classDefTableFriend.Format1StartGlyph(Table);

                    Dictionary<ushort, GlyphClass> classes1 = new Dictionary<ushort, GlyphClass>(glyphCount);
                    for (ushort index = 0; index < glyphCount; index++)
                        classes1[checked((ushort)(startGlyph + index))] = (GlyphClass)(ushort)classDefTableFriend.Format1ClassValue(Table, index);

                    return classes1;

                case 2:
                    ushort rangeCount = classDefTableFriend.Format2RangeCount(Table);

                    Dictionary<ushort, GlyphClass> classes2 = new Dictionary<ushort, GlyphClass>();
                    for (ushort range = 0; range < rangeCount; range++)
                    {                        
                        ushort rangeStart = classDefTableFriend.Format2RangeStartGlyph(Table, range);
                        ushort rangeEnd = classDefTableFriend.Format2RangeEndGlyph(Table, range);
                        ushort value = classDefTableFriend.Format2RangeClassValue(Table, range);

                        for (ushort glyph = rangeStart; glyph <= rangeEnd; glyph++)
                            classes2[glyph] = (GlyphClass)value;
                    }

                    return classes2;

                default:
                    throw new NotSupportedException($"Class definition table format {format} not supported.");
            }
        }

        public int GetGlyphWithAttachmentPointsCount()
        {
            if (!TableFriend.IsPresent)
                return 0;

            GlyphAttachList attachList = new GlyphAttachList(TableFriend.GetOffset(6));
            if (!attachList.IsPresent)
                return 0;

            object coverage = attachList.Coverage(Table);

            return GetCoverageGlyphCount(coverage);
        }

        private class GlyphAttachList
        {
            private int offset;

            public GlyphAttachList(int offset)
            {
                this.offset = offset;
            }

            public bool IsPresent
            {
                get { return offset != 0; }
            }

            public object Coverage(object fontTable)
            {
                dynamic fontTableFriend = fontTable.MakeFriend();

                return CoverageTable.Instantiate(offset + (int)fontTableFriend.GetOffset(offset));
            }
        }
    }
}
