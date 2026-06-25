using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> GetBatchByNumber<T>(
            this IEnumerable<T> source,
            int batchSize,
            int batchNumber)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки должен быть больше 0.");

            if (batchNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(batchNumber), "Номер пачки не может быть меньше 0.");

            return source
                .Skip(batchSize * batchNumber)
                .Take(batchSize);
        }
    }
}
