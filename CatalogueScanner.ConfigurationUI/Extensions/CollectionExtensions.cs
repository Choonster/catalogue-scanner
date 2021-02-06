using System;
using System.Collections.Generic;

namespace CatalogueScanner.ConfigurationUI.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="thisCollection">The collection to add the elements to.</param>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ICollection{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type <typeparamref name="T"/> is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="thisCollection"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null</exception>
        public static void AddRange<T>(this ICollection<T> thisCollection, IEnumerable<T> collection)
        {
            #region null checks
            if (thisCollection is null)
            {
                throw new ArgumentNullException(nameof(thisCollection));
            }

            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            } 
            #endregion

            if (thisCollection is List<T> list)
            {
                list.AddRange(collection);
            }
            else
            {
                foreach (var item in collection)
                {
                    thisCollection.Add(item);
                }
            }
        }
    }
}
