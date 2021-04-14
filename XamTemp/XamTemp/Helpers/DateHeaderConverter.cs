namespace XamTemp.Helpers
{
    using System;
    using System.Globalization;
    using Xamarin.CommunityToolkit.Extensions.Internals;
    using Xamarin.Forms;

    class DateHeaderConverter : ValueConverterExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DateTimeOffset result)
            {
                return result.ToLocalTime().DateTime.ToShortDateString();
            }

            throw new ArgumentException("Value is not DateTimeOffset", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
