using CatalogueScanner.Core.Localisation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;

namespace CatalogueScanner.Localisation.OrchardCore
{
    /// <summary>
    /// An implementation of <see cref="IPluralStringLocalizer{T}"/> using a wrapped <see cref="IStringLocalizer{T}"/> and the <see cref="IStringLocalizer{T}.Plural"/> extension methods from Orchard Core.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to provide strings for</typeparam>
    public class PluralStringLocalizer<T> : IPluralStringLocalizer<T>
    {
        private readonly IStringLocalizer<T> stringLocalizer;

        public PluralStringLocalizer(IStringLocalizer<T> stringLocalizer)
        {
            this.stringLocalizer = stringLocalizer;
        }

        public LocalizedString this[string name] => stringLocalizer[name];

        public LocalizedString this[string name, params object[] arguments] => stringLocalizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return stringLocalizer.GetAllStrings(includeParentCultures);
        }

        public LocalizedString Plural(int count, string singular, string plural, params object[] arguments)
        {
            return stringLocalizer.Plural(count, singular, plural, arguments);
        }

        public LocalizedString Plural(int count, string[] pluralForms, params object[] arguments)
        {
            return stringLocalizer.Plural(count, pluralForms, arguments);
        }
    }
}
