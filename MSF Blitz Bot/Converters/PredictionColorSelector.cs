using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MSFBlitzBot
{
    public class PredictionColorSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                float valueFloat => valueFloat > .6f ? new SolidColorBrush(Colors.LightGreen) : valueFloat < .4f ? new SolidColorBrush(Colors.LightCoral) : new SolidColorBrush(Colors.LightGoldenrodYellow),
                _ => Binding.DoNothing
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
