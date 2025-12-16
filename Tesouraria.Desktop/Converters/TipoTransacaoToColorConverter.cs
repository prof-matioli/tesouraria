using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Desktop.Converters
{
    public class TipoTransacaoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TipoTransacao tipo)
            {
                return tipo == TipoTransacao.Receita
                    ? Brushes.DarkGreen
                    : Brushes.DarkRed;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}