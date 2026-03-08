using System;
using System.Globalization;
using System.Windows.Data;

namespace FullTextSearchDevSummitDemo
{
  public class IntToRadioConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is null ? Binding.DoNothing : value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? parameter : Binding.DoNothing;
    }
  }
}
