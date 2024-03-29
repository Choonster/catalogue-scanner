﻿namespace CatalogueScanner.Core.Options;

public class EmailOptions
{
    public const string Email = "Email";

    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;

    public string ToEmail { get; set; } = null!;
    public string ToName { get; set; } = null!;

    /// <summary>
    /// If true, a digest email will be sent for every scanned catalogue, even if there were no matching items.
    /// </summary>
    public bool SendDigestWhenNoMatchingItems { get; set; }
}
