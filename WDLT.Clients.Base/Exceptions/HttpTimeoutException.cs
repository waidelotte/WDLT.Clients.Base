using System;
using RestSharp;

namespace WDLT.Clients.Base.Exceptions
{
    public class HttpTimeoutException : ClientRequestException
    {
        public HttpTimeoutException(IRestResponse response) : base(response)
        {

        }
    }
}