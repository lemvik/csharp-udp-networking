using System.Threading;

namespace Lem.Networking.Utilities.Resources
{
    public class OnceOnlyGuard
    {
        private const int NotEntered = 0;
        private const int Entered    = 1;
        private       int state      = NotEntered;

        public bool TryEntering => Interlocked.CompareExchange(ref state, Entered, NotEntered) == NotEntered;
    }
}