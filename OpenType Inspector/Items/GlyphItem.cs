namespace OpenTypeInspector
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    public class GlyphItem
    {
        public static readonly Typeface TextTypeface = new Typeface(SystemFonts.MessageFontFamily, SystemFonts.MessageFontStyle, SystemFonts.MessageFontWeight, FontStretches.Normal);

        private static readonly Brush ZeroClassBrush = Brushes.White;
        public static readonly Brush BaseBrush = Brushes.WhiteSmoke;
        public static readonly Brush LigatureBrush = Brushes.Honeydew;
        public static readonly Brush MarkBrush = Brushes.LightGoldenrodYellow;
        public static readonly Brush ComponentBrush = Brushes.Lavender;
        public static readonly Brush UnknownClassBrush = Brushes.AliceBlue;

        private ushort _glyphID;
        public ushort GlyphID { get { return _glyphID; } }
        public string GlyphString { get { return _text == null ? _glyphID.ToString() : "*"; } }

        private Geometry _geometry;
        public Geometry Geometry { get { return _geometry; } }

        private string _text;
        public string Text { get { return _text; } }

        public GlyphClass Class { get; set; }
        public Brush ClassBrush
        {
            get
            {
                switch (Class)
                {
                    case 0: return ZeroClassBrush;
                    case GlyphClass.Base: return BaseBrush;
                    case GlyphClass.Ligature: return LigatureBrush;
                    case GlyphClass.Mark: return MarkBrush;
                    case GlyphClass.Component: return ComponentBrush;
                    default: return UnknownClassBrush;
                }
            }
        }

        public GlyphItem(ushort glyphID, GlyphTypeface typeface)
        {
            _glyphID = glyphID;
            _geometry = typeface.GetGlyphOutline(glyphID, 1024, 1024);
        }
        public GlyphItem(string text)
        {
            _text = text;
            _geometry = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, TextTypeface, 11, Brushes.DarkBlue, 96).BuildGeometry(new Point(0, 0));
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_text))
                return string.Format("GlyphID = {0}", _glyphID);

            return _text;
        }
    }
}
