using System;

namespace Lem.Networking.Exceptions
{
    public class NetworkConstraintViolationException : Exception
    {
        public NetworkConstraintViolationException()
        {
        }

        public NetworkConstraintViolationException(string message) : base(message)
        {
        }

        public NetworkConstraintViolationException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}