using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Demo {
  public class QuantityToBackgroundConverter: IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
    {

        int quantity = 0;


        if(int.TryParse(value.ToString(),out quantity)==true) 
        {
            //quantity = (int)value;

            if (quantity>=100) return Brushes.Blue;
            if (quantity>=10) return Brushes.Green;
            if (quantity>=0) return Brushes.LightGreen;
            return Brushes.Gray; //quantity should not be below 0
        }
        //value is not an integer. Do not throw an exception in the converter, but return something that is obviously wrong
        return Brushes.Yellow;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }


  public class StringToBackgroundConverter : IValueConverter
  {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {

          string strVal = value.ToString();

          switch(strVal)
          {
              case "1":
                  return Brushes.LightCoral;
              default:
                  return Brushes.LightGray;
          }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
          throw new NotImplementedException();
      }
  }


  public class ResultToForegroundConverter : IValueConverter
  {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {

          string strVal = value.ToString();

          switch (strVal)
          {
              case "PASS":
                  return Brushes.Blue;
              default:
                  return Brushes.Red;
          }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
          throw new NotImplementedException();
      }
  }


}
