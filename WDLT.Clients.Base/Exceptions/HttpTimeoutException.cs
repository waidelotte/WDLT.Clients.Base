using System;

namespace WDLT.Clients.Base.Exceptions
{
    public class HttpTimeoutException : Exception
    {
        public HttpTimeoutException() {  }

        public HttpTimeoutException(string message) : base(message) { }
    }
}