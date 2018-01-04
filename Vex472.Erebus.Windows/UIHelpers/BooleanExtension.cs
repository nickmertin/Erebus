using System;
using System.Windows.Markup;

namespace Vex472.Erebus.Windows.UIHelpers
{
    /// <summary>
    /// Provides a static boolean value.
    /// </summary>
    public sealed class BooleanExtension : MarkupExtension
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BooleanExtension"/> class that provides the specified value.
        /// </summary>
        /// <param name="value">The value to provide.</param>
        public BooleanExtension(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value to provide.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// Gets the value to provide.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The value to provide.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider) => Value;
    }
}