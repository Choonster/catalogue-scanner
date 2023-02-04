using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Utility
{
    public static class Throttler
    {
        public static async Task<TResult[]> ExecuteInBatches<TInput, TResult>(IReadOnlyCollection<TInput> inputs, int batchSize, Func<TInput, Task<TResult>> taskFactory, bool continueOnCapturedContext = true)
        {
            #region null checks
            if (inputs is null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            if (taskFactory is null)
            {
                throw new ArgumentNullException(nameof(taskFactory));
            }
            #endregion


            var totalBatches = (int)Math.Ceiling((double)inputs.Count / batchSize);
            var results = new TResult[totalBatches][];

            for (var batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                var tasks = inputs.Skip(batchIndex * batchSize)
                                  .Take(batchSize)
                                  .Select(taskFactory)
                                  .ToList();

                results[batchIndex] = await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext);
            }

            return results.SelectMany(batch => batch).ToArray();
        }
    }
}
