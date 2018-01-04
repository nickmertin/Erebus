namespace Vex472.Erebus.Windows.UIHelpers
{
    /// <summary>
    /// Specifies the comparison to be performed by a <see cref="ComparisonValueConverter"/>.
    /// </summary>
    public enum Comparison : byte
    {
        /// <summary>
        /// The equal to (==) comparison.
        /// </summary>
        EqualTo,

        /// <summary>
        /// The not equal to (!=) comparison.
        /// </summary>
        NotEqualTo,

        /// <summary>
        /// The greater than (&gt;) comparison.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The less than (&lt;) comparison.
        /// </summary>
        LessThan,

        /// <summary>
        /// The greater than or equal to (&gt;=) comparison.
        /// </summary>
        GreaterThanOrEqualTo,

        /// <summary>
        /// The less than or equal to (&lt;=) comparison.
        /// </summary>
        LessThanOrEqualTo,
    }
}