using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiteDbExplorer.Converters
{
    public class BsonValueToNetValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is BsonValue)
            {
                return (value as BsonValue).RawValue;
            }
            else
            {
                throw new Exception("Cannot convert non BSON value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return new BsonValue(value);
        }
    }
}
