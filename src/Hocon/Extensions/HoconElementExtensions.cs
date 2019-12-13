namespace Hocon.Extensions
{
    /// <summary>
    /// HoconElementExtensions
    /// </summary>
    public static class HoconElementExtensions
    {
        /// <summary>
        /// Performs deep clone of the element's value.
        /// This is generally the same as <see cref="IHoconElement.Clone"/>, but
        /// for substitutions it returns the substitution itself since it's value will be closed
        /// during resolution process.
        /// </summary>
        public static IHoconElement CloneValue(this IHoconElement hoconElement, IHoconElement newParent)
        {
            return hoconElement is HoconSubstitution ? hoconElement : hoconElement.Clone(newParent);
        }
    }
}