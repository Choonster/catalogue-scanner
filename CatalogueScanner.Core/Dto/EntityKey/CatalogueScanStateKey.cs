﻿using System;

namespace CatalogueScanner.Core.Dto.EntityKey
{
    /// <summary>
    /// The unique identifier of a catalogue.
    /// </summary>
    /// <param name="CatalogueType">The type of catalogue, this should be unique for each catalogue app/system; e.g. SaleFinder.</param>
    /// <param name="Store">The store that the catalogue belongs to.</param>
    /// <param name="CatalogueId">The ID of the catalogue. Each catalogue within a type/store should have a unique ID.</param>
    public record CatalogueScanStateKey(string CatalogueType, string Store, string CatalogueId)
    {
        /// <summary>
        /// Creates a <see cref="CatalogueScanStateKey"/> instance from the specified key string generated by <see cref="ToString"/>.
        /// </summary>
        /// <param name="key">The key string</param>
        /// <returns>The key object</returns>
        public static CatalogueScanStateKey FromString(string key)
        {
            #region null checks
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }
            #endregion

            var parts = key.Split('|', 3);
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid Entity Key, must be in the form <CatalogueType>|<Store>|<CatalogueId>", nameof(key));
            }

            var catalogueType = parts[0];
            var store = parts[1];
            var catalogueId = parts[2];

            return new CatalogueScanStateKey(catalogueType, store, catalogueId);
        }

        /// <summary>
        /// Generates a key string for use with Durable Entities.
        /// </summary>
        /// <returns>The key string</returns>
        public override string ToString() => $"{CatalogueType}|{Store}|{CatalogueId}";
    }
}
