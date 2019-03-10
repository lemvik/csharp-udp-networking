using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lem.Networking.Utilities.Exceptions
{
    public static class ExceptionsExtensions
    {
        public static bool IsCancellation(this AggregateException aggregateException)
        {
            return aggregateException.InnerExceptions.All(inner => inner is TaskCanceledException);
        }
    }
}
