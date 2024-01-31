using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Infra.StorageClient;

public static class ChunkExtension
{
    public static IEnumerable<IEnumerable<T>> ToChunksOf<T>(this IEnumerable<T> source, int chunkSize) where T : class =>
        source
            .Select((x, i) => new { Index = i, Value = x })
            .Where(x => x.Value != null)
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
}