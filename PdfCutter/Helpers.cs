// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ViewModelBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PdfCutter
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    
    public class BoolToVisibilityConverter : ConverterBase<BoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool
                       ? ((bool)value ? Visibility.Visible : Visibility.Collapsed)
                       : Visibility.Collapsed;
        }
    }

    public abstract class ConverterBase<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T converter;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #region MarkupExtension members

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return converter ?? (converter = new T());
        }

        #endregion
    }

    public class NotBoolOperatorConverter : ConverterBase<NotBoolOperatorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (object.ReferenceEquals(value, null))
            {
                return null;
            }

            return !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (object.ReferenceEquals(value, null))
            {
                return null;
            }

            return !(bool)value;
        }
    }
}