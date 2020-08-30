using Microsoft.Extensions.Localization;

namespace CatalogueScanner.Core.Localisation
{
    /// <summary>
    /// An extension of <see cref="IStringLocalizer{T}"/> with support for plural forms.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to provide strings for</typeparam>
    public interface IPluralStringLocalizer<T> : IStringLocalizer<T>
    {
        /// <summary>
        /// Gets the plural form.
        /// </summary>
        /// <param name="localizer">The <see cref="IStringLocalizer"/>.</param>
        /// <param name="count">The number to be used for selecting the pluralization form.</param>
        /// <param name="singular">The singular form key.</param>
        /// <param name="plural">The plural form key.</param>
        /// <param name="arguments">The parameters used in the key.</param>
        LocalizedString Plural(int count, string singular, string plural, params object[] arguments);

        /// <summary>
        /// Gets the plural form.
        /// </summary>
        /// <param name="localizer">The <see cref="IStringLocalizer"/>.</param>
        /// <param name="count">The number to be used for selecting the pluralization form.</param>
        /// <param name="pluralForms">A list of pluralization forms.</param>
        /// <param name="arguments">The parameters used in the key.</param>
        LocalizedString Plural(int count, string[] pluralForms, params object[] arguments);
    }
}
