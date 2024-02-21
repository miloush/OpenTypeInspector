namespace OpenTypeInspector
{
    using System;
    using System.Windows.Data;

    public class EnumSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            Type enumType = parameter as Type ?? Type.GetType(parameter.ToString(), true);

            return Enum.GetValues(enumType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
