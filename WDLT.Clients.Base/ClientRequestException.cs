using System.Net.Http;
using RestSharp;

namespace WDLT.Clients.Base
{
    public class ClientRequestException : HttpRequestException
    {
        public ClientRequestException(IRestResponse response) : base($"Status code not OK. {response.ErrorMessage} {response.Content}")
        {
            Response = response;
        }

        public IRestResponse Response { get; }
    }
}