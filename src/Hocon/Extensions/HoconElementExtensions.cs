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

        /// <summary>
        /// Checks to see if this <see cref="IHoconElement"/> is a literal and if it is,
        /// if it's a supported string type.
        /// </summary>
        /// <param name="literal">The HOCON element to check.</param>
        public static bool IsString(this IHoconElement literal)
        {
            if (!(literal is HoconLiteral l)) return false;
            switch (l.LiteralType)
            {
                case HoconLiteralType.QuotedString:
                case HoconLiteralType.UnquotedString:
                case HoconLiteralType.TripleQuotedString:
                case HoconLiteralType.Whitespace:
                    return true;
                case HoconLiteralType.Null:
                    break;
                case HoconLiteralType.Bool:
                    break;
                case HoconLiteralType.Long:
                    break;
                case HoconLiteralType.Hex:
                    break;
                case HoconLiteralType.Octal:
                    break;
                case HoconLiteralType.Double:
                    break;
            }

            return false;
        }
    }
}