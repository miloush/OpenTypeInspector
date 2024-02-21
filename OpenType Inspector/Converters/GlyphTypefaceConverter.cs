namespace OpenTypeInspector
{
    using System;
    using System.Windows.Data;
    using System.Windows.Media;

    public class GlyphTypefaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(FontFamily))
            {
                GlyphTypeface typeface = (GlyphTypeface)value;

                return typeface.ToFontFamily();
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
