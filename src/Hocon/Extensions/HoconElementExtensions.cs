using System.Linq;

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

        public static bool IsLiteral(this HoconType hoconType)
        {
            switch (hoconType)
            {
                case HoconType.Boolean:
                case HoconType.Number:
                case HoconType.String:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMergeable(this HoconType t1, HoconType t2)
        {
            return t1 == t2 || (t1.IsLiteral() && t2.IsLiteral());
        }

        public static bool IsMergeable(this IHoconElement e1, IHoconElement e2)
        {
            return e1.Type.IsMergeable(e2.Type);
        }
    }
}