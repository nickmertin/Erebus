using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Vex472.Erebus.Windows.UIHelpers
{
    /// <summary>
    /// Converts a string URI to an image into a <see cref="BitmapImage"/>.
    /// </summary>
    public sealed class BitmapImageUriParseConverter : IValueConverter
    {
        /// <summary>
        /// Performs the conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The conversion parameter (not used).</param>
        /// <param name="culture">The current culture info (not used).</param>
        /// <returns>The <see cref="BitmapImage"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!typeof(BitmapImage).IsSubclassOf(targetType))
                throw new ArgumentException($"{nameof(BitmapImageUriParseConverter)} only converts to the BitmapImage type!", nameof(targetType));
            try
            {
                return new BitmapImage(new Uri(value as string));
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}