using System;
using System.Globalization;
using System.Windows.Data;

namespace Vex472.Erebus.Windows.UIHelpers
{
    /// <summary>
    /// Performs a comparison against an object to produce a boolean result.
    /// </summary>
    public sealed class ComparisonValueConverter : IValueConverter
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ComparisonValueConverter"/> class.
        /// </summary>
        public ComparisonValueConverter() { }

        /// <summary>
        /// Creates a new instance of the <see cref="ComparisonValueConverter"/> class with the specified comparison and value.
        /// </summary>
        /// <param name="comparison">The comparison to perform.</param>
        /// <param name="value">The value to compare to.</param>
        public ComparisonValueConverter(Comparison comparison, object value)
        {
            Comparison = comparison;
            CompareValue = value;
        }

        /// <summary>
        /// Gets or sets the comparison to perform.
        /// </summary>
        public Comparison Comparison { get; set; }

        /// <summary>
        /// Gets or sets the value to compare to.
        /// </summary>
        public object CompareValue { get; set; }

        /// <summary>
        /// Performs the comparison.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="targetType">The expected type; must be either <see cref="bool"/> or nullable <see cref="bool"/>.</param>
        /// <param name="parameter">The conversion parameter (not used).</param>
        /// <param name="culture">The current culture info (not used).</param>
        /// <returns>The result of the comparison, either True or False.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
                throw new ArgumentException("Target type must be either bool or Nullable<bool>.", nameof(targetType));
            dynamic x = value, y = CompareValue;
            switch (Comparison)
            {
                case Comparison.EqualTo:
                    return x == y;
                case Comparison.NotEqualTo:
                    return x != y;
                case Comparison.GreaterThan:
                    return x > y;
                case Comparison.LessThan:
                    return x < y;
                case Comparison.GreaterThanOrEqualTo:
                    return x >= y;
                case Comparison.LessThanOrEqualTo:
                    return x <= y;
            }
            throw new InvalidOperationException($"Unrecognized comparison {Comparison}");
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