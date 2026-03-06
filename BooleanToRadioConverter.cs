using System;
using System.Globalization;
using System.Windows.Data;

namespace FullTextSearchDevSummitDemo
{
  public class BooleanToRadioConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // Only check the RadioButton if the boolean value matches the converter parameter (which should be "True" or "False")
      return value is null ? Binding.DoNothing : value is bool b && b == System.Convert.ToBoolean(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // If the RadioButton is checked (value is true), return the parameter value (true/false)
      if (value is bool isChecked && isChecked)
      {
        return System.Convert.ToBoolean(parameter);
      }
      return Binding.DoNothing; // Prevents the other RadioButton from setting the value to false
    }
  }
}
